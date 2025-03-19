using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager Instance;

    // ★ 포스트 프로세싱 볼륨
    [Header("PostProcess Volume / Profile")]
    public PostProcessVolume postProcessVolume;

    // 가져올 효과들
    private ColorGrading colorGrading;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private Grain grain;

    // [선택] 화면 흔들림(카메라 흔들기) 연출을 위한 참조
    [Header("Camera Shake")]
    public Camera mainCamera;
    public float shakeMagnitude = 0.5f;
    public float shakeDuration = 0.2f;

    // ================== Awake ==================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryAssignPostProcessVolume();
        }
        else
        {
            Destroy(gameObject);
        }

        
    }

    // ================== 씬 로드 시 마다 볼륨 재할당 (태그 or Name) ==================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryAssignPostProcessVolume();
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // 새 씬의 메인카메라를 할당
        }

        if (colorGrading != null)
        {
            colorGrading.saturation.overrideState = true;
            colorGrading.temperature.overrideState = true;
        }

    }

    private void TryAssignPostProcessVolume()
    {
        if (postProcessVolume == null)
        {
            GameObject ppObj = GameObject.FindGameObjectWithTag("PostProcessVolume");
            if (ppObj != null)
            {
                postProcessVolume = ppObj.GetComponent<PostProcessVolume>();
            }
            else
            {
                Debug.LogWarning("PostProcessVolume 오브젝트를 씬에서 찾을 수 없습니다!");
                return;
            }
        }
        
        // 프로파일 인스턴스 생성: 에셋의 공유 인스턴스가 아니라 런타임 인스턴스를 사용하도록 함
        if (postProcessVolume.profile != null)
        {
            postProcessVolume.profile = Instantiate(postProcessVolume.profile);
        }
        
        if (postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGetSettings(out colorGrading);
            postProcessVolume.profile.TryGetSettings(out vignette);
            postProcessVolume.profile.TryGetSettings(out chromaticAberration);
            postProcessVolume.profile.TryGetSettings(out grain);
        }

        // 기본 상태 복원
        SetDefaultEffects();
    }

    // ================== 기본 상태 (아무 효과 없음) ==================
    public void SetDefaultEffects()
    {
        // 필요시 기본으로 켜둘 효과 설정 가능
        if (colorGrading != null)
        {
            colorGrading.saturation.value = 0f;
            colorGrading.temperature.value = 0f;
        }
        if (vignette != null)
        {
            // 비네트의 color, intensity, smoothness 등 모두 0~기본값
            vignette.intensity.value = 0f;
            vignette.color.value = Color.black;
        }
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0f;
        }
        if (grain != null)
        {
            grain.intensity.value = 0f;
        }
    }

    // ================== 1. 시간정지 ==================
    public void ApplyTimeStop()
    {
        if (colorGrading != null)
            colorGrading.saturation.value = -100f; 
        if (vignette != null)
        {
            vignette.intensity.value = 0.2f;
            vignette.smoothness.value = 0.5f;
            vignette.color.value = Color.black;
        }
        if (chromaticAberration != null)
            chromaticAberration.intensity.value = 0.3f;

        if (grain != null)
            grain.intensity.value = 0f; // 시간정지는 그레인 X
    }

    public void RemoveTimeStop()
    {
        SetDefaultEffects();
    }

    // ================== 2. 플레이어/공주 피격 ==================
    // 요구사항: “비네트 빨간색 + 잠깐 강하게 + 노이즈(그레인) + 화면 크게 흔들림”
    public void ApplyCharacterHitEffect(float duration = 0.5f)
    {
        // 비네트 빨간색, 강하게
        if (vignette != null)
        {
            vignette.color.value = Color.red;
            vignette.intensity.value = 0.45f; // 잠깐 세게
            vignette.smoothness.value = 0.6f;
        }

        // 그레인
        if (grain != null)
        {
            grain.intensity.value = 0.7f; 
        }

        // 색수차는 옵션. 원한다면 켜도 됨.
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0.3f;
        }

        // 화면 크게 흔들림
        if (mainCamera != null)
        {
            StartCoroutine(CoShakeCamera(shakeMagnitude * 2f, shakeDuration)); 
        }

        // 잠시 후 원상복구
        StartCoroutine(CoResetEffectAfter(duration));
    }

    // ================== 3. 플레이어가 적에게 피격시킴 ==================
    // “화면 강하게 흔들림”만 필요 → 다른 효과는 없음
    public void ApplyAttackHitEffect()
    {
        if (mainCamera != null)
        {
            StartCoroutine(CoShakeCamera(shakeMagnitude, shakeDuration));
        }
    }

    // ================== 4. 플레이어 행동불능 ==================
    // “시간정지보다 비네트 좀 더 강하게 + 색수차 조금”
    public void ApplyPlayerDisabled()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0.3f;  // 시간정지 0.2 -> 여기선 0.3
            vignette.color.value = Color.black;
        }
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0.2f;  // “아주 조금”
        }
        if (grain != null)
        {
            grain.intensity.value = 0f;
        }
        if (colorGrading != null)
        {
            colorGrading.saturation.value = 0f; // 흑백 X
        }
    }

    public void RemovePlayerDisabled()
    {
        SetDefaultEffects();
    }

    // ================== 5. 게임오버 ==================
    // “행동불능보다 비네트 조금 더 강하게”
    public void ApplyGameOver()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0.45f; // 행동불능 0.3 -> 여기선 0.45
            vignette.color.value = Color.black;
        }
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0.2f;  
        }
        if (grain != null)
        {
            grain.intensity.value = 0f;
        }
        if (colorGrading != null)
        {
            colorGrading.saturation.value = 0f;
        }
    }

    public void RemoveGameOver()
    {
        SetDefaultEffects();
    }

    // ================== 6. 되감기 ==================
    // “게임오버 비네트 수치 그대로 + 그레인 + 색수차 강하게”
    public void ApplyRewind()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0.45f;  // 게임오버값 유지
            vignette.color.value = Color.black;
        }
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0.8f; // 강하게
        }
        if (grain != null)
        {
            grain.intensity.value = 0.5f; // 그레인 추가
        }
        if (colorGrading != null)
        {
            colorGrading.saturation.value = 0f;
        }
    }

    public void RemoveRewind()
    {
        SetDefaultEffects();
    }

    // ================== 카메라 흔들림 코루틴 ==================
    private IEnumerator CoShakeCamera(float magnitude, float duration)
    {
        if (mainCamera == null) yield break;
        Vector3 originalPos = mainCamera.transform.localPosition;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            mainCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        mainCamera.transform.localPosition = originalPos;
    }

    // ================== 임시로 적용 후 일정 시간 뒤 복원 코루틴 ==================
    private IEnumerator CoResetEffectAfter(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        SetDefaultEffects();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
