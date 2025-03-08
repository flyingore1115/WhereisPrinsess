using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimePointManager : MonoBehaviour
{
    private static TimePointManager instance;
    public static TimePointManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("TimePointManager");
                instance = obj.AddComponent<TimePointManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private TimePointData lastCheckpointData = null;
    private bool hasCheckpoint = false;

    public float rewindDuration = 0.5f; // 되감기 연출 시간
    public GameObject restartPrompt;    // “아무 키나 누르면 시작” UI
    public AudioClip rewindSfx;         // 되감기 효과음
    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (restartPrompt != null)
            restartPrompt.SetActive(false);
    }

    // 체크포인트 저장 함수
    public void SaveCheckpoint(Vector2 princessPos, Vector2 playerPos)
    {
        TimePointData data = new TimePointData();
        data.princessPosition = princessPos;
        data.playerPosition = playerPos;
        
        // 현재 씬의 살아있는 적들을 저장
        BaseEnemy[] enemies = FindObjectsOfType<BaseEnemy>();
        foreach (BaseEnemy enemy in enemies)
        {
            if (enemy.gameObject.activeInHierarchy)
            {
                EnemyStateData esd = new EnemyStateData();
                // 이름 파싱: "(Clone)" 등 제거
                esd.enemyType = enemy.gameObject.name.Split('(')[0].Trim();
                esd.position = enemy.transform.position;
                data.enemyStates.Add(esd);
            }
        }
        
        lastCheckpointData = data;
        hasCheckpoint = true;
        
        // 체크포인트 파일 저장
        SaveLoadManager.PointCheck(data);

        Debug.Log($"[TimePointManager] 세이브 완료: 공주 {princessPos}, 플레이어 {playerPos}, 적 {data.enemyStates.Count}마리");
        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("되감기 포인트 지정됨");
    }

    // SetCheckpointData 메서드 추가 (오류 1 해결)
    public void SetCheckpointData(TimePointData data)
    {
        lastCheckpointData = data;
        hasCheckpoint = true;
    }

    public bool HasCheckpoint()
    {
        return hasCheckpoint;
    }

    // ApplyCheckpoint를 IEnumerator로 변경 (오류 2 해결)
    public IEnumerator ApplyCheckpoint(TimePointData checkpointData, bool waitForInput)
    {
        lastCheckpointData = checkpointData;
        hasCheckpoint = true;

        // 공주와 플레이어 위치 복원
        Princess princess = FindObjectOfType<Princess>();
        Player player = FindObjectOfType<Player>();

        if (princess != null)
            princess.transform.position = checkpointData.princessPosition;
        if (player != null)
            player.RestoreFromRewind(checkpointData.playerPosition);

        // 기존 적 제거
        BaseEnemy[] currentEnemies = FindObjectsOfType<BaseEnemy>();
        foreach (var e in currentEnemies)
        {
            Destroy(e.gameObject);
        }

        // 저장된 적 복원 (바닥 위치 보정 포함)
        foreach (EnemyStateData esd in checkpointData.enemyStates)
        {
            GameObject enemyPrefab = Resources.Load<GameObject>($"Prefab/Enemies/{esd.enemyType}");
            if (enemyPrefab != null)
            {
                Vector2 spawnPosition = GetGroundPosition(esd.position);
                GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

                Rigidbody2D rb = newEnemy.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.simulated = false;
                    yield return new WaitForSeconds(0.1f);
                    rb.simulated = true;
                }
            }
            else
            {
                Debug.LogWarning($"적 프리팹({esd.enemyType})을 찾을 수 없음");
            }
        }

        Debug.Log("[TimePointManager] 체크포인트 적용 완료.");

        // 이어하기 상황이라면 대기하지 않음
        if (waitForInput)
        {
            if (StatusTextManager.Instance != null)
                StatusTextManager.Instance.ShowMessage("아무 키나 누르면 시작");

            Time.timeScale = 0f;
            while (!Input.anyKeyDown)
            {
                yield return null;
            }
            Time.timeScale = 1f;
            if (StatusTextManager.Instance != null)
                StatusTextManager.Instance.ShowMessage("");
        }
    }


    public void RewindToCheckpoint()
    {
        if (!hasCheckpoint || lastCheckpointData == null)
        {
            Debug.LogWarning("[TimePointManager] 체크포인트가 없습니다.");
            return;
        }
        StartCoroutine(RewindCoroutine());
    }

    private IEnumerator RewindCoroutine()
    {
        Debug.Log("[TimePointManager] 되감기 중...");
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySFX("rewind");
        }

        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("되감기 중...");

        Princess princess = FindObjectOfType<Princess>();
        Player player = FindObjectOfType<Player>();
        if (princess == null || player == null)
        {
            Debug.LogWarning("[TimePointManager] 공주 또는 플레이어를 찾을 수 없음!");
            yield break;
        }

        Vector2 startPrinPos = princess.transform.position;
        Vector2 startPlayPos = player.transform.position;

        float elapsed = 0f;
        while (elapsed < rewindDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / rewindDuration;
            princess.transform.position = Vector2.Lerp(startPrinPos, lastCheckpointData.princessPosition, t);
            player.transform.position = Vector2.Lerp(startPlayPos, lastCheckpointData.playerPosition, t);
            yield return null;
        }

        princess.transform.position = lastCheckpointData.princessPosition;
        player.transform.position = lastCheckpointData.playerPosition;

        BaseEnemy[] currentEnemies = FindObjectsOfType<BaseEnemy>();
        foreach (var e in currentEnemies)
        {
            Destroy(e.gameObject);
        }

        foreach (EnemyStateData esd in lastCheckpointData.enemyStates)
        {
            GameObject enemyPrefab = Resources.Load<GameObject>($"Prefab/Enemies/{esd.enemyType}");
            if (enemyPrefab != null)
            {
                Vector2 spawnPosition = GetGroundPosition(esd.position);
                Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning($"적 프리팹({esd.enemyType})을 찾을 수 없음");
            }
        }

        Debug.Log("[TimePointManager] 되감기 완료.");

        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("아무 키나 누르면 시작");

        Time.timeScale = 0f;
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        Time.timeScale = 1f;
        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("");
    }

    // 적의 바닥 위치 보정 함수
    private Vector2 GetGroundPosition(Vector2 originalPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(originalPosition, Vector2.down, 5f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
        {
            return new Vector2(originalPosition.x, hit.point.y + 0.1f);
        }
        return originalPosition;
    }

    public TimePointData GetLastCheckpointData()
    {
        return lastCheckpointData;
    }

}
