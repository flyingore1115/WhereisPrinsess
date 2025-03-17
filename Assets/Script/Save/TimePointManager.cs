using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;              // ★ LINQ 사용
using MyGame;

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

    public float rewindDuration = 0.5f;
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

    /// <summary>
    /// 체크포인트가 존재하는가?
    /// </summary>
    public bool HasCheckpoint()
    {
        return hasCheckpoint;
    }

    /// <summary>
    /// 마지막 체크포인트 데이터 가져오기
    /// </summary>
    public TimePointData GetLastCheckpointData()
    {
        return lastCheckpointData;
    }

    /// <summary>
    /// 체크포인트 저장:
    /// 체크포인트 시점의 공주/플레이어 위치, 그리고 살아있는 적들만 ID와 위치, 스케일을 기록
    /// </summary>
    public void SaveCheckpoint(Vector2 princessPos, Vector2 playerPos)
    {
        TimePointData data = new TimePointData();
        data.princessPosition = princessPos;
        data.playerPosition = playerPos;

        // 씬 내의 모든 적 중, 죽지 않은(isDead=false) + activeInHierarchy=true 만 기록
        BaseEnemy[] enemies = FindObjectsOfType<BaseEnemy>();
        foreach (BaseEnemy enemy in enemies)
        {
            if (!enemy.isDead && enemy.gameObject.activeInHierarchy)
            {
                EnemyStateData esd = new EnemyStateData();
                esd.enemyType = enemy.prefabName;
                esd.enemyID = enemy.enemyID;
                esd.position = enemy.transform.position;
                esd.localScale = enemy.transform.localScale;

                data.enemyStates.Add(esd);
            }
        }

        lastCheckpointData = data;
        hasCheckpoint = true;

        Debug.Log($"[TimePointManager] 체크포인트 저장 완료: 공주({princessPos}), 플레이어({playerPos}), 적 {data.enemyStates.Count}마리");
        // 필요시 SaveLoadManager.PointCheck(data) 등

        if (RewindManager.Instance != null)
        {
            RewindManager.Instance.ClearSnapshotsAndRecordOne();
            // 혹은 ClearSnapshots()만 쓰고 싶다면 그걸로 변경
        }

        // ★ SaveLoadManager.PointCheck(data)를 호출해서 파일에 저장
        SaveLoadManager.PointCheck(data);

        if (RewindManager.Instance != null)
            RewindManager.Instance.ClearSnapshotsAndRecordOne();
    }

    public void SetCheckpointData(TimePointData data)
    {
        lastCheckpointData = data;
        hasCheckpoint = (data != null);
    }
    public void ClearCheckpointFlag()
    {
        lastCheckpointData = null;
        hasCheckpoint = false;
    }




    /// 체크포인트 적용: 공주/플레이어 위치를 복원하고, 
    ///  [기존 Destroy+Instantiate] 대신, 이미 씬에 있는 죽은 적을 되살린다
    public IEnumerator ApplyCheckpoint(TimePointData checkpointData, bool waitForInput)
    {
        if (checkpointData == null)
        {
            Debug.LogError("[TimePointManager] ApplyCheckpoint: checkpointData가 null입니다.");
            yield break;
        }

        lastCheckpointData = checkpointData;
        hasCheckpoint = true;

        // 필요한 오브젝트가 완전히 로드될 때까지 대기
        Princess princess = null;
        Player player = null;
        float waitTimer = 0f;
        while (princess == null || player == null)
        {
            princess = GameObject.FindGameObjectWithTag("Princess")?.GetComponent<Princess>();
            player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
            waitTimer += Time.deltaTime;
            if (waitTimer > 5f)
            {
                Debug.LogError("[TimePointManager] ApplyCheckpoint: 5초 내에 Princess 또는 Player를 찾지 못했습니다.");
                yield break;
            }
            yield return null;
        }

        // 둘 다 새로 생성된 오브젝트로 확인되었으므로, 체크포인트 데이터에 저장된 위치로 강제 재설정
        princess.transform.position = checkpointData.princessPosition;
        player.RestoreFromRewind(checkpointData.playerPosition);
        Debug.Log($"[TimePointManager] 체크포인트 적용: 공주 위치 {checkpointData.princessPosition}, 플레이어 위치 {checkpointData.playerPosition}");

        // 씬 내의 죽은 적들을 체크포인트 데이터에 따라 재활성화
        ReactivateDeadEnemies(checkpointData);

        Debug.Log("[TimePointManager] 체크포인트 적용 완료.");

        if (waitForInput)
        {
            Time.timeScale = 0f;
            while (!Input.anyKeyDown)
            {
                yield return null;
            }
            Time.timeScale = 1f;
        }
    }




    /// <summary>
    /// 죽은 적을 다시 활성화하는 함수
    /// 체크포인트 데이터(enemyStates)에 기록된 ID, prefabName과 일치하는 isDead=true 적만 되살림
    /// → 체크포인트 이전에 이미 죽어 있던 적은 데이터에 없으므로 부활 X
    /// </summary>

    private void ReactivateDeadEnemies(TimePointData checkpointData)
    {
        if (checkpointData == null) return;
        BaseEnemy[] allEnemies = Resources.FindObjectsOfTypeAll<BaseEnemy>()
            .Where(e => e.gameObject.scene.isLoaded)
            .ToArray();

        List<BaseEnemy> matched = new List<BaseEnemy>();

        foreach (EnemyStateData esd in checkpointData.enemyStates)
        {
            BaseEnemy candidate = allEnemies.FirstOrDefault(e =>
                e.enemyID == esd.enemyID &&
                e.prefabName == esd.enemyType
            );
            if (candidate != null && !matched.Contains(candidate))
            {
                // 위치/스케일 복원
                candidate.transform.position = esd.position;
                candidate.transform.localScale = esd.localScale;

                // ExplosiveEnemy라면 isActivated, isExploding 초기화
                ExplosiveEnemy ex = candidate as ExplosiveEnemy;
                if (ex != null) ex.ResetOnRewind();

                // 지금이 "죽어있던" 적이면 isDead=false + SetActive(true)
                if (candidate.isDead && !candidate.gameObject.activeInHierarchy)
                {
                    candidate.isDead = false;
                    candidate.gameObject.SetActive(true);
                }

                matched.Add(candidate);
            }
        }
    }


    /// <summary>
    /// 즉시 플레이어 부활
    /// </summary>
    public void ImmediateRevive()
    {
        if (!hasCheckpoint || lastCheckpointData == null)
        {
            Debug.LogWarning("[TimePointManager] 체크포인트가 없습니다.");
            return;
        }

        Princess princess = FindObjectOfType<Princess>();
        Player player = FindObjectOfType<Player>();
        if (princess == null || player == null)
        {
            Debug.LogWarning("[TimePointManager] 부활 대상(공주/플레이어) 찾지 못함!");
            return;
        }

        player.ignoreInput = true;
        Vector2 targetPos = princess.transform.position;
        player.transform.position = targetPos;

        PlayerOver playerOver = FindObjectOfType<PlayerOver>();
        if (playerOver != null && playerOver.IsDisabled)
        {
            playerOver.OnRewindComplete(targetPos);
        }

        princess.isControlled = true;
        Rigidbody2D prb = princess.GetComponent<Rigidbody2D>();
        if (prb != null)
        {
            prb.velocity = Vector2.zero;
        }

        CameraFollow cf = FindObjectOfType<CameraFollow>();
        if (cf != null)
        {
            cf.SetTarget(player.gameObject);
        }

        // 체크포인트 갱신
        SaveCheckpoint(princess.transform.position, player.transform.position);
    }

    /// <summary>
    /// 체크포인트가 있으면 되감기
    /// </summary>
    public void RewindToCheckpoint()
    {
        if (!hasCheckpoint || lastCheckpointData == null)
        {
            Debug.LogWarning("[TimePointManager] 체크포인트가 없습니다.");
            return;
        }
        StartCoroutine(RewindCoroutine());
    }

    /// <summary>
    /// 기존 되감기 코루틴(원본)
    /// </summary>
    private IEnumerator RewindCoroutine()
    {
        Debug.Log("[TimePointManager] 되감기 시작...");

        Princess princess = FindObjectOfType<Princess>();
        Player player = FindObjectOfType<Player>();
        if (princess == null || player == null)
        {
            Debug.LogWarning("[TimePointManager] 공주/플레이어 찾기 실패!");
            yield break;
        }

        // 위치 보정
        princess.transform.position = lastCheckpointData.princessPosition;
        player.transform.position = lastCheckpointData.playerPosition;
        Debug.Log($"[TimePointManager] 위치 보정 완료: 공주({lastCheckpointData.princessPosition}), 플레이어({lastCheckpointData.playerPosition})");

        // ★ 수정: 죽은 적만 다시 활성화
        ReactivateDeadEnemies(lastCheckpointData);

        Debug.Log("[TimePointManager] 되감기 완료.");

        // 플레이어 ImmediateRevive
        ImmediateRevive();

        // 대기
        Time.timeScale = 0f;
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        Time.timeScale = 1f;

        PlayerOver playerOver = FindObjectOfType<PlayerOver>();
        if (playerOver != null)
        {
            playerOver.ResumeAfterRewind();
        }
    }
}
