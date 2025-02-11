using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private AudioSource sfxSource;
    private AudioSource bgmSource;
    private Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();

    public List<AudioClip> audioClips = new List<AudioClip>(); // 효과음 클립 리스트
    public List<AudioClip> bgmClips = new List<AudioClip>();     // BGM 클립 리스트

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
        InitializeSounds();
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
        if (audioClips == null)
        {
            Debug.LogWarning("audioClips is null. Initializing empty list.");
            audioClips = new List<AudioClip>();
        }
        if (bgmClips == null)
        {
            Debug.LogWarning("bgmClips is null. Initializing empty list.");
            bgmClips = new List<AudioClip>();
        }

        foreach (var clip in audioClips)
        {
            if (clip != null)
            {
                soundClips[clip.name] = clip;
            }
        }
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
