using UnityEngine;

public class SniperEnemy : MonoBehaviour
{
    public GameObject bulletPrefab; // �ҷ� ������
    public Transform firePoint; // �ҷ� ���� ��ġ
    public float fireRate = 2f; // �ʴ� �߻� Ƚ��
    public float bulletSpeed = 10f; // �ҷ� �ӵ�
    private float nextFireTime;
    private Transform princess;

    void Start()
    {
        princess = GameObject.FindGameObjectWithTag("Princess").transform;

        // firePoint ��ġ�� ���� Ȯ��
        if (firePoint == null)
        {
            Debug.LogError("FirePoint�� �������� �ʾҽ��ϴ�!");
        }
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
        if (princess == null)
        {
            Debug.LogWarning("Princess �±׸� ���� ������Ʈ�� ã�� �� �����ϴ�.");
            return;
        }

        Vector2 direction = (princess.position - firePoint.position).normalized;

        // �ҷ� ���� �� ���� ����
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }
        else
        {
            Debug.LogError("�ҷ� �����տ� Rigidbody2D�� �����ϴ�!");
        }

        // �ҷ� ȸ�� ���� (optional: �ҷ��� Sprite�� ������ ���ߵ���)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
