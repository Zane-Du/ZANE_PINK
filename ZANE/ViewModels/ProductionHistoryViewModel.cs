using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ZANE.ViewModels;

public partial class ProductionHistoryViewModel : ObservableObject
{
    [ObservableProperty]
    private string _pageTitle = "生产历史";

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private int _clickCount;

    [RelayCommand]
    private void TestButton()
    {
        ClickCount++;
        StatusText = $"已点击 {ClickCount} 次";
        System.Windows.MessageBox.Show("生产历史 功能测试\n点击次数: " + ClickCount, "ZANE - 生产历史");
    }

    [RelayCommand]
    private void ResetStatus()
    {
        ClickCount = 0;
        StatusText = "状态已重置";
    }
}
