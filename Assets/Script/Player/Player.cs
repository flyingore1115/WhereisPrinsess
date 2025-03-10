using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D playerCollider;
    private SpriteRenderer spriteRenderer;
    private CameraShake cameraShake;

    public P_Movement movement;
    public P_Attack attack;
    public P_Shooting shooting;

    // 입력 무시 플래그: true일 경우 입력을 무시하고, 플레이어를 Idle 상태로 유지합니다.
    public bool ignoreInput = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        cameraShake = Camera.main.GetComponent<CameraShake>();

        movement.Init(rb, animator, spriteRenderer, shooting.firePoint);
        attack.Init(rb, playerCollider, cameraShake);
    }

    void Update()
    {
        // 조종 모드 활성 시, ignoreInput이 true로 설정되어 있다면
        if (ignoreInput)
        {
            // 강제로 플레이어의 이동과 애니메이션을 Idle 상태로 만듭니다.
            rb.velocity = Vector2.zero;
            animator.SetBool("isRun", false);
            return;
        }

        // 평소에는 정상적으로 입력 처리
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
}
