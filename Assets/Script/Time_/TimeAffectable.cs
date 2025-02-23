using UnityEngine;

public class TimeAffectable : MonoBehaviour, ITimeAffectable
{
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Material grayScaleMaterial; // 리소스에서 가져올 것

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;

        // Resources 폴더에서 "GrayScaleMaterial" 이름으로 로드
        grayScaleMaterial = Resources.Load<Material>("GrayScaleMaterial");

        if (grayScaleMaterial == null)
        {
            Debug.LogError("GrayScaleMaterial을 Resources 폴더에서 찾을 수 없습니다!");
        }
    }

    public void StopTime()
    {
        if (grayScaleMaterial != null)
        {
            spriteRenderer.material = grayScaleMaterial;
        }
    }

    public void ResumeTime()
    {
        // 움직임 재개 필요시 작성
    }

    public void RestoreColor()
    {
        spriteRenderer.material = originalMaterial;
    }
}
