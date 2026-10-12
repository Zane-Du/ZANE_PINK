using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ZANE.ViewModels;

public partial class ChartDisplayViewModel : ObservableObject
{
    [ObservableProperty]
    private string _chartType = "折线图";

    [ObservableProperty]
    private string _dataSource = "模拟数据";

    [ObservableProperty]
    private string _displayStatus = "图表就绪";

    [ObservableProperty]
    private ObservableCollection<ChartDataPoint> _dataPoints = new()
    {
        new() { Label = "T1", Value = 23.5 },
        new() { Label = "T2", Value = 45.2 },
        new() { Label = "T3", Value = 38.7 },
        new() { Label = "T4", Value = 52.1 },
        new() { Label = "T5", Value = 41.8 },
    };

    [RelayCommand]
    private void RefreshChart()
    {
        var rand = new Random();
        foreach (var dp in DataPoints)
            dp.Value = Math.Round(rand.NextDouble() * 100, 1);

        DisplayStatus = $"图表已刷新 ({DataPoints.Count} 个数据点)";
        System.Windows.MessageBox.Show($"图表数据已更新。\n当前图表类型: {ChartType}\n数据来源: {DataSource}", "ZANE - 图表显示");
    }

    [RelayCommand]
    private void ToggleChartType()
    {
        ChartType = ChartType switch
        {
            "折线图" => "柱状图",
            "柱状图" => "饼图",
            "饼图" => "折线图",
            _ => "折线图"
        };
        DisplayStatus = $"已切换至: {ChartType}";
    }

    [RelayCommand]
    private void ExportData()
    {
        System.Windows.MessageBox.Show("数据导出的功能待实现", "ZANE - 图表显示");
    }
}

public partial class ChartDataPoint : ObservableObject
{
    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private double _value;
}
