using UnityEngine;

public enum CursorModeType { Default, Attack, Shoot }

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    private const string RES_PATH = "IMG/";  // Resources/IMG 폴더

    private Texture2D cursorDefault;  // D_Mouse
    private Texture2D cursorAttack;   // A_Mouse
    private Texture2D cursorShoot;    // S_Mouse
    private Texture2D currentCursor;

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

    private void LoadResources()
    {
        cursorDefault = Resources.Load<Texture2D>(RES_PATH + "D_Mouse");
        cursorAttack  = Resources.Load<Texture2D>(RES_PATH + "A_Mouse");
        cursorShoot   = Resources.Load<Texture2D>(RES_PATH + "S_Mouse");
        SetCursor(CursorModeType.Default);
    }

    void Update()
    {
        // 1) 스토리 씬이나 메인 메뉴라면 항상 기본 커서
        if (MySceneManager.IsStoryScene || MySceneManager.IsMainMenu)
        {
            SetCursor(CursorModeType.Default);
            return;
        }

        // 2) Player 인스턴스가 없으면 기본 커서
        var player = Player.Instance;
        if (player == null)
        {
            SetCursor(CursorModeType.Default);
            return;
        }

        // 3) Player.cs 에서 외부에 공개한 프로퍼티로 사격 모드 여부를 읽어야 함
        //    (private 필드인 isShootingMode 를 직접 접근하면 안 됩니다)
        bool isShootingMode = player.IsShootingMode;  // ◆ 여기서만 사격 모드 판정

        // 4) 월드 좌표 + Raycast
        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;
        RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);

        // ────────────────────────────────────────────────────
        // A) 근접 공격 모드 (isShootingMode == false)
        //    • 마우스가 “적(Enemy)” 위에 있으면 Attack 커서
        //    • 그 외(빈 공간, 공주 위 포함)는 항상 Default 커서
        // ────────────────────────────────────────────────────
        if (!isShootingMode)
        {
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                SetCursor(CursorModeType.Attack);
            }
            else
            {
                SetCursor(CursorModeType.Default);
            }
            return;
        }

        // ────────────────────────────────────────────────────
        // B) 사격 모드 (isShootingMode == true)
        //    • 마우스가 “공주(Princess)” 위에 있으면 Default 커서 (공주를 못 쏘도록)
        //    • 그 외(적 위든 빈 공간이든 상관없이) Shoot 커서
        // ────────────────────────────────────────────────────
        if (hit.collider != null && hit.collider.CompareTag("Princess"))
        {
            SetCursor(CursorModeType.Default);
        }
        else
        {
            SetCursor(CursorModeType.Shoot);
        }
    }

    /// <summary>
    /// 실제 커서 이미지를 바꿉니다.
    /// </summary>
    public void SetCursor(CursorModeType mode)
    {
        Texture2D tex = mode switch
        {
            CursorModeType.Attack  => cursorAttack,
            CursorModeType.Shoot   => cursorShoot,
            _                      => cursorDefault
        };

        // 이미 같은 커서라면 변경하지 않음
        if (tex != null && tex != currentCursor)
        {
            Cursor.SetCursor(tex, Vector2.zero, CursorMode.Auto);
            currentCursor = tex;
        }
    }
}
