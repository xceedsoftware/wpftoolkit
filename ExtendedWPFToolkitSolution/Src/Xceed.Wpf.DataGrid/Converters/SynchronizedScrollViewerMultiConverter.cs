/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid.Converters
{
  internal class SynchronizedScrollViewerMultiConverter : IMultiValueConverter
  {
    #region IMultiValueConverter Members

    public object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      double largestValue = 0d;

      foreach( double value in values )
      {
        if( value > largestValue )
        {
          largestValue = value;
        }
      }

      return largestValue;
    }

    public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
    {
      return null;
    }

    #endregion
  }
}
