using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TimeAffectableAnimator : MonoBehaviour, ITimeAffectable
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        var tsc = FindFirstObjectByType<TimeStopController>();
        if (tsc != null)
            tsc.RegisterTimeAffectedObject(this);
    }

    public void StopTime()
    {
        if (anim != null)
            anim.speed = 0f; // Animator 멈춤
    }

    public void ResumeTime()
    {
        if (anim != null)
            anim.speed = 1f; // 다시 재생
    }
}
