using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private AudioSource sfxSource;
    private AudioSource bgmSource;
    private Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();

    // 필요하다면 inspector에 노출할 필요 없으므로 삭제하거나 주석처리 가능
    // public List<AudioClip> audioClips = new List<AudioClip>(); // 기존 효과음 클립 리스트
    // public List<AudioClip> bgmClips = new List<AudioClip>();     // 기존 BGM 클립 리스트

    private List<AudioClip> bgmClips = new List<AudioClip>(); // Resources에서 로드한 BGM 클립들을 저장

    private float masterVolume = 0.8f;
    private float sfxVolume = 0.8f;
    private float bgmVolume = 0.8f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("SoundManager Instance Already Exists. Destroying new instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("SoundManager Instance Created!");

        EnsureAudioSources();
        InitializeSounds();  // Resources 폴더에서 오디오 파일들을 로드합니다.
        ApplyVolume();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void EnsureAudioSources()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }
    }

    private void InitializeSounds()
    {
        // Audio/SFX 폴더에서 모든 오디오 클립 로드
        AudioClip[] loadedSFXClips = Resources.LoadAll<AudioClip>("Audio/SFX");
        if (loadedSFXClips.Length == 0)
        {
            Debug.LogWarning("Resources/Audio/SFX 폴더에서 오디오 클립을 찾을 수 없습니다.");
        }
        foreach (AudioClip clip in loadedSFXClips)
        {
            if (clip != null)
            {
                soundClips[clip.name] = clip;
            }
        }

        // Audio/BGM 폴더에서 모든 오디오 클립 로드
        AudioClip[] loadedBGMClips = Resources.LoadAll<AudioClip>("Audio/BGM");
        if (loadedBGMClips.Length == 0)
        {
            Debug.LogWarning("Resources/Audio/BGM 폴더에서 오디오 클립을 찾을 수 없습니다.");
        }
        bgmClips = new List<AudioClip>(loadedBGMClips);
    }


    private void ApplyVolume()
    {
        AudioListener.volume = masterVolume;
        sfxSource.volume = sfxVolume;
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
    }

    public void PlaySFX(string soundName)
    {
        EnsureAudioSources();

        if (soundClips.TryGetValue(soundName, out AudioClip clip))
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
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

    public void PlayBGM(string bgmName)
    {
        EnsureAudioSources();

        // bgmClips 리스트에서 이름이 일치하는 클립을 찾기
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
    }

    public void StopBGM()
    {
        bgmSource.Stop();
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
            case "Start":
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
