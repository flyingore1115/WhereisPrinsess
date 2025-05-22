using UnityEngine;

public class Enemy_bullet : MonoBehaviour, ITimeAffectable
{
    public float lifetime = 5f;
    private float elapsedLifetime = 0f;

    private Vector2 storedVelocity = Vector2.zero;

    private Rigidbody2D rb;
    private bool isTimeStopped = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 시간 정지 시스템에 등록
        TimeStopController tsc = FindFirstObjectByType<TimeStopController>();
        if (tsc != null)
        {
            tsc.RegisterTimeAffectedObject(this);
            if (tsc.IsTimeStopped)
            {
                StopTime(); // 게임 시작 시 시간 정지 상태라면 즉시 정지
            }
        }
    }

    void Start()
    {
        
    }

    void Update()
{
    if (isTimeStopped) return;

    elapsedLifetime += Time.deltaTime;
    if (elapsedLifetime >= lifetime)
    {
        Destroy(gameObject);
    }
}

    void FixedUpdate()
    {
        if (isTimeStopped) return;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("CameraBoundary"))
    {
        // 무시
        return;
    }
        Debug.Log($"Bullet collided with: {collision.name}");

        if (collision.CompareTag("Player"))
        {
            PlayerOver player = collision.GetComponent<PlayerOver>();
            if (player != null)
            {
                Debug.Log("Player found. Dealing damage.");
                player.TakeDamage(1);
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Princess"))
        {
            Princess princess = collision.GetComponent<Princess>();
            if (princess != null)
            {
                Debug.Log("Princess hit. Triggering Game Over.");
                princess.GameOver();
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Enemy"))
        {
            // 아무것도 안 함. (탄막 유지)
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // 시간 정지 시스템에서 제거
        TimeStopController timeStopController = FindFirstObjectByType<TimeStopController>();
        if (timeStopController != null)
        {
            timeStopController.RemoveTimeAffectedObject(this);
        }
    }

    // 시간 정지 기능 추가
public void StopTime()
{
    isTimeStopped = true;
    if (rb != null)
    {
        storedVelocity = rb.linearVelocity; // 현재 속도 저장
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
    }
}

public void ResumeTime()
{
    isTimeStopped = false;
    if (rb != null)
    {
        rb.simulated = true;
        rb.linearVelocity = storedVelocity; // 정지 전 속도로 복원
    }
}

}
