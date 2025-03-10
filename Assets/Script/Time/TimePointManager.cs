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

    public float rewindDuration = 0.5f;  // 되감기 연출 시간
    public GameObject restartPrompt;
    public AudioClip rewindSfx;
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

    // 체크포인트 저장
    public void SaveCheckpoint(Vector2 princessPos, Vector2 playerPos)
    {
        TimePointData data = new TimePointData();
        data.princessPosition = princessPos;
        data.playerPosition = playerPos;

        BaseEnemy[] enemies = FindObjectsOfType<BaseEnemy>();
        foreach (BaseEnemy enemy in enemies)
        {
            if (enemy.gameObject.activeInHierarchy)
            {
                EnemyStateData esd = new EnemyStateData();
                esd.enemyType = enemy.gameObject.name.Split('(')[0].Trim();
                esd.position = enemy.transform.position;
                data.enemyStates.Add(esd);
            }
        }

        lastCheckpointData = data;
        hasCheckpoint = true;

        // 파일 저장
        SaveLoadManager.PointCheck(data);

        Debug.Log($"[TimePointManager] 체크포인트 저장: 공주 {princessPos}, 플레이어 {playerPos}, 적 {data.enemyStates.Count}마리");
        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("Checkpoint set");
    }

    public void SetCheckpointData(TimePointData data)
    {
        lastCheckpointData = data;
        hasCheckpoint = true;
    }

    public bool HasCheckpoint()
    {
        return hasCheckpoint;
    }

    // ApplyCheckpoint (필요시 사용) – 씬 로드 직후나 이어하기 등에 사용
    public IEnumerator ApplyCheckpoint(TimePointData checkpointData, bool waitForInput)
    {
        lastCheckpointData = checkpointData;
        hasCheckpoint = true;

        Princess princess = FindObjectOfType<Princess>();
        Player player = FindObjectOfType<Player>();

        if (princess != null)
            princess.transform.position = checkpointData.princessPosition;
        if (player != null)
            player.RestoreFromRewind(checkpointData.playerPosition);

        // 적 제거
        BaseEnemy[] currentEnemies = FindObjectsOfType<BaseEnemy>();
        foreach (var e in currentEnemies)
            Destroy(e.gameObject);

        // 적 복원
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
        }

        Debug.Log("[TimePointManager] 체크포인트 적용 완료.");

        if (waitForInput)
        {
            if (StatusTextManager.Instance != null)
                StatusTextManager.Instance.ShowMessage("Press any key to continue");

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

    // (★) ImmediateReviveNoGameOver – 공주가 죽지 않고, 플레이어만 즉시 부활
    // 공주가 체크포인트에 도착했을 때 호출됨
    public void ImmediateReviveNoGameOver()
    {
        Princess princess = FindObjectOfType<Princess>();
        Player player = FindObjectOfType<Player>();
        if (princess == null || player == null)
        {
            Debug.LogWarning("[TimePointManager] ImmediateReviveNoGameOver – Can't find Princess or Player.");
            return;
        }

        // 플레이어가 Disable 상태인지 확인
        PlayerOver playerOver = FindObjectOfType<PlayerOver>();
        if (playerOver == null || !playerOver.IsDisabled)
        {
            Debug.LogWarning("[TimePointManager] Player is not in disabled state, skip immediate revive.");
            return;
        }

        // 1) 공주 움직임 정지
        princess.isControlled = true;
        Rigidbody2D prb = princess.GetComponent<Rigidbody2D>();
        if (prb != null)
        {
            prb.velocity = Vector2.zero;
        }

        // 2) 플레이어 입력 차단
        player.ignoreInput = true;

        // 3) 플레이어를 공주 위치로 이동
        Vector2 newPos = princess.transform.position;
        player.transform.position = newPos;
        Debug.Log($"[TimePointManager] ImmediateReviveNoGameOver => Player forced to {newPos}");

        // 4) PlayerOver.OnRewindComplete() 호출(체력 복원 및 카메라 재설정)
        playerOver.OnRewindComplete(newPos);
        Debug.Log("[TimePointManager] ImmediateReviveNoGameOver => Player Over revived.");

        // 5) 게임 정지 & 키 입력 대기
        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("아무 키나 누르면 부활 완료");
        Time.timeScale = 0f;
        StartCoroutine(WaitForAnyKeyThenResume());
    }

    // 키 입력 대기 후 ResumeAfterRewind()를 호출하는 코루틴
    private IEnumerator WaitForAnyKeyThenResume()
    {
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        Time.timeScale = 1f;
        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("");

        // 부활 후, 플레이어 정상화 + 공주도 재작동
        PlayerOver playerOver = FindObjectOfType<PlayerOver>();
        if (playerOver != null)
        {
            playerOver.ResumeAfterRewind();
        }
        else
        {
            Debug.LogWarning("[TimePointManager] PlayerOver not found after WaitForAnyKeyThenResume.");
        }
    }

    // RewindToCheckpoint: (필요하다면) 기존 되감기 로직
    public void RewindToCheckpoint()
    {
        if (!hasCheckpoint || lastCheckpointData == null)
        {
            Debug.LogWarning("[TimePointManager] 체크포인트가 없습니다.");
            return;
        }
        StartCoroutine(RewindCoroutine());
    }

    // RewindCoroutine – 완전한 되감기 로직 (필요시)
    private IEnumerator RewindCoroutine()
    {
        Debug.Log("[TimePointManager] 되감기 시작...");

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
            Debug.LogWarning("[TimePointManager] Cannot find Princess or Player.");
            yield break;
        }

        // 즉시 위치 보정
        princess.transform.position = lastCheckpointData.princessPosition;
        player.transform.position = lastCheckpointData.playerPosition;

        // 적 제거
        BaseEnemy[] currentEnemies = FindObjectsOfType<BaseEnemy>();
        foreach (var e in currentEnemies)
        {
            Destroy(e.gameObject);
        }
        // 적 복원
        foreach (EnemyStateData esd in lastCheckpointData.enemyStates)
        {
            GameObject enemyPrefab = Resources.Load<GameObject>($"Prefab/Enemies/{esd.enemyType}");
            if (enemyPrefab != null)
            {
                Vector2 spawnPosition = GetGroundPosition(esd.position);
                Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            }
        }

        Debug.Log("[TimePointManager] 되감기 완료.");

        // 원하는 경우 부활 처리 etc...

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
