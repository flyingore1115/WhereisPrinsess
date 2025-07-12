using UnityEngine;

using MyGame;
using System.Collections.Generic;

public class GameInitializer : MonoBehaviour
{
    private static readonly HashSet<string> _checkpointDoneScenes = new();
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

            // 씬 진입 시 자동 저장
        Player p = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        Princess pr = GameObject.FindGameObjectWithTag("Princess")?.GetComponent<Princess>();
        
        if (p != null && pr != null)
            {
                var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (!_checkpointDoneScenes.Contains(sceneName))
                {
                    Vector2 princessPos = Princess.Instance ? (Vector2)Princess.Instance.transform.position : Vector2.zero;
                    Vector2 playerPos = Player.Instance ? (Vector2)Player.Instance.transform.position : Vector2.zero;

                    TimePointManager.Instance.SaveCheckpoint(princessPos, playerPos);
                    _checkpointDoneScenes.Add(sceneName);   // 이 씬은 더 이상 처음이 아님
                    Debug.Log($"[GI] 최초 로드 – 체크포인트 저장 완료: {sceneName}");
                }
                //Debug.Log("[GameInitializer] 씬 진입 시 자동 체크포인트 저장 완료");
            }


        }
    }
}
