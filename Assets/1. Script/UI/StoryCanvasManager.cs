using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StoryCanvasManager : MonoBehaviour
{
    public static StoryCanvasManager Instance { get; private set; }

    [SerializeField] private Canvas rootCanvas;          // ⬅️ 최상위 Canvas

    [Header("Dialogue UI")]
    [SerializeField] private RectTransform dialogueContainer;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Text Fields")]
    [SerializeField] private Text characterNameText;
    [SerializeField] private Text dialogueText;
    [SerializeField] private Text tutorialText;

    [Header("CG Image")]
    [SerializeField] private Image cgImage;

    [Header("Control Buttons")]
    [SerializeField] private Button skipButton;
    [SerializeField] private Button autoPlayButton;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isStory = scene.name.Contains("Story");

        // ✅ 오브젝트는 그대로 두고, Canvas 만 on/off
        if (rootCanvas != null)
            rootCanvas.enabled = isStory;

        // 필요하면 세부 패널도 함께 조정
        if (!isStory)
        {
            dialoguePanel.SetActive(false);
            choicePanel.SetActive(false);
            if (cgImage != null) cgImage.enabled = false;
        }
    }

    // 공개 프로퍼티들 (필요하면 추가)
    public RectTransform DialogueContainer => dialogueContainer;
    public GameObject DialoguePanel => dialoguePanel;
    public GameObject ChoicePanel => choicePanel;
    public Button ChoiceButtonPrefab => choiceButtonPrefab;
    public Text CharacterNameText => characterNameText;
    public Text DialogueText => dialogueText;
    public Text TutorialText => tutorialText;
    public Image CGImage => cgImage;
    public Button SkipButton => skipButton;
    public Button AutoPlayButton => autoPlayButton;
}
