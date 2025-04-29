using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public enum EntryType { Dialogue, Choice, Action }

[Serializable]
public class DialogueEntry
{
    public EntryType type;
    public string characterName;
    public string dialogueText;
    public string[] options;
    public Func<IEnumerator> action;

    // Dialogue entry
    public DialogueEntry(string name, string text)
    {
        type = EntryType.Dialogue;
        characterName = name;
        dialogueText = text;
        options = null;
        action = null;
    }
    // Choice entry
    public DialogueEntry(string[] opts)
    {
        type = EntryType.Choice;
        characterName = null;
        dialogueText = null;
        options = opts;
        action = null;
    }
    // Action entry
    public DialogueEntry(Func<IEnumerator> act)
    {
        type = EntryType.Action;
        characterName = null;
        dialogueText = null;
        options = null;
        action = act;
    }
}

public class StorySceneManager : MonoBehaviour
{
    [Header("Scene Start Trigger ID")]
    public string startTriggerID;

    [Header("UI References")]
    public Text characterNameText;
    public Text dialogueText;
    public GameObject dialoguePanel;

    [Header("Choice UI References")]
    public GameObject choicePanel;
    public Button choiceButtonPrefab;

    [Header("Tutorial UI References")]
    public GameObject tutorialPanel;
    public Text tutorialText;

    [Header("Typing Effect Settings")]
    public float typingSpeed = 0.05f;

    [Header("Camera Settings for Dialogue")]
    public CameraFollow cameraFollow;
    public float dialogueCameraSize = 8f;
    public float cameraZoomDuration = 0.5f;

    private Dictionary<string, DialogueEntry[][]> dialogueSequences;
    private bool isDialogueActive;
    private Dictionary<string, int> dialogueTriggerCount;
    private Coroutine dialogueCoroutine;

    void Awake()
    {
        dialogueTriggerCount = new Dictionary<string, int>();
        InitializeSequences();
    }

    void Start()
    {
        dialoguePanel?.SetActive(false);
        choicePanel?.SetActive(false);
        tutorialPanel?.SetActive(false);

        if (!string.IsNullOrEmpty(startTriggerID))
            StartDialogueForTrigger(startTriggerID);
    }

