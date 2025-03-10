using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyGame
{
    public class TimePointManager : MonoBehaviour
    {
        private static TimePointManager instance;
        public static TimePointManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("TimePointManager");
                    instance = obj.AddComponent<TimePointManager>();
                    DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }

        private TimePointData lastCheckpointData = null;
        private bool hasCheckpoint = false;

        public float rewindDuration = 0.5f;  // 되감기 연출 시간
        public GameObject restartPrompt;     // "아무 키나 누르면 시작" UI
        public AudioClip rewindSfx;          // 되감기 효과음
        private AudioSource audioSource;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            if (restartPrompt != null)
                restartPrompt.SetActive(false);
        }

        // 체크포인트 저장: 현재 공주와 플레이어의 위치 및 적 상태를 저장합니다.
        public void SaveCheckpoint(Vector2 princessPos, Vector2 playerPos)
        {
            TimePointData data = new TimePointData();
            data.princessPosition = princessPos;
            data.playerPosition = playerPos;

            BaseEnemy[] enemies = FindObjectsOfType<BaseEnemy>();
            foreach (BaseEnemy enemy in enemies)
            {
                if (enemy.gameObject.activeInHierarchy)
                {
                    EnemyStateData esd = new EnemyStateData();
                    esd.enemyType = enemy.gameObject.name.Split('(')[0].Trim();
                    esd.position = enemy.transform.position;
                    data.enemyStates.Add(esd);
                }
            }

            lastCheckpointData = data;
            hasCheckpoint = true;

            SaveLoadManager.PointCheck(data);

            Debug.Log($"[TimePointManager] 체크포인트 저장 완료: 공주 {princessPos}, 플레이어 {playerPos}, 적 {data.enemyStates.Count}마리");
            if (StatusTextManager.Instance != null)
                StatusTextManager.Instance.ShowMessage("되감기 포인트 지정됨");
        }

        public void SetCheckpointData(TimePointData data)
        {
            lastCheckpointData = data;
            hasCheckpoint = true;
        }

        public bool HasCheckpoint()
        {
            return hasCheckpoint;
        }

        // ApplyCheckpoint: 체크포인트 데이터를 적용하는 코루틴 (외부 호출용)
        public IEnumerator ApplyCheckpoint(TimePointData checkpointData, bool waitForInput)
        {
            lastCheckpointData = checkpointData;
            hasCheckpoint = true;

            Princess princess = FindObjectOfType<Princess>();
            Player player = FindObjectOfType<Player>();

            if (princess != null)
                princess.transform.position = checkpointData.princessPosition;
            if (player != null)
                player.RestoreFromRewind(checkpointData.playerPosition);

            BaseEnemy[] currentEnemies = FindObjectsOfType<BaseEnemy>();
            foreach (var e in currentEnemies)
                Destroy(e.gameObject);

            foreach (EnemyStateData esd in checkpointData.enemyStates)
            {
                GameObject enemyPrefab = Resources.Load<GameObject>($"Prefab/Enemies/{esd.enemyType}");
                if (enemyPrefab != null)
                {
                    Vector2 spawnPosition = GetGroundPosition(esd.position);
                    GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                    Rigidbody2D rb = newEnemy.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.simulated = false;
                        yield return new WaitForSeconds(0.1f);
                        rb.simulated = true;
                    }
                }
                else
                {
                    Debug.LogWarning($"적 프리팹({esd.enemyType})을 찾을 수 없음");
                }
            }

            Debug.Log("[TimePointManager] 체크포인트 적용 완료.");

            if (waitForInput)
            {
                if (StatusTextManager.Instance != null)
                    StatusTextManager.Instance.ShowMessage("아무 키나 누르면 시작");

                Time.timeScale = 0f;
                while (!Input.anyKeyDown)
                {
                    yield return null;
                }
                Time.timeScale = 1f;
                if (StatusTextManager.Instance != null)
                    StatusTextManager.Instance.ShowMessage("");
            }
        }

        // ImmediateRevive: 세이브 포인트 도착 시, 즉시 플레이어 부활 처리 (체력 복원 및 위치 강제 보정)
        public void ImmediateRevive()
        {
            if (!hasCheckpoint || lastCheckpointData == null)
            {
                Debug.LogWarning("[TimePointManager] 체크포인트가 없습니다.");
                return;
            }

            Princess princess = FindObjectOfType<Princess>();
            Player player = FindObjectOfType<Player>();
            if (princess == null || player == null)
            {
                Debug.LogWarning("[TimePointManager] 부활 대상(공주 또는 플레이어)을 찾을 수 없음!");
                return;
            }

            // 플레이어 입력 차단
            player.ignoreInput = true;

            // 플레이어를 공주의 현재 위치로 강제 이동
            Vector2 targetPos = princess.transform.position;
            player.transform.position = targetPos;
            Debug.Log($"[TimePointManager] Force position => 공주 위치: {princess.transform.position}, 플레이어를 {targetPos}로 설정");

            // 부활 처리: PlayerOver.OnRewindComplete() 호출 (체력 복원 등)
            PlayerOver playerOver = FindObjectOfType<PlayerOver>();
            if (playerOver != null && playerOver.IsDisabled)
            {
                playerOver.OnRewindComplete(targetPos);
                Debug.Log("[TimePointManager] ImmediateRevive: 플레이어 부활 처리 완료.");
            }

            // 공주 정지 처리: 공주가 움직이지 않도록 isControlled = true
            princess.isControlled = true;
            Rigidbody2D prb = princess.GetComponent<Rigidbody2D>();
            if (prb != null)
            {
                prb.velocity = Vector2.zero;
                Debug.Log("[TimePointManager] 공주 정지 처리 완료.");
            }

            // 카메라 타겟 전환: 부활 후 카메라가 플레이어를 따라가도록
            CameraFollow cf = FindObjectOfType<CameraFollow>();
            if (cf != null)
            {
                cf.SetTarget(player.gameObject);
                Debug.Log("[TimePointManager] 카메라 타겟을 플레이어로 전환.");
            }

            // 부활 후 새 체크포인트 저장 (현재 위치 기준)
            SaveCheckpoint(princess.transform.position, player.transform.position);
        }

        // RewindToCheckpoint: 기존 되감기 로직을 실행합니다.
        public void RewindToCheckpoint()
        {
            if (!hasCheckpoint || lastCheckpointData == null)
            {
                Debug.LogWarning("[TimePointManager] 체크포인트가 없습니다.");
                return;
            }
            StartCoroutine(RewindCoroutine());
        }

        // RewindCoroutine: 전체 되감기 프로세스를 수행합니다.
        private IEnumerator RewindCoroutine()
        {
            Debug.Log("[TimePointManager] 되감기 시작...");
            Princess princess = FindObjectOfType<Princess>();
            Player player = FindObjectOfType<Player>();
            if (princess == null || player == null)
            {
                Debug.LogWarning("[TimePointManager] 공주 또는 플레이어를 찾을 수 없음!");
                yield break;
            }

            princess.transform.position = lastCheckpointData.princessPosition;
            player.transform.position = lastCheckpointData.playerPosition;
            Debug.Log($"[TimePointManager] 위치 보정 완료: 공주 {lastCheckpointData.princessPosition}, 플레이어 {lastCheckpointData.playerPosition}");

            BaseEnemy[] currentEnemies = FindObjectsOfType<BaseEnemy>();
            foreach (var e in currentEnemies)
                Destroy(e.gameObject);
            foreach (EnemyStateData esd in lastCheckpointData.enemyStates)
            {
                GameObject enemyPrefab = Resources.Load<GameObject>($"Prefab/Enemies/{esd.enemyType}");
                if (enemyPrefab != null)
                {
                    Vector2 spawnPosition = GetGroundPosition(esd.position);
                    Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                }
                else
                {
                    Debug.LogWarning($"적 프리팹({esd.enemyType})을 찾을 수 없음");
                }
            }

            Debug.Log("[TimePointManager] 되감기 완료.");

            ImmediateRevive();

            if (StatusTextManager.Instance != null)
                StatusTextManager.Instance.ShowMessage("아무 키나 누르면 시작");
            Time.timeScale = 0f;
            while (!Input.anyKeyDown)
            {
                yield return null;
            }
            Time.timeScale = 1f;
            if (StatusTextManager.Instance != null)
                StatusTextManager.Instance.ShowMessage("");

            PlayerOver playerOver = FindObjectOfType<PlayerOver>();
            if (playerOver != null)
            {
                playerOver.ResumeAfterRewind();
                Debug.Log("[TimePointManager] 부활 후 상태 복구 완료.");
            }
        }

        // 적의 바닥 위치 보정 함수
        private Vector2 GetGroundPosition(Vector2 originalPosition)
        {
            RaycastHit2D hit = Physics2D.Raycast(originalPosition, Vector2.down, 5f, LayerMask.GetMask("Ground"));
            if (hit.collider != null)
            {
                return new Vector2(originalPosition.x, hit.point.y + 0.1f);
            }
            return originalPosition;
        }

        public TimePointData GetLastCheckpointData()
        {
            return lastCheckpointData;
        }
    }
}
