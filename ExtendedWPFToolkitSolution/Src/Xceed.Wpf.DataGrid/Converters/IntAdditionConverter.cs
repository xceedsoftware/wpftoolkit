/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

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
