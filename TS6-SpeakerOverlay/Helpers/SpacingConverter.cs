using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TS6_SpeakerOverlay.Helpers
{
    public class SpacingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double spacing)
            {
                // spacing 是滑块的值，我们把它应用到 上下边距
                // Left, Top, Right, Bottom
                return new Thickness(0, spacing / 2, 0, spacing / 2);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}