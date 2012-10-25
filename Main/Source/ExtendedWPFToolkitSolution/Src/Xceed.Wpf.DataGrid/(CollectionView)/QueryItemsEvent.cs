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
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Xceed.Wpf.DataGrid
{
  public class QueryItemsEventArgs : EventArgs
  {
    internal QueryItemsEventArgs(
      DataGridVirtualizingCollectionView collectionView,
      DataGridVirtualizingCollectionViewGroup collectionViewGroup,
      AsyncQueryInfo asyncQueryInfo )
    {
      m_dataGridVirtualizingCollectionView = collectionView;

      // The collectionViewGroup can be null when we abort
      // a QueryItems for the old RootGroup when 
      // DataGridVirtualizingCollectionViewBase.ForceRefresh
      // is called
      m_readonlyGroupPath = ( collectionViewGroup != null )
        ? collectionViewGroup.GroupPath.AsReadOnly()
        : new ReadOnlyCollection<DataGridGroupInfo>( new List<DataGridGroupInfo>() );

      m_asyncQueryInfo = asyncQueryInfo;
    }


    #region CollectionView PROPERTY

    public DataGridVirtualizingCollectionView CollectionView
    {
      get
      {
        return m_dataGridVirtualizingCollectionView;
      }
    }

    #endregion CollectionView PROPERTY

    #region GroupPath PROPERTY

    public ReadOnlyCollection<DataGridGroupInfo> GroupPath
    {
      get
      {
        return m_readonlyGroupPath;
      }
    }

    #endregion GroupPath PROPERTY


    #region AsyncQueryInfo PROPERTY

    public AsyncQueryInfo AsyncQueryInfo
    {
      get
      {
        return m_asyncQueryInfo;
      }
    }

    #endregion AsyncQueryInfo PROPERTY


    #region PRIVATE FIELDS

    private DataGridVirtualizingCollectionView m_dataGridVirtualizingCollectionView;
    private AsyncQueryInfo m_asyncQueryInfo;

    private ReadOnlyCollection<DataGridGroupInfo> m_readonlyGroupPath;

    #endregion PRIVATE FIELDS
  }
}
