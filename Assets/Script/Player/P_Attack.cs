using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class P_Attack : MonoBehaviour
{
    public float attackRange = 10f;
    public float attackMoveSpeed = 150f;
    public int damageAmount = 2;
    public float comboAttackDelay = 0.3f; // 공격 간격
    public int comboThreshold = 3;        // 임계치 초과시 원콤 처리

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private bool isAttacking;

    // 선택된 적들을 저장하는 리스트 (순서대로)
    private static List<BaseEnemy> selectedEnemies = new List<BaseEnemy>();

    public void Init(Rigidbody2D rb, Collider2D playerCollider)
    {
        this.rb = rb;
        this.playerCollider = playerCollider;
    }

    public bool IsAttacking => isAttacking;

    public void HandleAttack()
    {
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
    }

    private IEnumerator MoveToEnemyAndAttack(GameObject enemyObj)
    {
        isAttacking = true;
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        playerCollider.enabled = false;

        while (Vector2.Distance(transform.position, enemyObj.transform.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, enemyObj.transform.position, attackMoveSpeed * Time.deltaTime);
            yield return null;
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
            Debug.LogWarning("[P_Attack] 공격 대상에 BaseEnemy 컴포넌트가 없습니다: " + enemyObj.name);
        }

        playerCollider.enabled = true;
        rb.gravityScale = 3;
        isAttacking = false;
    }
    // SHIFT 누른 상태에서 마우스 클릭으로 대상 선택/해제
    private void HandleTargetSelection()
    {
        // 좌클릭: 대상 선택
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                BaseEnemy enemy = hit.collider.GetComponent<BaseEnemy>();
                if (enemy != null && !selectedEnemies.Contains(enemy))
                {
                    float distance = Vector2.Distance(transform.position, enemy.transform.position);
                    if (distance <= attackRange)
                    {
                        selectedEnemies.Add(enemy);
                        // UI 표시: 순서 번호 (UI 프리팹 참조는 null로 둠 – 실제 프로젝트에서는 할당 필요)
                        enemy.DisplayOrderNumber(selectedEnemies.Count, null);
                        Debug.Log("[P_Attack] Selected enemy: " + enemy.name);
                    }
                }
            }
        }
        // 우클릭: 대상 선택 취소
        else if (Input.GetMouseButtonDown(1))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                BaseEnemy enemy = hit.collider.GetComponent<BaseEnemy>();
                if (enemy != null && selectedEnemies.Contains(enemy))
                {
                    selectedEnemies.Remove(enemy);
                    enemy.ClearOrderNumber();
                    Debug.Log("[P_Attack] Deselected enemy: " + enemy.name);
                    // 남은 적들의 순서 번호 업데이트
                    for (int i = 0; i < selectedEnemies.Count; i++)
                    {
                        selectedEnemies[i].DisplayOrderNumber(i + 1, null);
                    }
                }
            }
        }
    }

    // 콤보 어택 실행 (SHIFT 해제 시 호출)
    private IEnumerator ExecuteComboAttack()
    {
        isAttacking = true;
        playerCollider.enabled = false;

        for (int i = 0; i < selectedEnemies.Count; i++)
        {
            BaseEnemy enemy = selectedEnemies[i];
            if (enemy == null) continue;

            // 콤보 임계치 초과시, 원콤 처리 (적의 maxHealth 만큼 데미지)
            int damage = (i >= comboThreshold) ? enemy.maxHealth : damageAmount;
            enemy.TakeDamage(damage);
            Debug.Log("[P_Attack] Attacked enemy: " + enemy.name + " with damage: " + damage);
            yield return new WaitForSeconds(comboAttackDelay);
        }

        ClearSelection();
        playerCollider.enabled = true;
        isAttacking = false;
    }

    private void ClearSelection()
    {
        foreach (BaseEnemy enemy in selectedEnemies)
        {
            if (enemy != null)
                enemy.ClearOrderNumber();
        }
        selectedEnemies.Clear();
    }
}
