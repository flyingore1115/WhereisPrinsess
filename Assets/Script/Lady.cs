using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Lady : MonoBehaviour
{
    [Header("던질 프리팹들 (케이크, 빵 등)")]
    public GameObject[] throwPrefabs;

    [Header("케이크가 도착할 목표 지점들")]
    public Transform[] targetPoints;

    [Header("던질 출발점 (아가씨 손 위치)")]
    public Transform throwOrigin;    // null 이면 this.transform 사용

    [Header("비행에 걸리는 시간 (초)")]
    public float travelTime = 1f;

    [Header("포물선 최고 높이")]
    public float arcHeight = 2f;

    [Header("던질 간 딜레이 (초)")]
    public float throwDelay = 0.1f;

    [Header("회전 속도 (도/초)")]
    public float spinSpeed = 360f;

    [HideInInspector]
    public List<GameObject> spawned = new List<GameObject>();

    private bool hasThrown = false;

    /// <summary>
    /// 외부에서 호출: 튜토리얼 시작 시 케이크 던지기
    /// </summary>
    public void StartThrowing()
    {
        if (hasThrown) return;
        hasThrown = true;
        spawned.Clear();

        if (throwOrigin == null)
            throwOrigin = transform;

        StartCoroutine(ThrowSequence());
    }

    /// <summary>
    /// 순차적으로 약간의 딜레이를 두고 케이크 생성 → 포물선+스핀
    /// </summary>
    private IEnumerator ThrowSequence()
    {
        int count = Mathf.Min(throwPrefabs.Length, targetPoints.Length);
        for (int i = 0; i < count; i++)
        {
            // 1) 출발점에서 인스턴스 생성
            GameObject obj = Instantiate(
                throwPrefabs[i],
                throwOrigin.position,
                Quaternion.identity
            );

            // 튜토리얼 태그·스크립트
            obj.tag = "TutorialTarget";
            var tut = obj.AddComponent<ThrowableTutorialTarget>();
            tut.master = this;

            spawned.Add(obj);

            // 2) 포물선 비행 + 스핀
            StartCoroutine(ThrowArcWithSpin(obj,
                throwOrigin.position,
                targetPoints[i].position,
                travelTime
            ));

            yield return new WaitForSeconds(throwDelay);
        }
    }

    /// <summary>
    /// start→end 까지 time 초 동안 포물선(사인 곡선) 궤적으로 이동시키면서 spinSpeed 로 회전.
    /// 도착 즉시 스핀과 물리 모두 멈춤.
    /// </summary>
    private IEnumerator ThrowArcWithSpin(GameObject obj, Vector3 start, Vector3 end, float time)
    {
        float elapsed = 0f;

        // 물리 중력 끄고, Kinematic 상태로 전환
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;    // transform 이동만 사용
        }

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);

            // 직선 보간 + 사인 곡선 높이 보정
            Vector3 linePos = Vector3.Lerp(start, end, t);
            float height = arcHeight * Mathf.Sin(Mathf.PI * t);
            obj.transform.position = linePos + Vector3.up * height;

            // 회전
            obj.transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self);

            yield return null;
        }

        // 1) 정확히 목표 지점 찍기
        obj.transform.position = end;

        // 2) 회전 완전 멈춤: 방향 초기화 (원한다면 다른 값도 가능)
        obj.transform.rotation = Quaternion.identity;

        // 3) Rigidbody 존재 시, 완전히 정지
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            // 필요하면 아래처럼 회전 잠금
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    /// <summary>
    /// 터치 클릭 시 호출됨 (ThrowableTutorialTarget 내부에서)
    /// </summary>
    public void RegisterClick(GameObject obj)
    {
        // 클릭 카운트 등 처리
    }
}
