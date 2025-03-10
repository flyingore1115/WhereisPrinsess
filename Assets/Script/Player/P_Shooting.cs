using UnityEngine;

public class P_Shooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Transform player; //플레이어 위치 참조 (플레이어를 중심으로 회전해야 함)
    
    public float firePointRadius = 1.0f; //FirePoint가 플레이어 주변을 도는 반지름 길이

    public int currentAmmo = 6;
    public int maxAmmo = 6;
    public TMPro.TMP_Text ammoText;

    public float reloadEnergyCost = 10f; // 재장전 시 소모량

    void Update()
    {
        UpdateFirePointPosition(); //FirePoint 위치 업데이트
        HandleShooting();
    }

    private void UpdateFirePointPosition()
    {
        if (player == null || firePoint == null) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - player.position).normalized;

        // 🔥 FirePoint 위치를 플레이어를 중심으로 회전하는 방식으로 이동
        firePoint.position = player.position + (Vector3)direction * firePointRadius;

        // 🔥 FirePoint가 마우스를 바라보도록 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void HandleShooting()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ShootBullet();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    private void ShootBullet()
    {
        if (currentAmmo <= 0)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX("EmptyGunSound");
            }
            return;
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 shootDirection = (mousePosition - firePoint.position).normalized;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.SetDirection(shootDirection);

        TimeStopController timeStopController = FindObjectOfType<TimeStopController>();
        if (timeStopController != null && timeStopController.IsTimeStopped)
        {
            bulletScript.StopTime();
            timeStopController.RegisterTimeAffectedObject(bulletScript);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("PlayerGunSound");
        }

        currentAmmo--;
        UpdateAmmoUI();
    }

    private void Reload()
    {
        TimeStopController timeStopController = FindObjectOfType<TimeStopController>();
        if (timeStopController.currentTimeGauge < reloadEnergyCost)
        {
            Debug.Log("에너지 부족! 재장전 불가");
            return;
        }

        //재장전 에너지 소모
        timeStopController.currentTimeGauge -= reloadEnergyCost;
        timeStopController.currentTimeGauge = Mathf.Clamp(timeStopController.currentTimeGauge, 0, timeStopController.maxTimeGauge);

        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("RevolverSpin");
        }
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo} / {maxAmmo}";
        }
    }
}
