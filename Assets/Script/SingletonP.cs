using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonP : MonoBehaviour
{
    void Start(){
        DontDestroyOnLoad(gameObject);
    }
}
