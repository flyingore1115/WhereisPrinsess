using UnityEngine;
using System.Collections;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        if (GameManager.LoadedCheckpoint != null)
        {
            StartCoroutine(TimePointManager.Instance.ApplyCheckpoint(GameManager.LoadedCheckpoint, false));
            GameManager.LoadedCheckpoint = null; // 중복 적용 방지
        }
    }
}
