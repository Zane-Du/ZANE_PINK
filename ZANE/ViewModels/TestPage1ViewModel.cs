using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ZANE.ViewModels;

public partial class TestPage1ViewModel : ObservableObject
{
    [ObservableProperty]
    private string _pageTitle = "测试页面1";

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private int _clickCount;

    [RelayCommand]
    private void TestButton()
    {
        ClickCount++;
        StatusText = $"已点击 {ClickCount} 次";
        System.Windows.MessageBox.Show("测试页面1 功能测试\n点击次数: " + ClickCount, "ZANE - 测试页面1");
    }

    [RelayCommand]
    private void ResetStatus()
    {
        ClickCount = 0;
        StatusText = "状态已重置";
    }
}
