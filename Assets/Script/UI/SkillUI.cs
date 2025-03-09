using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SkillUI : MonoBehaviour
{
    public SkillManager skillManager;
    public SkillDescription skillDescription;
    public GameObject skillPanel;

    // 인스펙터에 미리 할당된 리스트 대신 SkillManager의 데이터를 동적으로 가져옴
    private List<SkillData> skillDataList = new List<SkillData>(); 
    public RectTransform skillUIRoot; // UI 부모 오브젝트
    public GameObject skillDetail; // 스킬 설명 패널
    public Transform clockCenter; // 시계 중심
    public Transform skillHandHour; // 시침 역할
    public Transform skillHandMinute; // 분침 역할
    public List<Image> skillIcons; // 스킬 아이콘들 (인스펙터 할당)
    public Text skillNameText;
    public Text skillLevelText;
    public Button upgradeButton;

    public List<Image> skillIconImage; // 기타 사용될 이미지 리스트

    [Header("Zoom Settings")]
    public float zoomMultiplier = 1.5f; // 인스펙터에서 조절 가능한 확대 배율
    public float zoomDuration = 0.3f;   // 줌 효과 지속 시간

    private int selectedSkillIndex = 0; // 플레이어가 선택한 스킬 인덱스
    private int unlockedSkillIndex = 0; // 해금된 스킬의 최대 인덱스
    private bool isUIActive = false;
    private bool isSkillDetailOpen = false; // 스킬 상세 보기 상태
    private Vector3 defaultScale; // UI 루트의 기본 scale
    private Vector3 defaultPosition; // UI 루트의 기본 anchoredPosition

    void Start()
    {
        // 동적으로 SkillManager의 allSkills를 가져와서 내부 리스트 초기화
        if (skillManager != null)
        {
            // 스킬 리스트를 깊은 복사가 아니라 참조를 새 리스트로 만들어 사용
            skillDataList = new List<SkillData>(skillManager.allSkills);
        }
        else
        {
            Debug.LogWarning("SkillManager가 할당되지 않았습니다.");
        }

        // 초기화: 스킬 패널과 상세 패널 비활성화
        if (skillPanel != null)
            skillPanel.SetActive(false);
        if (skillDetail != null)
            skillDetail.SetActive(false);

        // 기본 scale과 위치 저장
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

    void SelectPreviousSkill()
    {
        if (skillIcons.Count == 0) return;
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
        if (skillIcons.Count == 0) return;
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
        SkillData selectedSkill = null;
        if (selectedSkillIndex < skillDataList.Count)
            selectedSkill = skillDataList[selectedSkillIndex];
        else
        {
            Debug.LogWarning("선택된 스킬 인덱스가 리스트 범위를 벗어났습니다.");
            return;
        }
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
        // 스킬 데이터 리스트가 비어있으면 UI를 숨김
        if (skillDataList == null || skillDataList.Count == 0)
        {
            Debug.LogWarning("Skill Data List is empty. UI will be hidden.");
            if (skillPanel != null)
                skillPanel.SetActive(false);
            return;
        }

        // 아이콘 업데이트
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

        // 선택된 스킬 정보 업데이트
        SkillData selectedSkill = skillDataList[selectedSkillIndex];
        if (selectedSkill != null)
        {
            if (skillNameText != null)
                skillNameText.text = selectedSkill.skillName;
            if (skillLevelText != null)
                skillLevelText.text = "Level: " + skillManager.GetSkillLevel(selectedSkill);
        }

        bool isUnlocked = selectedSkillIndex <= unlockedSkillIndex;
        if (upgradeButton != null)
            upgradeButton.interactable = isUnlocked && skillManager.GetSkillLevel(selectedSkill) < selectedSkill.maxLevel;

        for (int i = 0; i < skillIcons.Count; i++)
        {
            skillIcons[i].color = (i <= unlockedSkillIndex) ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
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
        if (skillHandMinute != null)
            skillHandMinute.localRotation = Quaternion.Euler(0, 0, minuteAngle);
        Vector3 hourTargetPos = skillIcons[unlockedSkillIndex].rectTransform.position;
        Vector3 hourDirection = (hourTargetPos - clockCenter.position).normalized;
        float hourAngle = Mathf.Atan2(hourDirection.y, hourDirection.x) * Mathf.Rad2Deg - 90f;
        if (skillHandHour != null)
            skillHandHour.localRotation = Quaternion.Euler(0, 0, hourAngle);
    }

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
