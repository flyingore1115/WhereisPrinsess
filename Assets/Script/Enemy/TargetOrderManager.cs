using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame; // BaseEnemy, Player 등 포함
using TMPro;

public class TargetOrderManager : MonoBehaviour
{
    public static TargetOrderManager Instance;

    // 선택된 적 순서 목록
    private List<BaseEnemy> orderedEnemies = new List<BaseEnemy>();

    // 콤보 임계치 (예: 3마리까지는 일반 공격, 그 이후는 원콤)
    public int comboThreshold = 3;
    // 적 공격 사이 간격 (콤보 공격 시)
    public float comboAttackDelay = 0.3f;

    // 순서 번호 UI 프리팹 (TextMeshPro 컴포넌트가 있는 World Space Canvas 프리팹)
    public GameObject orderNumberUIPrefab;

    // 타겟 순서 지정 모드 활성 여부
    private bool selectionMode = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 타겟 순서 지정 모드를 시작 (예: 시간정지 상태에서 호출)
    public void StartTargetOrdering()
    {
        selectionMode = true;
        orderedEnemies.Clear();
        ClearAllOrderUI();
        Debug.Log("Target ordering mode activated.");
    }

    // 타겟 순서 지정 모드를 취소
    public void CancelTargetOrdering()
    {
        selectionMode = false;
        ClearAllOrderUI();
        orderedEnemies.Clear();
        Debug.Log("Target ordering mode cancelled.");
    }

    void Update()
    {
        if (!selectionMode) return;
        // 순서 지정은 시간정지 상태에서만 허용 (TimeStopController가 활성 상태여야 함)
        if (!TimeStopController.Instance.IsTimeStopped) return;

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                BaseEnemy enemy = hit.collider.GetComponent<BaseEnemy>();
                if (enemy != null && !orderedEnemies.Contains(enemy))
                {
                    AddTarget(enemy);
                }
            }
        }
    }

    private void AddTarget(BaseEnemy enemy)
    {
        orderedEnemies.Add(enemy);
        int order = orderedEnemies.Count;
        // 각 적에 순서 번호 UI 표시
        enemy.DisplayOrderNumber(order, orderNumberUIPrefab);
        Debug.Log("Added enemy: " + enemy.name + " as order " + order);
    }

    // 타겟 순서 지정 완료 후, 시간 정지 해제 시 호출되어 콤보 공격 실행
    public void ExecuteComboAttack()
    {
        if (orderedEnemies.Count == 0)
            return;
        StartCoroutine(ComboAttackRoutine());
    }

    private IEnumerator ComboAttackRoutine()
    {
        selectionMode = false;

        for (int i = 0; i < orderedEnemies.Count; i++)
        {
            BaseEnemy enemy = orderedEnemies[i];
            if (enemy == null) continue;

            // 콤보 수에 따라 공격 데미지 결정
            int damage = (i >= comboThreshold) ? enemy.maxHealth : 2; // 예: 일반 데미지 2, 콤보 임계치 초과 시 원콤
            enemy.TakeDamage(damage);
            Debug.Log("Attacked enemy: " + enemy.name + " with damage: " + damage);
            yield return new WaitForSeconds(comboAttackDelay);
        }

        ClearAllOrderUI();
        orderedEnemies.Clear();
    }

    private void ClearAllOrderUI()
    {
        // 씬 내 모든 BaseEnemy에 대해 순서 UI 제거
        BaseEnemy[] allEnemies = Object.FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
        foreach (BaseEnemy enemy in allEnemies)
        {
            enemy.ClearOrderNumber();
        }
    }

    public void ForceAddTarget(BaseEnemy enemy)
    {
        if (orderedEnemies.Contains(enemy)) return;

        orderedEnemies.Add(enemy);
        int order = orderedEnemies.Count;
        enemy.DisplayOrderNumber(order, orderNumberUIPrefab);
    }

}
