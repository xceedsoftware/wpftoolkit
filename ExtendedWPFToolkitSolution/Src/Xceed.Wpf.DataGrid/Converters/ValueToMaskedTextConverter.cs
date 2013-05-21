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
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Reflection;
using Xceed.Wpf.Toolkit;

namespace Xceed.Wpf.DataGrid.Converters
{
  public class ValueToMaskedTextConverter : IValueConverter
  {
    #region IValueConverter Members

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1800:DoNotCastUnnecessarily" )]
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( !targetType.IsAssignableFrom( typeof( string ) ) )
        return DependencyProperty.UnsetValue;

      string workingText = ( value == null ) ? string.Empty : value.ToString();

      string mask = null; // Defaults to no mask when no parameter is specified.

      if( parameter != null )
      {
        Type parameterType = parameter.GetType();
        if( parameterType == typeof( string ) )
        {
          string stringParameter = ( string )parameter;

          if( !string.IsNullOrEmpty( stringParameter ) )
            mask = stringParameter;
        }
        else
        {
          return DependencyProperty.UnsetValue;
        }
      }

      if( !string.IsNullOrEmpty( mask ) )
      {
        try
        {
          string rawText = string.Empty;

          CultureInfo currentCulture = CultureInfo.CurrentCulture;

          if( value != null )
          {
            try
            {
              Type valueDataType = value.GetType();

              MethodInfo valueToStringMethodInfo =
                valueDataType.GetMethod( "ToString", new Type[] { typeof( string ), typeof( IFormatProvider ) } );

              string formatSpecifier = MaskedTextBox.GetFormatSpecifierFromMask( mask, currentCulture );

              if( valueToStringMethodInfo != null )
              {
                rawText = ( string )valueToStringMethodInfo.Invoke( value, new object[] { formatSpecifier, currentCulture } );
              }
              else
              {
                rawText = value.ToString();
              }
            }
            catch
            {
              rawText = value.ToString();
            }
          }

          MaskedTextProvider maskedTextProvider = new MaskedTextProvider( mask, currentCulture );

          maskedTextProvider.Set( rawText );

          return maskedTextProvider.ToString( false, true );
        }
        catch
        {
        }
      }

      return value.ToString();
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      return Binding.DoNothing;
    }

    #endregion
  }
}
