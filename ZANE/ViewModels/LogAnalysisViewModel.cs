using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZANE.Logging;
using ZANE.Logging.Enums;
using ZANE.Logging.Models;

namespace ZANE.ViewModels;

public partial class LogAnalysisViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<RunLogModel> _runLogs = new();

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<RunLogModel> _filteredLogs = new();

    /// <summary>
    /// 最大缓存日志条数
    /// </summary>
    private const int MaxLogCount = 800;

    public LogAnalysisViewModel()
    {
        // 注册 DisplayRunLogAction，接收来自 Log4NetHelper 的日志回调
        Log4NetHelper.DisplayRunLogAction = AddRunLog;
    }

    /// <summary>
    /// 由 Log4NetHelper 回调，将日志添加到 RunLogs 头部
    /// </summary>
    private void AddRunLog(string message, Log4NetLevelEnum level, Log4NetTypeEnum type)
    {
        var log = new RunLogModel
        {
            Message = message,
            Time = DateTime.Now.ToString("MM-dd HH:mm:ss fff")
        };

        switch (level)
        {
            case Log4NetLevelEnum.错误:
                log.ErrVisibility = Visibility.Visible;
                break;
            case Log4NetLevelEnum.警告:
                log.WarningVisibility = Visibility.Visible;
                break;
            case Log4NetLevelEnum.成功:
                log.SuccessVisibility = Visibility.Visible;
                break;
            default:
                log.MessageVisibility = Visibility.Visible;
                break;
        }

        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            RunLogs.Insert(0, log);
            if (RunLogs.Count > MaxLogCount)
                RunLogs.RemoveAt(MaxLogCount);

            // 如果有过滤文本，更新筛选结果
            ApplyFilter();
        }));
    }

    partial void OnFilterTextChanged(string value)
    {
        ApplyFilter();
    }

    /// <summary>
    /// 根据 FilterText 筛选日志
    /// </summary>
    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(FilterText))
        {
            FilteredLogs = RunLogs;
            return;
        }

        var filtered = RunLogs
            .Where(log => log.Message.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        FilteredLogs = new ObservableCollection<RunLogModel>(filtered);
    }

    /// <summary>
    /// 搜索命令
    /// </summary>
    [RelayCommand]
    private void SearchLogs()
    {
        ApplyFilter();
    }

    /// <summary>
    /// 打开当前日志文件（用记事本打开）
    /// </summary>
    [RelayCommand]
    private void OpenLogFile()
    {
        string logType = Log4NetTypeEnum.运行日志.ToString();
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", logType,
            DateTime.Now.ToString("yyyy-MM-dd"), $"{DateTime.Now:HH}.txt");

        if (File.Exists(path))
            System.Diagnostics.Process.Start("notepad.exe", path);
        else
            MessageBox.Show($"未找到日志文件：{path}", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    /// <summary>
    /// 打开日志所在文件夹
    /// </summary>
    [RelayCommand]
    private void OpenLogDirectory()
    {
        string logType = Log4NetTypeEnum.运行日志.ToString();
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", logType,
            DateTime.Now.ToString("yyyy-MM-dd"));

        if (Directory.Exists(path))
            System.Diagnostics.Process.Start("explorer.exe", path);
        else
        {
            // 尝试打开父级目录
            string parentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", logType);
            if (Directory.Exists(parentPath))
                System.Diagnostics.Process.Start("explorer.exe", parentPath);
            else
                MessageBox.Show($"未找到日志目录：{path}", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// 加载日志（将文件日志读取到列表）
    /// </summary>
    [RelayCommand]
    private void LoadLogs()
    {
        string logType = Log4NetTypeEnum.运行日志.ToString();
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", logType,
            DateTime.Now.ToString("yyyy-MM-dd"), $"{DateTime.Now:HH}.txt");

        if (!File.Exists(path))
        {
            MessageBox.Show($"未找到日志文件：{path}", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var lines = File.ReadAllLines(path);
            RunLogs.Clear();
            foreach (var line in lines.Reverse().Take(MaxLogCount))
            {
                RunLogs.Add(new RunLogModel
                {
                    Time = string.Empty,
                    Message = line,
                    MessageVisibility = Visibility.Visible
                });
            }
            ApplyFilter();
            MessageBox.Show($"已从文件加载 {lines.Length} 条日志", "ZANE - 日志分析");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载日志文件失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 清空日志
    /// </summary>
    [RelayCommand]
    private void ClearLogs()
    {
        RunLogs.Clear();
        FilteredLogs.Clear();
    }
}
