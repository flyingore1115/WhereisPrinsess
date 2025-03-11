using UnityEngine;
using System.Collections;
using TMPro;

public class P_Shooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int currentAmmo = 6;
    public int maxAmmo = 6;
    public TMP_Text ammoText;

    public float reloadEnergyCost = 10f; // 재장전 시 소모량

    // 총알 UI 관련 변수
    [Tooltip("총알 UI가 보여지는 시간 (초)")]
    public float bulletUIDisplayDuration = 0.5f;
    [Tooltip("총알 UI 오브젝트 (평소에는 비활성화)")]
    public GameObject bulletUI;

    private Coroutine hideBulletUICoroutine;

    void Update()
    {
        HandleShooting();

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    public void HandleShooting()
    {
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

        // 총알 UI 활성화
        if (bulletUI != null)
        {
            bulletUI.SetActive(true);
            // 기존 코루틴이 있다면 중지 후 새 코루틴 시작
            if (hideBulletUICoroutine != null)
            {
                StopCoroutine(hideBulletUICoroutine);
            }
            hideBulletUICoroutine = StartCoroutine(HideBulletUIAfterDelay());
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("PlayerGunSound");
        }

        currentAmmo--;
        UpdateAmmoUI();
    }

    private IEnumerator HideBulletUIAfterDelay()
    {
        yield return new WaitForSecondsRealtime(bulletUIDisplayDuration);
        if (bulletUI != null)
        {
            bulletUI.SetActive(false);
        }
    }

    // 게임오버나 되감기 시 외부에서 호출할 수 있는 총알 UI 숨기기 함수
    public void HideBulletUI()
    {
        if (hideBulletUICoroutine != null)
        {
            StopCoroutine(hideBulletUICoroutine);
            hideBulletUICoroutine = null;
        }
        if (bulletUI != null)
        {
            bulletUI.SetActive(false);
        }
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
