using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

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

    [Header("튜토리얼 완료 상태")]
public bool isAttackTutorialComplete = false;


    //손잡기 튜토용

    private bool isInsideGrabPoint = false;
    public void SetInsideGrabPoint(bool inside) => isInsideGrabPoint = inside;
    public bool IsInsideGrabPoint => isInsideGrabPoint;

    // 데이터 분리용 맵
    private Dictionary<string, DialogueEntryData[]> dataMap;
    // 액션 호출용 맵
    private Dictionary<string, Func<IEnumerator>> actionMap;

    private bool isDialogueActive = false;

    public bool IsDialogueActive => isDialogueActive;

    private Coroutine dialogueCoroutine;

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

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
            { "GrabTutorial",   GrabTutorial },
            { "LoadNextScene", LoadNextScene }
            // ...추가 액션
        };

        if (StoryCanvasManager.Instance != null && StoryCanvasManager.Instance.DialogueContainer != null)
        {
            panelVisiblePos = StoryCanvasManager.Instance.DialogueContainer.anchoredPosition;
            panelHiddenPos = new Vector2(panelVisiblePos.x, -450f);
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

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene,
                              UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 1) 캔버스 위치 재초기화
        if (StoryCanvasManager.Instance != null &&
            StoryCanvasManager.Instance.DialogueContainer != null)
        {
            StoryCanvasManager.Instance.DialogueContainer.anchoredPosition = panelHiddenPos;
        }

        // 2) 씬 내 IntroDialogueMarker 찾기
        IntroDialogueMarker marker = FindFirstObjectByType<IntroDialogueMarker>();
        if (marker != null && !string.IsNullOrEmpty(marker.triggerID))
        {
            // 만약 대화가 진행중이면 먼저 종료
            if (isDialogueActive) StopAllCoroutines();
            isDialogueActive = false;

            StartDialogueForTrigger(marker.triggerID);
        }
    }

    private IEnumerator BeginIntroNextFrame(string triggerID)
    {
        yield return null;             // 한 프레임 대기
        StartDialogueForTrigger(triggerID);
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
    private IEnumerator AttackTutorial() //공격
    {
        StoryCanvasManager.Instance.DialoguePanel.SetActive(false);
        Player.Instance.ignoreInput = true;
        Player.Instance.attack.enabled = false;
        StoryCanvasManager.Instance.TutorialText.enabled = true;

        cameraFollow.EnableStoryMode(false);
        cameraFollow.SetCameraSize(cameraFollow.defaultSize);
        cameraFollow.SetTarget(Player.Instance.gameObject);

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

        isAttackTutorialComplete = true;

        StoryCanvasManager.Instance.DialoguePanel.SetActive(true);
        yield return null;
    }

    private IEnumerator LadyDoorSequence()
    {
        yield return StartCoroutine(lady.MoveToDoor());
        lady.TeleportToHallway();
        lady.transform.position += Vector3.right * 5f;
    }

    public IEnumerator HallwayTutorial()//복도튜토
    {
        // 메인캔버스 전체 끄기
        CanvasManager.Instance.SetGameUIActive(false);
        // 타임 게이지만 켜기
        CanvasManager.Instance.timeStopSlider.gameObject.SetActive(true);
        lady.ResumeAutoRun();
        TimeStopController.Instance.SetInputBlocked(false);

        yield return null;

        //lady.cs에서 스탑태그 닿으면 캔버스 끄게함
    }

    private IEnumerator GrabTutorial()//잡기튜토
    {
        //다음이 사격이니까 굳이 끄지 않음

        //메인캔버스 끄고 타임게이지랑 총알만
        CanvasManager.Instance.SetGameUIActive(false);

        CanvasManager.Instance.timeStopSlider.gameObject.SetActive(true);
        CanvasManager.Instance.bulletUI.SetActive(true);


        // Grab 튜토 시작 직후: 스토리 모드 해제 → 인게임 모드로 전환
        cameraFollow.EnableStoryMode(false);
        cameraFollow.SetCameraSize(cameraFollow.defaultSize);
        cameraFollow.SetTarget(Player.Instance.gameObject);
        // (optional) 즉시 따라오도록
        cameraFollow.immediateFollowInGame = true;


        StoryCanvasManager.Instance.DialoguePanel.SetActive(false);
        StoryCanvasManager.Instance.TutorialText.enabled = true;

        // 0) 세팅
        Player pl = Player.Instance;
        pl.ignoreInput = true;
        pl.attack.enabled = false;
        pl.shooting.enabled = false;

        // 1) 적 투척(연출용) + 시간정지
        //나중에 애니
        yield return new WaitForSeconds(0.5f);
        StoryCanvasManager.Instance.TutorialText.text = "스페이스바로 시간을 멈추세요!";
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        TimeStopController.Instance.StopTime();

        // 2) 공주 선택
        StoryCanvasManager.Instance.TutorialText.text = "Ctrl + 좌클릭으로 공주를 잡으세요.";
        pl.ignoreInput = false;                   // Ctrl 입력 허용
        yield return new WaitUntil(() => pl.holdingPrincess);

        // 3) 지정 위치로 이동
        Transform safe = GameObject.FindWithTag("GrabPoint").transform;
        StoryCanvasManager.Instance.TutorialText.text = "공주를 끌고 표시된 위치로 이동하세요.";
        yield return new WaitUntil(() =>
            Vector2.Distance(Princess.Instance.transform.position, safe.position) < 0.3f);

        // 4) 시간 해제 안내
        //StoryCanvasManager.Instance.TutorialText.text = "스페이스바로 시간을 해제하세요!";
        bool resumed = false;
        while (!resumed)
        {
            // GrabPoint 안에 있을 때만 해제 가능
            if (StorySceneManager.Instance.IsInsideGrabPoint &&
                Input.GetKeyDown(KeyCode.Space))
            {
                TimeStopController.Instance.ResumeTime();
                resumed = true;
            }
            yield return null;
        }

        // 5) 마무리
        StoryCanvasManager.Instance.TutorialText.enabled = false;
        pl.attack.enabled = true;
        pl.shooting.enabled = true;
        StoryCanvasManager.Instance.DialoguePanel.SetActive(true);
    }


    // ─────────────────────────────────────────

    public IEnumerator ShootingTutorial() //사격튜토 
    {
        StoryCanvasManager.Instance.DialoguePanel.SetActive(false);
        StoryCanvasManager.Instance.TutorialText.enabled = true;



        Player pl = Player.Instance;
        pl.ignoreInput = true;

        BaseEnemy enemy = FindFirstObjectByType<BaseEnemy>();
        Transform target = enemy.transform;

        // 0) 재장전부터 강제
        pl.shooting.SetTutorialMode(true);                      // 입력 해제 방지
        pl.shooting.currentAmmo = 0;
        pl.shooting.UpdateAmmoUI();

        //일반공격 X
        Player.Instance.attack.enabled = false;
        StoryCanvasManager.Instance.TutorialText.text = "우클릭으로 재장전하세요!";
        pl.ignoreInput = true;

        // 튜토리얼 코드에서만 재장전 처리
        while (pl.shooting.currentAmmo < pl.shooting.maxAmmo)
        {
            if (Input.GetMouseButtonDown(1))
                pl.shooting.HandleShooting(); // 재장전
            yield return null;
        }

        // 1) 각도 제한 켜고 사격 유도
        pl.shooting.EnableAngleLimit(target, 15f);
        StoryCanvasManager.Instance.TutorialText.text = "적을 향해 사격하세요!";


        while (enemy != null && !enemy.isDead)
        {
            if (Input.GetMouseButtonDown(0))
                pl.shooting.HandleShooting(); // 사격
            yield return null;
        }

        // 2) 해제 및 종료
        pl.shooting.DisableAngleLimit();

        pl.shooting.SetTutorialMode(false);                     // 입력 제한 복구
        Player.Instance.attack.enabled = true;

        StoryCanvasManager.Instance.TutorialText.enabled = false;
        pl.ignoreInput = false;
        StoryCanvasManager.Instance.DialoguePanel.SetActive(true);

        //수동끄기
        CanvasManager.Instance.timeStopSlider.gameObject.SetActive(false);
        CanvasManager.Instance.bulletUI.SetActive(false);
        CanvasManager.Instance.SetGameUIActive(false);

    }


    // ─────────────────────────────
    
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
        // 1) 플레이용 Cinemachine 끄기
        CinemachineCamera vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null) vcam.enabled = false;

        // 2) CameraFollow 켜고 스토리 모드 활성
        cameraFollow.enabled = true;
        cameraFollow.EnableStoryMode(true);
        cameraFollow.SetTarget(target, dialogueCameraSize);
        yield return new WaitForSeconds(waitTime);
        // 3) 원상 복구
        cameraFollow.EnableStoryMode(false, keepSize: true);
        cameraFollow.enabled = false;
        if (vcam != null) vcam.enabled = true;
    }

    private IEnumerator SmoothZoomCamera(float targetSize, float duration)
    {
        if (cameraFollow == null) yield break;


        float start = cameraFollow.GetCurrentSize();
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float interpolatedSize = Mathf.Lerp(start, targetSize, t / duration);
            cameraFollow.SetCameraSize(interpolatedSize);
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
        if (cameraFollow == null)
        { cameraFollow = FindFirstObjectByType<CameraFollow>(); }
        StartCoroutine(SmoothZoomCamera(cameraFollow.defaultSize, cameraZoomDuration));
        cameraFollow.EnableStoryMode(false);
        cameraFollow.enabled = true; // ★ 꺼졌던 컴포넌트 다시 켜기
        cameraFollow.SetTarget(GameObject.FindGameObjectWithTag("Player"), null, true);
        if (!MySceneManager.IsStoryScene)
            CanvasManager.Instance?.SetGameUIActive(true);

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
