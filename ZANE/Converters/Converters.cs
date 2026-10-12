using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ZANE.Services;

namespace ZANE.Converters;

public class ThemeToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AppTheme theme)
            return theme == AppTheme.Light;
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isLight)
            return isLight ? AppTheme.Light : AppTheme.Dark;
        return AppTheme.Light;
    }
}

public class ThemeToggleIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AppTheme theme)
            return theme == AppTheme.Light ? "☀️" : "🌙";
        return "☀️";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ThemeToggleTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AppTheme theme)
            return theme == AppTheme.Light ? "深色模式" : "浅色模式";
        return "深色模式";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
