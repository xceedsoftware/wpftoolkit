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
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Xceed.Wpf.DataGrid.Converters
{
  [ValueConversion( typeof( double ), typeof( string ) )]
  public class CurrencyConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( ( value != null ) && ( !object.Equals( string.Empty, value ) ) )
      {
        Type valueType = value.GetType();

        // Only Decimal or Double values can be converted
        if( ( valueType.IsAssignableFrom( typeof( decimal ) ) == false ) && ( valueType.IsAssignableFrom( typeof( double ) ) == false ) )
        {
          return Binding.DoNothing;
        }

        try
        {
          // Convert the string value provided by an editor to a double before formatting. 
          double tempDouble = System.Convert.ToDouble( value, CultureInfo.CurrentCulture );
          return string.Format( CultureInfo.CurrentCulture, "{0:C}", tempDouble );
        }
        catch
        {
          return Binding.DoNothing;
        }
      }

      return string.Format( CultureInfo.CurrentCulture, "{0}", value );
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      double result;

      if( double.TryParse( value as string, NumberStyles.Currency, CultureInfo.CurrentCulture, out result ) )
        return result;

      return Binding.DoNothing;
    }
  }
}
