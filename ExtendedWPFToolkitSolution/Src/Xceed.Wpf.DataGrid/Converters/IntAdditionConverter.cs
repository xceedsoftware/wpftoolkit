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
  [ValueConversion( typeof( int ), typeof( int ) )]
  public class IntAdditionConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert( 
      object value, 
      Type targetType, 
      object parameter, 
      CultureInfo culture )
    {
      if( ( !targetType.IsAssignableFrom( typeof( int ) ) )
        || ( value == null )
        || ( value.GetType() != typeof( int ) )
        || ( parameter == null ) )
      {
        return DependencyProperty.UnsetValue;
      }

      int myParameter;

      Type parameterType = parameter.GetType();
      if( parameterType == typeof( int ) )
      {
        myParameter = ( int )parameter;
      }
      else if( parameterType == typeof( string ) )
      {
        string stringParameter = ( string )parameter;
        try
        {
          myParameter = int.Parse( stringParameter, CultureInfo.InvariantCulture );
        }
        catch
        {
          return DependencyProperty.UnsetValue;
        }
      }
      else
      {
        return DependencyProperty.UnsetValue;
      }

      int myValue = ( int )value;

      myValue = myValue + myParameter;

      return myValue;
    }

    public object ConvertBack( 
      object value, 
      Type targetType, 
      object parameter, 
      CultureInfo culture )
    {
      if( ( targetType != typeof( int ) )
        || ( value == null )
        || ( value.GetType() != typeof( int ) )
        || ( parameter == null ) )
      {
        return DependencyProperty.UnsetValue;
      }

      int myParameter;

      Type parameterType = parameter.GetType();
      if( parameterType == typeof( int ) )
      {
        myParameter = ( int )parameter;
      }
      else if( parameterType == typeof( string ) )
      {
        string stringParameter = ( string )parameter;
        try
        {
          myParameter = int.Parse( stringParameter, CultureInfo.InvariantCulture );
        }
        catch
        {
          return DependencyProperty.UnsetValue;
        }
      }
      else
      {
        return DependencyProperty.UnsetValue;
      }

      int myValue = ( int )value;

      myValue = myValue - myParameter;

      return myValue;
    }

    #endregion
  }
}
