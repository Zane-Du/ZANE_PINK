using System.Windows;

namespace ZANE.Services;

public enum AppTheme
{
    Light,
    Dark
}

public class ThemeService
{
    private const string LightThemeUri = "pack://application:,,,/ZANE;component/Themes/LightTheme.xaml";
    private const string DarkThemeUri = "pack://application:,,,/ZANE;component/Themes/DarkTheme.xaml";

    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public event Action<AppTheme>? ThemeChanged;

    public void ApplyTheme(AppTheme theme)
    {
        CurrentTheme = theme;

        var app = Application.Current;
        if (app == null) return;

        var mergedDictionaries = app.Resources.MergedDictionaries;

        // Remove existing theme dictionaries
        var toRemove = mergedDictionaries
            .Where(d => d.Source?.ToString() is LightThemeUri or DarkThemeUri)
            .ToList();

        foreach (var dict in toRemove)
            mergedDictionaries.Remove(dict);

        // Add the new theme
        var uri = theme == AppTheme.Light ? LightThemeUri : DarkThemeUri;
        var newDict = new ResourceDictionary { Source = new Uri(uri, UriKind.Absolute) };
        mergedDictionaries.Add(newDict);

        ThemeChanged?.Invoke(theme);
    }

    public void ToggleTheme()
    {
        ApplyTheme(CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
    }
}
