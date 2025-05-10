// FixedRotation.cs
using UnityEngine;

public class FixedRotation : MonoBehaviour
{
    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}
