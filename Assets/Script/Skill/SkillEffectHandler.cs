using UnityEngine;

public class SkillEffectHandler : MonoBehaviour
{
    public SkillManager skillManager;
    private Player player;
    private TimeStopController timeController;

    void Start()
    {
        player = GetComponent<Player>();
        timeController = FindObjectOfType<TimeStopController>();

        ApplyPassiveSkills();
    }

    void ApplyPassiveSkills()
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

    void ApplySkillEffect(SkillData skill, int level)
    {
        switch (skill.skillName)
        {
            case "회피": // 랜덤 확률로 회피
                if (player != null && player.movement != null)
                {
                    player.movement.SetDodgeChance(skill.GetEffect(level)); // 회피 확률 적용
                    Debug.Log($"[Skill] Dodge chance increased to {skill.GetEffect(level) * 100}%");
                }
                break;
            case "이동 속도 증가": // 이동 속도 증가
                if (player != null && player.movement != null)
                {
                    player.movement.moveSpeed += skill.GetEffect(level);
                    if (level == skill.maxLevel)
                    {
                        player.movement.EnableDash();
                    }
                }
                break;

            case "에너지 총량 증가": // 에너지 총량 증가
                if (timeController != null)
                {
                    timeController.maxTimeGauge += skill.GetEffect(level);
                    if (level == skill.maxLevel)
                    {
                        timeController.passiveChargeRate += 0.5f;
                    }
                }
                break;

            case "적 처치 에너지 증가": // 적 처치 에너지 증가
                if (timeController != null)
                {
                    timeController.enemyKillGain += skill.GetEffect(level);
                }
                break;

            case "에너지 최적화": // 시간 정지 에너지 최적화 (소모량 감소 + 패시브 충전량 증가)
                if (timeController != null)
                {
                    timeController.timeStopDrainRate -= skill.GetEffect(level);
                    timeController.passiveChargeRate += skill.GetEffect(level) * 0.5f;
                }
                break;
        }
    }
}
