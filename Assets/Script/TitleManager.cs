using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TitleLoad()
    {
        SceneManager.LoadScene("Start");
        // 강제로 타임스케일을 1로 설정
        Time.timeScale = 1f;   
    }
}
