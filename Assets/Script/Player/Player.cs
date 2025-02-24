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
}
