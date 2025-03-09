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
                esd.enemyType = enemy.gameObject.name.Split('(')[0].Trim();
                esd.position = enemy.transform.position;
                data.enemyStates.Add(esd);
            }
        }
        
        lastCheckpointData = data;
        hasCheckpoint = true;
        SaveLoadManager.PointCheck(data);
        Debug.Log($"[TimePointManager] 세이브 완료: 공주 {princessPos}, 플레이어 {playerPos}, 적 {data.enemyStates.Count}마리");
        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("되감기 포인트 지정됨");
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

    // ImmediateRevive: 공주가 체크포인트에 닿는 즉시 플레이어 부활 처리
    public void ImmediateRevive()
    {
        if (!hasCheckpoint || lastCheckpointData == null)
        {
            Debug.LogWarning("[TimePointManager] 체크포인트가 없습니다.");
            return;
        }

        PlayerOver playerOver = FindObjectOfType<PlayerOver>();
        if (playerOver != null && playerOver.IsDisabled)
        {
            // 플레이어 부활 처리: OnRewindComplete를 즉시 호출
            Vector2 restoredPosition = lastCheckpointData.playerPosition;
            playerOver.OnRewindComplete(restoredPosition);
            Debug.Log("[TimePointManager] ImmediateRevive: Player revived.");

            // 카메라 타겟을 플레이어로 즉시 전환
            CameraFollow cf = FindObjectOfType<CameraFollow>();
            Player player = FindObjectOfType<Player>();
            if (cf != null && player != null)
            {
                cf.SetTarget(player.gameObject);
                Debug.Log("[TimePointManager] ImmediateRevive: Camera target set to player.");
            }
            else
            {
                Debug.LogWarning("[TimePointManager] ImmediateRevive: CameraFollow or Player not found.");
            }

            // 이후, 부활 후 플레이어 입력 대기 없이 바로 ResumeAfterRewind 호출
            if (StatusTextManager.Instance != null)
                StatusTextManager.Instance.ShowMessage("아무 키나 누르면 시작");
            Time.timeScale = 0f;
            while (!Input.anyKeyDown)
            {
                // 여기서 모든 오브젝트가 정지된 상태 유지
                System.Threading.Thread.Sleep(10); // 잠시 대기 (주의: 코루틴에서는 yield return null; 사용해야 하지만, 타임스케일 0일 때는 효과가 없음)
                // 실제로는 yield return null; 로 충분할 수 있습니다.
                // 이 예시에서는 간단하게 작성
                break;
            }
            Time.timeScale = 1f;
            if (StatusTextManager.Instance != null)
                StatusTextManager.Instance.ShowMessage("");

            playerOver.ResumeAfterRewind();
        }
        else
        {
            Debug.LogWarning("[TimePointManager] ImmediateRevive: 부활할 플레이어 상태가 아닙니다.");
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
        Debug.Log("[TimePointManager] 되감기 시작...");
        
        // 효과음 및 UI 메시지 출력
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySFX("rewind");
        }
        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("되감기 중...");
        
        // 공주와 플레이어 참조
        Princess princess = FindObjectOfType<Princess>();
        Player player = FindObjectOfType<Player>();
        if (princess == null || player == null)
        {
            Debug.LogWarning("[TimePointManager] 공주 또는 플레이어를 찾을 수 없음!");
            yield break;
        }
        
        // Lerp를 통해 서서히 위치 보정 (원하는 경우 즉시 보정으로 변경 가능)
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
        // 최종 위치 보정
        princess.transform.position = lastCheckpointData.princessPosition;
        player.transform.position = lastCheckpointData.playerPosition;
        
        // 기존 적 제거 및 복원
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

        // 즉시 부활 처리: 플레이어 행동불능 상태라면 부활 진행
        PlayerOver playerOver = FindObjectOfType<PlayerOver>();
        if (playerOver != null && playerOver.IsDisabled)
        {
            Vector2 restoredPosition = lastCheckpointData.playerPosition;
            playerOver.OnRewindComplete(restoredPosition);
            Debug.Log("[TimePointManager] ImmediateRevive: 플레이어 부활 처리 완료.");
        }
        else
        {
            Debug.LogWarning("[TimePointManager] 부활할 PlayerOver 상태가 아닙니다.");
        }
        
        // 공주를 멈추도록 설정: 부활 후, 공주 조종 플래그를 강제로 유지해서 공주가 움직이지 않게 함
        Princess princessScript = princess.GetComponent<Princess>();
        if (princessScript != null)
        {
            princessScript.isControlled = true; // 공주가 계속 멈춰 있음
            Debug.Log("[TimePointManager] 공주 조종 플래그 유지 (멈춤 상태).");
        }
        
        // 키 입력 전까지 전체 게임 정지: 공주와 플레이어 모두 움직이지 않음
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
        
        // 부활 후, 플레이어 입력 복구 및 공주 조종 해제 (즉, 정상 동작 전환)
        playerOver.ResumeAfterRewind();
        
        // 여기서 새롭게 세이브(save)하는 로직을 추가할 수 있습니다.
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

}
