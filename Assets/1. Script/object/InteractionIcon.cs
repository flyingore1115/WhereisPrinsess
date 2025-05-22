// InteractionIcon.cs
using UnityEngine;

public class InteractionIcon : MonoBehaviour
{
    GameObject iconInstance;
    Vector3    iconOffset = new Vector3(1f, 1f, 1f);

    [Tooltip("아이콘 스케일")] 
    public float iconScale = 0.1f;    

    void Awake()
    {
        var prefab = Resources.Load<GameObject>("Prefab/E_Icon");
        iconInstance = Instantiate(prefab, transform.position + iconOffset, Quaternion.identity);
        iconInstance.transform.localScale = Vector3.one * iconScale;
        iconInstance.SetActive(false);
    }

    void LateUpdate()
    {
        // 파괴된 상태라면 (iconInstance == null 이 true) 이 블록은 아예 안 들어감
        if (iconInstance)
            iconInstance.transform.position = transform.position + iconOffset;
    }

    public void Show()
    {
        // UnityEngine.Object 의 == 연산자가 override 되어,
        // 파괴된 오브젝트도 null 체크에 걸립니다.
        if (iconInstance)
            iconInstance.SetActive(true);
    }

    public void Hide()
    {
        if (iconInstance)
            iconInstance.SetActive(false);
    }

    void OnDestroy()
    {
        // 호스트가 파괴될 때 iconInstance 도 같이 정리해주면 더 깔끔합니다
        if (iconInstance)
        {
            Destroy(iconInstance);
            iconInstance = null;
        }
    }
}
