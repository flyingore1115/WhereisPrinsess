using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MyGame;

public class RewindManager : MonoBehaviour
{
    public static RewindManager Instance;

    [Header("Recording Settings")]
    public float recordTime = 5f; 
    private List<TimeSnapshot> snapshots = new List<TimeSnapshot>();

    [Header("References")]
    public GameObject player;
    public GameObject princess;

    private Animator playerAnimator;
    private Animator princessAnimator;

    [Header("Rewind Settings")]
    public float rewindFrameDelay = 0.0167f;
    private bool isRewinding = false;
    public bool IsRewinding { get { return isRewinding; } }

    private float snapshotInterval = 0.033f;
    private float lastSnapshotTime = 0f;

    private bool isGameOver = false; // 게임오버 상태

    private List<ITimeAffectable> timeAffectedObjects = new List<ITimeAffectable>();

    // ★ TimeStopController 연결(필요시)
    //    이 값이 null이 아니면, "시간정지 중"인지 가져다 쓸 수 있다.
    public TimeStopController timeStopController; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (player != null)
            playerAnimator = player.GetComponent<Animator>();
        if (princess != null)
            princessAnimator = princess.GetComponent<Animator>();

        // 필요하다면 수동 연결
        timeStopController = FindObjectOfType<TimeStopController>();

