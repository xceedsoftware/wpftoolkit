using System;
using System.Windows.Data;
using System.Globalization;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class TimeSpanEditor : DateTimeUpDownEditor
    {
        private sealed class TimeSpanConverter : IValueConverter
        {
            object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return DateTime.Today + (TimeSpan)value;
            }

            object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return ((DateTime)value).TimeOfDay;
            }
        }

        protected override void SetControlProperties()
        {
            Editor.Format = DateTimeFormat.LongTime;
        }

        protected override IValueConverter CreateValueConverter()
        {
            return new TimeSpanConverter();            
        }
    }
}
