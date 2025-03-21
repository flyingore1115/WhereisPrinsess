using UnityEngine;
using System.Collections;

using TMPro;  // TextMeshPro 사용

public class BaseEnemy : MonoBehaviour, ITimeAffectable
{
    // 기존 변수들
    protected Transform princess;
    protected Transform player;
    public string prefabName = "Self_Enemy";
    private static int globalIDCounter = 1;
    public int enemyID = -1;
    protected bool isTimeStopped = false;
    protected bool isStunned = false;
    protected bool isAggroOnPlayer = false;
    public bool isDead = false;
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;
    protected Material originalMaterial;
    public Material grayscaleMaterial;

    public int maxHealth = 3;

    private GameObject orderNumberUIInstance;
    
    [HideInInspector] public int currentHealth;  // 외부 TimePointManager가 접근 가능토록
    public TMP_Text healthDisplay; // 적 위에 표시할 텍스트 (Inspector에서 할당)

    protected virtual void Awake()
    {
        // ID 할당
        if (enemyID < 0)
        {
            enemyID = globalIDCounter++;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.sharedMaterial;
        }

        princess = GameObject.FindGameObjectWithTag("Princess")?.transform;
        player   = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 체력 초기화
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }

    // 체력 표시 업데이트 함수
    protected void UpdateHealthDisplay()
    {
        if (healthDisplay != null)
        {
            healthDisplay.text = currentHealth.ToString();
        }
    }

    public void SetHealth(int hp)
    {
        currentHealth = hp;
        if (currentHealth <= 0)
        {
            isDead = true;
            gameObject.SetActive(false);
        }
        else
        {
            isDead = false;
            if (!gameObject.activeInHierarchy)
                gameObject.SetActive(true);
        }
        UpdateHealthDisplay();
    }

    public void ResetDamageState()
    {
        isStunned = false;
        // 스프라이트 색상을 완전 불투명(알파 1)으로 복원
        if (spriteRenderer != null)
        {
            Color col = spriteRenderer.color;
            col.a = 1f;
            spriteRenderer.color = col;
        }
    }



    // 데미지 처리: damage 만큼 체력을 감소시키고, 0 이하가 되면 Die() 호출
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        UpdateHealthDisplay();
        Debug.Log($"[Enemy] {gameObject.name} 받는 피해: {damage}, 남은 체력: {currentHealth}");

        StartCoroutine(CoDamageBlink(0.5f)); // 0.5초간 깜빡

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 적 처치 처리
    protected virtual void Die()
    {
        isDead = true;
        // 여기서 죽음 애니메이션, 사운드, 혹은 파티클 효과를 넣을 수 있음.
        gameObject.SetActive(false);
    }

    // ★ 스턴 & 깜빡임 연출
    private IEnumerator CoDamageBlink(float stunDuration)
    {
        if (isStunned) yield break; // 이미 스턴 중이면 중복 실행 안 함
        isStunned = true;

        float timer = 0f;
        float blinkInterval = 0.1f;
        Color originalColor = spriteRenderer.color;
        Color blinkColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.6f);
        // 이동/AI를 멈추고 싶다면, 자식 클래스(ExplosiveEnemy 등)의 Update() 안에서
        // isStunned 체크해서 MoveTowardsTarget()를 막는 로직을 추가하거나
        // 혹은 isStunned를 별도로 활용

        while (timer < stunDuration)
        {
            // 투명도 60%로
            spriteRenderer.color = blinkColor;
            yield return new WaitForSeconds(blinkInterval);

            // 원래 색으로
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkInterval);

            timer += (blinkInterval * 2f);
        }

        // 스턴 해제
        spriteRenderer.color = originalColor;
        isStunned = false;
    }

    // 기존 StopTime(), ResumeTime(), RestoreColor(), AggroPlayer() 등 기존 함수들 그대로 유지
    public virtual void StopTime()
    {
        if (this == null || spriteRenderer == null) return;
        isTimeStopped = true;
        PostProcessingManager.Instance.ApplyTimeStop();
        if (animator != null)
        {
            animator.speed = 0;
        }
    }

    public virtual void ResumeTime()
    {
        if (spriteRenderer == null) return;
        isTimeStopped = false;
        if (animator != null)
        {
            animator.speed = 1;
        }
        PostProcessingManager.Instance.SetDefaultEffects();
    }

       public void DisplayOrderNumber(int order, GameObject uiPrefab)
    {
        if (uiPrefab == null)
            return;

        if (orderNumberUIInstance == null)
        {
            // UI 프리팹을 적의 자식으로 생성 (예: 적 중심 위에 배치)
            orderNumberUIInstance = Instantiate(uiPrefab, transform);
            orderNumberUIInstance.transform.localPosition = new Vector3(0, 1f, 0); // 위치는 상황에 맞게 조정
        }
        TMP_Text tmp = orderNumberUIInstance.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = order.ToString();
        }
    }

    // 순서 번호 UI를 제거하는 메서드
    public void ClearOrderNumber()
    {
        if (orderNumberUIInstance != null)
        {
            Destroy(orderNumberUIInstance);
            orderNumberUIInstance = null;
        }
    }


    public virtual void AggroPlayer()
    {
        if (isAggroOnPlayer) return;
        isAggroOnPlayer = true;
        Debug.Log($"[Enemy] {gameObject.name} is now targeting Player!");
        Invoke(nameof(ResetAggro), 5f);
    }

    protected void ResetAggro()
    {
        isAggroOnPlayer = false;
        Debug.Log($"[Enemy] {gameObject.name} is now targeting Princess.");
    }
}
