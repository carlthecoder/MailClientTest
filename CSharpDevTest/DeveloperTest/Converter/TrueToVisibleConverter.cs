using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DeveloperTest.Converter
{
    public sealed class TrueToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (bool)value;

            if (val)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visible)
            {
                if (visible == Visibility.Visible)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
