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
