using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

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
    public Image cgImage;
    public float cgFadeDuration = 0.5f;

    [Header("Tutorial UI References")]
    public Text tutorialText;

    [Header("Typing Effect Settings")]
    public float typingSpeed = 0.05f;

    [Header("Camera Settings for Dialogue")]
    public CameraFollow cameraFollow;
    public float dialogueCameraSize = 8f;
    public float cameraZoomDuration = 0.5f;

    [Header("Skip/AutoPlay Settings")]
    public bool skipEnabled = false;
    public bool autoPlayEnabled = false;
    public float autoPlayBaseDelay = 0.5f;
    public float autoPlayCharDelay = 0.05f;
    public Button skipButton;
    public Button autoPlayButton;

    [Header("Lady Tutorial")]
    public Lady lady;

    [Header("Panel Slide Settings")]
    public RectTransform dialogueContainer;
    public float panelSlideDuration = 0.5f;

    private Vector2 panelVisiblePos;
    private Vector2 panelHiddenPos;

    // 데이터 분리용 맵
    private Dictionary<string, DialogueEntryData[]> dataMap;
    // 액션 호출용 맵
    private Dictionary<string, Func<IEnumerator>> actionMap;

    private bool isDialogueActive = false;
    public bool IsDialogueActive => isDialogueActive;

    private Coroutine dialogueCoroutine;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 1) ScriptableObject로 분리된 대사 데이터 로드
        dataMap = new Dictionary<string, DialogueEntryData[]>();
        var sequenceAssets = Resources.LoadAll<DialogueSequenceSO>("Dialogue");
        foreach (var seq in sequenceAssets)
            dataMap[seq.triggerID] = seq.entries;

        // 2) 액션 매핑
        actionMap = new Dictionary<string, Func<IEnumerator>>()
        {
            { "AttackTutorial", AttackTutorial },
            { "LadyDoorSeq", LadyDoorSequence },
            { "HallwayTutorial", HallwayTutorial },
            
            // 향후 다른 액션 키 추가 가능
        };
        // 3) 패널 위치 초기화
        panelVisiblePos = dialogueContainer.anchoredPosition;
        panelHiddenPos = new Vector2(panelVisiblePos.x, -450f);
        dialogueContainer.anchoredPosition = panelHiddenPos;
    }

    void Start()
    {
        dialoguePanel?.SetActive(false);
        choicePanel?.SetActive(false);
        tutorialText.enabled = false;

        if (cgImage != null)
        {
            cgImage.gameObject.SetActive(true);  // 항상 켜진 상태 유지
            cgImage.sprite = Resources.Load<Sprite>("IMG/CG/fade"); // 검은 배경
            cgImage.canvasRenderer.SetAlpha(0f); // 완전히 검은 화면
        }


        if (!string.IsNullOrEmpty(startTriggerID))
            StartDialogueForTrigger(startTriggerID);
    }

    public void StartDialogueForTrigger(string triggerID)
    {
        if (isDialogueActive) return;
        if (!dataMap.TryGetValue(triggerID, out var entries)) return;

        isDialogueActive = true;
        Player.Instance.ignoreInput = true;
        dialoguePanel.SetActive(true);
        StartCoroutine(SmoothZoomCamera(dialogueCameraSize, cameraZoomDuration));
        dialogueCoroutine = StartCoroutine(PlayEntries(entries));
    }

    private IEnumerator PlayEntries(DialogueEntryData[] entries)
    {
        yield return SlideInPanel();

        foreach (var entry in entries)
        {
            switch (entry.type)
            {
                case EntryType.Dialogue:
                    yield return TypeSentence(entry.characterName, entry.dialogueText);

                    if (!string.IsNullOrEmpty(entry.actionKey))
                        yield return InvokeAction(entry.actionKey);

                    yield return WaitForNextKey(entry.dialogueText.Length);
                    break;

                case EntryType.Choice:
                    yield return ShowChoices(entry.options);
                    break;

                case EntryType.Action:
                    yield return InvokeAction(entry.actionKey);
                    break;
            }
        }

        EndDialogue();
    }

    private IEnumerator WaitForNextKey(int textLength)
    {
        if (skipEnabled)
            yield break;
        else if (autoPlayEnabled)
            yield return new WaitForSeconds(autoPlayBaseDelay + textLength * autoPlayCharDelay);
        else
        {
            yield return null;  // 이전 키 누름 소진
            yield return new WaitUntil(() => Input.anyKeyDown);
            yield return null;  // 다음 문장 분리
        }
    }

