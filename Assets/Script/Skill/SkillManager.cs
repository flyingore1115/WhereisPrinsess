using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SkillManager : MonoBehaviour
{
    public List<SkillData> allSkills; // 모든 스킬 목록
    private Dictionary<SkillData, int> acquiredSkills = new Dictionary<SkillData, int>(); // 습득한 스킬과 레벨

    public int maxSkillUnlock = 12; // 시침 위치 (최대 습득 가능 스킬)
    private int selectedSkillIndex = 0; // 현재 선택한 스킬 (분침)

    void Start()
    {
        foreach (SkillData skill in allSkills)
        {
            Debug.Log($"Skill Loaded: {skill.skillName}");
            AcquireSkill(skill); // 게임 시작 시 모든 스킬을 자동 획득
        }
        UpdateSkillSelection();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SelectPreviousSkill();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SelectNextSkill();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            UpgradeSkill();
        }
    }

    void SelectPreviousSkill()
    {
        selectedSkillIndex = (selectedSkillIndex - 1 + allSkills.Count) % allSkills.Count;
        UpdateSkillSelection();
    }

    void SelectNextSkill()
    {
        selectedSkillIndex = (selectedSkillIndex + 1) % allSkills.Count;
        UpdateSkillSelection();
    }

    void UpgradeSkill()
    {
        SkillData skill = allSkills[selectedSkillIndex];
        if (!acquiredSkills.ContainsKey(skill))
        {
            acquiredSkills[skill] = 1;
            Debug.Log($"Skill Acquired: {skill.skillName} (Level 1)");
        }
        else if (acquiredSkills[skill] < skill.maxLevel)
        {
            acquiredSkills[skill]++;
            Debug.Log($"Skill Upgraded: {skill.skillName} (Level {acquiredSkills[skill]})");
        }
        else
        {
            Debug.Log($"Skill {skill.skillName} is already at max level.");
        }
    }

    void UpdateSkillSelection()
    {
        Debug.Log($"현재 선택한 스킬: {allSkills[selectedSkillIndex].skillName}");
    }

    // ✅ 추가된 메서드
    public SkillData GetSkillByName(string skillName)
    {
        SkillData skill = allSkills.FirstOrDefault(s => s.skillName == skillName);
        if (skill == null)
        {
            Debug.LogWarning($"Skill not found: {skillName}");
        }
        return skill;
    }

    public bool HasSkill(SkillData skill)
    {
        bool hasSkill = acquiredSkills.ContainsKey(skill);
        Debug.Log($"HasSkill Check - {skill.skillName}: {hasSkill}");
        return hasSkill;
    }

    public int GetSkillLevel(SkillData skill)
    {
        int level = acquiredSkills.ContainsKey(skill) ? acquiredSkills[skill] : 0;
        return level;
    }

    public void AcquireSkill(SkillData skill)
    {
        if (!acquiredSkills.ContainsKey(skill))
        {
            acquiredSkills[skill] = 1;
            Debug.Log($"Skill Acquired: {skill.skillName} (Level 1)");
        }
    }
}
