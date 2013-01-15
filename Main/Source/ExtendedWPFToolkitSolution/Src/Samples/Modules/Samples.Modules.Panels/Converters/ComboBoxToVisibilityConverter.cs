using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Samples.Modules.Panels.Converters
{
  class ComboBoxToVisibilityConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( ( value is int ) && ( parameter is string )
        && ( int )value == Int32.Parse( ( string )parameter ) )
      {
        return Visibility.Visible;
      }

      return Visibility.Collapsed;
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
