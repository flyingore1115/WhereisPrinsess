using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugManager : MonoBehaviour
{
    public GameObject player; // 플레이어 오브젝트
    public GameObject princess; // 공주 오브젝트
    public KeyCode debugKey = KeyCode.BackQuote; // 디버그 모드 활성화 키 (기본은 `)
    private bool debugMode = false;
    private bool isInvincible = false; // 무적 모드 상태
    private bool isStop = false; //공주 멈추기

    void Update()
    {
        // 디버그 모드 토글
        if (Input.GetKeyDown(debugKey))
        {
            debugMode = !debugMode;
            Debug.Log($"Debug Mode: {(debugMode ? "ON" : "OFF")}");
        }

        if (debugMode)
        {
            HandleDebugCommands();
        }
    }

    void HandleDebugCommands()
    {
        // 무적 모드 토글
        if (Input.GetKeyDown(KeyCode.I))
        {
            isInvincible = !isInvincible;
            Debug.Log($"Invincibility: {(isInvincible ? "Enabled" : "Disabled")}");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            isStop = !isStop;
            Debug.Log($"Invincibility: {(isStop ? "Enabled" : "Disabled")}");
        }

        // 씬 변경
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeScene(1); // 씬 1로 변경
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeScene(2); // 씬 2로 변경
        }

        // 적 삭제
        if (Input.GetKeyDown(KeyCode.K))
        {
            DestroyAllEnemies();
        }
    }

    // 씬 변경 함수
    void ChangeScene(int sceneIndex)
    {
        Debug.Log($"Changing to Scene {sceneIndex}");
        SceneManager.LoadScene(sceneIndex);
    }

    // 적 삭제 함수
    void DestroyAllEnemies()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            Destroy(enemy);
        }
        Debug.Log("All enemies destroyed!");
    }

    // 무적 모드 활성화 상태 체크
    public bool IsInvincible()
    {
        return isInvincible;
    }
    public bool IsStop()
    {
        return isStop;
    }
}
