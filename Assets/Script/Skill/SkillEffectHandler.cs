using UnityEngine;

public class SkillEffectHandler : MonoBehaviour
{
    public SkillManager skillManager;
    private Player player;
    private TimeStopController timeController;

    void Start()
    {
        player = GetComponent<Player>();
        timeController = FindFirstObjectByType<TimeStopController>();

        ApplyPassiveSkills(); // 게임 시작 시 전체 패시브 적용
    }

    // 🔹 특정 패시브 스킬만 다시 적용하는 메서드 추가
    public void ApplyPassiveSkill(SkillData skill)
    {
        if (skill.skillType == SkillData.SkillType.Passive && skillManager.HasSkill(skill))
        {
            int level = skillManager.GetSkillLevel(skill);
            ApplySkillEffect(skill, level);
        }
    }

    public void ApplyPassiveSkills()
    {
        foreach (var skill in skillManager.allSkills)
        {
            if (skill.skillType == SkillData.SkillType.Passive && skillManager.HasSkill(skill))
            {
                int level = skillManager.GetSkillLevel(skill);
                ApplySkillEffect(skill, level);
            }
        }
    }

    public void ApplySkillEffect(SkillData skill, int level)
    {
        float effectValue = skill.GetEffect(level); // 🔹 중복 누적 방지를 위해 개별 값 저장

        switch (skill.skillName)
        {
            case "회피": // 랜덤 확률로 회피
                if (player != null && player.movement != null)
                {
                    player.movement.SetDodgeChance(effectValue);
                    Debug.Log($"[Skill] Dodge chance set to {effectValue * 100}%");
                }
                break;

            case "이동 속도 증가": // 이동 속도 증가
                if (player != null && player.movement != null)
                {
                    // 업그레이드 효과가 반영된 새로운 속도를 계산해서 할당
                    float effect = skill.GetEffect(level);
                    player.movement.moveSpeed = player.movement.moveSpeed + effect;
                    if (level == skill.maxLevel)
                    {
                        player.movement.EnableDash();//이거 그냥 이름만 있음
                    }
                }
                break;

            case "에너지 총량 증가": // 에너지 총량 증가
                if (timeController != null)
                {
                    timeController.maxTimeGauge = 100f + effectValue; // 기본 최대 게이지 100 + 증가값
                    if (level == skill.maxLevel)
                    {
                        timeController.passiveChargeRate += 0.5f;
                    }
                }
                break;

            case "적 처치 에너지 증가": // 적 처치 에너지 증가
                if (timeController != null)
                {
                    timeController.enemyKillGain = 10f + effectValue; // 기본값 10 + 증가값
                }
                break;

            case "에너지 최적화": // 시간 정지 에너지 최적화 (소모량 감소 + 패시브 충전량 증가)
                if (timeController != null)
                {
                    timeController.timeStopDrainRate = 5f - effectValue; // 기본값 5 - 감소량
                    timeController.passiveChargeRate = 1f + (effectValue * 0.5f); // 기본 충전량 1 + 증가량
                }
                break;
        }
    }
}
