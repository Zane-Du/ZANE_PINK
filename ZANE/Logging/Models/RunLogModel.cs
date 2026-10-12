using System.Windows;

namespace ZANE.Logging.Models;

public class RunLogModel
{
    /// <summary>
    /// 时间
    /// </summary>
    public string Time { get; set; } = string.Empty;

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    public Visibility ErrVisibility { get; set; } = Visibility.Collapsed;
    public Visibility WarningVisibility { get; set; } = Visibility.Collapsed;
    public Visibility SuccessVisibility { get; set; } = Visibility.Collapsed;
    public Visibility MessageVisibility { get; set; } = Visibility.Collapsed;
}
