using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame;

public static class SaveLoadManager
{
    private static string filePath = System.IO.Path.Combine(Application.persistentDataPath, "checkpoint.json");

    // 기존: PointCheck(TimePointData data)
// 변경: PointCheck(GameStateData data)
    public static void PointCheck(GameStateData data)
    {
        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(filePath, json);
        Debug.Log($"[SaveLoadManager] 체크포인트 저장(GameStateData): {filePath}");
    }

    // 기존: bool LoadCheckpoint(out TimePointData data)
    // 변경: bool LoadCheckpoint(out GameStateData data)
    public static bool LoadCheckpoint(out GameStateData data)
    {
        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            data = JsonUtility.FromJson<GameStateData>(json);
            Debug.Log("[SaveLoadManager] 체크포인트(GameStateData) 불러오기 성공");
            return true;
        }
        data = null;
        Debug.LogWarning("[SaveLoadManager] 저장된 체크포인트가 없습니다.");
        return false;
    }


    public static void DeleteCheckpoint()
    {
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            Debug.Log("[SaveLoadManager] 체크포인트 삭제");
        }
    }
}
