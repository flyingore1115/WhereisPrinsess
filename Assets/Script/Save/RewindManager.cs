using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MyGame;
using UnityEngine.SceneManagement;

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

    // 예시) 기존 0.001f가 너무 빠를 수 있으므로 0.02~0.03으로 조정
    private float snapshotInterval = 0.02f;
    private float lastSnapshotTime = 0f;

    private bool isGameOver = false; // 게임오버 상태

    private List<ITimeAffectable> timeAffectedObjects = new List<ITimeAffectable>();

    public TimeStopController timeStopController; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[RewindManager] Awake -> Instance 할당");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[RewindManager] OnSceneLoaded -> " + scene.name);
        AssignSceneObjects();
    }

    void AssignSceneObjects()
    {
        Debug.Log("[RewindManager] AssignSceneObjects 호출");

        var foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer == null)
            Debug.LogWarning("[RewindManager] Player 태그를 가진 오브젝트가 없음");
        else
            Debug.Log("[RewindManager] Found Player = " + foundPlayer.name);

        var foundPrincess = GameObject.FindGameObjectWithTag("Princess");
        if (foundPrincess == null)
            Debug.LogWarning("[RewindManager] Princess 태그를 가진 오브젝트가 없음");
        else
            Debug.Log("[RewindManager] Found Princess = " + foundPrincess.name);

        player = foundPlayer;
        princess = foundPrincess;

        if (player != null)
            playerAnimator = player.GetComponent<Animator>();
        if (princess != null)
            princessAnimator = princess.GetComponent<Animator>();

        FindTimeAffectedObjects();
    }

    void FixedUpdate()
    {
        if (isGameOver) return;
        if (timeStopController != null && timeStopController.IsTimeStopped) return;
        if (isRewinding) return;

        if (Time.time - lastSnapshotTime >= snapshotInterval)
        {
            RecordSnapshot();
            lastSnapshotTime = Time.time;
        }
    }

    public bool IsGameOver() { return isGameOver; }

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

        Player pScript = player ? player.GetComponent<Player>() : null;
        if (pScript != null)
        {
            pScript.ignoreInput = true;
            pScript.applyRewindGrayscale = true;
        }
        if (timeStopController != null && timeStopController.IsTimeStopped)
        {
            timeStopController.SendMessage("ResumeTime", SendMessageOptions.DontRequireReceiver);
        }

        ApplyGrayscaleEffect(true);

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
                playerAnimator.Play(int.Parse(snap1.playerAnimationState), 0, snap1.playerNormalizedTime);
            if (princessAnimator != null)
                princessAnimator.Play(int.Parse(snap1.princessAnimationState), 0, snap1.princessNormalizedTime);

            yield return new WaitForSecondsRealtime(rewindFrameDelay);
        }

        // 되감기 끝 -> 체크포인트 복원
        if (TimePointManager.Instance.HasCheckpoint())
        {
            yield return StartCoroutine(TimePointManager.Instance.ApplyCheckpoint(
                TimePointManager.Instance.GetLastCheckpointData(), 
                false
            ));
        }

        // 이제 플레이어가 여전히 Disable 상태이면 OnRewindComplete() 호출
        PlayerOver playerOver = FindObjectOfType<PlayerOver>();
        if (playerOver != null && playerOver.IsDisabled)
        {
            Debug.Log("[RewindManager] 되감기 후에도 플레이어가 Disable -> OnRewindComplete 호출");
            playerOver.OnRewindComplete(playerOver.transform.position);
        }

        snapshots.Clear();
        Debug.Log("[RewindManager] Rewind End");

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
            if (isGrayscale) obj.StopTime();
            else obj.ResumeTime();
        }
    }
}
