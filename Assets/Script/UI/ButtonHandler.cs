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

    public void OnQuitButton()
    {
        MySceneManager.Instance.QuitGame();
    }

    public void TitleLoad()
    {
        MySceneManager.Instance.MainMenuScene();
        // 강제로 타임스케일을 1로 설정
        
        Time.timeScale = 1f;   
    }
}
