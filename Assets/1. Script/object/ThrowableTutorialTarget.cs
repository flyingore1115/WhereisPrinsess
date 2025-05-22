// ThrowableTutorialTarget.cs
using UnityEngine;

public class ThrowableTutorialTarget : MonoBehaviour
{
    private Lady master;
    private bool initialized = false;

    // Lady.ThrowSequence 에서 반드시 호출할 것
    public void Initialize(Lady lady)
    {
        master = lady;
        initialized = true;
    }

    void Start()
    {
        if (!initialized)
            Debug.LogError($"[ThrowableTutorialTarget] Initialize 호출되지 않음: {name}");
    }

    void OnMouseDown()
    {
        if (!initialized) return;
        if (!TimeStopController.Instance.IsTimeStopped) return;

        master.RegisterClick(gameObject);
        master.spawned.Remove(gameObject);
        Destroy(gameObject);
    }
}
