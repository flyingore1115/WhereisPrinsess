using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TriggerZone : MonoBehaviour
{
    [Tooltip("StorySceneManager가 인식할 트리거ID")]
    public string triggerID;

    StorySceneManager manager;
    Collider2D col;

    void Awake()
    {
        manager = FindFirstObjectByType<StorySceneManager>();
        col = GetComponent<Collider2D>();
        col.isTrigger = true; // 혹시 빠져 있다면 켜두기
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || manager == null) return;

        // 대사 시작
        manager.StartDialogueForTrigger(triggerID);

        if (Player.Instance != null)
   {
       Player.Instance.ignoreInput = true;
       // P_Movement.ResetInput 을 호출하면 속도와 대시 상태를 초기화합니다
       Player.Instance.movement.ResetInput();
   }

        // 다시는 이 존에서 안 불리도록 꺼 버린다
        col.enabled = false;
    }
}
