using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerOver : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Health Bar UI")]
    public Slider healthSlider;       // 인스펙터에서 할당 (메인 캔버스 하위에 배치)

    public Transform princess;
    public Transform playerTransform; // 본인 transform

    private bool isDisabled = false;
    public bool IsDisabled => isDisabled;

    private Rigidbody2D rb;
    private Animator animator;
    private Player player;
    private float originalGravityScale;

    // 피해 애니메이션 효과를 부드럽게 적용하기 위한 코루틴 변수
    private Coroutine healthLerpCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar(currentHealth);  // 초기 체력바 업데이트
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
    }

    void Update()
    {
        // 체력이 0 이하이면 플레이어 비활성화 처리
        if (currentHealth <= 0)
        {
            DisablePlayer();
        }
    }

    // 체력바를 부드럽게 업데이트하는 코루틴 (duration은 조절 가능)
    private IEnumerator LerpHealthBar(int fromHealth, int toHealth, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            int lerpedHealth = Mathf.RoundToInt(Mathf.Lerp(fromHealth, toHealth, t));
            UpdateHealthBar(lerpedHealth);
            yield return null;
        }
        UpdateHealthBar(toHealth);
    }

    // 체력바 UI 업데이트 함수
    private void UpdateHealthBar(int health)
    {
        if (healthSlider != null)
        {
            healthSlider.value = health;
        }
    }

    // 체력을 복원할 때 (예: 부활시)
    public void RestoreHealth(int amount)
    {
        currentHealth = maxHealth;
        UpdateHealthBar(currentHealth);
        Debug.Log($"[PlayerOver] 체력 복원: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(int damage)
    {
        int newHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        if (healthLerpCoroutine != null)
        {
            StopCoroutine(healthLerpCoroutine);
        }
        // 0.5초 동안 부드럽게 체력을 줄임
        healthLerpCoroutine = StartCoroutine(LerpHealthBar(currentHealth, newHealth, 0.5f));
        currentHealth = newHealth;

        
    }

public void DisablePlayer()
{
    if (isDisabled) return;
    Debug.Log("플레이어 행동불능!");
    isDisabled = true;

    rb.linearVelocity = Vector2.zero;

    if (player != null)
    {
        player.ignoreInput = true;
        Debug.Log("[PlayerOver] Player input ignored.");
    }

    // 📌 상태 메시지 출력
    StatusTextManager stm = FindFirstObjectByType<StatusTextManager>();
    if (stm != null)
    {
        stm.ShowMessage("플레이어가 행동불능 상태가 되었습니다!");
    }

    // 📌 카메라 타겟을 공주로 전환
    CinemachineAutoTarget.SetCinemachineTarget(princess.gameObject);

}


    // 부활 시 호출 – 플레이어 위치, 체력 복원, 카메라 재설정
    public void OnRewindComplete(Vector2 restoredPosition)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        transform.position = restoredPosition;
        Debug.Log($"[PlayerOver] OnRewindComplete: 위치 => {restoredPosition}");
        RestoreHealth(0);
        rb.gravityScale = originalGravityScale;
        if (player != null)
        {
            player.ignoreInput = false;
            Debug.Log("[PlayerOver] OnRewindComplete -> ignoreInput=false, isDisabled=false");
        }
        CameraFollow cf = FindFirstObjectByType<CameraFollow>(); ;
        if (cf != null)
        {
            cf.SetTarget(gameObject);
            Debug.Log("[PlayerOver] 카메라 타겟이 플레이어로 재설정됨");
        }
        isDisabled = false;
    }

    // 키 입력 후, 정상 플레이 재개
    public void ResumeAfterRewind()
    {
        isDisabled = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        if (player != null)
        {
            player.ignoreInput = false;
            Debug.Log("[PlayerOver] 플레이어 입력 복원");
        }
        Princess princessScript = princess.GetComponent<Princess>();
        if (princessScript != null)
        {
            princessScript.isControlled = false;
            Debug.Log("[PlayerOver] 공주 조종 플래그 해제");
        }
    }
public void ForceSetHealth(int value)
{
    currentHealth = Mathf.Clamp(value, 0, maxHealth);
    UpdateHealthBar(currentHealth);
}

}
