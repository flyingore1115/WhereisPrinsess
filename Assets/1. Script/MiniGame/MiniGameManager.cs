using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance { get; private set; }

    private ArrowInputMiniGame activeGame;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        Debug.Log("[MiniGameManager] 자동 테스트 실행");
        PlayArrowInput(
            () => Debug.Log("🎉 성공"),
            () => Debug.Log("💥 실패"),
            4, 8f
        );
    }

    public void PlayArrowInput(System.Action onSuccess, System.Action onFailure,
                               int length = 4, float limit = 5f)
    {
        if (activeGame != null) return;

        var prefab = Resources.Load<GameObject>("Prefab/ArrowInputMiniGameUI");
        var obj = Instantiate(prefab);
        activeGame = obj.GetComponent<ArrowInputMiniGame>();

        activeGame.sequenceLength = length;
        activeGame.timeLimit = limit;
        activeGame.onSuccess = () => { onSuccess?.Invoke(); activeGame = null; };
        activeGame.onFailure = () => { onFailure?.Invoke(); activeGame = null; };

        activeGame.Begin();
    }
}
