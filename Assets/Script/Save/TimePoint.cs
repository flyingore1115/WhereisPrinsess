using UnityEngine;
using MyGame;

public class TimePoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 공주가 체크포인트에 닿았을 때만 작동
        if (collision.CompareTag("Princess"))
        {
            Princess princess = collision.GetComponent<Princess>();
            PlayerOver playerOver = FindObjectOfType<PlayerOver>();
            if (princess == null || playerOver == null) return;

            // 만약 플레이어가 행동불능 상태라면, 공주가 죽지 않고 즉시 부활 로직을 실행
            if (playerOver.IsDisabled)
            {
                // 1) 공주를 멈추고, 플레이어를 공주 위치로 이동 & 체력 복원
                TimePointManager.Instance.ImmediateRevive();
                // 2) (선택) 공주와 플레이어의 새 좌표를 체크포인트로 저장
                //    만약 "위치 변환 후"에 새 좌표를 저장하고 싶다면:
                Princess actualPrincess = FindObjectOfType<Princess>();
                Player actualPlayer = FindObjectOfType<Player>();
                if (actualPrincess != null && actualPlayer != null)
                {
                    TimePointManager.Instance.SaveCheckpoint(actualPrincess.transform.position,
                                                             actualPlayer.transform.position);
                }
            }
            else
            {
                // 플레이어가 정상 상태라면, 기존대로 체크포인트 저장만 진행
                Player player = FindObjectOfType<Player>();
                if (player != null && princess != null)
                {
                    TimePointManager.Instance.SaveCheckpoint(princess.transform.position, player.transform.position);
                }
            }
        }
    }
}
