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
}
