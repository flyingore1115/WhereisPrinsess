using UnityEngine;

public class MoveTimeAffectable : MonoBehaviour, ITimeAffectable
{
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb;
    private Material originalMaterial;
    private Material grayScaleMaterial;
    private bool isTimeStopped = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        originalMaterial = spriteRenderer.material;
        grayScaleMaterial = Resources.Load<Material>("GrayScaleMaterial");

        if (grayScaleMaterial == null)
        {
            Debug.LogError("GrayScaleMaterial을 Resources 폴더에서 찾을 수 없습니다!");
        }
    }

    public void StopTime()
    {
        if (this == null || spriteRenderer == null) return;

        isTimeStopped = true;
        if (grayScaleMaterial != null)
        {
            spriteRenderer.material = grayScaleMaterial;
        }

        if (animator != null)
        {
            animator.speed = 0;
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            // 기존의 rb.simulated = false; 호출을 제거하여, 콜라이더가 여전히 물리 계산에 포함되도록 함.
        }
    }

    public void ResumeTime()
    {
        if (this == null || spriteRenderer == null) return;

        isTimeStopped = false;
        RestoreColor();

        if (animator != null)
        {
            animator.speed = 1;
        }

        if (rb != null)
        {
            // rb.simulated = true; 호출 없이 속도만 0으로 재설정
            rb.velocity = Vector2.zero; // 혹시 남아있는 힘 제거
        }
    }

    public void RestoreColor()
    {
        if (this == null || spriteRenderer == null) return;

        spriteRenderer.material = originalMaterial;
    }
}
