/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2025 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Globalization;
using System.Windows.Data;
using System;

namespace Xceed.Wpf.Toolkit.Converters
{
  public class BooleanToTimeSpanConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      string param = parameter as string;
      if( value is bool useAnimations )
      {
        if( useAnimations ) 
        {
          if( param == "adddelay" )
          {
            return TimeSpan.FromSeconds( 0.7 );
          }
          else
          {
            return TimeSpan.Zero;
          }
        }
        else 
        { 
          return TimeSpan.FromDays(1);
        }
      }
      return TimeSpan.Zero;
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
