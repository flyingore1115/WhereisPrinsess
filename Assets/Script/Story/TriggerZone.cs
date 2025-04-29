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

        // 다시는 이 존에서 안 불리도록 꺼 버린다
        col.enabled = false;
        // 선택 사항: 스크립트 자체를 꺼버리고 싶다면
        // this.enabled = false;
        // 또는 완전히 삭제하려면
        // Destroy(this);
    }
}
