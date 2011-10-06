using System;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Windows.Controls.Core.Converters
{
    public class ExpandableObjectMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int childLevel = (int)value;
            return new Thickness(childLevel * 15, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
