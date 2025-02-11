using UnityEngine.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    //--------------------------------------------점수관리
    public Text scoreText; // UI 텍스트
    private int score = 0;

    //----------------------------------------------------------------

    public static GameManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

       

    void Update()
    {

    }


    public void AddScore(int points)
    {
        score += points;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    

}
