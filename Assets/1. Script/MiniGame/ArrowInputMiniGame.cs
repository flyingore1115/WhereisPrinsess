using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ArrowInputMiniGame : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Transform sequencePanel;
    [SerializeField] private Slider    timerSlider;
    [SerializeField] private TMP_Text  promptText;
    [SerializeField] private GameObject arrowSlotPrefab;

    [Header("게임 설정")]
    public float timeLimit      = 8f;
    public int   sequenceLength = 4;

    [Header("레이아웃")]
    public float slotSpacing = 80f;
    public float slotScale   = 1f;

    public System.Action onSuccess;
    public System.Action onFailure;

    /* ───────────── 내부 상태 ───────────── */
    readonly KeyCode[] arrowKeys =
        { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow };

    readonly Dictionary<KeyCode, KeyCode> wasdAlias = new()
    {
        { KeyCode.A, KeyCode.LeftArrow }, { KeyCode.D, KeyCode.RightArrow },
        { KeyCode.W, KeyCode.UpArrow },  { KeyCode.S, KeyCode.DownArrow }
    };

    List<KeyCode> sequence;
    List<GameObject> slotList = new();
    int   inputIndex;
    float timeLeft;
    bool  isRunning;
    bool  prevIgnore;

    /* ───────────── API ───────────── */
    public void Begin()
    {
        if (Player.Instance != null)
        { prevIgnore = Player.Instance.ignoreInput; Player.Instance.ignoreInput = true; }

        GenerateSequence();
        BuildUI();

        timeLeft   = timeLimit;
        inputIndex = 0;
        isRunning  = true;

        timerSlider.maxValue = timeLimit;
        timerSlider.value    = timeLimit;
    }

    /* ───────────── Update ───────────── */
    void Update()
    {
        if (!isRunning) return;

        /* 타이머 */
        timeLeft -= Time.deltaTime;
        timerSlider.value = timeLeft;
        if (timeLeft <= 0f) { End(false); return; }

        /* 입력 */
        KeyCode expected = sequence[inputIndex];
        KeyCode pressed  = GetPressedKey();
        if (pressed == KeyCode.None) return;

        if (pressed == expected)
        {
            MarkArrowAsCorrect(inputIndex);
            inputIndex++;
            if (inputIndex >= sequence.Count) End(true);
        }
        else
        {
            End(false);
        }
    }

    /* ───────────── UI 구축 ───────────── */
    void GenerateSequence()
    {
        sequence = new();
        for (int i = 0; i < sequenceLength; i++)
            sequence.Add(arrowKeys[Random.Range(0, arrowKeys.Length)]);
    }

    void BuildUI()
    {
        foreach (Transform c in sequencePanel) Destroy(c.gameObject);
        slotList.Clear();

        float startX = -(sequence.Count - 1) * slotSpacing * 0.5f;

        for (int i = 0; i < sequence.Count; i++)
        {
            var slot = Instantiate(arrowSlotPrefab, sequencePanel);
            slotList.Add(slot);

            var txt = slot.GetComponentInChildren<TMP_Text>();
            if (txt) txt.text = KeyToArrowSymbol(sequence[i]);

            var rt = slot.GetComponent<RectTransform>();
            rt.localScale       = Vector3.one * slotScale;
            rt.anchoredPosition = new Vector2(startX + i * slotSpacing, 0f);
        }
    }

    void MarkArrowAsCorrect(int idx)
    {
        var img = slotList[idx].GetComponent<Image>();
        img.color = Color.green;
    }

    /* ───────────── 종료 ───────────── */
    void End(bool success)
    {
        isRunning = false;
        if (Player.Instance) Player.Instance.ignoreInput = prevIgnore;

        if (success) onSuccess?.Invoke(); else onFailure?.Invoke();
        Destroy(gameObject);
    }

    /* ───────────── 입력 유틸 ───────────── */
    KeyCode GetPressedKey()
    {
        foreach (var k in arrowKeys)
            if (Input.GetKeyDown(k)) return k;

        foreach (var pair in wasdAlias)
            if (Input.GetKeyDown(pair.Key)) return pair.Value;

        return KeyCode.None;
    }


    string KeyToArrowSymbol(KeyCode k) =>
        k switch {
            KeyCode.LeftArrow  => "←",
            KeyCode.RightArrow => "→",
            KeyCode.UpArrow    => "↑",
            KeyCode.DownArrow  => "↓",
            _ => "?"
        };
}
