using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HttpTool.Desktop.Converters;

/// <summary>
/// 值转换器集合
/// </summary>
public class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NullToVisibleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EnumConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null || value == null)
            return false;

        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            return Enum.Parse(targetType, parameter.ToString()!);
        }
        return Binding.DoNothing;
    }
}

public class ZeroToCollapsedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ZeroToVisibleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class AuthValueVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is HttpTool.Core.Enums.AuthType authType)
        {
            return authType != HttpTool.Core.Enums.AuthType.None ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusColorConverter : IValueConverter
{
    private static readonly SolidColorBrush SuccessBrush = new(Colors.Green);
    private static readonly SolidColorBrush ErrorBrush = new(Colors.Red);
    private static readonly SolidColorBrush DefaultBrush = new(Colors.Black);

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSuccess)
        {
            return isSuccess ? SuccessBrush : ErrorBrush;
        }
        return DefaultBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BinaryToCollapsedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is HttpTool.Core.Enums.BodyType bodyType)
        {
            return bodyType == HttpTool.Core.Enums.BodyType.Binary ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EnumToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null || value == null)
            return Visibility.Collapsed;

        return value.ToString() == parameter.ToString() ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class HttpMethodToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush GetBrush = new(Color.FromRgb(0x28, 0xa7, 0x45));      // Green
    private static readonly SolidColorBrush PostBrush = new(Color.FromRgb(0x00, 0x7b, 0xff));    // Blue
    private static readonly SolidColorBrush PutBrush = new(Color.FromRgb(0xfd, 0x7e, 0x14));     // Orange
    private static readonly SolidColorBrush DeleteBrush = new(Color.FromRgb(0xdc, 0x35, 0x45)); // Red
    private static readonly SolidColorBrush PatchBrush = new(Color.FromRgb(0x6f, 0x42, 0xc1));   // Purple
    private static readonly SolidColorBrush HeadBrush = new(Color.FromRgb(0x6c, 0x75, 0x7d));   // Gray
    private static readonly SolidColorBrush OptionsBrush = new(Color.FromRgb(0x20, 0xc9, 0x97)); // Teal
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(0x6c, 0x75, 0x7d)); // Gray

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is HttpTool.Core.Enums.HttpMethodType method)
        {
            return method switch
            {
                HttpTool.Core.Enums.HttpMethodType.GET => GetBrush,
                HttpTool.Core.Enums.HttpMethodType.POST => PostBrush,
                HttpTool.Core.Enums.HttpMethodType.PUT => PutBrush,
                HttpTool.Core.Enums.HttpMethodType.DELETE => DeleteBrush,
                HttpTool.Core.Enums.HttpMethodType.PATCH => PatchBrush,
                HttpTool.Core.Enums.HttpMethodType.HEAD => HeadBrush,
                HttpTool.Core.Enums.HttpMethodType.OPTIONS => OptionsBrush,
                _ => DefaultBrush
            };
        }
        return DefaultBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
