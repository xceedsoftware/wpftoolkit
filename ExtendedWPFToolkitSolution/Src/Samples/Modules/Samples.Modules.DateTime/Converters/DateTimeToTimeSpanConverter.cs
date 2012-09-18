using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Samples.Modules.DateTime.Converters
{
  public class DateTimeToTimeSpanConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( value is System.DateTime )
      {
        System.DateTime time = (System.DateTime)value;
        return new TimeSpan( time.Hour, time.Minute, 0 );
      }
      return value;
    }
    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
