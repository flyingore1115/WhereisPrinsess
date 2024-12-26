using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public Transform princess; // 공주의 Transform
    public Camera mainCamera; // 메인 카메라
    public float cameraMoveSpeed = 5f; // 카메라 이동 속도

    private bool isGameOver = false;

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        // 모든 움직임 정지
        Time.timeScale = 0f;

        // 게임 오버 효과 시작
        StartCoroutine(GameOverSequence());
    }

    private System.Collections.IEnumerator GameOverSequence()
    {
        // 카메라 이동
        while (Vector3.Distance(mainCamera.transform.position, princess.position + new Vector3(0, 0, -10)) > 0.1f)
        {
            Vector3 targetPosition = princess.position + new Vector3(0, 0, -10); // 공주 위치로 카메라 이동
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, cameraMoveSpeed * Time.unscaledDeltaTime);
            yield return null;
        }

        // 배경 어둡게 처리
        DarkenBackground();

        Debug.Log("Game Over!");
    }

    private void DarkenBackground()
    {
        var allObjects = FindObjectsOfType<SpriteRenderer>();

        foreach (var obj in allObjects)
        {
            if (obj.gameObject.CompareTag("Princess")) continue; // 공주는 제외

            Color color = obj.color;
            color.a = 0.2f; // 투명도를 낮춰 어둡게 처리
            obj.color = color;
        }
    }
}
