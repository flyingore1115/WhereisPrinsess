using UnityEngine;

public class Bullet : MonoBehaviour, ITimeAffectable
{
    public float speed = 30f;
    private Rigidbody2D rb;
    private bool isTimeStopped = false;
    private Vector2 direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // 발사 방향에 따라 각도 설정
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FixedUpdate()
    {
        if (isTimeStopped) return;

        rb.velocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject); // 적 제거
            Destroy(gameObject); // 총알도 제거

            // 시간 게이지 충전
            TimeStopController timeStopController = FindObjectOfType<TimeStopController>();
            if (timeStopController != null)
            {
                timeStopController.AddTimeGauge(5f);
            }
        }
    }


    void OnDestroy()
    {
        // 파괴될 때 리스트에서 제거 요청
        TimeStopController timeStopController = FindObjectOfType<TimeStopController>();
        if (timeStopController != null)
        {
            timeStopController.RemoveTimeAffectedObject(this);
        }
    }



    public void StopTime()
    {
        isTimeStopped = true;
        rb.simulated = false; // 물리 멈춤
    }

    public void ResumeTime()
    {
        isTimeStopped = false;
        rb.simulated = true; // 물리 재개
    }

    public void RestoreColor() { } // 총알은 색 변화 필요 없으니까 빈 상태
}
