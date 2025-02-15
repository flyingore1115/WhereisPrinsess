using UnityEngine;

public class Enemy_bullet : MonoBehaviour
{

    public float lifetime = 5f;
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Bullet collided with: {collision.name}"); // 충돌 감지 확인 로그

        if (collision.CompareTag("Player"))
        {
            PlayerOver player = collision.GetComponent<PlayerOver>();
            if (player != null)
            {
                Debug.Log("Player found. Dealing damage."); // 데미지 처리 확인
                player.TakeDamage();
            }

            Destroy(gameObject); // 총알 제거
        }
        else if (collision.CompareTag("Princess"))
        {
            Princess princess = collision.GetComponent<Princess>();
            if (princess != null)
            {
                Debug.Log("Princess hit. Triggering Game Over."); // 공주 타격 확인
                princess.GameOver();
            }
            Destroy(gameObject);
        }
        
        else
        {
            Destroy(gameObject);
        }
    }

}
