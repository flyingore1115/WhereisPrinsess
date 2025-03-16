using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class StorySceneManager : MonoBehaviour
{
    [Header("UI References")]
    public Text characterNameText;  // 캐릭터 이름 표시
    public Text dialogueText;       // 대사 표시
    public GameObject dialoguePanel; // 대화창 (없을 때 비활성화)

    [Header("Camera")]
    public CameraFollow cameraFollow; // 카메라 팔로우 스크립트

    [Header("Character Manager")]
    public CharacterManager characterManager; // 캐릭터 정보 관리

    [Header("Dialogue Data")]
    public List<DialogueData> dialogues; // 대사 목록
    private int currentDialogueIndex = 0;

    [Header("Typing Effect Settings")]
    public float typingSpeed = 0.05f; // 타이핑 속도 조절 가능 (인스펙터에서 설정)

    private bool isTyping = false; // 현재 타이핑 중인지 확인

    void Start()
    {
        if (dialogues.Count > 0)
        {
            ShowDialogue(currentDialogueIndex);
        }
        else
        {
            Debug.LogWarning("스토리 대사가 없습니다!");
        }
    }

    void Update()
    {
        // 마우스를 클릭하면 다음 대사로 이동
        if (Input.GetMouseButtonDown(0) && !isTyping)
        {
            NextDialogue();
        }
    }

    void ShowDialogue(int index)
    {
        if (index >= dialogues.Count)
        {
            EndStoryScene();
            return;
        }

        DialogueData dialogue = dialogues[index];

        // 캐릭터 이름과 대사 UI 적용
        characterNameText.text = dialogue.characterName;
        dialogueText.text = ""; // 타이핑 효과를 위해 초기화

        // 캐릭터 트랜스폼 찾기
        Transform characterTransform = characterManager.GetCharacterTransform(dialogue.characterName);

        // 카메라를 현재 대사 중인 캐릭터로 이동
        if (cameraFollow != null && characterTransform != null)
        {
            cameraFollow.SetTarget(characterTransform.gameObject);
        }

        // 타이핑 효과 실행
        StartCoroutine(TypeSentence(dialogue.dialogueText));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void NextDialogue()
    {
        currentDialogueIndex++;
        ShowDialogue(currentDialogueIndex);
    }

    void EndStoryScene()
    {
        Debug.Log("스토리 씬 종료! 다음 씬으로 이동할 수 있습니다.");
        // 필요하면 SceneManager.LoadScene("다음 씬 이름") 호출 가능
    }
}
