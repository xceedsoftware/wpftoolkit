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

namespace Xceed.Wpf.DataGrid
{
  internal class ColumnActualWidthChangedEventArgs : EventArgs
  {
    internal ColumnActualWidthChangedEventArgs( ColumnBase column, double oldValue, double newValue )
    {
      m_column = column;
      m_oldValue = oldValue;
      m_newValue = newValue;
    }

    internal ColumnBase Column
    {
      get
      {
        return m_column;
      }
    }

    internal double NewValue
    {
      get
      {
        return m_newValue;
      }
    }

    internal double OldValue
    {
      get
      {
        return m_oldValue;
      }
    }

    private ColumnBase m_column;
    private double m_oldValue;
    private double m_newValue;
  }

  internal delegate void ColumnActualWidthChangedEventHandler( object sender, ColumnActualWidthChangedEventArgs e );
}
