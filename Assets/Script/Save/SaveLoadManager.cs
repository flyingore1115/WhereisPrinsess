using UnityEngine;
using System.IO;

public static class SaveLoadManager
{
    private static string filePath = Path.Combine(Application.persistentDataPath, "checkpoint.json");

    public static void PointCheck(TimePointData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
        Debug.Log($"[SaveLoadManager] 체크포인트 저장: {filePath}");
    }

    public static bool LoadCheckpoint(out TimePointData data)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<TimePointData>(json);
            Debug.Log("[SaveLoadManager] 체크포인트 불러오기 성공");
            return true;
        }
        data = null;
        Debug.LogWarning("[SaveLoadManager] 저장된 체크포인트가 없습니다.");
        return false;
    }

    public static void DeleteCheckpoint()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("[SaveLoadManager] 체크포인트 삭제");
        }
    }
}
