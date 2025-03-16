using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        if (TimePointManager.Instance.HasCheckpoint())
        {
            StartCoroutine(
                TimePointManager.Instance.ApplyCheckpoint(
                    TimePointManager.Instance.GetLastCheckpointData(),
                    false // 체크포인트 적용 후 대기 입력 없이 바로 시작
                )
            );
        }
    }
}
