using UnityEngine;
using System.Collections;

public class P_Attack : MonoBehaviour
{
    public float attackRange = 10f;
    public float attackMoveSpeed = 150f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private bool isAttacking;
    private CameraShake cameraShake;

    public void Init(Rigidbody2D rb, Collider2D playerCollider, CameraShake cameraShake)
    {
        this.rb = rb;
        this.playerCollider = playerCollider;
        this.cameraShake = cameraShake;
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

    private IEnumerator MoveToEnemyAndAttack(GameObject enemy)
    {
        isAttacking = true;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        playerCollider.enabled = false;

        if (cameraShake != null)
        {
            StartCoroutine(cameraShake.Shake(0.05f, 0.2f));
        }

        // 이동하여 적에게 접근
        while (Vector2.Distance(transform.position, enemy.transform.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, enemy.transform.position, attackMoveSpeed * Time.deltaTime);
            yield return null;
        }
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("PlayerAttackSound");
        }
        
        // 적 오브젝트를 제거하기 전에 위치를 저장합니다.
        Vector3 enemyPosition = enemy.transform.position;
        Destroy(enemy);
        FindObjectOfType<TimeStopController>().AddTimeGauge(5f);

        // 저장된 적 위치를 이용하여 탈출 방향 계산
        Vector2 escapeDirection = (transform.position - enemyPosition).normalized;
        transform.position += (Vector3)escapeDirection * 0.5f;

        playerCollider.enabled = true;
        rb.gravityScale = 3;
        isAttacking = false;
    }
}
