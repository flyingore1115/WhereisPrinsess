using UnityEngine;

[System.Serializable]
public class TimeSnapshot
{
    public Vector3 playerPosition;
    public Vector3 princessPosition;
    public Vector2 playerVelocity;
    public Vector2 princessVelocity;

    // ✅ 애니메이션 상태 저장 (플레이어 & 공주)
    public string playerAnimationState;
    public float playerNormalizedTime;
    public string princessAnimationState;
    public float princessNormalizedTime;
}
