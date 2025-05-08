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

    public static StorySceneManager Instance { get; private set; }


    [Header("Scene Start Trigger ID")]
    public string startTriggerID;

    [Header("UI References")]
    public Text characterNameText;
    public Text dialogueText;
    public GameObject dialoguePanel;

    [Header("Choice UI References")]
    public GameObject choicePanel;
    public Button choiceButtonPrefab;

    [Header("CG Display")]
    // Resources/IMG/CG 폴더에서 불러올 스프라이트를 띄울 Image 컴포넌트
    public UnityEngine.UI.Image cgImage;
    public float cgFadeDuration = 0.5f;

    [Header("Tutorial UI References")]
    public GameObject tutorialPanel;
    public Text tutorialText;

    [Header("Typing Effect Settings")]
    public float typingSpeed = 0.05f;

    [Header("Camera Settings for Dialogue")]
    public CameraFollow cameraFollow;
    public float dialogueCameraSize = 8f;
    public float cameraZoomDuration = 0.5f;

    [Header("Skip/AutoPlay Settings")]
    public bool skipEnabled = false;         // 스킵 모드 on/off
    public bool autoPlayEnabled = false;     // 자동재생 모드 on/off
    public float autoPlayBaseDelay = 0.5f;   // 자동재생 기본 대기시간
    public float autoPlayCharDelay = 0.05f;  // 글자수당 추가 대기시간
    public Button skipButton;
    public Button autoPlayButton;

    [Header("Lady Tutorial")]
    public Lady lady;

    [Header("Panel Slide Settings")]
    public RectTransform dialogueContainer;    // 대화판넬(텍스트 포함)을 감싸는 빈 오브젝트
    public float panelSlideDuration = 0.5f;

    private Vector2 panelVisiblePos;           // 화면에 보일 때 Y
    private Vector2 panelHiddenPos;            // 화면 아래 숨길 때 Y

    private Dictionary<string, DialogueEntry[][]> dialogueSequences;
    private bool isDialogueActive = false;
    public bool IsDialogueActive => isDialogueActive;


    private Dictionary<string, int> dialogueTriggerCount;
    private Coroutine dialogueCoroutine;

    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지
        }
        else
        {
            Destroy(gameObject); // 중복 방지
            return;
        }

        dialogueTriggerCount = new Dictionary<string, int>();
        InitializeSequences();

        // 패널 위치 초기화
        panelVisiblePos = dialogueContainer.anchoredPosition;        // 보이는 위치는 현재 위치로
        panelHiddenPos = new Vector2(panelVisiblePos.x, -450f);      // 숨겨진 위치는 y = -450
        dialogueContainer.anchoredPosition = panelHiddenPos;
    }

    void Start()
    {
        dialoguePanel?.SetActive(false);
        choicePanel?.SetActive(false);
        tutorialPanel?.SetActive(false);

        if (!string.IsNullOrEmpty(startTriggerID))
            StartDialogueForTrigger(startTriggerID);

        if (cgImage != null)//처음에 검은화면뜨는건데 나중에 수정해야해
        {
            cgImage.sprite = Resources.Load<Sprite>("IMG/CG/fade");
            cgImage.canvasRenderer.SetAlpha(1f);
            cgImage.gameObject.SetActive(false);
        }
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
                        new DialogueEntry("???", "...일어날 수 있겠니?"),
                        new DialogueEntry(() => HideCG()),
                        new DialogueEntry("", "대충 일어남"),
                        new DialogueEntry("Mr. 스프라우트", "괜찮니? 영 상태가 좋아 보이지는 않는구나."),
                        new DialogueEntry("", "당신은 일어서 앞으로 걸어보려 시도했으나, 갓 태어난 기린처럼 휘청거렸다."),
                        new DialogueEntry("Mr. 스프라우트", "아하..."),
                        new DialogueEntry("Mr. 스프라우트", "걷기는 A나 D, 혹은 방향키로 할 수 있단다."),
                        new DialogueEntry("Mr. 스프라우트", "할 수 있겠니?"),
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
                        new DialogueEntry(() => HideCG()),
                        new DialogueEntry("Mr. 스프라우트","병실은 301호란다.")
                    }
                }
            },
            { "two", new DialogueEntry[][]
                {
                    new DialogueEntry[]
                    {
                        new DialogueEntry("???", "...?"),

                        new DialogueEntry(() => AttackTutorial()),

                        new DialogueEntry("소녀", "튜토리얼 종료"),

                        // 카메라를 아가씨로 이동
                        new DialogueEntry(() => FocusCameraOnTarget("Lady", 1.0f)),

                        new DialogueEntry("","안에 있는 것은 여자아이였다."),
                        new DialogueEntry("","오늘 생일을 맞은 듯, 생일 모자와 각종 선물을 침대 위에 두고 있는 어린 소녀 말이다."),
                        
                        
                        
                        // 카메라를 스프라우트에게로
                        new DialogueEntry(() => FocusCameraOnTarget("Ms_Sprout", 1.0f)),
                        new DialogueEntry("Ms. 스프라우트", "아..."),

                        
                        new DialogueEntry(() => FocusCameraOnTarget("Lady", 1.0f)),
                        new DialogueEntry("소녀", "...이건 뭐야?"),
                        new DialogueEntry("소녀", "꺼져!!!"),

                        new DialogueEntry(() => FocusCameraOnTarget("Player", 1.0f)),

                        // 아가씨 애니메이션 재생
                        new DialogueEntry(() => PlayCharacterAnimation("Lady", "imsi")), //임시

                        
                    }
                }
            },
            {
                "test", new DialogueEntry[][]
                {
                    new DialogueEntry[]
                    {
                        new DialogueEntry("Ms.스프라우트", "테스트테스트!!"),
                        new DialogueEntry(() => ShowCG("test")),    // CG 시작
                        new DialogueEntry("", "이 장면을 보아라."),
                        new DialogueEntry("", "이는 바로 CG!!!!"),
                        new DialogueEntry(() => HideCG()),                 // CG 종료
                        new DialogueEntry("아가씨", "아오..."),
                    }
                }
            }
        };
    }


    // 공격 튜토리얼 (시간 정지 -> 클릭 -> 시간 해제)
