using UnityEngine;

public enum CursorModeType { Default, Attack, Shoot }

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    readonly string RES_PATH = "IMG/";      // Resources/IMG 폴더

    Texture2D cursorDefault;  // D_Mouse
    Texture2D cursorAttack;   // A_Mouse
    Texture2D cursorShoot;    // S_Mouse
    Texture2D currentCursor;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadResources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadResources()
    {
        cursorDefault = Resources.Load<Texture2D>(RES_PATH + "D_Mouse");
        cursorAttack  = Resources.Load<Texture2D>(RES_PATH + "A_Mouse");
        cursorShoot   = Resources.Load<Texture2D>(RES_PATH + "S_Mouse");

        SetCursor(CursorModeType.Default);   // 시작 커서
    }

void Update()
{
    // 1) 스토리씬 또는 메인메뉴면 기본 커서
    if (MySceneManager.IsStoryScene || MySceneManager.IsMainMenu)
    {
        SetCursor(CursorModeType.Default);
        return;
    }

    // 2) 월드 좌표 계산
    Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    wp.z = 0f;

    // 3) 단일 Raycast
    RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);

    // 4) 태그 비교로 모드 결정
    if (hit.collider != null && hit.collider.CompareTag("Enemy"))
    {
        SetCursor(CursorModeType.Attack);
    }
    else
    {
        SetCursor(CursorModeType.Shoot);
    }
}


    public void SetCursor(CursorModeType mode)
    {
        Texture2D tex = mode switch
        {
            CursorModeType.Attack  => cursorAttack,
            CursorModeType.Shoot   => cursorShoot,
            _                      => cursorDefault
        };

        if (tex != null && tex != currentCursor)
        {
            Cursor.SetCursor(tex, Vector2.zero, UnityEngine.CursorMode.Auto);
            currentCursor = tex;
        }
    }
}
