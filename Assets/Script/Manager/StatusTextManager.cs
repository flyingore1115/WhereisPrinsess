using UnityEngine;
using UnityEngine.UI;

public class StatusTextManager : MonoBehaviour
{
    public static StatusTextManager Instance;

    public Text statusText; // UI 텍스트 연결
    public float messageDuration = 2f; // 메시지 표시 시간

    private Coroutine currentMessageCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (statusText != null)
        {
            statusText.enabled = false; // UI 텍스트를 비활성화
        }
    }

    public void ShowMessage(string message)
    {
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine); // 이전 코루틴 중지
        }

        currentMessageCoroutine = StartCoroutine(DisplayMessage(message)); // 새 메시지 표시
    }

    private System.Collections.IEnumerator DisplayMessage(string message)
    {
        if (statusText != null)
        {
            statusText.text = message; // 메시지 설정
            statusText.enabled = true; // 텍스트 활성화

            yield return new WaitForSeconds(messageDuration);

            statusText.enabled = false; // 텍스트 비활성화
        }
    }
}
