using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource sfxSource;
    private AudioSource bgmSource;
    private AudioSource rewindSfxSource;  // 되감기 효과음 전용 AudioSource
    private Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();

    private List<AudioClip> bgmClips = new List<AudioClip>(); // Resources에서 로드한 BGM 클립들을 저장

    private float masterVolume = 0.8f;
    private float sfxVolume = 0.8f;
    private float bgmVolume = 0.8f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureAudioSources();
        InitializeSounds();
        ApplyVolume();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void EnsureAudioSources()
    {
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }

        // 전용 되감기 효과음 AudioSource 생성 (만약 없으면)
        if (rewindSfxSource == null)
        {
            GameObject rewindObj = new GameObject("RewindSFX");
            rewindObj.transform.parent = this.transform;
            rewindSfxSource = rewindObj.AddComponent<AudioSource>();
            rewindSfxSource.loop = true;
        }
    }

    private void InitializeSounds()
    {
        // Audio/SFX 폴더에서 모든 오디오 클립 로드
        AudioClip[] loadedSFXClips = Resources.LoadAll<AudioClip>("Audio/SFX");
        foreach (AudioClip clip in loadedSFXClips)
        {
            if (clip != null)
                soundClips[clip.name] = clip;
        }
        // Audio/BGM 폴더에서 모든 오디오 클립 로드
        AudioClip[] loadedBGMClips = Resources.LoadAll<AudioClip>("Audio/BGM");
        bgmClips = new List<AudioClip>(loadedBGMClips);
    }

    private void ApplyVolume()
    {
        AudioListener.volume = masterVolume;
        sfxSource.volume = sfxVolume;
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
        if (rewindSfxSource != null)
            rewindSfxSource.volume = sfxVolume; // 필요에 따라 별도 볼륨 설정 가능
    }

    public void PlaySFX(string soundName)
    {
        EnsureAudioSources();
        if (soundClips.TryGetValue(soundName, out AudioClip clip))
        {
            if (clip != null)
                sfxSource.PlayOneShot(clip, sfxVolume);
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found in SoundManager!");
        }
    }

    public void PlayBGM(string bgmName)
    {
        EnsureAudioSources();
        AudioClip bgmClip = bgmClips.Find(b => b.name == bgmName);
        if (bgmClip != null)
        {
            if (bgmSource.clip == bgmClip)
                return;
            bgmSource.clip = bgmClip;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM '{bgmName}' not found in SoundManager.");
        }
    }

    public void StopAllSounds()
    {
        bgmSource.Stop();
        sfxSource.Stop();
        rewindSfxSource.Stop();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // 재생: 되감기 효과음 전용 AudioSource 사용
    public void PlayLoopSFX(string soundName)
    {
        EnsureAudioSources();
        // 재생 전에 rewindSfxSource를 활성화
        if (rewindSfxSource != null)
        {
            rewindSfxSource.enabled = true;
        }

        if (soundClips.TryGetValue(soundName, out AudioClip clip))
        {
            if (clip != null)
            {
                rewindSfxSource.clip = clip;
                rewindSfxSource.loop = true;
                rewindSfxSource.time = 0f;
                rewindSfxSource.Play();
            }
            else
            {
                Debug.LogWarning($"AudioClip '{soundName}' is null!");
            }
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found in SoundManager!");
        }
    }

    public void StopLoopSFX(string soundName)
    {
        if (rewindSfxSource != null)
        {
            // Stop 효과음
            rewindSfxSource.Stop();
            // 초기화: clip, loop, time
            rewindSfxSource.clip = null;
            rewindSfxSource.loop = false;
            rewindSfxSource.time = 0f;
            // AudioSource를 완전히 비활성화해서 내부 재생 예약을 초기화
            rewindSfxSource.enabled = false;
            Debug.Log($"StopLoopSFX: Sound '{soundName}' stopped.");
        }
        else
        {
            Debug.Log("StopLoopSFX: rewindSfxSource is null.");
        }
    }


    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp(volume, 0, 100) / 100f;
        bgmSource.volume = bgmVolume;
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp(volume, 0, 100) / 100f;
        AudioListener.volume = masterVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp(volume, 0, 100) / 100f;
        sfxSource.volume = sfxVolume;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureAudioSources();
        switch (scene.name)
        {
            case "MainMenu":
                PlayBGM("BGM_Start");
                break;
            case "New_Game":
                PlayBGM("BGM_InGame");
                break;
            default:
                StopBGM();
                break;
        }
    }
}
