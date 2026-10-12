using ZANE.Services;

namespace ZANE;

/// <summary>
/// 简单的服务定位器（替代 DI 容器）
/// </summary>
public static class AppServiceProvider
{
    private static AuthService? _authService;

    public static AuthService AuthService =>
        _authService ??= new AuthService();
}
