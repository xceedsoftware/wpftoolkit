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
  internal class ColumnVisiblePositionChangedEventArgs : EventArgs
  {
    internal ColumnVisiblePositionChangedEventArgs( ColumnBase triggeringColumn, int oldPosition, int newPosition )
    {
      m_triggeringColumn = triggeringColumn;
      m_newPosition = newPosition;
      m_oldPosition = oldPosition;
    }

    internal ColumnBase TriggeringColumn
    {
      get
      {
        return m_triggeringColumn;
      }
    }

    internal int PositionDelta
    {
      get
      {
        return m_newPosition - m_oldPosition;
      }
    }

    private ColumnBase m_triggeringColumn;
    private int m_newPosition;
    private int m_oldPosition;
  }
}
