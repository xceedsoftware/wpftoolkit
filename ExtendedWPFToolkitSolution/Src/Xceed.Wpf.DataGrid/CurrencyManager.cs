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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class CurrencyManager : IWeakEventListener
  {
    internal CurrencyManager( DataGridContext dataGridContext, CollectionView collectionView )
    {
      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      if( collectionView == null )
        throw new ArgumentNullException( "collectionView" );

      m_dataGridContext = dataGridContext;
      m_collectionView = collectionView;

      Debug.Assert( m_dataGridContext.CurrentItem == m_collectionView.CurrentItem );

      this.RegisterListeners();
    }

    #region IsSetCurrentInProgress Private Property

    private bool IsSetCurrentInProgress
    {
      get
      {
        return m_dataGridContext.DataGridControl.IsSetCurrentInProgress;
      }
    }

    #endregion

    #region ShouldSynchronizeCurrentItem Private Property

    private bool ShouldSynchronizeCurrentItem
    {
      get
      {
        return m_dataGridContext.DataGridControl.ShouldSynchronizeCurrentItem;
      }
    }

    #endregion

    #region ShouldSynchronizeSelectionWithCurrent Private Property

    private bool ShouldSynchronizeSelectionWithCurrent
    {
      get
      {
        // #case 158670 
        // When using DataGridVirtualizingCollectionViewBase, the synchronization between
        // currentItem and Selection will only be done if currentItem is modified in code-behind.
        var dgvcvb = m_collectionView.SourceCollection as DataGridVirtualizingCollectionViewBase;
        if( ( dgvcvb != null ) && !dgvcvb.CanSynchronizeSelectionWithCurrent )
          return false;

        return m_dataGridContext.DataGridControl.SynchronizeSelectionWithCurrent;
      }
    }

    #endregion

    internal void CleanManager()
    {
      this.UnregisterListeners();
    }

    private void ChangeCollectionViewCurrentItem()
    {
      if( m_isCurrentChanging || !this.ShouldSynchronizeCurrentItem )
        return;

      m_isCurrentChanging = true;

      try
      {
        // Synchronize the CurrentItem of the CollecitonView with the one of the DataGridContext.
        m_collectionView.MoveCurrentToPosition( m_dataGridContext.CurrentItemIndex );
      }
      finally
      {
        m_isCurrentChanging = false;
      }
    }

    private void ChangeDataGridContextCurrentItem()
    {
      // Prevent when a SetCurrent is in progress
      if( this.IsSetCurrentInProgress )
        return;

      if( m_isCurrentChanging || !this.ShouldSynchronizeCurrentItem )
        return;

      m_isCurrentChanging = true;

      try
      {
        // Synchronize the CurrentItem of the DataGridContext
        // with the one of the CollecitonView.
        m_dataGridContext.SetCurrent(
          m_collectionView.CurrentItem,
          null,
          m_collectionView.CurrentPosition,
          m_dataGridContext.CurrentColumn,
          false,
          false,
          this.ShouldSynchronizeSelectionWithCurrent,
          AutoScrollCurrentItemSourceTriggers.CollectionViewCurrentItemChanged );

        if( m_collectionView.CurrentItem == null )
        {
          this.ChangeCurrentDataGridContext();
        }
      }
      catch( DataGridException )
      {
        // When we deleted an item in edit that contain invalid data, we don't want a throw to go all the way up to the end user.
        // We try to abort the edit in that case.

        if( ( m_dataGridContext.IsCurrent ) && ( m_dataGridContext.DataGridControl.IsBeingEdited ) && ( !m_dataGridContext.Items.Contains( m_dataGridContext.CurrentItem ) ) )
        {
          m_dataGridContext.CancelEdit();

          m_dataGridContext.SetCurrent(
            m_collectionView.CurrentItem,
            null,
            m_collectionView.CurrentPosition,
            m_dataGridContext.CurrentColumn,
            false,
            false,
            this.ShouldSynchronizeSelectionWithCurrent,
            AutoScrollCurrentItemSourceTriggers.CollectionViewCurrentItemChanged );

          if( m_collectionView.CurrentItem == null )
          {
            this.ChangeCurrentDataGridContext();
          }
        }
      }
      finally
      {
        m_isCurrentChanging = false;
      }
    }

    private void ChangeCurrentDataGridContext()
    {
      var parentDataGridContext = m_dataGridContext.ParentDataGridContext;
      var parentItem = m_dataGridContext.ParentItem;

      if( ( parentDataGridContext == null ) || ( parentItem == null ) )
        return;

      var childContexts = CurrencyManager.GetChildContexts( parentDataGridContext, parentItem ).ToList();

      if( childContexts.Count > 1 )
      {
        var currentContextIndex = childContexts.IndexOf( m_dataGridContext );
        int lookForwardFrom;
        int lookBackwardFrom;

        if( currentContextIndex < 0 )
        {
          lookForwardFrom = 0;
          lookBackwardFrom = -1;
        }
        else
        {
          lookForwardFrom = currentContextIndex + 1;
          lookBackwardFrom = currentContextIndex - 1;
        }

        for( int i = lookForwardFrom; i < childContexts.Count; i++ )
        {
          var childContext = childContexts[ i ];
          if( childContext.Items.Count <= 0 )
            continue;

          childContext.SetCurrentItemCore( childContext.Items.GetItemAt( 0 ), false, this.ShouldSynchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers.CollectionViewCurrentItemChanged );
          return;
        }

        for( int i = lookBackwardFrom; i >= 0; i-- )
        {
          var childContext = childContexts[ i ];

          var itemsCount = childContext.Items.Count;
          if( itemsCount <= 0 )
            continue;

          childContext.SetCurrentItemCore( childContext.Items.GetItemAt( itemsCount - 1 ), false, this.ShouldSynchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers.CollectionViewCurrentItemChanged );
          return;
        }
      }

      // No context after or before us have been found, we will set the CurrentItem to our ParentItem.
      parentDataGridContext.SetCurrentItemCore( parentItem, false, this.ShouldSynchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers.CollectionViewCurrentItemChanged );
    }

    private void RegisterListeners()
    {
      // We are not checking this.ShouldSynchronizeCurrentItem since it might return the wrong value if the grid
      // is still in the process of being instantiated.
      CurrentItemChangedEventManager.AddListener( m_dataGridContext, this );
      CurrentChangedEventManager.AddListener( m_collectionView, this );
    }

    private void UnregisterListeners()
    {
      // We are not checking this.ShouldSynchronizeCurrentItem since it might have returned the wrong value 
      // when RegisterListeners was called if the grid was still in the process of being instantiated.
      CurrentItemChangedEventManager.RemoveListener( m_dataGridContext, this );
      CurrentChangedEventManager.RemoveListener( m_collectionView, this );
    }

    private static IEnumerable<DataGridContext> GetChildContexts( DataGridContext dataGridContext, object item )
    {
      Debug.Assert( dataGridContext != null );
      Debug.Assert( item != null );

      return ( from detailConfig in dataGridContext.DetailConfigurations
               let childContext = dataGridContext.GetChildContext( item, detailConfig )
               where ( childContext != null )
               select childContext );
    }

    private void OnDataGridContextCurrentItemChanged( object sender, EventArgs e )
    {
      Debug.Assert( sender == m_dataGridContext );

      this.ChangeCollectionViewCurrentItem();
    }

    private void OnCollectionViewCurrentChanged( object sender, EventArgs e )
    {
      Debug.Assert( sender == m_collectionView );

      this.ChangeDataGridContextCurrentItem();
    }

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    private bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CurrentItemChangedEventManager ) )
      {
        this.OnDataGridContextCurrentItemChanged( sender, e );
      }
      else if( managerType == typeof( CurrentChangedEventManager ) )
      {
        this.OnCollectionViewCurrentChanged( sender, e );
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    private readonly DataGridContext m_dataGridContext;
    private readonly CollectionView m_collectionView;
    private bool m_isCurrentChanging;
  }
}
