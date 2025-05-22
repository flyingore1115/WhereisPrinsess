using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill System/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;         // 스킬 이름
    public string description;       // 스킬 설명
    public Sprite skillIcon;         // UI 아이콘
    public SkillType skillType;      // 액티브, 패시브, 시간 관련 스킬 등
    public int maxLevel = 6;         // 최대 업그레이드 단계
    public float[] levelEffects;     // 각 레벨별 효과 값

    public enum SkillType
    {
        Active,
        Passive,
        TimeRelated
    }

    public float GetEffect(int level)
    {
        return level > 0 && level <= maxLevel ? levelEffects[level - 1] : 0f;
    }
}
