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

namespace Xceed.Wpf.DataGrid.Diagnostics
{
  internal class DataGridTraceArg<T> : DataGridTraceArg
  {
    internal DataGridTraceArg( T value )
    {
      m_value = value;
    }

    protected override string FormatOutput( CultureInfo culture )
    {
      if( object.ReferenceEquals( m_value, null ) )
        return "<null>";

      var formattable = m_value as IFormattable;
      if( formattable != null )
        return formattable.ToString( null, culture );

      var valueType = m_value.GetType();
      if( valueType.IsGenericType && ( valueType.GetGenericTypeDefinition() == typeof( Nullable<> ) ) )
      {
        valueType = Nullable.GetUnderlyingType( valueType );
      }

      if( valueType.IsEnum )
        return m_value.ToString();

      if( valueType.IsPrimitive || ( typeof( string ) == valueType ) )
        return string.Format( culture, "{0}", m_value );

      var hashCode = m_value.GetHashCode();
      var name = default( string );

      if( m_value is DataGridControl )
      {
        var dataGridControl = m_value as DataGridControl;

        name = dataGridControl.GridUniqueName;

        if( string.IsNullOrEmpty( name ) )
        {
          name = dataGridControl.Name;
        }
      }
      else if( m_value is FrameworkElement )
      {
        var fe = m_value as FrameworkElement;

        name = fe.Name;
      }
      else if( m_value is FrameworkContentElement )
      {
        var fce = m_value as FrameworkContentElement;

        name = fce.Name;
      }

      if( !string.IsNullOrEmpty( name ) )
        return string.Format( culture, "('{2}' -> {0}, <{1}>)", hashCode, valueType, name );

      return string.Format( culture, "({0}, <{1}>)", hashCode, valueType );
    }

    private readonly T m_value;
  }
}
