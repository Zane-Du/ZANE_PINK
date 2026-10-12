using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ZANE.ViewModels;

public partial class ControlPanelViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isSystemRunning = true;

    [ObservableProperty]
    private string _systemState = "运行中";

    [ObservableProperty]
    private double _progressValue = 65;

    [ObservableProperty]
    private string _lastAction = "无操作";

    [RelayCommand]
    private void StartSystem()
    {
        IsSystemRunning = true;
        SystemState = "运行中";
        LastAction = "系统已启动";
    }

    [RelayCommand]
    private void StopSystem()
    {
        IsSystemRunning = false;
        SystemState = "已停止";
        LastAction = "系统已停止";
    }

    [RelayCommand]
    private void EmergencyStop()
    {
        IsSystemRunning = false;
        SystemState = "紧急停止";
        LastAction = "⚠️ 紧急停止已触发";
        System.Windows.MessageBox.Show("紧急停止已触发！\n请检查系统状态。", "ZANE - 控制面板", 
            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
    }

    [RelayCommand]
    private void RunDiagnostic()
    {
        var rand = new Random();
        ProgressValue = 0;

        // 模拟诊断进度
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        timer.Tick += (s, e) =>
        {
            ProgressValue += rand.NextDouble() * 10;
            if (ProgressValue >= 100)
            {
                ProgressValue = 100;
                timer.Stop();
                LastAction = "诊断完成 - 系统正常 ✅";
                System.Windows.MessageBox.Show("系统诊断完成，所有模块运行正常。", "ZANE - 诊断结果");
            }
        };

        timer.Start();
        LastAction = "正在运行诊断...";
    }
}
