using UnityEngine;
using MyGame;

public class Player : MonoBehaviour, ITimeAffectable
{
    public static Player Instance { get; private set; }

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D playerCollider;
    private SpriteRenderer spriteRenderer;

    public P_Movement movement;
    public P_Attack attack;
    public P_Shooting shooting;

    public bool ignoreInput = false;
    public bool isGameOver = false;
    public bool holdingPrincess = false;

    public bool applyRewindGrayscale = false;

    public int health = 100;
    public float timeEnergy = 100f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지
        }
        else
        {
            Destroy(gameObject); // 중복 방지
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        movement.Init(rb, animator, spriteRenderer, shooting.firePoint);
        attack.Init(rb, playerCollider);

    }

    private void Start()
    {
        FindPrincess();
    }

    private void FindPrincess()
    {
        if (GameObject.FindGameObjectWithTag("Princess") != null)
        {
            //Debug.Log("[Player] 공주 참조를 찾았습니다.");
        }
        else
        {
            Debug.LogError("[Player] 공주를 찾을 수 없습니다! 씬에서 제대로 배치되었는지 확인하세요.");
        }
    }

    void Update()
    {
        if (ignoreInput)
        {
            animator.SetBool("isRun", false);
            return;
        }
        movement.HandleMovement(attack.IsAttacking);
        // 좌클릭(공격/사격) & 우클릭(재장전) 분기 처리[^4][^5][^6]
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            wp.z = 0f;
            var hit = Physics2D.Raycast(wp, Vector2.zero);
            if (hit.collider != null && hit.collider.GetComponent<BaseEnemy>() != null)
                attack.HandleAttack();
            else
                shooting.HandleShooting();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            shooting.HandleShooting();  // 재장전은 HandleShooting 내부에서 처리됨[^3]
        }
    }

    public void StartHoldingPrincess()
    {
        holdingPrincess = true;
    }

    public void StopHoldingPrincess()
    {
        holdingPrincess = false;
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
        health = gameState.playerHealth;
        timeEnergy = gameState.playerTimeEnergy;
    }

    public void StopTime()
    {
        if (PostProcessingManager.Instance != null)
            PostProcessingManager.Instance.ApplyTimeStop();
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
        if (PostProcessingManager.Instance != null)
            PostProcessingManager.Instance.SetDefaultEffects();
    }
}
