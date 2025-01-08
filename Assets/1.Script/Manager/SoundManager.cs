using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private AudioSource sfxSource;
    private Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();
    public List<AudioClip> audioClips; // 에디터에서 클립 등록

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
            sfxSource = GetComponent<AudioSource>();
            InitializeSounds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSounds()
    {
        foreach (var clip in audioClips)
        {
            if (clip != null)
            {
                soundClips[clip.name] = clip;
            }
        }
    }

    public void PlaySFX(string soundName)
    {
        if (soundClips.TryGetValue(soundName, out AudioClip clip))
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }
    }

    public void PauseAllSounds()
    {
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (var source in audioSources)
        {
            source.Pause();
        }
    }

    public void StopAllSounds()
    {
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (var source in audioSources)
        {
            source.Stop();
        }
    }
}
