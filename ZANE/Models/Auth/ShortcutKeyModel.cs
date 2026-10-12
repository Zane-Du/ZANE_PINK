namespace ZANE.Models.Auth;

/// <summary>
/// 快捷键模型 —— 定义全局快捷键组合
/// </summary>
public class ShortcutKeyModel
{
    public bool IsCrtl { get; set; }
    public bool IsAlt { get; set; }
    public bool IsShift { get; set; }
    public char Key { get; set; }
    public Action? Action { get; set; }
}
