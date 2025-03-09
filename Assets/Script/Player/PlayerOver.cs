using UnityEngine;
using System.Collections;

public class PlayerOver : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    public HeartUI heartUI;

    // 반드시 인스펙터에 할당: 공주 Transform, 플레이어 Transform(자신)
    public Transform princess;
    public Transform playerTransform;

    private bool isDisabled = false;
    public bool IsDisabled { get { return isDisabled; } }

    private Rigidbody2D rb;
    private Animator animator;
    private Player player;
    private float originalGravityScale;

    void Start()
    {
        currentHealth = maxHealth;
        if (heartUI != null)
            heartUI.UpdateHearts(currentHealth, maxHealth);

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
        originalGravityScale = rb.gravityScale;
    }
    
    public void RestoreHealth(int amount)
    {
        currentHealth = maxHealth;
        Debug.Log($"[PlayerOver] 체력 복원: {currentHealth}/{maxHealth}");
        if (heartUI != null)
            heartUI.UpdateHearts(currentHealth, maxHealth);
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
        StatusTextManager.Instance.ShowMessage("메이드가 행동불능이 되었습니다!");

        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;

        if (player != null)
        {
            player.ignoreInput = true;
            Debug.Log("[PlayerOver] Player input ignored.");
        }
        else
        {
            Debug.LogWarning("[PlayerOver] Player script not found.");
        }

        // 카메라 타겟을 공주로 전환
        CameraFollow cf = FindObjectOfType<CameraFollow>();
        if (cf != null)
        {
            if (princess != null)
            {
                cf.SetTarget(princess.gameObject);
                Debug.Log("[PlayerOver] Camera target set to princess.");
            }
            else
            {
                Debug.LogWarning("[PlayerOver] Princess reference not assigned.");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerOver] CameraFollow not found.");
        }
    }
    
    public void OnRewindComplete(Vector2 restoredPosition)
    {
        transform.position = restoredPosition;
        RestoreHealth(0);
        Debug.Log("[PlayerOver] OnRewindComplete: Player restored.");

        rb.gravityScale = originalGravityScale;
        Debug.Log($"[PlayerOver] GravityScale restored to {originalGravityScale}.");

        CameraFollow cf = FindObjectOfType<CameraFollow>();
        if (cf != null)
        {
            cf.SetTarget(gameObject);
            Debug.Log("[PlayerOver] Camera target reset to player.");
        }
        else
        {
            Debug.LogWarning("[PlayerOver] CameraFollow not found on rewind complete.");
        }
    }
    
    public void ResumeAfterRewind()
    {
        if (player != null)
        {
            player.ignoreInput = false;
            Debug.Log("[PlayerOver] Player input re-enabled.");
        }
        // 공주 재작동: 공주 조종 플래그 해제
        Princess princessScript = princess.GetComponent<Princess>();
        if (princessScript != null)
        {
            princessScript.isControlled = false;
            Debug.Log("[PlayerOver] Princess control flag set to false.");
        }
        isDisabled = false;
    }
}