private IEnumerator AttackTutorial()
{
    // 대화창과 튜토리얼창 UI 전환
    dialoguePanel.SetActive(false);
    tutorialPanel.SetActive(true);
    tutorialText.enabled = true;  // 이 줄도 명시적으로 추가해도 좋음

    // 0) 튜토리얼 대상 던지기
    lady.StartThrowing(); // 추가됨

    // 1) 시간정지 안내 및 실행
    lady.StartThrowing();                 // ① 먼저 던지고
    yield return new WaitForSeconds(0.3f);// ② 살짝 날아가게 둔다

    tutorialText.text = "스페이스바를 눌러 시간을 멈춰보세요.";
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
    TimeStopController.Instance.StopTime();
    // 2) 클릭 안내

    tutorialText.text = "던진 케이크를 모두 클릭하세요.";
    yield return new WaitUntil(() =>                     // ★ 3개 클릭될 때까지 대기
        Player.Instance.attack.SelectedCount >= lady.spawned.Count);

    tutorialText.text = "스페이스바를 눌러 시간을 해제하세요.";
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));   // ★ 해제 입력
    TimeStopController.Instance.ResumeTime();

    // 4) 튜토리얼 종료 및 대화창 복귀
    tutorialPanel.SetActive(false);
    dialoguePanel.SetActive(true);
    yield return new WaitForSeconds(0.5f);
}



