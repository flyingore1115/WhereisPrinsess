using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    // 하트 이미지 배열: 인스펙터에서 3개의 하트 Image를 할당
    public Image[] heartImages; 
    public Sprite fullHeart;  // 채워진 하트
    public Sprite emptyHeart; // 비어있는 하트

    // 현재 체력과 최대 체력을 전달받아 하트 UI를 업데이트하는 함수
    public void UpdateHearts(int currentHealth, int maxHealth)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHealth)
            {
                heartImages[i].sprite = fullHeart;
            }
            else
            {
                heartImages[i].sprite = emptyHeart;
            }
        }
    }
}
