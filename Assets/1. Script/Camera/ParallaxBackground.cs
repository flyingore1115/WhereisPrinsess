// ParallaxBackground.cs
using UnityEngine;

[DefaultExecutionOrder(100)]      // ★추가: CinemachineBrain(-300)보다 뒤에서 실행
public class ParallaxBackground : MonoBehaviour
{
    [Tooltip("0 = 고정, 1 = 카메라와 1:1")]
    public float parallaxFactor = 0.5f;

    private Transform cam;
    private Vector3   startPos;   // ★추가: 배경의 최초 위치(기준점)

    void Start()
    {
        cam      = Camera.main.transform;
        startPos = transform.position;    // 기준점 저장
    }

    void LateUpdate()
    {
        // 카메라가 원점에서 얼마나 이동했는지를 비율만큼 반영
        Vector3 camOffset = cam.position * parallaxFactor;
        transform.position = new Vector3(
            startPos.x + camOffset.x,
            startPos.y + camOffset.y,
            startPos.z                                // Z는 그대로
        );
    }
}
