using UnityEngine;

public class Interactable : MonoBehaviour
{
    InteractionUIController ui;
    public string actionDescription = "말 걸기";
    public string triggerID = "Default";

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
        

        // 아가씨(Lady)가 HallwayAutoRun 모드인지 체크
        var lady = FindFirstObjectByType<Lady>();
        if (lady != null && lady.mode == Lady.LadyMode.HallwayAutoRun)
        {
            // 복도 자동 달리기 중이니 아이콘을 띄우지 않는다
            isPlayerInside = false;
            Debug.Log("자동달리기중!!!! 체크함!!!");
            return;
        }

        isPlayerInside = true;

        // 대화가 진행 중이거나 플레이어 ignoreInput 상태가 아니어야 아이콘 표시
        if (!Player.Instance.ignoreInput && !StorySceneManager.Instance.IsDialogueActive)
        {
            ui.Show("E", actionDescription);
        }
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

        // 자동 달리기 중이라면: 아이콘이 떠 있더라도 즉시 숨기고
        // 상호작용 자체를 차단
        var lady = FindFirstObjectByType<Lady>();
        if (lady != null && lady.mode == Lady.LadyMode.HallwayAutoRun)
        {
            ui.Hide();
            Debug.Log("업데이트에서도 체크함!!");
            return;
        }

        // E 키 입력 시에만 실제 상호작용(대화) 실행
        if (Input.GetKeyDown(KeyCode.E))
            Interact();
    }

    public void Interact()
    {
        // ← 여기에 무조건 모드 체크를 한 번 더!
    var lady = FindFirstObjectByType<Lady>();
    if (lady != null && lady.mode == Lady.LadyMode.HallwayAutoRun)
    {
        // 자동 달리기 중에는 절대 대사 시작 금지
        Debug.Log("[Interactable] HallwayAutoRun 중이므로 상호작용 무시");
        return;
    }
    
        Debug.Log($"[Interactable] {triggerID}와 상호작용 발생!");
        var manager = Object.FindFirstObjectByType<StorySceneManager>();
        if (manager != null)
            manager.StartDialogueForTrigger(triggerID);
        else
            Debug.LogWarning("[Interactable] StorySceneManager가 없습니다!");
    }
}
