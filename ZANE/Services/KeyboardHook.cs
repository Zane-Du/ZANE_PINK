using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using ZANE.Models.Auth;

namespace ZANE.Services;

/// <summary>
/// 全局键盘钩子 —— 用于注册全局快捷键（如 Ctrl+Z）
/// 基于 Windows 低级键盘钩子 (WH_KEYBOARD_LL)
/// </summary>
public sealed class KeyboardHook : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;

    // 虚拟键码映射
    private static readonly Dictionary<int, char> CharMap = new()
    {
        { 0x30, '0' }, { 0x31, '1' }, { 0x32, '2' }, { 0x33, '3' }, { 0x34, '4' },
        { 0x35, '5' }, { 0x36, '6' }, { 0x37, '7' }, { 0x38, '8' }, { 0x39, '9' },
        { 0x41, 'A' }, { 0x42, 'B' }, { 0x43, 'C' }, { 0x44, 'D' }, { 0x45, 'E' },
        { 0x46, 'F' }, { 0x47, 'G' }, { 0x48, 'H' }, { 0x49, 'I' }, { 0x4A, 'J' },
        { 0x4B, 'K' }, { 0x4C, 'L' }, { 0x4D, 'M' }, { 0x4E, 'N' }, { 0x4F, 'O' },
        { 0x50, 'P' }, { 0x51, 'Q' }, { 0x52, 'R' }, { 0x53, 'S' }, { 0x54, 'T' },
        { 0x55, 'U' }, { 0x56, 'V' }, { 0x57, 'W' }, { 0x58, 'X' }, { 0x59, 'Y' },
        { 0x5A, 'Z' },
    };

    private const byte VK_LCONTROL = 0xA2;
    private const byte VK_RCONTROL = 0xA3;
    private const byte VK_LSHIFT = 0xA0;
    private const byte VK_RSHIFT = 0xA1;
    private const byte VK_LALT = 0xA4;
    private const byte VK_RALT = 0xA5;

    // 单例
    private static readonly Lazy<KeyboardHook> Lazy = new(() => new KeyboardHook());
    public static KeyboardHook Instance => Lazy.Value;

    private delegate int HookProc(int nCode, int wParam, IntPtr lParam);

    private HookProc? _hookProc;
    private int _hookHandle;
    private bool _isStarted;
    private readonly List<ShortcutKeyModel> _shortcuts = new();

    private KeyboardHook() { }

    /// <summary>
    /// 注册全局快捷键
    /// </summary>
    public void Register(ShortcutKeyModel shortcut)
    {
        _shortcuts.Add(shortcut);
    }

    /// <summary>
    /// 启动监听
    /// </summary>
    public bool Start()
    {
        if (_isStarted) return true;

        try
        {
            _hookProc = HookCallback;
            using var curProcess = Process.GetCurrentProcess();
            using var mainModule = curProcess.MainModule;
            if (mainModule == null) return false;

            var moduleHandle = GetModuleHandle(mainModule.ModuleName);
            _hookHandle = SetWindowsHookEx(
                WH_KEYBOARD_LL,
                _hookProc,
                moduleHandle,
                0);

            if (_hookHandle != 0)
            {
                _isStarted = true;
                Debug.WriteLine("[KeyboardHook] 启动成功");
                return true;
            }

            Debug.WriteLine("[KeyboardHook] 启动失败");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[KeyboardHook] 启动异常：{ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 停止监听
    /// </summary>
    public void Stop()
    {
        if (_hookHandle != 0)
        {
            UnhookWindowsHookEx(_hookHandle);
            _hookHandle = 0;
        }
        _isStarted = false;
        Debug.WriteLine("[KeyboardHook] 已停止");
    }

    /// <summary>
    /// 键盘钩子回调
    /// </summary>
    private int HookCallback(int nCode, int wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == WM_KEYDOWN)
        {
            var vkCode = Marshal.ReadInt32(lParam);

            if (CharMap.TryGetValue(vkCode, out var ch))
            {
                bool isCtrl = (GetKeyState(VK_LCONTROL) & 0x80) != 0 ||
                              (GetKeyState(VK_RCONTROL) & 0x80) != 0;
                bool isShift = (GetKeyState(VK_LSHIFT) & 0x80) != 0 ||
                               (GetKeyState(VK_RSHIFT) & 0x80) != 0;
                bool isAlt = (GetKeyState(VK_LALT) & 0x80) != 0 ||
                             (GetKeyState(VK_RALT) & 0x80) != 0;

                foreach (var shortcut in _shortcuts)
                {
                    if (shortcut.IsCrtl == isCtrl &&
                        shortcut.IsAlt == isAlt &&
                        shortcut.IsShift == isShift &&
                        shortcut.Key == ch)
                    {
                        shortcut.Action?.Invoke();
                        break;
                    }
                }
            }
        }

        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        Stop();
        _shortcuts.Clear();
    }

    // ─── Win32 API ───
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(int idHook);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}
