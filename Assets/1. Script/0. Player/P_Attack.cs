using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MyGame;

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
    public GameObject attackParticlePrefab;     // 공격 시 나타날 파티클

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private float originalGravity;

    private bool isAttacking;
    public bool IsAttacking => isAttacking;

    private float defaultGravity;
    private Coroutine currentAttackCo;



    public void Init(Rigidbody2D rb, Collider2D playerCollider)
    {
        this.rb = rb;
        this.playerCollider = playerCollider;
        originalGravity = rb.gravityScale;
        defaultGravity = rb.gravityScale;
    }

    void Update()
    {
        if (TimeStopController.Instance != null && TimeStopController.Instance.IsTimeStopped)
            return;
        HandleAttack();
    }

    /// <summary>
    /// 공격 로직 처리 (매 프레임 호출)
    /// </summary>
    public void HandleAttack()
    {
        if (Player.Instance.holdingPrincess) return;
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;

        // 지점에 겹친 모든 Collider 검색
        var hits = Physics2D.OverlapPointAll(wp);
        IDamageable target = null;

        foreach (var col in hits)
        {
            target = col.GetComponent<IDamageable>()
                  ?? col.GetComponentInParent<IDamageable>()
                  ?? col.GetComponentInChildren<IDamageable>();

            if (target != null) break;   // IDamageable 찾으면 즉시 탈출
        }

        if (target == null) return;      // 아무 대상도 없으면 종료

        float dist = Vector2.Distance(transform.position, target.transform.position);
        if (dist <= attackRange)
            StartAttack(target);
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
        }
    }

    // 외부에서 호출해 공격을 강제 취소
    public void CancelAttack()
    {
        if (currentAttackCo != null)
        {
            StopCoroutine(currentAttackCo);
            rb.gravityScale = originalGravity;
            playerCollider.enabled = true;
            isAttacking = false;
        }
    }


    /// <summary>
    /// 일반 공격: 플레이어가 적에게 돌진 후 공격
    /// </summary>
        public void StartAttack(IDamageable target)
    {
        // 이미 돌진 중이면 취소 후 다시 실행
        CancelAttack();
        currentAttackCo = StartCoroutine(AttackRoutine(target));
    }

    private IEnumerator AttackRoutine(IDamageable target)
    {
        isAttacking = true;
        rb.gravityScale       = 0;
        playerCollider.enabled = false;

        // 안전 null 체크 (파괴됐으면 루프 즉시 탈출)
        while (target != null && (target as Object) != null &&
               Vector2.Distance(transform.position, target.transform.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                target.transform.position,
                attackMoveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 타깃이 살아있을 때만 데미지·이펙트
        if (target != null && (target as Object) != null)
        {
            if (attackParticlePrefab != null)
                Instantiate(attackParticlePrefab, target.transform.position, Quaternion.identity);
            SoundManager.Instance?.PlaySFX("PlayerAttackSound");
            target.Hit(damageAmount);
        }

        playerCollider.enabled = true;
        rb.gravityScale        = originalGravity;
        isAttacking            = false;
        currentAttackCo        = null;
    }

    /* 기존 HandleAttack 내부에서
       StartCoroutine(MoveToEnemyAndAttack(target)) → StartAttack(target) 로 변경 */
}
