using UnityEngine;

public enum EntryType { Dialogue, Choice, Action }

[System.Serializable]
public class DialogueEntryData
{
    public EntryType type;
    public string characterName;
    [TextArea] public string dialogueText;
    public string[] options;
    public string actionKey;  // "AttackTutorial" 같은 액션 이름
}

[CreateAssetMenu(menuName = "Dialogue/Sequence")]
public class DialogueSequenceSO : ScriptableObject
{
    public string triggerID;                   // 예: "Intro", "Scene2"
    public DialogueEntryData[] entries;        // 재생 순서대로 입력
}
