using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 폭발 예고 + 실제 폭발 처리 스크립트
///   ● 베이스 스프라이트 크기에 맞춰 파티클 크기를 자동 조정
///   ● 느낌표와 슬라이더는 베이스 오브젝트 중앙(로컬 0,0) 기준으로 배치
///   ● 폭발 파티클은 베이스 스프라이트 위치에서 Y -1만큼 내려서 생성
/// </summary>
public class ExplosionWarning : MonoBehaviour
{
    [Header("경고 지속 시간 (초)")]
    public float warningDuration = 1.5f;

    [Header("베이스 스프라이트 (경고 배경)")]
    // 이 SpriteRenderer의 크기(Scale)를 기반으로 파티클 크기를 맞춥니다.
    public SpriteRenderer baseSprite;

    [Header("느낌표 오브젝트 (자식으로 배치)")]
    // Hierarchy: ExplosionWarning → Exclamation
    //   이 오브젝트는 베이스 중앙 로컬 X=0, Y=-0.7 위치로 설정됩니다.
    public GameObject exclamationObject;

    [Header("원형 슬라이더 (UI Image, Fill Method: Radial 360)")]
    // Hierarchy: ExplosionWarning → CircularSlider(Image)
    //   이 오브젝트는 베이스 중앙 로컬 X=0, Y=0 위치로 설정됩니다.
    public Image circularSliderImage;

    [Header("위아래 흔들림 설정")]
    public float oscillationAmplitude = 0.5f;   // 로컬 Y방향 최대 이동량
    public float oscillationSpeed = 2f;         // 흔들림 속도

    [Header("폭발 효과 프리팹")]
    public GameObject explosionEffectPrefab;    // 실제 폭발 파티클 프리팹

    // 내부 상태
    private float elapsedTime = 0f;
    private Vector3 exclamationStartLocalPos;
    private Vector3 sliderStartLocalPos;

    void Awake()
    {
        // 필수 참조 체크
        if (baseSprite == null)
        {
            Debug.LogError("[ExplosionWarning] baseSprite가 할당되지 않았습니다.");
            Destroy(gameObject);
            return;
        }
        if (exclamationObject == null)
        {
            Debug.LogError("[ExplosionWarning] exclamationObject가 할당되지 않았습니다.");
            Destroy(gameObject);
            return;
        }
        if (circularSliderImage == null)
        {
            Debug.LogError("[ExplosionWarning] circularSliderImage가 할당되지 않았습니다.");
            Destroy(gameObject);
            return;
        }

        // ● 느낌표를 베이스 중앙 로컬 Y = -0.7 위치에 배치
        exclamationStartLocalPos = new Vector3(0f, -0.7f, 0f);
        exclamationObject.transform.localPosition = exclamationStartLocalPos;

        // ● 슬라이더를 베이스 중앙 로컬 Y = 0 위치에 배치
        sliderStartLocalPos = Vector3.zero;
        circularSliderImage.rectTransform.localPosition = sliderStartLocalPos;

        // 슬라이더 초기 채움 비율 = 1 (전부 채워진 상태)
        circularSliderImage.fillAmount = 1f;
    }

    void Start()
    {
        // 경고 코루틴 시작
        StartCoroutine(WarningRoutine());
    }

    void Update()
    {
        // 위아래 흔들림 (local Y 오프셋)
        float offsetY = Mathf.Sin(Time.time * oscillationSpeed) * oscillationAmplitude;

        // 느낌표 오브젝트 흔들기 (로컬 기준)
        Vector3 exPos = exclamationStartLocalPos;
        exPos.y += offsetY;
        exclamationObject.transform.localPosition = exPos;

        // 슬라이더 오브젝트 흔들기 (로컬 기준)
        Vector3 slPos = sliderStartLocalPos;
        slPos.y += offsetY;
        circularSliderImage.rectTransform.localPosition = slPos;
    }

    /// <summary>
    /// 경고 루틴: warningDuration 동안 슬라이더를 줄여가며 시간 경과 후 폭발을 수행
    /// </summary>
    private IEnumerator WarningRoutine()
{
    elapsedTime = 0f;

    while (elapsedTime < warningDuration)
    {
        // ① 시간정지 중인 동안엔 elapsedTime을 추가하지 않고
        if (!(TimeStopController.Instance != null && TimeStopController.Instance.IsTimeStopped))
        {
            // ② 시간이 흐르는 때에만 누적
            elapsedTime += Time.deltaTime;
        }
        // ③ 누적된 기준(elapsedTime)의 비율을 매 프레임 반영
        float fill = 1f - Mathf.Clamp01(elapsedTime / warningDuration);
        circularSliderImage.fillAmount = fill;

        yield return null;
    }

    // 경고 종료 → 폭발
    Explode();
    Destroy(gameObject);
}


    /// <summary>
    /// 실제 폭발 생성 및 데미지 판정
    ///  ● 베이스 스프라이트 중심에서 Y -1만큼 내려서 파티클 생성
    ///  ● 파티클 크기는 베이스 스프라이트 지름에 맞춤
    /// </summary>
    private void Explode()
    {
        // 1) 폭발 이펙트 생성 (파티클은 베이스 위치 Y -1)
        if (explosionEffectPrefab != null)
        {
            Vector3 spawnPos = baseSprite.transform.position + new Vector3(0f, -1f, 0f);
            GameObject effect = Instantiate(
                explosionEffectPrefab,
                spawnPos,
                Quaternion.identity
            );

            // 2) "파티클 크기"를 baseSprite의 스케일에 맞춤
            //    baseSprite.bounds.extents.x는 반경이므로, 지름으로 변환
            float spriteDiameter = baseSprite.bounds.extents.x * 2f;
            effect.transform.localScale = new Vector3(spriteDiameter, spriteDiameter, 1f);
        }

        // 3) 폭발 범위 내 데미지 판정 (baseSprite.bounds.extents.x를 반경으로 사용)
        float radius = baseSprite.bounds.extents.x;
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            baseSprite.transform.position,
            radius
        );

        foreach (var col in hits)
        {
            if (col.CompareTag("Player"))
            {
                col.GetComponent<PlayerOver>()?.TakeDamage(1);
            }
            else if (col.CompareTag("Princess"))
            {
                col.GetComponent<Princess>()?.GameOver();
            }
            else
            {
                //
            }
        }
    }

    // Editor에서 폭발 범위 시각화
    void OnDrawGizmosSelected()
    {
        if (baseSprite != null)
        {
            Gizmos.color = Color.red;
            float radius = baseSprite.bounds.extents.x;
            Gizmos.DrawWireSphere(baseSprite.transform.position, radius);
        }
    }
}
