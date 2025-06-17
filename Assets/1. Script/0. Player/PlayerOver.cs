using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerOver : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    public int CurrentHealth => currentHealth;

    [Header("Health Bar UI")]
    public Slider healthSlider;

    public Transform princess;         // 동적 할당
    public Transform playerTransform;  // 필요 없으면 제거 가능

    public bool IsDisabled; //일단 넣어둔거

    private Rigidbody2D rb;
    private Animator animator;
    private Player player;

    // 외부 컴포넌트
    private CameraFollow cameraFollow;
    private StatusTextManager statusTextManager;

    private float originalGravityScale;
    private Coroutine healthLerpCoroutine;

    private bool hasTriggeredGameOver = false; // 한 번만 호출하기 위한 플래그

    void Awake()
    {
        currentHealth = maxHealth;
        UpdateHealthBar(currentHealth);

        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;

        cameraFollow = FindFirstObjectByType<CameraFollow>();
        statusTextManager = FindFirstObjectByType<StatusTextManager>();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
        UpdateHealthBar(currentHealth);
    }

    void Update()
    {
        if (currentHealth <= 0 && !hasTriggeredGameOver)
        {
            hasTriggeredGameOver = true;
            // 더 이상 DisablePlayer()를 부르지 않고, 바로 게임오버 매니저로 넘긴다.
            var gm = FindFirstObjectByType<GameOverManager>();
            if (gm != null)
            {
                gm.TriggerGameOver();
            }
            else
            {
                Debug.LogError("[PlayerOver] GameOverManager를 찾을 수 없습니다!");
            }
        }
    }

    private void UpdateHealthBar(int health)
    {
        if (CanvasManager.Instance != null)
        CanvasManager.Instance.UpdateHealthUI(health, maxHealth);
    }

    public void RestoreHealth(int amount)
    {
        currentHealth = maxHealth;
        UpdateHealthBar(currentHealth);
        Debug.Log($"[PlayerOver] 체력 복원: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        Debug.Log($"받는 피해: {damage}, 남은 체력: {currentHealth}");
        PostProcessingManager.Instance.ApplyCharacterHitEffect();
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

    // 나중에 부활시키기
    /*
    public void DisablePlayer()
    {
        if (isDisabled) return;
        isDisabled = true;

        // 원래 있던 동작들 (카메라 포커스 전환, UI 메시지 등)을 모두 제거했습니다.
        // 이제 플레이어가 죽으면 바로 GameOverManager로 넘어갑니다.
    }
    */

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

        hasTriggeredGameOver = false; // 되감기 이후 다시 체크할 수 있도록 리셋
    }

    public void ResumeAfterRewind()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        if (player != null)
        {
            player.ignoreInput = false;
            Debug.Log("[PlayerOver] 입력 복원");
        }

        if (princess != null)
        {
            var ps = princess.GetComponent<Princess>();
            if (ps != null)
            {
                ps.isControlled = false;
                Debug.Log("[PlayerOver] Princess 컨트롤 해제");
            }
        }

        hasTriggeredGameOver = false; // 되감기 이후 다시 체크할 수 있도록 리셋
    }

    public void ForceSetHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        UpdateHealthBar(currentHealth);
    }
}
