using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyStateData
{
    public string enemyType;  // 예: 프리팹 이름
    public Vector2 position;  // 적 위치
}

[Serializable]
public class GameStateData
{
    public TimePointData checkpointData;
    public int playerBulletCount;
    public float playerGauge;
    public List<string> unlockedSkills;
}

[System.Serializable]
public class TimePointData
{
    public Vector2 princessPosition;
    public Vector2 playerPosition;
    public List<EnemyStateData> enemyStates = new List<EnemyStateData>();
}

