using UnityEngine;
using TMPro;

public class FontChangerTMP : MonoBehaviour
{
    public TMP_FontAsset newFont; // 새로 적용할 TextMeshPro 폰트

    void Start()
    {
        TextMeshProUGUI[] textComponents = GetComponentsInChildren<TextMeshProUGUI>();

        foreach (TextMeshProUGUI text in textComponents)
        {
            text.font = newFont;
        }
    }
}
