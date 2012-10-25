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
  [ValueConversion(typeof(int), typeof(double))]
  public class LevelToOpacityConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert( 
      object value, 
      Type targetType, 
      object parameter, 
      CultureInfo culture )
    {
      if( ( value == null )
        || ( value.GetType() != typeof( int ) )
        || ( !targetType.IsAssignableFrom( typeof( double ) ) )
        || ( parameter == null ) )
      {
        return DependencyProperty.UnsetValue;
      }

      double step;

      Type parameterType = parameter.GetType();
      if( parameterType == typeof( double ) )
      {
        step = ( double )parameter;
      }
      else if( parameterType == typeof( string ) )
      {
        string strParam = (string)parameter;
        try
        {
          step = Double.Parse( strParam, CultureInfo.InvariantCulture );
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

      int level = ( int )value - 1;
      double opacity = 1.0d;

      //subtract 'step' opacity for each level
      opacity -= level * step;

      if( opacity < MinOpacity )
      {
        opacity = MinOpacity;
      }

      if( opacity > 1.0d )
      {
        opacity = 1.0d;
      }

      return opacity;

    }

    public object ConvertBack( 
      object value, 
      Type targetType, 
      object parameter, 
      CultureInfo culture )
    {
      if( ( value.GetType() != typeof( double ) )
        || ( targetType != typeof( int ) )
        || ( parameter == null ) )
      {
        return DependencyProperty.UnsetValue;
      }

      double step;

      Type parameterType = parameter.GetType();
      if( parameterType == typeof( double ) )
      {
        step = ( double )parameter;
      }
      else if( parameterType == typeof( string ) )
      {
        string strParam = ( string )parameter;
        try
        {
          step = Double.Parse( strParam, CultureInfo.InvariantCulture );
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

      double opacity = ( double )value;
      int level = 1;

      level += ( int )Math.Ceiling( ( 1.0d - opacity ) / step );

      return level;
    }

    #endregion

    private const double MinOpacity = 0.2d;
  }
}
