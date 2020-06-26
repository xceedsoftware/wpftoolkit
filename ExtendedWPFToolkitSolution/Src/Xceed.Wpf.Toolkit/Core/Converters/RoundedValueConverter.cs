/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.Core.Converters
{
  public class RoundedValueConverter : IValueConverter
  {
    #region Precision Property

    public int Precision
    {
      get
      {
        return _precision;
      }
      set
      {
        _precision = value;
      }
    }

    private int _precision = 0;

    #endregion

    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( value is double )
      {
        return Math.Round( ( double )value, _precision );
      }
      else if( value is Point )
      {
        return new Point( Math.Round( ( ( Point )value ).X, _precision ), Math.Round( ( ( Point )value ).Y, _precision ) );
      }
      else
      {
        return value;
      }
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      return value;
    }
  }
}
