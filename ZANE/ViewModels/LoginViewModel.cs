using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using ZANE.Services;

namespace ZANE.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private string _account = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoginVisible = true;

    [ObservableProperty]
    private string _currentUserName = "未登录";

    public bool LoginSucceeded { get; private set; }

    public LoginViewModel()
    {
        _authService = AppServiceProvider.AuthService;
        IsLoginVisible = !_authService.IsLoggedIn;
        if (_authService.IsLoggedIn)
            CurrentUserName = _authService.CurrentUser?.Name ?? "未登录";
    }

    [RelayCommand]
    private void Login()
    {
        if (string.IsNullOrWhiteSpace(Account) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "请输入账号和密码！";
            return;
        }

        if (_authService.Login(Account, Password))
        {
            LoginSucceeded = true;
            CurrentUserName = _authService.CurrentUser?.Name ?? "用户";
            IsLoginVisible = false;
            ErrorMessage = string.Empty;
            Growl.Success($"欢迎回来，{CurrentUserName}！");
        }
        else
        {
            ErrorMessage = "账号或密码错误！";
        }
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        LoginSucceeded = false;
        IsLoginVisible = true;
        CurrentUserName = "未登录";
        Account = string.Empty;
        Password = string.Empty;
        ErrorMessage = string.Empty;
    }
}
