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

    private bool       preparedAttack = false;
    private GameObject preparedTarget = null;

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

        bool timeStopped = TimeStopController.Instance != null &&TimeStopController.Instance.IsTimeStopped;

                /* ──────────────────────────────────────────
           Ⅰ.  시간정지 상태 입력 처리
        ──────────────────────────────────────────*/
        if (timeStopped)
        {
            
           // 1) Ctrl + 좌클릭  ─ Princess or Enemy 선택
            if (Input.GetMouseButtonDown(0) &&
                (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition); wp.z = 0;
                RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);

                if (hit.collider)
                {
                    // (a) 공주 클릭 → 손잡기
                    if (hit.collider.CompareTag("Princess"))
                    {
                        Princess pr = hit.collider.GetComponent<Princess>();
                        if (pr != null && !holdingPrincess)
                        {
                            holdingPrincess = true;
                            pr.StartBeingHeld();
                        }
                    }
                    // (b) 적 클릭 → 그 적만 시간정지 해제
                    else if (hit.collider.CompareTag("Enemy"))
                    {
                        BaseEnemy en = hit.collider.GetComponent<BaseEnemy>();
                        if (en != null) en.ResumeTime();
                    }
                }
            }
            // 2) Ctrl 미사용 좌클릭  → Prepared-Attack 설정
            else if (Input.GetMouseButtonDown(0) &&
                     !Input.GetKey(KeyCode.LeftControl) &&
                     !Input.GetKey(KeyCode.RightControl))
            {
                Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition); wp.z = 0;
                RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);
                BaseEnemy enemy = hit.collider ? hit.collider.GetComponent<BaseEnemy>() : null;
                if (enemy != null && !enemy.isDead)
                {
                    preparedAttack = true;
                    preparedTarget = enemy.gameObject;
                    //준비 공격 애니메이션 트리거 ※
                    //if (animator != null)
                    //    animator.SetTrigger("prepareAttack");
                }
            }

           /* 3) 준비상태 취소 : 임의의 다른 입력 */
           if (preparedAttack && (Input.anyKeyDown || Input.GetMouseButtonDown(1)))
           {
               // Space(시간 해제)는 허용, 나머지는 취소
                bool cancel =
                    Input.GetMouseButtonDown(1) ||                     // 우클릭
                    Input.GetKeyDown(KeyCode.Escape) ||
                    Input.GetKeyDown(KeyCode.Q)     ||
                    Input.GetKeyDown(KeyCode.E)     ||
                    Input.GetKeyDown(KeyCode.R);

                if (cancel)
                {
                    preparedAttack = false;
                    preparedTarget = null;
                    if (animator != null)
                        animator.ResetTrigger("prepareAttack");
                }
           }

           // 시간정지 중에는 Move/Attack/Shooting 모두 금지
           return;
       } // ─── (timeStopped) 블록 끝 ───

        /* ──────────────────────────────────────────
           Ⅱ.  시간정지가 해제된 뒤 준비 공격 실행
        ──────────────────────────────────────────*/
        if (!timeStopped && preparedAttack)
        {
           // 수정: 손잡기 상태면 해제
           if (holdingPrincess)
           {
               StopHoldingPrincess();
               Princess.Instance?.StopBeingHeld();
           }
           
            //준비 상태 트리거 종료 + 공격 트리거 발동 ※
            if (animator != null)
            {
                animator.ResetTrigger("prepareAttack");
                animator.SetTrigger("attack");
            }

           if (preparedTarget != null)
                StartCoroutine(attack.MoveToEnemyAndAttack(preparedTarget));

           preparedAttack = false;
           preparedTarget = null;
       }

        /* ──────────────────────────────────────────
           Ⅲ.  평상시 입력 (기존 로직 그대로)
        ──────────────────────────────────────────*/

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
