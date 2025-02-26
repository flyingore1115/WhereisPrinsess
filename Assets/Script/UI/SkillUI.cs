using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillUI : MonoBehaviour
{
    public SkillManager skillManager;
    public GameObject skillPanel;
    public Transform clockCenter; // 시계 중심
    public Transform skillHandHour; // 시침 역할
    public Transform skillHandMinute; // 분침 역할
    public List<Image> skillIcons; // 시계 숫자 위치의 스킬 아이콘들
    public Text skillNameText;
    public Text skillLevelText;
    public Button upgradeButton;

    private int selectedSkillIndex = 0;
    private bool isUIActive = false;

    void Start()
    {
        skillPanel.SetActive(false); // 기본적으로 UI 비활성화
        UpdateSkillUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleSkillPanel();
        }

        if (!isUIActive) return; // UI가 비활성화 상태면 입력을 무시

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

    void ToggleSkillPanel()
    {
        isUIActive = !isUIActive;
        skillPanel.SetActive(isUIActive);

        if (isUIActive)
        {
            Time.timeScale = 0f; // 게임 정지
        }
        else
        {
            Time.timeScale = 1f; // 게임 다시 시작
        }
    }

    void SelectPreviousSkill()
    {
        selectedSkillIndex = (selectedSkillIndex - 1 + skillIcons.Count) % skillIcons.Count;
        UpdateSkillUI();
    }

    void SelectNextSkill()
    {
        selectedSkillIndex = (selectedSkillIndex + 1) % skillIcons.Count;
        UpdateSkillUI();
    }

    void UpgradeSkill()
    {
        SkillData skill = skillManager.allSkills[selectedSkillIndex];
        //skillManager.UpgradeSkill(skill);
        UpdateSkillUI();
    }

    void UpdateSkillUI()
    {
        SkillData skill = skillManager.allSkills[selectedSkillIndex];
        skillNameText.text = skill.skillName;
        skillLevelText.text = "Level: " + skillManager.GetSkillLevel(skill);

        upgradeButton.interactable = skillManager.GetSkillLevel(skill) < skill.maxLevel;

        RotateClockHands();
    }

    void RotateClockHands()
    {
        if (skillIcons.Count == 0 || selectedSkillIndex >= skillIcons.Count) return;

        Vector3 targetPosition = skillIcons[selectedSkillIndex].rectTransform.position; // 아이콘 위치 가져오기
        Vector3 direction = (targetPosition - clockCenter.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float finalAngle = angle - 90f; // 12시 방향 보정

        skillHandHour.localRotation = Quaternion.Euler(0, 0, finalAngle);
        skillHandMinute.localRotation = Quaternion.Euler(0, 0, finalAngle);
    }
}
