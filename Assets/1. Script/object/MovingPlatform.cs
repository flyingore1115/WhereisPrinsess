using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
    [Tooltip("이동 방향 (X/Y 선택)")]
    public Vector2 moveDirection = new Vector2(1f, 0f);

    [Tooltip("이동 거리 (X/Y 이동 값)")]
    public float moveDistance = 5f;

    [Tooltip("이동 속도")]
    public float moveSpeed = 2f;

    [Tooltip("반복적으로 이동할 것인가?")]
    public bool isLooping = false;

    [Tooltip("활성화 시 이동하고 비활성화 시 원위치로 돌아오는가?")]
    public bool returnOnDeactivate = true;

    private Vector3 startPosition;
    private bool isActivated = false;
    private bool isMoving = false;

    void Start()
    {
        startPosition = transform.position;
    }

    public void SetActiveState(bool active)
    {
        isActivated = active;
        if (isLooping)
        {
            if (!isMoving) StartCoroutine(LoopMovement());
        }
        else
        {
            if (isActivated)
            {
                StopAllCoroutines();
                StartCoroutine(MoveToPosition(startPosition + (Vector3)(moveDirection.normalized * moveDistance)));
            }
            else if (returnOnDeactivate)
            {
                StopAllCoroutines();
                StartCoroutine(MoveToPosition(startPosition));
            }
        }
    }

    IEnumerator MoveToPosition(Vector3 targetPos)
    {
        isMoving = true;
        while ((transform.position - targetPos).sqrMagnitude > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        isMoving = false;
    }

    IEnumerator LoopMovement()
    {
        isMoving = true;
        while (isActivated)
        {
            Vector3 targetPos = startPosition + (Vector3)(moveDirection.normalized * moveDistance);
            yield return StartCoroutine(MoveToPosition(targetPos));

            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(MoveToPosition(startPosition));

            yield return new WaitForSeconds(0.5f);
        }
        isMoving = false;
    }

    /// <summary>
    /// Rewind 시에 호출해서 플랫폼을 “초기 위치”로 곧장 되돌립니다.
    /// 모션 중이던 코루틴을 멈추고, startPosition으로 즉시 복귀합니다.
    /// </summary>
    public void ResetOnRewind()
    {
        isActivated = false;
        StopAllCoroutines();
        transform.position = startPosition;
        isMoving = false;
    }
}
