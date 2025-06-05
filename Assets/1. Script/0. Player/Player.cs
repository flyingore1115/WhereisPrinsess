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

    // ────────────────────────────────────────────────────
    // ■ 새로 추가: 공격 모드 구분 변수
    //    false = 근접(Attack) 모드, true = 사격(Shoot) 모드
    // ────────────────────────────────────────────────────
    private bool isShootingMode = false;
    public bool IsShootingMode
    {
        get { return isShootingMode; }
    }

    // ────────────────────────────────────────────────────
    // ■ 근접 공격 준비 관련 (시간정지 중에 사용하던 기존 코드)
    // ────────────────────────────────────────────────────
    private bool preparedAttack = false;
    private IDamageable preparedTarget = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴 방지
        }
        else
        {
            Destroy(gameObject);
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
        if (GameObject.FindGameObjectWithTag("Princess") == null)
        {
            Debug.LogError("[Player] 공주를 찾을 수 없습니다! 씬에서 올바르게 배치되었는지 확인하세요.");
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
        // 0. 시간정지 상태일 때: (기존의 “시간정지 중에는 특수 입력 허용” 로직 재사용)
        // ────────────────────────────────────────────────
        if (timeStopped)
        {
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool ctrl  = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            // (0-1) Shift + 좌클릭 → 정지된 총알 생성 (사격 모드 관계없이)
            if (Input.GetMouseButtonDown(0))
            {
                if (!PointerOverTagged("Enemy", "Princess"))
                    shooting.HandleShooting();
            }

            // (0-2) Ctrl + 좌클릭 → 손잡기 or 적 시간 해제 (기존 로직)
            if (Input.GetMouseButtonDown(0) && ctrl)
            {
                Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                wp.z = 0;
                RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);

                if (hit.collider)
                {
                    if (hit.collider.CompareTag("Princess"))
                    {
                        Princess pr = hit.collider.GetComponent<Princess>();
                        if (pr != null && !holdingPrincess)
                        {
                            holdingPrincess = true;
                            pr.StartBeingHeld();
                        }
                    }
                    else if (hit.collider.CompareTag("Enemy"))
                    {
                        BaseEnemy enemy = hit.collider.GetComponent<BaseEnemy>();
                        if (enemy != null)
                            enemy.ResumeTime();
                    }
                }
            }

            // (0-3) Ctrl 미사용 + 좌클릭 → 준비 공격 상태 (기존 근접 공격 준비)
            else if (Input.GetMouseButtonDown(0))
            {
                Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                wp.z = 0;
                RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);

                IDamageable target = hit.collider ? hit.collider.GetComponent<IDamageable>() : null;
                if (target != null)
                {
                    preparedAttack  = true;
                    preparedTarget  = target;
                    animator.SetTrigger("isPrepareAttack");
                }
            }

            // (0-4) 준비 공격 취소
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
                    preparedAttack  = false;
                    preparedTarget  = null;
                    animator.ResetTrigger("isPrepareAttack");
                }
            }

            return;
        }

        // ────────────────────────────────────────────────
        // 1. 시간정지 해제 후: 준비 공격 실행 (기존 근접 공격)
        // ────────────────────────────────────────────────
        if (!timeStopped && preparedAttack)
        {
            if (holdingPrincess)
            {
                StopHoldingPrincess();
                Princess.Instance?.StopBeingHeld();
            }

            animator.ResetTrigger("isPrepareAttack");
            animator.SetTrigger("isAttack");
            if (preparedTarget != null)
                attack.StartAttack(preparedTarget);

            preparedAttack = false;
            preparedTarget = null;
        }

        // ────────────────────────────────────────────────
        // 2. 일반 상태 입력 (이동 + 공격 모드 구분)
        // ────────────────────────────────────────────────
        // 2-1) 이동 처리
        movement.HandleMovement(attack.IsAttacking);

        // 2-2) 우클릭 → 공격 모드 토글 (근접 ↔ 사격)
        if (Input.GetMouseButtonDown(1))
        {
            isShootingMode = !isShootingMode;
        }

        // 2-3) R키 → 재장전 (사격 모드가 아니어도 R은 재장전으로 처리)
        if (Input.GetKeyDown(KeyCode.R))
        {
            shooting.Reload();
        }

        // 2-4) 좌클릭 → 모드별 처리
        if (Input.GetMouseButtonDown(0))
        {
            // “근접 모드”인 경우
            if (!isShootingMode)
            {
                Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                wp.z = 0f;
                RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);

                IDamageable target = null;
                if (hit.collider != null)
                    target = hit.collider.GetComponent<IDamageable>() ?? hit.collider.GetComponentInParent<IDamageable>() ?? hit.collider.GetComponentInChildren<IDamageable>();

                if (hit.collider && hit.collider.GetComponent<IDamageable>() != null)
                {
                    if (!holdingPrincess)
                        attack.StartAttack(target);
                }
                // 적이 아닌 빈 공간 클릭 시에는 아무 것도 하지 않음
            }
            // “사격 모드”인 경우
            else
            {
                // 실제 사격 로직(탄 생성)을 P_Shooting 쪽으로 위임
                shooting.HandleShooting();
            }
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
            shooting.currentAmmo = gameState.playerBulletCount;
        health     = gameState.playerHealth;
        timeEnergy = gameState.playerTimeEnergy;
    }

    public void StopTime()
    {
        if (animator != null && isGameOver)
            animator.speed = 0;
    }

    public void ResumeTime()
    {
        if (animator != null)
            animator.speed = 1;
    }

    bool PointerOverTagged(params string[] tags)
    {
        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0;
        RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);
        return hit.collider && System.Array.Exists(tags, t => hit.collider.CompareTag(t));
    }
}
