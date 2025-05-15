using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StoryCanvasManager : MonoBehaviour
{
    public static StoryCanvasManager Instance { get; private set; }

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
        // 스토리 씬일 때만 활성화, 아니면 비활성화
        bool isStory = scene.name.Contains("Story");
        gameObject.SetActive(isStory);
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
