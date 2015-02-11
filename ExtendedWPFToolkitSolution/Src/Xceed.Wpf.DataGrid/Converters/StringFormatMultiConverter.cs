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
using System.Globalization;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid.Converters
{
  public class StringFormatMultiConverter : IMultiValueConverter
  {
    public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
    {
      if( ( values == null ) || ( values.Length <= 0 ) )
        return null;

      var data = values[ 0 ];

      var format = ( values.Length > 1 ) ? values[ 1 ] as string : null;
      if( string.IsNullOrEmpty( format ) )
        return data;

      var currentCulture = ( values.Length > 2 ) ? values[ 2 ] as CultureInfo : null;
      if( currentCulture == null )
      {
        currentCulture = culture ?? CultureInfo.CurrentCulture;
      }

      return string.Format( currentCulture, format, data );
    }

    public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
