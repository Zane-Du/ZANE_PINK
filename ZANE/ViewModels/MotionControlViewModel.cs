using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ZANE.ViewModels;

public partial class MotionControlViewModel : ObservableObject
{
    [ObservableProperty]
    private string _pageTitle = "运动控制";

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private int _clickCount;

    [RelayCommand]
    private void TestButton()
    {
        ClickCount++;
        StatusText = $"已点击 {ClickCount} 次";
        System.Windows.MessageBox.Show("运动控制 功能测试\n点击次数: " + ClickCount, "ZANE - 运动控制");
    }

    [RelayCommand]
    private void ResetStatus()
    {
        ClickCount = 0;
        StatusText = "状态已重置";
    }
}