        FindTimeAffectedObjects();
    }

    void FixedUpdate()
    {
        // (1) "게임오버 중"이면 스냅샷 기록 안 함
        if (isGameOver) return;

        // (2) "시간정지 중"이면 스냅샷 기록 안 함
        if (timeStopController != null && timeStopController.IsTimeStopped)
        {
            return;
        }

        // (3) "되감기 중"이면 스냅샷 기록 안 함 (선택)
        if (isRewinding) return;

        // 그 외에만 스냅샷 기록
        if (Time.time - lastSnapshotTime >= snapshotInterval)
        {
            RecordSnapshot();
            lastSnapshotTime = Time.time;
        }
    }

    /// <summary>
    /// "게임오버인지" 외부가 확인할 때 쓸 수 있는 메서드
    /// </summary>
    public bool IsGameOver()
    {
        return isGameOver;
    }

    /// <summary>
    /// 스냅샷 하나 기록
    /// </summary>
    void RecordSnapshot()
    {
        if (player == null || princess == null) return;

        Rigidbody2D pRb = player.GetComponent<Rigidbody2D>();
        Rigidbody2D cRb = princess.GetComponent<Rigidbody2D>();

        TimeSnapshot snap = new TimeSnapshot();
        snap.playerPosition   = player.transform.position;
        snap.princessPosition = princess.transform.position;
        snap.playerVelocity   = (pRb != null) ? pRb.velocity : Vector2.zero;
        snap.princessVelocity = (cRb != null) ? cRb.velocity : Vector2.zero;

        if (playerAnimator != null)
        {
            var st = playerAnimator.GetCurrentAnimatorStateInfo(0);
            snap.playerAnimationState = st.shortNameHash.ToString();
            snap.playerNormalizedTime = st.normalizedTime;
        }
        if (princessAnimator != null)
        {
            var st2 = princessAnimator.GetCurrentAnimatorStateInfo(0);
            snap.princessAnimationState = st2.shortNameHash.ToString();
            snap.princessNormalizedTime = st2.normalizedTime;
        }

        snapshots.Add(snap);

        float maxCount = recordTime / snapshotInterval;
        if (snapshots.Count > maxCount)
            snapshots.RemoveAt(0);
    }

    public void ClearSnapshots()
    {
        snapshots.Clear();
        Debug.Log("[RewindManager] ClearSnapshots() - 이전 스냅샷 전부 제거");
    }

    public void ClearSnapshotsAndRecordOne()
    {
        snapshots.Clear();
        Debug.Log("[RewindManager] ClearSnapshotsAndRecordOne() - 스냅샷 클리어 후 한 장 강제 기록");
        RecordSnapshot();
    }

    /// <summary>
    /// 게임오버 -> 스냅샷 기록 막기
    /// </summary>
    public void SetGameOver(bool val)
    {
        isGameOver = val;
        Debug.Log("[RewindManager] SetGameOver=" + val);
    }

    public void StartRewind()
    {
        if (snapshots.Count < 2)
        {
            Debug.LogWarning($"스냅샷이 {snapshots.Count}개 -> 역재생 애니 없음");
            if (TimePointManager.Instance.HasCheckpoint())
            {
                StartCoroutine(OnlyCheckpointRestore());
            }
            return;
        }

        if (!isRewinding)
        {
            int n = Mathf.Min(Mathf.RoundToInt(recordTime / snapshotInterval), snapshots.Count);
            if (n > 0)
            {
                snapshots = snapshots.Skip(snapshots.Count - n).ToList();
            }

            StartCoroutine(RewindCoroutine());
        }
    }

    IEnumerator OnlyCheckpointRestore()
    {
        Debug.Log("[RewindManager] 스냅샷 부족 -> 체크포인트 복원만 수행");
        if (TimePointManager.Instance.HasCheckpoint())
        {
            yield return StartCoroutine(TimePointManager.Instance.ApplyCheckpoint(
                TimePointManager.Instance.GetLastCheckpointData(),
                false
            ));
        }
    }

    IEnumerator RewindCoroutine()
    {
        // 로컬 복사
        List<TimeSnapshot> localSnapshots = new List<TimeSnapshot>(snapshots);
        if (localSnapshots.Count < 2)
        {
            Debug.LogWarning("RewindCoroutine: 스냅샷 2장 미만 -> 체크포인트 복원");
            if (TimePointManager.Instance.HasCheckpoint())
            {
                yield return StartCoroutine(TimePointManager.Instance.ApplyCheckpoint(
                    TimePointManager.Instance.GetLastCheckpointData(), false
                ));
            }
            yield break;
        }

        isRewinding = true;
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.3f;

        Rigidbody2D pRb = (player) ? player.GetComponent<Rigidbody2D>() : null;
        Rigidbody2D cRb = (princess) ? princess.GetComponent<Rigidbody2D>() : null;
        if (pRb != null) pRb.isKinematic = true;
        if (cRb != null) cRb.isKinematic = true;

        // 플레이어(시간정지도 불가) 
        Player pScript = player.GetComponent<Player>();
        if (pScript != null)
        {
            pScript.ignoreInput = true;
            pScript.applyRewindGrayscale = true;
        }
        // ★추가: TimeStopController도 잠금
        if (timeStopController != null && timeStopController.IsTimeStopped)
        {
            // 되감기 시작 시 시간정지 자동 해제
            timeStopController.SendMessage("ResumeTime", SendMessageOptions.DontRequireReceiver);
        }

        ApplyGrayscaleEffect(true);

        // 역재생 루프
        for (int i = localSnapshots.Count - 1; i > 0; i--)
        {
            TimeSnapshot snap1 = localSnapshots[i];
            TimeSnapshot snap2 = localSnapshots[i - 1];

            player.transform.position   = Vector3.Lerp(snap1.playerPosition,   snap2.playerPosition,   0.5f);
            princess.transform.position = Vector3.Lerp(snap1.princessPosition, snap2.princessPosition, 0.5f);

            if (pRb != null)
                pRb.velocity = Vector2.Lerp(snap1.playerVelocity,   snap2.playerVelocity,   0.5f);
            if (cRb != null)
                cRb.velocity = Vector2.Lerp(snap1.princessVelocity, snap2.princessVelocity, 0.5f);

            if (playerAnimator != null)
            {
                playerAnimator.Play(int.Parse(snap1.playerAnimationState), 0, snap1.playerNormalizedTime);
            }
            if (princessAnimator != null)
            {
                princessAnimator.Play(int.Parse(snap1.princessAnimationState), 0, snap1.princessNormalizedTime);
            }

            yield return new WaitForSecondsRealtime(rewindFrameDelay);
        }

        // 루프 끝 -> 체크포인트 복원
        if (TimePointManager.Instance.HasCheckpoint())
        {
            yield return StartCoroutine(TimePointManager.Instance.ApplyCheckpoint(
                TimePointManager.Instance.GetLastCheckpointData(), false
            ));
        }

        snapshots.Clear();

        if (pRb != null) pRb.isKinematic = false;
        if (cRb != null) cRb.isKinematic = false;

        if (pScript != null)
        {
            pScript.ignoreInput = false;
            pScript.applyRewindGrayscale = false;
        }

        ApplyGrayscaleEffect(false);

        Time.timeScale = originalTimeScale;
        isRewinding = false;
        Debug.Log("[RewindManager] Rewind End");
    }

    void FindTimeAffectedObjects()
    {
        timeAffectedObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<ITimeAffectable>()
            .ToList();
    }

    void ApplyGrayscaleEffect(bool isGrayscale)
    {
        foreach (var obj in timeAffectedObjects)
        {
            if (isGrayscale)
                obj.StopTime();
            else
                obj.ResumeTime();
        }
    }
}
