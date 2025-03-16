using UnityEngine;
using System;

public class BaseEnemy : MonoBehaviour, ITimeAffectable
{
    protected Transform princess;
    protected Transform player;

    // (원본) 적 종류 식별용
    public string prefabName = "Self_Enemy";

    // ★추가: 고유 ID
    //  (체크포인트 시점에 살아있는 적만 기록 + 되감기 시 ID로 재활성화)
    private static int globalIDCounter = 1;
    public int enemyID = -1;

    protected bool isTimeStopped = false;
    protected bool isAggroOnPlayer = false;
    public bool isDead = false;

    protected SpriteRenderer spriteRenderer;
    protected Animator animator;
    protected Material originalMaterial;
    public Material grayscaleMaterial;

    protected virtual void Awake()
    {
        // ★ ID가 없으면 새로 할당
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
    }

    public virtual void StopTime()
    {
        if (this == null || spriteRenderer == null) return;

        isTimeStopped = true;
        if (grayscaleMaterial != null)
        {
            Material newGrayMat = new Material(grayscaleMaterial);
            if (originalMaterial != null && originalMaterial.mainTexture != null)
            {
                newGrayMat.mainTexture = originalMaterial.mainTexture;
            }
            newGrayMat.color = spriteRenderer.color;
            spriteRenderer.material = newGrayMat;
        }
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
        RestoreColor();
    }

    public void RestoreColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
        }
    }

    // (원본) 적이 공격받으면 파괴
    // → 수정: gameObject.SetActive(false)
    public virtual void TakeDamage()
    {
        if (isDead) return;
        Debug.Log($"[Enemy] {gameObject.name} took damage!");
        isDead = true;
        gameObject.SetActive(false); 
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
