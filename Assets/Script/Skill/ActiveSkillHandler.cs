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
        if (Input.GetKeyDown(KeyCode.Q)) ActivateSkill("expand_sight");
        if (Input.GetKeyDown(KeyCode.E)) ActivateSkill("princess_shield");
        if (Input.GetKeyDown(KeyCode.F)) ActivateSkill("aggro");
        if (Input.GetKeyDown(KeyCode.R)) ActivateSkill("slow_enemies");
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
            case "expand_sight": // 시야 확장
                if (mainCamera != null)
                {
                    mainCamera.orthographicSize += 2f;
                }
                yield return new WaitForSeconds(effectDuration);
                if (mainCamera != null)
                {
                    mainCamera.orthographicSize -= 2f;
                }
                break;

            case "princess_shield": // 공주 보호막
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

            case "aggro": // 어그로 끌기
                Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 10f);
                foreach (var enemy in enemies)
                {
                    ExplosiveEnemy enemyAI = enemy.GetComponent<ExplosiveEnemy>();
                    if (enemyAI != null)
                    {
                        enemyAI.AggroPlayer();
                    }
                }
                yield return new WaitForSeconds(effectDuration);
                break;

            case "slow_enemies": // 적 시간 감속
                Collider2D[] allEnemies = FindObjectsOfType<Collider2D>();
                foreach (var enemy in allEnemies)
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
