using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager Instance;

    [Header("URP Volume")]
    public Volume postProcessVolume;

    private ColorAdjustments colorAdjustments;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private FilmGrain filmGrain;

    [Header("Camera Shake")]
    public Camera mainCamera;
    public float shakeMagnitude = 0.5f;
    public float shakeDuration = 0.2f;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryAssignPostProcessVolume();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void TryAssignPostProcessVolume()
    {
        if (postProcessVolume == null)
        {
            GameObject ppObj = GameObject.FindGameObjectWithTag("PostProcessVolume");
            if (ppObj != null)
            {
                postProcessVolume = ppObj.GetComponent<Volume>();
            }
            else
            {
                Debug.LogWarning("URP Volume 오브젝트를 찾을 수 없습니다!");
                return;
            }
        }

        if (postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out colorAdjustments);
            postProcessVolume.profile.TryGet(out vignette);
            postProcessVolume.profile.TryGet(out chromaticAberration);
            postProcessVolume.profile.TryGet(out filmGrain);
        }

        SetDefaultEffects();
    }

    public void SetDefaultEffects()
    {
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = 0f;
        }
        if (vignette != null)
        {
            vignette.intensity.value = 0f;
            vignette.color.value = Color.black;
        }
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0f;
        }
        if (filmGrain != null)
        {
            filmGrain.intensity.value = 0f;
        }
    }

    public void ApplyTimeStop()
    {
        if (colorAdjustments != null)
            colorAdjustments.saturation.value = -100f;

        if (vignette != null)
        {
            vignette.intensity.value = 0.2f;
            vignette.smoothness.value = 0.5f;
            vignette.color.value = Color.black;
        }

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = 0.3f;

        if (filmGrain != null)
            filmGrain.intensity.value = 0f;
    }

    public void RemoveTimeStop()
    {
        SetDefaultEffects();
    }

    public void ApplyCharacterHitEffect(float duration = 0.5f)
    {
        StartCoroutine(CoVignetteSmooth(Color.red, 0f, 0.45f, 0.1f));
        StartCoroutine(CoResetEffectAfter(duration));
    }

    private IEnumerator CoVignetteSmooth(Color color, float from, float to, float time)
    {
        if (vignette == null) yield break;

        vignette.color.value = color;
        vignette.smoothness.value = 0.6f;

        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / time;
            vignette.intensity.value = Mathf.Lerp(from, to, t);
            yield return null;
        }
        vignette.intensity.value = to;
    }

    public void ApplyAttackHitEffect()
    {
        if (mainCamera != null)
        {
            StartCoroutine(CoShakeCamera(shakeMagnitude, shakeDuration));
        }
    }

    public void ApplyPlayerDisabled()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0.3f;
            vignette.color.value = Color.black;
        }
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0.2f;
        }
        if (filmGrain != null)
        {
            filmGrain.intensity.value = 0f;
        }
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = 0f;
        }
    }

    public void RemovePlayerDisabled()
    {
        SetDefaultEffects();
    }

    public void ApplyGameOver()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0.45f;
            vignette.color.value = Color.black;
        }
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0.2f;
        }
        if (filmGrain != null)
        {
            filmGrain.intensity.value = 0f;
        }
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = 0f;
        }
    }

    public void RemoveGameOver()
    {
        SetDefaultEffects();
    }

    public void ApplyRewind()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0.45f;
            vignette.color.value = Color.black;
        }
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0.8f;
        }
        if (filmGrain != null)
        {
            filmGrain.intensity.value = 0.5f;
        }
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = 0f;
        }
    }

    public void RemoveRewind()
    {
        SetDefaultEffects();
    }

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
