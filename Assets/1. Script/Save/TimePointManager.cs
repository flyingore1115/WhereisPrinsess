using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameStateData lastGameStateData = null; // 새로 추가된 플레이어 상태 데이터
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

    public bool HasCheckpoint() { return hasCheckpoint; }
    public TimePointData GetLastCheckpointData() { return lastCheckpointData; }

    public void SaveCheckpoint(Vector2 princessPos, Vector2 playerPos)
    {
        TimePointData data = new TimePointData();
        data.princessPosition = princessPos;
        data.playerPosition = playerPos;

        // 적 상태 저장 (기존 로직 유지)
        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
        foreach (BaseEnemy enemy in enemies)
        {
            if (!enemy.isDead && enemy.gameObject.activeInHierarchy)
            {
                EnemyStateData esd = new EnemyStateData();
                esd.enemyType = enemy.prefabName;
                esd.enemyID = enemy.enemyID;
                esd.position = enemy.transform.position;
                esd.localScale = enemy.transform.localScale;
                esd.health = enemy.currentHealth;

                data.enemyStates.Add(esd);
            }
        }

        lastCheckpointData = data;
        hasCheckpoint = true;

        // 플레이어 상태도 함께 저장 (예: 총알, 체력, 시간 에너지)
        GameStateData gameState = new GameStateData();
        gameState.checkpointData = data;
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            gameState.playerBulletCount = player.shooting.currentAmmo;
            gameState.playerGauge = 0; // gauge 관련 값은 필요에 따라
            gameState.playerHealth = player.health;

            var tsc = TimeStopController.Instance;
            gameState.playerTimeEnergy = (tsc != null) ? tsc.CurrentGauge : player.timeEnergy;

            gameState.unlockedSkills = new List<string>(); // 필요시 처리
        }
        lastGameStateData = gameState;

        Debug.Log($"[TimePointManager] 체크포인트 저장 완료: 공주({princessPos}), 플레이어({playerPos}), 적 {data.enemyStates.Count}마리");
        SaveLoadManager.PointCheck(gameState); // GameStateData로 저장 (SaveLoadManager도 이에 맞게 수정)

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

    public IEnumerator ApplyCheckpoint(TimePointData checkpointData, bool waitForInput)
    {
        if (checkpointData == null) yield break;

        // ① Player만 찾을 수 있으면 바로 진행 — princess는 Optional
        Player player = null;
        float waitTimer = 0f;
        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
            waitTimer += Time.deltaTime;
            if (waitTimer > 5f)
            {
                Debug.LogError("[TPM] Player 못 찾음 – Abort");
                yield break;
            }
            yield return null;
        }

        // ② Princess 대신 Lady 지원
        Princess princess = GameObject.FindGameObjectWithTag("Princess")?.GetComponent<Princess>();
        Lady lady = (princess == null) ? FindFirstObjectByType<Lady>() : null;

        // ③ 위치 복원
        if (princess != null)
            princess.transform.position = checkpointData.princessPosition;
        else if (lady != null)
            lady.transform.position = checkpointData.princessPosition;

        player.RestoreFromRewind(checkpointData.playerPosition);

        // ④ 속도·모드 클린업
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        if (lady != null)
        {
            var lrb = lady.GetComponent<Rigidbody2D>();
            lrb.linearVelocity = Vector2.zero;
            lady.ForceIdle();               // Idle + isStopped = true
        }

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
        // — 위치&속도 복원 (princess 또는 lady가 null이 아니어야 처리)
        if (princess != null)
        {
            princess.transform.position = checkpointData.princessPosition;
            var prb = princess.GetComponent<Rigidbody2D>();
            if (prb != null) prb.linearVelocity = Vector2.zero;
        }
        else if (lady != null)
        {
            lady.transform.position = checkpointData.princessPosition;
            var lrb = lady.GetComponent<Rigidbody2D>();
            if (lrb != null) lrb.linearVelocity = Vector2.zero;
        }

        // 플레이어는 항상 처리
        player.transform.position = checkpointData.playerPosition;
        var prRb = player.GetComponent<Rigidbody2D>();
        if (prRb != null) prRb.linearVelocity = Vector2.zero;
        player.GetComponent<P_Movement>().ResetInput();

        // ▶ 플레이어 상태 복원 (체력, 에너지 등)
        if (lastGameStateData != null)
        {
            var playerOver = player.GetComponent<PlayerOver>();
            if (playerOver != null)
            {
                playerOver.ForceSetHealth(lastGameStateData.playerHealth);  // 아래 정의
            }

            player.health = lastGameStateData.playerHealth;
            player.timeEnergy = lastGameStateData.playerTimeEnergy;
            player.shooting.currentAmmo = lastGameStateData.playerBulletCount;
            player.shooting.UpdateAmmoUI();
            Debug.Log($"[TPM] 체력/에너지 복원 완료: 체력={player.health}, 에너지={player.timeEnergy}, 탄={player.shooting.currentAmmo}");
    TimeStopController tsc = TimeStopController.Instance;
if (tsc != null)
{
    tsc.SetGauge(player.timeEnergy);  // ← 강제 동기화
    Debug.Log($"[TPM] 에너지 게이지 동기화 완료: {player.timeEnergy}");
}

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
                candidate.transform.position = esd.position;
                candidate.transform.localScale = esd.localScale;

                // ExplosiveEnemy라면 추가 상태 초기화
                ExplosiveEnemy ex = candidate as ExplosiveEnemy;
                if (ex != null) ex.ResetOnRewind();

                // 죽은 적이라면 활성화
                if (candidate.isDead && !candidate.gameObject.activeInHierarchy)
                {
                    candidate.isDead = false;
                    candidate.gameObject.SetActive(true);
                }

                candidate.SetHealth(esd.health);

                // **피격 상태 리셋 호출**
                candidate.ResetDamageState();
                // 그리고 일반 상태로 복원 (ResumeTime() 호출)
                candidate.ResumeTime();

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

        Princess princess = FindFirstObjectByType<Princess>();
        Player player = FindFirstObjectByType<Player>();
        if (princess == null || player == null)
        {
            Debug.LogWarning("[TimePointManager] 부활 대상(공주/플레이어) 찾지 못함!");
            return;
        }

        player.ignoreInput = true;
        Vector2 targetPos = princess.transform.position;
        player.transform.position = targetPos;

        PlayerOver playerOver = FindFirstObjectByType<PlayerOver>();
        if (playerOver != null && playerOver.IsDisabled)
        {
            playerOver.OnRewindComplete(targetPos);
        }

        princess.isControlled = true;
        Rigidbody2D prb = princess.GetComponent<Rigidbody2D>();
        if (prb != null)
        {
            prb.linearVelocity = Vector2.zero;
        }

        CameraFollow cf = FindFirstObjectByType<CameraFollow>();
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

        Princess princess = FindFirstObjectByType<Princess>();
        Player player = FindFirstObjectByType<Player>();
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

        PlayerOver playerOver = FindFirstObjectByType<PlayerOver>();
        if (playerOver != null)
        {
            playerOver.ResumeAfterRewind();
        }
    }
}
