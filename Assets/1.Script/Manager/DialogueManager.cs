using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public Image playerSprite;
    public Image npcSprite;
    public Text dialogueText;

    [Header("Dialogue Settings")]
    public string[] dialogues; // 대사 리스트
    public bool[] isPlayerSpeaking; // 각 대사의 화자가 플레이어인지 여부
    public float typingSpeed = 0.05f; // 글자 출력 속도

    private int currentDialogueIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false; // 현재 타이핑 중인지 여부

    void Start()
    {
        ShowDialogue();
    }

    void Update()
    {
        // 엔터 키 또는 마우스 클릭 감지
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 타이핑 중이면 즉시 대사 출력 완료
                CompleteTyping();
            }
            else
            {
                // 다음 대사 출력
                ShowDialogue();
            }
        }
    }

    public void ShowDialogue()
    {
        if (currentDialogueIndex >= dialogues.Length)
        {
            Debug.Log("대화가 끝났습니다.");
            return;
        }

        // 대화 텍스트 초기화
        dialogueText.text = "";

        // 화자에 따라 스프라이트 밝기 조정
        if (isPlayerSpeaking[currentDialogueIndex])
        {
            HighlightCharacter(playerSprite, npcSprite);
        }
        else
        {
            HighlightCharacter(npcSprite, playerSprite);
        }

        // 기존 코루틴이 실행 중이라면 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 새로운 대사 출력 시작
        typingCoroutine = StartCoroutine(TypeDialogue(dialogues[currentDialogueIndex]));
        currentDialogueIndex++;
    }

    IEnumerator TypeDialogue(string dialogue)
    {
        isTyping = true;

        foreach (char letter in dialogue.ToCharArray())
        {
            dialogueText.text += letter; // 한 글자씩 추가
            yield return new WaitForSeconds(typingSpeed); // 출력 속도만큼 대기
        }

        isTyping = false; // 타이핑 완료
    }

    void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 현재 대사를 한 번에 모두 출력
        dialogueText.text = dialogues[currentDialogueIndex - 1];
        isTyping = false;
    }

    void HighlightCharacter(Image activeSprite, Image inactiveSprite)
    {
        // 선택된 캐릭터 밝게
        activeSprite.color = new Color(1f, 1f, 1f, 1f); // 기본 색상
        // 선택되지 않은 캐릭터 어둡게
        inactiveSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f); // 어두운 색상
    }
}
