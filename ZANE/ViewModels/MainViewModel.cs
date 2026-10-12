using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using ZANE.Models;
using ZANE.Services;

namespace ZANE.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ThemeService _themeService;
    private readonly AuthService _authService;

    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    [ObservableProperty]
    private AppTheme _currentTheme;

    [ObservableProperty]
    private ObservableCollection<NavigationItem> _navItems = new();

    [ObservableProperty]
    private ObservableCollection<TopMenuItem> _topMenuItems = new();

    [ObservableProperty]
    private string _currentUserName = "未登录";

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private bool _loginPopupOpen;

    [ObservableProperty]
    private string _loginAccount = string.Empty;

    [ObservableProperty]
    private string _loginPassword = string.Empty;

    [ObservableProperty]
    private string _loginError = string.Empty;

    [ObservableProperty]
    private GridLength _sidebarWidth = new(220);

    private readonly Dictionary<string, ObservableObject> _viewModelCache = new();

    // 所有页面 ViewModel
    private readonly MainPageViewModel _mainPageVM;
    private readonly LogAnalysisViewModel _logAnalysisVM;
    private readonly ParameterConfigViewModel _parameterConfigVM;
    private readonly ChartDisplayViewModel _chartDisplayVM;
    private readonly ControlPanelViewModel _controlPanelVM;
    private readonly MESTestViewModel _mesTestVM;
    private readonly CameraCalibViewModel _cameraCalibVM;
    private readonly ImageDisplayViewModel _imageDisplayVM;
    private readonly MotionControlViewModel _motionControlVM;
    private readonly AspNetViewModel _aspNetVM;
    private readonly ProductionHistoryViewModel _productionHistoryVM;
    private readonly DeviceConfigViewModel _deviceConfigVM;
    private readonly SignalConfigViewModel _signalConfigVM;
    private readonly RoleManagementViewModel _roleManagementVM;
    private readonly UserManagementPageViewModel _userManagementPageVM;
    private readonly TestPage2ViewModel _testPage2VM;
    private readonly TestPage3ViewModel _testPage3VM;

    public MainViewModel(ThemeService themeService)
    {
        _themeService = themeService;
        _currentTheme = _themeService.CurrentTheme;
        _authService = AppServiceProvider.AuthService;
        CurrentUserName = _authService.IsLoggedIn ? _authService.CurrentUser!.Name : "未登录";
        IsLoggedIn = _authService.IsLoggedIn;
        _authService.AuthStateChanged += OnAuthStateChanged;

        _mainPageVM = new MainPageViewModel();
        _logAnalysisVM = new LogAnalysisViewModel();
        _parameterConfigVM = new ParameterConfigViewModel();
        _chartDisplayVM = new ChartDisplayViewModel();
        _controlPanelVM = new ControlPanelViewModel();
        _mesTestVM = new MESTestViewModel();
        _cameraCalibVM = new CameraCalibViewModel();
        _imageDisplayVM = new ImageDisplayViewModel();
        _motionControlVM = new MotionControlViewModel();
        _aspNetVM = new AspNetViewModel();
        _productionHistoryVM = new ProductionHistoryViewModel();
        _deviceConfigVM = new DeviceConfigViewModel();
        _signalConfigVM = new SignalConfigViewModel();
        _roleManagementVM = new RoleManagementViewModel();
        _userManagementPageVM = new UserManagementPageViewModel();
        _testPage2VM = new TestPage2ViewModel();
        _testPage3VM = new TestPage3ViewModel();

        InitializeNavItems();
        InitializeTopMenuItems();

        if (NavItems.Count > 0)
            NavigateTo(NavItems[0]);
    }


    private void InitializeTopMenuItems()
    {
        TopMenuItems = new ObservableCollection<TopMenuItem>
        {
            new()
            {
                Header = "浅色模式",
                Icon = "☀️",
                SubItems = new List<TopMenuSubItem>
                {
                    new() { Text = "浅色模式", Icon = "☀️", Command = () => SetTheme(AppTheme.Light) },
                    new() { Text = "深色模式", Icon = "🌙", Command = () => SetTheme(AppTheme.Dark) },
                }
            },
            new()
            {
                Header = "未登录",
                Icon = "🔑",
                SubItems = new List<TopMenuSubItem>
                {
                    new() { Text = "用户登录", Icon = "👤", Command = () => OpenLoginPopupCommand.Execute(null) },
                    new() { Text = "注销登录", Icon = "🚪", Command = () => LogoutCommand.Execute(null) },
                }
            },
            new()
            {
                Header = "通知",
                Icon = "🔔",
                SubItems = new List<TopMenuSubItem>
                {
                    new() { Text = "系统消息", Icon = "💬", Command = () => System.Windows.MessageBox.Show("暂无新系统消息", "ZANE") },
                    new() { Text = "任务提醒", Icon = "⏰", Command = () => System.Windows.MessageBox.Show("暂无待办任务", "ZANE") },
                    new() { Text = "更新日志", Icon = "📦", Command = () => System.Windows.MessageBox.Show("ZANE v1.0.0 已发布", "ZANE") },
                }
            },
            new()
            {
                Header = "设置",
                Icon = "⚙️",
                SubItems = new List<TopMenuSubItem>
                {
                    new() { Text = "系统设置", Icon = "🖥️", Command = () => System.Windows.MessageBox.Show("系统设置页面", "ZANE") },
                    new() { Text = "网络配置", Icon = "🌐", Command = () => System.Windows.MessageBox.Show("网络配置页面", "ZANE") },
                    new() { Text = "关于 ZANE", Icon = "ℹ️", Command = () => System.Windows.MessageBox.Show("ZANE v1.0.0\n基于 WPF + CommunityToolkit.MVVM", "关于 ZANE") },
                }
            },
        };
    }

    private void SetTheme(AppTheme theme)
    {
        _themeService.ApplyTheme(theme);
        CurrentTheme = _themeService.CurrentTheme;

        if (TopMenuItems.Count > 0)
        {
            var themeItem = TopMenuItems[0];
            themeItem.Icon = theme == AppTheme.Light ? "☀️" : "🌙";
            themeItem.Header = theme == AppTheme.Light ? "浅色模式" : "深色模式";
        }

        var current = CurrentViewModel;
        CurrentViewModel = null;
        System.Windows.Application.Current.Dispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.Background,
            new Action(() => CurrentViewModel = current));
    }

    private void OnAuthStateChanged()
    {
        CurrentUserName = _authService.IsLoggedIn
            ? _authService.CurrentUser?.Name ?? "未登录"
            : "未登录";
        IsLoggedIn = _authService.IsLoggedIn;

        // 更新顶部菜单的登录信息
        if (TopMenuItems.Count > 1)
        {
            var loginItem = TopMenuItems[1];
            loginItem.Header = _authService.IsLoggedIn
                ? $"已登录: {CurrentUserName}"
                : "未登录";
        }
    }

    [RelayCommand]
    private void OpenLoginPopup()
    {
        LoginAccount = string.Empty;
        LoginPassword = string.Empty;
        LoginError = string.Empty;
        LoginPopupOpen = true;
    }

    [RelayCommand]
    private void Login()
    {
        if (string.IsNullOrWhiteSpace(LoginAccount) || string.IsNullOrWhiteSpace(LoginPassword))
        {
            LoginError = "请输入账号和密码！";
            return;
        }

        if (_authService.Login(LoginAccount, LoginPassword))
        {
            LoginPopupOpen = false;
            OnAuthStateChanged();
            HandyControl.Controls.Growl.Success($"欢迎，{_authService.CurrentUser?.Name}！");
        }
        else
        {
            LoginError = "账号或密码错误！";
        }
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        OnAuthStateChanged();
        HandyControl.Controls.Growl.Info("已注销登录");
    }

    [RelayCommand]
    private void CancelLogin()
    {
        LoginPopupOpen = false;
        LoginError = string.Empty;
    }

    [RelayCommand]
    private void NavigateTo(NavigationItem item)
    {
        // 用户管理和角色管理页面仅限管理员访问
        if ((item.ViewName == "UserManagement" || item.ViewName == "TestPage1")&& !IsAdmin())
        {
            HandyControl.Controls.Growl.Warning("仅管理员可访问此页面！");
            return;
        }

        foreach (var nav in NavItems)
            nav.IsSelected = false;

        item.IsSelected = true;

        var vm = GetViewModel(item.ViewName);
        CurrentViewModel = vm;
    }

    private bool IsAdmin()
    {
        return _authService.IsLoggedIn && _authService.CurrentRole != null&& _authService.CurrentRole.Level == ulong.MaxValue;
    }

    private ObservableObject GetViewModel(string viewName)
    {
        if (_viewModelCache.TryGetValue(viewName, out var cached))
            return cached;

        ObservableObject vm = viewName switch
        {
            "MainPage"           => _mainPageVM,
            "LogAnalysis"        => _logAnalysisVM,
            "ParameterConfig"    => _parameterConfigVM,
            "ChartDisplay"       => _chartDisplayVM,
            "ControlPanel"       => _controlPanelVM,
            "MESTest"            => _mesTestVM,
            "CameraCalib"        => _cameraCalibVM,
            "ImageDisplay"       => _imageDisplayVM,
            "MotionControl"      => _motionControlVM,
            "AspNet"             => _aspNetVM,
            "ProductionHistory"  => _productionHistoryVM,
            "DeviceConfig"       => _deviceConfigVM,
            "SignalConfig"       => _signalConfigVM,
            "UserManagement"     => _roleManagementVM,
            "TestPage1"          => _userManagementPageVM,
            "TestPage2"          => _testPage2VM,
            "TestPage3"          => _testPage3VM,
            _ => _mainPageVM
        };

        _viewModelCache[viewName] = vm;
        return vm;
    }


    private void InitializeNavItems()
    {
        var items = new List<NavigationItem>
        {
            new() { Icon = "🏠", Title = "主页面",       ViewName = "MainPage" },
            new() { Icon = "📋", Title = "日志分析",     ViewName = "LogAnalysis" },
            new() { Icon = "⚙️", Title = "参数配置",     ViewName = "ParameterConfig" },

            new() { Icon = "🔧", Title = "MES测试",      ViewName = "MESTest" },
            new() { Icon = "👥", Title = "角色管理",     ViewName = "UserManagement" },
            new() { Icon = "👤", Title = "用户管理",    ViewName = "TestPage1" },
            new() { Icon = "🧪", Title = "测试页面2",    ViewName = "TestPage2" },
            new() { Icon = "🧪", Title = "测试页面3",    ViewName = "TestPage3" },
            new() { Icon = "📷", Title = "相机标定",     ViewName = "CameraCalib" },
            new() { Icon = "🖼️", Title = "图像显示",     ViewName = "ImageDisplay" },
            new() { Icon = "🎯", Title = "运动控制",     ViewName = "MotionControl" },
            new() { Icon = "🌐", Title = "ASP.NET",      ViewName = "AspNet" },
            new() { Icon = "🏭", Title = "生产历史",     ViewName = "ProductionHistory" },
            new() { Icon = "💻", Title = "设备配置",     ViewName = "DeviceConfig" },
            new() { Icon = "📡", Title = "信号配置",     ViewName = "SignalConfig" },
            new() { Icon = "📊", Title = "图表显示",     ViewName = "ChartDisplay" },
            new() { Icon = "🎛️", Title = "控制面板",     ViewName = "ControlPanel" },

          
        };

        NavItems = new ObservableCollection<NavigationItem>(items);
    }

}