public void ShowTutorialMessage(string message)
{
    if (tutorialPanel != null && tutorialText != null)
    {
        tutorialPanel.SetActive(true);
        tutorialText.text = message;
    }
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

        int count = dialogueTriggerCount.ContainsKey(triggerID) ? dialogueTriggerCount[triggerID] : 0;
        int index = Mathf.Clamp(count, 0, sequences.Length - 1);
        DialogueEntry[] entries = sequences[index];
        dialogueTriggerCount[triggerID] = count + 1;

        isDialogueActive = true;
        Player.Instance.ignoreInput = true;
        dialoguePanel.SetActive(true);
        StartCoroutine(SmoothZoomCamera(dialogueCameraSize, cameraZoomDuration));
        StartCoroutine(StartDialogueSequence(entries)); // ▶ 여기를 통해 순차적으로 실행
    }
    private IEnumerator StartDialogueSequence(DialogueEntry[] entries)
    {
        yield return StartCoroutine(SlideInPanel());         // 1. 패널 완전히 올라오고
        yield return StartCoroutine(PlayEntries(entries));   // 2. 그다음 대사 시작
    }

    private IEnumerator PlayEntries(DialogueEntry[] entries)
    {
        yield return null; // UI 안정화용

        foreach (var entry in entries)
        {
            switch (entry.type)
            {
                case EntryType.Dialogue:
                    yield return StartCoroutine(TypeSentence(entry.characterName, entry.dialogueText));

                    if (skipEnabled)
                    {
                        // 아무 대기 없이 바로 다음으로
                    }
                    else if (autoPlayEnabled)
                    {
                        float delay = autoPlayBaseDelay + entry.dialogueText.Length * autoPlayCharDelay;
                        yield return new WaitForSeconds(delay);
                    }
                    else
                    {
                        yield return new WaitUntil(() => Input.anyKeyDown);
                    }
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

        for (int i = 0; i < text.Length; i++)
        {
            if (skipEnabled)
            {
                dialogueText.text = text;
                yield break;
            }

            dialogueText.text += text[i];
            // 이름이 공백일 때만 N_beep, 그 외는 일반 beep
            string sfx = string.IsNullOrEmpty(charName) ? "N_beep" : "beep";
            SoundManager.Instance.PlaySFX(sfx);

            yield return new WaitForSeconds(typingSpeed);
        }
    }



    // ● UI 버튼에서 OnClick으로 연결할 메서드
    public void ToggleSkip()
    {
        skipEnabled = !skipEnabled;
        SetButtonAlpha(skipButton, skipEnabled ? 0.4f : 0f);
        Debug.Log($"Skip 모드: {skipEnabled}");
    }

    public void ToggleAutoPlay()
    {
        autoPlayEnabled = !autoPlayEnabled;
        SetButtonAlpha(autoPlayButton, autoPlayEnabled ? 0.4f : 0f);
        Debug.Log($"AutoPlay 모드: {autoPlayEnabled}");
    }

    // Resources/IMG/CG/<fileName>.png에서 불러와 화면에 띄움
    private IEnumerator ShowCG(string fileName)
    {
        if (cgImage == null) yield break;

        var sprite = Resources.Load<Sprite>($"IMG/CG/{fileName}");
        if (sprite == null)
        {
            Debug.LogWarning($"CG 파일을 찾을 수 없습니다: {fileName}");
            yield break;
        }

        cgImage.sprite = sprite;
        cgImage.gameObject.SetActive(true);
        cgImage.canvasRenderer.SetAlpha(0f);
        cgImage.CrossFadeAlpha(1f, cgFadeDuration, false);
        yield return new WaitForSeconds(cgFadeDuration);
    }


    // CG 끄기
    private IEnumerator HideCG()
    {
        if (cgImage == null) yield break;

        cgImage.CrossFadeAlpha(0f, cgFadeDuration, false);
        yield return new WaitForSeconds(cgFadeDuration);
        cgImage.gameObject.SetActive(false);
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
        // 슬라이드 아웃 후 나머지 정리
        StartCoroutine(EndDialogueRoutine());
    }

    private IEnumerator EndDialogueRoutine()
    {
        // 1) 패널 아래로 슬라이드
        yield return StartCoroutine(SlideOutPanel());

        // 2) 패널 끔
        dialoguePanel.SetActive(false);

        // 3) 플레이어 입력 복원, 카메라 복원 등 기존 EndDialogue 로직
        Player.Instance.ignoreInput = false;
        StartCoroutine(SmoothZoomCamera(cameraFollow.defaultSize, cameraZoomDuration));
        cameraFollow.EnableStoryMode(false);
        cameraFollow.SetTarget(GameObject.FindGameObjectWithTag("Player"));
        isDialogueActive = false;

        // 4) 대화가 완전히 끝난 뒤 모드 초기화
        skipEnabled     = false;
        autoPlayEnabled = false;
    }

    //슬라이드 인
    private IEnumerator SlideInPanel()
    {
        dialogueText.text = "";
        characterNameText.text = "";

        float t = 0f;
        while (t < panelSlideDuration)
        {
            t += Time.deltaTime;
            dialogueContainer.anchoredPosition = Vector2.Lerp(panelHiddenPos, panelVisiblePos, t / panelSlideDuration);
            yield return null;
        }
        dialogueContainer.anchoredPosition = panelVisiblePos;
    }

    //슬라이드 아웃
    private IEnumerator SlideOutPanel()
    {
        float t = 0f;
        while (t < panelSlideDuration)
        {
            t += Time.deltaTime;
            dialogueContainer.anchoredPosition = Vector2.Lerp(panelVisiblePos, panelHiddenPos, t / panelSlideDuration);
            yield return null;
        }
        dialogueContainer.anchoredPosition = panelHiddenPos;
    }

    private void SetButtonAlpha(Button btn, float alpha)
    {
        var img = btn.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }


}
