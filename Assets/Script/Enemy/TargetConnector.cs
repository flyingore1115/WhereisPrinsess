using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class TargetConnector : MonoBehaviour
{
    public static TargetConnector Instance;

    private LineRenderer lineRenderer;

    [Tooltip("선이 그려질 때 적 위치에서 위로 띄울 오프셋")]
    public float verticalOffset = 1.0f;

    [Tooltip("선의 두께")]
    public float lineWidth = 0.1f;

    [Tooltip("선의 색상")]
    public Color lineColor = Color.white;

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

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true; // 2D 월드좌표 사용
        lineRenderer.positionCount = 0;
        lineRenderer.widthMultiplier = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.sortingOrder = 100;
    }

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
                pos.y += verticalOffset;
                lineRenderer.SetPosition(i, pos);
            }
        }
    }
}
