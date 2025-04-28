using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public string triggerID; // Inspector에 A, Intro 등 넣어두세요.

    private StorySceneManager manager;

    void Awake()
    {
        manager = FindFirstObjectByType<StorySceneManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && manager != null)
            manager.StartDialogueForTrigger(triggerID);
    }
}
