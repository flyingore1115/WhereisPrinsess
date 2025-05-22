using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;

        if (other.CompareTag("Princess"))
        {
            Princess princess = other.GetComponent<Princess>();
            if (princess != null)
            {
                Debug.Log("[SpikeTrap] 공주 사망");
                princess.GameOver();
            }
        }
        else if (other.CompareTag("Player"))
        {
            PlayerOver playerOver = other.GetComponent<PlayerOver>();
            if (playerOver != null)
            {
                Debug.Log("[SpikeTrap] 플레이어 즉사 처리");
                playerOver.ForceSetHealth(0);
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            BaseEnemy enemy = other.GetComponent<BaseEnemy>();
            if (enemy != null && !enemy.isDead)
            {
                Debug.Log("[SpikeTrap] 적 사망");
                enemy.TakeDamage(9999);
            }
        }
    }
}
