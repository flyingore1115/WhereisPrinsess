using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class StorySceneManager : MonoBehaviour
{
    [Header("UI References")]
    public Text characterNameText;   // 캐릭터 이름 UI
    public Text dialogueText;        // 대사 UI
    public GameObject dialoguePanel; // 대화창 패널

    [Header("Typing Effect Settings")]
    public float typingSpeed = 0.05f; // 글자 하나씩 찍히는 속도
    private bool isTyping = false;

    [Header("Camera Settings for Dialogue")]
    public CameraFollow cameraFollow;     // 씬 내 CameraFollow 참조
    public float dialogueCameraSize = 8f;   // 대화 중 확대될 카메라 크기
    public float cameraZoomDuration = 0.5f; // 줌 인/아웃 시간

    // ─────────────────────────────────────────────────────────────────────────────
    // '트리거ID'별 대사를 코드에 직접 등록
    // ─────────────────────────────────────────────────────────────────────────────
    // 여기서는 기본 대화 시퀀스만 등록하고, 재대화 시에는 대체 대사를 출력합니다.
    private Dictionary<string, DialogueLine[]> dialogueMap = new Dictionary<string, DialogueLine[]>
    {
        {
            "A",
            new DialogueLine[]
            {
                new DialogueLine("???",  "일어나보렴,"),
                new DialogueLine("???",  "많이 긴장한 모양이구나"),
                new DialogueLine("???",  "한번 몸을 움직여보겠니?"),
            }
        },
        {
            "B",
            new DialogueLine[]
            {
                new DialogueLine("프릴", "문이 잠겼어. 이걸 어떻게 열지?")
            }
        },
        // 필요한 만큼 추가 가능
    };

    // 대화 진행 여부를 판단하기 위한 플래그 (대화 시작 후 true로 설정)
    private bool isDialogueActive = false;
    // 각 트리거별로 몇 번 대화가 진행되었는지 기록 (처음 대화면 0, 이후 재대화 시 1 이상)
    private Dictionary<string, int> dialogueTriggerCount = new Dictionary<string, int>();

    private Coroutine dialogueCoroutine;

    void Start()
    {
        // 게임 시작 시 대화창 패널은 비활성화
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    // 외부에서 트리거 ID를 넘겨 대화를 시작할 때 호출
    public void StartDialogueForTrigger(string triggerID)
    {
        // 대화가 이미 진행 중이면 무시
        if (isDialogueActive)
        {
            Debug.Log("대화가 진행 중이므로 새로운 대화 호출을 무시합니다.");
            return;
        }

        DialogueLine[] lines = null;

        // 같은 트리거에 대해 이전에 대화한 적이 있으면 대체 대사 사용
        if (dialogueTriggerCount.ContainsKey(triggerID) && dialogueTriggerCount[triggerID] >= 1)
        {
            // 예시: 재대화 시 간단한 안내 메시지 출력
            lines = new DialogueLine[]
            {
                new DialogueLine("", "이미 대화를 마쳤습니다. 다른 곳으로 이동하세요.")
            };
        }
        else
        {
            if (!dialogueMap.TryGetValue(triggerID, out lines))
            {
                Debug.LogWarning($"[StorySceneManager] 존재하지 않는 트리거 ID: {triggerID}");
                return;
            }
            dialogueTriggerCount[triggerID] = 1; // 처음 대화임을 기록
        }

        isDialogueActive = true;

        // 플레이어 입력 차단 (대화 중에는 이동, 공격 등 X)
        if (Player.Instance != null)
            Player.Instance.ignoreInput = true;

        // 대화창 패널 활성화
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // 부드러운 카메라 줌 인 시작
        if (cameraFollow != null)
            StartCoroutine(SmoothZoomCamera(dialogueCameraSize, cameraZoomDuration));

        // 대사 재생 코루틴 시작
        dialogueCoroutine = StartCoroutine(ShowMultipleDialogues(lines));
    }

    // 여러 줄의 대사를 순차적으로 보여주고, 각 줄마다 아무 키나 누를 때까지 대기
    private IEnumerator ShowMultipleDialogues(DialogueLine[] lines)
    {
        foreach (var line in lines)
        {
            // 한 줄씩 타이핑 효과로 출력
            yield return StartCoroutine(TypeSentence(line.characterName, line.dialogueText));

            // 모든 글자가 출력된 후, 아무 키나 누르면 다음 대사로 넘어감
            yield return new WaitUntil(() => Input.anyKeyDown);
        }

        // 모든 대사가 끝나면 후속 처리
        EndDialogue();
    }

    // 한 문장을 타이핑 효과로 출력하는 코루틴
    private IEnumerator TypeSentence(string charName, string text)
    {
        isTyping = true;
        characterNameText.text = charName;
        dialogueText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    // 대화가 모두 끝난 후 호출 (대화창 비활성화, 플레이어 입력 복원, 카메라 원복)
    private void EndDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // 플레이어 입력 복원
        if (Player.Instance != null)
            Player.Instance.ignoreInput = false;

        // 부드러운 카메라 줌 아웃 (기본 크기로)
        if (cameraFollow != null)
            StartCoroutine(SmoothZoomCamera(cameraFollow.defaultSize, cameraZoomDuration));

        isDialogueActive = false;

        Debug.Log("대사가 모두 끝났습니다. 후속 처리를 여기에 넣으세요.");
    }

    // 카메라 줌을 부드럽게 처리하는 코루틴 (targetSize로 duration 시간 동안 보간)
    private IEnumerator SmoothZoomCamera(float targetSize, float duration)
    {
        if (cameraFollow == null)
            yield break;

        // CameraFollow의 카메라 크기 접근은 CameraFollow 내부의 Camera 컴포넌트를 통해서 합니다.
        Camera cam = cameraFollow.GetComponent<Camera>();
        if (cam == null)
            yield break;

        float startSize = cam.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newSize = Mathf.Lerp(startSize, targetSize, elapsed / duration);
            cameraFollow.SetCameraSize(newSize);
            yield return null;
        }
        cameraFollow.SetCameraSize(targetSize);
    }
}

[System.Serializable]
public struct DialogueLine
{
    public string characterName; 
    public string dialogueText;

    public DialogueLine(string name, string text)
    {
        characterName = name;
        dialogueText = text;
    }
}
