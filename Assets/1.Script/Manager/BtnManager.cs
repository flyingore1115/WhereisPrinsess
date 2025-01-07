using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnManager : MonoBehaviour
{
    void OnEnable()
    {
        transform.SetAsLastSibling(); // 현재 UI 요소를 가장 앞으로 이동
    }
}
