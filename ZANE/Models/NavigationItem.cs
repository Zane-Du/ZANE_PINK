using CommunityToolkit.Mvvm.ComponentModel;

namespace ZANE.Models;

public partial class NavigationItem : ObservableObject
{
    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _viewName = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private string? _badge;
}
