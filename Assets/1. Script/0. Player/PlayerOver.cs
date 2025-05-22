using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerOver : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Health Bar UI")]
    public Slider healthSlider;

    public Transform princess;         // 동적 할당
    public Transform playerTransform;  // 필요 없으면 제거 가능

    private bool isDisabled = false;
    public bool IsDisabled => isDisabled;

    private Rigidbody2D rb;
    private Animator animator;
    private Player player;

    // 외부 컴포넌트
    private CameraFollow cameraFollow;
    private StatusTextManager statusTextManager;

    private float originalGravityScale;
    private Coroutine healthLerpCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;

        // 씬에 있는 매니저들 캐시
        cameraFollow = FindFirstObjectByType<CameraFollow>();
        statusTextManager = FindFirstObjectByType<StatusTextManager>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar(currentHealth);

        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
    }

    void Update()
    {
        if (currentHealth <= 0)
        {
            DisablePlayer();
        }
    }

    private void UpdateHealthBar(int health)
    {
        if (healthSlider != null)
            healthSlider.value = health;
    }

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
            StopCoroutine(healthLerpCoroutine);
        healthLerpCoroutine = StartCoroutine(LerpHealthBar(currentHealth, newHealth, 0.5f));
        currentHealth = newHealth;
    }

    private IEnumerator LerpHealthBar(int fromH, int toH, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            int v = Mathf.RoundToInt(Mathf.Lerp(fromH, toH, t / dur));
            UpdateHealthBar(v);
            yield return null;
        }
        UpdateHealthBar(toH);
    }

    public void DisablePlayer()
    {
        if (isDisabled) return;
        isDisabled = true;

        // 1) princess 항상 재탐색
        if (princess == null)
        {
            var prObj = GameObject.FindGameObjectWithTag("Princess");
            if (prObj != null)
                princess = prObj.transform;
        }

        // 2) 카메라 타겟 전환
        if (cameraFollow != null && princess != null)
        {
            cameraFollow.SetTarget(princess.gameObject);
            Debug.Log("[PlayerOver] 카메라 타겟: Princess");
        }
        else
        {
            Debug.LogWarning("[PlayerOver] CameraFollow 또는 Princess 미할당");
        }

        // 3) 상태 텍스트 표시
        // 3) 상태 텍스트 매니저 재탐색 (필요 시)
        if (statusTextManager == null)
        {
            statusTextManager = FindFirstObjectByType<StatusTextManager>();
        }

        if (statusTextManager != null)
        {
            statusTextManager.ShowMessage("플레이어가 행동불능 상태가 되었습니다!");
            Debug.Log("[PlayerOver] 상태 텍스트 출력 성공");
        }
        else
        {
            Debug.LogWarning("[PlayerOver] StatusTextManager 미발견");
        }

        // 4) 플레이어 입력 무시 및 정지
        rb.linearVelocity = Vector2.zero;
        if (player != null)
        {
            player.ignoreInput = true;
            Debug.Log("[PlayerOver] Player input ignored.");
        }

        Debug.Log("플레이어 행동불능 처리 완료");
    }

    public void OnRewindComplete(Vector2 restoredPosition)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        transform.position = restoredPosition;
        RestoreHealth(0);
        rb.gravityScale = originalGravityScale;

        if (player != null)
        {
            player.ignoreInput = false;
            Debug.Log("[PlayerOver] 입력 복원");
        }

        // 카메라 플레이어로 복귀
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(gameObject);
            Debug.Log("[PlayerOver] 카메라 타겟: Player");
        }

        isDisabled = false;
    }

    public void ResumeAfterRewind()
    {
        isDisabled = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        if (player != null)
        {
            player.ignoreInput = false;
            Debug.Log("[PlayerOver] 입력 복원");
        }

        // 공주 컨트롤 플래그 해제
        if (princess != null)
        {
            var ps = princess.GetComponent<Princess>();
            if (ps != null)
            {
                ps.isControlled = false;
                Debug.Log("[PlayerOver] Princess 컨트롤 해제");
            }
        }
    }

    public void ForceSetHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        UpdateHealthBar(currentHealth);
    }
}
