using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SniperEnemy : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float bulletSpeed = 10f;
    private float nextFireTime;
    private Transform princess;

    void Start()
    {
        princess = GameObject.FindGameObjectWithTag("Princess").transform;
    }

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            FireAtPrincess();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void FireAtPrincess()
    {
        Vector2 direction = (princess.position - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = direction * bulletSpeed;
    }
}
