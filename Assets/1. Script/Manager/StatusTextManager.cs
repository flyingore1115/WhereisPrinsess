using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatusTextManager : MonoBehaviour
{
    public static StatusTextManager Instance;

    public Text statusText; // Inspector에 연결
    public float messageDuration = 2f;

    private Coroutine currentMessageCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (statusText != null)
            statusText.enabled = false;
    }

    // 즉시 메시지 표시 후 일정 시간 뒤 사라지는 방식
    public void ShowMessage(string message)
    {
        // 만약 “아무 키나 누르면 시작” 처럼 계속 띄워두려면, ‘유지’ 방식으로 해도 좋음
        if (currentMessageCoroutine != null)
            StopCoroutine(currentMessageCoroutine);
        currentMessageCoroutine = StartCoroutine(DisplayMessage(message));
    }

    private IEnumerator DisplayMessage(string message)
    {
        if (statusText == null) yield break;

        if (string.IsNullOrEmpty(message))
        {
            // message가 빈 문자열이면 텍스트 숨김
            statusText.text = "";
            statusText.enabled = false;
            yield break;
        }

        statusText.text = message;
        statusText.enabled = true;

        // 만약 “아무 키나…” 같은 경우는 무한 표시가 필요할 수도 있으니 조건 분기 가능
        // 여기서는 예시로, messageDuration이 9999 등으로 설정해도 됨
        yield return new WaitForSecondsRealtime(messageDuration);
        statusText.enabled = false;
    }
}
