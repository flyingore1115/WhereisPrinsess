using UnityEngine;

public class TimePoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Princess"))
        {
            Princess princess = collision.GetComponent<Princess>();
            Player player = FindObjectOfType<Player>();
            if (princess != null && player != null)
            {
                TimePointManager.Instance.SaveCheckpoint(princess.transform.position, player.transform.position);

                // 즉시 부활 처리: 만약 플레이어가 행동불능 상태라면
                PlayerOver playerOver = FindObjectOfType<PlayerOver>();
                if (playerOver != null && playerOver.IsDisabled)
                {
                    TimePointManager.Instance.ImmediateRevive();
                }
            }
        }
    }

    
}
