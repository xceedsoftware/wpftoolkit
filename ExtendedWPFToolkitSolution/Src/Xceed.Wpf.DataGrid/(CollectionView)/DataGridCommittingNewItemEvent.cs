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
  public class DataGridCommittingNewItemEventArgs : DataGridItemCancelEventArgs
  {
    public DataGridCommittingNewItemEventArgs( DataGridCollectionViewBase collectionView, object item, bool cancel )
      : base( collectionView, item, cancel )
    {
      m_index = -1;
      m_newCount = -1;
    }

    #region Index Property

    public int Index
    {
      get { return m_index; }
      set { m_index = value; }
    }

    private int m_index;

    #endregion Index Property

    #region NewCount

    public int NewCount
    {
      get { return m_newCount; }
      set { m_newCount = value; }
    }

    private int m_newCount;

    #endregion NewCount
  }
}
