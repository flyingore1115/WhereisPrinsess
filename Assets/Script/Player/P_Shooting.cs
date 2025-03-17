using UnityEngine;
using System.Collections;
using TMPro;

public class P_Shooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Transform player; // 플레이어 중심, FirePoint의 기준
    public float firePointRadius = 1.0f; // 플레이어 중심에서 FirePoint까지의 거리

    public int currentAmmo = 6;
    public int maxAmmo = 6;
    public TMP_Text ammoText;

    public float reloadEnergyCost = 10f; // 재장전 시 소모량

    [Tooltip("총알 UI가 보여지는 시간 (초)")]
    public float bulletUIDisplayDuration = 0.5f;
    [Tooltip("총알 UI 오브젝트 (평소에는 비활성화)")]
    public GameObject bulletUI;

    private Coroutine hideBulletUICoroutine;

    void Update()
    {
        // FirePoint의 위치와 회전 업데이트
        UpdateFirePointPosition();

        // 재장전 입력 처리
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    /// <summary>
    /// 플레이어 중심으로 마우스 방향에 따라 FirePoint 위치와 회전을 업데이트합니다.
    /// </summary>
    private void UpdateFirePointPosition()
    {
        if (player == null || firePoint == null) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Z축은 0으로 고정
        mousePosition.z = 0;
        Vector2 direction = (mousePosition - player.position).normalized;
        firePoint.position = player.position + (Vector3)direction * firePointRadius;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void HandleShooting(){
        if (Input.GetMouseButtonDown(1))
        {
            ShootBullet();
        }
    }

    private void ShootBullet()
    {
        if (currentAmmo <= 0)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("EmptyGunSound");
            return;
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector2 shootDirection = (mousePosition - firePoint.position).normalized;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(shootDirection);
        }

        if (bulletUI != null)
        {
            bulletUI.SetActive(true);
            if (hideBulletUICoroutine != null)
            {
                StopCoroutine(hideBulletUICoroutine);
            }
            hideBulletUICoroutine = StartCoroutine(HideBulletUIAfterDelay());
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX("PlayerGunSound");

        currentAmmo--;
        UpdateAmmoUI();
    }

    private IEnumerator HideBulletUIAfterDelay()
    {
        yield return new WaitForSecondsRealtime(bulletUIDisplayDuration);
        if (bulletUI != null)
            bulletUI.SetActive(false);
    }

    public void HideBulletUI()
    {
        bulletUI.SetActive(false);
    }

    private void Reload()
    {
        TimeStopController timeStopController = FindObjectOfType<TimeStopController>();
        if (timeStopController.currentTimeGauge < reloadEnergyCost)
        {
            Debug.Log("에너지 부족! 재장전 불가");
            return;
        }

        timeStopController.currentTimeGauge -= reloadEnergyCost;
        timeStopController.currentTimeGauge = Mathf.Clamp(timeStopController.currentTimeGauge, 0, timeStopController.maxTimeGauge);

        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX("RevolverSpin");
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = $"{currentAmmo} / {maxAmmo}";
    }
}
