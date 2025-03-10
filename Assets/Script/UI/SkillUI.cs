using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SkillUI : MonoBehaviour
{
    public SkillManager skillManager;
    public SkillDescription skillDescription;
    public GameObject skillPanel;

    private List<SkillData> skillDataList = new List<SkillData>(); 
    public RectTransform skillUIRoot;
    public GameObject skillDetail;  // ✅ 스킬 설명 패널
    public Transform clockCenter;
    public Transform skillHandHour;
    public Transform skillHandMinute;
    public List<Image> skillIcons;
    public Text skillNameText;
    public Text skillLevelText;
    public Button upgradeButton;

    private int selectedSkillIndex = 0;
    private int unlockedSkillIndex = 0;
    private bool isUIActive = false;
    private bool isSkillDetailOpen = false;
    private Vector3 defaultScale;
    private Vector3 defaultPosition;

    private Coroutine minuteHandRotationCoroutine;
    private Coroutine hourHandRotationCoroutine;

    void Start()
    {
        if (skillManager != null)
        {
            skillDataList = new List<SkillData>(skillManager.allSkills);
        }
        else
        {
            Debug.LogWarning("SkillManager가 할당되지 않았습니다.");
        }

        if (skillPanel != null)
            skillPanel.SetActive(false);
        if (skillDetail != null)
            skillDetail.SetActive(false);

        if (skillUIRoot != null)
        {
            defaultScale = skillUIRoot.localScale;
            defaultPosition = skillUIRoot.anchoredPosition;
        }

        UpdateSkillUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleSkillPanel();
        }

        if (!isUIActive)
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            SelectPreviousSkill();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            SelectNextSkill();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            UpgradeSkill();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleSkillDetail();  // ✅ 스킬 상세 설명 토글
        }

        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                UnlockSkills(i);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            UnlockSkills(12);
        }
    }

    void ToggleSkillPanel()
    {
        isUIActive = !isUIActive;
        if (skillPanel != null)
            skillPanel.SetActive(isUIActive);
        Time.timeScale = isUIActive ? 0f : 1f;
    }

    void ToggleSkillDetail()
    {
        if (skillDetail == null) return;

        isSkillDetailOpen = !isSkillDetailOpen;
        skillDetail.SetActive(isSkillDetailOpen);
    }

    void SelectPreviousSkill()
    {
        if (skillIcons.Count == 0) return;
        selectedSkillIndex = (selectedSkillIndex - 1 + skillIcons.Count) % skillIcons.Count;
        if (selectedSkillIndex > unlockedSkillIndex)
        {
            selectedSkillIndex = unlockedSkillIndex;
        }
        UpdateSkillUI();
    }

    void SelectNextSkill()
    {
        if (skillIcons.Count == 0) return;
        selectedSkillIndex = (selectedSkillIndex + 1) % skillIcons.Count;
        if (selectedSkillIndex > unlockedSkillIndex)
        {
            selectedSkillIndex = 0;
        }
        UpdateSkillUI();
    }

    void UpgradeSkill()
    {
        if (selectedSkillIndex > unlockedSkillIndex)
        {
            Debug.Log("아직 해금 X");
            return;
        }
        SkillData selectedSkill = skillDataList[selectedSkillIndex];
        if (selectedSkill != null)
        {
            skillManager.UpgradeSkill(selectedSkill);
            UpdateSkillUI();
        }
    }

    void UnlockSkills(int count)
    {
        if (skillIcons.Count == 0) return;
        unlockedSkillIndex = Mathf.Clamp(count - 1, 0, skillIcons.Count - 1);
        UpdateSkillUI();
    }

    void UpdateSkillUI()
    {
        if (skillDataList == null || skillDataList.Count == 0)
        {
            Debug.LogWarning("Skill Data List is empty. UI will be hidden.");
            if (skillPanel != null)
                skillPanel.SetActive(false);
            return;
        }

        for (int i = 0; i < skillIcons.Count; i++)
        {
            if (i < skillDataList.Count && skillDataList[i] != null && skillDataList[i].skillIcon != null)
            {
                skillIcons[i].sprite = skillDataList[i].skillIcon;
                skillIcons[i].enabled = true;
            }
            else
            {
                skillIcons[i].sprite = null;
                skillIcons[i].enabled = false;
            }
        }

        SkillData selectedSkill = skillDataList[selectedSkillIndex];
        if (selectedSkill != null)
        {
            if (skillNameText != null)
                skillNameText.text = selectedSkill.skillName;
            if (skillLevelText != null)
                skillLevelText.text = "Level: " + skillManager.GetSkillLevel(selectedSkill);
        }

        RotateClockHands();

        if (skillDescription != null)
            skillDescription.UpdateSkillDescription(selectedSkill);
    }

    void RotateClockHands()
    {
        if (skillIcons.Count == 0)
            return;

        Vector3 minuteTargetPos = skillIcons[selectedSkillIndex].rectTransform.position;
        Vector3 minuteDirection = (minuteTargetPos - clockCenter.position).normalized;
        float minuteAngle = Mathf.Atan2(minuteDirection.y, minuteDirection.x) * Mathf.Rad2Deg - 90f;

        if (minuteHandRotationCoroutine != null)
            StopCoroutine(minuteHandRotationCoroutine);
        minuteHandRotationCoroutine = StartCoroutine(RotateHandSmoothly(skillHandMinute, minuteAngle));

        Vector3 hourTargetPos = skillIcons[unlockedSkillIndex].rectTransform.position;
        Vector3 hourDirection = (hourTargetPos - clockCenter.position).normalized;
        float hourAngle = Mathf.Atan2(hourDirection.y, hourDirection.x) * Mathf.Rad2Deg - 90f;

        if (hourHandRotationCoroutine != null)
            StopCoroutine(hourHandRotationCoroutine);
        hourHandRotationCoroutine = StartCoroutine(RotateHandSmoothly(skillHandHour, hourAngle));
    }

    IEnumerator RotateHandSmoothly(Transform hand, float targetAngle)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Quaternion startRotation = hand.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            hand.localRotation = Quaternion.Lerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }

        hand.localRotation = targetRotation;
    }
}
