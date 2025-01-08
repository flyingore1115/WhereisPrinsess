using UnityEngine;

public class Coin : MonoBehaviour
{
    public float floatSpeed = 1f; // 위아래로 움직이는 속도
    public float floatAmount = 0.5f; // 움직이는 범위
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // 위아래로 천천히 움직이게
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 점수 추가
            ScoreManager.Instance.AddScore(500);
            // 상태 텍스트 표시
            StatusTextManager.Instance.ShowMessage("코인을 획득했습니다!");

            // 코인 오브젝트 제거
            Destroy(gameObject);
        }
    }
}
