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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Xceed.Utils.Wpf;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class ColumnHierarchyManager : IWeakEventListener
  {
    internal ColumnHierarchyManager( ColumnCollection columns)
    {
      if( columns == null )
        throw new ArgumentNullException( "columns" );

      m_columns = columns;
      m_visibleColumns = new ReadOnlyColumnCollection( new ObservableColumnCollection() );
      m_columnsByVisiblePosition = new HashedLinkedList<ColumnBase>();

      m_model.LayoutChanging += new EventHandler<ColumnHierarchyModel.LayoutChangingEventArgs>( this.OnViewLayoutChanging );
      m_model.LayoutChanged += new EventHandler<ColumnHierarchyModel.LayoutChangedEventArgs>( this.OnViewLayoutChanged );
      m_model.StatusChanged += new EventHandler<ColumnHierarchyModel.StatusChangedEventArgs>( this.OnViewStatusChanged );
    }

    #region Columns Property

    internal ColumnCollection Columns
    {
      get
      {
        return m_columns;
      }
    }

    private readonly ColumnCollection m_columns;

    #endregion

    #region VisibleColumns Property

    internal ReadOnlyColumnCollection VisibleColumns
    {
      get
      {
        return m_visibleColumns;
      }
    }

    private readonly ReadOnlyColumnCollection m_visibleColumns;

    #endregion

    #region ColumnsByVisiblePosition Property

    internal HashedLinkedList<ColumnBase> ColumnsByVisiblePosition
    {
      get
      {
        return m_columnsByVisiblePosition;
      }
    }

    private readonly HashedLinkedList<ColumnBase> m_columnsByVisiblePosition;

    #endregion

    #region IsUpdateDeferred Property

    internal bool IsUpdateDeferred
    {
      get
      {
        lock( this.SyncRoot )
        {
          return ( m_deferUpdateCount != 0 );
        }
      }
    }

    #endregion

    #region SyncRoot Private Property

    private object SyncRoot
    {
      get
      {
        return m_model;
      }
    }

    #endregion

    #region LayoutChanging Event

    internal event EventHandler LayoutChanging;

    private void OnLayoutChanging()
    {
      var handler = this.LayoutChanging;
      if( handler == null )
        return;

      handler.Invoke( this, EventArgs.Empty );
    }

    #endregion

    #region LayoutChanged Event

    internal event EventHandler LayoutChanged;

    private void OnLayoutChanged()
    {
      var handler = this.LayoutChanged;
      if( handler == null )
        return;

      handler.Invoke( this, EventArgs.Empty );
    }

    #endregion

    internal void Initialize( DataGridControl dataGridControl )
    {
      if( dataGridControl == null )
        throw new ArgumentNullException( "dataGridControl" );

      if( m_dataGridControl == dataGridControl )
        return;

      if( m_dataGridControl != null )
        throw new InvalidOperationException( "The current object is already initialized." );

      m_dataGridControl = dataGridControl;
      m_columns.DataGridControl = dataGridControl;

      this.PrepareInitialLayout();
    }

    internal void Clear()
    {
      if( m_dataGridControl == null )
        return;

      this.ClearLayout();

      m_columns.DataGridControl = null;

      m_dataGridControl = null;
      m_desiredFixedColumnCount = null;
      m_synchronizeChildColumnNamesWithChildColumns = false;
    }

    internal IDisposable DeferUpdate()
    {
      return this.DeferUpdate( new UpdateOptions() );
    }

    internal IDisposable DeferUpdate( UpdateOptions options )
    {
      return new DeferredDisposable( new DeferUpdateState( this, options ) );
    }

    internal ILevelMarkers GetLevelMarkersFor( ColumnCollection collection )
    {
      if( collection == null )
        return null;

      var level = int.MinValue;

      if( collection == m_columns )
      {
        level = 0;
      }

      if( ( level < 0 ) || ( level >= m_model.LevelCount ) )
        return null;

      return this.WrapLevelMarkers( m_model.GetLevelMarkers( level ) );
    }

    internal IColumnLocation GetColumnLocationFor( ColumnBase column )
    {
      return ( IColumnLocation )this.WrapLocation( m_model[ column ] );
    }

    internal int GetFixedColumnCount()
    {
      var levelCount = m_model.LevelCount;
      if( levelCount == 0 )
        return 0;

      // Get the top-most level.
      var markers = m_model.GetLevelMarkers( levelCount - 1 );

      var splitterLocation = markers.Splitter;
      if( splitterLocation == null )
        return 0;

      var count = 0;
      var location = splitterLocation.GetPreviousSiblingOrCousin();

      // Count the number of visibile columns that are located before the splitter.
      while( location != null )
      {
        if( location.Type == LocationType.Column )
        {
          var columnLocation = ( ColumnHierarchyModel.IColumnLocation )location;
          if( columnLocation.Column.Visible )
          {
            count++;
          }
        }

        location = location.GetPreviousSiblingOrCousin();
      }

      return count;
    }

    internal void SetFixedColumnCount( int count )
    {
      this.Update( new UpdateOptions( count ) );
    }

    private static int ConvertLevelToMergedHeaderIndex( int level, int mergedHeaderCount )
    {
      if( level <= 0 )
        throw new ArgumentException( "The level must be greater than or equal to 1.", "level" );

      if( mergedHeaderCount <= 0 )
        throw new ArgumentException( "The number of merged headers is expected to be greater than or equal to 1.", "mergedHeaderCount" );

      if( level > mergedHeaderCount )
        throw new ArgumentException( "The level must be less than or equal to the number of merged headers.", "level" );

      return ( mergedHeaderCount - level );
    }

    private static int ConvertMergedHeaderIndexToLevel( int index, int mergedHeaderCount )
    {
      if( index < 0 )
        throw new ArgumentException( "The index must be greater than or equal to zero.", "index" );

      if( mergedHeaderCount <= 0 )
        throw new ArgumentException( "The number of merged headers is expected to be greater than or equal to 1.", "mergedHeaderCount" );

      if( index >= mergedHeaderCount )
        throw new ArgumentException( "The index must be less than the number of merged headers.", "index" );

      return ( mergedHeaderCount - index );
    }

    private void PrepareInitialLayout()
    {
      Debug.Assert( m_model.LevelCount == 0 );

      this.RegisterEvents( m_columns );

      m_model.AddLevel( 0 );

      this.SetColumnsInView( 0 );    

      this.UpdateLevelsInView();
      this.Update();
    }

    private void ClearLayout()
    {
      for( int i = m_model.LevelCount - 1; i > 0; i-- )
      {
        this.RemoveLevelInView( i );
      }

      Debug.Assert( m_model.LevelCount == 1 );

      foreach( var column in this.GetColumns( 0 ) )
      {
        this.UnregisterEvents( column );
      }

      m_model.RemoveLevel( 0 );

      Debug.Assert( m_model.LevelCount == 0 );

      this.UnregisterEvents( m_columns );
    }

    private void Update()
    {
      this.Update( new UpdateOptions() );
    }

    private void Update( UpdateOptions options )
    {
      lock( this.SyncRoot )
      {
        try
        {
          // The current object has not been initialized yet.
          if( m_dataGridControl == null )
            return;

          // The update is deferred and must not be applied yet.
          if( m_deferUpdateCount != 0 )
            return;
        }
        finally
        {
          if( options.DesiredFixedColumnCount.HasValue )
          {
            m_desiredFixedColumnCount = Math.Max( 0, options.DesiredFixedColumnCount.Value );
          }

          if( !m_synchronizeChildColumnNamesWithChildColumns && ( options.SynchronizeChildColumnNamesWithChildColumns == true ) )
          {
            m_synchronizeChildColumnNamesWithChildColumns = true;

            for( int i = m_model.LevelCount - 1; i > 0; i-- )
            {
              foreach( var columnLocation in this.GetColumnsLocation( this.GetLocationsOnLevelFromFirstToLast( i ) ) )
              {
                this.SetPosition( columnLocation, columnLocation.Status.Position.SetChildrenTimestamp( this.GetTimestamp() ) );
              }
            }

            this.InvalidateLayout();
          }
        }
      }

      // Prevent reentrancy.
      if( m_isApplyingChanges.IsSet )
        return;

      this.UpdateLayout();
    }

    private void UpdateLayout()
    {
      Debug.Assert( !m_isApplyingChanges.IsSet );

      if( ( m_positionVersion == m_modelPositionVersion ) && ( m_visibilityVersion == m_modelVisibilityVersion ) && ( m_layoutVersion == m_modelLayoutVersion ) )
      {
        // Nothing has changed, so nothing to do.
        if( ( !m_desiredFixedColumnCount.HasValue ) || ( m_desiredFixedColumnCount == this.GetFixedColumnCount() ) )
        {
          m_desiredFixedColumnCount = null;

          this.ResetTimestamp();
          return;
        }
      }

      this.OnLayoutChanging();

      this.ResetMainColumn();

      while( true )
      {
        if( m_positionVersion != m_modelPositionVersion )
        {
          m_positionVersion = m_modelPositionVersion;
          this.ApplyPosition();
        }
        else if( m_visibilityVersion != m_modelVisibilityVersion )
        {
          m_visibilityVersion = m_modelVisibilityVersion;
          this.ApplyVisibility();
        }
        else if( m_desiredFixedColumnCount.HasValue )
        {
          var fixedColumnCount = m_desiredFixedColumnCount.Value;
          m_desiredFixedColumnCount = null;

          this.ApplyFixedColumnCount( fixedColumnCount );
        }
        else if( m_layoutVersion != m_modelLayoutVersion )
        {
          m_layoutVersion = m_modelLayoutVersion;
          this.ApplyLayout();
        }
        else
        {
          this.ResetTimestamp();
          break;
        }
      }

      Debug.Assert( m_timestamp == 0u );
      Debug.Assert( !m_desiredFixedColumnCount.HasValue );
      Debug.Assert( m_positionVersion == m_modelPositionVersion );
      Debug.Assert( m_visibilityVersion == m_modelVisibilityVersion );
      Debug.Assert( m_layoutVersion == m_modelLayoutVersion );

      this.OnLayoutChanged();
    }

    private void ApplyLayout()
    {
      if( m_isApplyingChanges.IsSet )
        throw new InvalidOperationException( "The columns' layout is already being applied." );

      using( m_isApplyingChanges.Set() )
      {
        for( int i = m_model.LevelCount - 1; i >= 0; i-- )
        {
          this.ApplyColumnsLayoutFromView( i );

          if( i == 0 )
          {
            this.SetVisibleColumnsFromView( i, m_visibleColumns, m_columnsByVisiblePosition );
          }
        }
      }
    }

    private void ApplyPosition()
    {
      if( m_isApplyingChanges.IsSet )
        throw new InvalidOperationException( "The columns' relations are already being applied." );

      var levelCount = m_model.LevelCount;
      if( levelCount <= 0 )
        return;

      using( m_isApplyingChanges.Set() )
      {
        this.UpdateLevelsInView();
      }
    }

    private void ApplyVisibility()
    {
      if( m_isApplyingChanges.IsSet )
        throw new InvalidOperationException( "The columns' visibility is already being applied." );

      var levelCount = m_model.LevelCount;
      if( levelCount <= 0 )
        return;

      using( m_isApplyingChanges.Set() )
      {
        foreach( var location in this.GetLocationsOnLevel( levelCount - 1 ) )
        {
          // We must redo a visibility pass if the layout has changed while applying the visiblity.
          if( !this.ApplyColumnsVisibilityFromView( location ) )
            return;
        }
      }
    }

    private void ApplyFixedColumnCount( int count )
    {
      Debug.Assert( count >= 0 );

      // There is no level or the splitter is already at the proper location.
      var levelCount = m_model.LevelCount;
      if( ( levelCount == 0 ) || ( count == this.GetFixedColumnCount() ) )
        return;

      // Get the top-most level.
      var markers = m_model.GetLevelMarkers( levelCount - 1 );

      var startLocation = markers.Start;
      Debug.Assert( startLocation != null );
      Debug.Assert( startLocation.Type == LocationType.Start );

      var splitterLocation = markers.Splitter;
      Debug.Assert( splitterLocation != null );
      Debug.Assert( splitterLocation.Type == LocationType.Splitter );

      var targetLocation = default( ColumnHierarchyModel.ILocation );
      var moveBefore = false;

      if( count <= 0 )
      {
        targetLocation = startLocation;
      }
      else
      {
        var remaining = count;
        var currentLocation = startLocation;
        var found = false;

        while( !found )
        {
          currentLocation = currentLocation.GetNextSiblingOrCousin();
          if( currentLocation == null )
            throw new InvalidOperationException( "Unexpected location." );

          switch( currentLocation.Type )
          {
            case LocationType.Column:
              {
                var column = ( ( ColumnHierarchyModel.IColumnLocation )currentLocation ).Column;
                Debug.Assert( column != null );

                if( column.Visible )
                {
                  remaining--;

                  if( remaining == 0 )
                  {
                    targetLocation = currentLocation;
                    found = true;
                    break;
                  }
                }
              }
              break;

            // The splitter cannot be set past the orphan section.
            case LocationType.Orphan:
              {
                targetLocation = currentLocation;
                moveBefore = true;
                found = true;
              }
              break;
          }
        }
      }

      Debug.Assert( targetLocation != null );

      if( moveBefore )
      {
        if( !splitterLocation.CanMoveBefore( targetLocation ) )
          throw new InvalidOperationException( "The fixed-column splitter cannot be moved." );

        splitterLocation.MoveBefore( targetLocation );
      }
      else
      {
        if( !splitterLocation.CanMoveAfter( targetLocation ) )
          throw new InvalidOperationException( "The fixed-column splitter cannot be moved." );

        splitterLocation.MoveAfter( targetLocation );
      }
    }

    private void ResetStatus()
    {
      if( ( m_timestamp == 0u ) || ( m_model.LevelCount <= 0 ) )
        return;

      foreach( var location in this.GetLocationsOnLevel( m_model.LevelCount - 1 ) )
      {
        this.ResetStatusOnLocation( location );
      }
    }

    private void ResetStatusOnLocation( ColumnHierarchyModel.ILocation location )
    {
      if( location == null )
        return;

      var columnLocation = location as ColumnHierarchyModel.IColumnLocation;
      if( columnLocation != null )
      {
        this.SetStatus( columnLocation, new ColumnStatus( ColumnPositionStatus.Default, ColumnVisibilityStatus.Create( columnLocation.Column.Visible ) ) );
      }

      foreach( var childLocation in this.GetChildLocations( location ) )
      {
        this.ResetStatusOnLocation( childLocation );
      }
    }

    private void InsertLevelInView( int level )
    {
      Debug.Assert( ( level > 0 ) && ( level <= m_model.LevelCount ) );

      m_model.AddLevel( level );

      this.SetColumnsInView( level );
    }

    private void RemoveLevelInView( int level )
    {
      Debug.Assert( ( level > 0 ) && ( level < m_model.LevelCount ) );

      foreach( var location in this.GetLocationsOnLevel( level ) )
      {
        var parentColumnLocation = location.GetParent() as ColumnHierarchyModel.IColumnLocation;
        if( parentColumnLocation != null )
        {
          this.UpdateVisibility( parentColumnLocation );
        }

        var columnLocation = location as ColumnHierarchyModel.IColumnLocation;
        if( columnLocation != null )
        {
          this.UnregisterEvents( columnLocation.Column );
        }
      }

      m_model.RemoveLevel( level );
    }

    private void UpdateLevelsInView()
    {
      var levelCount = m_model.LevelCount;
      if( levelCount == 0 )
        return;

      if( levelCount > 1 )
      {
        var movedLocations = new HashSet<ColumnHierarchyModel.IColumnLocation>();
        var childLocationsToMove = new Queue<ColumnHierarchyModel.IColumnLocation>();
        var parentLocationsToInspect = new Queue<ColumnHierarchyModel.IColumnLocation>();
        var relationsHistory = new List<ParentChildRelationshipSnapshot>[ levelCount ];
        var relationsHistoryComparer = new ParentChildRelationshipSnapshotComparer();

        for( int i = 0; i < levelCount; i++ )
        {
          relationsHistory[ i ] = new List<ParentChildRelationshipSnapshot>();

          foreach( var columnLocation in this.GetColumnsLocation( this.GetLocationsOnLevel( i ) ) )
          {
            var positionStatus = columnLocation.Status.Position;

            if( ( positionStatus.ParentTimestamp != 0u ) && ( i < levelCount - 1 ) )
            {
              childLocationsToMove.Enqueue( columnLocation );
            }

            if( ( positionStatus.ChildrenTimestamp != 0u ) && ( i > 0 ) )
            {
              parentLocationsToInspect.Enqueue( columnLocation );

              foreach( var childColumnLocation in this.GetColumnsLocation( this.GetChildLocations( columnLocation ) ) )
              {
                childLocationsToMove.Enqueue( childColumnLocation );
              }

              var entry = new ParentChildRelationshipSnapshot( columnLocation, positionStatus.ChildrenTimestamp );
              var history = relationsHistory[ i ];

              // Insert the entries in a specific order to meet the GetChildColumnLocationDestination method needs.
              var index = history.BinarySearch( entry, relationsHistoryComparer );

              if( index < 0 )
              {
                index = ~index;
              }

              if( index >= history.Count )
              {
                history.Add( entry );
              }
              else
              {
                history.Insert( index, entry );
              }
            }
          }
        }
      }

      var isAFlattenDetail = ( m_columns.ParentDetailConfiguration != null )
                          && ( m_dataGridControl != null )
                          && ( m_dataGridControl.AreDetailsFlatten );

      // Reorder the columns on each level.
      for( int i = 0; i < levelCount; i++ )
      {
        // Reorder one of the child level.
        if( i < levelCount - 1 )
        {
          foreach( var parentLocation in this.GetLocationsOnLevel( i + 1 ) )
          {
            ColumnLocationOrderer.Reorder( this.GetColumnsLocation( this.GetChildLocationsFromFirstToLast( parentLocation ) ).ToList(), !isAFlattenDetail );
          }
        }
        // Reorder the top level.
        else
        {
          ColumnLocationOrderer.Reorder( this.GetColumnsLocation( this.GetLocationsOnLevelFromFirstToLast( i ) ).ToList(), !isAFlattenDetail );
        }
      }
    }

    private Tuple<bool, bool> SetColumnsInView( int level )
    {
      var columns = m_columns;
      var addAny = false;

      Debug.Assert( columns != null );

      var columnsToRemove = new HashSet<ColumnBase>( this.GetColumns( level ) );

      // Add the missing columns into the view.
      foreach( var column in columns )
      {
        if( !columnsToRemove.Remove( column ) )
        {
          var propagation = PropagationMode.None;

          if( level > 0 )
          {
            var visibilitySource = DependencyPropertyHelper.GetValueSource( column, ColumnBase.VisibleProperty );

            propagation = ( visibilitySource.BaseValueSource == BaseValueSource.Default ) ? PropagationMode.BottomUp : PropagationMode.TopDown;
          }

          this.AddColumnInView( column, propagation, level );
          addAny = true;
        }
      }

      var removedAny = false;

      // Remove the columns that should no longer be in the view.
      foreach( var column in columnsToRemove )
      {
        this.RemoveColumnInView( column );
        removedAny = true;
      }

      return new Tuple<bool, bool>( addAny, removedAny );
    }

    private void ResetMainColumn()
    {
      // The main column must only be reset when details are flatten because it impact column positioning.
      if( ( m_dataGridControl == null ) || !m_dataGridControl.AreDetailsFlatten )
        return;

      if( m_isApplyingChanges.IsSet )
        throw new InvalidOperationException( "The columns' relations are already being applied." );

      var levelCount = m_model.LevelCount;
      if( levelCount <= 0 )
        return;

      using( m_isApplyingChanges.Set() )
      {
        var mainColumn = m_columns.MainColumn;
        if( mainColumn != null )
        {
          // We must not reset the main column if it was set explicitly.
          var mainColumnLocation = m_model[ mainColumn ];
          if( ( mainColumnLocation == null ) || ( mainColumnLocation.Status.Position.MainColumnTimestamp != 0u ) )
            return;

          m_columns.MainColumn = null;
        }
      }
    }

    private ColumnHierarchyModel.IColumnLocation AddColumnInView( ColumnBase column, PropagationMode mode, int level )
    {
      Debug.Assert( column != null );
      Debug.Assert( ( level >= 0 ) && ( level < m_model.LevelCount ) );

      var location = m_model[ column ];
      if( location == null )
      {
        var timestamp = this.GetTimestamp();
        var visibilityStatus = ColumnVisibilityStatus.Create( column.Visible, mode, timestamp );
        var positionStatus = ColumnPositionStatus.Default;

        if( level > 0 )
        {
          positionStatus = positionStatus.SetChildrenTimestamp( timestamp );
        }

        switch( DependencyPropertyHelper.GetValueSource( column, ColumnBase.VisiblePositionProperty ).BaseValueSource )
        {
          case BaseValueSource.Default:
          case BaseValueSource.Inherited:
          case BaseValueSource.Unknown:
            break;

          default:
            positionStatus = positionStatus.SetVisiblePositionTimestamp( timestamp );
            break;
        }

        var columnStatus = new ColumnStatus( positionStatus, visibilityStatus );

        location = m_model.Add( column, columnStatus, level );
        Debug.Assert( location != null );

        this.RegisterEvents( column );
      }

      return location;
    }

    private void RemoveColumnInView( ColumnBase column )
    {
      Debug.Assert( column != null );

      var location = m_model[ column ];
      if( location == null )
        return;

      var parentColumnLocation = location.GetParent() as ColumnHierarchyModel.IColumnLocation;
      if( parentColumnLocation != null )
      {
        this.UpdateVisibility( parentColumnLocation );
      }

      m_model.Remove( column );

      this.UnregisterEvents( column );
    }

    private ColumnHierarchyModel.IColumnLocation ReplaceColumnInView( ColumnBase oldColumn, ColumnBase newColumn )
    {
      Debug.Assert( ( oldColumn != null ) && ( newColumn != null ) );

      var oldColumnLocation = m_model[ oldColumn ];
      if( ( oldColumn == newColumn ) || ( oldColumnLocation == null ) )
        return oldColumnLocation;

      var newColumnLocation = m_model[ newColumn ];
      if( newColumnLocation == null )
      {
        newColumnLocation = this.AddColumnInView( newColumn, PropagationMode.BottomUp, oldColumnLocation.Level );
        Debug.Assert( newColumnLocation != null );
      }
      else if( newColumnLocation.Level != oldColumnLocation.Level )
      {
        throw new InvalidOperationException( "The replaced column is not on the same level." );
      }

      Debug.Assert( newColumnLocation.CanMoveAfter( oldColumnLocation ) );
      newColumnLocation.MoveAfter( oldColumnLocation );

      // Update the location timestamp to keep the column under the parent column.
      this.SetPosition( newColumnLocation, newColumnLocation.Status.Position.SetLocationTimestamp( this.GetTimestamp() ) );
      this.SetPosition( newColumnLocation, newColumnLocation.Status.Position.SetDraggableStatusTimestamp( this.GetTimestamp() ) );

      var lastChildColumnLocation = this.GetColumnsLocation( this.GetChildLocationsFromFirstToLast( newColumnLocation ) ).FirstOrDefault();
      var childColumnLocations = this.GetColumnsLocation( this.GetChildLocationsFromFirstToLast( oldColumnLocation ) ).ToList();

      // Move the columns under the new column and keep them in the same order.
      foreach( var childColumnLocation in childColumnLocations )
      {
        if( lastChildColumnLocation != null )
        {
          Debug.Assert( childColumnLocation.CanMoveAfter( lastChildColumnLocation ) );
          childColumnLocation.MoveAfter( lastChildColumnLocation );
        }
        else
        {
          Debug.Assert( childColumnLocation.CanMoveUnder( newColumnLocation ) );
          childColumnLocation.MoveUnder( newColumnLocation );
        }

        lastChildColumnLocation = childColumnLocation;
      }

      // Reserve a timestamp that is going to be used to keep child columns under their new parent column.
      // We want that timestamp to be of "lower priority" than any visible position timestamp update
      // on the child columns so a reordering may be done.
      var locationTimestamp = this.GetTimestamp();

      // Update the timestamps on the transfered child columns to keep them under their new parent column.
      foreach( var entry in ( from childColumnLocation in childColumnLocations
                              let positionStatus = childColumnLocation.Status.Position
                              orderby positionStatus.VisiblePositionTimestamp
                              select new
                              {
                                Location = childColumnLocation,
                                Status = positionStatus
                              } ) )
      {
        var positionStatus = entry.Status.SetLocationTimestamp( locationTimestamp );

        if( positionStatus.VisiblePositionTimestamp != 0u )
        {
          // Since a visible position reordering was scheduled before the parent column was replaced,
          // we must update the visible position timestamp to apply the reordering in the same order
          // under the new parent column.
          positionStatus = positionStatus.SetVisiblePositionTimestamp( this.GetTimestamp() );
        }

        this.SetPosition( entry.Location, positionStatus );
      }

      this.RemoveColumnInView( oldColumn );

      return newColumnLocation;
    }

    private bool MoveChildColumnUnderLocationInView( ColumnHierarchyModel.IColumnLocation location, ColumnHierarchyModel.ILocation pivot )
    {
      if( ( location == null ) || ( pivot == null ) )
        return false;

      // The column is already at the proper location.
      if( object.Equals( location.GetParent(), pivot ) )
        return false;

      Debug.Assert( location.CanMoveUnder( pivot ) );
      location.MoveUnder( pivot );

      var positionStatus = location.Status.Position;
      if( positionStatus.VisiblePositionTimestamp == 0u )
      {
        // Update the child column status to move it at the right place under its new parent location.
        this.SetPosition( location, positionStatus.SetVisiblePositionTimestamp( this.GetTimestamp() ) );
      }

      return true;
    }

    private ColumnHierarchyModel.ILocation GetChildColumnLocationDestination( ColumnHierarchyModel.IColumnLocation location, IList<ParentChildRelationshipSnapshot> relationsHistory )
    {
      Debug.Assert( location != null );
      Debug.Assert( ( location.Level >= 0 ) && ( location.Level < m_model.LevelCount - 1 ) );


      var column = location.Column;
      var positionStatus = location.Status.Position;
      var orphanLocation = m_model.GetLevelMarkers( location.Level + 1 ).Orphan;
      Debug.Assert( orphanLocation != null );

      var fallbackParentLocation = location.GetParent();
      var fallbackTimestamp = 0u;

      // Since the target column wants to be under a specific column (or orphan), we must consider the new parent location
      // as a possible candidate if it is not one already.
      if( ( positionStatus.ParentTimestamp != 0u ) || ( positionStatus.LocationTimestamp != 0u ) )
      {
        if( positionStatus.ParentTimestamp > positionStatus.LocationTimestamp )
        {         
          fallbackParentLocation = orphanLocation;
          fallbackTimestamp = positionStatus.ParentTimestamp;
        }
        else
        {

          fallbackParentLocation = location.GetParent();
          fallbackTimestamp = positionStatus.LocationTimestamp;

          Debug.Assert( object.Equals( fallbackParentLocation, location.GetParent() ) );
        }
      }

      // Figure out if one of the candidates wants the column.
      foreach( var relationHistory in relationsHistory )
      {
        // Since the fallback parent location wants the column and it has an higher priority than any remaining
        // candidates, there is no need to look for a candidate.
        if( relationHistory.Timestamp < fallbackTimestamp )
          break;

        // Since the fallback location is the candidate and it does not want the child column anymore,
        // the column should become orphan.
        if( object.Equals( fallbackParentLocation, relationHistory.Location ) )
          return orphanLocation;
      }

      Debug.Assert( fallbackParentLocation != null );
      return fallbackParentLocation;
    }

    private void ApplyColumnsLayoutFromView( int level )
    {
      var location = this.GetLocationsOnLevelFromFirstToLast( level ).FirstOrDefault();
      var position = 0;

      while( location != null )
      {
        var columnLocation = location as ColumnHierarchyModel.IColumnLocation;
        if( columnLocation == null )
        {
          location = location.GetNextSiblingOrCousin();
          continue;
        }

        var parentColumnLocation = columnLocation.GetParent() as ColumnHierarchyModel.IColumnLocation;
        var column = columnLocation.Column;

        column.VisiblePosition = position;

        if( position == 0 )
        {
          var columns = column.ContainingCollection;
          if( columns != null )
          {
            if( ( columns.MainColumn == null ) || ( ( m_dataGridControl != null ) && m_dataGridControl.AreDetailsFlatten ) )
            {
              columns.MainColumn = column;
            }
          }
        }

        location = location.GetNextSiblingOrCousin();
        position++;
      }
    }

    private bool ApplyColumnsVisibilityFromView( ColumnHierarchyModel.ILocation location )
    {
      Debug.Assert( location != null );

      var columnLocation = location as ColumnHierarchyModel.IColumnLocation;
      var childrenLocation = this.GetChildLocations( location ).ToList();
      var visibilityStatus = this.GetStatus( location ).Visibility;

      // Apply visibility changes top-down.
      if( childrenLocation.Count > 0 )
      {
        if( visibilityStatus.Mode == PropagationMode.TopDown )
        {
          var childVisibilityStatus = visibilityStatus.SetMode( PropagationMode.TopDown );

          foreach( var childLocation in ( from l in childrenLocation
                                          let cl = l as ColumnHierarchyModel.IColumnLocation
                                          where ( cl != null )
                                             && ( cl.Status.Visibility.Timestamp <= childVisibilityStatus.Timestamp )
                                          select cl ) )
          {
            this.SetVisibility( childLocation, childVisibilityStatus );
          }

          // The current column will need to query its child column visibility to determine its visibility.
          if( columnLocation != null )
          {
            this.SetVisibility( columnLocation, visibilityStatus.SetMode( PropagationMode.BottomUp ) );
          }
        }

        // Apply the child columns visibility.
        foreach( var childLocation in childrenLocation )
        {
          // The layout has been modified while we were updating a child column's visibility.
          // A new visibility pass must be done.
          if( !this.ApplyColumnsVisibilityFromView( childLocation ) )
            return false;
        }

        // Refresh the status in case it changed while applying the child columns visibility.
        visibilityStatus = this.GetStatus( location ).Visibility;
      }

      // Update the current column's visibility based on its children.
      if( columnLocation != null )
      {
        if( childrenLocation.Count > 0 )
        {
          // Make sure the child columns visible state is not dirty because a user did some changes while we were updating
          // the child columns visibility.
          if( visibilityStatus.Mode == PropagationMode.BottomUp )
          {
            // One of the children is not up-to-date.  We need redo the visibility pass.
            // We return true because the layout is still the same.
            if( !childrenLocation.All( l => this.GetStatus( l ).Visibility.Mode == PropagationMode.None ) )
              return true;

            visibilityStatus = ColumnVisibilityStatus.Create( childrenLocation.Any( l => this.GetStatus( l ).Visibility.Visible ) );
            this.SetVisibility( columnLocation, visibilityStatus );
          }
        }
        else
        {
          // A merged column must be hidden when it contains no child column.
          visibilityStatus = ColumnVisibilityStatus.Create( visibilityStatus.Visible && ( location.Level == 0 ) );
          this.SetVisibility( columnLocation, visibilityStatus );
        }
      }

      // A new visibility pass is required when the visibility needs to be progagated.
      if( ( visibilityStatus.Mode != PropagationMode.None ) || ( columnLocation == null ) )
        return true;

      var column = columnLocation.Column;
      Debug.Assert( column != null );

      // Nothing to do.
      if( column.Visible == visibilityStatus.Visible )
        return true;

      // The layout is no longer the same since the columns is now hidden or displayed.
      this.InvalidateLayout();

      var layoutVersion = m_modelLayoutVersion;

      column.Visible = visibilityStatus.Visible;

      // The versions will be different if the user altered the layout while we were updating the column's visibility.
      return ( layoutVersion == m_modelLayoutVersion );
    }

    private void SetVisibleColumnsFromView( int level, ReadOnlyColumnCollection visibleColumns, HashedLinkedList<ColumnBase> columnsByVisiblePosition )
    {
      Debug.Assert( visibleColumns != null );
      Debug.Assert( columnsByVisiblePosition != null );

      using( visibleColumns.DeferNotifications() )
      {
        // Prevent a Reset event for nothing.
        if( visibleColumns.Count != 0 )
        {
          visibleColumns.InternalClear();
        }

        columnsByVisiblePosition.Clear();

        foreach( var column in this.GetColumns( this.GetColumnsLocation( this.GetLocationsOnLevelFromFirstToLast( level ) ) ) )
        {
          columnsByVisiblePosition.AddLast( column );

          using( column.InhibitVisiblePositionChanging() )
          {
            if( column.Visible )
            {
              visibleColumns.InternalAdd( column );
            }
          }
        }
      }

      var firstVisibleColumn = default( ColumnBase );
      var lastVisibleColumn = default( ColumnBase );

      foreach( var column in columnsByVisiblePosition )
      {
        if( column.Visible )
        {
          if( firstVisibleColumn == null )
          {
            firstVisibleColumn = column;
          }
          else
          {
            column.ClearIsFirstVisible();
          }

          if( lastVisibleColumn != null )
          {
            lastVisibleColumn.ClearIsLastVisible();
          }

          lastVisibleColumn = column;
        }
        else
        {
          column.ClearIsFirstVisible();
          column.ClearIsLastVisible();
        }
      }

      if( firstVisibleColumn != null )
      {
        firstVisibleColumn.SetIsFirstVisible( true );
      }

      if( lastVisibleColumn != null )
      {
        lastVisibleColumn.SetIsLastVisible( true );
      }
    }

    private IEnumerable<ColumnHierarchyModel.ILocation> GetLocationsOnLevelFromFirstToLast( int level )
    {
      var location = ( ColumnHierarchyModel.ILocation )m_model.GetLevelMarkers( level ).Start;
      Debug.Assert( location.GetPreviousSiblingOrCousin() == null );

      while( location != null )
      {
        yield return location;

        location = location.GetNextSiblingOrCousin();
      }
    }

    private IEnumerable<ColumnHierarchyModel.ILocation> GetLocationsOnLevelFromLastToFirst( int level )
    {
      var location = ( ColumnHierarchyModel.ILocation )m_model.GetLevelMarkers( level ).End;
      Debug.Assert( location.GetNextSiblingOrCousin() == null );

      while( location != null )
      {
        yield return location;

        location = location.GetPreviousSiblingOrCousin();
      }
    }

    private IEnumerable<ColumnHierarchyModel.ILocation> GetLocationsOnLevel( int level )
    {
      return this.GetLocationsOnLevelFromFirstToLast( level );
    }

    private IEnumerable<ColumnHierarchyModel.ILocation> GetChildLocationsFromFirstToLast( ColumnHierarchyModel.ILocation parentLocation )
    {
      if( parentLocation != null )
      {
        var location = parentLocation.GetFirstChild();

        while( location != null )
        {
          yield return location;

          location = location.GetNextSibling();
        }
      }
    }

    private IEnumerable<ColumnHierarchyModel.ILocation> GetChildLocationsFromLastToFirst( ColumnHierarchyModel.ILocation parentLocation )
    {
      return this.GetChildLocationsFromFirstToLast( parentLocation ).Reverse();
    }

    private IEnumerable<ColumnHierarchyModel.ILocation> GetChildLocations( ColumnHierarchyModel.ILocation parentLocation )
    {
      return this.GetChildLocationsFromFirstToLast( parentLocation );
    }

    private IEnumerable<ColumnHierarchyModel.IColumnLocation> GetColumnsLocation( IEnumerable<ColumnHierarchyModel.ILocation> locations )
    {
      return ( from location in locations
               let columnLocation = location as ColumnHierarchyModel.IColumnLocation
               where ( columnLocation != null )
               select columnLocation );
    }

    private IEnumerable<ColumnBase> GetColumns( IEnumerable<ColumnHierarchyModel.IColumnLocation> locations )
    {
      return ( from location in locations
               select location.Column );
    }

    private IEnumerable<ColumnBase> GetColumns( int level )
    {
      return this.GetColumns( this.GetColumnsLocation( this.GetLocationsOnLevel( level ) ) );
    }

    private ColumnStatus GetStatus( ColumnHierarchyModel.ILocation location )
    {
      var columnLocation = location as ColumnHierarchyModel.IColumnLocation;
      if( columnLocation != null )
        return columnLocation.Status;

      return new ColumnStatus( ColumnVisibilityStatus.Create( false ) );
    }

    private void InvalidateLayout()
    {
      unchecked
      {
        m_modelLayoutVersion++;
      }
    }

    private void InvalidatePosition()
    {
      unchecked
      {
        m_modelPositionVersion++;
      }
    }

    private void InvalidateVisibility()
    {
      unchecked
      {
        m_modelVisibilityVersion++;
      }
    }

    private uint GetTimestamp()
    {
      checked
      {
        m_timestamp++;
      }

      return m_timestamp;
    }

    private void ResetTimestamp()
    {
      if( m_timestamp == 0u )
        return;

      this.ResetStatus();

      // The position and visibility counter are reset here since a call to ResetStatus may update them.  These updates may be safely ignored.
      m_positionVersion = m_modelPositionVersion;
      m_visibilityVersion = m_modelVisibilityVersion;

      // The counter is reset to lower the chance of an overflow.
      m_timestamp = 0u;
    }

    private void UpdateVisibility( ColumnHierarchyModel.IColumnLocation location )
    {
      if( location == null )
        return;

      // An update is already scheduled.
      var visibilityStatus = location.Status.Visibility;
      if( visibilityStatus.Mode != PropagationMode.None )
        return;

      this.SetVisibility( location, visibilityStatus.SetMode( PropagationMode.BottomUp ) );
    }

    private void SetVisibility( ColumnHierarchyModel.IColumnLocation location, ColumnVisibilityStatus visibilityStatus )
    {
      if( location == null )
        return;

      var status = location.Status;

      this.SetStatus( location, status.SetVisibilityStatus( visibilityStatus ) );
    }

    private void SetPosition( ColumnHierarchyModel.IColumnLocation location, ColumnPositionStatus positionStatus )
    {
      if( location == null )
        return;

      var status = location.Status;

      this.SetStatus( location, status.SetPositionStatus( positionStatus ) );
    }

    private void SetStatus( ColumnHierarchyModel.IColumnLocation location, ColumnStatus status )
    {
      if( ( location == null ) || object.Equals( location.Status, status ) )
        return;

      location.Status = status;
    }

    private ILevelMarkers WrapLevelMarkers( ColumnHierarchyModel.IMarkers markers )
    {
      if( markers == null )
        return null;

      return new Markers( this, markers );
    }

    private ILocation WrapLocation( ColumnHierarchyModel.ILocation location )
    {
      if( location == null )
        return null;

      if( location.Type == LocationType.Column )
      {
        Debug.Assert( location is ColumnHierarchyModel.IColumnLocation );
        return new ColumnLocation( this, ( ColumnHierarchyModel.IColumnLocation )location );
      }

      return new Location( this, location );
    }

    private void RegisterEvents( ColumnCollection collection )
    {
      Debug.Assert( collection != null );

      CollectionChangedEventManager.AddListener( collection, this );
      PropertyChangedEventManager.AddListener( collection, this, string.Empty );
    }

    private void UnregisterEvents( ColumnCollection collection )
    {
      Debug.Assert( collection != null );

      CollectionChangedEventManager.RemoveListener( collection, this );
      PropertyChangedEventManager.RemoveListener( collection, this, string.Empty );
    }

    private void RegisterEvents( ColumnBase column )
    {
      Debug.Assert( column != null );
      PropertyChangedEventManager.AddListener( column, this, string.Empty );
    }

    private void UnregisterEvents( ColumnBase column )
    {
      Debug.Assert( column != null );

      PropertyChangedEventManager.RemoveListener( column, this, string.Empty );
    }

    private void OnColumnsChanged( NotifyCollectionChangedEventArgs e )
    {
      Debug.Assert( m_model.LevelCount > 0 );

      var resetFixedRegions = false;
      var resetItemContainerGenerator = false;

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Add:
          {
            var items = e.NewItems;
            if( ( items == null ) || ( items.Count <= 0 ) )
              return;

            Debug.Assert( m_model.LevelCount > 0 );

            // Only reset the fixed headers/footers if columns are at the master level.
            resetFixedRegions = ( m_columns.ParentDetailConfiguration == null );

            foreach( ColumnBase column in items )
            {
              this.AddColumnInView( column, PropagationMode.None, 0 );
            }
          }
          break;

        case NotifyCollectionChangedAction.Remove:
          {
            var items = e.OldItems;
            if( ( items == null ) || ( items.Count <= 0 ) )
              return;

            resetItemContainerGenerator = true;

            foreach( ColumnBase column in items )
            {
              this.RemoveColumnInView( column );
            }
          }
          break;

        case NotifyCollectionChangedAction.Move:
          // Nothing to do.
          return;

        case NotifyCollectionChangedAction.Replace:
          {
            var oldItems = e.OldItems;
            if( ( oldItems == null ) || ( oldItems.Count <= 0 ) )
              return;

            var newItems = e.NewItems;
            if( ( newItems == null ) || ( newItems.Count <= 0 ) )
              return;

            if( newItems.Count != oldItems.Count )
              throw new NotSupportedException();

            resetItemContainerGenerator = true;

            for( int i = 0; i < oldItems.Count; i++ )
            {
              this.ReplaceColumnInView( ( ColumnBase )oldItems[ i ], ( ColumnBase )newItems[ i ] );
            }
          }
          break;

        case NotifyCollectionChangedAction.Reset:
          {
            var result = this.SetColumnsInView( 0 );
            resetFixedRegions = result.Item1;
            resetItemContainerGenerator = result.Item2;
          }
          break;

        default:
          throw new NotSupportedException();
      }

      this.Update();

      if( resetFixedRegions )
      {
        m_dataGridControl.ResetFixedRegions();
      }
      // When a column is removed, we need to clear the generator from every container to avoid problem
      // when a new instance of a column with the same field name is reinserted.
      else if( resetItemContainerGenerator )
      {
        var itemContainerGenerator = m_dataGridControl.CustomItemContainerGenerator;
        if( itemContainerGenerator != null )
        {
          itemContainerGenerator.RemoveAllAndNotify();
        }

        // Only reset the fixed headers/footers if columns are at the master level.
        if( m_columns.ParentDetailConfiguration == null )
        {
          m_dataGridControl.ResetFixedRegions();
        }
      }
    }

    private void OnColumnCollectionPropertyChanged( PropertyChangedEventArgs e )
    {
      var mayHaveChanged = string.IsNullOrEmpty( e.PropertyName );

      if( mayHaveChanged || ( e.PropertyName == ColumnCollection.MainColumnPropertyName ) )
      {
        // We don't want to alter the layout while it is being applied.  Furthermore, it is most likely the
        // new layout that triggered the change notification.
        if( !m_isApplyingChanges.IsSet )
        {
          // The ColumnCollection.MainColumn (ColumnBase.IsMainColumn) property impacts the columns layout when details are flatten.
          if( ( m_dataGridControl != null ) && m_dataGridControl.AreDetailsFlatten )
          {
            var mainColumn = m_columns.MainColumn;
            var mainColumnLocation = ( mainColumn != null ) ? m_model[ mainColumn ] : null;

            if( mainColumnLocation != null )
            {
              Debug.Assert( mainColumnLocation.Level == 0 );

              var columnsToMove = new Stack<ColumnBase>();
              columnsToMove.Push( mainColumnLocation.Column );

              var parentLocation = mainColumnLocation.GetParent();
              while( parentLocation != null )
              {
                var parentColumnLocation = parentLocation as ColumnHierarchyModel.IColumnLocation;
                if( parentColumnLocation == null )
                {
                  // The desired main column is under one of the orphan section.
                  // It is impossible to move the column and its parent to the first location.
                  columnsToMove.Clear();
                  break;
                }

                columnsToMove.Push( parentColumnLocation.Column );
                parentLocation = parentLocation.GetParent();
              }

              while( columnsToMove.Count > 0 )
              {
                var column = columnsToMove.Pop();
                var columnLocation = m_model[ column ];
                Debug.Assert( columnLocation != null );

                var pivotLocation = columnLocation;
                var previousLocation = ( ColumnHierarchyModel.ILocation )pivotLocation;

                while( previousLocation != null )
                {
                  if( previousLocation.Type == LocationType.Column )
                  {
                    pivotLocation = ( ColumnHierarchyModel.IColumnLocation )previousLocation;
                  }

                  previousLocation = previousLocation.GetPreviousSibling();
                }

                if( !object.Equals( columnLocation, pivotLocation ) )
                {
                  Debug.Assert( columnLocation.CanMoveBefore( pivotLocation ) );
                  columnLocation.MoveBefore( pivotLocation );

                  this.SetPosition( columnLocation, columnLocation.Status.Position.SetLocationTimestamp( this.GetTimestamp() ) );
                }
              }
            }
          }
        }

        this.InvalidateLayout();
        this.Update();
      }
    }

    private void OnColumnPropertyChanged( ColumnBase column, PropertyChangedEventArgs e )
    {
      var mayHaveChanged = string.IsNullOrEmpty( e.PropertyName );
      var columnLocation = m_model[ column ];
      var needUpdate = false;

      Debug.Assert( columnLocation != null );

      if( mayHaveChanged || ( e.PropertyName == ColumnBase.VisibleProperty.Name ) )
      {
        if( column.Visible != columnLocation.Status.Visibility.Visible )
        {
          this.SetVisibility( columnLocation, ColumnVisibilityStatus.Create( column.Visible, PropagationMode.TopDown, this.GetTimestamp() ) );
        }

        needUpdate = true;
      }

      if( mayHaveChanged || ( e.PropertyName == ColumnBase.VisiblePositionProperty.Name ) )
      {
        this.SetPosition( columnLocation, columnLocation.Status.Position.SetVisiblePositionTimestamp( this.GetTimestamp() ) );

        // We must invalidate the layout no matter what to make sure the column's visible position is recalculated and reapplied.
        this.InvalidateLayout();

        needUpdate = true;
      }

      if( mayHaveChanged || ( e.PropertyName == ColumnBase.DraggableStatusProperty.Name ) )
      {
        this.SetPosition( columnLocation, columnLocation.Status.Position.SetDraggableStatusTimestamp( this.GetTimestamp() ) );

        needUpdate = true;
      }

      if( mayHaveChanged || ( e.PropertyName == ColumnBase.IsMainColumnPropertyName ) )
      {
        this.SetPosition( columnLocation, columnLocation.Status.Position.SetMainColumnTimestamp( this.GetTimestamp() ) );

        needUpdate = true;
      }

      if( mayHaveChanged)
      {
        ColumnHierarchyModel parentColumnLocation =  null;

        if( !object.Equals( columnLocation.GetParent(), parentColumnLocation ) )
        {
          this.SetPosition( columnLocation, columnLocation.Status.Position.SetParentTimestamp( this.GetTimestamp() ) );
        }

        needUpdate = true;
      }

      if( needUpdate )
      {
        this.Update();
      }
    }

    private void OnViewLayoutChanging( object sender, ColumnHierarchyModel.LayoutChangingEventArgs e )
    {
      switch( e.Action )
      {
        case ColumnHierarchyModel.LayoutChangingAction.ChangingParent:
        case ColumnHierarchyModel.LayoutChangingAction.ChangingFirstChild:
        case ColumnHierarchyModel.LayoutChangingAction.Removing:
          {
            var location = e.Location.GetParent() as ColumnHierarchyModel.IColumnLocation;
            if( location != null )
            {
              this.UpdateVisibility( location );
            }
          }
          break;
      }

      this.InvalidateLayout();
    }

    private void OnViewLayoutChanged( object sender, ColumnHierarchyModel.LayoutChangedEventArgs e )
    {
      switch( e.Action )
      {
        case ColumnHierarchyModel.LayoutChangedAction.ParentChanged:
        case ColumnHierarchyModel.LayoutChangedAction.FirstChildChanged:
          {
            var location = e.Location.GetParent() as ColumnHierarchyModel.IColumnLocation;
            if( location != null )
            {
              this.UpdateVisibility( location );
            }
          }
          break;
      }

      this.InvalidateLayout();
    }

    private void OnViewStatusChanged( object sender, ColumnHierarchyModel.StatusChangedEventArgs e )
    {
      var oldStatus = e.OldValue;
      var oldVisibilityStatus = oldStatus.Visibility;
      var oldPositionStatus = oldStatus.Position;
      var newStatus = e.NewValue;
      var newVisibilityStatus = newStatus.Visibility;
      var newPositionStatus = newStatus.Position;

      if( oldVisibilityStatus.Visible != newVisibilityStatus.Visible )
      {
        var parentColumnLocation = e.Location.GetParent() as ColumnHierarchyModel.IColumnLocation;
        if( parentColumnLocation != null )
        {
          this.UpdateVisibility( parentColumnLocation );
        }

        // The layout is no longer the same since a columns is now hidden or displayed.
        this.InvalidateLayout();
      }

      if( !object.Equals( oldPositionStatus, newPositionStatus ) )
      {
        this.InvalidatePosition();
      }

      this.InvalidateVisibility();
    }

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    private bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        Debug.Assert( m_dataGridControl != null, "The current object should be initialized." );

        var eventArgs = ( NotifyCollectionChangedEventArgs )e;

        if( sender == m_columns )
        {
          this.OnColumnsChanged( eventArgs );
        }
      }
      else if( managerType == typeof( PropertyChangedEventManager ) )
      {
        Debug.Assert( m_dataGridControl != null, "The current object should be initialized." );

        var eventArgs = ( PropertyChangedEventArgs )e;

        if( sender is ColumnBase )
        {
          this.OnColumnPropertyChanged( ( ColumnBase )sender, eventArgs );
        }
        else if( sender == m_columns )
        {
          this.OnColumnCollectionPropertyChanged( eventArgs );
        }
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    private readonly ColumnHierarchyModel m_model = new ColumnHierarchyModel();
    private readonly AutoResetFlag m_isApplyingChanges = AutoResetFlagFactory.Create( true );
    private DataGridControl m_dataGridControl;
    private int m_deferUpdateCount; //0
    private int? m_desiredFixedColumnCount; //null
    private bool m_synchronizeChildColumnNamesWithChildColumns; //false
    private uint m_timestamp; //0
    private int m_modelLayoutVersion; //0
    private int m_modelPositionVersion; //0
    private int m_modelVisibilityVersion; //0
    private int m_layoutVersion; //0
    private int m_positionVersion; //0
    private int m_visibilityVersion; //0

    #region ILevelMarkers Internal Interface

    internal interface ILevelMarkers
    {
      ILocation Start
      {
        get;
      }

      ILocation End
      {
        get;
      }

      ILocation Splitter
      {
        get;
      }

      ILocation Orphan
      {
        get;
      }
    }

    #endregion

    #region ILocation Internal Interface

    internal interface ILocation
    {
      LocationType Type
      {
        get;
      }

      ILocation GetParent();
      ILocation GetFirstChild();
      ILocation GetLastChild();

      ILocation GetPreviousSibling();
      ILocation GetNextSibling();

      ILocation GetPreviousSiblingOrCousin();
      ILocation GetNextSiblingOrCousin();

      bool CanMoveBefore( ILocation location );
      bool CanMoveAfter( ILocation location );
      bool CanMoveUnder( ILocation location );

      void MoveBefore( ILocation location );
      void MoveAfter( ILocation location );
      void MoveUnder( ILocation location );
    }

    #endregion

    #region IColumnLocation Internal Interface

    internal interface IColumnLocation : ILocation
    {
      ColumnBase Column
      {
        get;
      }
    }

    #endregion

    #region UpdateOptions Internal Struct

    internal struct UpdateOptions
    {
      internal UpdateOptions( int desiredFixedColumnCount )
      {
        m_desiredFixedColumnCount = desiredFixedColumnCount;
        m_synchronizeChildColumnNamesWithChildColumns = default( bool? );
      }

      internal UpdateOptions( int desiredFixedColumnCount, bool synchronizeChildColumnNamesWithChildColumns )
      {
        m_desiredFixedColumnCount = desiredFixedColumnCount;
        m_synchronizeChildColumnNamesWithChildColumns = synchronizeChildColumnNamesWithChildColumns;
      }

      internal int? DesiredFixedColumnCount
      {
        get
        {
          return m_desiredFixedColumnCount;
        }
        set
        {
          m_desiredFixedColumnCount = value;
        }
      }

      internal bool? SynchronizeChildColumnNamesWithChildColumns
      {
        get
        {
          return m_synchronizeChildColumnNamesWithChildColumns;
        }
        set
        {
          m_synchronizeChildColumnNamesWithChildColumns = value;
        }
      }

      private int? m_desiredFixedColumnCount;
      private bool? m_synchronizeChildColumnNamesWithChildColumns;
    }

    #endregion

    #region Markers Private Class

    private sealed class Markers : ILevelMarkers
    {
      internal Markers( ColumnHierarchyManager owner, ColumnHierarchyModel.IMarkers markers )
      {
        Debug.Assert( owner != null );
        Debug.Assert( markers != null );

        m_owner = owner;
        m_markers = markers;
      }

      public ILocation Start
      {
        get
        {
          return m_owner.WrapLocation( m_markers.Start );
        }
      }

      public ILocation End
      {
        get
        {
          return m_owner.WrapLocation( m_markers.End );
        }
      }

      public ILocation Splitter
      {
        get
        {
          return m_owner.WrapLocation( m_markers.Splitter );
        }
      }

      public ILocation Orphan
      {
        get
        {
          return m_owner.WrapLocation( m_markers.Orphan );
        }
      }

      public override int GetHashCode()
      {
        return m_markers.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as Markers;
        if( target == null )
          return false;

        return ( m_owner == target.m_owner )
            && ( object.Equals( m_markers, target.m_markers ) );
      }

      private readonly ColumnHierarchyManager m_owner;
      private readonly ColumnHierarchyModel.IMarkers m_markers;
    }

    #endregion

    #region Location Private Class

    private class Location : ILocation
    {
      internal Location( ColumnHierarchyManager owner, ColumnHierarchyModel.ILocation location )
      {
        Debug.Assert( owner != null );
        Debug.Assert( location != null );

        m_owner = owner;
        m_location = location;
      }

      protected ColumnHierarchyManager Owner
      {
        get
        {
          return m_owner;
        }
      }

      protected ColumnHierarchyModel.ILocation LocationCache
      {
        get
        {
          return m_location;
        }
      }

      LocationType ILocation.Type
      {
        get
        {
          return m_location.Type;
        }
      }

      public override int GetHashCode()
      {
        return m_location.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as Location;
        if( target == null )
          return false;

        return ( m_owner == target.m_owner )
            && ( object.Equals( m_location, target.m_location ) );
      }

      protected virtual void OnMovedBefore( Location location )
      {
      }

      protected virtual void OnMovedAfter( Location location )
      {
      }

      protected virtual void OnMovedUnder( Location location )
      {
      }

      ILocation ILocation.GetParent()
      {
        return m_owner.WrapLocation( m_location.GetParent() );
      }

      ILocation ILocation.GetFirstChild()
      {
        return m_owner.WrapLocation( m_location.GetFirstChild() );
      }

      ILocation ILocation.GetLastChild()
      {
        var location = m_location.GetFirstChild();
        if( location == null )
          return m_owner.WrapLocation( null );

        var next = location.GetNextSibling();
        while( next != null )
        {
          location = next;
          next = next.GetNextSibling();
        }

        return m_owner.WrapLocation( location );
      }

      ILocation ILocation.GetPreviousSibling()
      {
        return m_owner.WrapLocation( m_location.GetPreviousSibling() );
      }

      ILocation ILocation.GetNextSibling()
      {
        return m_owner.WrapLocation( m_location.GetNextSibling() );
      }

      ILocation ILocation.GetPreviousSiblingOrCousin()
      {
        return m_owner.WrapLocation( m_location.GetPreviousSiblingOrCousin() );
      }

      ILocation ILocation.GetNextSiblingOrCousin()
      {
        return m_owner.WrapLocation( m_location.GetNextSiblingOrCousin() );
      }

      bool ILocation.CanMoveBefore( ILocation location )
      {
        var target = location as Location;
        if( target == null )
          return false;

        return m_location.CanMoveBefore( target.m_location );
      }

      bool ILocation.CanMoveAfter( ILocation location )
      {
        var target = location as Location;
        if( target == null )
          return false;

        return m_location.CanMoveAfter( target.m_location );
      }

      bool ILocation.CanMoveUnder( ILocation location )
      {
        var target = location as Location;
        if( target == null )
          return false;

        return m_location.CanMoveUnder( target.m_location );
      }

      void ILocation.MoveBefore( ILocation location )
      {
        Debug.Assert( ( ( ILocation )this ).CanMoveBefore( location ) );

        var pivot = ( Location )location;
        Debug.Assert( pivot != null );

        m_location.MoveBefore( pivot.m_location );
        this.OnMovedBefore( pivot );

        m_owner.Update();
      }

      void ILocation.MoveAfter( ILocation location )
      {
        Debug.Assert( ( ( ILocation )this ).CanMoveAfter( location ) );

        var pivot = ( Location )location;
        Debug.Assert( pivot != null );

        m_location.MoveAfter( pivot.m_location );
        this.OnMovedAfter( pivot );

        m_owner.Update();
      }

      void ILocation.MoveUnder( ILocation location )
      {
        Debug.Assert( ( ( ILocation )this ).CanMoveUnder( location ) );

        var pivot = ( Location )location;
        Debug.Assert( pivot != null );

        m_location.MoveUnder( pivot.m_location );
        this.OnMovedUnder( pivot );

        m_owner.Update();
      }

      private readonly ColumnHierarchyManager m_owner;
      private readonly ColumnHierarchyModel.ILocation m_location;
    }

    #endregion

    #region ColumnLocation Private Class

    private sealed class ColumnLocation : Location, IColumnLocation
    {
      internal ColumnLocation( ColumnHierarchyManager owner, ColumnHierarchyModel.IColumnLocation location )
        : base( owner, location )
      {
      }

      private ColumnHierarchyModel.IColumnLocation TargetLocation
      {
        get
        {
          return ( ColumnHierarchyModel.IColumnLocation )this.LocationCache;
        }
      }

      ColumnBase IColumnLocation.Column
      {
        get
        {
          return this.TargetLocation.Column;
        }
      }

      protected override void OnMovedBefore( Location location )
      {
        base.OnMovedBefore( location );

        this.InvalidatePosition();
      }

      protected override void OnMovedAfter( Location location )
      {
        base.OnMovedAfter( location );

        this.InvalidatePosition();
      }

      protected override void OnMovedUnder( Location location )
      {
        base.OnMovedUnder( location );

        this.InvalidatePosition();
      }

      private void InvalidatePosition()
      {
        var owner = this.Owner;
        var columnLocation = this.TargetLocation;
        var positionStatus = columnLocation.Status.Position.SetLocationTimestamp( owner.GetTimestamp() );

        owner.SetPosition( columnLocation, positionStatus );
      }
    }

    #endregion

    #region ColumnHierarchyModel Private Class

    private sealed class ColumnHierarchyModel : ColumnHierarchyModel<ColumnBase, ColumnStatus>
    {
    }

    #endregion

    #region ColumnLocationOrderer Private Class

    private static class ColumnLocationOrderer
    {
      internal static void Reorder( IList<ColumnHierarchyModel.IColumnLocation> locations, bool useDraggableStatus )
      {
        if( ColumnLocationOrderer.IsUnchanged( locations ) )
          return;

        var lockedFirst = new List<int>();
        var lockedLast = new List<int>();
        var unlocked = new List<int>();

        ColumnLocationOrderer.Split( locations, useDraggableStatus, lockedFirst, lockedLast, unlocked );

        var lockedFirstStartIndex = 0;
        var unlockedStartIndex = lockedFirstStartIndex + lockedFirst.Count;
        var lockedLastStartIndex = unlockedStartIndex + unlocked.Count;

        ColumnLocationOrderer.Reorder( locations, lockedFirst, lockedFirstStartIndex );
        ColumnLocationOrderer.Reorder( locations, unlocked, unlockedStartIndex );
        ColumnLocationOrderer.Reorder( locations, lockedLast, lockedLastStartIndex );

        var newOrder = new int[ locations.Count ];

        lockedFirst.CopyTo( newOrder, lockedFirstStartIndex );
        unlocked.CopyTo( newOrder, unlockedStartIndex );
        lockedLast.CopyTo( newOrder, lockedLastStartIndex );

        // Move the locations to get them in the desired order.
        ColumnLocationOrderer.Apply( locations, newOrder );
      }

      private static void Reorder( IList<ColumnHierarchyModel.IColumnLocation> locations, IList<int> indexes, int startingPosition )
      {
        Debug.Assert( locations != null );
        Debug.Assert( indexes != null );

        var count = indexes.Count;
        if( count <= 1 )
          return;

        var positionsStatus = new ColumnPositionStatus[ count ];
        var newOrder = new int[ count ];
        var movableCount = 0;
        var reorderCount = 0;

        // Do a pass to check if a reordering is required.
        for( int i = 0; i < positionsStatus.Length; i++ )
        {
          var index = indexes[ i ];
          var positionStatus = locations[ index ].Status.Position;

          positionsStatus[ i ] = positionStatus;

          if( positionStatus.LocationTimestamp > positionStatus.VisiblePositionTimestamp )
          {
            // This location is considered fixed and will not be sorted.
            newOrder[ i ] = index;
          }
          else
          {
            // This location is considered movable and may be sorted.
            newOrder[ i ] = -1;
            movableCount++;

            // This location visible position has changed, so we will need to apply
            // an algorithm to figure out where to put it.
            if( positionStatus.VisiblePositionTimestamp != 0u )
            {
              reorderCount++;
            }
          }
        }

        // Nothing to do if all columns stay where they are.
        if( ( reorderCount <= 0 ) || ( movableCount <= 1 ) )
          return;

        var visiblePositions = new List<int>( count );
        var slots = new List<Slot>( movableCount );
        var movables = new List<int>( movableCount );
        var reorder = new List<Tuple<int, uint>>( reorderCount );

        // Separate the movable locations that were updated from the ones that were not.
        for( int i = 0; i < newOrder.Length; i++ )
        {
          visiblePositions.Add( locations[ indexes[ i ] ].Column.VisiblePosition );

          // This location is fixed.
          if( newOrder[ i ] >= 0 )
            continue;

          slots.Add( new Slot( i, startingPosition + i ) );

          var timestamp = positionsStatus[ i ].VisiblePositionTimestamp;
          if( timestamp == 0u )
          {
            movables.Add( i );
          }
          else
          {
            reorder.Add( new Tuple<int, uint>( i, timestamp ) );
          }
        }

        var comparer = new MovableComparer( slots, visiblePositions );

        // Reinsert the updated movable locations in a time fashion order into the not updated movable locations.
        foreach( var i in ( from item in reorder
                            orderby item.Item2, item.Item1
                            select item.Item1 ) )
        {
          // This property is set to help the comparer identify the element the binary search is looking for.
          // The algorithm is complex and need more information than the ones provided by the IComparer<>.Compare method.
          comparer.LookFor = i;

          var index = movables.BinarySearch( i, comparer );
          if( index < 0 )
          {
            index = ~index;
          }

          if( index >= movables.Count )
          {
            movables.Add( i );
          }
          else
          {
            movables.Insert( index, i );
          }
        }

        Debug.Assert( movables.Count == movableCount );

        // Reinsert the movable locations into the fixed locations.
        for( int i = 0; i < movables.Count; i++ )
        {
          newOrder[ slots[ i ].Index ] = indexes[ movables[ i ] ];
        }

        // Set the results.
        for( int i = 0; i < newOrder.Length; i++ )
        {
          indexes[ i ] = newOrder[ i ];
        }
      }

      private static void Apply( IList<ColumnHierarchyModel.IColumnLocation> locations, IList<int> indexes )
      {
        Debug.Assert( locations != null );
        Debug.Assert( indexes != null );
        Debug.Assert( locations.Count == indexes.Count );

        for( int i = 0; i < indexes.Count - 1; i++ )
        {
          var pivot = locations[ indexes[ i ] ];
          var target = locations[ indexes[ i + 1 ] ];
          var next = pivot.GetNextSibling();

          while( !object.Equals( next, target ) )
          {
            if( ( next == null ) || ( next.Type == LocationType.Column ) )
            {
              Debug.Assert( target.CanMoveAfter( pivot ) );
              target.MoveAfter( pivot );
              break;
            }
            // A splitter may be located between the columns.  Make sure not to move the column if it
            // is the case or the splitter will go back next to the starting location.
            else
            {
              next = next.GetNextSibling();
            }
          }
        }
      }

      private static bool IsUnchanged( IList<ColumnHierarchyModel.IColumnLocation> locations )
      {
        if( ( locations == null ) || ( locations.Count <= 1 ) )
          return true;

        // Do a pass to check if a reordering is required.
        foreach( var location in locations )
        {
          var positionStatus = location.Status.Position;

          // This location's lock type may have changed, so does its position.
          if( ( positionStatus.DraggableStatusTimestamp != 0u ) || ( positionStatus.MainColumnTimestamp != 0u ) )
            return false;

          if( location.Column.DraggableStatus != ColumnDraggableStatus.Draggable )
          {
            // This location may have change position, so we will need to apply an algorithm to figure out where to put it.
            if( ( positionStatus.LocationTimestamp != 0u ) || ( positionStatus.VisiblePositionTimestamp != 0u ) )
              return false;
          }
          else
          {
            // This location is considered fixed and will not be sorted.
            if( positionStatus.LocationTimestamp > positionStatus.VisiblePositionTimestamp )
              continue;

            // This location visible position has changed, so we will need to apply
            // an algorithm to figure out where to put it.
            if( positionStatus.VisiblePositionTimestamp != 0u )
              return false;
          }
        }

        return true;
      }

      private static void Split( IList<ColumnHierarchyModel.IColumnLocation> locations, bool useDraggableStatus, IList<int> lockedFirst, IList<int> lockedLast, IList<int> unlocked )
      {
        Debug.Assert( locations != null );
        Debug.Assert( lockedFirst != null );
        Debug.Assert( lockedLast != null );
        Debug.Assert( unlocked != null );

        for( int i = 0; i < locations.Count; i++ )
        {
          var column = locations[ i ].Column;
          var draggableStatus = column.DraggableStatus;

          if( useDraggableStatus )
          {
            switch( draggableStatus )
            {
              case ColumnDraggableStatus.FirstUndraggable:
                {
                  lockedFirst.Add( i );
                }
                break;

              case ColumnDraggableStatus.LastUndraggable:
                {
                  lockedLast.Add( i );
                }
                break;

              case ColumnDraggableStatus.Draggable:
                {
                  unlocked.Add( i );
                }
                break;

              default:
                throw new NotImplementedException();
            }
          }
          else
          {
            unlocked.Add( i );
          }
        }
      }

      private sealed class MovableComparer : IComparer<int>
      {
        private static readonly IComparer<Slot> SlotComparer = new VisiblePositionComparer();

        internal MovableComparer( List<Slot> slots, IList<int> positions )
        {
          m_slots = slots;
          m_positions = positions;
        }

        internal int LookFor
        {
          set
          {
            m_lookFor = value;
          }
        }

        public int Compare( int x, int y )
        {
          if( x == m_lookFor )
          {
            var xPosition = m_positions[ x ];
            var yPosition = m_positions[ y ];

            // Since we are using this comparer to reinsert updated locations in time order,
            // a visible position that is lesser than any other location should always be inserted before.
            if( ( xPosition < yPosition ) )
              return -1;

            // For a visible position that is greater or equal, we must look at the available movables
            // slots to insert the column in the slot that is the nearest to the desired target position.
            var xSlot = this.FindSlot( xPosition );
            var ySlot = this.FindSlot( yPosition );

            var compare = xSlot.CompareTo( ySlot );
            if( compare != 0 )
              return compare;

            var position = this.FindPosition( xSlot );

            // The location is moving toward the end.
            if( x < position )
            {
              if( y <= position )
                return 1;

              return -1;
            }
            // The location is moving toward the start.
            else if( x > position )
            {
              if( y >= position )
                return -1;

              return 1;
            }
          }
          else if( y == m_lookFor )
          {
            var xPosition = m_positions[ x ];
            var yPosition = m_positions[ y ];

            // Since we are using this comparer to reinsert updated locations in time order,
            // a visible position that is lesser than any other location should always be inserted before.
            if( ( yPosition < xPosition ) )
              return 1;

            // For a visible position that is greater or equal, we must look at the available movables
            // slots to insert the column in the slot that is the nearest to the desired target position.
            var xSlot = this.FindSlot( xPosition );
            var ySlot = this.FindSlot( yPosition );

            var compare = xSlot.CompareTo( ySlot );
            if( compare != 0 )
              return compare;

            var position = this.FindPosition( ySlot );

            // The location is moving toward the end.
            if( y < position )
            {
              if( x <= position )
                return -1;

              return 1;
            }
            // The location is moving toward the start.
            else if( y > position )
            {
              if( x >= position )
                return 1;

              return -1;
            }
          }

          return x.CompareTo( y );
        }

        private int FindSlot( int position )
        {
          var index = m_slots.BinarySearch( new Slot( 0, position ), MovableComparer.SlotComparer );
          if( index < 0 )
            return ~index;

          return index;
        }

        private int FindPosition( int slot )
        {
          Debug.Assert( ( slot >= 0 ) && ( slot <= m_slots.Count ) );

          if( slot >= m_slots.Count )
            return m_slots.Last().Index + 1;

          return m_slots[ slot ].Index;
        }

        private readonly List<Slot> m_slots;
        private readonly IList<int> m_positions;
        private int m_lookFor;

        private sealed class VisiblePositionComparer : IComparer<Slot>
        {
          public int Compare( Slot x, Slot y )
          {
            return x.VisiblePosition.CompareTo( y.VisiblePosition );
          }
        }
      }

      private struct Slot
      {
        internal Slot( int index, int visiblePosition )
        {
          Debug.Assert( index >= 0 );
          Debug.Assert( visiblePosition >= 0 );

          this.Index = index;
          this.VisiblePosition = visiblePosition;
        }

        internal readonly int Index;
        internal readonly int VisiblePosition;
      }
    }

    #endregion

    #region DeferUpdateState Private Class

    private sealed class DeferUpdateState : DeferredDisposableState
    {
      internal DeferUpdateState( ColumnHierarchyManager target, UpdateOptions options )
      {
        if( target == null )
          throw new ArgumentNullException( "target" );

        m_target = target;
        m_options = options;
      }

      protected override object SyncRoot
      {
        get
        {
          return m_target.SyncRoot;
        }
      }

      protected override bool IsDeferred
      {
        get
        {
          return ( m_target.m_deferUpdateCount != 0 );
        }
      }

      protected override void Increment()
      {
        m_target.m_deferUpdateCount++;
      }

      protected override void Decrement()
      {
        m_target.m_deferUpdateCount--;
      }

      protected override void OnDeferEnded( bool disposing )
      {
        if( !disposing )
          return;

        m_target.Update( m_options );
      }

      private readonly ColumnHierarchyManager m_target;
      private readonly UpdateOptions m_options;
    }

    #endregion

    #region ColumnStatus Private Struct

    private struct ColumnStatus
    {
      internal ColumnStatus( ColumnVisibilityStatus visibility )
        : this( default( ColumnPositionStatus ), visibility )
      {
      }

      internal ColumnStatus( ColumnPositionStatus position, ColumnVisibilityStatus visibility )
      {
        this.Position = position;
        this.Visibility = visibility;
      }

      internal ColumnStatus SetPositionStatus( ColumnPositionStatus status )
      {
        return new ColumnStatus( status, this.Visibility );
      }

      internal ColumnStatus SetVisibilityStatus( ColumnVisibilityStatus status )
      {
        return new ColumnStatus( this.Position, status );
      }

      internal readonly ColumnPositionStatus Position;
      internal readonly ColumnVisibilityStatus Visibility;
    }

    #endregion

    #region ColumnVisibilityStatus Private Struct

    [DebuggerDisplay( "Visible = {Visible}, Mode = {Mode}" )]
    private struct ColumnVisibilityStatus
    {
      private ColumnVisibilityStatus( bool visible, PropagationMode mode, uint timestamp )
      {
        m_visible = visible;
        m_mode = mode;
        m_timestamp = timestamp;
      }

      internal ColumnVisibilityStatus SetMode( PropagationMode mode )
      {
        return ColumnVisibilityStatus.Create( m_visible, mode, m_timestamp );
      }

      internal bool Visible
      {
        get
        {
          return m_visible;
        }
      }

      internal PropagationMode Mode
      {
        get
        {
          return m_mode;
        }
      }

      internal uint Timestamp
      {
        get
        {
          return m_timestamp;
        }
      }

      internal static ColumnVisibilityStatus Create( bool visible )
      {
        return ColumnVisibilityStatus.Create( visible, PropagationMode.None, 0u );
      }

      internal static ColumnVisibilityStatus Create( bool visible, PropagationMode mode, uint timestamp )
      {
        return new ColumnVisibilityStatus( visible, mode, timestamp );
      }

      private readonly bool m_visible;
      private readonly PropagationMode m_mode;
      private readonly uint m_timestamp;
    }

    #endregion

    #region ColumnPositionStatus Private Struct

    private struct ColumnPositionStatus
    {
      internal static readonly ColumnPositionStatus Default = new ColumnPositionStatus();

      private ColumnPositionStatus( uint parent, uint children, uint visiblePosition, uint location, uint draggableStatus, uint mainColumn )
      {
        m_parentTimestamp = parent;
        m_childrenTimestamp = children;
        m_visiblePositionTimestamp = visiblePosition;
        m_locationTimestamp = location;
        m_draggableStatusTimestamp = draggableStatus;
        m_mainColumnTimestamp = mainColumn;
      }

      internal uint ParentTimestamp
      {
        get
        {
          return m_parentTimestamp;
        }
      }

      internal uint ChildrenTimestamp
      {
        get
        {
          return m_childrenTimestamp;
        }
      }

      internal uint VisiblePositionTimestamp
      {
        get
        {
          return m_visiblePositionTimestamp;
        }
      }

      internal uint LocationTimestamp
      {
        get
        {
          return m_locationTimestamp;
        }
      }

      internal uint DraggableStatusTimestamp
      {
        get
        {
          return m_draggableStatusTimestamp;
        }
      }

      internal uint MainColumnTimestamp
      {
        get
        {
          return m_mainColumnTimestamp;
        }
      }

      internal ColumnPositionStatus SetParentTimestamp( uint timestamp )
      {
        return new ColumnPositionStatus( timestamp, m_childrenTimestamp, m_visiblePositionTimestamp, m_locationTimestamp, m_draggableStatusTimestamp, m_mainColumnTimestamp );
      }

      internal ColumnPositionStatus SetChildrenTimestamp( uint timestamp )
      {
        return new ColumnPositionStatus( m_parentTimestamp, timestamp, m_visiblePositionTimestamp, m_locationTimestamp, m_draggableStatusTimestamp, m_mainColumnTimestamp );
      }

      internal ColumnPositionStatus SetVisiblePositionTimestamp( uint timestamp )
      {
        return new ColumnPositionStatus( m_parentTimestamp, m_childrenTimestamp, timestamp, m_locationTimestamp, m_draggableStatusTimestamp, m_mainColumnTimestamp );
      }

      internal ColumnPositionStatus SetLocationTimestamp( uint timestamp )
      {
        return new ColumnPositionStatus( m_parentTimestamp, m_childrenTimestamp, m_visiblePositionTimestamp, timestamp, m_draggableStatusTimestamp, m_mainColumnTimestamp );
      }

      internal ColumnPositionStatus SetDraggableStatusTimestamp( uint timestamp )
      {
        return new ColumnPositionStatus( m_parentTimestamp, m_childrenTimestamp, m_visiblePositionTimestamp, m_locationTimestamp, timestamp, m_mainColumnTimestamp );
      }

      internal ColumnPositionStatus SetMainColumnTimestamp( uint timestamp )
      {
        return new ColumnPositionStatus( m_parentTimestamp, m_childrenTimestamp, m_visiblePositionTimestamp, m_locationTimestamp, m_draggableStatusTimestamp, timestamp );
      }

      private readonly uint m_parentTimestamp;
      private readonly uint m_childrenTimestamp;
      private readonly uint m_visiblePositionTimestamp;
      private readonly uint m_locationTimestamp;
      private readonly uint m_draggableStatusTimestamp;
      private readonly uint m_mainColumnTimestamp;
    }

    #endregion

    #region ParentChildRelationshipSnapshot Private Struct

    private struct ParentChildRelationshipSnapshot
    {
      internal ParentChildRelationshipSnapshot( ColumnHierarchyModel.IColumnLocation location, uint timestamp )
      {
        m_location = location;
        m_timestamp = timestamp;
      }

      internal ColumnHierarchyModel.IColumnLocation Location
      {
        get
        {
          return m_location;
        }
      }

      internal uint Timestamp
      {
        get
        {
          return m_timestamp;
        }
      }

      private readonly ColumnHierarchyModel.IColumnLocation m_location;
      private readonly uint m_timestamp;
    }

    #endregion

    #region ParentChildRelationshipSnapshotComparer Private Class

    private sealed class ParentChildRelationshipSnapshotComparer : IComparer<ParentChildRelationshipSnapshot>
    {
      public int Compare( ParentChildRelationshipSnapshot x, ParentChildRelationshipSnapshot y )
      {
        var compare = x.Timestamp.CompareTo( y.Timestamp );
        if( compare != 0 )
          return -compare;

        return -string.CompareOrdinal( x.Location.Column.FieldName, y.Location.Column.FieldName );
      }
    }

    #endregion

    #region PropagationMode Private Enum

    private enum PropagationMode : byte
    {
      None = 0,
      BottomUp,
      TopDown,
    }

    #endregion
  }
}
