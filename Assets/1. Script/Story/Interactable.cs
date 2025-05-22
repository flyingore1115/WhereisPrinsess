using UnityEngine;


[RequireComponent(typeof(InteractionIcon))]

public class Interactable : MonoBehaviour
{
    InteractionIcon icon;
    bool isPlayerInside = false;
    public string triggerID;

    void Awake()
    {
        icon = GetComponent<InteractionIcon>();
       if (icon == null)
           Debug.LogError($"[Interactable] InteractionIcon 컴포넌트를 찾을 수 없습니다: {name}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        // Lady가 달리는 중이면 인터랙트 차단
        var lady = FindFirstObjectByType<Lady>();
        if (lady != null && lady.mode == Lady.LadyMode.HallwayAutoRun)
       return;
        isPlayerInside = true;
        if (!Player.Instance.ignoreInput && !StorySceneManager.Instance.IsDialogueActive)
            icon.Show();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = false;
        icon.Hide();
    }

    void Update()
    {
        if (!isPlayerInside) return;                      // 범위 밖이면 무시
        if (Player.Instance.ignoreInput) return;          // 이동/대화 중이면 무시
        if (StorySceneManager.Instance != null && StorySceneManager.Instance.IsDialogueActive) return;
        // Lady 달리기 중이면 E키 무시
        var lady = FindFirstObjectByType<Lady>();
        if (lady != null && lady.mode == Lady.LadyMode.HallwayAutoRun) return;
        if (Input.GetKeyDown(KeyCode.E))
            Interact();
    }

    public void Interact()
    {
        Debug.Log($"[Interactable] {triggerID}와 상호작용 발생!");
        StorySceneManager manager = Object.FindFirstObjectByType<StorySceneManager>();
        if (manager != null)
            manager.StartDialogueForTrigger(triggerID);
        else
            Debug.LogWarning("[Interactable] StorySceneManager가 없습니다!");
    }
}
