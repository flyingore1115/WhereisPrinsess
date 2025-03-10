using UnityEngine;

public class TimeAffectable : MonoBehaviour, ITimeAffectable
{
    public Material grayscaleMaterial;
    private Material originalMaterial;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.sharedMaterial;
        }
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
        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
        }
    }
}
