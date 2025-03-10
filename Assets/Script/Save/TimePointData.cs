using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    [Serializable]
    public class EnemyStateData
    {
        public string enemyType;
        public Vector2 position;
    }

    [Serializable]
    public class TimePointData
    {
        public Vector2 princessPosition;
        public Vector2 playerPosition;
        public List<EnemyStateData> enemyStates = new List<EnemyStateData>();
    }

    [Serializable]
    public class GameStateData
    {
        public TimePointData checkpointData;
        public int playerBulletCount;
        public float playerGauge;
        public List<string> unlockedSkills;
    }

    [Serializable]
    public class TimeSnapshot
    {
        public Vector2 princessPosition;
        public Vector2 playerPosition;
    }
}
