using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class StorySceneManager : MonoBehaviour
{
    [Header("Scene Start Trigger")]
    public string startTriggerID;       // 씬 시작 시 자동 대사 실행용 ID (빈 문자열이면 무시)

    [Header("UI References")]
    public Text characterNameText;      // 캐릭터 이름 텍스트
    public Text dialogueText;           // 대사 텍스트
    public GameObject dialoguePanel;    // 대화창 패널

    [Header("Choice UI References")]
    public GameObject choicePanel;      // 선택지 패널
    public Button choiceButtonPrefab;   // 선택지 버튼 프리팹

    [Header("Typing Effect Settings")]
    public float typingSpeed = 0.05f;   // 글자 타이핑 속도

    [Header("Camera Settings for Dialogue")]
    public CameraFollow cameraFollow;   // 카메라 팔로우 스크립트 참조
    public float dialogueCameraSize = 8f;
    public float cameraZoomDuration = 0.5f;

    // 트리거ID별 대사 시퀀스 (회차에 따라 다른 배열)
    private Dictionary<string, DialogueEntry[][]> dialogueSequences = new Dictionary<string, DialogueEntry[][]>
    {
        { "Intro", new DialogueEntry[][]
            {
                new DialogueEntry[]
                {
                    new DialogueEntry("", "…"),
                    new DialogueEntry("", "일… 나, …?"),
                    new DialogueEntry("", "이제 일어나보렴"),
                    new DialogueEntry("", "…어,"),
                    new DialogueEntry("Mr. 스프라우트 박사", "아, 일어났다."),
                    new DialogueEntry("스프라우트", "그 잠깐 사이에 잠들다니, 많이 피곤했나보구나"),
                    new DialogueEntry("스프라우트", "일어날 수 있겠니?"),
                    // 선택지
                    new DialogueEntry(new string[]{ "네에…" }),
                    new DialogueEntry("", "당신은 겨우 일어서 발을 딛으려 했다."),
                    new DialogueEntry("", "… 아무래도 걷는 것을 잊어버린 모양인지 발을 디딜 수 없었지만"),
                    new DialogueEntry("스프라우트", "걷기는 A나 D, 혹은 방향키로 할 수 있단다."),
                    new DialogueEntry("스프라우트", "아무튼, 다들 기다리고 있단다."),
                    new DialogueEntry("스프라우트", "…"),
                    new DialogueEntry("스프라우트", "긴장하지 않아도 돼, 물론 꽤… 까칠하지만 사실 속은 굉장히 여린 아이거든"),
                    new DialogueEntry("스프라우트", "그러니까 문제는 없을거란다! …아마도"),
                    new DialogueEntry("스프라우트", "저기 앞에 있는 304호 병실 문을 열고 들어가면 돼")
                }
            }
        },
        { "re", new DialogueEntry[][]
            {
            new DialogueEntry[]
                {
                    new DialogueEntry("스프라우트", "304호 병실은 여기서 조금만 걸어가면 돼"),
                    new DialogueEntry("스프라우트", "딸아이와 친하게 지내줄거라 기대하고 있단다")
                }
            }
        },
        { "A", new DialogueEntry[][]
            {
                new DialogueEntry[]
                {
                    new DialogueEntry("", "…"),
                    new DialogueEntry("", "…?"),
                    new DialogueEntry("", "넌 뭐야?"),
                    new DialogueEntry("Ms. 스프라우트", "아, 왔구나!"),
                    new DialogueEntry("스프라우트", "인사하렴 ■■■! 이제부터 네 친구가 되어줄 아이란다!"),
                    new DialogueEntry("", "… 뭐?"),
                    new DialogueEntry("", "당신은 분위기가 점차 냉각되는 것을 느낀다.")
                }
            }
        }
    };

    private bool isDialogueActive = false;
    private Dictionary<string, int> dialogueTriggerCount = new Dictionary<string, int>();
    private Coroutine dialogueCoroutine;

    void Start()
    {
        dialoguePanel?.SetActive(false);
        choicePanel?.SetActive(false);

        // 씬 시작 자동 대화
        if (!string.IsNullOrEmpty(startTriggerID))
            StartDialogueForTrigger(startTriggerID);
    }

    // 플레이어가 트리거 영역에 진입했을 때
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDialogueActive)
            StartDialogueForTrigger(gameObject.name);
    }

    public void StartDialogueForTrigger(string triggerID)
    {
        if (isDialogueActive) return;
        if (!dialogueSequences.TryGetValue(triggerID, out var sequences)) return;

        int count = dialogueTriggerCount.ContainsKey(triggerID) ? dialogueTriggerCount[triggerID] : 0;
        int index = Mathf.Clamp(count, 0, sequences.Length - 1);
        DialogueEntry[] entries = sequences[index];
        dialogueTriggerCount[triggerID] = count + 1;

        isDialogueActive = true;
        Player.Instance.ignoreInput = true;
        dialoguePanel.SetActive(true);
        StartCoroutine(SmoothZoomCamera(dialogueCameraSize, cameraZoomDuration));

        dialogueCoroutine = StartCoroutine(PlayEntries(entries));
    }

    private IEnumerator PlayEntries(DialogueEntry[] entries)
    {
        foreach (var entry in entries)
        {
            if (entry.options != null && entry.options.Length > 0)
                yield return StartCoroutine(ShowChoices(entry.options));
            else
            {
                yield return StartCoroutine(TypeSentence(entry.characterName, entry.dialogueText));
                yield return new WaitUntil(() => Input.anyKeyDown);
            }
        }
        EndDialogue();
    }

    private IEnumerator ShowChoices(string[] options)
    {
        choicePanel.SetActive(true);
        int selected = -1;
        var buttons = new List<Button>();
        for (int i = 0; i < options.Length; i++)
        {
            var btn = Instantiate(choiceButtonPrefab, choicePanel.transform);
            int idx = i;
            btn.GetComponentInChildren<Text>().text = options[i];
            btn.onClick.AddListener(() => { selected = idx; });
            buttons.Add(btn);
        }
        yield return new WaitUntil(() => selected >= 0);
        buttons.ForEach(b => Destroy(b.gameObject));
        choicePanel.SetActive(false);
        // 선택값: options[selected]
    }

    private IEnumerator TypeSentence(string charName, string text)
    {
        characterNameText.text = charName;
        dialogueText.text = string.Empty;
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator SmoothZoomCamera(float targetSize, float duration)
    {
        if (cameraFollow == null) yield break;
        var cam = cameraFollow.GetComponent<Camera>();
        if (cam == null) yield break;

        float start = cam.orthographicSize;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cameraFollow.SetCameraSize(Mathf.Lerp(start, targetSize, t / duration));
            yield return null;
        }
        cameraFollow.SetCameraSize(targetSize);
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        Player.Instance.ignoreInput = false;
        StartCoroutine(SmoothZoomCamera(cameraFollow.defaultSize, cameraZoomDuration));
        isDialogueActive = false;
    }
}

[System.Serializable]
public class DialogueEntry
{
    public string characterName;
    public string dialogueText;
    public string[] options;

    public DialogueEntry(string name, string text)
    {
        characterName = name;
        dialogueText = text;
        options = null;
    }
    public DialogueEntry(string[] opts)
    {
        characterName = null;
        dialogueText = null;
        options = opts;
    }
}
