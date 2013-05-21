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
  public class CommitItemsEventArgs : EventArgs
  {
    internal CommitItemsEventArgs( DataGridVirtualizingCollectionViewBase collectionView, AsyncCommitInfo asyncCommitInfo )
    {
      m_dataGridVirtualizingCollectionViewBase = collectionView;
      m_asyncCommitInfo = asyncCommitInfo;
    }

    public DataGridVirtualizingCollectionViewBase CollectionView
    {
      get
      {
        return m_dataGridVirtualizingCollectionViewBase;
      }
    }

    public AsyncCommitInfo AsyncCommitInfo
    {
      get
      {
        return m_asyncCommitInfo;
      }
    }

    #region PRIVATE FIELDS

    private DataGridVirtualizingCollectionViewBase m_dataGridVirtualizingCollectionViewBase;
    private AsyncCommitInfo m_asyncCommitInfo;

    #endregion PRIVATE FIELDS
  }
}
