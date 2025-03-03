using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SkillUI : MonoBehaviour
{
    public SkillManager skillManager;
    public SkillDescription skillDescription;
    public GameObject skillPanel;

    public List<SkillData> skillDataList; // 모든 스킬 데이터 리스트
    public RectTransform skillUIRoot; // UI 부모 오브젝트
    public GameObject skillDetail; // 스킬 설명
    public Transform clockCenter; // 시계 중심
    public Transform skillHandHour; // 시침 역할
    public Transform skillHandMinute; // 분침 역할
    public List<Image> skillIcons; // 시계 숫자 위치의 스킬 아이콘들
    public Text skillNameText;
    public Text skillLevelText;
    public Button upgradeButton;

    public List<Image> skillIconImage;

    [Header("Zoom Settings")]
    public float zoomMultiplier = 1.5f; // 인스펙터에서 조절 가능한 확대 배율
    public float zoomDuration = 0.3f;   // 줌 효과 지속 시간

    private int selectedSkillIndex = 0; // 플레이어가 선택한 스킬 (분침 위치)
    private int unlockedSkillIndex = 0; // 해금된 스킬의 최대 위치 (시침 위치)
    private bool isUIActive = false;
    private bool isSkillDetailOpen = false; // 스킬 상세 보기 상태
    private Vector3 defaultScale; // UI 루트의 기본 scale
    private Vector3 defaultPosition; // UI 루트의 기본 anchoredPosition

    void Start()
    {
        skillPanel.SetActive(false);
        skillDetail.SetActive(false); // 상세 패널 비활성화
        defaultScale = skillUIRoot.localScale; // 기본 scale 저장 (예: (1,1,1))
        defaultPosition = skillUIRoot.anchoredPosition; // 기본 위치 저장 (예: (0,0))
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
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleSkillDetail();
        }

        // 숫자키 1~9로 테스트용 스킬 해금
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                UnlockSkills(i);
            }
        }
    }

    void ToggleSkillPanel()
    {
        isUIActive = !isUIActive;
        skillPanel.SetActive(isUIActive);
        Time.timeScale = isUIActive ? 0f : 1f;
    }

    void SelectPreviousSkill()
    {
        selectedSkillIndex = (selectedSkillIndex - 1 + skillIcons.Count) % skillIcons.Count;
        if (selectedSkillIndex == skillIcons.Count - 1 || selectedSkillIndex > unlockedSkillIndex)
        {
            Debug.Log("처음에서 뒤로 이동 -> 마지막 해금된 스킬로 이동");
            selectedSkillIndex = unlockedSkillIndex;
        }
        UpdateSkillUI();
    }

    void SelectNextSkill()
    {
        selectedSkillIndex = (selectedSkillIndex + 1) % skillIcons.Count;
        if (selectedSkillIndex > unlockedSkillIndex)
        {
            Debug.Log("해금되지 않은 스킬을 선택하여 초기 위치로 이동");
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
        skillManager.UpgradeSkill();
        UpdateSkillUI();
    }

    void UnlockSkills(int count)
    {
        unlockedSkillIndex = Mathf.Clamp(count - 1, 0, skillIcons.Count - 1);
        UpdateSkillUI();
    }

    void UpdateSkillUI()
    {
        for (int i = 0; i < skillIcons.Count; i++)
        {
            if (i < skillDataList.Count && skillDataList[i].skillIcon != null)
            {
                skillIcons[i].sprite = skillDataList[i].skillIcon; // 🔹 각 스킬 아이콘 적용
                skillIcons[i].enabled = true; // 🔹 아이콘 활성화
            }
            else
            {
                skillIcons[i].sprite = null; // 🔹 아이콘 없으면 None으로 설정
                skillIcons[i].enabled = false; // 🔹 비활성화
            }
        }

        // 🔹 선택된 스킬의 아이콘을 개별적으로 업데이트 (예: 패널 UI)
        SkillData selectedSkill = skillDataList[selectedSkillIndex];
        skillNameText.text = selectedSkill.skillName;
        skillLevelText.text = "Level: " + skillManager.GetSkillLevel(selectedSkill);

        bool isUnlocked = selectedSkillIndex <= unlockedSkillIndex;
        upgradeButton.interactable = isUnlocked && skillManager.GetSkillLevel(selectedSkill) < selectedSkill.maxLevel;

        for (int i = 0; i < skillIcons.Count; i++)
        {
            skillIcons[i].color = (i <= unlockedSkillIndex) ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        RotateClockHands();
        skillDescription.UpdateSkillDescription(selectedSkill);
    }


    void RotateClockHands()
    {
        if (skillIcons.Count == 0)
            return;
        Vector3 minuteTargetPos = skillIcons[selectedSkillIndex].rectTransform.position;
        Vector3 minuteDirection = (minuteTargetPos - clockCenter.position).normalized;
        float minuteAngle = Mathf.Atan2(minuteDirection.y, minuteDirection.x) * Mathf.Rad2Deg - 90f;
        skillHandMinute.localRotation = Quaternion.Euler(0, 0, minuteAngle);
        Vector3 hourTargetPos = skillIcons[unlockedSkillIndex].rectTransform.position;
        Vector3 hourDirection = (hourTargetPos - clockCenter.position).normalized;
        float hourAngle = Mathf.Atan2(hourDirection.y, hourDirection.x) * Mathf.Rad2Deg - 90f;
        skillHandHour.localRotation = Quaternion.Euler(0, 0, hourAngle);
    }

    // 선택된 스킬 아이콘을 기준으로 확대(줌 인)하도록 UI 루트의 Scale과 anchoredPosition을 조정
    void ToggleSkillDetail()
    {
        if (isSkillDetailOpen)
        {
            skillDetail.SetActive(false);
            StartCoroutine(ZoomAndRepositionUI(skillUIRoot, defaultScale, defaultPosition));
        }
        else
        {
            skillDetail.SetActive(true);
            Vector2 iconPos = skillIcons[selectedSkillIndex].rectTransform.anchoredPosition;
            Vector3 targetScale = defaultScale * zoomMultiplier;
            // 목표 anchoredPosition = - (targetScale * iconPos)
            Vector3 targetPos = - new Vector3(targetScale.x * iconPos.x, targetScale.y * iconPos.y, defaultPosition.z);
            StartCoroutine(ZoomAndRepositionUI(skillUIRoot, targetScale, targetPos));
        }
        isSkillDetailOpen = !isSkillDetailOpen;
    }

    IEnumerator ZoomAndRepositionUI(RectTransform uiRoot, Vector3 targetScale, Vector3 targetPosition)
    {
        float elapsed = 0f;
        Vector3 startScale = uiRoot.localScale;
        Vector3 startPos = uiRoot.anchoredPosition;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            uiRoot.localScale = Vector3.Lerp(startScale, targetScale, elapsed / zoomDuration);
            uiRoot.anchoredPosition = Vector3.Lerp(startPos, targetPosition, elapsed / zoomDuration);
            yield return null;
        }
        uiRoot.localScale = targetScale;
        uiRoot.anchoredPosition = targetPosition;
    }
}
