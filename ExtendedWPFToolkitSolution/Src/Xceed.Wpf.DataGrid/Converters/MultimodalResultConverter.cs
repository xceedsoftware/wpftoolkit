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
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Xceed.Wpf.DataGrid.Converters
{
  [ValueConversion( typeof( object[] ), typeof( string ) )]
  public class MultimodalResultConverter : IValueConverter
  {
    private string m_separator = ", ";

    public string Separator
    {
      get
      {
        return m_separator;
      }
      set
      {
        m_separator = value;
      }
    }

    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      object[] values = value as object[];

      if( values == null )
        return DependencyProperty.UnsetValue;

      StringBuilder result = new StringBuilder();

      if( values.Length > 0 )
      {
        for( int i = 0; i < values.Length - 1; i++ )
        {
          result.Append( values[ i ].ToString() + m_separator );
        }

        result.Append( values[ values.Length - 1 ].ToString() );
      }

      return result.ToString();
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      // Stat values come from read-only properties.
      return Binding.DoNothing;
    }
  }
}
