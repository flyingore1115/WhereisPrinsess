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

    private float snapshotInterval = 0.02f;
    private float lastSnapshotTime = 0f;

    private List<ITimeAffectable> timeAffectedObjects = new List<ITimeAffectable>();

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
        playerAnimator = player.GetComponent<Animator>();
        princessAnimator = princess.GetComponent<Animator>();
        FindTimeAffectedObjects();
    }

    void FixedUpdate()
    {
        if (!isRewinding && Time.time - lastSnapshotTime >= snapshotInterval)
        {
            RecordSnapshot();
            lastSnapshotTime = Time.time;
        }
    }

    void RecordSnapshot()
    {
        if (player == null || princess == null) return;

        Rigidbody2D pRb = player.GetComponent<Rigidbody2D>();
        Rigidbody2D cRb = princess.GetComponent<Rigidbody2D>();

        TimeSnapshot snapshot = new TimeSnapshot();
        snapshot.playerPosition = player.transform.position;
        snapshot.princessPosition = princess.transform.position;
        snapshot.playerVelocity = (pRb != null) ? pRb.velocity : Vector2.zero;
        snapshot.princessVelocity = (cRb != null) ? cRb.velocity : Vector2.zero;

        if (playerAnimator != null)
        {
            snapshot.playerAnimationState = playerAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash.ToString();
            snapshot.playerNormalizedTime = playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }

        if (princessAnimator != null)
        {
            snapshot.princessAnimationState = princessAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash.ToString();
            snapshot.princessNormalizedTime = princessAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }

        snapshots.Add(snapshot);

        float maxCount = recordTime / snapshotInterval;
        if (snapshots.Count > maxCount)
        {
            snapshots.RemoveAt(0);
        }
    }

    public void StartRewind()
    {
        if (!isRewinding)
        {
            if (TimePointManager.Instance.HasCheckpoint())
            {
                Vector3 checkpointPos = TimePointManager.Instance.GetLastCheckpointData().playerPosition;
                snapshots = snapshots.FindAll(snap => snap.playerPosition.x >= checkpointPos.x);
            }
            StartCoroutine(RewindCoroutine());
        }
    }

    IEnumerator RewindCoroutine()
    {
        isRewinding = true;
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.2f;

        Rigidbody2D pRb = player.GetComponent<Rigidbody2D>();
        Rigidbody2D cRb = princess.GetComponent<Rigidbody2D>();

        if (pRb != null) pRb.isKinematic = true;
        if (cRb != null) cRb.isKinematic = true;

        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.ignoreInput = true;
            playerScript.applyRewindGrayscale = true;
        }

        ApplyGrayscaleEffect(true);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLoopSFX("rewind");
        }

        for (int i = snapshots.Count - 1; i > 0; i--)
        {
            TimeSnapshot snap1 = snapshots[i];
            TimeSnapshot snap2 = snapshots[i - 1];

            player.transform.position = Vector3.Lerp(snap1.playerPosition, snap2.playerPosition, 0.5f);
            princess.transform.position = Vector3.Lerp(snap1.princessPosition, snap2.princessPosition, 0.5f);

            if (pRb != null) pRb.velocity = Vector2.Lerp(snap1.playerVelocity, snap2.playerVelocity, 0.5f);
            if (cRb != null) cRb.velocity = Vector2.Lerp(snap1.princessVelocity, snap2.princessVelocity, 0.5f);

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

        snapshots.Clear();

        if (pRb != null) pRb.isKinematic = false;
        if (cRb != null) cRb.isKinematic = false;
        if (playerScript != null)
        {
            playerScript.ignoreInput = false;
            playerScript.applyRewindGrayscale = false;
        }

        ApplyGrayscaleEffect(false);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopLoopSFX("rewind");
            Debug.Log("효과음 멈춤");
        }

        Time.timeScale = originalTimeScale;
        isRewinding = false;
    }

    void FindTimeAffectedObjects()
    {
        timeAffectedObjects = FindObjectsOfType<MonoBehaviour>().OfType<ITimeAffectable>().ToList();
    }

    void ApplyGrayscaleEffect(bool isGrayscale)
    {
        foreach (var obj in timeAffectedObjects)
        {
            if (isGrayscale)
            {
                obj.StopTime();
            }
            else
            {
                obj.ResumeTime();
            }
        }
    }
}
