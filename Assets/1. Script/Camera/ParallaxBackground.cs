using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public float parallaxFactor = 0.5f; // 0 = 고정, 1 = 완전 따라감
    private Transform cam;
    private Vector3 lastCamPos;

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cam.position - lastCamPos;
        transform.position += new Vector3(delta.x * parallaxFactor, delta.y * parallaxFactor, 0f);
        lastCamPos = cam.position;
    }
}
