// Windows 専用：VRCBuildAutoAgree
// Build & Publish を検知 → 著作権同意モーダルの出現を待って、保存した座標を Left Click。
// ・OK 位置は 3 秒カウントダウン後に現在マウス位置で保存
// ・未設定で Build されたら、手順をコンソールに案内
// ・コンソールにはユーザー向けの進行ログを出す（Done / Failed も明示）

using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class VRCBuildAutoAgree
{
    // ====== 製品設定 ======
    const string PRODUCT   = "VRCBuildAutoAgree";
    const string MENU_ROOT = "Tools/VRCBuildAutoAgree";

    // 対象ウィンドウ/モーダル判定
    const string TARGET_WINDOW_TITLE = "VRChat SDK";
    const string MODAL_TITLE         = "Copyright ownership agreement";
    const string MODAL_BODY_PREFIX   = "By clicking OK, I certify";

    // タイミング
    const int    OffsetCountdownSec  = 3;     // オフセット保存のカウントダウン（秒）
    const int    ClickDelayMs        = 200;    // クリック前の安定待ち（ms）
    const double WaitModalTimeoutSec = 5.0;   // モーダル検知の最大待機（秒）

    // EditorPrefs キー
    static readonly string PREF_ENABLED  = $"{PRODUCT}.Enabled";
    static readonly string PREF_OFFSET_X = $"{PRODUCT}.OffsetX";
    static readonly string PREF_OFFSET_Y = $"{PRODUCT}.OffsetY";

    // 状態
    static bool _enabled;
    static EditorWindow  _sdkWindow;
    static VisualElement _hookedRoot;
    static bool _rootHooked;

    // Build → モーダル待機
    static bool   _waitingModal;
    static double _waitStart;

    // 3秒キャプチャ
    static bool   _offsetArming;
    static double _offsetStart;
    static int    _lastShownRemain = -1;

    static VRCBuildAutoAgree()
    {
        _enabled = EditorPrefs.GetBool(PREF_ENABLED, false);
        EditorApplication.update += Tick;
        Log($"Loaded. Enabled={_enabled}");
    }

    // ===== メニュー =====
    [MenuItem(MENU_ROOT + "/Enable")]
    public static void Enable()
    {
        _enabled = true;
        EditorPrefs.SetBool(PREF_ENABLED, true);
        Log("Enabled.");
    }

    [MenuItem(MENU_ROOT + "/Disable")]
    public static void Disable()
    {
        _enabled = false;
        _waitingModal  = false;
        _offsetArming  = false;
        EditorPrefs.SetBool(PREF_ENABLED, false);
        Log("Disabled.");
    }

    [MenuItem(MENU_ROOT + "/Set OK Offset (3s countdown)")]
    public static void SetOffsetCountdown()
    {
        var win = FindSdkWindow();
        if (win == null) { Warn("VRChat SDK window not found. Open it first."); return; }

        _sdkWindow      = win;
        _offsetArming   = true;
        _offsetStart    = EditorApplication.timeSinceStartup;
        _lastShownRemain = -1;
        Log($"Move mouse to the OK button area... capturing in {OffsetCountdownSec} seconds.");
    }

    [MenuItem(MENU_ROOT + "/Reset Offset")]
    public static void ResetOffset()
    {
        EditorPrefs.DeleteKey(PREF_OFFSET_X);
        EditorPrefs.DeleteKey(PREF_OFFSET_Y);
        Log("Offset cleared.");
    }

    [MenuItem(MENU_ROOT + "/Test Click Now")]
    public static void TestClickNow()
    {
        var win = FindSdkWindow();
        if (win == null) { Warn("VRChat SDK window not found."); return; }
        var ok = ClickAtOffset(win, out var reason);
        if (ok) Done(); else Failed(reason);
    }

    // ===== メインループ =====
    static void Tick()
    {
        try
        {
            // A) オフセット保存のカウントダウン
            if (_offsetArming && _sdkWindow != null)
            {
                double elapsed = EditorApplication.timeSinceStartup - _offsetStart;
                int remain = Mathf.Max(0, OffsetCountdownSec - (int)Math.Floor(elapsed));
                if (remain != _lastShownRemain)
                {
                    if (remain > 0) Log($"Capturing in {remain}...");
                    _lastShownRemain = remain;
                }
                if (elapsed >= OffsetCountdownSec)
                {
                    _offsetArming = false;
                    SaveOffsetFromMouse(_sdkWindow);
                }
            }

            if (!_enabled) return;

            // B) SDK ウィンドウと Root フック
            var win = FindSdkWindow();
            if (win != null)
            {
                _sdkWindow = win;
                TryHookRoot(win.rootVisualElement);
            }

            // C) Build 後：モーダル検知→クリック
            if (_waitingModal && _sdkWindow != null)
            {
                if (IsAgreementModalVisible(_sdkWindow.rootVisualElement))
                {
                    Log("Agreement modal detected. Clicking saved offset.");
                    var ok = ClickAtOffset(_sdkWindow, out var reason);
                    _waitingModal = false;
                    if (ok) Done(); else Failed(reason);
                }
                else if (EditorApplication.timeSinceStartup - _waitStart > WaitModalTimeoutSec)
                {
                    Warn("Modal not detected within timeout. Clicking anyway.");
                    var ok = ClickAtOffset(_sdkWindow, out var reason);
                    _waitingModal = false;
                    if (ok) Done(); else Failed("Modal timeout: " + reason);
                }
            }
        }
        catch (Exception e)
        {
            Failed("Unexpected: " + e.Message);
        }
    }

    // ===== Build 検知（Root でキャプチャ）=====
    static void TryHookRoot(VisualElement root)
    {
        if (root == null) return;

        if (_hookedRoot != root) { _rootHooked = false; _hookedRoot = null; }
        if (_rootHooked) return;

        root.RegisterCallback<ClickEvent>(OnAnyClickCaptured, TrickleDown.TrickleDown);
        _rootHooked = true;
        _hookedRoot = root;
        Log("Hooked Build click (root capture).");
    }

    static void OnAnyClickCaptured(ClickEvent e)
    {
        var ve = e.target as VisualElement;
        if (ve == null) return;

        if (IsBuildPublishElementOrAncestor(ve))
        {
            // オフセット未設定なら案内して終了
            if (!OffsetIsSet())
            {
                Failed($"OKボタンの位置が未設定です。{MENU_ROOT}/Set OK Offset (3s countdown) を実行して設定してください。");
                return;
            }

            _waitingModal = true;
            _waitStart    = EditorApplication.timeSinceStartup;
            Log("Build detected. Waiting for agreement modal...");
        }
    }

    static bool IsBuildPublishElementOrAncestor(VisualElement start)
    {
        var v = start;
        for (int i = 0; i < 10 && v != null; i++)
        {
            if (LooksLikeBuildPublish(v)) return true;
            v = v.parent;
        }
        return false;
    }

    static bool LooksLikeBuildPublish(VisualElement ve)
    {
        // name（ダンプでは "main-action-button"）
        if ((ve.name ?? "") == "main-action-button") return true;

        // Button テキスト
        if (ve is Button btn && (btn.text ?? "").Trim()
            .Equals("Build & Publish", StringComparison.OrdinalIgnoreCase)) return true;

        // 子 Label のテキスト
        var lbl = ve.Q<Label>();
        if (lbl != null && (lbl.text ?? "").Trim()
            .Equals("Build & Publish", StringComparison.OrdinalIgnoreCase)) return true;

        // class の保険
        var cls = string.Join(" ", ve.GetClasses()).ToLowerInvariant();
        if (cls.Contains("main-action-button")) return true;

        return false;
    }

    // ===== モーダル検知 =====
    static bool IsAgreementModalVisible(VisualElement root)
    {
        if (root == null) return false;

        var title = root.Query<Label>().ToList()
            .FirstOrDefault(l => string.Equals(l.text?.Trim(), MODAL_TITLE, StringComparison.OrdinalIgnoreCase));
        if (title != null) return true;

        var body = root.Query<Label>().ToList()
            .FirstOrDefault(l => (l.text ?? "").StartsWith(MODAL_BODY_PREFIX, StringComparison.Ordinal));
        if (body != null) return true;

        return false;
    }

    // ===== クリック実行（Left Click 固定）=====
    static bool ClickAtOffset(EditorWindow win, out string reasonIfFailed)
    {
        reasonIfFailed = "";

        if (!OffsetIsSet())
        {
            reasonIfFailed = "Offset not set";
            return false;
        }

        float offx = EditorPrefs.GetFloat(PREF_OFFSET_X);
        float offy = EditorPrefs.GetFloat(PREF_OFFSET_Y);

        try
        {
            win.Focus(); // OS入力を受けるためフォーカス

            int x = Mathf.RoundToInt(win.position.x + offx);
            int y = Mathf.RoundToInt(win.position.y + offy);

            NativeMove(x, y);
            System.Threading.Thread.Sleep(ClickDelayMs);
            NativeLeftClick();

            Log($"LeftClick at ({x},{y}) on '{win.titleContent.text}'.");
            return true;
        }
        catch (Exception e)
        {
            reasonIfFailed = e.Message;
            return false;
        }
    }

    static bool OffsetIsSet()
    {
        return EditorPrefs.HasKey(PREF_OFFSET_X) && EditorPrefs.HasKey(PREF_OFFSET_Y);
    }

    static void SaveOffsetFromMouse(EditorWindow win)
    {
        GetCursorPos(out POINT p);
        var offset = new Vector2(p.X - win.position.x, p.Y - win.position.y);
        EditorPrefs.SetFloat(PREF_OFFSET_X, offset.x);
        EditorPrefs.SetFloat(PREF_OFFSET_Y, offset.y);
        Log($"Offset saved: ({offset.x:0},{offset.y:0})");
        Done("Offset capture");
    }

    static EditorWindow FindSdkWindow()
    {
        return Resources.FindObjectsOfTypeAll<EditorWindow>()
            .FirstOrDefault(w => w.titleContent.text == TARGET_WINDOW_TITLE);
    }

    // ===== ユーザー向けログ =====
    static void Log(string msg)   => Debug.Log($"[{PRODUCT}] {msg}");
    static void Warn(string msg)  => Debug.LogWarning($"[{PRODUCT}] {msg}");
    static void Done(string what = null)
        => Debug.Log($"[{PRODUCT}] Done{(string.IsNullOrEmpty(what) ? "" : $" ({what})")}!");
    static void Failed(string msg)
        => Debug.LogError($"[{PRODUCT}] Failed: {msg}");

    // ===== Win32 ネイティブ =====
    [StructLayout(LayoutKind.Sequential)]
    struct POINT { public int X; public int Y; }

    [DllImport("user32.dll")] static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    struct INPUT
    {
        public uint type; // 0 = mouse
        public MOUSEINPUT mi;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT
    {
        public int dx, dy;
        public uint mouseData, dwFlags, time;
        public IntPtr dwExtraInfo;
    }

    const uint INPUT_MOUSE        = 0;
    const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    const uint MOUSEEVENTF_LEFTUP   = 0x0004;

    [DllImport("user32.dll")] static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    static void NativeMove(int x, int y) => SetCursorPos(x, y);

    static void NativeLeftClick()
    {
        var down = new INPUT { type = INPUT_MOUSE, mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTDOWN } };
        var up   = new INPUT { type = INPUT_MOUSE, mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTUP } };
        SendInput(2, new[] { down, up }, Marshal.SizeOf<INPUT>());
    }
}
