using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이어 사격 로직 및 탄약 수 World-Space UI 표시
/// - 마우스 거리 기반으로 FirePoint 반경이 변하고,
/// - 사격 모드일 때만 FirePoint를 활성화합니다.
/// </summary>
[RequireComponent(typeof(P_Movement))]
public class P_Shooting : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Transform player;

    [Header("Fire Point Distance Settings")]
    [Tooltip("마우스 거리와 비교할 최소 반경")]
    public float minFirePointRadius = 0.5f;
    [Tooltip("마우스 거리와 비교할 최대 반경")]
    public float maxFirePointRadius = 2.0f;

    [Header("Ammo Data")]
    public int currentAmmo = 6;
    public int maxAmmo = 6;

    [Header("Reload")]
    public float reloadEnergyCost;

    [Header("World-Space Ammo UI")]
    [Tooltip("플레이어 근처에 떠서 사라지는 World-Space UI Prefab")]
    public GameObject ammoWorldUIPrefab;
    [Tooltip("World-Space UI 오프셋")]
    public Vector3 worldUIOffset = new Vector3(0f, 1.5f, 0f);
    [Tooltip("World-Space UI 표시 시간 (초)")]
    public float worldUIDisplayDuration = 0.5f;
    [Tooltip("World-Space UI 크기 배수")]
    public float worldUIScale = 1f;

    // 튜토리얼 각도 제한
    private bool tutorialMode = false;
    private Transform tutorialTarget;
    private float allowedAngle = 15f;

    void Update()
    {
        // 사격 모드일 때만 FirePoint 활성화
        bool shootingMode = Player.Instance != null && Player.Instance.IsShootingMode;
        if (firePoint != null)
            firePoint.gameObject.SetActive(shootingMode);

        UpdateFirePointPosition();
    }

    /// <summary>
    /// 튜토리얼 모드 설정 (StorySceneManager 호출용)
    /// </summary>
    public void SetTutorialMode(bool active)
    {
        tutorialMode = active;
        if (!active)
            tutorialTarget = null;
    }

    /// <summary>
    /// HUD나 World UI 갱신 (StorySceneManager 호출용)
    /// </summary>
    public void UpdateAmmoUI()
    {
        ShowWorldAmmoUI();
    }

    /// <summary>
    /// 각도 제한 모드 활성화
    /// </summary>
    public void EnableAngleLimit(Transform target, float angleDeg)
    {
        tutorialMode = true;
        tutorialTarget = target;
        allowedAngle = angleDeg;
    }

    /// <summary>
    /// 각도 제한 모드 비활성화
    /// </summary>
    public void DisableAngleLimit()
    {
        tutorialMode = false;
        tutorialTarget = null;
    }

    /// <summary>
    /// 사격 및 재장전 입력 처리
    /// </summary>
    public void HandleShooting()
    {
        if (MySceneManager.IsStoryScene && !tutorialMode)
            return;
        if (Player.Instance.holdingPrincess)
            return;

        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;
        Vector2 dir = (wp - firePoint.position).normalized;

        if (Input.GetMouseButtonDown(0))
        {
            ShootBullet(dir);
        }
    }

    /// <summary>
    /// 실제 탄발사 처리
    /// </summary>
    private void ShootBullet(Vector2 dir)
    {
        if (currentAmmo <= 0)
        {
            SoundManager.Instance?.PlaySFX("EmptyGunSound");
            return;
        }

        // 튜토리얼 각도 체크
        if (tutorialMode && tutorialTarget != null)
        {
            float angle = Vector2.Angle((tutorialTarget.position - firePoint.position), dir);
            if (angle > allowedAngle)
            {
                SoundManager.Instance?.PlaySFX("EmptyGunSound");
                return;
            }
        }

        // 탄 생성
        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        var bs = b.GetComponent<Bullet>();
        if (bs != null) bs.SetDirection(dir);
        SoundManager.Instance?.PlaySFX("PlayerGunSound");

        // 탄약 갱신
        currentAmmo = Mathf.Max(0, currentAmmo - 1);

        // World-Space UI 표시
        ShowWorldAmmoUI();
    }

    /// <summary>
    /// 월드 공간에서 탄약 UI를 띄웁니다.
    /// </summary>
    private void ShowWorldAmmoUI()
    {
        if (ammoWorldUIPrefab == null || player == null)
            return;
        var go = Instantiate(ammoWorldUIPrefab);
        go.transform.localScale = Vector3.one * worldUIScale;
        var ui = go.GetComponent<WorldAmmoUI>();
        if (ui != null)
        {
            ui.Init(currentAmmo, maxAmmo, player, worldUIOffset, worldUIDisplayDuration);
        }
    }

    /// <summary>
    /// 재장전 처리
    /// </summary>
    public void Reload()
    {
         if (currentAmmo >= maxAmmo) return;  // 이미 가득 차 있음
         
        var tsc = TimeStopController.Instance;
        if (!tsc.TrySpendGauge(reloadEnergyCost))
        {
            Debug.Log("에너지 부족! 재장전 불가");
            return;
        }

        currentAmmo = maxAmmo;
        SoundManager.Instance?.PlaySFX("RevolverSpin");

        // 재장전 후에도 UI로 표시
        ShowWorldAmmoUI();
    }

    /// <summary>
    /// 파이어포인트 위치 및 회전 업데이트
    /// </summary>
    private void UpdateFirePointPosition()
    {
        if (player == null || firePoint == null) return;

        Vector3 mp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mp.z = 0f;
        Vector2 dir = (mp - player.position).normalized;

        // 마우스 거리 기반 동적 반경 계산
        float dist = Vector2.Distance(mp, player.position);
        float radius = Mathf.Clamp(dist, minFirePointRadius, maxFirePointRadius);
        firePoint.position = player.position + (Vector3)dir * radius;

        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, ang);
    }
}
