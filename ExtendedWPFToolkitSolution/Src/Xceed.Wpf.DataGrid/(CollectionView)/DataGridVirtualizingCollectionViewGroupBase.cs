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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class DataGridVirtualizingCollectionViewGroupBase : CollectionViewGroup, IWeakEventListener, IDisposable
  {
    #region CONSTRUCTORS

    internal DataGridVirtualizingCollectionViewGroupBase(
      object name,
      int initialItemsCount,
      int startGlobalIndex,
      DataGridVirtualizingCollectionViewGroupBase parent,
      int level,
      bool isBottomLevel )
      : base( name )
    {
      m_parent = parent;
      m_level = level;
      m_isBottomLevel = isBottomLevel;
      m_virtualItemCount = initialItemsCount;
      m_startGlobalIndex = startGlobalIndex;
    }

    #endregion CONSTRUCTORS


    #region DO NOT USE THOSE

    #region Items PROPERTY

    [Obsolete( "The Items property is obsolete and has been replaced by the VirtualItems property. When referencing through a CollectionViewGroup, the CollectionViewGroupExtensions.GetItems extension method can be used.", true )]
    public new ReadOnlyObservableCollection<object> Items
    {
      get
      {
        return base.Items;
      }
    }

    #endregion Items PROPERTY

    #region ItemCount PROPERTY

    [Obsolete( "The ItemCount property is obsolete and has been replaced by the VirtualItemCount property. When referencing through a CollectionViewGroup, the CollectionViewGroupExtensions.GetItemCount extension method can be used.", true )]
    public new int ItemCount
    {
      get
      {
        return this.VirtualItemCount;
      }
    }

    #endregion ItemCount PROPERTY

    #endregion DO NOT USE THOSE


    #region IsBottomLevel PROPERTY

    public override bool IsBottomLevel
    {
      get
      {
        return m_isBottomLevel;
      }
    }

    #endregion IsBottomLevel PROPERTY


    #region DATA VIRTUALIZATION

    public int VirtualItemCount
    {
      get
      {
        if( this.GetVirtualPageManager() == null )
          return 0;

        if( m_virtualItemCount == -1 )
        {
          if( m_isBottomLevel )
          {
            this.EnsureProtectedVirtualItems();
            m_virtualItemCount = m_protectedVirtualItems.Count;
          }
          else
          {
            m_virtualItemCount = this.QueryItemCount();
          }
        }

        return m_virtualItemCount;
      }
    }

    internal abstract int QueryItemCount();

    public IList<object> VirtualItems
    {
      get
      {
        if( m_virtualItems == null )
        {
          this.EnsureProtectedVirtualItems();


          ObservableCollection<object> obserableProtectedItems =
            m_protectedVirtualItems as ObservableCollection<object>;

          if( obserableProtectedItems != null )
          {
            Debug.Assert( !m_isBottomLevel );
            m_virtualItems = new ReadOnlyObservableCollection<object>( obserableProtectedItems );
          }
          else
          {
            Debug.Assert( m_isBottomLevel );
            m_virtualItems = m_protectedVirtualItems;
          }
        }

        return m_virtualItems;
      }
    }

    private void EnsureProtectedVirtualItems()
    {
      if( m_protectedVirtualItems != null )
        return;

      DataGridVirtualizingCollectionViewBase collectionView = this.GetCollectionView();

      Debug.Assert( collectionView != null );

      if( m_isBottomLevel )
      {
        m_protectedVirtualItems = this.CreateNewVirtualList();
      }
      else
      {
        ObservableCollection<GroupDescription> groupDescriptions = collectionView.GroupDescriptions;

        int groupDescriptionCount = ( groupDescriptions == null ) ? 0 : groupDescriptions.Count;

        int nextLevel = m_level + 1;

        if( nextLevel > ( groupDescriptionCount - 1 ) )
          throw new DataGridInternalException( "At attempt was made to retrieve child groups for a non-existing group description." );

        GroupDescription subGroupBy = groupDescriptions[ nextLevel ];

        if( subGroupBy == null )
          throw new InvalidOperationException( "TODDOOC: " );

        bool nextLevelIsBottom = ( nextLevel == ( groupDescriptionCount - 1 ) );

        m_protectedVirtualItems = this.QuerySubCollectionViewGroupList( subGroupBy, nextLevel, nextLevelIsBottom );
      }
    }

    private VirtualList CreateNewVirtualList()
    {
      Debug.Assert( m_isBottomLevel );

      DataGridVirtualizingCollectionViewBase collectionView = this.GetCollectionView();

      DataGridPageManagerBase virtualPageManagerBase = collectionView.RootGroup.GetVirtualPageManager();

      VirtualList virtualItemList = new VirtualList( virtualPageManagerBase, m_virtualItemCount );
      virtualPageManagerBase.LinkVirtualListAndCollectionViewGroup( virtualItemList, this );

      return virtualItemList;
    }

    internal abstract ObservableCollection<object> QuerySubCollectionViewGroupList( GroupDescription subGroupBy, int nextLevel, bool nextLevelIsBottom );


    private int GetSubLevelCount()
    {
      CollectionView collectionView = this.GetCollectionView();

      ObservableCollection<GroupDescription> groupDescriptions = collectionView.GroupDescriptions;

      int groupDescriptionsCount = ( groupDescriptions == null ) ? 0 : groupDescriptions.Count;

      DataGridVirtualizingCollectionViewGroupBase parentCollectionViewGroup = this.Parent;

      int level = 0;
      while( parentCollectionViewGroup != null )
      {
        level++;
        parentCollectionViewGroup = parentCollectionViewGroup.Parent;
      }

      Debug.Assert( groupDescriptionsCount >= level );

      return ( groupDescriptionsCount - level );
    }

    internal virtual GroupDescription GetSubGroupBy()
    {
      CollectionView collectionView = this.GetCollectionView();

      ObservableCollection<GroupDescription> groupDescriptions = collectionView.GroupDescriptions;

      int groupDescriptionsCount = ( groupDescriptions == null ) ? 0 : groupDescriptions.Count;

      if( groupDescriptionsCount == 0 )
        return null;


      DataGridVirtualizingCollectionViewGroupBase parentCollectionViewGroup = this.Parent;

      int level = 0;
      while( parentCollectionViewGroup != null )
      {
        level++;
        parentCollectionViewGroup = parentCollectionViewGroup.Parent;
      }

      Debug.Assert( groupDescriptionsCount >= level );

      return groupDescriptions[ level ];
    }

    internal virtual void OnProtectedVirtualItemsCreated( IList<object> protectedVirtualItems )
    {
    }

    #endregion DATA VIRTUALIZATION


    #region PROTECTED METHODS

    protected virtual DataGridVirtualizingCollectionViewBase GetCollectionView()
    {
      if( m_parent != null )
        return m_parent.GetCollectionView();

      return null;
    }

    #endregion PROTECTED METHODS

    #region INTERNAL PROPERTIES

    internal DataGridVirtualizingCollectionViewGroupBase Parent
    {
      get
      {
        return m_parent;
      }
    }

    internal int Level
    {
      get
      {
        return m_level;
      }
    }

    internal int StartGlobalIndex
    {
      get
      {
        return m_startGlobalIndex;
      }
    }

    #endregion INTERNAL PROPERTIES

    #region INTERNAL METHODS

    private object OperateOnGlobalIndex( int index, Func<int, DataGridVirtualizingCollectionViewGroupBase, object> function )
    {
      this.EnsureProtectedVirtualItems();

      if( this.IsBottomLevel )
      {
        return function( index, this );
      }
      else
      {
        // The Count property of the virtualItems collection will return the sub-group count since this is not the bottom level.
        int count = m_protectedVirtualItems.Count;

        for( int i = 0; i < count; i++ )
        {
          DataGridVirtualizingCollectionViewGroupBase subGroup = m_protectedVirtualItems[ i ] as DataGridVirtualizingCollectionViewGroupBase;

          if( subGroup == null )
            throw new InvalidOperationException( "Sub-group cannot be null (Nothing in Visual Basic)." );

          // VirtualItemCount will return the sum of data items contained in this group and its possible subgroups.
          int subGroupItemCount = subGroup.VirtualItemCount;

          if( index < subGroupItemCount )
            return subGroup.OperateOnGlobalIndex( index, function );

          index -= subGroupItemCount;
        }
      }

      throw new ArgumentOutOfRangeException( "index" );
    }

    internal object GetItemAtGlobalIndex( int globalIndex )
    {
      Func<int, DataGridVirtualizingCollectionViewGroupBase, object> getItemFunction = ( localIndex, cvg ) => 
      {
        return cvg.m_protectedVirtualItems[ localIndex ];
      };

      return this.OperateOnGlobalIndex( globalIndex, getItemFunction );
    }

    internal void LockGlobalIndex( int globalIndex )
    {
      Func<int,DataGridVirtualizingCollectionViewGroupBase,object> lockingAction = ( localIndex, cvg ) => 
      {
        VirtualList virtualItemList = cvg.VirtualItems as VirtualList;

        if( virtualItemList != null )
        {
          virtualItemList.LockPageForLocalIndex( localIndex );
        }

        return null;
      };

      this.OperateOnGlobalIndex( globalIndex, lockingAction );
    }

    internal void UnlockGlobalIndex( int globalIndex )
    {
      var collectionView = this.GetCollectionView();

      if( collectionView != null )
      {
        var rootGroup = collectionView.RootGroup;

        if( ( rootGroup != null ) 
          && ( ( globalIndex >= rootGroup.m_virtualItemCount ) ) )
        {
          // This can happen when refreshing a CollectionView and the record count has changed in such a way that the global index
          // to unlock is out of range of the new count.
          return;
        }
      }

      Func<int, DataGridVirtualizingCollectionViewGroupBase, object> unlockingAction = ( localIndex, cvg ) => 
      {
        VirtualList virtualItemList = cvg.VirtualItems as VirtualList;

        if( virtualItemList != null )
        {
          virtualItemList.UnlockPageForLocalIndex( localIndex );
        }

        return null;
      };

      this.OperateOnGlobalIndex( globalIndex, unlockingAction );
    }

    internal virtual void DisposeCore()
    {
      m_protectedVirtualItems = new List<object>();
      m_virtualItems = m_protectedVirtualItems;
      m_parent = null;
    }

    internal virtual int GetGlobalIndexOf( object item )
    {
      return m_parent.GetGlobalIndexOf( item );
    }

    internal virtual DataGridPageManagerBase GetVirtualPageManager()
    {
      return m_parent.GetVirtualPageManager();
    }

    #endregion INTERNAL METHODS

    #region PRIVATE FIELDS

    private bool m_isBottomLevel;
    private int m_level;

    private int m_startGlobalIndex;
    private int m_virtualItemCount;

    private IList<object> m_protectedVirtualItems;
    private IList<object> m_virtualItems;

    private DataGridVirtualizingCollectionViewGroupBase m_parent;

    #endregion PRIVATE FIELDS


    #region WEAK EVENT HANDLER

    private void VirtualItemList_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      DataGridVirtualizingCollectionViewBase collectionView = this.GetCollectionView();

      lock( collectionView.DeferredOperationManager )
      {
        DeferredOperation deferredOperation = null;

        switch( e.Action )
        {
          case NotifyCollectionChangedAction.Replace:
            {
              deferredOperation = new DeferredOperation(
                DeferredOperation.DeferredOperationAction.Replace, -1,
                e.NewStartingIndex + m_startGlobalIndex,
                e.NewItems,
                e.OldStartingIndex + m_startGlobalIndex,
                e.OldItems );

              break;
            }

          case NotifyCollectionChangedAction.Reset:
            {
              deferredOperation = new DeferredOperation(
                DeferredOperation.DeferredOperationAction.Refresh, -1, null );

              break;
            }

          default:
            throw new NotSupportedException( e.Action.ToString() + " is not a supported action." );
        }

        if( deferredOperation != null )
          collectionView.ExecuteOrQueueSourceItemOperation( deferredOperation );
      }
    }

    #endregion WEAK EVENT HANDLER

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        this.VirtualItemList_CollectionChanged( sender, ( NotifyCollectionChangedEventArgs )e );
        return true;
      }

      return false;
    }

    #endregion IWeakEventListener Members


    #region IDisposable Members

    public void Dispose()
    {
      this.DisposeCore();
    }

    #endregion
  }
}
