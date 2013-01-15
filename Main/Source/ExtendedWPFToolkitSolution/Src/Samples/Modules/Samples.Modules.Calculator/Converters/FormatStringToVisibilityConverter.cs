using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Samples.Modules.Calculator.Converters
{
  class FormatStringToVisibilityConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      //When FormatString received is empty, make the Precision property Visible.
      //This is to prevent something like this: Precision = "5" AND FormatString = "C2".
      if( string.IsNullOrEmpty( ( string )value ) )
        return Visibility.Visible;
      return Visibility.Hidden;
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
