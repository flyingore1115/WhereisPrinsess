using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Tooltip("StorySceneManager가 인식할 트리거ID (예: Trigger_A, Trigger_B 등)")]
    public string triggerID;

    // 상호작용이 발생하면 실행되는 메서드
    public void Interact()
    {
        Debug.Log($"[Interactable] {triggerID}와 상호작용 발생!");

        // 씬에 있는 StorySceneManager를 찾는다 (싱글톤이면 싱글톤 참조하는 방법도 OK)
        StorySceneManager manager = Object.FindFirstObjectByType<StorySceneManager>();

        if (manager != null)
        {
            manager.StartDialogueForTrigger(triggerID);
        }
        else
        {
            Debug.LogWarning("[Interactable] StorySceneManager가 씬에 없습니다!");
        }

        // 필요 시, 대사 외 다른 로직(문 열기, 아이템 획득 등)도 여기에 작성 가능
    }
}
