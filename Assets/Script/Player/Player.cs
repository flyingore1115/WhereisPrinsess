using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    public TMP_Text ammoText;
    //이동
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    //공격
    public LayerMask groundLayer;
    public float attackRange = 3f;
    public float attackMoveSpeed = 15f;
    private bool isGrounded;
    private bool isAttacking = false;

    public GameObject bulletPrefab; // 총알 프리팹
    public Transform firePoint; // 발사 위치 (플레이어 중심 or 총구)

    public int currentAmmo = 6;
    public int maxAmmo = 6; //총탄 얼마나

    //적
    private ExplosiveEnemy[] explosiveEnemies;
    private SniperEnemy[] sniperEnemies;

    //컴포넌트
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D playerCollider;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    void Update()
    {
        if (!isAttacking)
        {
            float moveInput = Input.GetAxis("Horizontal");
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

            // 방향키 입력에 따라 애니메이션 상태 설정
            if (moveInput != 0)
            {
                animator.SetBool("isRun", true);

                // 좌우 방향 전환 (Sprite만 뒤집기)
                if (moveInput > 0)
                {
                    spriteRenderer.flipX = false; // 오른쪽 바라보기
                    firePoint.localPosition = new Vector3(Mathf.Abs(firePoint.localPosition.x), firePoint.localPosition.y, firePoint.localPosition.z);
                }
                else if (moveInput < 0)
                {
                    spriteRenderer.flipX = true; // 왼쪽 바라보기
                    firePoint.localPosition = new Vector3(-Mathf.Abs(firePoint.localPosition.x), firePoint.localPosition.y, firePoint.localPosition.z);
                }
            }
            else
            {
                animator.SetBool("isRun", false);
            }

            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            if (Input.GetKeyDown(KeyCode.W) && isGrounded)
            {
                SoundManager.Instance.PlaySFX("playerJumpSound");
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }

        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftControl))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                float distanceToEnemy = Vector2.Distance(transform.position, hit.collider.transform.position);
                if (distanceToEnemy <= attackRange)
                {
                    StartCoroutine(MoveToEnemyAndAttack(hit.collider.gameObject));
                }
            }
        }

        if (Input.GetMouseButtonDown(1)) // 우클릭
        {
            ShootBullet();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }

        if (Input.GetKeyDown(KeyCode.E)) // 특정 키로 어그로 끌기
        {
            foreach (var enemy in explosiveEnemies)
            {
                enemy.AggroPlayer();
            }

            foreach (var enemy in sniperEnemies)
            {
                enemy.AggroPlayer();
            }
        }

        if (Input.GetKeyDown(KeyCode.K)) // 치트모드
        {
            // "Enemy" 태그가 붙은 모든 오브젝트 찾기
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (enemies.Length == 0)
            {
                Debug.Log("삭제할 적이 없습니다.");
                return;
            }

            // 배열을 순회하며 모든 적 오브젝트 삭제
            foreach (GameObject enemy in enemies)
            {
                Destroy(enemy);
            }

            Debug.Log(enemies.Length + "개의 적을 삭제했습니다.");
        }
    }

    private System.Collections.IEnumerator MoveToEnemyAndAttack(GameObject enemy)
    {
        isAttacking = true;

        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        playerCollider.enabled = false;

        CameraShake cameraShake = Camera.main.GetComponent<CameraShake>(); //카메라 흔들림
        if (cameraShake != null)
        {
            StartCoroutine(cameraShake.Shake(0.05f, 0.2f));
        }

        // 적 위치로 이동
        while (Vector2.Distance(transform.position, enemy.transform.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, enemy.transform.position, attackMoveSpeed * Time.deltaTime);
            yield return null;
        }

        // 적 제거

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("playerAttackSound");
        }
        
        FindObjectOfType<TimeStopController>().AddTimeGauge(5f);
        Destroy(enemy);

        // 카메라 흔들기 추가
        

        // 공격 후 플레이어 위치를 적의 위치에서 멀어지도록 약간 조정
        Vector2 escapeDirection = (transform.position - enemy.transform.position).normalized;
        transform.position += (Vector3)escapeDirection * 0.5f;

        // 충돌, 중력 및 속도 복원
        playerCollider.enabled = true;
        rb.gravityScale = 3;
        isAttacking = false;
    }


    void ShootBullet()
    {
        if (currentAmmo <= 0)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX("EmptyGunSound");
            }
            return;
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 shootDirection = (mousePosition - firePoint.position).normalized;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.SetDirection(shootDirection);

        TimeStopController timeStopController = FindObjectOfType<TimeStopController>();
        if (timeStopController != null && timeStopController.IsTimeStopped)
        {
            bulletScript.StopTime();
            timeStopController.RegisterTimeAffectedObject(bulletScript);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("PlayerGunSound");
        }
        currentAmmo--; // 발사 시 탄약 차감
        UpdateAmmoUI(); // 탄창 UI 업데이트
    }

    void Reload()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("RevolverSpin");
        }
        Debug.Log("재장전 완료!");
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo} / {maxAmmo}";
        }
    }
    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, attackRange);
    //}
}
