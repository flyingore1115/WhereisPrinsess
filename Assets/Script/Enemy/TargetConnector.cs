using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class TargetConnector : MonoBehaviour
{
    public static TargetConnector Instance;

    public Material lineMaterial;

    private LineRenderer lineRenderer;

    [Tooltip("선이 그려질 때, UI가 있다면 UI의 중앙 위치를 사용하고, 없으면 적 위치에서 위로 띄울 오프셋")]
    public float verticalOffset = 1.0f;

    [Tooltip("선의 두께")]
    public float lineWidth = 0.1f;

    [Tooltip("선의 색상")]
    //public Color lineColor = Color.white;

    void Awake()
    {
        // 싱글톤 체크
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환해도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 중복 방지
            return;
        }
        
        gameObject.layer = LayerMask.NameToLayer("line");

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true; // 2D 월드좌표 사용
        lineRenderer.positionCount = 0;
        lineRenderer.widthMultiplier = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        //lineRenderer.startColor = lineColor;
        //lineRenderer.endColor = lineColor;
        lineRenderer.sortingOrder = 100;

        if (lineMaterial != null)
        lineRenderer.material = lineMaterial;

        
    }

    /// <summary>
    /// 선택된 적 리스트를 받아 선을 갱신합니다.
    /// 각 적의 순서번호 UI가 있으면 그 위치를, 없으면 기본 오프셋을 사용합니다.
    /// </summary>
    public void UpdateLine(List<BaseEnemy> enemies)
    {
        if (enemies == null || enemies.Count == 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = enemies.Count;
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null)
            {
                Vector3 pos = enemies[i].transform.position;

                // 적의 자식에 Canvas가 있다면(순서번호 UI가 생성되어 있다면) 그 위치를 사용
                Canvas uiCanvas = enemies[i].GetComponentInChildren<Canvas>();
                if (uiCanvas != null)
                {
                    pos = uiCanvas.transform.position;
                }
                else
                {
                    pos.y += verticalOffset;
                }
                lineRenderer.SetPosition(i, pos);
            }
        }
    }
}
