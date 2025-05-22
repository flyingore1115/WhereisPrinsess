using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossIntroUI : MonoBehaviour
{
    public RectTransform topPanel;
    public RectTransform bottomPanel;
    public RectTransform bossImage;
    public TextMeshProUGUI bossText;

    public float panelMoveDuration = 0.5f;
    public float imageMoveDuration = 0.5f;
    public float textFadeDuration  = 0.5f;
    public Vector2 topPanelStartPos;
    public Vector2 bottomPanelStartPos;
    public Vector2 imageStartPos;
    public Vector2 textStartPos;

    private CanvasGroup textGroup;

    void Awake()
    {
        textGroup = bossText.GetComponent<CanvasGroup>();
        if (textGroup == null)
        {
            textGroup = bossText.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void PlayIntro(string bossName)
    {
        bossText.text = bossName;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // 초기화
        topPanel.anchoredPosition    = topPanelStartPos;
        bottomPanel.anchoredPosition = bottomPanelStartPos;
        bossImage.anchoredPosition   = imageStartPos;
        bossText.rectTransform.anchoredPosition = textStartPos;
        textGroup.alpha = 0;

        // 1. 위아래 패널 이동
        yield return StartCoroutine(MoveRect(topPanel, Vector2.zero, panelMoveDuration));
        yield return StartCoroutine(MoveRect(bottomPanel, Vector2.zero, panelMoveDuration));

        // 2. 이미지 위로 이동
        yield return StartCoroutine(MoveRect(bossImage, Vector2.zero, imageMoveDuration));

        // 3. 텍스트 알파 + 이동
        yield return StartCoroutine(ShowText());
    }

    private IEnumerator MoveRect(RectTransform rect, Vector2 targetPos, float duration)
    {
        Vector2 startPos = rect.anchoredPosition;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t / duration);
            yield return null;
        }
        rect.anchoredPosition = targetPos;
    }

    private IEnumerator ShowText()
    {
        Vector2 startPos = textStartPos;
        Vector2 endPos   = Vector2.zero;
        float t = 0;

        while (t < textFadeDuration)
        {
            t += Time.deltaTime;
            bossText.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t / textFadeDuration);
            textGroup.alpha = t / textFadeDuration;
            yield return null;
        }

        bossText.rectTransform.anchoredPosition = endPos;
        textGroup.alpha = 1;
    }
}
