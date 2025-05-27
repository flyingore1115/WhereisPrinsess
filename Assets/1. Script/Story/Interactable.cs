using UnityEngine;

public class Interactable : MonoBehaviour
{
    InteractionUIController ui;
    public string actionDescription = "말 걸기"; // 행동 설명
    public string triggerID = "Default";        // 트리거 ID

    private bool isPlayerInside = false;

    void Awake()
    {
        ui = GetComponent<InteractionUIController>();
        if (ui == null)
            Debug.LogError("InteractionUIController 컴포넌트가 없습니다!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = true;
        if (!Player.Instance.ignoreInput && !StorySceneManager.Instance.IsDialogueActive)
            ui.Show("E", actionDescription);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = false;
        ui.Hide();
    }

    void Update()
    {
        if (!isPlayerInside) return;
        if (Player.Instance.ignoreInput) return;
        if (StorySceneManager.Instance != null && StorySceneManager.Instance.IsDialogueActive) return;

        var lady = FindFirstObjectByType<Lady>();
        if (lady != null && lady.mode == Lady.LadyMode.HallwayAutoRun) return;

        if (Input.GetKeyDown(KeyCode.E))
            Interact();
    }

    public void Interact()
    {
        Debug.Log($"[Interactable] {triggerID}와 상호작용 발생!");
        var manager = Object.FindFirstObjectByType<StorySceneManager>();
        if (manager != null)
            manager.StartDialogueForTrigger(triggerID);
        else
            Debug.LogWarning("[Interactable] StorySceneManager가 없습니다!");
    }
}
