using UnityEngine;
using MyGame;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        var tpManager = TimePointManager.Instance;
        if (TimePointManager.Instance.HasCheckpoint() && TimePointManager.Instance.GetLastCheckpointData() != null)
        {
            StartCoroutine(TimePointManager.Instance.ApplyCheckpoint(
                TimePointManager.Instance.GetLastCheckpointData(), 
                false
            ));
        }
        else
        {
            Debug.Log("[GameInitializer] 체크포인트 데이터가 없습니다. 새 게임 시작 위치로 설정합니다.");
            // 체크포인트가 없는 경우, 각 캐릭터를 기본 시작 위치로 재설정
            Princess princess = GameObject.FindGameObjectWithTag("Princess")?.GetComponent<Princess>();
            Player player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
            if (princess != null)
            {
                princess.ResetToDefaultPosition();
            }
            // 플레이어는 씬에 배치된 위치 그대로 사용 (또는 필요하면 별도 초기화)
        }
    }
}
