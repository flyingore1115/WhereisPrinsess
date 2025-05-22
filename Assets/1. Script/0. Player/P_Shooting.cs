using UnityEngine;
using System.Collections;
using TMPro;

public class P_Shooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Transform player;
    public float firePointRadius = 1.0f;

    public int currentAmmo = 6;
    public int maxAmmo = 6;
    public TMP_Text ammoText;

    public float reloadEnergyCost;
    [Tooltip("총알 UI가 보여지는 시간 (초)")]
    public float bulletUIDisplayDuration = 0.5f;
    [Tooltip("총알 UI 오브젝트 (평소에는 비활성화)")]
    public GameObject bulletUI;

    // 튜토리얼 각도 제한
    private bool tutorialMode = false;
    private Transform tutorialTarget;
    private float allowedAngle = 15f;

    private Coroutine hideBulletUICoroutine;


    void Awake()          // 탄 UI 자동 연결(인스펙터 미설정 대비)
    {
        if (ammoText == null && CanvasManager.Instance != null)
            ammoText = CanvasManager.Instance.bulletText;
        if (bulletUI == null && CanvasManager.Instance != null)
            bulletUI = CanvasManager.Instance.bulletUI;
 }

    void Update()
    {
        UpdateFirePointPosition();
    }

    public void EnableAngleLimit(Transform target, float angleDeg)
    {
        tutorialMode = true;
        tutorialTarget = target;
        allowedAngle = angleDeg;
    }

    public void DisableAngleLimit()
    {
        tutorialMode = false;
        tutorialTarget = null;
    }


    // 반드시 public으로 정의된 HandleShooting() 메서드
    public void HandleShooting()
    {
        if (MySceneManager.IsStoryScene && !tutorialMode)
            return;
        if (Player.Instance.holdingPrincess)
            return;

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPoint.z = 0f;
        Vector2 direction = (worldPoint - firePoint.position).normalized;

        if (Input.GetMouseButtonDown(0))
        {
            ShootBullet(direction);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Reload();
        }
    }

    private void ShootBullet(Vector2 dir)
    {
        if (currentAmmo <= 0)
        {
            SoundManager.Instance?.PlaySFX("EmptyGunSound");
            return;
        }

        // ▸ 튜토리얼 각도 체크
        if (tutorialMode && tutorialTarget != null)
        {
            float ang = Vector2.Angle((tutorialTarget.position - firePoint.position), dir);
            if (ang > allowedAngle)
            {   // 빗맞음 처리
                SoundManager.Instance?.PlaySFX("EmptyGunSound");
                return;
            }
        }

        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        var bs = b.GetComponent<Bullet>();
        if (bs != null) bs.SetDirection(dir);
        SoundManager.Instance?.PlaySFX("PlayerGunSound");
        currentAmmo--;
        UpdateAmmoUI();

        if (bulletUI != null)
    {
        bulletUI.SetActive(true);
        UpdateAmmoUI();
        if (hideBulletUICoroutine != null)
                StopCoroutine(hideBulletUICoroutine);
        hideBulletUICoroutine = StartCoroutine(HideBulletUIAfterDelay());
    }
    }


    private void UpdateFirePointPosition()
    {
        if (player == null || firePoint == null) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector2 direction = (mousePosition - player.position).normalized;
        firePoint.position = player.position + (Vector3)direction * firePointRadius;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
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
            UpdateAmmoUI();
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
        HideBulletUI();
    }

    public void HideBulletUI()
    {
        if (bulletUI != null)
        {
            bulletUI.SetActive(false);
            Debug.Log("불렛UI 비활성화함+++++++++++++++++++++");
        }
    }

    private void Reload()
    {
        var tsc = TimeStopController.Instance;

        if (!tsc.TrySpendGauge(reloadEnergyCost))
        {
            Debug.Log("에너지 부족! 재장전 불가");
            return;
        }

        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        SoundManager.Instance?.PlaySFX("RevolverSpin");
    }

    public void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = $"{currentAmmo} / {maxAmmo}";
    }
    public void SetTutorialMode(bool active)
 {
     tutorialMode = active;
     if (!active) tutorialTarget = null;   // 종료 시 각도 제한도 해제
 }
}
