using UnityEngine;

public class TimeAffectable : MonoBehaviour, ITimeAffectable
{
    public void StopTime()
    {
        // 시간 정지 시 포스트 프로세싱 효과 적용
        if (PostProcessingManager.Instance != null)
            PostProcessingManager.Instance.ApplyTimeStop();
    }

    public void ResumeTime()
    {
        // 원래 색으로 복원
        if (PostProcessingManager.Instance != null)
            PostProcessingManager.Instance.SetDefaultEffects();
    }
}
