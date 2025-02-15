using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GroundMaker : MonoBehaviour
{
    public Tilemap tilemap; // 블럭을 설치할 타일맵
    public Tile blockTile; // 설치할 블럭 타일
    public float maxPlaceDistance = 5f; // 블럭 설치 최대 거리
    public int maxBlocks = 5; // 플레이어가 설치할 수 있는 최대 블럭 수
    public float blockLifetime = 10f; // 블럭이 유지되는 시간(초)

    // 플레이어가 설치한 블럭 정보 저장
    private Dictionary<Vector3Int, float> playerPlacedBlocks = new Dictionary<Vector3Int, float>();

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // 우클릭: 블럭 설치
        {
            HandleBlockPlacement();
        }

        if (Input.GetMouseButtonDown(0)) // 좌클릭: 블럭 제거
        {
            HandleBlockRemoval();
        }

        // 블럭 자동 파괴 관리
        UpdateBlockLifetime();
    }

    void HandleBlockPlacement()
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
            Debug.Log("Block placement too far.");
            return;
        }

        // 이미 설치된 블럭이 있는 경우
        if (playerPlacedBlocks.ContainsKey(cellPosition))
        {
            Debug.Log("Block already exists at: " + cellPosition);
            return;
        }

        // 블럭 개수 제한 확인
        if (playerPlacedBlocks.Count >= maxBlocks)
        {
            Debug.Log("Maximum block limit reached.");
            return;
        }

        // 타일맵에 타일이 없는 경우만 블럭 설치
        if (!tilemap.HasTile(cellPosition))
        {
            PlaceBlock(cellPosition);
        }
        else
        {
            Debug.Log("Cannot place block here. Tile already exists.");
        }
    }

    void HandleBlockRemoval()
    {
        // 마우스 위치 가져오기
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // Z축 고정

        // 마우스 위치를 타일맵의 셀 위치로 변환
        Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);

        // 플레이어가 설치한 블럭인지 확인 후 제거
        if (playerPlacedBlocks.ContainsKey(cellPosition))
        {
            tilemap.SetTile(cellPosition, null);
            playerPlacedBlocks.Remove(cellPosition);
            Debug.Log("Block removed at: " + cellPosition);
        }
        else
        {
            Debug.Log("No removable block at: " + cellPosition);
        }
    }

    void PlaceBlock(Vector3Int cellPosition)
    {
        tilemap.SetTile(cellPosition, blockTile);
        playerPlacedBlocks[cellPosition] = Time.time; // 설치 시간 기록
        Debug.Log("Block placed at: " + cellPosition);
    }

    void UpdateBlockLifetime()
    {
        List<Vector3Int> expiredBlocks = new List<Vector3Int>();

        foreach (var block in playerPlacedBlocks)
        {
            if (Time.time - block.Value >= blockLifetime)
            {
                expiredBlocks.Add(block.Key); // 수명이 지난 블럭을 기록
            }
        }

        // 수명이 지난 블럭 제거
        foreach (var cellPosition in expiredBlocks)
        {
            tilemap.SetTile(cellPosition, null);
            playerPlacedBlocks.Remove(cellPosition);
            Debug.Log("Block removed at: " + cellPosition);
        }
    }
}
