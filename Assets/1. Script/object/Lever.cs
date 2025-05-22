using UnityEngine;
using System.Collections;

public class Lever : MonoBehaviour
{
    [Tooltip("이 레버가 제어하는 오브젝트")]
    public MovingPlatform connectedPlatform;

    [Tooltip("레버 회전 속도")]
    public float rotationSpeed = 200f;

    [Tooltip("활성화된 상태의 회전 각도")]
    public float activatedAngle = 30f; // 오른쪽으로 30도 회전

    [Tooltip("비활성화된 상태의 회전 각도")]
    public float deactivatedAngle = -30f; // 왼쪽으로 -30도 회전 (기본)

    private bool isActivated = false; // 레버 상태
    private Coroutine rotationCoroutine;

    void Start()
    {
        // 레버를 기본 상태로 설정
        transform.localRotation = Quaternion.Euler(0, 0, deactivatedAngle);
    }

    void OnMouseDown()
    {
        ToggleLever();
    }

    void ToggleLever()
    {
        isActivated = !isActivated;

        // 기존 회전 코루틴 중지 후 새 코루틴 실행
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        rotationCoroutine = StartCoroutine(RotateLever(isActivated ? activatedAngle : deactivatedAngle));

        // 연결된 오브젝트에 신호 전달
        if (connectedPlatform != null)
        {
            connectedPlatform.SetActiveState(isActivated);
        }
    }

    IEnumerator RotateLever(float targetAngle)
    {
        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        transform.localRotation = targetRotation;
    }
}
