using System.IO;
using System.Windows;
using ZANE.Logging;
using ZANE.Logging.Enums;
using ZANE.Models.Auth;
using ZANE.Services;
using ZANE.ViewModels;

namespace ZANE;

public partial class App : Application
{
    public static ThemeService ThemeService { get; } = new();

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        InitializeLog4Net();

        var mainViewModel = new MainViewModel(ThemeService);

        var mainWindow = new MainWindow
        {
            DataContext = mainViewModel
        };

        mainWindow.Show();

        // 注册全局快捷键：Ctrl+Z 管理员自动登录
        KeyboardHook.Instance.Register(new ShortcutKeyModel
        {
            IsCrtl = true,
            Key = 'Z',
            Action = () =>
            {
                var auth = AppServiceProvider.AuthService;
                if (!auth.IsLoggedIn)
                {
                    auth.Login("admin", "admin");
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        //HandyControl.Controls.Growl.Success("Ctrl+Z 管理员快捷登录成功");
                    });
                }
            }
        });
        KeyboardHook.Instance.Start();
    }

    private static void InitializeLog4Net()
    {
        string logBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        string runLogPath = Path.Combine(logBasePath, Log4NetTypeEnum.运行日志.ToString()) + @"\";

        Log4NetInitialization.Log4NetConfigs = new List<Log4NetConfigModel>
        {
            new()
            {
                LoggerName = Log4NetTypeEnum.运行日志.ToString(),
                SavePath = runLogPath,
                DatePattern = "yyyy-MM-dd/HH'.txt'",
                MaxSizeRollBackups = 24,
                MaxFileSize = "10MB",
                ConversionPattern = "%date{yyyy-MM-dd HH:mm:ss fff} %message%newline",
                Header = ""
            }
        };

        Log4NetInitialization.SaveLog4NetXmlConfig();
        Log4NetInitialization.Log4NetXmlLoadConfiguration();
    }
}
