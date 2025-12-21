using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

/*
* Boolo値をVisibility列挙体に変換するコンバータ
*/

namespace MyFileManager;
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType,
                          object parameter, CultureInfo culture)
    {
        bool flag = value is bool b && b;

        // ConverterParameter に "Invert" が来たら反転
        if (parameter as string == "Invert")
            flag = !flag;

        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType,
                              object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
