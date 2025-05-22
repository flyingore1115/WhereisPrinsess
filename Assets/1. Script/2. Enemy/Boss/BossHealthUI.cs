using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;       // 보스 체력 게이지
    public TMP_Text bossNameText;     // 보스 이름
    public TMP_Text hitCountText;     // 남은 타격 횟수 or HP

    private int maxHP;
    private int currentHP;

    public void InitBossUI(string bossName, int maxHP)
    {
        this.maxHP = maxHP;
        this.currentHP = maxHP;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = maxHP;
        }
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }
        UpdateHPText();
    }

    public void UpdateHP(int hp)
    {
        currentHP = hp;
        if (healthSlider != null)
            healthSlider.value = currentHP;
        
        UpdateHPText();
    }

    private void UpdateHPText()
    {
        if (hitCountText != null)
        {
            hitCountText.text = $"{currentHP} / {maxHP}";
        }
    }
}
