using UnityEngine;
using UnityEngine.UI;

public class WeaponChargeUI : MonoBehaviour
{
    public Slider chargeSlider;

    public void SetRatio(float t)
    {
        chargeSlider.value = t;
    }

    public void Show()  => gameObject.SetActive(true);
    public void Hide()  => gameObject.SetActive(false);
    public void ResetUI()
    {
        chargeSlider.value = 0;
        Hide();
    }
}
