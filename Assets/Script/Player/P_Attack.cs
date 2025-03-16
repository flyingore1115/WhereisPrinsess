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

    private IEnumerator MoveToEnemyAndAttack(GameObject enemyObj)
    {
        isAttacking = true;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        playerCollider.enabled = false;

        if (cameraShake != null)
        {
            StartCoroutine(cameraShake.Shake(0.05f, 0.2f));
        }

        // 플레이어가 적에게 접근
        while (Vector2.Distance(transform.position, enemyObj.transform.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, enemyObj.transform.position, attackMoveSpeed * Time.deltaTime);
            yield return null;
        }
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("PlayerAttackSound");
        }
        
        // 기존 Destroy 대신 적의 TakeDamage() 호출
        BaseEnemy enemyScript = enemyObj.GetComponent<BaseEnemy>();
        if (enemyScript != null)
        {
            enemyScript.TakeDamage();
        }
        else
        {
            Debug.LogWarning($"[P_Attack] 공격 대상에 BaseEnemy 컴포넌트가 없습니다: {enemyObj.name}");
        }

        playerCollider.enabled = true;
        rb.gravityScale = 3;
        isAttacking = false;
    }
}
