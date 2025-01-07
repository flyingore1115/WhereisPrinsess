using UnityEngine;

public class MouseManager : MonoBehaviour
{
    // 커서 이미지 설정
    public Texture2D defaultCursor;      // 기본 커서 이미지
    public Texture2D clickableCursor;   // 클릭 가능한 커서 이미지
    private Texture2D currentCursor;    // 현재 사용 중인 커서

    // 줌 관리
    public Camera mainCamera;           // 줌 조정에 사용할 메인 카메라
    public float zoomSpeed = 1f;        // 줌 조정 속도
    public float minZoom = 2f;          // 최소 줌 배율
    public float maxZoom = 10f;         // 최대 줌 배율

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
        HandleMouseScroll(); // 마우스 스크롤로 줌 조정
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

    void HandleMouseScroll()
    {
        // 마우스 휠 입력 값 가져오기
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0f)
        {
            // 현재 카메라 배율
            float currentZoom = mainCamera.orthographicSize;

            // 스크롤 입력에 따라 배율 조정
            currentZoom -= scrollInput * zoomSpeed;

            // 최소/최대 배율 제한
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            // 변경된 배율 적용
            mainCamera.orthographicSize = currentZoom;

            // 디버그 출력
            Debug.Log($"Current Zoom Level: {currentZoom}");
        }
    }
}
