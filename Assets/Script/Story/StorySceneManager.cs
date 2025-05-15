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

    [Header("CG Display")]
    public float cgFadeDuration = 0.5f;

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

    [Header("Lady Tutorial")]
    public Lady lady;

    [Header("Panel Slide Settings")]
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
            { "LadyDoorSeq",     LadyDoorSequence },
            { "HallwayTutorial", HallwayTutorial },
            { "ShootingTutorial", ShootingTutorial},
            { "LoadNextScene", LoadNextScene }
            // ...추가 액션
        };

        if (StoryCanvasManager.Instance != null && StoryCanvasManager.Instance.DialogueContainer != null)
        {
            panelVisiblePos = StoryCanvasManager.Instance.DialogueContainer.anchoredPosition;
            panelHiddenPos  = new Vector2(panelVisiblePos.x, -450f);
            StoryCanvasManager.Instance.DialogueContainer.anchoredPosition = panelHiddenPos;
        }
        else
        {
            Debug.Log("[SSM] Awake: CanvasManager 아직 준비 안됨, 위치 초기화는 Start에서 처리하세요.");
        }

        TimeStopController.Instance.SetInputBlocked(true);
    }

    void Start()
    {
        
        StoryCanvasManager.Instance.DialoguePanel?.SetActive(false);
        StoryCanvasManager.Instance.ChoicePanel?.SetActive(false);
        StoryCanvasManager.Instance.TutorialText.enabled = false;

        // CG 초기화
        if (StoryCanvasManager.Instance.CGImage != null)
        {
            StoryCanvasManager.Instance.CGImage.gameObject.SetActive(true);
            StoryCanvasManager.Instance.CGImage.sprite = Resources.Load<Sprite>("IMG/CG/fade");
            StoryCanvasManager.Instance.CGImage.canvasRenderer.SetAlpha(0f);
        }

        // UI 버튼 이벤트
        if (StoryCanvasManager.Instance.SkipButton != null)
            StoryCanvasManager.Instance.SkipButton.onClick.AddListener(ToggleSkip);
        if (StoryCanvasManager.Instance.AutoPlayButton != null)
            StoryCanvasManager.Instance.AutoPlayButton.onClick.AddListener(ToggleAutoPlay);

        // 버튼 초기 알파
        SetButtonAlpha(StoryCanvasManager.Instance.SkipButton, 0f);
        SetButtonAlpha(StoryCanvasManager.Instance.AutoPlayButton, 0f);

        if (!string.IsNullOrEmpty(startTriggerID))
            StartDialogueForTrigger(startTriggerID);


    }

    public void StartDialogueForTrigger(string triggerID)
    {
        if (isDialogueActive) return;
        if (!dataMap.TryGetValue(triggerID, out var entries)) return;

        isDialogueActive = true;
        Player.Instance.ignoreInput = true;
        StoryCanvasManager.Instance.DialoguePanel.SetActive(true);
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
        {
            yield break; // 즉시 다음
        }
        else if (autoPlayEnabled)
        {
            // 이전보다 빠르게 넘기도록 수정 (기존보다 약 50% 빠름)
            float delay = autoPlayBaseDelay + textLength * autoPlayCharDelay * 0.5f;
            yield return new WaitForSeconds(delay);
        }
        else
        {
            yield return null;  // 이전 키 입력 소진
            yield return new WaitUntil(() => Input.anyKeyDown);
            yield return null;
        }
    }


    private IEnumerator InvokeAction(string key)
    {
        if (key.StartsWith("ShowCG:"))
        {
            yield return ShowCG(key.Substring(7));
        }
        else if (key == "HideCG")
        {
            yield return HideCG();
        }
        else if (key.StartsWith("FocusCameraOnTarget:"))
        {
            var parts = key.Split(':');
            if (parts.Length >= 3 && float.TryParse(parts[2], out float t))
                yield return FocusCameraOnTarget(parts[1], t);
            else
                Debug.LogWarning($"[SSM] FocusCameraOnTarget 포맷 오류: {key}");
        }
        else if (actionMap.TryGetValue(key, out var act))
        {
            yield return StartCoroutine(act());
        }
        else
        {
            Debug.LogWarning($"[SSM] 알 수 없는 actionKey: {key}");
        }
    }

    private IEnumerator TypeSentence(string charName, string text)
    {
        // 스킵 모드일 경우 즉시 출력
        if (skipEnabled)
        {
            StoryCanvasManager.Instance.CharacterNameText.text = charName;
            StoryCanvasManager.Instance.DialogueText.text = text;
            yield break;
        }

        StoryCanvasManager.Instance.CharacterNameText.text = charName;
        StoryCanvasManager.Instance.DialogueText.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            if (Input.anyKeyDown)
            {
                StoryCanvasManager.Instance.DialogueText.text = text;
                yield break;
            }

            StoryCanvasManager.Instance.DialogueText.text += text[i];
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(string.IsNullOrEmpty(charName) ? "N_beep" : "beep");
            }
            float timer = 0f;
            while (timer < typingSpeed)
            {
                if (Input.anyKeyDown)
                {
                    StoryCanvasManager.Instance.DialogueText.text = text;
                    yield break;
                }
                timer += Time.deltaTime;
                yield return null;
            }
        }

        StoryCanvasManager.Instance.DialogueText.text = text;
    }

    private IEnumerator ShowChoices(string[] options)
    {
        StoryCanvasManager.Instance.ChoicePanel.SetActive(true);
        int selected = -1;
        var buttons = new List<Button>();

        for (int i = 0; i < options.Length; i++)
        {
            var btn = Instantiate(StoryCanvasManager.Instance.ChoiceButtonPrefab, StoryCanvasManager.Instance.ChoicePanel.transform);
            btn.GetComponentInChildren<Text>().text = options[i];
            int idx = i;
            btn.onClick.AddListener(() => selected = idx);
            buttons.Add(btn);
        }

        yield return new WaitUntil(() => selected >= 0);
        buttons.ForEach(b => Destroy(b.gameObject));
        StoryCanvasManager.Instance.ChoicePanel.SetActive(false);
    }

    // ─────────────────────────────────────────
    private IEnumerator AttackTutorial()
    {
        StoryCanvasManager.Instance.DialoguePanel.SetActive(false);
        Player.Instance.ignoreInput = true;
        Player.Instance.attack.enabled = false;
        StoryCanvasManager.Instance.TutorialText.enabled = true;

        lady.StartThrowing();
        yield return new WaitForSeconds(0.3f);

        StoryCanvasManager.Instance.TutorialText.text = "스페이스바를 눌러 시간을 멈춰보세요.";
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        TimeStopController.Instance.StopTime();

        Player.Instance.attack.enabled = true;
        StoryCanvasManager.Instance.TutorialText.text = "던진 케이크를 모두 클릭하세요.";
        yield return new WaitUntil(() =>
            lady.spawned.Count == 0);

        StoryCanvasManager.Instance.TutorialText.text = "스페이스바를 눌러 시간을 해제하세요.";
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        TimeStopController.Instance.ResumeTime();

        var pm = Player.Instance.GetComponent<P_Movement>();
        if (pm != null) yield return new WaitUntil(() => pm.IsGrounded);

        Player.Instance.attack.enabled = false;
        Player.Instance.ignoreInput = false;
        StoryCanvasManager.Instance.TutorialText.enabled = false;

        StoryCanvasManager.Instance.DialoguePanel.SetActive(true);
        yield return null;
    }

    private IEnumerator LadyDoorSequence()
    {
        yield return StartCoroutine(lady.MoveToDoor());
        lady.TeleportToHallway();
        lady.transform.position += Vector3.right * 5f;
    }

    public IEnumerator HallwayTutorial()
    {
        lady.ResumeAutoRun();
        TimeStopController.Instance.SetInputBlocked(false);
        yield return null;
    }

    public IEnumerator ShootingTutorial()
    {
        StoryCanvasManager.Instance.DialoguePanel.SetActive(false);
        StoryCanvasManager.Instance.TutorialText.enabled = true;
        Player.Instance.ignoreInput = true;

        // ① 적 스폰(혹은 미리 배치된 Enemy 찾기)
        BaseEnemy enemy = FindFirstObjectByType<BaseEnemy>();
        Transform target = enemy.transform;

        // ② 플레이어 총알 각도 제한 켜기
        Player.Instance.shooting.EnableAngleLimit(target, 15f);

        Player.Instance.ignoreInput = false;

        StoryCanvasManager.Instance.TutorialText.text = "Shift + 좌클릭으로 사격하세요!";
        // ③ 적이 죽을 때까지 대기
        yield return new WaitUntil(() => enemy == null || enemy.isDead);

        // ④ 제한 해제 및 마무리
        Player.Instance.shooting.DisableAngleLimit();
        StoryCanvasManager.Instance.TutorialText.enabled = false;
        Player.Instance.ignoreInput = false;
        StoryCanvasManager.Instance.DialoguePanel.SetActive(true);
    }

    // ─────────────────────────────────────────
    public void ShowTutorialMessage(string message)
    {
        StoryCanvasManager.Instance.TutorialText.text = message;
    }

    private IEnumerator ShowCG(string fileName)
    {
        if (StoryCanvasManager.Instance.CGImage == null) yield break;
        var sp = Resources.Load<Sprite>($"IMG/CG/{fileName}");
        if (sp == null)
        {
            Debug.LogWarning($"[SSM] CG 못 찾음: {fileName}");
            yield break;
        }
        StoryCanvasManager.Instance.CGImage.sprite = sp;
        StoryCanvasManager.Instance.CGImage.CrossFadeAlpha(1f, cgFadeDuration, false);
        yield return new WaitForSeconds(cgFadeDuration);
    }

    private IEnumerator HideCG()
    {
        if (StoryCanvasManager.Instance.CGImage == null) yield break;
        StoryCanvasManager.Instance.CGImage.CrossFadeAlpha(0f, cgFadeDuration, false);
        yield return new WaitForSeconds(cgFadeDuration);
        StoryCanvasManager.Instance.CGImage.sprite = null;
    }

    private IEnumerator FocusCameraOnTarget(string targetName, float waitTime)
    {
        var target = GameObject.Find(targetName);
        if (target == null) yield break;
        cameraFollow.EnableStoryMode(true);
        cameraFollow.SetTarget(target);
        yield return new WaitForSeconds(waitTime);
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
        StoryCanvasManager.Instance.DialoguePanel.SetActive(false);

        Player.Instance.ignoreInput = false;
        StartCoroutine(SmoothZoomCamera(cameraFollow.defaultSize, cameraZoomDuration));
        cameraFollow.EnableStoryMode(false);
        cameraFollow.SetTarget(GameObject.FindGameObjectWithTag("Player"));

        isDialogueActive = false;
        skipEnabled = false;
        autoPlayEnabled = false;
        SetButtonAlpha(StoryCanvasManager.Instance.SkipButton, 0f);
        SetButtonAlpha(StoryCanvasManager.Instance.AutoPlayButton, 0f);
    }

    private IEnumerator SlideInPanel()
    {
        StoryCanvasManager.Instance.CharacterNameText.text = "";
        StoryCanvasManager.Instance.DialogueText.text = "";

        float t = 0f;
        while (t < panelSlideDuration)
        {
            t += Time.deltaTime;
            StoryCanvasManager.Instance.DialogueContainer.anchoredPosition =
                Vector2.Lerp(panelHiddenPos, panelVisiblePos, t / panelSlideDuration);
            yield return null;
        }
        StoryCanvasManager.Instance.DialogueContainer.anchoredPosition = panelVisiblePos;
    }

    private IEnumerator SlideOutPanel()
    {
        float t = 0f;
        while (t < panelSlideDuration)
        {
            t += Time.deltaTime;
            StoryCanvasManager.Instance.DialogueContainer.anchoredPosition =
                Vector2.Lerp(panelVisiblePos, panelHiddenPos, t / panelSlideDuration);
            yield return null;
        }
        StoryCanvasManager.Instance.DialogueContainer.anchoredPosition = panelHiddenPos;
    }

    public void ToggleSkip()
    {

        skipEnabled = !skipEnabled;
        SetButtonAlpha(StoryCanvasManager.Instance.SkipButton, skipEnabled ? 0.4f : 0f);
        Debug.Log($"Skip 모드: {skipEnabled}");
    }

    public void ToggleAutoPlay()
    {
        autoPlayEnabled = !autoPlayEnabled;
        SetButtonAlpha(StoryCanvasManager.Instance.AutoPlayButton, autoPlayEnabled ? 0.4f : 0f);
        Debug.Log($"AutoPlay 모드: {autoPlayEnabled}");
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
    private IEnumerator LoadNextScene()
{
    yield return new WaitForSeconds(0.3f); // 연출 텀
    MySceneManager.Instance.LoadNextScene();
}
}
