using UnityEngine;
using System.Collections;

public class PrincessControlHandler : MonoBehaviour
{
    public static PrincessControlHandler Instance;

    [Header("Control Settings")]
    public Transform princessTransform;   // 공주 오브젝트 참조
    public float moveSpeed = 3f;            // 공주 조종 이동 속도
    public float controlDuration = 5f;      // 기본 조종 지속 시간 (스킬 레벨에 따라 조정 가능)
    public float toggleCooldownTime = 0.5f; // 토글 입력 쿨타임 (외부 호출 시 참고용)

    private bool isControlling = false;
    private float controlTimer = 0f;
    private float lastToggleTime = -Mathf.Infinity;

    // 외부에서 조종 모드 여부 확인
    public bool IsControlling { get { return isControlling; } }

    private CameraFollow cameraFollow;     // 메인 카메라의 CameraFollow 스크립트
    private Player player;                 // 플레이어 참조
    private SpriteRenderer princessSprite; // 공주 스프라이트 렌더러 (방향 전환에 사용)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[PrincessControlHandler] Instance created.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = FindFirstObjectByType<Player>();

        if (Camera.main != null)
        {
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
        }

        if (princessTransform != null)
        {
            princessSprite = princessTransform.GetComponent<SpriteRenderer>();
            if (princessSprite == null)
                Debug.LogWarning("[PrincessControlHandler] Princess SpriteRenderer not found.");
        }
    }

    void Update()
    {
        // 기존 P키 입력 제거! 이제 외부(ActiveSkillHandler)에서 ToggleControlMode()를 호출합니다.

        if (isControlling)
        {
            controlTimer -= Time.deltaTime;
            Debug.Log($"[PrincessControlHandler] Control Timer: {controlTimer:F2}");
            if (controlTimer <= 0f)
            {
                Debug.Log("[PrincessControlHandler] Control duration expired.");
                ExitControlMode();
                return;
            }

            // 공주 이동 처리
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            Vector2 inputDir = new Vector2(moveX, moveY).normalized;
            if (inputDir != Vector2.zero)
            {
                princessTransform.Translate(inputDir * moveSpeed * Time.deltaTime);
                Debug.Log($"[PrincessControlHandler] Moving princess: dir = {inputDir}");

                // 입력 방향에 따라 스프라이트 flipX 처리
                if (princessSprite != null)
                {
                    if (moveX < 0)
                        princessSprite.flipX = true;
                    else if (moveX > 0)
                        princessSprite.flipX = false;
                }
            }
        }
    }

    /// <summary>
    /// 외부에서 호출하여 조종 모드를 토글합니다.
    /// ActiveSkillHandler에서 P키 입력으로 호출됩니다.
    /// </summary>
    public void ToggleControlMode()
    {
        // 토글 쿨타임 검사 (외부 호출 시에도 적용)
        if (Time.time - lastToggleTime < toggleCooldownTime)
        {
            Debug.Log("[PrincessControlHandler] Toggle ignored due to cooldown.");
            return;
        }
        lastToggleTime = Time.time;

        if (!isControlling)
            EnterControlMode();
        else
            ExitControlMode();
    }

    void EnterControlMode()
    {
        if (isControlling) return;
        isControlling = true;
        controlTimer = controlDuration;
        Debug.Log($"[PrincessControlHandler] Entering control mode for {controlDuration} seconds.");

        // 플레이어 입력 무시: Player 스크립트의 ignoreInput 플래그 설정
        if (player != null)
        {
            player.ignoreInput = true;
            Debug.Log("[PrincessControlHandler] Player input ignored.");
        }
        else
        {
            Debug.LogWarning("[PrincessControlHandler] Player not found.");
        }

        // 공주 조종 플래그 활성화 (Princess.cs의 isControlled)
        Princess princessScript = princessTransform.GetComponent<Princess>();
        if (princessScript != null)
        {
            princessScript.isControlled = true;
            Debug.Log("[PrincessControlHandler] Princess control flag set to true.");
        }
        else
        {
            Debug.LogWarning("[PrincessControlHandler] Princess script not found.");
        }

        // 카메라 추적 대상을 공주로 변경 (Transform -> GameObject)
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(princessTransform.gameObject);
            Debug.Log("[PrincessControlHandler] Camera target set to princess.");
        }
    }

    void ExitControlMode()
    {
        if (!isControlling) return;
        isControlling = false;
        Debug.Log("[PrincessControlHandler] Exiting control mode.");

        // 플레이어 입력 무시 해제
        if (player != null)
        {
            player.ignoreInput = false;
            Debug.Log("[PrincessControlHandler] Player input re-enabled.");
        }
        else
        {
            Debug.LogWarning("[PrincessControlHandler] Player not found on exit.");
        }

        // 공주 조종 플래그 비활성화
        Princess princessScript = princessTransform.GetComponent<Princess>();
        if (princessScript != null)
        {
            princessScript.isControlled = false;
            Debug.Log("[PrincessControlHandler] Princess control flag set to false.");
        }

        // 카메라를 즉시 플레이어로 전환
        if (cameraFollow != null && player != null)
        {
            cameraFollow.SetTarget(player.gameObject);
            Debug.Log("[PrincessControlHandler] Camera target set to player.");
        }
    }
}
