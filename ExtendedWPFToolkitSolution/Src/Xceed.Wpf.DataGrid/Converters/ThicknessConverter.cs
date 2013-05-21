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

namespace Xceed.Wpf.DataGrid.Converters
{
  [ValueConversion( typeof( double ), typeof( Thickness ) )]
  public class ThicknessConverter : IValueConverter
  {
    private bool m_inverseValue;

    public bool InverseValue
    {
      get
      {
        return m_inverseValue;
      }
      set
      {
        m_inverseValue = value;
      }
    }


    #region IValueConverter Members

    public object Convert( 
      object value, 
      Type targetType, 
      object parameter, 
      CultureInfo culture )
    {
      if( ( !targetType.IsAssignableFrom( typeof( Thickness ) ) )
        || ( value == null )
        || ( value.GetType() != typeof( double ) ) )
      {
        return DependencyProperty.UnsetValue;
      }

      ThicknessSides sides = ThicknessSides.All; // Defaults to All when no parameter is present;
      if(parameter != null)
      {
        Type parameterType = parameter.GetType();
        if( parameterType == typeof(ThicknessSides) )
        {
          sides = ( ThicknessSides )parameter;
        }
        else if( parameterType == typeof( string ) )
        {
          string stringParameter = (string)parameter;
          if( !string.IsNullOrEmpty( stringParameter ) )
          {
            try
            {
              sides = ( ThicknessSides )Enum.Parse( typeof( ThicknessSides ), stringParameter );
            }
            catch
            {
              return DependencyProperty.UnsetValue;
            }
          }
        }
        else
        {
          return DependencyProperty.UnsetValue;
        }
      }

      double doubleValue = ( double )value;

      if( this.InverseValue == true )
      {
        doubleValue *= -1;
      }

      Thickness thickness = new Thickness();

      if( IsSideSet( sides, ThicknessSides.Left ) )
        thickness.Left = doubleValue;

      if( IsSideSet( sides, ThicknessSides.Top ) )
        thickness.Top = doubleValue;

      if( IsSideSet( sides, ThicknessSides.Right ) )
        thickness.Right = doubleValue;

      if( IsSideSet( sides, ThicknessSides.Bottom ) )
        thickness.Bottom = doubleValue;

      return thickness;
    }

    public object ConvertBack( 
      object value, 
      Type targetType, 
      object parameter, 
      CultureInfo culture )
    {
      return Binding.DoNothing;
    }

    #endregion

    private static bool IsSideSet( ThicknessSides sides, ThicknessSides sideToTest )
    {
      return ( ( sides & sideToTest ) == sideToTest );
    }

    [Flags]
    public enum ThicknessSides
    {
      Left = 0x01,
      Top = 0x02,
      Right = 0x04,
      Bottom = 0x08,
      All = 0x0F
    }
  }
}
