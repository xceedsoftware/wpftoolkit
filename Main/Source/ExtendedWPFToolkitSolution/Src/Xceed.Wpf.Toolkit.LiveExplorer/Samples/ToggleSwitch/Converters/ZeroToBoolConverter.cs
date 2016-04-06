using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.ToggleSwitch.Converters
{
  public class ZeroToBoolConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( value == null )
        return false;

      double number = 0d;

      try
      {
        number = System.Convert.ToDouble( value );
      }
      catch
      {
        return false;
      }

      return number == 0d;
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
