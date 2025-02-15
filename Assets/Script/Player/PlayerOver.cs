using UnityEngine;

public class PlayerOver : MonoBehaviour
{
    public int maxHealth = 3; // 최대 체력
    private int currentHealth;
    public Camera mainCamera; // 메인 카메라
    public Transform princess; // 공주의 Transform
    public float cameraMoveSpeed = 5f; // 카메라 이동 속도

    private bool isDisabled = false; // 행동 불능 상태
    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void TakeDamage()
    {
        if (isDisabled) return;

        currentHealth--;
        Debug.Log($"Player Health: {currentHealth}");
        StatusTextManager.Instance.ShowMessage($"메이드의 남은 체력: {currentHealth}");


        // 체력이 0 이하가 되면 행동 불능
        if (currentHealth <= 0)
        {
            DisablePlayer();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("fall")) // 구멍과 충돌
        {
            DisablePlayer();
        }
    }

    private void DisablePlayer()
    {
        if (isDisabled) return; // 이미 행동 불능 상태라면 실행하지 않음

        isDisabled = true;
        StatusTextManager.Instance.ShowMessage("메이드가 행동불능이 되었습니다!");

        // 이동 및 조작 중지
        rb.velocity = Vector2.zero; // 움직임 정지
        rb.gravityScale = 0; // 중력 해제
        GetComponent<Player>().enabled = false; // 이동 및 공격 스크립트 비활성화

        // 카메라를 공주에게 이동
        StartCoroutine(MoveCameraToPrincess());
    }

    private System.Collections.IEnumerator MoveCameraToPrincess()
    {
        Vector3 targetPosition = princess.position + new Vector3(0, 0, -10);

        while (Vector3.Distance(mainCamera.transform.position, targetPosition) > 0.1f)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, cameraMoveSpeed * Time.unscaledDeltaTime);
            yield return null;
        }

        Debug.Log("Camera moved to Princess.");
    }
}
