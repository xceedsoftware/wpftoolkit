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
using System.Windows;

namespace Xceed.Wpf.DataGrid.Converters
{
  [ValueConversion( typeof( double ), typeof( double ) )]
  public class NegativeDoubleConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert( 
      object value, 
      Type targetType, 
      object parameter, 
      CultureInfo culture )
    {
      if( ( value == null )
        || ( value.GetType() != typeof( double ) )
        || ( !targetType.IsAssignableFrom( typeof( double ) ) ) )
      {
        return DependencyProperty.UnsetValue;
      }

      double doubleValue = ( double )value;
      return ( doubleValue * -1d );
    }

    public object ConvertBack( 
      object value, 
      Type targetType, 
      object parameter, 
      CultureInfo culture )
    {
      return this.Convert( value, targetType, parameter, culture );
    }

    #endregion
  }
}
