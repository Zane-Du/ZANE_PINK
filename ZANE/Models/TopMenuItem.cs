using CommunityToolkit.Mvvm.ComponentModel;

namespace ZANE.Models;

public partial class TopMenuItem : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    public List<TopMenuSubItem> SubItems { get; set; } = new();
}

public partial class TopMenuSubItem : ObservableObject
{
    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    public Action? Command { get; set; }
}
