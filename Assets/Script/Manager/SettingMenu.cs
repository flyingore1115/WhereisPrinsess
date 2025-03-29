using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingMenu : MonoBehaviour
{
    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle fullscreenToggle;

    void Start()
    {
        // 초기값 설정 (예시)
        masterVolumeSlider.value = 0.8f;
        bgmVolumeSlider.value = 0.8f;
        sfxVolumeSlider.value = 0.8f;

        // 이벤트 리스너 연결
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmVolumeSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void SetMasterVolume(float volume)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetMasterVolume(volume);
    }

    public void SetBGMVolume(float volume)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetBGMVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSFXVolume(volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // **닫기 버튼**을 누르면 PauseManager 통해서 close
    public void CloseButton()
    {
        PauseManager.Instance.CloseSettings();
    }
}
