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

    // 시간 정지 중 선택된 적들을 저장 (콤보 공격용)
    private List<BaseEnemy> selectedEnemies = new List<BaseEnemy>();

    // 튜토리얼 등 외부에서 현재 선택된 적(물체) 개수를 읽기 위해 추가
    public int SelectedCount => selectedEnemies.Count;

    private List<GameObject> attackTargets = new List<GameObject>();

    // 시간 정지 상태를 프레임 간 추적
    private bool wasTimeStoppedLastFrame = false;

    

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
    /// 공격 로직 처리 (매 프레임 호출)
    /// </summary>
    public void HandleAttack()
    {
        if(Player.Instance.holdingPrincess)
            return;
        TimeStopController timeStop = FindFirstObjectByType<TimeStopController>();
        bool isNowTimeStopped = (timeStop != null && timeStop.IsTimeStopped);

        // 이전 프레임은 시간정지였고 지금 해제되었으면 콤보 공격 실행
        if (wasTimeStoppedLastFrame && !isNowTimeStopped && selectedEnemies.Count > 0)
        {
            Debug.Log("시간 정지 해제됨 → 콤보 공격 실행");
            StartCoroutine(ExecuteComboAttack());
        }
        wasTimeStoppedLastFrame = isNowTimeStopped;

        // SHIFT 입력 시 사격 우선 처리 (일반 공격 무시)
        if (Input.GetKey(KeyCode.LeftShift))
            return;

        if (isNowTimeStopped)
        {
            HandleTargetSelectionDuringTimeStop();
        }
        else
        {
            // 일반(즉시) 공격 처리
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    Camera.main.ScreenToWorldPoint(Input.mousePosition),
                    Vector2.zero
                );
                BaseEnemy enemy = hit.collider ? hit.collider.GetComponent<BaseEnemy>() : null;
                if (enemy != null)
                {
                    float distance = Vector2.Distance(transform.position, hit.collider.transform.position);
                    if (distance <= attackRange)
                    {
                        StartCoroutine(MoveToEnemyAndAttack(hit.collider.gameObject));
                    }
                }
            }
        }

        // 이동 입력 시 선택 취소 (선택 상태 초기화)
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 ||
            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0)
        {
            if (selectedEnemies.Count > 0)
                ClearSelection();
        }
    }

    /// <summary>
    /// 시간 정지 중 적 클릭(좌클릭: 선택, 우클릭: 선택 해제)
    /// </summary>
private void HandleTargetSelectionDuringTimeStop()
{
    if (Input.GetMouseButtonDown(0))
    {
        RaycastHit2D hit = Physics2D.Raycast(
            Camera.main.ScreenToWorldPoint(Input.mousePosition),
            Vector2.zero
        );

        // ◆ 적 또는 튜토리얼 타겟 클릭 시 선택
        if (hit.collider != null 
            && (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("TutorialTarget")))
        {
            BaseEnemy enemy = hit.collider.GetComponent<BaseEnemy>();
            if (enemy != null && !selectedEnemies.Contains(enemy))
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist <= attackRange)
                {
                    selectedEnemies.Add(enemy);
                    enemy.DisplayOrderNumber(selectedEnemies.Count, orderNumberUIPrefab);
                    if (SpriteTargetConnector.Instance != null)
                        SpriteTargetConnector.Instance.SetSelectedTargets(selectedEnemies);
                }
            }
        }
        // ◆ 그 외 클릭(허공 포함) 시 전부 해제
        else
        {
            ClearSelection();
        }
    }

    // (우클릭 분기는 완전히 제거해도 무방합니다.)
}


    /// <summary>
    /// 일반 공격: 플레이어가 적에게 돌진 후 공격
    /// </summary>
    private IEnumerator MoveToEnemyAndAttack(GameObject enemyObj)
    {
        isAttacking = true;
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        playerCollider.enabled = false;

        while (Vector2.Distance(transform.position, enemyObj.transform.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                enemyObj.transform.position,
                attackMoveSpeed * Time.deltaTime
            );
            yield return null;
        }

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

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("PlayerAttackSound");
        }

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
    /// 콤보 공격: 시간 정지 해제 후 선택된 적들을 순서대로 공격
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

            while (Vector2.Distance(transform.position, enemy.transform.position) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    enemy.transform.position,
                    attackMoveSpeed * Time.deltaTime
                );
                yield return null;
            }

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

            int dmg = (i >= comboThreshold) ? enemy.maxHealth : damageAmount;
            enemy.TakeDamage(dmg);
            Debug.Log("Combo attacked enemy: " + enemy.name + " for damage: " + dmg);
            yield return new WaitForSeconds(comboAttackDelay);
        }

        ClearSelection();

        if (SpriteTargetConnector.Instance != null)
            SpriteTargetConnector.Instance.OnAttackFinished();

        playerCollider.enabled = true;
        rb.gravityScale = savedGravity;
        isAttacking = false;
    }

    /// <summary>
    /// 선택 취소: 선택된 적 UI 초기화 및 SpriteTargetConnector 초기화
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
        if (SpriteTargetConnector.Instance != null)
            SpriteTargetConnector.Instance.ClearAllSegments();
    }

    
    public void RegisterTarget(BaseEnemy enemy)
    {
        if (!selectedEnemies.Contains(enemy))
        {
            selectedEnemies.Add(enemy);
            enemy.DisplayOrderNumber(selectedEnemies.Count, orderNumberUIPrefab);
            if (SpriteTargetConnector.Instance != null)
                SpriteTargetConnector.Instance.SetSelectedTargets(selectedEnemies);
        }
    }
}
