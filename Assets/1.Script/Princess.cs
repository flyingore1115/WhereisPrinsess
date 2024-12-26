using UnityEngine;

public class Princess : MonoBehaviour
{
    public float moveSpeed = 3f;
    public GameObject gameOverScreen; // 게임 오버 화면 UI 오브젝트
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private bool isGameOver = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 게임 오버 화면 비활성화
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }

        // 플레이어 오브젝트를 찾아 충돌 비활성화
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider2D>();
            Collider2D princessCollider = GetComponent<Collider2D>();

            if (playerCollider != null && princessCollider != null)
            {
                Physics2D.IgnoreCollision(princessCollider, playerCollider);
            }
        }
    }

    void Update()
    {
        if (FindObjectOfType<DebugManager>()?.IsStop() == true)
        {
        }
        else{
            if (!isGameOver)
            {
                // Princess moves to the right continuously
                rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
            }
            Debug.Log("Princess is invincible!");
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isGameOver && other.CompareTag("Enemy")) // 적과 충돌 확인
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        if (FindObjectOfType<DebugManager>()?.IsInvincible() == true)
        {
            Debug.Log("Princess is invincible!");
            return;
        }
        else
        {
            isGameOver = true;
            Debug.Log("Game Over"); // 콘솔에 게임 오버 출력

            // GameOverManager 호출
            GameOverManager gameOverManager = FindObjectOfType<GameOverManager>();
            if (gameOverManager != null)
            {
                gameOverManager.TriggerGameOver(); // 게임 오버 효과 시작
            }
        }
    }
}
