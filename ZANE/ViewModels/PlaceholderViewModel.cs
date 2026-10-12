using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ZANE.ViewModels;

public partial class PlaceholderViewModel : ObservableObject
{
    [ObservableProperty]
    private string _pageTitle = "功能开发中";

    [ObservableProperty]
    private string _description = "此页面功能正在开发中，敬请期待...";

    [ObservableProperty]
    private int _placeholderClickCount;

    [RelayCommand]
    private void ShowInfo()
    {
        PlaceholderClickCount++;
        System.Windows.MessageBox.Show($"这是一个占位页面。\n点击次数: {PlaceholderClickCount}\n\n后续版本将实现完整功能。", 
            "ZANE - " + PageTitle);
    }
}
