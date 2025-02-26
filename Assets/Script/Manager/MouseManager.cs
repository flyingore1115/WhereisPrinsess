using UnityEngine;

public class MouseManager : MonoBehaviour
{
    // 커서 이미지 설정
    public Texture2D defaultCursor;      // 기본 커서 이미지
    public Texture2D clickableCursor;   // 클릭 가능한 커서 이미지
    private Texture2D currentCursor;    // 현재 사용 중인 커서
    public Camera mainCamera;           // 메인 카메라


    void Start()
    {
        // 카메라 초기화
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 기본 커서 설정
        if (defaultCursor != null)
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
            currentCursor = defaultCursor;
        }
    }

    void Update()
    {
        UpdateCursor();      // 마우스 커서 업데이트
    }

    void UpdateCursor()
    {
        // 마우스 위치를 월드 좌표로 변환
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));

        // 2D에서 OverlapPoint로 충돌 감지
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

        // 기본 커서와 클릭 가능한 커서를 전환
        Texture2D newCursor = defaultCursor;

        if (hit != null && hit.CompareTag("Enemy"))
        {
            newCursor = clickableCursor;
        }

        // 커서 변경
        if (currentCursor != newCursor)
        {
            Cursor.SetCursor(newCursor, Vector2.zero, CursorMode.Auto);
            currentCursor = newCursor;
        }
    }
}
