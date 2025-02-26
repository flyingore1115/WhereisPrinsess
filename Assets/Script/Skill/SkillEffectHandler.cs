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
            case "speed_boost": // 이동 속도 증가
                if (player != null && player.movement != null)
                {
                    player.movement.moveSpeed += skill.GetEffect(level);
                    if (level == skill.maxLevel)
                    {
                        player.movement.EnableDash();
                    }
                }
                break;

            case "max_energy_increase": // 에너지 총량 증가
                if (timeController != null)
                {
                    timeController.maxTimeGauge += skill.GetEffect(level);
                    if (level == skill.maxLevel)
                    {
                        timeController.passiveChargeRate += 0.5f;
                    }
                }
                break;

            case "energy_gain_on_kill_increase": // 적 처치 시 에너지 획득 증가
                if (timeController != null)
                {
                    timeController.enemyKillGain += skill.GetEffect(level);
                }
                break;

            case "energy_efficiency": // 시간 정지 에너지 최적화 (소모량 감소 + 패시브 충전량 증가)
                if (timeController != null)
                {
                    timeController.timeStopDrainRate -= skill.GetEffect(level);
                    timeController.passiveChargeRate += skill.GetEffect(level) * 0.5f;
                }
                break;
        }
    }
}
