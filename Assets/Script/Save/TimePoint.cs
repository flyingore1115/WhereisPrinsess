using UnityEngine;
using MyGame;

public class TimePoint : MonoBehaviour
{
    private bool isUsed = false;  // ★ 추가: 이미 사용된 체크포인트인가?

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 공주가 체크포인트에 닿았을 때만 작동
        if (collision.CompareTag("Princess"))
        {
            // 이미 이 체크포인트 사용됨
            if (isUsed) return;

            Princess princess = collision.GetComponent<Princess>();
            PlayerOver playerOver = FindObjectOfType<PlayerOver>();
            if (princess == null || playerOver == null) return;

            // 실제 공주/플레이어 참조
            Princess actualPrincess = FindObjectOfType<Princess>();
            Player actualPlayer = FindObjectOfType<Player>();
            if (actualPrincess == null || actualPlayer == null) return;

            // 일단 체크포인트 사용 처리
            isUsed = true;

            // (원하는 경우) 콜라이더 제거
            // Destroy(GetComponent<Collider2D>());
            // or GetComponent<Collider2D>().enabled = false;

            // 플레이어가 행동불능 상태라면 즉시 부활 로직
            if (playerOver.IsDisabled)
            {
                TimePointManager.Instance.SaveCheckpoint(actualPrincess.transform.position,
                                                         actualPlayer.transform.position);
            }
            else
            {
                // 플레이어 정상 상태 -> 기존대로 체크포인트 저장
                TimePointManager.Instance.SaveCheckpoint(princess.transform.position,
                                                         actualPlayer.transform.position);
            }

            // 사운드 추가
            // SoundManager.Instance.PlaySFX("");
        }
    }
}
