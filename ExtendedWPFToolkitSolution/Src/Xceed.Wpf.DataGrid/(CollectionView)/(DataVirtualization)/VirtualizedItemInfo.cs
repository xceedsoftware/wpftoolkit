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
