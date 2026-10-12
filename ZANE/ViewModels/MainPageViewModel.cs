using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ZANE.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _welcomeText = "欢迎使用 ZANE 系统";

    [ObservableProperty]
    private string _statusText = "系统运行正常";

    [ObservableProperty]
    private int _clickCount;

    [RelayCommand]
    private void Greet()
    {
        ClickCount++;
        StatusText = $"已点击 {ClickCount} 次欢迎按钮";
        System.Windows.MessageBox.Show($"你好！欢迎使用 ZANE。\n当前点击次数: {ClickCount}", "ZANE - 主页面");
    }

    [RelayCommand]
    private void ResetStatus()
    {
        ClickCount = 0;
        StatusText = "状态已重置";
    }
}
