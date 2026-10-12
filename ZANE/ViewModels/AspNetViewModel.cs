using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZANE.Logging;

namespace ZANE.ViewModels;

public partial class AspNetViewModel : ObservableObject
{
    [ObservableProperty]
    private string _pageTitle = "ASP.NET";

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private int _clickCount;

    [RelayCommand]
    private void TestButton()
    {
        ClickCount++;
        StatusText = $"已点击 {ClickCount} 次";
        System.Windows.MessageBox.Show("ASP.NET 功能测试\n点击次数: " + ClickCount, "ZANE - ASP.NET");
        "klkl".LogRun(Logging.Enums.Log4NetLevelEnum.成功,true);
        "klkl4".LogRun(Logging.Enums.Log4NetLevelEnum.警告,true);
        "klkl2".LogRun(Logging.Enums.Log4NetLevelEnum.错误, true);
        "klkl3".LogRun(Logging.Enums.Log4NetLevelEnum.信息,false);
      
    }

    [RelayCommand]
    private void ResetStatus()
    {
        ClickCount = 0;
        StatusText = "状态已重置";
    }
}
