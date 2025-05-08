using UnityEngine;
using System.Collections.Generic;


public class Lady : MonoBehaviour
{
    [Tooltip("던질 객체(케이크, 빵) 프리팹들")]
    public GameObject[] throwPrefabs;
    [Tooltip("던질 위치 포인트(3개 지정)")]
    public Transform[] spawnPoints;
    [Tooltip("던지기 힘(임펄스)")]
    public float throwForce = 5f;

    private int clickCount = 0;

    // 생성된 오브젝트 목록
    [HideInInspector]
    public List<GameObject> spawned = new List<GameObject>();

    public void StartThrowing()
    {
        spawned.Clear();
        int count = Mathf.Min(throwPrefabs.Length, spawnPoints.Length);
        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(throwPrefabs[i], spawnPoints[i].position, Quaternion.identity);
            // 물리 던지기
            if (obj.TryGetComponent<Rigidbody2D>(out var rb))
                rb.AddForce(spawnPoints[i].up * throwForce, ForceMode2D.Impulse);

            // 클릭용 태그 지정 (Inspector에서 'TutorialTarget' 태그 만들어서 할당)
            obj.tag = "TutorialTarget";

            var tut = obj.AddComponent<ThrowableTutorialTarget>();
            tut.master = this;

            spawned.Add(obj);
        }
    }

    public void RegisterClick(GameObject obj)
{
    clickCount++;
}
}
