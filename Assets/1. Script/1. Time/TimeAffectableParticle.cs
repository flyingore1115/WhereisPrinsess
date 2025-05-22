using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class TimeAffectableParticle : MonoBehaviour, ITimeAffectable
{
    private ParticleSystem ps;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        // TimeStopController 등록
        TimeStopController tsc = FindFirstObjectByType<TimeStopController>();
        if (tsc != null)
            tsc.RegisterTimeAffectedObject(this);
    }

    public void StopTime()
    {
        if (ps != null)
            ps.Pause(); // 파티클 일시정지
    }

    public void ResumeTime()
    {
        if (ps != null)
            ps.Play(); // 파티클 재생
    }
}
