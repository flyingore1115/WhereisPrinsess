using UnityEngine;
using System.Collections;

[RequireComponent(typeof(InteractionUIController))]
public class Lever : MonoBehaviour
{
    private InteractionUIController ui;

    [Tooltip("이 레버가 제어하는 오브젝트")]
    public MovingPlatform connectedPlatform;

    [Tooltip("레버 회전 속도")]
    public float rotationSpeed = 200f;

    [Tooltip("활성화된 상태의 회전 각도")]
    public float activatedAngle = 30f;

    [Tooltip("비활성화된 상태의 회전 각도")]
    public float deactivatedAngle = -30f;

    [Tooltip("인터랙션 시 표시할 행동 텍스트")]
    public string actionText = "레버 당기기";

    private bool isActivated = false;

    // 초기 회전값
    private float initialAngle;

    private bool isPlayerInRange = false;
    private Coroutine rotationCoroutine;



    void Awake()
    {
        ui = GetComponent<InteractionUIController>();
        if (ui == null)
            Debug.LogError($"[Lever] InteractionUIController 컴포넌트를 찾을 수 없습니다: {name}");
        initialAngle = deactivatedAngle;//초기회전값 저장
    }

    void Start()
    {
        // 기본 회전 및 UI 숨김
        transform.localRotation = Quaternion.Euler(0, 0, deactivatedAngle);
        ui.Hide();
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleLever();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInRange = true;
        ui.Show("E", actionText);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInRange = false;
        ui.Hide();
    }

    void ToggleLever()
    {
        isActivated = !isActivated;

        if (rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);

        rotationCoroutine = StartCoroutine(RotateLever(
            isActivated ? activatedAngle : deactivatedAngle
        ));

        if (connectedPlatform != null)
            connectedPlatform.SetActiveState(isActivated);
    }

    IEnumerator RotateLever(float targetAngle)
    {
        Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
        while (Quaternion.Angle(transform.localRotation, targetRot) > 0.1f)
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }
        transform.localRotation = targetRot;
    }
    
    /// <summary>
    /// Rewind 시점에 호출합니다.
    /// 레버를 “당기기 전 상태” (deactivatedAngle)로 되돌리고,
    /// 연결된 플랫폼은 비활성화 상태로 리셋합니다.
    /// </summary>
    public void ResetOnRewind()
    {
        // ① 레버 상태 초기화
        isActivated = false;
        StopAllCoroutines();
        transform.localRotation = Quaternion.Euler(0, 0, deactivatedAngle);

        // ② 레버와 연결된 플랫폼 되돌리기
        if (connectedPlatform != null)
        {
            connectedPlatform.ResetOnRewind();  
        }
    }
}
