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
using System.Linq;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  public class VirtualizedItemInfo
  {
    #region CONSTRUCTORS

    internal VirtualizedItemInfo( int index, object dataItem )
    {
      m_dataIndex = index;
      m_dataItem = dataItem;
    }

    #endregion CONSTRUCTORS

    #region DataItem Property

    public object DataItem
    {
      get
      {
        return m_dataItem;
      }
      internal set
      {
        m_dataItem = value;
      }
    }

    #endregion DataItem Property

    #region Index Property

    public int Index
    {
      get
      {
        return m_dataIndex;
      }
    }

    #endregion Index Property

    #region OldValues Property

    public VirtualizedItemValueCollection OldValues
    {
      get
      {
        return m_oldValues;
      }
      internal set
      {
        m_oldValues = value;
      }
    }

    #endregion OldValues Property

    #region INTERNAL PROPERTIES

    internal bool IsDirty
    {
      get
      {
        return m_oldValues != null;
      }
    }

    #endregion INTERNAL PROPERTIES

    #region PRIVATE FIELDS

    private object m_dataItem;
    private int m_dataIndex;
    private VirtualizedItemValueCollection m_oldValues;

    #endregion PRIVATE FIELDS
  }
}
