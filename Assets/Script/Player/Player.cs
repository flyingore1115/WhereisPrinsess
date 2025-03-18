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

    // 입력 무시 플래그 및 게임오버 상태
    public bool ignoreInput = false;
    public bool isGameOver = false;

    // 흑백 효과용 마테리얼
    public Material grayscaleMaterial;
    private Material originalMaterial;

    // 되감기 시 흑백 효과 적용 여부
    public bool applyRewindGrayscale = false;

    // 플레이어 상태: 체력, 시간 에너지 등 (필요에 따라 값 수정)
    public int health = 100;
    public float timeEnergy = 100f;

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

    // 게임 상태 복원을 위한 메서드 (예: 총알 수, 체력, 시간 에너지)
    public void RestoreState(GameStateData gameState)
    {
        if (shooting != null)
        {
            shooting.currentAmmo = gameState.playerBulletCount;
        }
        health = gameState.playerHealth;
        timeEnergy = gameState.playerTimeEnergy;
    }

    // ITimeAffectable 구현 – 되감기 모드와 게임오버 모드를 구분하여 처리
    public void StopTime()
    {
        // 되감기 모드에서는 applyRewindGrayscale 조건으로 처리하고,
        // 게임오버 시(isGameOver true)에는 강제로 그레이스케일 및 애니메이션 정지
        if ((applyRewindGrayscale || isGameOver) && spriteRenderer != null && grayscaleMaterial != null)
        {
            spriteRenderer.material = new Material(grayscaleMaterial);
            spriteRenderer.color = Color.white;
        }
        if (animator != null && isGameOver)
        {
            animator.speed = 0;
        }
    }

    public void ResumeTime()
    {
        if (animator != null)
        {
            animator.speed = 1;
        }
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
