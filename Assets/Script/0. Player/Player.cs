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

    private bool preparedAttack = false;
    private IDamageable preparedTarget = null;

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

        bool timeStopped = TimeStopController.Instance != null && TimeStopController.Instance.IsTimeStopped;

        // ────────────────────────────────────────────────
        // 0. 시간정지 상태일 때 (모든 입력 제한 + 특수 입력 허용)
        // ────────────────────────────────────────────────
        if (timeStopped)
        {
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            // 0-1. Shift + 좌클릭 → 정지된 총알 생성
            if (Input.GetMouseButtonDown(0))
            {
                if (!PointerOverTagged("Enemy", "Princess"))
                    shooting.HandleShooting();
            }

            // 0-2. Ctrl + 좌클릭 → 손잡기 or 적 시간 해제
            if (Input.GetMouseButtonDown(0) && ctrl)
            {
                Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition); wp.z = 0;
                RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);

                if (hit.collider)
                {
                    // Princess 클릭 → 손잡기 시작
                    if (hit.collider.CompareTag("Princess"))
                    {
                        Princess pr = hit.collider.GetComponent<Princess>();
                        if (pr != null && !holdingPrincess)
                        {
                            holdingPrincess = true;
                            pr.StartBeingHeld();
                        }
                    }
                    // Enemy 클릭 → 해당 적만 ResumeTime()
                    else if (hit.collider.CompareTag("Enemy"))
                    {
                        BaseEnemy enemy = hit.collider.GetComponent<BaseEnemy>();
                        if (enemy != null)
                            enemy.ResumeTime();
                    }
                }
            }

            // 0-3. Ctrl 미사용 + 좌클릭 → 준비 공격 상태
            else if (Input.GetMouseButtonDown(0))
            {
                Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition); wp.z = 0;
                RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);
                IDamageable target = hit.collider ? hit.collider.GetComponent<IDamageable>() : null;
                if (target != null)
                {
                    preparedAttack = true;
                    preparedTarget = target;
                    animator.SetTrigger("prepareAttack"); // 준비 포즈
                }
            }

            // 0-4. 준비 공격 취소: 특정 입력 시만
            if (preparedAttack)
            {
                bool cancel =
                    Input.GetMouseButtonDown(1) ||
                    Input.GetKeyDown(KeyCode.A) ||
                    Input.GetKeyDown(KeyCode.W) ||
                    Input.GetKeyDown(KeyCode.D) ||
                    Input.GetKeyDown(KeyCode.R);

                if (cancel)
                {
                    preparedAttack = false;
                    preparedTarget = null;
                    animator.ResetTrigger("prepareAttack");
                }
            }

            return; // 시간정지 중에는 나머지 입력 차단
        }

        // ────────────────────────────────────────────────
        // 1. 시간정지 해제 후 → 준비 공격 실행
        // ────────────────────────────────────────────────
        if (!timeStopped && preparedAttack)
        {
            // 손잡기 상태라면 해제
            if (holdingPrincess)
            {
                StopHoldingPrincess();
                Princess.Instance?.StopBeingHeld();
            }

            // 공격 애니메이션
            animator.ResetTrigger("prepareAttack");
            animator.SetTrigger("attack");

            if (preparedTarget != null)
                StartCoroutine(attack.MoveToEnemyAndAttack(preparedTarget));

            preparedAttack = false;
            preparedTarget = null;
        }

        // ────────────────────────────────────────────────
        // 2. 일반 상태 입력 (이동, 공격, 사격)
        // ────────────────────────────────────────────────
        movement.HandleMovement(attack.IsAttacking);

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition); wp.z = 0f;
            var hit = Physics2D.Raycast(wp, Vector2.zero);

            var ho = hit.collider ? hit.collider.GetComponent<HallwayObstacle>() : null;

            if (hit.collider && hit.collider.GetComponent<IDamageable>() != null)
            {
                // ▸ 손잡기 중이면 공격 자체를 막는다
                if (!holdingPrincess)
                    attack.HandleAttack();
            }
            else
            {
                // ▸ IDamageable이 없고, 손잡기· 아님 ⇒ 사격
                if (!holdingPrincess)
                    shooting.HandleShooting();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            shooting.HandleShooting();        // 우클릭 = 재장전
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

    bool PointerOverTagged(params string[] tags)
{
    Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition); wp.z = 0;
    RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);
    return hit.collider && System.Array.Exists(tags, t => hit.collider.CompareTag(t));
}
}
