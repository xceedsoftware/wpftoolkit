using System;
using System.Windows.Data;
using System.Windows;

namespace Microsoft.Windows.Controls.Mag.Converters
{
    internal class BorderThicknessToStrokeThicknessConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Thickness thickness = (Thickness)value;
            return (thickness.Bottom + thickness.Left + thickness.Right + thickness.Top) / 4;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
