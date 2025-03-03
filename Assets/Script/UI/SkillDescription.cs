using UnityEngine;
using TMPro;

public class SkillDescription : MonoBehaviour
{
    public TextMeshProUGUI skillTitleText; // 스킬 제목
    public TextMeshProUGUI skillDescriptionText; // 스킬 설명

    // 스킬 정보를 업데이트하는 함수
    public void UpdateSkillDescription(SkillData skill)
    {
        if (skill == null)
        {
            skillTitleText.text = "스킬 없음";
            skillDescriptionText.text = "선택된 스킬이 없습니다.";
            return;
        }

        skillTitleText.text = skill.skillName; // 스킬 제목
        skillDescriptionText.text = skill.description; // 스킬 설명
    }
}
