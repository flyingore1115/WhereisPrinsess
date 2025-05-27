using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;

public class InteractionUIController : MonoBehaviour
{
    [Tooltip("상호작용 오브젝트 기준 UI 표시 위치 오프셋")]
    public Vector3 worldOffset = new Vector3(1.5f, 1f, 0);

    [Tooltip("UI 전체 스케일 (프리팹 Scale × worldScale)")]
    public float worldScale = 0.01f;

    [Tooltip("슬라이드 애니메이션 이동 거리")]
    public float slideDistance = 0.5f;

    [Tooltip("슬라이드 애니메이션 지속 시간")]
    public float slideDuration = 0.2f;

    private GameObject uiInstance;
    private Transform container;
    private TextMeshProUGUI keyText;
    private TextMeshProUGUI actionText;

    private Vector3 visibleLocalPos = Vector3.zero;
    private Vector3 hiddenLocalPos;
    private Coroutine slideCoroutine;

    void Awake()
    {
        // 1) Prefab 로드 & 인스턴스화
        var prefab = Resources.Load<GameObject>("Prefab/InteractionUI");
        if (prefab == null)
        {
            Debug.LogError("[InteractionUIController] 프리팹 'Resources/Prefab/InteractionUI'를 찾을 수 없습니다.");
            return;
        }

        uiInstance = Instantiate(prefab, transform.position + worldOffset, Quaternion.identity);
        // 씬 루트에 붙여 NPC 비활성화에 영향받지 않도록
        uiInstance.transform.SetParent(null, true);
        // worldScale 적용
        uiInstance.transform.localScale = Vector3.one * worldScale;

        // 2) Container 찾기
        container = uiInstance
            .GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name == "Container");
        if (container == null)
        {
            Debug.LogError("[InteractionUIController] 'Container' 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // 3) KeyText / ActionText 찾기
        keyText = container
            .GetComponentsInChildren<TextMeshProUGUI>(true)
            .FirstOrDefault(t => t.gameObject.name == "KeyText");
        actionText = container
            .GetComponentsInChildren<TextMeshProUGUI>(true)
            .FirstOrDefault(t => t.gameObject.name == "ActionText");
        if (keyText == null || actionText == null)
        {
            Debug.LogError("[InteractionUIController] 'KeyText' 또는 'ActionText'를 찾을 수 없습니다.");
            return;
        }

        // 4) 슬라이드용 초기 위치 계산
        visibleLocalPos = Vector3.zero;
        hiddenLocalPos  = visibleLocalPos - new Vector3(0, slideDistance, 0);
        container.localPosition = hiddenLocalPos;

        uiInstance.SetActive(false);
    }

    void LateUpdate()
    {
        // 매 프레임 오브젝트 기준 worldOffset 위치 고정
        if (uiInstance != null)
            uiInstance.transform.position = transform.position + worldOffset;
    }

    /// <summary>키+행동 텍스트 세팅 후 슬라이드-in</summary>
    public void Show(string key, string action)
    {
        if (container == null || uiInstance == null) return;

        keyText.text    = key;
        actionText.text = action;
        uiInstance.SetActive(true);

        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideTo(visibleLocalPos));
    }

    /// <summary>슬라이드-out 후 비활성화</summary>
    public void Hide()
    {
        if (container == null || uiInstance == null) return;

        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideAndDeactivate());
    }

    private IEnumerator SlideTo(Vector3 to)
    {
        Vector3 from = container.localPosition;
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            container.localPosition = Vector3.Lerp(from, to, elapsed / slideDuration);
            yield return null;
        }
        container.localPosition = to;
    }

    private IEnumerator SlideAndDeactivate()
    {
        yield return SlideTo(hiddenLocalPos);
        uiInstance.SetActive(false);
    }

    void OnDestroy()
    {
        if (uiInstance != null)
            Destroy(uiInstance);
    }
}
