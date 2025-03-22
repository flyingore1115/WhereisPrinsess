using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class P_Attack : MonoBehaviour
{
    [Header("Basic Attack Settings")]
    public float attackRange = 10f;             // 일반 공격 사거리
    public float attackMoveSpeed = 150f;        // 적에게 이동 시 속도
    public int damageAmount = 2;                // 공격 데미지 (기본)
    
    [Header("Combo Attack Settings")]
    public float comboAttackDelay = 0.2f;       // 콤보 공격 간 대기 시간
    public int comboThreshold = 3;             // 콤보 임계치 (초과 시 원콤)

    [Header("Prefabs for Effects")]
    public GameObject orderNumberUIPrefab;      // 적 위에 표시될 순서번호 UI (월드스페이스)
    public GameObject attackParticlePrefab;     // 공격 시 나타날 파티클

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private float originalGravity;

    private bool isAttacking;
    public bool IsAttacking => isAttacking;

    private TargetConnector targetConnector;

    void Start()
    {
        targetConnector = FindFirstObjectByType<TargetConnector>(); 
        // or targetConnector = GameObject.FindObjectOfType<TargetConnector>();
    }

    // 시간 정지 상태를 프레임 간 추적
    private bool wasTimeStoppedLastFrame = false;

    // 시간 정지 중 선택된 적들을 저장 (순차 공격용)
    private List<BaseEnemy> selectedEnemies = new List<BaseEnemy>();
    public void Init(Rigidbody2D rb, Collider2D playerCollider)
    {
        this.rb = rb;
        this.playerCollider = playerCollider;
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        HandleAttack();
    }

    /// <summary>
    /// 공격 로직 처리 (Update()에서 매 프레임 호출)
    /// </summary>
    public void HandleAttack()
    {
        // 최신 Unity에서는 FindObjectOfType<T>()가 deprecate → FindFirstObjectByType<T>() 사용
        TimeStopController timeStop = FindFirstObjectByType<TimeStopController>();
        bool isNowTimeStopped = (timeStop != null && timeStop.IsTimeStopped);

        // 이전 프레임에 시간정지였고 → 지금 해제됐으며 → 선택된 적들이 있으면 콤보 공격
        if (wasTimeStoppedLastFrame && !isNowTimeStopped && selectedEnemies.Count > 0)
        {
            Debug.Log("시간 정지 해제됨 → 콤보 공격 실행");
            StartCoroutine(ExecuteComboAttack());
        }
        wasTimeStoppedLastFrame = isNowTimeStopped;

        // SHIFT 키가 눌려있다면 → 사격 우선 → 이 스크립트(일반 공격)는 처리 안 함
        if (Input.GetKey(KeyCode.LeftShift))
            return;

        // 시간정지 상태에서는 적 타겟 선택, 아닐 때는 일반 공격
        if (isNowTimeStopped)
        {
            HandleTargetSelectionDuringTimeStop();
        }
        else
        {
            // 일반(즉시) 공격
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    Camera.main.ScreenToWorldPoint(Input.mousePosition),
                    Vector2.zero
                );
                if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                {
                    float distance = Vector2.Distance(
                        transform.position,
                        hit.collider.transform.position
                    );
                    if (distance <= attackRange)
                    {
                        StartCoroutine(MoveToEnemyAndAttack(hit.collider.gameObject));
                    }
                }
            }
        }

        // 이동 입력 발생 시 선택 취소
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 ||
            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0)
        {
            if (selectedEnemies.Count > 0)
                ClearSelection();
        }
    }

    /// <summary>
    /// 시간 정지 중 적 클릭 → 순서 지정, 우클릭 → 선택 해제
    /// </summary>
    private void HandleTargetSelectionDuringTimeStop()
    {
        // 좌클릭: 적 선택
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(
                Camera.main.ScreenToWorldPoint(Input.mousePosition),
                Vector2.zero
            );
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                BaseEnemy enemy = hit.collider.GetComponent<BaseEnemy>();
                if (enemy != null && !selectedEnemies.Contains(enemy))
                {
                    float distance = Vector2.Distance(transform.position, enemy.transform.position);
                    if (distance <= attackRange)
                    {
                        selectedEnemies.Add(enemy);
                        enemy.DisplayOrderNumber(selectedEnemies.Count, orderNumberUIPrefab);
                        Debug.Log("Selected enemy: " + enemy.name);
                        if (targetConnector != null)
                            TargetConnector.Instance.UpdateLine(selectedEnemies);
                    }
                }
            }
        }
        // 우클릭: 적 선택 해제
        else if (Input.GetMouseButtonDown(1))
        {
            RaycastHit2D hit = Physics2D.Raycast(
                Camera.main.ScreenToWorldPoint(Input.mousePosition),
                Vector2.zero
            );
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                BaseEnemy enemy = hit.collider.GetComponent<BaseEnemy>();
                if (enemy != null && selectedEnemies.Contains(enemy))
                {
                    selectedEnemies.Remove(enemy);
                    enemy.ClearOrderNumber();

                    if (targetConnector != null)
                        TargetConnector.Instance.UpdateLine(selectedEnemies);

                    // 남은 적들의 순서 재지정
                    for (int i = 0; i < selectedEnemies.Count; i++)
                    {
                        selectedEnemies[i].DisplayOrderNumber(i + 1, orderNumberUIPrefab);
                    }
                    Debug.Log("Deselected enemy: " + enemy.name);
                }
            }
        }
    }

    /// <summary>
    /// 일반 공격: 플레이어가 적에게 즉시 돌진 후 공격
    /// </summary>
    private IEnumerator MoveToEnemyAndAttack(GameObject enemyObj)
    {
        isAttacking = true;
        rb.gravityScale = 0;
        // Rigidbody2D.velocity → linearVelocity 권장
        rb.linearVelocity = Vector2.zero;
        playerCollider.enabled = false;

        // 적에게 이동
        while (Vector2.Distance(transform.position, enemyObj.transform.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                enemyObj.transform.position,
                attackMoveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 공격 파티클 효과
        if (attackParticlePrefab != null)
        {
            GameObject effect = Instantiate(
                attackParticlePrefab,
                enemyObj.transform.position,
                Quaternion.identity
            );
            var psr = effect.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                psr.sortingOrder = 100;
            }
        }

        // 사운드
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("PlayerAttackSound");
        }

        // 적에게 데미지
        BaseEnemy enemy = enemyObj.GetComponent<BaseEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damageAmount);
        }
        else
        {
            Debug.LogWarning("[P_Attack] Enemy component missing on " + enemyObj.name);
        }

        playerCollider.enabled = true;
        rb.gravityScale = originalGravity;
        isAttacking = false;
    }

    /// <summary>
    /// 시간 정지 해제 후: 선택된 적들을 순서대로 공격(콤보)
    /// </summary>
    private IEnumerator ExecuteComboAttack()
    {
        isAttacking = true;
        float savedGravity = rb.gravityScale;
        rb.gravityScale = 0;
        playerCollider.enabled = false;

        for (int i = 0; i < selectedEnemies.Count; i++)
        {
            BaseEnemy enemy = selectedEnemies[i];
            if (enemy == null) continue;

            // 적에게 이동
            while (Vector2.Distance(transform.position, enemy.transform.position) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    enemy.transform.position,
                    attackMoveSpeed * Time.deltaTime
                );
                yield return null;
            }

            // 콤보 공격 파티클
            if (attackParticlePrefab != null)
            {
                GameObject effect = Instantiate(
                    attackParticlePrefab,
                    enemy.transform.position,
                    Quaternion.identity
                );
                var psr = effect.GetComponent<ParticleSystemRenderer>();
                if (psr != null)
                {
                    psr.sortingOrder = 100;
                }
            }

            // 콤보 임계치 초과 시 원콤(=적 maxHealth)
            int dmg = (i >= comboThreshold) ? enemy.maxHealth : damageAmount;
            enemy.TakeDamage(dmg);
            Debug.Log("Combo attacked enemy: " + enemy.name + " for damage: " + dmg);

            yield return new WaitForSeconds(comboAttackDelay);
        }

        ClearSelection();
        playerCollider.enabled = true;
        rb.gravityScale = savedGravity;
        isAttacking = false;
    }

    /// <summary>
    /// 타겟 선택 해제 (이동 입력 or 콤보 끝)
    /// </summary>
    private void ClearSelection()
    {
        foreach (BaseEnemy enemy in selectedEnemies)
        {
            if (enemy != null)
            {
                enemy.ClearOrderNumber();
            }
        }
        selectedEnemies.Clear();

        if (targetConnector != null)
            TargetConnector.Instance.UpdateLine(selectedEnemies);
    }
}
