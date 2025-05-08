using UnityEngine;

public class Bullet : MonoBehaviour, ITimeAffectable
{
    public float speed = 30f;
    public float lifetime = 5f;  // 총알 생명주기
    private float timer = 0f;
    private Rigidbody2D rb;
    private bool isTimeStopped = false;
    private Vector2 direction;
    public int damageAmount = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // TimeStopController에 자신 등록
        TimeStopController tsc = FindFirstObjectByType<TimeStopController>();
        if (tsc != null)
        {
            tsc.RegisterTimeAffectedObject(this);
            if (tsc.IsTimeStopped)
            {
                StopTime();
            }
        }
    }

    void Update()
    {
        // 시간 정지 중이면 lifetime 타이머 업데이트를 멈춤
        if (!isTimeStopped)
        {
            timer += Time.deltaTime;
            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }

    void FixedUpdate()
    {
        if (isTimeStopped) return;
        rb.linearVelocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("Bullet hit Enemy!");

            BaseEnemy enemy = collision.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
            }

            Destroy(gameObject);
        }
    }


    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnDestroy()
    {
        // 파괴될 때 TimeStopController 목록에서 제거
        TimeStopController timeStopController = FindFirstObjectByType<TimeStopController>();
        if (timeStopController != null)
        {
            timeStopController.RemoveTimeAffectedObject(this);
        }
    }

    public void StopTime()
    {
        isTimeStopped = true;
        rb.linearVelocity = Vector2.zero; // 속도 0 설정
        rb.simulated = false;       // 물리 시뮬레이션 중단
    }

    public void ResumeTime()
    {
        isTimeStopped = false;
        rb.simulated = true;        // 물리 시뮬레이션 재개
    }
}
