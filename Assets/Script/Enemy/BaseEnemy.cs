using UnityEngine;

public class BaseEnemy : MonoBehaviour, ITimeAffectable
{
    protected Transform princess;
    protected Transform player;
    protected bool isTimeStopped = false;
    protected bool isAggroOnPlayer = false;

    protected SpriteRenderer spriteRenderer;
    protected Animator animator;
    protected Material originalMaterial;
    public Material grayscaleMaterial;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.sharedMaterial;
        }

        princess = GameObject.FindGameObjectWithTag("Princess")?.transform;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public virtual void StopTime()
    {
        if (this == null || spriteRenderer == null) return;
            
        isTimeStopped = true;
        if (grayscaleMaterial != null)
        {
            // 새로운 Material 인스턴스 생성 및 원본 텍스처 복사
            Material newGrayMat = new Material(grayscaleMaterial);
            if (originalMaterial != null && originalMaterial.mainTexture != null)
            {
                newGrayMat.mainTexture = originalMaterial.mainTexture;
            }
            // 원본 색상 복사 (예: spriteRenderer.color를 그대로 설정)
            newGrayMat.color = spriteRenderer.color;
            
            // 만약 쉐이더에서 _Color 대신 _MainColor를 사용한다면:
            // newGrayMat.SetColor("_MainColor", spriteRenderer.color);
            
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
            spriteRenderer.material = originalMaterial; // 원래 마테리얼 복원
        }
    }


    public virtual void TakeDamage()
    {
        Debug.Log($"[Enemy] {gameObject.name} Took Damage!");
        Destroy(gameObject);
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
