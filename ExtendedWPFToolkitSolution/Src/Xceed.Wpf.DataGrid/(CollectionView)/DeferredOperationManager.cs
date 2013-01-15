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
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Xceed.Wpf.DataGrid
{

  internal class DeferredOperationManager
  {
    public DeferredOperationManager(
      DataGridCollectionViewBase collectionViewToUpdate,
      Dispatcher dispatcher,
      bool postPendingRefreshWithoutDispatching )
    {
      m_collectionViewToUpdate = collectionViewToUpdate;

      if( postPendingRefreshWithoutDispatching )
        this.Add( new DeferredOperation( DeferredOperation.DeferredOperationAction.Refresh, -1, null ) );

      m_dispatcher = dispatcher;
    }

    #region HasPendingOperations Property

    public bool HasPendingOperations
    {
      get
      {
        lock( this )
        {
          return ( m_pendingFlags.Data != 0 ) || ( m_deferredOperations != null );
        }
      }
    }

    #endregion

    #region RefreshPending Property

    public bool RefreshPending
    {
      get
      {
        return m_pendingFlags[ ( int )DeferredOperationManagerPendingFlags.RefreshPending ];
      }
      private set
      {
        m_pendingFlags[ ( int )DeferredOperationManagerPendingFlags.RefreshPending ] = value;
      }
    }

    #endregion

    #region ResortPending Property

    public bool ResortPending
    {
      get
      {
        return m_pendingFlags[ ( int )DeferredOperationManagerPendingFlags.ResortPending ];
      }
      private set
      {
        m_pendingFlags[ ( int )DeferredOperationManagerPendingFlags.ResortPending ] = value;
      }
    }

    #endregion

    #region RegroupPending Property

    public bool RegroupPending
    {
      get
      {
        return m_pendingFlags[ ( int )DeferredOperationManagerPendingFlags.RegroupPending ];
      }
      private set
      {
        m_pendingFlags[ ( int )DeferredOperationManagerPendingFlags.RegroupPending ] = value;
      }
    }

    #endregion

    #region RefreshDistincValuesPending Property

    public bool RefreshDistincValuesPending
    {
      get
      {
        return m_pendingFlags[ ( int )DeferredOperationManagerPendingFlags.RefreshDistincValuesPending ];
      }
      private set
      {
        m_pendingFlags[ ( int )DeferredOperationManagerPendingFlags.RefreshDistincValuesPending ] = value;
      }
    }

    #endregion

    #region RefreshDistincValuesWithFilteredItemChangedPending Property

    public bool RefreshDistincValuesWithFilteredItemChangedPending
    {
      get
      {
        return m_pendingFlags[ ( int )DeferredOperationManagerPendingFlags.RefreshDistincValuesWithFilteredItemChangedPending ];
      }
      private set
      {
        m_pendingFlags[ ( int )DeferredOperationManagerPendingFlags.RefreshDistincValuesWithFilteredItemChangedPending ] = value;
      }
    }

    #endregion

    public void ClearInvalidatedGroups()
    {
      lock( this )
      {
        if( ( m_dispatcherOperation != null ) && ( !this.HasPendingOperations ) )
        {
          m_dispatcherOperation.Abort();
          m_dispatcherOperation = null;
        }

        m_invalidatedGroups = null;
      }
    }

    public void ClearDeferredOperations()
    {
      lock( this )
      {
        if( ( m_dispatcherOperation != null ) && ( m_invalidatedGroups == null ) )
        {
          m_dispatcherOperation.Abort();
          m_dispatcherOperation = null;
        }

        m_pendingFlags[ -1 ] = false;
        m_deferredOperations = null;
      }
    }

    public void PurgeAddWithRemoveOrReplace()
    {
      if( m_deferredOperations == null )
        return;

      if( m_deferredOperations.Count >= 2 )
      {
        DeferredOperation addOperation = m_deferredOperations[ 0 ];

        if( addOperation.Action != DeferredOperation.DeferredOperationAction.Add )
          return;

        int addIndex = addOperation.NewStartingIndex;
        int addCount = addOperation.NewItems.Count;

        int firstIndexToRemove = 1;
        int lastIndexToRemove = -1;

        int count = m_deferredOperations.Count;
        for( int i = 1; i < count; i++ )
        {
          DeferredOperation operation = m_deferredOperations[ i ];

          bool replaced = ( operation.Action == DeferredOperation.DeferredOperationAction.Replace );
          bool removed = ( operation.Action == DeferredOperation.DeferredOperationAction.Remove );

          if( replaced || removed )
          {
            if( removed && ( i < count - 1 ) )
            {
              Debug.Fail( "How come we have a remove operation before the end?" );
              return;
            }

            if( ( addIndex == operation.OldStartingIndex )
              && ( addCount == operation.OldItems.Count ) )
            {
              lastIndexToRemove = i;

              if( removed )
                firstIndexToRemove = 0;
            }
            else
            {
              Debug.Fail( "Why do we have a replace or remove operation with different indexes?" );
              return;
            }
          }
          else
          {
            // Can be normal, we can receive 2 add for the same item and same position
            // when we are bound to an IBindingList ( like a DataView )
            return;
          }
        }

        if( lastIndexToRemove > -1 )
        {
          m_deferredOperations.RemoveRange(
            firstIndexToRemove,
            ( lastIndexToRemove - firstIndexToRemove ) + 1 );
        }
      }
    }

    public void Add( DeferredOperation operation )
    {
      lock( this )
      {
        if( this.RefreshPending )
          return;

        switch( operation.Action )
        {
          case DeferredOperation.DeferredOperationAction.Refresh:
            {
              this.RefreshPending = true;
              m_deferredOperations = null;
              break;
            }

          case DeferredOperation.DeferredOperationAction.RefreshDistincValues:
            {
              if( operation.FilteredItemsChanged )
              {
                this.RefreshDistincValuesWithFilteredItemChangedPending = true;
              }
              else
              {
                this.RefreshDistincValuesPending = true;
              }

              break;
            }

          case DeferredOperation.DeferredOperationAction.Regroup:
            {
              this.RegroupPending = true;
              break;
            }

          case DeferredOperation.DeferredOperationAction.Resort:
            {
              this.ResortPending = true;
              break;
            }

          default:
            {
              if( m_deferredOperations == null )
                m_deferredOperations = new List<DeferredOperation>();

              m_deferredOperations.Add( operation );
              break;
            }
        }

        if( ( m_dispatcher != null ) && ( m_dispatcherOperation == null ) )
        {
          Debug.Assert( m_collectionViewToUpdate.Loaded );

          m_dispatcherOperation = m_dispatcher.BeginInvoke(
            DispatcherPriority.DataBind,
            new DispatcherOperationCallback( this.Dispatched_Process ),
            null );
        }
      }
    }

    public void Combine( DeferredOperationManager sourceDeferredOperationManager )
    {
      lock( this )
      {
        this.RefreshPending |= sourceDeferredOperationManager.RefreshPending;
        this.RegroupPending |= sourceDeferredOperationManager.RegroupPending;
        this.ResortPending |= sourceDeferredOperationManager.ResortPending;

        this.RefreshDistincValuesPending |= sourceDeferredOperationManager.RefreshDistincValuesPending;

        this.RefreshDistincValuesWithFilteredItemChangedPending |=
          sourceDeferredOperationManager.RefreshDistincValuesWithFilteredItemChangedPending;

        if( this.RefreshPending )
        {
          m_deferredOperations = null;
          return;
        }

        List<DeferredOperation> sourceOperations = sourceDeferredOperationManager.m_deferredOperations;

        if( ( sourceOperations == null ) || ( sourceOperations.Count == 0 ) )
          return;

        if( m_deferredOperations == null )
          m_deferredOperations = new List<DeferredOperation>();

        m_deferredOperations.InsertRange( 0, sourceOperations );
      }
    }

    public void Process()
    {
      this.Process( true );
    }

    internal void InvalidateGroupStats( DataGridCollectionViewGroup group )
    {
      if( group == null )
        return;

      lock( this )
      {
        if( this.RefreshPending )
          return;

        if( m_invalidatedGroups == null )
          m_invalidatedGroups = new List<DataGridCollectionViewGroup>( 64 );

        DataGridCollectionViewGroup parent = group;

        while( parent != null )
        {
          if( !m_invalidatedGroups.Contains( parent ) )
          {
            parent.ClearStatFunctionsResult();
            m_invalidatedGroups.Add( parent );
          }

          parent = parent.Parent;
        }

        if( ( m_dispatcherOperation == null ) && ( m_dispatcher != null ) )
        {
          m_dispatcherOperation = m_dispatcher.BeginInvoke(
            DispatcherPriority.DataBind,
            new DispatcherOperationCallback( this.Dispatched_Process ),
            null );
        }
      }
    }

    public bool ContainsItemForRemoveOperation( object item )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      if( m_deferredOperations == null )
        return false;

      int deferredOperationsCount = m_deferredOperations.Count;

      for( int i = 0; i < deferredOperationsCount; i++ )
      {
        DeferredOperation deferredOperation = m_deferredOperations[ i ];

        if( deferredOperation.Action == DeferredOperation.DeferredOperationAction.Remove )
        {
          int oldItemsCount = ( deferredOperation.OldItems == null ) ? 0 : deferredOperation.OldItems.Count;

          for( int j = 0; j < oldItemsCount; j++ )
          {
            object oldItem = deferredOperation.OldItems[ j ];

            if( Object.Equals( oldItem, item ) )
              return true;
          }
        }
      }

      return false;
    }

    private object Dispatched_Process( object e )
    {
      this.Process( false );
      return null;
    }

    private void Process( bool processAll )
    {
      //This method will be called again when Dispose() is called on the DeferRefreshHelper of the CollectionView.
      if( m_collectionViewToUpdate.InDeferRefresh )
        return;

      bool refreshForced = false;

      // We lock here since it is possible that a DataTable Column's value array is being redimensioned 
      // while we are trying to process the DeferredOperation.
      lock( m_collectionViewToUpdate.SyncRoot )
      {
        lock( this )
        {
          // Abort the current dispatcher operation
          if( m_dispatcherOperation != null )
          {
            m_dispatcherOperation.Abort();
            m_dispatcherOperation = null;
          }

          int operationIndex = 0;

          long startTicks = DateTime.Now.Ticks;

          // The fact of processing a deferredOperations can cause other DeferRefresh on the CollectionView
          // that will finaly call this Process() again even if not yet completed.
          // So we must cache all the flags and deferredOperations in local variable to prevent
          // double execution of the operation.
          List<DeferredOperation> deferredOperations = m_deferredOperations;
          m_deferredOperations = null;
          bool refreshDistincValuesWithFilteredItemChangedPending = this.RefreshDistincValuesWithFilteredItemChangedPending;
          this.RefreshDistincValuesWithFilteredItemChangedPending = false;
          bool refreshDistincValuesPending = this.RefreshDistincValuesPending;
          this.RefreshDistincValuesPending = false;
          bool refreshPending = this.RefreshPending;
          this.RefreshPending = false;
          bool regroupPending = this.RegroupPending;
          this.RegroupPending = false;
          bool resortPending = this.ResortPending;
          this.ResortPending = false;
          List<DataGridCollectionViewGroup> invalidatedGroups = m_invalidatedGroups;
          m_invalidatedGroups = null;
          bool ensureGroupPosition = true;

          if( refreshPending )
          {
            Debug.Assert( deferredOperations == null );

            m_collectionViewToUpdate.ExecuteSourceItemOperation( new DeferredOperation(
              DeferredOperation.DeferredOperationAction.Refresh, -1, null ), out refreshForced );
          }
          else if( regroupPending )
          {
            ensureGroupPosition = false;

            // Regrouping also do resorting
            m_collectionViewToUpdate.ExecuteSourceItemOperation( new DeferredOperation(
              DeferredOperation.DeferredOperationAction.Regroup, -1, null ), out refreshForced );
          }
          else if( resortPending )
          {
            ensureGroupPosition = false;

            m_collectionViewToUpdate.ExecuteSourceItemOperation( new DeferredOperation(
              DeferredOperation.DeferredOperationAction.Resort, -1, null ), out refreshForced );
          }

          if( ( deferredOperations != null ) && ( !refreshForced ) )
          {
            int count = deferredOperations.Count;

            while( ( operationIndex < count ) && ( ( processAll ) || ( DateTime.Now.Ticks - startTicks ) < 1000000 ) && ( !refreshForced ) )
            {
              m_collectionViewToUpdate.ExecuteSourceItemOperation( deferredOperations[ operationIndex ], out refreshForced );
              operationIndex++;
            }

            if( ( operationIndex < count ) && ( !refreshForced ) )
            {
              deferredOperations.RemoveRange( 0, operationIndex );

              if( m_deferredOperations == null )
              {
                m_deferredOperations = deferredOperations;
              }
              else
              {
                Debug.Assert( false, "Should never get there." );
                m_deferredOperations.InsertRange( 0, deferredOperations );
              }
            }
          }

          // The recalculation of the StatFunctions is performed last.
          if( invalidatedGroups != null )
            m_collectionViewToUpdate.ProcessInvalidatedGroupStats( invalidatedGroups, ensureGroupPosition );

          // Since a refresh has been forced, we don't want to process anything else.
          if( !refreshForced )
          {
            if( ( ( processAll ) || ( DateTime.Now.Ticks - startTicks ) < 1000000 ) )
            {
              if( refreshDistincValuesWithFilteredItemChangedPending || refreshDistincValuesPending )
              {
                m_collectionViewToUpdate.ExecuteSourceItemOperation( new DeferredOperation(
                  DeferredOperation.DeferredOperationAction.RefreshDistincValues, refreshDistincValuesWithFilteredItemChangedPending ), out refreshForced );
              }
            }
            else
            {
              if( refreshDistincValuesWithFilteredItemChangedPending )
                this.RefreshDistincValuesWithFilteredItemChangedPending = true;

              if( refreshDistincValuesPending )
                this.RefreshDistincValuesPending = true;
            }
          }

          if( this.HasPendingOperations )
          {
            Debug.Assert( !processAll );
            Debug.Assert( m_dispatcher != null );

            // Requeue pending operations
            if( m_dispatcherOperation == null )
            {
              if( m_dispatcher != null )
              {
                m_dispatcherOperation = m_dispatcher.BeginInvoke(
                  DispatcherPriority.Input,
                  new DispatcherOperationCallback( this.Dispatched_Process ),
                  null );
              }
            }
            else
            {
              m_dispatcherOperation.Priority = DispatcherPriority.Input;
            }
          }
        }
      }
    }

    private DataGridCollectionViewBase m_collectionViewToUpdate;
    private List<DeferredOperation> m_deferredOperations;
    private BitVector32 m_pendingFlags;

    private Dispatcher m_dispatcher;
    private DispatcherOperation m_dispatcherOperation;

    private List<DataGridCollectionViewGroup> m_invalidatedGroups;

    [Flags]
    private enum DeferredOperationManagerPendingFlags
    {
      RegroupPending = 1,
      ResortPending = 2,
      RefreshPending = 4,
      RefreshDistincValuesPending = 8,
      RefreshDistincValuesWithFilteredItemChangedPending = 16
    }
  }
}
