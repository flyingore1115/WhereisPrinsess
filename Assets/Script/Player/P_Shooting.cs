using UnityEngine;

public class P_Shooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int currentAmmo = 6;
    public int maxAmmo = 6;
    public TMPro.TMP_Text ammoText;

    public float reloadEnergyCost = 10f; // 재장전 시 소모량

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
