using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BossIntroUI : MonoBehaviour
{
    [Header("패널")]
    public RectTransform topPanel;
    public RectTransform bottomPanel;

    [Header("텍스트 및 이미지 그룹")]
    public RectTransform textRoot;            // 텍스트 기준
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI bossTitleText;
    public Vector2 textRootStartPos;
    public Vector2 textRootTargetPos;

    public CanvasGroup textGroup;

    [Header("보스 이미지")]
    public Image bossImage;
    public Vector2 imageStartPos;
    public Vector2 imageTargetPos;

    [Header("판넬 시작 위치")]
    public Vector2 topPanelStartPos;
    public Vector2 bottomPanelStartPos;

    [Header("속도 설정")]
    public float panelMoveDuration = 0.5f;
    public float imageMoveDuration = 0.5f;
    public float textFadeDuration = 0.5f;
    public float outroFadeDuration = 0.5f;

    private bool canExit = false;

    public System.Action OnIntroEnd; // 외부에서 연결할 콜백

    void Start()
    {
        PlayIntro("테디", "곰인형 집행관");
    }

    public void PlayIntro(string bossName, string bossTitle)
    {
        bossNameText.text = bossName;
        bossTitleText.text = bossTitle;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // 초기화
        topPanel.anchoredPosition = topPanelStartPos;
        bottomPanel.anchoredPosition = bottomPanelStartPos;
        textRoot.anchoredPosition = textRootStartPos;
        textGroup.alpha = 0;
        bossImage.rectTransform.anchoredPosition = imageStartPos;
        Color imageColor = bossImage.color;
        imageColor.a = 0;
        bossImage.color = imageColor;

        // 판넬 + 이미지 이동 동시에
        float t = 0;
        while (t < panelMoveDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / panelMoveDuration);
            float smooth = Mathf.SmoothStep(0, 1, lerp);

            topPanel.anchoredPosition = Vector2.Lerp(topPanelStartPos, Vector2.zero, smooth);
            bottomPanel.anchoredPosition = Vector2.Lerp(bottomPanelStartPos, Vector2.zero, smooth);
            bossImage.rectTransform.anchoredPosition = Vector2.Lerp(imageStartPos, imageTargetPos, smooth);

            imageColor.a = smooth;
            bossImage.color = imageColor;

            yield return null;
        }
        topPanel.anchoredPosition = Vector2.zero;
        bottomPanel.anchoredPosition = Vector2.zero;
        bossImage.rectTransform.anchoredPosition = imageTargetPos;
        imageColor.a = 1;
        bossImage.color = imageColor;

        // 텍스트 등장
        t = 0;
        while (t < textFadeDuration)
        {
            t += Time.deltaTime;
            float smooth = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / textFadeDuration));
            textRoot.anchoredPosition = Vector2.Lerp(textRootStartPos, textRootTargetPos, smooth);
            textGroup.alpha = smooth;
            yield return null;
        }
        textRoot.anchoredPosition = textRootTargetPos;
        textGroup.alpha = 1;

        canExit = true;
    }

    void Update()
    {
        if (canExit && Input.anyKeyDown)
        {
            canExit = false;
            StartCoroutine(FadeOutAndClose());
        }
    }

    private IEnumerator FadeOutAndClose()
    {
        float t = 0;
        Vector2 topStart = topPanel.anchoredPosition;
        Vector2 botStart = bottomPanel.anchoredPosition;

        Vector2 topTarget = topPanelStartPos;
        Vector2 botTarget = bottomPanelStartPos;

        Vector2 imageStart = bossImage.rectTransform.anchoredPosition;
        Vector2 imageTarget = imageStartPos;

        Color imageColor = bossImage.color;

        while (t < outroFadeDuration)
        {
            t += Time.deltaTime;
            float smooth = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / outroFadeDuration));

            topPanel.anchoredPosition = Vector2.Lerp(topStart, topTarget, smooth);
            bottomPanel.anchoredPosition = Vector2.Lerp(botStart, botTarget, smooth);

            imageColor.a = 1f - smooth;
            bossImage.color = imageColor;
            textGroup.alpha = 1f - smooth;

            yield return null;
        }

        topPanel.anchoredPosition = topTarget;
        bottomPanel.anchoredPosition = botTarget;
        imageColor.a = 0f;
        bossImage.color = imageColor;
        textGroup.alpha = 0f;

        OnIntroEnd?.Invoke(); // ✅ 인트로 종료 콜백
        Destroy(gameObject);  // ✅ 자기 자신 삭제
    }
}
