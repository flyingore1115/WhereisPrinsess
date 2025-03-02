using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActiveSkillHandler : MonoBehaviour
{
    public SkillManager skillManager;
    public Camera mainCamera;
    public Transform princess;
    private Dictionary<string, Coroutine> activeSkillCoroutines = new Dictionary<string, Coroutine>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) ActivateSkill("시야 확장");
        if (Input.GetKeyDown(KeyCode.E)) ActivateSkill("공주 보호막");
        if (Input.GetKeyDown(KeyCode.F)) ActivateSkill("어그로");
        if (Input.GetKeyDown(KeyCode.T)) ActivateSkill("적 시간 감속");
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
            case "시야 확장": // 시야 확장
                if (mainCamera != null)
                {
                    float originalSize = mainCamera.orthographicSize;
                    float expandedSize = originalSize + 2f;
                    float expandTime = 0.3f;
                    float shrinkTime = 0.3f;
                    float holdTime = effectDuration - expandTime - shrinkTime;
                    if (holdTime < 0) holdTime = 0f; // 효과 지속시간이 너무 짧은 경우

                    float t = 0f;
                    // 0.5초간 확장
                    while (t < expandTime)
                    {
                        t += Time.deltaTime;
                        mainCamera.orthographicSize = Mathf.Lerp(originalSize, expandedSize, t / expandTime);
                        yield return null;
                    }
                    mainCamera.orthographicSize = expandedSize;

                    // holdTime 동안 확장 상태 유지
                    yield return new WaitForSeconds(holdTime);

                    t = 0f;
                    // 마지막 0.5초간 원래 크기로 축소
                    while (t < shrinkTime)
                    {
                        t += Time.deltaTime;
                        mainCamera.orthographicSize = Mathf.Lerp(expandedSize, originalSize, t / shrinkTime);
                        yield return null;
                    }
                    mainCamera.orthographicSize = originalSize;
                }
                break;


            case "공주 보호막": // 공주 보호막
                if (princess != null)
                {
                    Princess princessScript = princess.GetComponent<Princess>();
                    if (princessScript != null)
                    {
                        bool maxLevel = (level == skill.maxLevel);
                        princessScript.EnableShield(effectDuration, maxLevel);
                    }
                }
                yield return new WaitForSeconds(effectDuration);
                break;

            case "어그로": // 어그로 끌기
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

            case "반격 시스템": // 반격 기능
                TimeStopController timeStop = FindObjectOfType<TimeStopController>();
                if (timeStop != null && timeStop.IsTimeStopped)
                {
                    Debug.Log("[Skill] Counterattack Activated!");
                    Collider2D[] counterAttackEnemies = Physics2D.OverlapCircleAll(transform.position, 5f);
                    foreach (var enemy in counterAttackEnemies)
                    {
                        ExplosiveEnemy enemyAI = enemy.GetComponent<ExplosiveEnemy>();
                        if (enemyAI != null)
                        {
                            enemyAI.TakeDamage(); // 반격 피해 적용
                        }
                    }
                }
                break;

            case "적 시간 감속": // 적 시간 감속
                Collider2D[] slowedEnemies = FindObjectsOfType<Collider2D>();
                foreach (var enemy in slowedEnemies)
                {
                    ExplosiveEnemy enemyAI = enemy.GetComponent<ExplosiveEnemy>();
                    if (enemyAI != null)
                    {
                        enemyAI.SlowDown(effectDuration);
                    }
                }
                yield return new WaitForSeconds(effectDuration);
                break;
        }

        activeSkillCoroutines.Remove(skill.skillName);
    }
}