private IEnumerator InvokeAction(string key)
{
    if (key.StartsWith("ShowCG:"))
    {
        string fileName = key.Substring("ShowCG:".Length);
        yield return ShowCG(fileName);
    }
    else if (key == "HideCG")
    {
        yield return HideCG();
    }
     else if (key.StartsWith("FocusCameraOnTarget:"))
    {
        // 예: FocusCameraOnTarget:Lady:1.0
        string[] parts = key.Split(':');
        if (parts.Length >= 3)
        {
            string targetName = parts[1];
            if (float.TryParse(parts[2], out float waitTime))
            {
                yield return FocusCameraOnTarget(targetName, waitTime);
            }
            else
            {
                Debug.LogWarning($"[StorySceneManager] FocusCameraOnTarget 시간 파싱 실패: {key}");
            }
        }
        else
        {
            Debug.LogWarning($"[StorySceneManager] FocusCameraOnTarget 포맷 오류: {key}");
        }
    }
    else if (actionMap.TryGetValue(key, out var act))
    {
        yield return StartCoroutine(act());
    }
    else
    {
        Debug.LogWarning($"[StorySceneManager] 알 수 없는 actionKey: {key}");
    }
}



    private IEnumerator TypeSentence(string charName, string text)
    {
        characterNameText.text = charName;
        dialogueText.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            if (Input.anyKeyDown)
            {
                dialogueText.text = text;
                yield break;
            }

            dialogueText.text += text[i];
            SoundManager.Instance.PlaySFX(string.IsNullOrEmpty(charName) ? "N_beep" : "beep");

            float timer = 0f;
            while (timer < typingSpeed)
            {
                if (Input.anyKeyDown)
                {
                    dialogueText.text = text;
                    yield break;
                }
                timer += Time.deltaTime;
                yield return null;
            }
        }

        dialogueText.text = text;
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
            btn.onClick.AddListener(() => selected = idx);
            buttons.Add(btn);
        }

        yield return new WaitUntil(() => selected >= 0);
        buttons.ForEach(b => Destroy(b.gameObject));
        choicePanel.SetActive(false);
    }

    // ────────────────────────────────────────────────────────────
    // AttackTutorial 코루틴 (시간 멈춤 이전엔 클릭 비활성, 이후 활성)// StorySceneManager.cs 에서
private IEnumerator AttackTutorial()
{
    // 0) 대화판 숨기고 플레이어 이동·공격 모두 잠시 봉인
    dialoguePanel.SetActive(false);
    Player.Instance.ignoreInput = true;
    Player.Instance.attack.enabled = false;
    tutorialText.enabled = true;

    // 1) 케이크 던지기
    lady.StartThrowing();
    yield return new WaitForSeconds(0.3f);

    // 2) 시간 정지 유도
    tutorialText.text = "스페이스바를 눌러 시간을 멈춰보세요.";
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
    TimeStopController.Instance.StopTime();

    // 3) 공격만 활성화
    Player.Instance.attack.enabled = true;
    tutorialText.text = "던진 케이크를 모두 클릭하세요.";
    yield return new WaitUntil(() =>
        Player.Instance.attack.SelectedCount >= lady.spawned.Count);

    // 4) 시간 해제
    tutorialText.text = "스페이스바를 눌러 시간을 해제하세요.";
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
    TimeStopController.Instance.ResumeTime();

    // 5) 플레이어 착지 대기
    var pm = Player.Instance.GetComponent<P_Movement>();
    if (pm != null)
        yield return new WaitUntil(() => pm.IsGrounded);

    // 6) 공격 비활성화, 입력 해제, 튜토리얼 텍스트 끄기
    Player.Instance.attack.enabled = false;
    Player.Instance.ignoreInput  = false;
    tutorialText.enabled = false;

    // 7) 이제 대화창 켜면 곧바로 다음 대사 TypeSentence 가 실행
    dialoguePanel.SetActive(true);
    yield return null;      // 한 프레임 기다려 텍스트 셋팅
}

    // ────────────────────────────────────────────────────────────

    // ① AttackTutorial 끝에서 호출
