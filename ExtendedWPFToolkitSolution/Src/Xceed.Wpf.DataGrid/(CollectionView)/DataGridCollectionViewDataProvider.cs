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
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridCollectionViewDataProvider : DataGridCollectionViewBaseDataProvider
  {
    #region CONSTRUCTORS

    public DataGridCollectionViewDataProvider( DataGridCollectionViewSource parentSource )
      : base( parentSource )
    {
    }

    #endregion CONSTRUCTORS

    internal override DataGridCollectionViewBase EnsureDataGridCollectionViewBaseCore()
    {
      DataGridCollectionViewSource parentSource = this.ParentSource as DataGridCollectionViewSource;
      object source = parentSource.OriginalSource;
      Type itemType = parentSource.ItemType;
      DataSourceProvider dataSourceProvider = source as DataSourceProvider;

      if( dataSourceProvider != m_dataSourceProvider )
      {
        if( m_dataSourceProvider != null )
          DataChangedEventManager.RemoveListener( m_dataSourceProvider, this );

        m_dataSourceProvider = dataSourceProvider;

        if( m_dataSourceProvider != null )
        {
          DataChangedEventManager.AddListener( m_dataSourceProvider, this );
          m_dataSourceProvider.InitialLoad();
        }
      }

      if( dataSourceProvider != null )
        source = dataSourceProvider.Data;

      IListSource listSource = source as IListSource;

      if( listSource != null )
        source = listSource.GetList();

      if( source == null )
        return null;

      IEnumerable enumerableSource = source as IEnumerable;
      Debug.Assert( enumerableSource != null );

      DataGridCollectionViewBase currentView = this.CurrentView;

      if( ( currentView != null )
        && ( currentView.SourceCollection == enumerableSource )
        && ( currentView.DesiredItemType == itemType ) )
      {
        // No changes.
        return currentView;
      }

      return new DataGridCollectionView(
        enumerableSource,
        itemType,
        parentSource.AutoCreateItemProperties,
        parentSource.AutoCreateDetailDescriptions,
        parentSource.AutoCreateForeignKeyDescriptions );
    }


    private DataSourceProvider m_dataSourceProvider;
  }
}
