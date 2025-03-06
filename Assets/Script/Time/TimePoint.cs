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
            }
        }
    }
}
