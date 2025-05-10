// InteractionIcon.cs
using UnityEngine;
public class InteractionIcon : MonoBehaviour
{
    GameObject iconInstance;
    Vector3 iconOffset = new Vector3(1f, 1f, 1f);

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
        if (iconInstance != null)
            iconInstance.transform.position = transform.position + iconOffset;
    }

    public void Show() => iconInstance?.SetActive(true);
    public void Hide() => iconInstance?.SetActive(false);
}
