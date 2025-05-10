using UnityEngine;


[RequireComponent(typeof(InteractionIcon))]

public class Interactable : MonoBehaviour
{
    InteractionIcon icon;
    bool isPlayerInside = false;
    public string triggerID;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = true;
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
