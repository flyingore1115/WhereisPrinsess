using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    [Serializable]
    public class EnemyStateData
    {
        public string enemyType;    // prefabName
        public int enemyID;         // 고유 ID
        public Vector2 position;    // 위치
        public Vector2 localScale;  // 로컬 스케일

        public int health; //체력
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
        public int playerHealth;
        public float playerTimeEnergy;
        public int   playerGaugeStacks; 

    }

    [Serializable]
    public class TimeSnapshot
    {
        public Vector2 princessPosition;
        public Vector2 playerPosition;
        public Vector2 playerVelocity;
        public Vector2 princessVelocity;

        public List<string> unlockedSkills;

        // 애니메이션 상태
        public string playerAnimationState;
        public float playerNormalizedTime;
        public string princessAnimationState;
        public float princessNormalizedTime;

        
    }
}
