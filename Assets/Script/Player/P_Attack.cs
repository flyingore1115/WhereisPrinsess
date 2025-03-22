using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class P_Attack : MonoBehaviour
{
    public float attackRange = 10f;
    public float attackMoveSpeed = 150f;
    public int damageAmount = 2;
    public float comboAttackDelay = 0.2f;
    public int comboThreshold = 3;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private bool isAttacking;
    private float originalGravity;

    // 선택된 적들을 저장하는 리스트 (콤보 공격용)
    private List<BaseEnemy> selectedEnemies = new List<BaseEnemy>();

    // 시간정지 상태 추적
    private bool wasTimeStoppedLastFrame = false;

    public GameObject orderNumberUIPrefab;

    public void Init(Rigidbody2D rb, Collider2D playerCollider)
    {
        this.rb = rb;
        this.playerCollider = playerCollider;
        originalGravity = rb.gravityScale;
    }

    public bool IsAttacking => isAttacking;

    public void HandleAttack()
    {
        bool isNowTimeStopped = (TimeStopController.Instance != null && TimeStopController.Instance.IsTimeStopped);

        // 시간정지에서 해제된 시점 감지 → 콤보 공격 실행
        if (wasTimeStoppedLastFrame && !isNowTimeStopped && selectedEnemies.Count > 0)
        {
            Debug.Log("시간 정지 해제됨 → 콤보 공격 실행");
            StartCoroutine(ExecuteComboAttack());
        }
        wasTimeStoppedLastFrame = isNowTimeStopped;

        // 사격 모드는 SHIFT 키가 눌린 상태에서 P_Shooting이 처리하므로, 여기선 SHIFT 미사용 분기만 처리
        if (Input.GetKey(KeyCode.LeftShift))
            return;

        if (isNowTimeStopped)
        {
            HandleTargetSelectionDuringTimeStop();
        }
        else // 일반 공격
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                {
                    float distance = Vector2.Distance(transform.position, hit.collider.transform.position);
                    if (distance <= attackRange)
                    {
                        StartCoroutine(MoveToEnemyAndAttack(hit.collider.gameObject));
                    }
                }
            }
        }

        // 이동 입력 시 선택 취소 (타겟 선택 모드 중)
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0)
        {
            if (selectedEnemies.Count > 0)
                ClearSelection();
        }
    }

    private void HandleTargetSelectionDuringTimeStop()
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
                        enemy.DisplayOrderNumber(selectedEnemies.Count, orderNumberUIPrefab);
                        Debug.Log("Selected enemy: " + enemy.name);
                    }
                }
            }
        }
        // 우클릭: 대상 선택 해제
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
                    // 남은 적 순서 번호 업데이트
                    for (int i = 0; i < selectedEnemies.Count; i++)
                    {
                        selectedEnemies[i].DisplayOrderNumber(i + 1, null);
                    }
                    Debug.Log("Deselected enemy: " + enemy.name);
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
            Debug.LogWarning("[P_Attack] Enemy component missing on " + enemyObj.name);
        }

        playerCollider.enabled = true;
        rb.gravityScale = originalGravity;
        isAttacking = false;
    }

    private IEnumerator ExecuteComboAttack()
    {
        isAttacking = true;
        // 콤보 공격 동안 플레이어 이동 및 충돌 방지를 위해 중력 제거 및 Collider 비활성화
        float savedGravity = rb.gravityScale;
        rb.gravityScale = 0;
        playerCollider.enabled = false;

        for (int i = 0; i < selectedEnemies.Count; i++)
        {
            BaseEnemy enemy = selectedEnemies[i];
            if (enemy == null) continue;

            // 플레이어를 적 방향으로 날아가게 함 (콤보 공격)
            while (Vector2.Distance(transform.position, enemy.transform.position) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, enemy.transform.position, attackMoveSpeed * Time.deltaTime);
                yield return null;
            }

            // 콤보 임계치 초과 시 원콤 처리
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
