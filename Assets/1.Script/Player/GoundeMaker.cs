using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GroundMaker : MonoBehaviour
{
    public Tilemap tilemap; // 블럭을 설치할 타일맵
    public Tile blockTile; // 설치할 블럭 타일
    public float maxPlaceDistance = 5f; // 블럭 설치 최대 거리

    // 플레이어가 설치한 블럭을 저장
    private HashSet<Vector3Int> playerPlacedBlocks = new HashSet<Vector3Int>();

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // 우클릭
        {
            HandleBlockPlacementOrRemoval();
        }
    }

    void HandleBlockPlacementOrRemoval()
    {
        // 마우스 위치 가져오기
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // Z축 고정

        // 마우스 위치를 타일맵의 셀 위치로 변환
        Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);

        // 설치 거리 확인
        Vector3 playerPosition = transform.position;
        if (Vector3.Distance(playerPosition, tilemap.CellToWorld(cellPosition)) > maxPlaceDistance)
        {
            Debug.Log("Block placement/removal too far.");
            return;
        }

        // 이미 설치된 블럭이 있는 경우 삭제
        if (playerPlacedBlocks.Contains(cellPosition))
        {
            RemoveBlock(cellPosition);
        }
        else if (!tilemap.HasTile(cellPosition))
        {
            // 설치된 타일이 없는 경우만 블럭 설치
            PlaceBlock(cellPosition);
        }
        else
        {
            Debug.Log("Cannot place block here. Tile already exists.");
        }
    }

    void PlaceBlock(Vector3Int cellPosition)
    {
        tilemap.SetTile(cellPosition, blockTile);
        playerPlacedBlocks.Add(cellPosition); // 플레이어가 설치한 위치 저장
        Debug.Log("Block placed at: " + cellPosition);
    }

    void RemoveBlock(Vector3Int cellPosition)
    {
        tilemap.SetTile(cellPosition, null);
        playerPlacedBlocks.Remove(cellPosition); // 저장된 위치에서 제거
        Debug.Log("Block removed at: " + cellPosition);
    }
}
