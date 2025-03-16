using UnityEngine;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    [Tooltip("스토리 씬에서 사용할 캐릭터 목록")]
    public List<CharacterData> characters; // 인스펙터에서 등록할 캐릭터 목록

    private Dictionary<string, Transform> characterDict = new Dictionary<string, Transform>();

    void Awake()
    {
        // 딕셔너리에 캐릭터 정보 저장
        foreach (var entry in characters)
        {
            if (!characterDict.ContainsKey(entry.characterName))
            {
                characterDict.Add(entry.characterName, entry.characterTransform);
            }
        }
    }

    public Transform GetCharacterTransform(string characterName)
    {
        if (characterDict.TryGetValue(characterName, out Transform characterTransform))
        {
            return characterTransform;
        }
        Debug.LogWarning($"[CharacterManager] 캐릭터 '{characterName}'을(를) 찾을 수 없습니다!");
        return null;
    }
}
