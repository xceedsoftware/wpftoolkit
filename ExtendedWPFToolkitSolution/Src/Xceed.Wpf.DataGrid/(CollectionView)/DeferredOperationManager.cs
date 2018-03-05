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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DeferredOperationManager
  {
    #region Static Fields

    private static readonly TimeSpan MaxProcessDuration = TimeSpan.FromMilliseconds( 100d );
    private static readonly TimeSpan MaxQueueDuration = TimeSpan.FromMilliseconds( 20d );

    #endregion

    internal DeferredOperationManager( DataGridCollectionViewBase collectionView, Dispatcher dispatcher, bool postPendingRefreshWithoutDispatching )
    {
      m_collectionView = collectionView;
      m_deferredOperations = new List<DeferredOperation>( 1000 );
      m_invalidatedGroups = new HashSet<DataGridCollectionViewGroup>();

      if( postPendingRefreshWithoutDispatching )
      {
        this.Add( new DeferredOperation( DeferredOperation.DeferredOperationAction.Refresh, -1, null ) );
      }

      m_dispatcher = dispatcher;
    }

    #region HasPendingOperations Property

    public bool HasPendingOperations
    {
      get
      {
        lock( this )
        {
          return ( m_pendingFlags.Data != 0 ) || ( m_deferredOperations.Count > 0 );
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

    #region DeferProcessOfInvalidatedGroupStats Property

    internal bool DeferProcessOfInvalidatedGroupStats
    {
      private get
      {
        return m_deferProcessOfInvalidatedGroupStats;
      }
      set
      {
        lock( this )
        {
          //Make sure that stats calculation that have been defer are processed before defering again!
          if( value && ( m_deferredOperationsToProcess > 0 ) )
          {
            m_forceProcessOfInvalidatedGroupStats = true;
          }
          m_deferProcessOfInvalidatedGroupStats = value;
        }
      }
    }

    private bool m_deferProcessOfInvalidatedGroupStats;

    #endregion

    public void ClearInvalidatedGroups()
    {
      lock( this )
      {
        if( ( m_dispatcherOperation != null ) && ( !this.HasPendingOperations ) )
        {
          m_dispatcherOperation.Abort();
          m_dispatcherOperation = null;
          m_dispatcherOperationStartTime = DateTime.MinValue;
          m_hasNewOperationsSinceStartTime = false;
        }

        m_invalidatedGroups.Clear();
      }
    }

    public void ClearDeferredOperations()
    {
      lock( this )
      {
        if( ( m_dispatcherOperation != null ) && ( m_invalidatedGroups.Count == 0 ) )
        {
          m_dispatcherOperation.Abort();
          m_dispatcherOperation = null;
          m_dispatcherOperationStartTime = DateTime.MinValue;
          m_hasNewOperationsSinceStartTime = false;
        }

        m_pendingFlags[ -1 ] = false;
        m_deferredOperations.Clear();
      }
    }

    public void PurgeAddWithRemoveOrReplace()
    {
      if( m_deferredOperations.Count >= 2 )
      {
        var addOperation = m_deferredOperations[ 0 ];

        if( addOperation.Action != DeferredOperation.DeferredOperationAction.Add )
          return;

        var addIndex = addOperation.NewStartingIndex;
        var addCount = addOperation.NewItems.Count;

        var firstIndexToRemove = 1;
        var lastIndexToRemove = -1;

        var count = m_deferredOperations.Count;
        for( int i = 1; i < count; i++ )
        {
          var operation = m_deferredOperations[ i ];

          var replaced = ( operation.Action == DeferredOperation.DeferredOperationAction.Replace );
          var removed = ( operation.Action == DeferredOperation.DeferredOperationAction.Remove );

          if( replaced || removed )
          {
            if( removed && ( i < count - 1 ) )
            {
              Debug.Fail( "How come we have a remove operation before the end?" );
              return;
            }

            if( ( addIndex == operation.OldStartingIndex ) && ( addCount == operation.OldItems.Count ) )
            {
              lastIndexToRemove = i;

              if( removed )
              {
                firstIndexToRemove = 0;
              }
            }
            else
            {
              Debug.Fail( "Why do we have a replace or remove operation with different indexes?" );
              return;
            }
          }
          else
          {
            // Can be normal, we can receive 2 adds for the same item and same position when we are bound to an IBindingList ( like a DataView )
            return;
          }
        }

        if( lastIndexToRemove > -1 )
        {
          m_deferredOperations.RemoveRange( firstIndexToRemove, ( lastIndexToRemove - firstIndexToRemove ) + 1 );
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
              m_deferredOperations.Clear();
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
              m_deferredOperations.Add( operation );
              m_hasNewOperationsSinceStartTime = true;

              if( this.DeferProcessOfInvalidatedGroupStats )
              {
                m_deferredOperationsToProcess++;
              }

              break;
            }
        }

        if( ( m_dispatcher != null ) && ( m_dispatcherOperation == null ) )
        {
          Debug.Assert( m_collectionView.Loaded );

          m_dispatcherOperation = m_dispatcher.BeginInvoke( DispatcherPriority.DataBind, new DispatcherOperationCallback( this.Dispatched_Process ), null );
          m_dispatcherOperationStartTime = DateTime.UtcNow;
          m_hasNewOperationsSinceStartTime = false;
        }
      }
    }

    public void Combine( DeferredOperationManager sourceDeferredOperationManager )
    {
      lock( this )
      {
        this.RefreshPending = this.RefreshPending || sourceDeferredOperationManager.RefreshPending;
        this.RegroupPending = this.RegroupPending || sourceDeferredOperationManager.RegroupPending;
        this.ResortPending = this.ResortPending || sourceDeferredOperationManager.ResortPending;
        this.RefreshDistincValuesPending = this.RefreshDistincValuesPending || sourceDeferredOperationManager.RefreshDistincValuesPending;
        this.RefreshDistincValuesWithFilteredItemChangedPending = this.RefreshDistincValuesWithFilteredItemChangedPending || sourceDeferredOperationManager.RefreshDistincValuesWithFilteredItemChangedPending;

        if( this.RefreshPending )
        {
          m_deferredOperations.Clear();
          return;
        }

        List<DeferredOperation> sourceOperations = sourceDeferredOperationManager.m_deferredOperations;

        if( sourceOperations.Count == 0 )
          return;

        m_deferredOperations.InsertRange( 0, sourceOperations );
      }
    }

    public void Process()
    {
      this.Process( true );
    }

    public void InvalidateGroupStats( DataGridCollectionViewGroup group, bool calculateAllStats = false, bool resortGroups = true )
    {
      if( group == null )
        return;

      lock( this )
      {
        if( this.RefreshPending )
          return;

        // When set to false, this will prevent group resorting when the DataGridCollectionView.ProcessInvalidatedGroupStats() is called through the disptached call below.
        // If the current method is called again and this variable is set to true before groups are processed, it will be fine, as the grid is now in a state that will support group resorting.
        m_resortGroups = resortGroups;

        var parent = group;
        while( parent != null )
        {
          if( !m_invalidatedGroups.Contains( parent ) )
          {
            m_invalidatedGroups.Add( parent );
          }

          parent = parent.Parent;
        }

        if( m_dispatcher != null )
        {
          if( m_dispatcherOperation == null )
          {
            m_dispatcherOperation = m_dispatcher.BeginInvoke( DispatcherPriority.DataBind, new DispatcherOperationCallback( this.Dispatched_Process ), null );
            m_dispatcherOperationStartTime = DateTime.UtcNow;
          }
          else
          {
            m_hasNewOperationsSinceStartTime = true;
          }
        }
      }
    }

    private object Dispatched_Process( object e )
    {
      this.Process( false );
      return null;
    }

    private void Process( bool processAll )
    {
      //This method will be called again when Dispose() is called on the DeferRefreshHelper of the CollectionView.
      if( m_collectionView.InDeferRefresh )
        return;

      if( !processAll )
      {
        lock( this )
        {
          if( ( m_hasNewOperationsSinceStartTime )
            && ( m_dispatcherOperation != null )
            && ( DateTime.UtcNow.Subtract( m_dispatcherOperationStartTime ) < DeferredOperationManager.MaxQueueDuration ) )
          {
            Debug.Assert( m_dispatcher != null );
            m_dispatcherOperation = m_dispatcher.BeginInvoke( m_dispatcherOperation.Priority, new DispatcherOperationCallback( this.Dispatched_Process ), null );
            m_hasNewOperationsSinceStartTime = false;

            return;
          }
        }
      }

      // We lock here since it is possible that a DataTable Column's value array is being redimensioned 
      // while we are trying to process the DeferredOperation.
      lock( m_collectionView.SyncRoot )
      {
        lock( this )
        {
          // Abort the current dispatcher operation
          if( m_dispatcherOperation != null )
          {
            m_dispatcherOperation.Abort();
            m_dispatcherOperation = null;
            m_dispatcherOperationStartTime = DateTime.MinValue;
            m_hasNewOperationsSinceStartTime = false;
          }

          // A deferredOperation can cause a DeferRefresh to be disposed of, hence provoking a re-entry in this method.
          // As a result, all variables must be cached to prevent re-executing the same deferredOperation.

          List<DeferredOperation> deferredOperations = m_deferredOperations;
          //Set a new list in case new operations are added while processing the current operations. 
          m_deferredOperations = new List<DeferredOperation>( 10 );
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
          bool refreshForced = false;

          m_collectionView.RaisePreBatchCollectionChanged();

          DateTime startTime = DateTime.UtcNow;

          if( refreshPending )
          {
            m_collectionView.ExecuteSourceItemOperation( new DeferredOperation( DeferredOperation.DeferredOperationAction.Refresh, -1, null ), out refreshForced );
          }
          else if( regroupPending )
          {
            m_collectionView.ExecuteSourceItemOperation( new DeferredOperation( DeferredOperation.DeferredOperationAction.Regroup, -1, null ), out refreshForced );
          }
          else if( resortPending )
          {
            m_collectionView.ExecuteSourceItemOperation( new DeferredOperation( DeferredOperation.DeferredOperationAction.Resort, -1, null ), out refreshForced );
          }

          if( ( deferredOperations.Count > 0 ) && ( !refreshForced ) )
          {
            int count = deferredOperations.Count;
            int operationIndex = 0;

            while( ( operationIndex < count ) && ( !refreshForced ) && ( processAll || ( DateTime.UtcNow.Subtract( startTime ) < DeferredOperationManager.MaxProcessDuration ) ) )
            {
              m_collectionView.ExecuteSourceItemOperation( deferredOperations[ operationIndex ], out refreshForced );
              operationIndex++;
            }

            if( ( operationIndex < count ) && ( !refreshForced ) )
            {
              deferredOperations.RemoveRange( 0, operationIndex );
              m_deferredOperationsToProcess -= operationIndex;
            }
            else
            {
              deferredOperations.Clear();
              m_deferredOperationsToProcess = 0;
            }
          }

          if( m_deferredOperations.Count == 0 )
          {
            m_deferredOperations = deferredOperations;
          }
          else
          {
            // When this method is re-entered, the processAll parameter should be true, which means all deferredOperations should have been processed. When assigning it back to m_deferredOperations,
            // it's count should be 0.  Thus in the previous call on the stack, the "if" condition should always be true, hence this assert.
            Debug.Assert( false, "Should never get there." );

            //To be GC friendly, keep the original list, which at one point is the one created with a capacity of a 1000 items.
            deferredOperations.InsertRange( deferredOperations.Count, m_deferredOperations );
            m_deferredOperations = deferredOperations;
          }

          // The recalculation of the StatFunctions is performed last.
          if( ( m_deferredOperationsToProcess <= 0 || m_forceProcessOfInvalidatedGroupStats ) && ( m_invalidatedGroups.Count != 0 ) )
          {
            m_collectionView.ProcessInvalidatedGroupStats( m_invalidatedGroups, m_resortGroups );
            m_invalidatedGroups.Clear();
            m_forceProcessOfInvalidatedGroupStats = false;
          }

          // Since a refresh has been forced, we don't want to process anything else.
          if( !refreshForced )
          {
            if( processAll || ( DateTime.UtcNow.Subtract( startTime ) < DeferredOperationManager.MaxProcessDuration ) )
            {
              if( refreshDistincValuesWithFilteredItemChangedPending || refreshDistincValuesPending )
              {
                m_collectionView.ExecuteSourceItemOperation( new DeferredOperation( DeferredOperation.DeferredOperationAction.RefreshDistincValues,
                                                                                    refreshDistincValuesWithFilteredItemChangedPending ), out refreshForced );
              }
            }
            else
            {
              if( refreshDistincValuesWithFilteredItemChangedPending )
              {
                this.RefreshDistincValuesWithFilteredItemChangedPending = true;
              }

              if( refreshDistincValuesPending )
              {
                this.RefreshDistincValuesPending = true;
              }
            }
          }

          m_collectionView.RaisePostBatchCollectionChanged();

          if( this.HasPendingOperations || m_invalidatedGroups.Count != 0 )
          {
            Debug.Assert( !processAll );
            Debug.Assert( m_dispatcher != null );

            // Requeue pending operations
            if( m_dispatcherOperation == null )
            {
              if( m_dispatcher != null )
              {
                m_dispatcherOperation = m_dispatcher.BeginInvoke( DispatcherPriority.Input, new DispatcherOperationCallback( this.Dispatched_Process ), null );
                m_dispatcherOperationStartTime = DateTime.UtcNow;
                m_hasNewOperationsSinceStartTime = false;
              }
            }
            else
            {
              m_dispatcherOperation.Priority = DispatcherPriority.Input;
              m_dispatcherOperationStartTime = DateTime.UtcNow;
            }
          }
        }
      }
    }

    private readonly DataGridCollectionViewBase m_collectionView;
    private List<DeferredOperation> m_deferredOperations;
    private HashSet<DataGridCollectionViewGroup> m_invalidatedGroups;
    private int m_deferredOperationsToProcess;
    private BitVector32 m_pendingFlags;
    private bool m_forceProcessOfInvalidatedGroupStats;
    private bool m_resortGroups;

    private readonly Dispatcher m_dispatcher;
    private DispatcherOperation m_dispatcherOperation;
    private DateTime m_dispatcherOperationStartTime;
    private bool m_hasNewOperationsSinceStartTime;

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
