using UnityEngine;
using System.Collections;

public class PlayerOver : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    public HeartUI heartUI;

    public Transform princess;
    public Transform playerTransform; // 본인 transform

    private bool isDisabled = false;
    public bool IsDisabled => isDisabled;

    private Rigidbody2D rb;
    private Animator animator;
    private Player player;
    // ★수정: 원래 중력을 Awake()에서 저장 (Inspector의 값 유지)
    private float originalGravityScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // ★ Awake()에서 원래 gravityScale 값을 저장
        originalGravityScale = rb.gravityScale;
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (heartUI != null)
            heartUI.UpdateHearts(currentHealth, maxHealth);

        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
        // 원래 중력은 이미 Awake()에서 저장되었음
    }
    
    public void RestoreHealth(int amount)
    {
        currentHealth = maxHealth;
        if (heartUI != null)
            heartUI.UpdateHearts(currentHealth, maxHealth);
        Debug.Log($"[PlayerOver] 체력 복원: {currentHealth}/{maxHealth}");
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (heartUI != null)
            heartUI.UpdateHearts(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            DisablePlayer();
        }
    }
    
    public void DisablePlayer()
    {
        if (isDisabled) return;
        isDisabled = true;

        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("메이드가 행동불능이 되었습니다!");

        rb.velocity = Vector2.zero;

        if (player != null)
        {
            player.ignoreInput = true;
            Debug.Log("[PlayerOver] Player input ignored.");
        }

        // 카메라 타겟을 공주로 전환
        CameraFollow cf = FindObjectOfType<CameraFollow>();
        if (cf != null && princess != null)
        {
            cf.SetTarget(princess.gameObject);
        }
    }
    
    // 부활 시 호출 – 플레이어 위치, 체력 복원, 카메라 재설정
    public void OnRewindComplete(Vector2 restoredPosition)
    {
        // 일시적으로 kinematic
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;

        transform.position = restoredPosition;
        Debug.Log($"[PlayerOver] OnRewindComplete: 위치 => {restoredPosition}");

        RestoreHealth(0);
        // ★ 복원 시, 원래 중력으로 복원
        rb.gravityScale = originalGravityScale;

        CameraFollow cf = FindObjectOfType<CameraFollow>();
        if (cf != null)
        {
            cf.SetTarget(gameObject);
            Debug.Log("[PlayerOver] 카메라 타겟을 플레이어로 재설정");
        }
    }
    
    // 키 입력 후, 정상 플레이 재개
    public void ResumeAfterRewind()
    {
        isDisabled = false;
        rb.isKinematic = false;

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
}
