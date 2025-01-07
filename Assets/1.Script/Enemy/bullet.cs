using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 5f; // 총알의 최대 수명

    void Start()
    {
        // 일정 시간 후 총알 파괴
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player hit by bullet!");
            Destroy(gameObject); 
        }
        else if (collision.CompareTag("Princess"))
        {
            Debug.Log("Princess hit by bullet!");
            Princess princessScript = collision.GetComponent<Princess>();
            if (princessScript != null)
            {
                princessScript.GameOver(); // 공주 사망 처리
            }
            Destroy(gameObject); // 공주에 닿으면 총알 파괴
        }
        else
        {
            // 벽이나 다른 오브젝트에 닿으면 총알 파괴
            Destroy(gameObject);
        }
    }
}