    void InitializeSequences()
    {
        dialogueSequences = new Dictionary<string, DialogueEntry[][]>
        {
            { "Intro", new DialogueEntry[][]
                {
                    new DialogueEntry[]
                    {
                        new DialogueEntry("", "…"),
                        new DialogueEntry("???", "일… 나, …?"),
                        new DialogueEntry("", "귓가에 남성의 목소리가 들린다."),
                        new DialogueEntry("???", "일어날 수 있겠니?"),
                        new DialogueEntry("Mr. 스프라우트", "괜찮니? 영 상태가 좋아 보이지는 않는구나."),
                        new DialogueEntry("", "당신은 일어서 앞으로 걸어보려 시도했으나, 갓 태어난 기린처럼 휘청거렸다."),
                        new DialogueEntry("Mr. 스프라우트", "아하..."),
                        new DialogueEntry("Mr. 스프라우트", "걷기는 A나 D, 혹은 방향키로 할 수 있단다."),
                        new DialogueEntry("Mr. ", "할 수 있겠니?"),
                        new DialogueEntry(new string[]{ "네에…" }),
                        new DialogueEntry("Mr. 스프라우트", "어디로 가야 하는지는 기억나지? 저기 앞의 301호가 목적지란다"),
                        new DialogueEntry("Mr. 스프라우트", "긴장할 필요는 없어. 그 애는 조...금 예민하긴 하지만 본성은 착한 아이란다."),
                        new DialogueEntry("Mr. 스프라우트", "분명 같이 잘 지낼 수 있을거야."),
                        new DialogueEntry("", "당신은 알았다는 듯 슬며시 고개를 끄덕거렸다."),
                    }
                }
            },
            { "Intro2", new DialogueEntry[][]
                {
                    new DialogueEntry[]
                    {
                        new DialogueEntry("Mr. 스프라우트","병실은 301호란다.")
                    }
                }
            },
            { "two", new DialogueEntry[][]
                {
                    new DialogueEntry[]
                    {
                        new DialogueEntry("???", "...?"),
                        //new DialogueEntry(() => AttackTutorial()),

                        // 카메라를 아가씨로 이동
                        new DialogueEntry(() => FocusCameraOnTarget("Lady", 1.0f)),

                        new DialogueEntry("","안에 있는 것은 여자아이였다."),
                        new DialogueEntry("","오늘 생일을 맞은 듯, 생일 모자와 각종 선물을 침대 위에 두고 있는 어린 소녀 말이다."),
                        
                        // 아가씨 애니메이션 재생
                        //new DialogueEntry(() => PlayCharacterAnimation("Lady", "Throw")), 병실 커튼 여는 애니
                        
                        new DialogueEntry("소녀", "...이건 뭐야?"),
                        // 카메라를 스프라우트에게로
                        new DialogueEntry(() => FocusCameraOnTarget("Ms_Sprout", 1.0f)),
                        new DialogueEntry("Ms. 스프라우트", "아..."),
                    }
                }
            }
        };
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDialogueActive)
            StartDialogueForTrigger(gameObject.name);
    }

    public void StartDialogueForTrigger(string triggerID)
    {
        if (isDialogueActive) return;
        if (!dialogueSequences.TryGetValue(triggerID, out var sequences)) return;

        int count = dialogueTriggerCount.ContainsKey(triggerID)
            ? dialogueTriggerCount[triggerID] : 0;
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
            switch (entry.type)
            {
                case EntryType.Dialogue:
                    yield return StartCoroutine(TypeSentence(entry.characterName, entry.dialogueText));
                    yield return new WaitUntil(() => Input.anyKeyDown);
                    break;
                case EntryType.Choice:
                    yield return StartCoroutine(ShowChoices(entry.options));
                    break;
                case EntryType.Action:
                    yield return StartCoroutine(entry.action());
                    break;
            }
        }
        EndDialogue();
    }

    private IEnumerator ShowChoices(string[] options)
    {
        choicePanel.SetActive(true);
        int selected = -1;
        List<Button> buttons = new List<Button>();
        for (int i = 0; i < options.Length; i++)
        {
            Button btn = Instantiate(choiceButtonPrefab, choicePanel.transform);
            int idx = i;
            btn.GetComponentInChildren<Text>().text = options[i];
            btn.onClick.AddListener(() => selected = idx);
            buttons.Add(btn);
        }
        yield return new WaitUntil(() => selected >= 0);
        buttons.ForEach(b => Destroy(b.gameObject));
        choicePanel.SetActive(false);
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

    // 공격 튜토리얼 (시간 정지 -> 클릭 -> 시간 해제)
    private IEnumerator AttackTutorial()
    {
        dialoguePanel.SetActive(false);
        tutorialPanel.SetActive(true);
        tutorialText.text = "스페이스바를 눌러 시간을 멈춰보세요.";
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        Player.Instance.StopTime();

        tutorialText.text = "케이크나 빵을 클릭하세요.";
        yield return new WaitUntil(() =>
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);
                return hit.collider != null && hit.collider.CompareTag("TutorialTarget");
            }
            return false;
        });

        Player.Instance.ResumeTime();

        tutorialPanel.SetActive(false);
        dialoguePanel.SetActive(true);
        yield return new WaitForSeconds(0.5f);
    }

    // 카메라를 특정 대상을 따라 부드럽게 이동시킴
    private IEnumerator FocusCameraOnTarget(string targetName, float waitTime)
    {
        GameObject target = GameObject.Find(targetName);
        if (target == null)
        {
            Debug.LogWarning($"[StorySceneManager] 타겟 '{targetName}'를 찾을 수 없습니다.");
            yield break;
        }
        cameraFollow.EnableStoryMode(true);
        cameraFollow.SetTarget(target);
        yield return new WaitForSeconds(waitTime);
    }

    // 특정 캐릭터의 애니메이션 트리거를 재생함
    private IEnumerator PlayCharacterAnimation(string characterName, string triggerName)
    {
        GameObject character = GameObject.Find(characterName);
        if (character == null)
        {
            Debug.LogWarning($"[StorySceneManager] 캐릭터 '{characterName}'를 찾을 수 없습니다.");
            yield break;
        }
        Animator animator = character.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"[StorySceneManager] Animator가 '{characterName}'에 없습니다.");
            yield break;
        }
        animator.SetTrigger(triggerName);
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator SmoothZoomCamera(float targetSize, float duration)
    {
        if (cameraFollow == null) yield break;
        Camera cam = cameraFollow.GetComponent<Camera>();
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
        // 대화창 비활성화
        dialoguePanel.SetActive(false);
        
        // 플레이어 입력 복원
        Player.Instance.ignoreInput = false;

        // 카메라 줌 아웃 (기본 사이즈로 복원)
        StartCoroutine(SmoothZoomCamera(cameraFollow.defaultSize, cameraZoomDuration));

        // 카메라 설정 복원: 즉시 이동 모드로, 플레이어를 타겟
        cameraFollow.EnableStoryMode(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            cameraFollow.SetTarget(player);
        }

        isDialogueActive = false;
    }

}
