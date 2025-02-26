using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimeAffectable
{
    void StopTime();
    void ResumeTime();
    void RestoreColor();
}