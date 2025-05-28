using UnityEngine;

public class MouseCameraParallax : MonoBehaviour
{
    [Header("이동 범위")]
    public float maxOffsetX = 1.5f;
    public float maxOffsetY = 0.8f;

    [Header("부드럽게 따라오는 정도")]
    [Range(1f, 20f)]
    public float smoothSpeed = 5f;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        Vector2 mouseViewport = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        // -0.5 ~ +0.5 범위로 보정
        float dx = (mouseViewport.x - 0.5f) * 2f;
        float dy = (mouseViewport.y - 0.5f) * 2f;

        Vector3 targetPos = initialPosition + new Vector3(dx * maxOffsetX, dy * maxOffsetY, 0);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
    }
}
