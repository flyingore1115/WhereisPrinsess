using UnityEngine;

public class TimeAffectable : MonoBehaviour, ITimeAffectable
{
    public Material grayscaleMaterial;
    private Material originalMaterial;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // ✅ 원본 Material이 있는 경우에만 저장
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.sharedMaterial;
        }
    }

    public void StopTime()
    {
        if (spriteRenderer != null && grayscaleMaterial != null)
        {
            // ✅ 강제로 Material을 변경 (복사된 오브젝트도 정상 작동)
            spriteRenderer.material = grayscaleMaterial;
        }
    }

    public void ResumeTime()
    {
        RestoreColor();
    }

    public void RestoreColor()
    {
        if (spriteRenderer != null && originalMaterial != null)
        {
            // ✅ 원본 Material 복구 (복사본도 동일하게 적용됨)
            spriteRenderer.material = originalMaterial;
        }
    }
}
