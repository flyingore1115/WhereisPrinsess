using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SkillManager : MonoBehaviour
{
    public List<SkillData> allSkills; // 모든 스킬 목록
    private Dictionary<SkillData, int> acquiredSkills = new Dictionary<SkillData, int>(); // 습득한 스킬과 레벨

    public int maxSkillUnlock = 12; // 최대 습득 가능 스킬 수 (시침 위치 관련)

    private SkillEffectHandler skillEffectHandler;

    void Awake()
    {
        skillEffectHandler = FindFirstObjectByType<SkillEffectHandler>();


        // 게임 시작 시 모든 스킬을 자동 획득
        foreach (SkillData skill in allSkills)
        {
            AcquireSkill(skill);
        }
        UpdateSkillSelection();
    }

    void Start()
    {
        // 추가 초기화가 필요한 경우 Start()에서 처리
    }

    void UpdateSkillSelection()
    {
        // 디버그 로그: 현재 선택한 스킬(예: 0번 스킬)을 출력 (UI와 연동 시 해당 값은 UI에서 관리)
        Debug.Log($"현재 선택한 스킬: {allSkills[0].skillName}");
    }

    // 스킬 업그레이드 메서드: 인자로 전달된 스킬 데이터를 업그레이드함
    public void UpgradeSkill(SkillData skill)
    {
        if (!acquiredSkills.ContainsKey(skill))
        {
            acquiredSkills[skill] = 1;
            Debug.Log($"Skill Acquired: {skill.skillName} (Level 1)");
        }
        else if (acquiredSkills[skill] < skill.maxLevel)
        {
            acquiredSkills[skill]++;
            Debug.Log($"Skill Upgraded: {skill.skillName} (Level {acquiredSkills[skill]})");

            // 만약 공주 보호막 스킬이고, 이제 maxLevel에 도달했다면 여벌 목숨 추가
            if (skill.skillName == "공주 보호막" && acquiredSkills[skill] >= skill.maxLevel)
            {
                // Princess 인스턴스 찾기
                Princess princess = FindFirstObjectByType<Princess>();
                if (princess != null)
                {
                    princess.AddExtraLife();
                }
            }

            // 패시브 스킬일 경우, 특정 패시브만 다시 적용
            if (skill.skillType == SkillData.SkillType.Passive)
            {
                skillEffectHandler.ApplyPassiveSkill(skill);
            }
        }
        else
        {
            Debug.Log($"{skill.skillName} 이미 만렙임");
        }
    }


    public bool HasSkill(SkillData skill)
    {
        bool hasSkill = acquiredSkills.ContainsKey(skill);
        //Debug.Log($"HasSkill Check - {skill.skillName}: {hasSkill}");
        return hasSkill;
    }

    public int GetSkillLevel(SkillData skill)
    {
        return acquiredSkills.ContainsKey(skill) ? acquiredSkills[skill] : 0;
    }

    public void AcquireSkill(SkillData skill)
    {
        if (!acquiredSkills.ContainsKey(skill))
        {
            acquiredSkills[skill] = 1;
            //Debug.Log($"Skill Acquired: {skill.skillName} (Level 1)");
        }
    }
    public SkillData GetSkillByName(string skillName)
    {
        SkillData skill = allSkills.FirstOrDefault(s => s.skillName == skillName);
        if (skill == null)
        {
            Debug.LogWarning($"Skill not found: {skillName}");
        }
        return skill;
    }
}
