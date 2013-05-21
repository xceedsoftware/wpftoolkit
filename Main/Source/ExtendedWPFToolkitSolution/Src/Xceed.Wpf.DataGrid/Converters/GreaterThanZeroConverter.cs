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
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Xceed.Wpf.DataGrid.Converters
{
  [ValueConversion( typeof( object ), typeof( bool ) )]
  public class GreaterThanZeroConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( !targetType.IsAssignableFrom( typeof( bool ) ) )
        return DependencyProperty.UnsetValue;

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

      return number > 0d;
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      return Binding.DoNothing;
    }

    #endregion
  }
}
