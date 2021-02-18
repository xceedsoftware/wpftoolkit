/**************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ************************************************************************************/
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.RangeSlider.Converters
{
  public class AbsoluteAdditionConverter : IMultiValueConverter
  {
    public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
    {
      if( (values != null) && (values.Count() == 2) )
      {
        var min = (double)values[ 0 ];
        var max = (double)values[ 1 ];
        return Math.Abs( min ) + Math.Abs( max );
      }

      return null;
    }

    public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
