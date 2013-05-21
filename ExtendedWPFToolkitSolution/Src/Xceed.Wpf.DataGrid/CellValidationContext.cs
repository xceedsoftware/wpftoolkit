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
using System.Collections.Generic;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  public class CellValidationContext
  {
    public CellValidationContext( object dataItem, Cell cell )
    {
      m_cell = cell;
      m_dataItem = dataItem;
    }

    public object DataItem
    {
      get
      {
        return m_dataItem;
      }
    }

    public Cell Cell
    {
      get
      {
        return m_cell;
      }
    }

    private object m_dataItem;
    private Cell m_cell;
  }
}
