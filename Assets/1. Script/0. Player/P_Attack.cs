using UnityEngine;
using System.Collections;
using MyGame;      // IDamageable, 기타 공용 네임스페이스

/// <summary>
/// 플레이어(메이드)의 **일반 근접 공격** 전담 스크립트  
/// • 좌클릭 시 대상(IDamageable)로 돌진 후 타격  
/// • 0.5초 쿨타임 적용  
/// • 공격 중엔 Rigidbody 중력‧Collider 비활성화로 이동 제어  
/// </summary>
public class P_Attack : MonoBehaviour
{
    /*──────────────────────────────────────────────
     * ▣ 인스펙터 노출 변수
     *─────────────────────────────────────────────*/

    [Header("Basic Attack Settings")]
    [Tooltip("일반 공격 가능 사거리(유닛)")]
    public float attackRange = 10f;

    [Tooltip("타깃에게 돌진할 때의 속도")]
    public float attackMoveSpeed = 150f;

    [Tooltip("타깃에게 주는 기본 데미지")]
    public int damageAmount = 2;

    [Header("Attack Cooldown")]
    [Tooltip("공격 쿨타임(초)")]
    public float attackCooldown = 0.5f;

    /*──────────────────────────────────────────────
     * ▣ 내부 상태 변수
     *─────────────────────────────────────────────*/

    private float lastAttackTime = -Mathf.Infinity; // 마지막 공격 시각
    private Rigidbody2D rb;                          // 플레이어 Rigidbody
    private Collider2D  playerCollider;              // 플레이어 Collider
    private float originalGravity;                   // 원래 중력값

    private bool      isAttacking;                   // 공격 중 여부 플래그
    public  bool      IsAttacking => isAttacking;    // 읽기 전용 프로퍼티
    private Coroutine currentAttackCo;               // 현재 실행 중 코루틴

    /*──────────────────────────────────────────────
     * ▣ 초기화
     *─────────────────────────────────────────────*/

    /// <summary>
    /// Player.cs에서 호출하여 의존성 주입  
    /// </summary>
    public void Init(Rigidbody2D rb, Collider2D col)
    {
        this.rb            = rb;
        this.playerCollider = col;
        originalGravity     = rb.gravityScale;
    }

    void Update()
    {
        // 시간 정지 중에는 입력 무시
        if (TimeStopController.Instance != null &&
            TimeStopController.Instance.IsTimeStopped)
            return;

        HandleAttack();
    }

    /*──────────────────────────────────────────────
     * ▣ 공격 입력 처리
     *─────────────────────────────────────────────*/

    /// <summary>
    /// 매 프레임 호출해 좌클릭 입력을 검사하고
    /// 조건이 맞으면 공격을 시작
    /// </summary>
    private void HandleAttack()
    {
        if (Player.Instance.holdingPrincess) return;      // 공주 손잡기 중엔 불가
        if (!Input.GetMouseButtonDown(0)) return;         // 좌클릭 아닐 때 무시
        if (Player.Instance != null && Player.Instance.IsShootingMode) return;  //사격모드일때 무시

        // 쿨타임 체크
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        // 마우스 월드 좌표
        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;

        // 클릭 지점에 겹친 Collider 탐색
        var hits = Physics2D.OverlapPointAll(wp);
        IDamageable target = null;

        foreach (var col in hits)
        {
            target = col.GetComponent<IDamageable>()?? col.GetComponentInParent<IDamageable>()?? col.GetComponentInChildren<IDamageable>();
            if (target != null) break;
        }
        if (target == null) return; // 공격 대상 없음

        // 사거리 체크
        float dist = Vector2.Distance(transform.position, target.transform.position);
        if (dist <= attackRange)
        {
            StartAttack(target);
            lastAttackTime = Time.time; // 쿨타임 갱신
        }
    }

    /*──────────────────────────────────────────────
     * ▣ 외부(또는 내부) 호출용 보조 메서드
     *─────────────────────────────────────────────*/

    /// <summary>
    /// 강제 공격 취소(코루틴 중지·상태 복구)
    /// </summary>
    public void CancelAttack()
    {
        if (currentAttackCo != null)
        {
            StopCoroutine(currentAttackCo);
            rb.gravityScale     = originalGravity;
            playerCollider.enabled = true;
            isAttacking         = false;
        }
    }

    /// <summary>
    /// 공격 루틴을 시작
    /// </summary>
    public void StartAttack(IDamageable target)
    {
        CancelAttack();
        currentAttackCo = StartCoroutine(AttackRoutine(target));
    }

    /*──────────────────────────────────────────────
     * ▣ 코루틴: 돌진 → 타격
     *─────────────────────────────────────────────*/

    /// <summary>
    /// 타깃까지 돌진한 뒤 데미지를 주는 메인 코루틴  
    /// • 이동 중 중력·Collider 비활성  
    /// • 타깃이 사라지면 즉시 종료  
    /// </summary>
    private IEnumerator AttackRoutine(IDamageable target)
    {
        isAttacking           = true;
        rb.gravityScale       = 0f;
        playerCollider.enabled = false;

        // 타깃에게 근접할 때까지 이동
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

        // 근접 성공 시 데미지·이펙트
        if (target != null && (target as Object) != null)
        {
            // (선택) 파티클·사운드
            SoundManager.Instance?.PlaySFX("PlayerAttackSound");

            // 실제 데미지
            target.Hit(damageAmount);
        }

        // 상태 복구
        playerCollider.enabled = true;
        rb.gravityScale        = originalGravity;
        isAttacking            = false;
        currentAttackCo        = null;
    }
}
