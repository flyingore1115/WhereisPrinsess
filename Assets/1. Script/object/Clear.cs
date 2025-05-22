using UnityEngine;
using UnityEngine.SceneManagement;

public class Clear : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        MySceneManager.Instance.LoadNextScene();
    }


}
