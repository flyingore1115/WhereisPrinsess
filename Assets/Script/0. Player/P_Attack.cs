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
    public GameObject attackParticlePrefab;     // 공격 시 나타날 파티클

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private float originalGravity;

    private bool isAttacking;
    public bool IsAttacking => isAttacking;

    private float defaultGravity;

    

    public void Init(Rigidbody2D rb, Collider2D playerCollider)
    {
        this.rb = rb;
        this.playerCollider = playerCollider;
        originalGravity = rb.gravityScale;
        defaultGravity    = rb.gravityScale;
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
        if (Player.Instance.holdingPrincess)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPoint.z = 0f;
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
            BaseEnemy enemy = hit.collider ? hit.collider.GetComponent<BaseEnemy>() : null;
            if (enemy != null)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance <= attackRange)
                {
                    StartCoroutine(MoveToEnemyAndAttack(hit.collider.gameObject));
                }
            }
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
    }
}


    /// <summary>
    /// 일반 공격: 플레이어가 적에게 돌진 후 공격
    /// </summary>
    private IEnumerator MoveToEnemyAndAttack(GameObject enemyObj)
    {
        isAttacking = true;
        rb.gravityScale = 0;
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
}
