using UnityEngine;
public class ButtonHandler : MonoBehaviour
{
    public void OnNewGameButton()
    {
        GameManager.Instance.NewGame();
    }

    public void OnContinueButton()
    {
        GameManager.Instance.ContinueGame();
    }
    //게임 재개
    public void OnReume()
    {
        PauseManager.Instance.Resume();
    }

    public void OnSettingsButton()
    {
        PauseManager.Instance.OpenSettings();
    }

    public void OnQuitButton()
    {
        MySceneManager.Instance.QuitGame();
    }

    public void TitleLoad()
    {
        MySceneManager.Instance.MainMenuScene();
        Time.timeScale = 1f;   
    }
}