private IEnumerator LadyDoorSequence()
{
    Debug.Log("[LadyDoorSeq] 시작");
    yield return StartCoroutine(lady.MoveToDoor());     // 문까지 걷기

    // 페이드 없이 바로 복도 스폰
    lady.TeleportToHallway();
    Debug.Log("[LadyDoorSeq] 복도 스폰 완료");

    StartCoroutine(HallwayTutorial());
}


    private IEnumerator HallwayTutorial()
    {
        // LadyAutoRunner는 자동 시작
        lady.GetComponent<Lady>().ResumeAutoRun();

        // 여기는 TutorialCheckpoint가 종료 조건을 대신 감시
        // 종료 시 DialogueEntryData triggerID "LadyAfterRun" 실행
        yield break;
    }


    public void ShowTutorialMessage(string message)
    {
        if (tutorialText != null)
            tutorialText.text = message;
    }

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
        cgImage.CrossFadeAlpha(1f, cgFadeDuration, false);
        yield return new WaitForSeconds(cgFadeDuration);
    }


    private IEnumerator HideCG()
    {
        if (cgImage == null) yield break;

        cgImage.CrossFadeAlpha(0f, cgFadeDuration, false);
        yield return new WaitForSeconds(cgFadeDuration);
        cgImage.sprite = null; // CG 비움
    }


    private IEnumerator FocusCameraOnTarget(string targetName, float waitTime)
    {
        var target = GameObject.Find(targetName);
        if (target == null) yield break;

        cameraFollow.EnableStoryMode(true);
        cameraFollow.SetTarget(target);
        yield return new WaitForSeconds(waitTime);
    }

    private IEnumerator PlayCharacterAnimation(string characterName, string triggerName)
    {
        var character = GameObject.Find(characterName);
        if (character == null) yield break;

        var animator = character.GetComponent<Animator>();
        if (animator == null) yield break;

        animator.SetTrigger(triggerName);
        yield return new WaitForSeconds(0.5f);
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
        StartCoroutine(EndDialogueRoutine());
    }

    private IEnumerator EndDialogueRoutine()
    {
        yield return SlideOutPanel();
        dialoguePanel.SetActive(false);

        Player.Instance.ignoreInput = false;
        StartCoroutine(SmoothZoomCamera(cameraFollow.defaultSize, cameraZoomDuration));
        cameraFollow.EnableStoryMode(false);
        cameraFollow.SetTarget(GameObject.FindGameObjectWithTag("Player"));

        isDialogueActive = false;
        skipEnabled = false;
        autoPlayEnabled = false;
    }

    private IEnumerator SlideInPanel()
    {
        dialogueText.text = "";
        characterNameText.text = "";

        float t = 0f;
        while (t < panelSlideDuration)
        {
            t += Time.deltaTime;
            dialogueContainer.anchoredPosition =
                Vector2.Lerp(panelHiddenPos, panelVisiblePos, t / panelSlideDuration);
            yield return null;
        }
        dialogueContainer.anchoredPosition = panelVisiblePos;
    }

    private IEnumerator SlideOutPanel()
    {
        float t = 0f;
        while (t < panelSlideDuration)
        {
            t += Time.deltaTime;
            dialogueContainer.anchoredPosition =
                Vector2.Lerp(panelVisiblePos, panelHiddenPos, t / panelSlideDuration);
            yield return null;
        }
        dialogueContainer.anchoredPosition = panelHiddenPos;
    }

    private void SetButtonAlpha(Button btn, float alpha)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null)
        {
            var c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
}
