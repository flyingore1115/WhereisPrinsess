using UnityEngine;

public class TutorialGrabPoint : MonoBehaviour
{
    void OnDrawGizmos()=> Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size);

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Princess"))
            Debug.Log("[GrabPoint] Princess reached");
    }
}
