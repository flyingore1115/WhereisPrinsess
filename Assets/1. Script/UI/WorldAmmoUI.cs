using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class WorldAmmoUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI ammoText;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2f, 0);
    public float displayDuration = 0.5f;

    private Transform target;
    private CanvasGroup cg;

    /// <summary>
    /// 외부에서 초기화합니다.
    /// </summary>
    public void Init(int currentAmmo, int maxAmmo, Transform target, Vector3 offset, float duration)
    {
        this.target = target;
        this.offset = offset;
        this.displayDuration = duration;

        if (ammoText != null)
            ammoText.text = $"{currentAmmo} / {maxAmmo}";

        cg = GetComponent<CanvasGroup>();
        cg.alpha = 1f;

        // 일정 시간 후 자동 파괴
        StartCoroutine(HideAndDestroy());
    }

    void LateUpdate()
    {
        if (target == null) return;
        // 플레이어 위치 + 오프셋
        transform.position = target.position + offset;
        // 항상 카메라를 바라보도록
        transform.rotation = Camera.main.transform.rotation;
    }

    IEnumerator HideAndDestroy()
    {
        yield return new WaitForSeconds(displayDuration);
        // 간단하게 곧장 사라지도록
        Destroy(gameObject);
    }
}
