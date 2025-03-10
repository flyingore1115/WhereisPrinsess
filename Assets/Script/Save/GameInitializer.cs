using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using MyGame;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        if (GameManager.LoadedCheckpoint != null)
        {
            StartCoroutine(TimePointManager.Instance.ApplyCheckpoint(GameManager.LoadedCheckpoint, false));
            GameManager.LoadedCheckpoint = null;
        }
    }
}
