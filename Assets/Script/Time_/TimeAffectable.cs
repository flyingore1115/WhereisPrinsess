using UnityEngine;

public class TimeAffectable : MonoBehaviour, ITimeAffectable
{
    public Material grayscaleMaterial; // 흑백 쉐이더 Material
    private Material originalMaterial;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;
    }

    public void StopTime()
    {
        if (spriteRenderer != null && grayscaleMaterial != null)
        {
            spriteRenderer.material = grayscaleMaterial;
        }
    }

    public void ResumeTime()
    {
        RestoreColor();
    }

    public void RestoreColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
        }
    }
}
