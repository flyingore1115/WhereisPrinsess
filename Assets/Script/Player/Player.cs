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
        movement.HandleMovement(attack.IsAttacking);
        attack.HandleAttack();
        shooting.HandleShooting();
    }

    // 기존: 되감기 시 위치 복원
    public void RestoreFromRewind(Vector2 rewindPosition)
    {
        transform.position = rewindPosition;
    }

    // 추가: 저장된 상태(예: 탄약 수 등)를 복원하는 메서드
    public void RestoreState(GameStateData gameState)
    {
        if (shooting != null)
        {
            shooting.currentAmmo = gameState.playerBulletCount;
            // 필요하면 UI 업데이트 함수 호출 (예: shooting.UpdateAmmoUI();)
        }
        // 플레이어의 다른 상태(예: 체력, 스킬 등)도 복원할 수 있습니다.
    }
}
