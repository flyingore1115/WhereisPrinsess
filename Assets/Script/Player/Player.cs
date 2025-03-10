using UnityEngine;
using MyGame;

public class Player : MonoBehaviour, ITimeAffectable
{
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D playerCollider;
    private SpriteRenderer spriteRenderer;
    private CameraShake cameraShake;

    public P_Movement movement;
    public P_Attack attack;
    public P_Shooting shooting;

    // 입력 무시 플래그
    public bool ignoreInput = false;

    // 흑백 효과를 위한 마테리얼 (Inspector에서 할당)
    public Material grayscaleMaterial;
    private Material originalMaterial;

    // 되감기 모드일 때 흑백 효과 적용 여부 (시간정지 모드에서는 false)
    public bool applyRewindGrayscale = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        cameraShake = Camera.main.GetComponent<CameraShake>();

        movement.Init(rb, animator, spriteRenderer, shooting.firePoint);
        attack.Init(rb, playerCollider, cameraShake);

        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.sharedMaterial;
        }
    }

    void Update()
    {
        if (ignoreInput)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("isRun", false);
            return;
        }
        movement.HandleMovement(attack.IsAttacking);
        attack.HandleAttack();
        shooting.HandleShooting();
    }

    public void RestoreFromRewind(Vector2 rewindPosition)
    {
        transform.position = rewindPosition;
    }

    public void RestoreState(GameStateData gameState)
    {
        if (shooting != null)
        {
            shooting.currentAmmo = gameState.playerBulletCount;
        }
    }

    // ITimeAffectable 구현
    // 되감기 모드일 때만 applyRewindGrayscale가 true이면 흑백 효과 적용
    public void StopTime()
    {
        if (applyRewindGrayscale && spriteRenderer != null && grayscaleMaterial != null)
        {
            // 새로운 인스턴스로 생성해서 재질 문제 방지
            spriteRenderer.material = new Material(grayscaleMaterial);
            spriteRenderer.color = Color.white;
        }
    }

    public void ResumeTime()
    {
        RestoreColor();
    }

    public void RestoreColor()
    {
        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
        }
    }
}
