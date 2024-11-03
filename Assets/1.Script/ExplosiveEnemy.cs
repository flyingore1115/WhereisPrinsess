using UnityEngine;

public class ExplosiveEnemy : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float explosionRadius = 2f;
    public int damage = 50;
    private Transform princess;

    void Start()
    {
        princess = GameObject.FindGameObjectWithTag("Princess").transform;
    }

    void Update()
    {
        MoveTowardsPrincess();
    }

    void MoveTowardsPrincess()
    {
        if (princess != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, princess.position, moveSpeed * Time.deltaTime);
            float distanceToPrincess = Vector2.Distance(transform.position, princess.position);
            if (distanceToPrincess <= explosionRadius)
            {
                Explode();
            }
        }
    }

    void Explode()
    {
        // Here, you would add damage logic to the princess
        Debug.Log("Explosive Enemy explodes and deals damage!");
        Destroy(gameObject);
    }
}
