using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActiveSkillHandler : MonoBehaviour
{
    public SkillManager skillManager;
    public Camera mainCamera;
    public Transform princess;  // 공주 오브젝트 참조 (PrincessControlHandler에서 사용 가능)
    private Dictionary<string, Coroutine> activeSkillCoroutines = new Dictionary<string, Coroutine>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) ActivateSkill("시야 확장");
        if (Input.GetKeyDown(KeyCode.E)) ActivateSkill("공주 보호막");
        if (Input.GetKeyDown(KeyCode.F)) ActivateSkill("어그로");
        if (Input.GetKeyDown(KeyCode.T)) ActivateSkill("적 시간 감속"); //이거 나중에 삭제
        if (Input.GetKeyDown(KeyCode.H)) ActivateSkill("체력 되감기");
        if (Input.GetKeyDown(KeyCode.P)) ActivateSkill("공주 조종"); // 기존 P키 입력을 스킬 핸들러에서 제어
    }

    void ActivateSkill(string skillName)
    {
        SkillData skill = skillManager.GetSkillByName(skillName);
        if (skill != null && skillManager.HasSkill(skill))
        {
            int level = skillManager.GetSkillLevel(skill);

            if (activeSkillCoroutines.ContainsKey(skillName))
            {
                StopCoroutine(activeSkillCoroutines[skillName]);
            }

            Coroutine newCoroutine = StartCoroutine(ApplySkillEffect(skill, level));
            activeSkillCoroutines[skillName] = newCoroutine;
        }
    }

    IEnumerator ApplySkillEffect(SkillData skill, int level)
    {
        float effectDuration = skill.GetEffect(level);
        Debug.Log($"[Active Skill] {skill.skillName} activated for {effectDuration} seconds");

        switch (skill.skillName)
        {
            case "공주 조종":
                {
                    // 조종 모드는 PrincessControlHandler에서 직접 실행되도록 변경
                    if (PrincessControlHandler.Instance != null)
                    {
                        PrincessControlHandler.Instance.ToggleControlMode();
                        Debug.Log("[Active Skill] 공주 조종 mode toggled.");
                    }
                    else
                    {
                        Debug.LogWarning("[Active Skill] PrincessControlHandler instance not found.");
                    }
                    // 효과 지속 시간 동안 대기 (조종 지속 시간은 내부에서 관리)
                    yield return new WaitForSeconds(effectDuration);
                }
                break;

            // 기존 스킬 로직들은 그대로 유지
            case "시야 확장":
                if (mainCamera != null)
                {
                    float originalSize = mainCamera.orthographicSize;
                    float expandedSize = originalSize + 2f;
                    float expandTime = 0.3f;
                    float shrinkTime = 0.3f;
                    float holdTime = effectDuration - expandTime - shrinkTime;
                    if (holdTime < 0) holdTime = 0f;
                    float t = 0f;
                    while (t < expandTime)
                    {
                        t += Time.deltaTime;
                        mainCamera.orthographicSize = Mathf.Lerp(originalSize, expandedSize, t / expandTime);
                        yield return null;
                    }
                    mainCamera.orthographicSize = expandedSize;
                    yield return new WaitForSeconds(holdTime);
                    t = 0f;
                    while (t < shrinkTime)
                    {
                        t += Time.deltaTime;
                        mainCamera.orthographicSize = Mathf.Lerp(expandedSize, originalSize, t / shrinkTime);
                        yield return null;
                    }
                    mainCamera.orthographicSize = originalSize;
                }
                break;

            case "공주 보호막":
                if (princess != null)
                {
                    Princess princessScript = princess.GetComponent<Princess>();
                    if (princessScript != null)
                    {
                        bool maxLevel = (level >= skill.maxLevel);
                        Debug.Log($"[ActiveSkillHandler] 공주 보호막 호출: level = {level}, skill.maxLevel = {skill.maxLevel}, maxLevel = {maxLevel}");
                        princessScript.EnableShield(effectDuration, maxLevel);
                    }
                }
                break;

            case "어그로":
                Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, 10f);
                foreach (var enemy in nearbyEnemies)
                {
                    ExplosiveEnemy enemyAI = enemy.GetComponent<ExplosiveEnemy>();
                    if (enemyAI != null)
                    {
                        enemyAI.AggroPlayer();
                    }
                }
                yield return new WaitForSeconds(effectDuration);
                break;
                case "체력 되감기":
                {
                    float energyCost = 20f;
                    var tsc = TimeStopController.Instance;
                    if (tsc != null && tsc.TrySpendGauge(energyCost))
                    {
                        PlayerOver maid = FindFirstObjectByType<PlayerOver>();
                        if (maid != null)
                        {
                            maid.RestoreHealth(1);
                            Debug.Log("[Active Skill] 메이드 체력 되감기 activated: 체력 +1");
                        }
                    }
                    else
                    {
                        Debug.Log("시간 에너지가 부족합니다. 메이드 체력 회복 실패");
                    }
                    yield return new WaitForSeconds(effectDuration);
                }
                break;

            case "반격 시스템":
                {
                    //구현안함
                }
                break;
        }

        activeSkillCoroutines.Remove(skill.skillName);
    }
}
