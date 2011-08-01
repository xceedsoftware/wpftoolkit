using System;
using System.Windows.Data;

namespace Microsoft.Windows.Controls.Core.Converters
{
    public class ObjectTypeToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.GetType().Name;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
