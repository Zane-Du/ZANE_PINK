using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ZANE.ViewModels;

public partial class CameraCalibViewModel : ObservableObject
{
    [ObservableProperty]
    private string _pageTitle = "相机标定";

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private int _clickCount;

    [RelayCommand]
    private void TestButton()
    {
        ClickCount++;
        StatusText = $"已点击 {ClickCount} 次";
        System.Windows.MessageBox.Show("相机标定 功能测试\n点击次数: " + ClickCount, "ZANE - 相机标定");
    }

    [RelayCommand]
    private void ResetStatus()
    {
        ClickCount = 0;
        StatusText = "状态已重置";
    }
}
