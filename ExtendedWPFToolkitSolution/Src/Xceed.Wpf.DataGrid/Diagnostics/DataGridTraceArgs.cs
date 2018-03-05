/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Globalization;

namespace Xceed.Wpf.DataGrid.Diagnostics
{
  internal static class DataGridTraceArgs
  {
    internal static DataGridTraceArg Action<T>( T value )
    {
      return new TraceArg<T>( "Action", value );
    }

    internal static DataGridTraceArg DataSource<T>( T value )
    {
      return new TraceArg<T>( "DataSource", value );
    }

    internal static DataGridTraceArg DataGridControl<T>( T value )
    {
      return new TraceArg<T>( "DataGridControl", value );
    }

    internal static DataGridTraceArg Generator<T>( T value )
    {
      return new TraceArg<T>( "ItemContainerGenerator", value );
    }

    internal static DataGridTraceArg Count<T>( T value )
    {
      return new TraceArg<T>( "Count", value );
    }

    internal static DataGridTraceArg ItemCount<T>( T value )
    {
      return new TraceArg<T>( "ItemCount", value );
    }

    internal static DataGridTraceArg Container<T>( T value )
    {
      return new TraceArg<T>( "Container", value );
    }

    internal static DataGridTraceArg Group<T>( T value )
    {
      return new TraceArg<T>( "Group", value );
    }

    internal static DataGridTraceArg Index<T>( T value )
    {
      return new TraceArg<T>( "Index", value );
    }

    internal static DataGridTraceArg GeneratorIndex<T>( T value )
    {
      return new TraceArg<T>( "GIndex", value );
    }

    internal static DataGridTraceArg From<T>( T value )
    {
      return new TraceArg<T>( "From", value );
    }

    internal static DataGridTraceArg To<T>( T value )
    {
      return new TraceArg<T>( "To", value );
    }

    internal static DataGridTraceArg Item<T>( T value )
    {
      return new TraceArg<T>( "Item", value );
    }

    internal static DataGridTraceArg Node<T>( T value )
    {
      return new TraceArg<T>( "Node", value );
    }

    internal static DataGridTraceArg NextNode<T>( T value )
    {
      return new TraceArg<T>( "NNode", value );
    }

    internal static DataGridTraceArg PreviousNode<T>( T value )
    {
      return new TraceArg<T>( "PNode", value );
    }

    internal static DataGridTraceArg LastNode<T>( T value )
    {
      return new TraceArg<T>( "LNode", value );
    }

    internal static DataGridTraceArg Value<T>( T value )
    {
      return new TraceArg<T>( "Value", value );
    }

    #region TraceArg Private Class

    private sealed class TraceArg<T> : DataGridTraceArg<T>
    {
      internal TraceArg( string label, T value )
        : base( value )
      {
        m_label = label;
      }

      protected sealed override string FormatOutput( CultureInfo culture )
      {
        var label = m_label;
        var value = base.FormatOutput( culture );

        if( string.IsNullOrEmpty( label ) )
          return value;

        if( value == null )
          return string.Format( "{0}: <null>", label );

        if( value.Length == 0 )
          return string.Format( "{0}: ''", label );

        return string.Format( "{0}: {1}", label, value );
      }

      private readonly string m_label;
    }

    #endregion
  }
}
