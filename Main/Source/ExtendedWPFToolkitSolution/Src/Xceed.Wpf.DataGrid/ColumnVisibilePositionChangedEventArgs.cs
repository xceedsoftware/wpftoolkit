/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  internal class ColumnVisiblePositionChangedEventArgs : EventArgs
  {
    #region PUBLIC CONTRUCTORS

    public ColumnVisiblePositionChangedEventArgs( ColumnBase triggeringColumn, int oldPosition, int newPosition )
    {
      m_triggeringColumn = triggeringColumn;
      m_newPosition = newPosition;
      m_oldPosition = oldPosition;
    }

    #endregion

    #region PUBLIC PROPERTIES

    public ColumnBase TriggeringColumn
    {
      get
      {
        return m_triggeringColumn;
      }
    }

    public int PositionDelta
    {
      get
      {
        return m_newPosition - m_oldPosition;
      }
    }

    #endregion

    #region PRIVATE FIELDS

    private ColumnBase m_triggeringColumn;
    private int m_newPosition;
    private int m_oldPosition;

    #endregion
  }
}
