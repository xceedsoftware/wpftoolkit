using System;
using System.Windows.Data;

namespace Microsoft.Windows.Controls.Core.Converters
{
    public class TimeFormatToDateTimeFormatConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeFormat timeFormat = (TimeFormat)value;
            switch (timeFormat)
            {
                case TimeFormat.Custom:
                    return DateTimeFormat.Custom;
                case TimeFormat.ShortTime:
                    return DateTimeFormat.ShortTime;
                case TimeFormat.LongTime:
                    return DateTimeFormat.LongTime;
                default:
                    return DateTimeFormat.ShortTime;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
