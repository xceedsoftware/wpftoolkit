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
using System.Diagnostics;
using System.Linq;
using Xceed.Utils.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class ColumnHierarchyModel<TColumn, TStatus> where TColumn : class
  {
    #region Static Fields

    private const int None = -1;

    #endregion

    #region LevelCount Property

    internal int LevelCount
    {
      get
      {
        return m_level;
      }
    }

    #endregion

    #region [] Property

    internal IColumnLocation this[ TColumn column ]
    {
      get
      {
        if( object.ReferenceEquals( column, null ) )
          throw new ArgumentNullException( "column" );

        if( m_capacity <= 0 )
          return null;

        var hashCode = column.GetHashCode();
        var index = this.FindColumn( column, hashCode );
        if( index == ColumnHierarchyModel<TColumn, TStatus>.None )
          return null;

        var entry = m_columns[ index ];
        return new ColumnLocation( this, entry.Column, entry.HashCode, index );
      }
    }

    #endregion

    #region LayoutChanging Event

    internal event EventHandler<LayoutChangingEventArgs> LayoutChanging;

    private void OnLayoutChanging( LayoutChangingEventArgs e )
    {
      var handler = this.LayoutChanging;
      if( handler == null )
        return;

      try
      {
        m_preventLayoutChanged = true;
        handler.Invoke( this, e );
      }
      finally
      {
        m_preventLayoutChanged = false;
      }
    }

    #endregion

    #region LayoutChanged Event

    internal event EventHandler<LayoutChangedEventArgs> LayoutChanged;

    private void OnLayoutChanged( LayoutChangedEventArgs e )
    {
      var handler = this.LayoutChanged;
      if( handler == null )
        return;

      try
      {
        m_preventLayoutChanged = true;
        handler.Invoke( this, e );
      }
      finally
      {
        m_preventLayoutChanged = false;
      }
    }

    #endregion

    #region StatusChanged Event

    internal event EventHandler<StatusChangedEventArgs> StatusChanged;

    private void OnStatusChanged( StatusChangedEventArgs e )
    {
      var handler = this.StatusChanged;
      if( handler == null )
        return;

      try
      {
        m_preventLayoutChanged = true;
        handler.Invoke( this, e );
      }
      finally
      {
        m_preventLayoutChanged = false;
      }
    }

    #endregion

    internal IMarkers AddLevel( int level )
    {
      if( level < 0 )
        throw new ArgumentException( "The level must be greater than or equal to zero.", "level" );

      if( level > m_level )
        throw new ArgumentException( "Only one level can be inserted at a time.", "level" );

      if( m_preventLayoutChanged )
        throw new InvalidOperationException( "The layout cannot be updated." );

      // Resize the array to make space for a new level.
      if( ( m_marks == null ) || ( m_marks.Length < m_level + 1 ) )
      {
        var currentSize = ( m_marks != null ) ? m_marks.Length : 0;
        var desizedSize = ( int )Math.Min( Math.Max( m_level + 1, currentSize * 2L ), int.MaxValue );

        Array.Resize( ref m_marks, desizedSize );
        Debug.Assert( m_marks != null );

        for( int i = currentSize; i < m_marks.Length; i++ )
        {
          m_marks[ i ] = MarkEntry.Default;
        }

        // This should only happen when the array is of size int.MaxValue.  Technically, this should never happen.
        if( !object.Equals( m_marks.Last(), MarkEntry.Default ) )
          throw new InvalidOperationException( "No more levels could be created." );
      }

      return this.AddLevelCore( level );
    }

    internal void RemoveLevel( int level )
    {
      if( level < 0 )
        throw new ArgumentException( "The level must be greater than or equal to zero.", "level" );

      if( level >= m_level )
        throw new ArgumentException( "The level does not exist.", "level" );

      if( m_preventLayoutChanged )
        throw new InvalidOperationException( "The layout cannot be updated." );

      this.RemoveLevelCore( level );
    }

    internal IColumnLocation Add( TColumn column, int level )
    {
      return this.Add( column, default( TStatus ), level );
    }

    internal IColumnLocation Add( TColumn column, TStatus status, int level )
    {
      if( object.ReferenceEquals( column, null ) )
        throw new ArgumentNullException( "column" );

      if( level < 0 )
        throw new ArgumentException( "The level must be greater than or equal to zero.", "level" );

      if( level >= m_level )
        throw new ArgumentException( "The level is not available.", "level" );

      if( m_preventLayoutChanged )
        throw new InvalidOperationException( "The layout cannot be updated." );

      var hashCode = column.GetHashCode();

      if( m_size > 0 )
      {
        if( this.FindColumn( column, hashCode ) != ColumnHierarchyModel<TColumn, TStatus>.None )
          throw new ArgumentException( "The column is already in the collection.", "column" );
      }

      var orphan = this.FindMark( level ).Orphan;
      Debug.Assert( orphan != ColumnHierarchyModel<TColumn, TStatus>.None );

      var index = this.AddCore( column, status, hashCode );
      var location = new ColumnLocation( this, column, hashCode, index );

      this.OnLayoutChanged( new LayoutChangedEventArgs( location, LayoutChangedAction.Added ) );

      if( !object.Equals( status, default( TStatus ) ) )
      {
        this.OnStatusChanged( new StatusChangedEventArgs( location, default( TStatus ), status ) );
      }

      this.ConnectBefore( index, orphan );

      return location;
    }

    internal bool Remove( TColumn column )
    {
      if( object.ReferenceEquals( column, null ) )
        throw new ArgumentNullException( "column" );

      if( m_preventLayoutChanged )
        throw new InvalidOperationException( "The layout cannot be updated." );

      if( m_size <= 0 )
        return false;

      var index = this.FindColumn( column );
      if( index == ColumnHierarchyModel<TColumn, TStatus>.None )
        return false;

      var level = this.GetLevel( index );
      var relations = m_relations[ index ];

      // Move all children under the level's orphan entry.
      if( relations.FirstChild != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        var orphan = this.FindMark( level ).Orphan;
        Debug.Assert( orphan != ColumnHierarchyModel<TColumn, TStatus>.None );

        var current = relations.FirstChild;
        var next = this.GetNextSibling( current );

        Debug.Assert( this.IsColumn( current ) );
        this.Disconnect( current );
        this.ConnectUnder( current, orphan );

        var previous = current;
        current = next;

        while( current != ColumnHierarchyModel<TColumn, TStatus>.None )
        {
          next = this.GetNextSibling( current );

          Debug.Assert( this.IsColumn( current ) );
          this.Disconnect( current );
          this.ConnectAfter( current, previous );

          previous = current;
          current = next;
        }
      }

      Debug.Assert( this.GetFirstChild( index ) == ColumnHierarchyModel<TColumn, TStatus>.None );

      this.OnLayoutChanging( new LayoutChangingEventArgs( this.GetTargetLocation( index ), LayoutChangingAction.Removing ) );
      this.Disconnect( index );
      this.RemoveCore( index );

      return true;
    }

    internal bool Contains( TColumn column )
    {
      if( ( m_size <= 0 ) || object.ReferenceEquals( column, null ) )
        return false;

      return ( this.FindColumn( column ) != ColumnHierarchyModel<TColumn, TStatus>.None );
    }

    internal void Clear()
    {
      if( m_preventLayoutChanged )
        throw new InvalidOperationException( "The layout cannot be updated." );

      if( m_size <= 0 )
        return;

      this.OnLayoutChanging( new LayoutChangingEventArgs( null, LayoutChangingAction.Clearing ) );

      Debug.Assert( m_buckets != null );
      Debug.Assert( m_columns != null );
      Debug.Assert( m_status != null );
      Debug.Assert( m_relations != null );
      Debug.Assert( m_marks != null );

      for( int i = 0; i < m_level; i++ )
      {
        m_marks[ i ] = MarkEntry.Default;
      }

      for( int i = 0; i < m_capacity; i++ )
      {
        m_buckets[ i ] = ColumnHierarchyModel<TColumn, TStatus>.None;
        m_columns[ i ] = new ColumnEntry( ( i < m_capacity - 1 ) ? i + 1 : ColumnHierarchyModel<TColumn, TStatus>.None );
        m_status[ i ] = default( TStatus );
        m_relations[ i ] = RelationEntry.Default;
      }

      m_level = 0;
      m_free = 0;
      m_size = 0;

      this.OnLayoutChanged( new LayoutChangedEventArgs( null, LayoutChangedAction.Cleared ) );
    }

    internal IMarkers GetLevelMarkers( int level )
    {
      if( level < 0 )
        throw new ArgumentException( "The level must be greater than or equal to zero.", "level" );

      if( level >= m_level )
        throw new ArgumentException( "The level must be less than LevelCount.", "level" );

      var entry = this.FindMark( level );

      return new Markers( this, level, entry );
    }

    private Markers AddLevelCore( int level )
    {
      Debug.Assert( ( level >= 0 ) && ( level <= m_level ) );
      Debug.Assert( m_marks != null );
      Debug.Assert( level < m_marks.Length );
      Debug.Assert( object.Equals( m_marks.Last(), MarkEntry.Default ) );

      // Offset the marks to insert a entry that will deal with the new level.
      for( int i = m_marks.Length - 1; i > level; i-- )
      {
        m_marks[ i ] = m_marks[ i - 1 ];
      }

      var start = this.AddCore( default( TStatus ) );
      var end = this.AddCore( default( TStatus ) );
      var splitter = this.AddCore( default( TStatus ) );
      var orphan = this.AddCore( default( TStatus ) );
      var markEntry = new MarkEntry( start, end, splitter, orphan );
      var markers = new Markers( this, level, markEntry );

      m_marks[ level ] = markEntry;
      m_level++;

      this.OnLayoutChanged( new LayoutChangedEventArgs( markers.Start, LayoutChangedAction.Added ) );
      this.OnLayoutChanged( new LayoutChangedEventArgs( markers.End, LayoutChangedAction.Added ) );
      this.OnLayoutChanged( new LayoutChangedEventArgs( markers.Splitter, LayoutChangedAction.Added ) );
      this.OnLayoutChanged( new LayoutChangedEventArgs( markers.Orphan, LayoutChangedAction.Added ) );

      var parentStart = ColumnHierarchyModel<TColumn, TStatus>.None;
      var parentEnd = ColumnHierarchyModel<TColumn, TStatus>.None;
      var parentSplitter = ColumnHierarchyModel<TColumn, TStatus>.None;
      var parentOrphan = ColumnHierarchyModel<TColumn, TStatus>.None;
      var childStart = ColumnHierarchyModel<TColumn, TStatus>.None;
      var childEnd = ColumnHierarchyModel<TColumn, TStatus>.None;
      var childSplitter = ColumnHierarchyModel<TColumn, TStatus>.None;
      var childOrphan = ColumnHierarchyModel<TColumn, TStatus>.None;

      if( level + 1 < m_marks.Length )
      {
        var entry = m_marks[ level + 1 ];

        parentStart = entry.Start;
        parentEnd = entry.End;
        parentSplitter = entry.Splitter;
        parentOrphan = entry.Orphan;
      }

      if( level > 0 )
      {
        var entry = m_marks[ level - 1 ];

        childStart = entry.Start;
        childEnd = entry.End;
        childSplitter = entry.Splitter;
        childOrphan = entry.Orphan;
      }

      // Move the entries on the child level under the new level entries.
      if( childStart != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        Debug.Assert( childStart != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( childEnd != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( childSplitter != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( childOrphan != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( childOrphan == this.GetPreviousSiblingOrCousin( childEnd ) );
        Debug.Assert( this.GetNextSiblingOrCousin( childEnd ) == ColumnHierarchyModel<TColumn, TStatus>.None );

        var next = childOrphan;
        var current = this.GetPreviousSiblingOrCousin( next );

        // Move the child orphan entry under the new level's orphan entry.
        this.Disconnect( childOrphan );
        this.ConnectUnder( childOrphan, orphan );

        // Move the columns under the new level's orphan entry.
        while( current != ColumnHierarchyModel<TColumn, TStatus>.None )
        {
          var previous = this.GetPreviousSiblingOrCousin( current );

          if( this.IsColumn( current ) )
          {
            this.Disconnect( current );
            this.ConnectBefore( current, next );

            next = current;
          }

          current = previous;
        }

        // Move the remaining marker entries under the new level's related marker entries.
        this.Disconnect( childStart );
        this.Disconnect( childEnd );
        this.Disconnect( childSplitter );

        this.ConnectUnder( childStart, start );
        this.ConnectUnder( childEnd, end );
        this.ConnectUnder( childSplitter, splitter );

        Debug.Assert( this.GetFirstChild( start ) != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( this.GetFirstChild( end ) != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( this.GetFirstChild( splitter ) != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( this.GetFirstChild( orphan ) != ColumnHierarchyModel<TColumn, TStatus>.None );
      }

      // Move the new level's marker entries under the parent level's related marker entries.
      if( parentStart != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        Debug.Assert( parentStart != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( parentEnd != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( parentSplitter != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( parentOrphan != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( parentOrphan == this.GetPreviousSiblingOrCousin( parentEnd ) );
        Debug.Assert( this.GetNextSiblingOrCousin( parentEnd ) == ColumnHierarchyModel<TColumn, TStatus>.None );

        Debug.Assert( this.GetFirstChild( parentStart ) == ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( this.GetFirstChild( parentEnd ) == ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( this.GetFirstChild( parentSplitter ) == ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( this.GetFirstChild( parentOrphan ) == ColumnHierarchyModel<TColumn, TStatus>.None );

        this.ConnectUnder( start, parentStart );
        this.ConnectUnder( end, parentEnd );
        this.ConnectUnder( splitter, parentSplitter );
        this.ConnectUnder( orphan, parentOrphan );
      }
      // Order the new level's marker entries.
      else
      {
        this.ConnectAfter( splitter, start );
        this.ConnectAfter( orphan, splitter );
        this.ConnectAfter( end, orphan );
      }

      return markers;
    }

    private void RemoveLevelCore( int level )
    {
      var markers = this.FindMark( level );
      var start = markers.Start;
      var end = markers.End;
      var splitter = markers.Splitter;
      var orphan = markers.Orphan;
      Debug.Assert( start != ColumnHierarchyModel<TColumn, TStatus>.None );
      Debug.Assert( end != ColumnHierarchyModel<TColumn, TStatus>.None );
      Debug.Assert( splitter != ColumnHierarchyModel<TColumn, TStatus>.None );
      Debug.Assert( orphan != ColumnHierarchyModel<TColumn, TStatus>.None );

      var parentStart = ColumnHierarchyModel<TColumn, TStatus>.None;
      var parentEnd = ColumnHierarchyModel<TColumn, TStatus>.None;
      var parentSplitter = ColumnHierarchyModel<TColumn, TStatus>.None;
      var parentOrphan = ColumnHierarchyModel<TColumn, TStatus>.None;
      var childStart = ColumnHierarchyModel<TColumn, TStatus>.None;
      var childEnd = ColumnHierarchyModel<TColumn, TStatus>.None;
      var childSplitter = ColumnHierarchyModel<TColumn, TStatus>.None;
      var childOrphan = ColumnHierarchyModel<TColumn, TStatus>.None;

      if( level + 1 < m_marks.Length )
      {
        var entry = m_marks[ level + 1 ];

        parentStart = entry.Start;
        parentEnd = entry.End;
        parentSplitter = entry.Splitter;
        parentOrphan = entry.Orphan;
      }

      if( level > 0 )
      {
        var entry = m_marks[ level - 1 ];

        childStart = entry.Start;
        childEnd = entry.End;
        childSplitter = entry.Splitter;
        childOrphan = entry.Orphan;
      }

      if( childStart != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        Debug.Assert( childStart != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( childEnd != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( childSplitter != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( childOrphan != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( childOrphan == this.GetPreviousSiblingOrCousin( childEnd ) );
        Debug.Assert( this.GetNextSiblingOrCousin( childEnd ) == ColumnHierarchyModel<TColumn, TStatus>.None );

        // Link all entries on the child level together.  They will be rearrange later if necessary.
        var next = ColumnHierarchyModel<TColumn, TStatus>.None;
        var current = childEnd;

        while( current != ColumnHierarchyModel<TColumn, TStatus>.None )
        {
          var previous = this.GetPreviousSiblingOrCousin( current );

          this.Disconnect( current );

          if( next != ColumnHierarchyModel<TColumn, TStatus>.None )
          {
            this.ConnectBefore( current, next );
          }
          else
          {
            this.SetRelations( current, new RelationEntry( ColumnHierarchyModel<TColumn, TStatus>.None, this.GetFirstChild( current ), ColumnHierarchyModel<TColumn, TStatus>.None, ColumnHierarchyModel<TColumn, TStatus>.None ) );
          }

          next = current;
          current = previous;
        }
      }

      // Remove any relation to entries of the removed level.
      {
        var current = end;
        while( current != ColumnHierarchyModel<TColumn, TStatus>.None )
        {
          var previous = this.GetPreviousSiblingOrCousin( current );

          this.OnLayoutChanging( new LayoutChangingEventArgs( this.GetTargetLocation( current ), LayoutChangingAction.Removing ) );
          this.Disconnect( current );
          this.RemoveCore( current );

          current = previous;
        }
      }

      // Offset the marks to remove the level that was removed.
      for( int i = level; i < m_level - 1; i++ )
      {
        m_marks[ i ] = m_marks[ i + 1 ];
      }

      m_level--;
      m_marks[ m_level ] = MarkEntry.Default;

      // Reconnect the entries on the child level under the parent level's related entries.
      if( ( childStart != ColumnHierarchyModel<TColumn, TStatus>.None ) && ( parentStart != ColumnHierarchyModel<TColumn, TStatus>.None ) )
      {
        Debug.Assert( parentStart != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( parentEnd != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( parentSplitter != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( parentOrphan != ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( parentOrphan == this.GetPreviousSibling( parentEnd ) );
        Debug.Assert( this.GetFirstChild( parentStart ) == ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( this.GetFirstChild( parentEnd ) == ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( this.GetFirstChild( parentSplitter ) == ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( this.GetFirstChild( parentOrphan ) == ColumnHierarchyModel<TColumn, TStatus>.None );
        Debug.Assert( this.GetNextSiblingOrCousin( parentEnd ) == ColumnHierarchyModel<TColumn, TStatus>.None );

        var next = childOrphan;
        var current = this.GetPreviousSibling( next );

        // Move the child orphan entry under the parent level's orphan entry.
        this.Disconnect( childOrphan );
        this.ConnectUnder( childOrphan, parentOrphan );

        // Move the columns under the parent level's orphan entry.
        while( current != ColumnHierarchyModel<TColumn, TStatus>.None )
        {
          var previous = this.GetPreviousSibling( current );

          if( this.IsColumn( current ) )
          {
            this.Disconnect( current );
            this.ConnectBefore( current, next );

            next = current;
          }

          current = previous;
        }

        // Move the remaining marker entries under the parent level's related marker entries.
        this.Disconnect( childStart );
        this.Disconnect( childEnd );
        this.Disconnect( childSplitter );

        this.ConnectUnder( childStart, parentStart );
        this.ConnectUnder( childEnd, parentEnd );
        this.ConnectUnder( childSplitter, parentSplitter );

        Debug.Assert( this.GetFirstChild( parentStart ) == childStart );
        Debug.Assert( this.GetFirstChild( parentEnd ) == childEnd );
        Debug.Assert( this.GetFirstChild( parentSplitter ) == childSplitter );
        Debug.Assert( this.GetFirstChild( parentOrphan ) != ColumnHierarchyModel<TColumn, TStatus>.None );
      }
    }

    private int AddCore( TStatus status )
    {
      this.EnsureCapacity();

      Debug.Assert( m_columns != null );
      Debug.Assert( m_status != null );
      Debug.Assert( m_relations != null );

      var free = m_free;

      m_free = m_columns[ m_free ].Next;
      m_columns[ free ] = new ColumnEntry( ColumnHierarchyModel<TColumn, TStatus>.None );
      m_status[ free ] = status;
      m_relations[ free ] = RelationEntry.Default;

      m_size++;

      return free;
    }

    private int AddCore( TColumn column, TStatus status, int hashCode )
    {
      var index = this.AddCore( status );

      Debug.Assert( ( index >= 0 ) && ( index < m_capacity ) );
      Debug.Assert( m_buckets != null );
      Debug.Assert( m_columns != null );
      Debug.Assert( m_status != null );

      var bucket = this.GetBucket( hashCode );
      Debug.Assert( ( bucket >= 0 ) && ( bucket < m_buckets.Length ) );

      var collision = m_buckets[ bucket ];

      m_buckets[ bucket ] = index;
      m_columns[ index ] = new ColumnEntry( column, hashCode, collision );

      return index;
    }

    private void RemoveCore( int index )
    {
      Debug.Assert( m_buckets != null );
      Debug.Assert( m_columns != null );
      Debug.Assert( m_status != null );
      Debug.Assert( m_relations != null );
      Debug.Assert( ( index >= 0 ) && ( index < m_capacity ) );

      m_status[ index ] = default( TStatus );
      m_relations[ index ] = RelationEntry.Default;

      var entry = m_columns[ index ];

      // Only column entry needs to adjust the buckets.
      if( this.IsColumn( entry ) )
      {
        var bucket = this.GetBucket( entry.HashCode );
        Debug.Assert( ( bucket >= 0 ) && ( bucket < m_buckets.Length ) );
        Debug.Assert( m_buckets[ bucket ] != ColumnHierarchyModel<TColumn, TStatus>.None );

        if( m_buckets[ bucket ] == index )
        {
          m_buckets[ bucket ] = entry.Next;
        }
        else
        {
          // Repair the linked list.
          var previous = m_buckets[ bucket ];
          var next = m_columns[ previous ].Next;

          while( next != index )
          {
            Debug.Assert( next != ColumnHierarchyModel<TColumn, TStatus>.None );
            previous = next;
            next = m_columns[ next ].Next;
          }

          Debug.Assert( next == index );

          m_columns[ previous ] = m_columns[ previous ].SetNext( entry.Next );
        }
      }

      m_columns[ index ] = new ColumnEntry( m_free );
      m_free = index;

      m_size--;
    }

    private void Disconnect( int index )
    {
      var relations = m_relations[ index ];

      if( relations.Previous != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        this.SetRelations( relations.Previous, m_relations[ relations.Previous ].SetNext( relations.Next ) );
      }
      else if( relations.Parent != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        Debug.Assert( m_relations[ relations.Parent ].FirstChild == index );

        this.SetRelations( relations.Parent, m_relations[ relations.Parent ].SetFirstChild( relations.Next ) );
      }

      if( relations.Next != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        this.SetRelations( relations.Next, m_relations[ relations.Next ].SetPrevious( relations.Previous ) );
      }
    }

    private void ConnectBefore( int index, int pivot )
    {
      var sourceRelations = m_relations[ index ];
      var pivotRelations = m_relations[ pivot ];

      if( pivotRelations.Previous != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        Debug.Assert( pivotRelations.Previous != index );

        this.SetRelations( pivotRelations.Previous, m_relations[ pivotRelations.Previous ].SetNext( index ) );
      }
      else if( pivotRelations.Parent != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        Debug.Assert( m_relations[ pivotRelations.Parent ].FirstChild == pivot );

        this.SetRelations( pivotRelations.Parent, m_relations[ pivotRelations.Parent ].SetFirstChild( index ) );
      }

      this.SetRelations( index, new RelationEntry( pivotRelations.Parent, sourceRelations.FirstChild, pivotRelations.Previous, pivot ) );
      this.SetRelations( pivot, pivotRelations.SetPrevious( index ) );
    }

    private void ConnectAfter( int index, int pivot )
    {
      var sourceRelations = m_relations[ index ];
      var pivotRelations = m_relations[ pivot ];

      if( pivotRelations.Next != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        Debug.Assert( pivotRelations.Next != index );

        this.SetRelations( pivotRelations.Next, m_relations[ pivotRelations.Next ].SetPrevious( index ) );
      }

      this.SetRelations( index, new RelationEntry( pivotRelations.Parent, sourceRelations.FirstChild, pivot, pivotRelations.Next ) );
      this.SetRelations( pivot, pivotRelations.SetNext( index ) );
    }

    private void ConnectUnder( int index, int pivot )
    {
      var sourceRelations = m_relations[ index ];
      var pivotRelations = m_relations[ pivot ];

      if( pivotRelations.FirstChild != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        Debug.Assert( pivotRelations.FirstChild != index );
        Debug.Assert( m_relations[ pivotRelations.FirstChild ].Previous == ColumnHierarchyModel<TColumn, TStatus>.None );

        this.SetRelations( pivotRelations.FirstChild, m_relations[ pivotRelations.FirstChild ].SetPrevious( index ) );
      }

      this.SetRelations( index, new RelationEntry( pivot, sourceRelations.FirstChild, ColumnHierarchyModel<TColumn, TStatus>.None, pivotRelations.FirstChild ) );
      this.SetRelations( pivot, pivotRelations.SetFirstChild( index ) );
    }

    private void MoveBefore( int source, int pivot )
    {
      if( m_preventLayoutChanged )
        throw new InvalidOperationException( "The layout cannot be updated." );

      Debug.Assert( ( source >= 0 ) && ( source < m_capacity ) );
      Debug.Assert( ( pivot >= 0 ) && ( pivot < m_capacity ) );

      // The entry is already at the proper location.
      if( m_relations[ pivot ].Previous == source )
        return;

      if( source == pivot )
        throw new ArgumentException( "Cannot move the entry before itself.", "source" );

      Debug.Assert( this.GetAncestorCount( source ) == this.GetAncestorCount( pivot ), "Cannot move entries on different level." );
      Debug.Assert( !this.IsStart( pivot ), "No entry can be moved before the start." );
      Debug.Assert( !this.IsStart( source ) && !this.IsEnd( source ) && !this.IsOrphan( source ), "The entry cannot be moved." );
      Debug.Assert( !this.IsSplitter( source ) || ( this.GetLevel( source ) == m_level - 1 ), "Only the topmost splitter can be moved." );
      Debug.Assert( !this.IsSplitter( pivot ) || ( this.GetLevel( pivot ) == m_level - 1 ), "An entry cannot be moved before the splitter on this level." );

      this.Disconnect( source );
      this.ConnectBefore( source, pivot );
    }

    private void MoveAfter( int source, int pivot )
    {
      if( m_preventLayoutChanged )
        throw new InvalidOperationException( "The layout cannot be updated." );

      Debug.Assert( ( source >= 0 ) && ( source < m_capacity ) );
      Debug.Assert( ( pivot >= 0 ) && ( pivot < m_capacity ) );

      // The entry is already at the proper location.
      if( m_relations[ pivot ].Next == source )
        return;

      if( source == pivot )
        throw new ArgumentException( "Cannot move the entry after itself.", "source" );

      Debug.Assert( this.GetAncestorCount( source ) == this.GetAncestorCount( pivot ), "Cannot move entries on different level." );
      Debug.Assert( !this.IsEnd( pivot ), "No entry can be moved after the end." );
      Debug.Assert( !this.IsOrphan( pivot ), "No entry can be moved after an orphan entry." );
      Debug.Assert( !this.IsStart( source ) && !this.IsEnd( source ) && !this.IsOrphan( source ), "The entry cannot be moved." );
      Debug.Assert( !this.IsSplitter( source ) || ( this.GetLevel( source ) == m_level - 1 ), "Only the topmost splitter can be move." );
      Debug.Assert( !this.IsSplitter( pivot ) || ( this.GetLevel( pivot ) == m_level - 1 ), "An entry cannot be moved after the splitter on this level." );

      this.Disconnect( source );
      this.ConnectAfter( source, pivot );
    }

    private void MoveUnder( int source, int pivot )
    {
      if( m_preventLayoutChanged )
        throw new InvalidOperationException( "The layout cannot be updated." );

      Debug.Assert( ( source >= 0 ) && ( source < m_capacity ) );
      Debug.Assert( ( pivot >= 0 ) && ( pivot < m_capacity ) );

      // The entry is already at the proper location.
      if( m_relations[ source ].Parent == pivot )
        return;

      if( source == pivot )
        throw new ArgumentException( "Cannot move the entry under itself.", "source" );

      Debug.Assert( this.GetAncestorCount( source ) == ( this.GetAncestorCount( pivot ) + 1 ), "The entry must be a location on the previous level." );
      Debug.Assert( !this.IsStart( source ) && !this.IsEnd( source ) && !this.IsOrphan( source ), "The entry cannot be moved." );
      Debug.Assert( !this.IsSplitter( source ), "A splitter cannot be moved under another entry." );
      Debug.Assert( !this.IsSplitter( pivot ), "An entry cannot be moved under a splitter." );

      this.Disconnect( source );
      this.ConnectUnder( source, pivot );
    }

    private void SetRelations( int index, RelationEntry relations )
    {
      Debug.Assert( ( index >= 0 ) && ( index < m_capacity ) );

      var current = m_relations[ index ];
      var location = this.GetTargetLocation( index );
      Debug.Assert( location != null );

      if( current.Parent != relations.Parent )
      {
        this.OnLayoutChanging( new LayoutChangingEventArgs( location, LayoutChangingAction.ChangingParent ) );
      }

      if( current.FirstChild != relations.FirstChild )
      {
        this.OnLayoutChanging( new LayoutChangingEventArgs( location, LayoutChangingAction.ChangingFirstChild ) );
      }

      if( current.Previous != relations.Previous )
      {
        this.OnLayoutChanging( new LayoutChangingEventArgs( location, LayoutChangingAction.ChangingPrevious ) );
      }

      if( current.Next != relations.Next )
      {
        this.OnLayoutChanging( new LayoutChangingEventArgs( location, LayoutChangingAction.ChangingNext ) );
      }

      m_relations[ index ] = relations;

      if( current.Parent != relations.Parent )
      {
        this.OnLayoutChanged( new LayoutChangedEventArgs( location, LayoutChangedAction.ParentChanged ) );
      }

      if( current.FirstChild != relations.FirstChild )
      {
        this.OnLayoutChanged( new LayoutChangedEventArgs( location, LayoutChangedAction.FirstChildChanged ) );
      }

      if( current.Previous != relations.Previous )
      {
        this.OnLayoutChanged( new LayoutChangedEventArgs( location, LayoutChangedAction.PreviousChanged ) );
      }

      if( current.Next != relations.Next )
      {
        this.OnLayoutChanged( new LayoutChangedEventArgs( location, LayoutChangedAction.NextChanged ) );
      }
    }

    private TStatus GetStatus( int index )
    {
      Debug.Assert( ( index >= 0 ) && ( index < m_capacity ) );

      return m_status[ index ];
    }

    private void SetStatus( int index, TStatus value )
    {
      Debug.Assert( ( index >= 0 ) && ( index < m_capacity ) );

      var oldValue = m_status[ index ];
      if( object.Equals( oldValue, value ) )
        return;

      var columnLocation = this.GetTargetLocation( index ) as IColumnLocation;
      Debug.Assert( columnLocation != null );

      m_status[ index ] = value;

      this.OnStatusChanged( new StatusChangedEventArgs( columnLocation, oldValue, value ) );
    }

    private bool IsStart( int index )
    {
      if( ( index < 0 ) || ( index >= m_capacity ) || this.IsColumn( index ) )
        return false;

      for( int level = m_level - 1; level >= 0; level-- )
      {
        if( index == this.FindMark( level ).Start )
          return true;
      }

      return false;
    }

    private bool IsEnd( int index )
    {
      if( ( index < 0 ) || ( index >= m_capacity ) || this.IsColumn( index ) )
        return false;

      for( int level = m_level - 1; level >= 0; level-- )
      {
        if( index == this.FindMark( level ).End )
          return true;
      }

      return false;
    }

    private bool IsSplitter( int index )
    {
      if( ( index < 0 ) || ( index >= m_capacity ) || this.IsColumn( index ) )
        return false;

      for( int level = m_level - 1; level >= 0; level-- )
      {
        if( index == this.FindMark( level ).Splitter )
          return true;
      }

      return false;
    }

    private bool IsOrphan( int index )
    {
      if( ( index < 0 ) || ( index >= m_capacity ) || this.IsColumn( index ) )
        return false;

      for( int level = m_level - 1; level >= 0; level-- )
      {
        if( index == this.FindMark( level ).Orphan )
          return true;
      }

      return false;
    }

    private bool IsColumn( int index )
    {
      if( ( index < 0 ) || ( index >= m_capacity ) )
        return false;

      return this.IsColumn( m_columns[ index ] );
    }

    private bool IsColumn( ColumnEntry entry )
    {
      return !object.ReferenceEquals( entry.Column, null );
    }

    private MarkEntry FindMark( int level )
    {
      Debug.Assert( ( level >= 0 ) && ( level < m_level ) );
      Debug.Assert( m_marks != null );
      Debug.Assert( level < m_marks.Length );

      return m_marks[ level ];
    }

    private int FindColumn( TColumn column )
    {
      Debug.Assert( !object.ReferenceEquals( column, null ) );

      return this.FindColumn( column, column.GetHashCode() );
    }

    private int FindColumn( TColumn column, int hashCode )
    {
      Debug.Assert( !object.ReferenceEquals( column, null ) );
      Debug.Assert( m_buckets != null );
      Debug.Assert( m_columns != null );

      var bucket = this.GetBucket( hashCode );
      Debug.Assert( ( bucket >= 0 ) && ( bucket < m_buckets.Length ) );

      var index = m_buckets[ bucket ];

      while( index != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        var entry = m_columns[ index ];
        if( ( entry.HashCode == hashCode ) && object.Equals( entry.Column, column ) )
          return index;

        index = entry.Next;
      }

      return ColumnHierarchyModel<TColumn, TStatus>.None;
    }

    private StartLocation CreateStartLocation( int level, int index )
    {
      Debug.Assert( this.IsStart( index ) );

      return new StartLocation( this, level, index );
    }

    private EndLocation CreateEndLocation( int level, int index )
    {
      Debug.Assert( this.IsEnd( index ) );

      return new EndLocation( this, level, index );
    }

    private SplitterLocation CreateSplitterLocation( int level, int index )
    {
      Debug.Assert( this.IsSplitter( index ) );

      return new SplitterLocation( this, level, index );
    }

    private OrphanLocation CreateOrphanLocation( int level, int index )
    {
      Debug.Assert( this.IsOrphan( index ) );

      return new OrphanLocation( this, level, index );
    }

    private ColumnLocation CreateColumnLocation( int index )
    {
      Debug.Assert( this.IsColumn( index ) );

      var entry = m_columns[ index ];

      return new ColumnLocation( this, entry.Column, entry.HashCode, index );
    }

    private int GetParent( int index )
    {
      Debug.Assert( ( index >= 0 ) && ( index < m_capacity ) );
      Debug.Assert( m_relations != null );

      return m_relations[ index ].Parent;
    }

    private int GetFirstChild( int index )
    {
      Debug.Assert( ( index >= 0 ) && ( index < m_capacity ) );
      Debug.Assert( m_relations != null );

      return m_relations[ index ].FirstChild;
    }

    private int GetPreviousSibling( int index )
    {
      Debug.Assert( ( index >= 0 ) && ( index < m_capacity ) );
      Debug.Assert( m_relations != null );

      return m_relations[ index ].Previous;
    }

    private int GetNextSibling( int index )
    {
      Debug.Assert( ( index >= 0 ) && ( index < m_capacity ) );
      Debug.Assert( m_relations != null );

      return m_relations[ index ].Next;
    }

    private int GetPreviousSiblingOrCousin( int index )
    {
      Debug.Assert( ( index >= 0 ) && ( index < m_capacity ) );

      var relations = m_relations[ index ];
      var target = relations.Previous;

      if( target != ColumnHierarchyModel<TColumn, TStatus>.None )
        return target;

      var parent = relations.Parent;
      if( parent != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        var oncle = parent;

        while( true )
        {
          oncle = this.GetPreviousSiblingOrCousin( oncle );
          if( oncle == ColumnHierarchyModel<TColumn, TStatus>.None )
            break;

          target = this.GetFirstChild( oncle );
          if( target != ColumnHierarchyModel<TColumn, TStatus>.None )
          {
            while( this.GetNextSibling( target ) != ColumnHierarchyModel<TColumn, TStatus>.None )
            {
              target = this.GetNextSibling( target );
            }

            return target;
          }
        }
      }

      // No more sibling.
      return ColumnHierarchyModel<TColumn, TStatus>.None;
    }

    private int GetNextSiblingOrCousin( int index )
    {
      Debug.Assert( ( index >= 0 ) && ( index < m_capacity ) );

      var relations = m_relations[ index ];
      var target = relations.Next;

      if( target != ColumnHierarchyModel<TColumn, TStatus>.None )
        return target;

      var parent = relations.Parent;
      if( parent != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        var oncle = parent;

        while( true )
        {
          oncle = this.GetNextSiblingOrCousin( oncle );
          if( oncle == ColumnHierarchyModel<TColumn, TStatus>.None )
            break;

          target = this.GetFirstChild( oncle );
          if( target != ColumnHierarchyModel<TColumn, TStatus>.None )
            return target;
        }
      }

      // No more sibling.
      return ColumnHierarchyModel<TColumn, TStatus>.None;
    }

    private ILocation GetTargetLocation( int index )
    {
      if( index == ColumnHierarchyModel<TColumn, TStatus>.None )
        return null;

      if( this.IsColumn( index ) )
        return this.CreateColumnLocation( index );

      for( int level = m_level - 1; level >= 0; level-- )
      {
        var entry = this.FindMark( level );
        if( entry.Start == index )
          return this.CreateStartLocation( level, index );

        if( entry.End == index )
          return this.CreateEndLocation( level, index );

        if( entry.Splitter == index )
          return this.CreateSplitterLocation( level, index );

        if( entry.Orphan == index )
          return this.CreateOrphanLocation( level, index );
      }

      throw new InvalidOperationException();
    }

    private int GetLevel( int index )
    {
      Debug.Assert( m_level > 0 );

      var ancestors = this.GetAncestorCount( index );
      Debug.Assert( ancestors < m_level );

      return m_level - ancestors - 1;
    }

    private int GetAncestorCount( int index )
    {
      Debug.Assert( ( index >= 0 ) && ( index < m_capacity ) );

      var count = -1;

      while( index != ColumnHierarchyModel<TColumn, TStatus>.None )
      {
        index = this.GetParent( index );
        count++;
      }

      Debug.Assert( count >= 0 );

      return count;
    }

    private int GetBucket( int hashCode )
    {
      return ColumnHierarchyModel<TColumn, TStatus>.GetBucket( hashCode, m_capacity );
    }

    private void EnsureMarks( int level, MarkEntry entry )
    {
      if( ( m_marks == null ) || ( level < 0 ) || ( level >= m_marks.Length ) || !object.Equals( entry, m_marks[ level ] ) )
        throw new InvalidOperationException( "The markers are no longer valid." );
    }

    private int EnsureStart( int level, int index )
    {
      if( ( m_marks == null ) || ( level < 0 ) || ( level >= m_marks.Length ) || ( index != m_marks[ level ].Start ) )
        throw new InvalidOperationException( "The location is no longer valid." );

      return level;
    }

    private int EnsureEnd( int level, int index )
    {
      if( ( m_marks == null ) || ( level < 0 ) || ( level >= m_marks.Length ) || ( index != m_marks[ level ].End ) )
        throw new InvalidOperationException( "The location is no longer valid." );

      return level;
    }

    private int EnsureSplitter( int level, int index )
    {
      if( ( m_marks == null ) || ( level < 0 ) || ( level >= m_marks.Length ) || ( index != m_marks[ level ].Splitter ) )
        throw new InvalidOperationException( "The location is no longer valid." );

      return level;
    }

    private int EnsureOrphan( int level, int index )
    {
      if( ( m_marks == null ) || ( level < 0 ) || ( level >= m_marks.Length ) || ( index != m_marks[ level ].Orphan ) )
        throw new InvalidOperationException( "The location is no longer valid." );

      return level;
    }

    private int EnsureColumn( TColumn column, int hashCode, int index )
    {
      if( ( index < 0 ) || ( index >= m_capacity ) )
        throw new InvalidOperationException( "The column location is no longer valid." );

      var entry = m_columns[ index ];
      if( ( hashCode != entry.HashCode ) || !object.Equals( column, entry.Column ) )
        throw new InvalidOperationException( "The column location is no longer valid." );

      return this.GetLevel( index );
    }

    private void EnsureCapacity()
    {
      // Make sure the collection is not full.
      if( m_size != m_capacity )
        return;

      Debug.Assert( m_free == ColumnHierarchyModel<TColumn, TStatus>.None );
      this.EnsureCapacity( m_capacity * 2L );
      Debug.Assert( m_free != ColumnHierarchyModel<TColumn, TStatus>.None );
      Debug.Assert( m_size != m_capacity );
    }

    private void EnsureCapacity( long min )
    {
      if( ( m_capacity >= min ) && ( m_capacity > 0 ) )
        return;

      var capacity = ColumnHierarchyModel<TColumn, TStatus>.FindNextSize( min );
      Debug.Assert( capacity > 0 );

      m_buckets = new int[ capacity ];

      Array.Resize( ref m_columns, capacity );
      Array.Resize( ref m_status, capacity );
      Array.Resize( ref m_relations, capacity );

      for( int i = 0; i < m_buckets.Length; i++ )
      {
        m_buckets[ i ] = ColumnHierarchyModel<TColumn, TStatus>.None;
      }

      for( int i = m_capacity; i < capacity; i++ )
      {
        m_columns[ i ] = new ColumnEntry( ( i < capacity - 1 ) ? i + 1 : ColumnHierarchyModel<TColumn, TStatus>.None );
        m_status[ i ] = default( TStatus );
        m_relations[ i ] = RelationEntry.Default;
      }

      // Rehash the elements to initialize the buckets.
      for( int i = 0; i < m_capacity; i++ )
      {
        var entry = m_columns[ i ];

        // Only column entry needs to adjust the buckets.
        if( this.IsColumn( entry ) )
        {
          var bucket = ColumnHierarchyModel<TColumn, TStatus>.GetBucket( entry.HashCode, capacity );
          Debug.Assert( ( bucket >= 0 ) && ( bucket < capacity ) );

          var index = m_buckets[ bucket ];

          m_buckets[ bucket ] = i;
          m_columns[ i ] = entry.SetNext( index );
        }
      }

      m_free = m_capacity;
      m_capacity = capacity;
    }

    private static int FindNextSize( long min )
    {
      var sizes = ArrayHelper.Sizes;

      for( int i = 0; i < sizes.Length; i++ )
      {
        var size = sizes[ i ];

        if( size >= min )
          return size;
      }

      throw new InvalidOperationException( "Cannot find a larger size." );
    }

    private static int GetBucket( int hashCode, int capacity )
    {
      Debug.Assert( capacity > 0 );

      // Remove the negative sign without using Math.Abs to handle the case of int.MinValue.
      return ( hashCode & 0x7fffffff ) % capacity;
    }

    private int[] m_buckets;
    private ColumnEntry[] m_columns;
    private TStatus[] m_status;
    private RelationEntry[] m_relations;
    private MarkEntry[] m_marks;
    private int m_level; //0
    private int m_capacity; //0
    private int m_size; //0
    private bool m_preventLayoutChanged; //false
    private int m_free = ColumnHierarchyModel<TColumn, TStatus>.None;

    #region IMarkers Internal Interface

    internal interface IMarkers
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

      int Level
      {
        get;
      }

      ILocation GetParent();
      ILocation GetFirstChild();

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
      TColumn Column
      {
        get;
      }

      TStatus Status
      {
        get;
        set;
      }
    }

    #endregion

    #region LayoutChangingEventArgs Internal Class

    internal sealed class LayoutChangingEventArgs : EventArgs
    {
      internal LayoutChangingEventArgs( ILocation location, LayoutChangingAction action )
      {
        m_location = location;
        m_action = action;
      }

      internal ILocation Location
      {
        get
        {
          return m_location;
        }
      }

      internal LayoutChangingAction Action
      {
        get
        {
          return m_action;
        }
      }

      private readonly ILocation m_location;
      private readonly LayoutChangingAction m_action;
    }

    #endregion

    #region LayoutChangedEventArgs Internal Class

    internal sealed class LayoutChangedEventArgs : EventArgs
    {
      internal LayoutChangedEventArgs( ILocation location, LayoutChangedAction action )
      {
        m_location = location;
        m_action = action;
      }

      internal ILocation Location
      {
        get
        {
          return m_location;
        }
      }

      internal LayoutChangedAction Action
      {
        get
        {
          return m_action;
        }
      }

      private readonly ILocation m_location;
      private readonly LayoutChangedAction m_action;
    }

    #endregion

    #region StatusChangedEventArgs Internal Class

    internal sealed class StatusChangedEventArgs : EventArgs
    {
      internal StatusChangedEventArgs( IColumnLocation location, TStatus oldValue, TStatus newValue )
      {
        m_location = location;
        m_oldValue = oldValue;
        m_newValue = newValue;
      }

      internal IColumnLocation Location
      {
        get
        {
          return m_location;
        }
      }

      internal TStatus OldValue
      {
        get
        {
          return m_oldValue;
        }
      }

      internal TStatus NewValue
      {
        get
        {
          return m_newValue;
        }
      }

      private readonly IColumnLocation m_location;
      private readonly TStatus m_oldValue;
      private readonly TStatus m_newValue;
    }

    #endregion

    #region LayoutChangingAction Internal Enum

    internal enum LayoutChangingAction
    {
      Clearing,
      Removing,
      ChangingParent,
      ChangingFirstChild,
      ChangingPrevious,
      ChangingNext,
    }

    #endregion

    #region LayoutChangedAction Internal Enum

    internal enum LayoutChangedAction
    {
      Cleared,
      Added,
      ParentChanged,
      FirstChildChanged,
      PreviousChanged,
      NextChanged,
    }

    #endregion

    #region IPivotLocation Private Interface

    private interface IPivotLocation
    {
      bool CanBeNextSiblingOf( IMovableLocation location );
      bool CanBePreviousSiblingOf( IMovableLocation location );
      bool CanBeParentOf( IMovableLocation location );

      void SetHasNextSiblingOf( IMovableLocation location );
      void SetHasPreviousSiblingOf( IMovableLocation location );
      void SetHasParentOf( IMovableLocation location );
    }

    #endregion

    #region IMovableLocation Private Interface

    private interface IMovableLocation
    {
      bool CanMoveBefore( StartLocation location );
      bool CanMoveBefore( EndLocation location );
      bool CanMoveBefore( SplitterLocation location );
      bool CanMoveBefore( OrphanLocation location );
      bool CanMoveBefore( ColumnLocation location );

      bool CanMoveAfter( StartLocation location );
      bool CanMoveAfter( EndLocation location );
      bool CanMoveAfter( SplitterLocation location );
      bool CanMoveAfter( OrphanLocation location );
      bool CanMoveAfter( ColumnLocation location );

      bool CanMoveUnder( StartLocation location );
      bool CanMoveUnder( EndLocation location );
      bool CanMoveUnder( SplitterLocation location );
      bool CanMoveUnder( OrphanLocation location );
      bool CanMoveUnder( ColumnLocation location );

      void MoveBefore( StartLocation location );
      void MoveBefore( EndLocation location );
      void MoveBefore( SplitterLocation location );
      void MoveBefore( OrphanLocation location );
      void MoveBefore( ColumnLocation location );

      void MoveAfter( StartLocation location );
      void MoveAfter( EndLocation location );
      void MoveAfter( SplitterLocation location );
      void MoveAfter( OrphanLocation location );
      void MoveAfter( ColumnLocation location );

      void MoveUnder( StartLocation location );
      void MoveUnder( EndLocation location );
      void MoveUnder( SplitterLocation location );
      void MoveUnder( OrphanLocation location );
      void MoveUnder( ColumnLocation location );
    }

    #endregion

    #region Markers Private Class

    private sealed class Markers : IMarkers
    {
      internal Markers( ColumnHierarchyModel<TColumn, TStatus> owner, int level, MarkEntry entry )
      {
        Debug.Assert( owner != null );

        m_owner = owner;
        m_level = level;
        m_entry = entry;
      }

      public ILocation Start
      {
        get
        {
          this.EnsureMarks();

          return m_owner.CreateStartLocation( m_level, m_entry.Start );
        }
      }

      public ILocation End
      {
        get
        {
          this.EnsureMarks();

          return m_owner.CreateEndLocation( m_level, m_entry.End );
        }
      }

      public ILocation Splitter
      {
        get
        {
          this.EnsureMarks();

          return m_owner.CreateSplitterLocation( m_level, m_entry.Splitter );
        }
      }

      public ILocation Orphan
      {
        get
        {
          this.EnsureMarks();

          return m_owner.CreateOrphanLocation( m_level, m_entry.Orphan );
        }
      }

      public override int GetHashCode()
      {
        return m_level;
      }

      public override bool Equals( object obj )
      {
        var target = obj as Markers;
        if( target == null )
          return false;

        return ( m_level == target.m_level )
            && ( object.Equals( m_entry, target.m_entry ) )
            && ( m_owner == target.m_owner );
      }

      private void EnsureMarks()
      {
        m_owner.EnsureMarks( m_level, m_entry );
      }

      private readonly ColumnHierarchyModel<TColumn, TStatus> m_owner;
      private readonly int m_level;
      private readonly MarkEntry m_entry;
    }

    #endregion

    #region Location Private Class

    private abstract class Location : ILocation, IMovableLocation, IPivotLocation
    {
      protected Location( ColumnHierarchyModel<TColumn, TStatus> owner, int index )
      {
        Debug.Assert( owner != null );

        m_owner = owner;
        m_index = index;
      }

      protected abstract LocationType Type
      {
        get;
      }

      protected int Level
      {
        get
        {
          return this.EnsureLocation();
        }
      }

      internal int Index
      {
        get
        {
          return m_index;
        }
      }

      internal ColumnHierarchyModel<TColumn, TStatus> Owner
      {
        get
        {
          return m_owner;
        }
      }

      LocationType ILocation.Type
      {
        get
        {
          return this.Type;
        }
      }

      int ILocation.Level
      {
        get
        {
          return this.Level;
        }
      }

      public override int GetHashCode()
      {
        return m_index;
      }

      public override bool Equals( object obj )
      {
        var target = obj as Location;
        if( target == null )
          return false;

        return ( m_index == target.m_index )
            && ( m_owner == target.m_owner );
      }

      protected TStatus GetStatus()
      {
        this.EnsureLocation();

        return m_owner.GetStatus( m_index );
      }

      protected void SetStatus( TStatus status )
      {
        this.EnsureLocation();

        m_owner.SetStatus( m_index, status );
      }

      protected void MoveBeforeCore( Location location )
      {
        m_owner.MoveBefore( m_index, location.m_index );
      }

      protected void MoveAfterCore( Location location )
      {
        m_owner.MoveAfter( m_index, location.m_index );
      }

      protected void MoveUnderCore( Location location )
      {
        m_owner.MoveUnder( m_index, location.m_index );
      }

      protected virtual bool CanMoveBefore( StartLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveBefore( EndLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveBefore( SplitterLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveBefore( OrphanLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveBefore( ColumnLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveAfter( StartLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveAfter( EndLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveAfter( SplitterLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveAfter( OrphanLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveAfter( ColumnLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveUnder( StartLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveUnder( EndLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveUnder( SplitterLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveUnder( OrphanLocation location )
      {
        return false;
      }

      protected virtual bool CanMoveUnder( ColumnLocation location )
      {
        return false;
      }

      protected virtual void MoveBefore( StartLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveBefore( EndLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveBefore( SplitterLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveBefore( OrphanLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveBefore( ColumnLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveAfter( StartLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveAfter( EndLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveAfter( SplitterLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveAfter( OrphanLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveAfter( ColumnLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveUnder( StartLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveUnder( EndLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveUnder( SplitterLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveUnder( OrphanLocation location )
      {
        throw new NotSupportedException();
      }

      protected virtual void MoveUnder( ColumnLocation location )
      {
        throw new NotSupportedException();
      }

      protected abstract bool CanBeNextSiblingOf( IMovableLocation location );
      protected abstract bool CanBePreviousSiblingOf( IMovableLocation location );
      protected abstract bool CanBeParentOf( IMovableLocation location );
      protected abstract void SetHasNextSiblingOf( IMovableLocation location );
      protected abstract void SetHasPreviousSiblingOf( IMovableLocation location );
      protected abstract void SetHasParentOf( IMovableLocation location );

      internal abstract int EnsureLocation();

      private int EnsureLocation( ILocation location )
      {
        if( location == null )
          throw new ArgumentNullException( "location" );

        var target = location as Location;
        if( target == null )
          throw new ArgumentException( "The location must derive from Location.", "location" );

        if( target.Owner != m_owner )
          throw new ArgumentException( "The location must share the same owner.", "location" );

        return target.EnsureLocation();
      }

      private void EnsureSiblingLocation( ILocation location )
      {
        var sourceLevel = this.EnsureLocation();
        var targetLevel = this.EnsureLocation( location );

        if( sourceLevel != targetLevel )
          throw new ArgumentException( "The location must be on the same level.", "location" );
      }

      private void EnsureParentLocation( ILocation location )
      {
        var sourceLevel = this.EnsureLocation();
        var targetLevel = this.EnsureLocation( location );

        if( sourceLevel != targetLevel - 1 )
          throw new ArgumentException( "The location must be on the parent level.", "location" );
      }

      ILocation ILocation.GetParent()
      {
        this.EnsureLocation();
        return m_owner.GetTargetLocation( m_owner.GetParent( m_index ) );
      }

      ILocation ILocation.GetFirstChild()
      {
        this.EnsureLocation();
        return m_owner.GetTargetLocation( m_owner.GetFirstChild( m_index ) );
      }

      ILocation ILocation.GetPreviousSibling()
      {
        this.EnsureLocation();
        return m_owner.GetTargetLocation( m_owner.GetPreviousSibling( m_index ) );
      }

      ILocation ILocation.GetNextSibling()
      {
        this.EnsureLocation();
        return m_owner.GetTargetLocation( m_owner.GetNextSibling( m_index ) );
      }

      ILocation ILocation.GetPreviousSiblingOrCousin()
      {
        this.EnsureLocation();
        return m_owner.GetTargetLocation( m_owner.GetPreviousSiblingOrCousin( m_index ) );
      }

      ILocation ILocation.GetNextSiblingOrCousin()
      {
        this.EnsureLocation();
        return m_owner.GetTargetLocation( m_owner.GetNextSiblingOrCousin( m_index ) );
      }

      bool ILocation.CanMoveBefore( ILocation location )
      {
        if( location == null )
          return false;

        if( object.Equals( location, this ) )
          return false;

        var currentLevel = this.EnsureLocation();
        var targetLevel = this.EnsureLocation( location );

        if( currentLevel != targetLevel )
          return false;

        Debug.Assert( location is IPivotLocation );

        return ( ( IPivotLocation )location ).CanBeNextSiblingOf( this );
      }

      bool ILocation.CanMoveAfter( ILocation location )
      {
        if( location == null )
          return false;

        if( object.Equals( location, this ) )
          return false;

        var currentLevel = this.EnsureLocation();
        var targetLevel = this.EnsureLocation( location );

        if( currentLevel != targetLevel )
          return false;

        Debug.Assert( location is IPivotLocation );

        return ( ( IPivotLocation )location ).CanBePreviousSiblingOf( this );
      }

      bool ILocation.CanMoveUnder( ILocation location )
      {
        if( location == null )
          return false;

        if( object.Equals( location, this ) )
          return false;

        var currentLevel = this.EnsureLocation();
        var targetLevel = this.EnsureLocation( location );

        if( currentLevel != targetLevel - 1 )
          return false;

        Debug.Assert( location is IPivotLocation );

        return ( ( IPivotLocation )location ).CanBeParentOf( this );
      }

      void ILocation.MoveBefore( ILocation location )
      {
        this.EnsureSiblingLocation( location );

        Debug.Assert( ( ( ILocation )this ).CanMoveBefore( location ) );
        Debug.Assert( location is IPivotLocation );

        ( ( IPivotLocation )location ).SetHasNextSiblingOf( this );
      }

      void ILocation.MoveAfter( ILocation location )
      {
        this.EnsureSiblingLocation( location );

        Debug.Assert( ( ( ILocation )this ).CanMoveAfter( location ) );
        Debug.Assert( location is IPivotLocation );

        ( ( IPivotLocation )location ).SetHasPreviousSiblingOf( this );
      }

      void ILocation.MoveUnder( ILocation location )
      {
        this.EnsureParentLocation( location );

        Debug.Assert( ( ( ILocation )this ).CanMoveUnder( location ) );
        Debug.Assert( location is IPivotLocation );

        ( ( IPivotLocation )location ).SetHasParentOf( this );
      }

      bool IMovableLocation.CanMoveBefore( StartLocation location )
      {
        return this.CanMoveBefore( location );
      }

      bool IMovableLocation.CanMoveBefore( EndLocation location )
      {
        return this.CanMoveBefore( location );
      }

      bool IMovableLocation.CanMoveBefore( SplitterLocation location )
      {
        return this.CanMoveBefore( location );
      }

      bool IMovableLocation.CanMoveBefore( OrphanLocation location )
      {
        return this.CanMoveBefore( location );
      }

      bool IMovableLocation.CanMoveBefore( ColumnLocation location )
      {
        return this.CanMoveBefore( location );
      }

      bool IMovableLocation.CanMoveAfter( StartLocation location )
      {
        return this.CanMoveAfter( location );
      }

      bool IMovableLocation.CanMoveAfter( EndLocation location )
      {
        return this.CanMoveAfter( location );
      }

      bool IMovableLocation.CanMoveAfter( SplitterLocation location )
      {
        return this.CanMoveAfter( location );
      }

      bool IMovableLocation.CanMoveAfter( OrphanLocation location )
      {
        return this.CanMoveAfter( location );
      }

      bool IMovableLocation.CanMoveAfter( ColumnLocation location )
      {
        return this.CanMoveAfter( location );
      }

      bool IMovableLocation.CanMoveUnder( StartLocation location )
      {
        return this.CanMoveUnder( location );
      }

      bool IMovableLocation.CanMoveUnder( EndLocation location )
      {
        return this.CanMoveUnder( location );
      }

      bool IMovableLocation.CanMoveUnder( SplitterLocation location )
      {
        return this.CanMoveUnder( location );
      }

      bool IMovableLocation.CanMoveUnder( OrphanLocation location )
      {
        return this.CanMoveUnder( location );
      }

      bool IMovableLocation.CanMoveUnder( ColumnLocation location )
      {
        return this.CanMoveUnder( location );
      }

      void IMovableLocation.MoveBefore( StartLocation location )
      {
        this.MoveBefore( location );
      }

      void IMovableLocation.MoveBefore( EndLocation location )
      {
        this.MoveBefore( location );
      }

      void IMovableLocation.MoveBefore( SplitterLocation location )
      {
        this.MoveBefore( location );
      }

      void IMovableLocation.MoveBefore( OrphanLocation location )
      {
        this.MoveBefore( location );
      }

      void IMovableLocation.MoveBefore( ColumnLocation location )
      {
        this.MoveBefore( location );
      }

      void IMovableLocation.MoveAfter( StartLocation location )
      {
        this.MoveAfter( location );
      }

      void IMovableLocation.MoveAfter( EndLocation location )
      {
        this.MoveAfter( location );
      }

      void IMovableLocation.MoveAfter( SplitterLocation location )
      {
        this.MoveAfter( location );
      }

      void IMovableLocation.MoveAfter( OrphanLocation location )
      {
        this.MoveAfter( location );
      }

      void IMovableLocation.MoveAfter( ColumnLocation location )
      {
        this.MoveAfter( location );
      }

      void IMovableLocation.MoveUnder( StartLocation location )
      {
        this.MoveUnder( location );
      }

      void IMovableLocation.MoveUnder( EndLocation location )
      {
        this.MoveUnder( location );
      }

      void IMovableLocation.MoveUnder( SplitterLocation location )
      {
        this.MoveUnder( location );
      }

      void IMovableLocation.MoveUnder( OrphanLocation location )
      {
        this.MoveUnder( location );
      }

      void IMovableLocation.MoveUnder( ColumnLocation location )
      {
        this.MoveUnder( location );
      }

      bool IPivotLocation.CanBeNextSiblingOf( IMovableLocation location )
      {
        return this.CanBeNextSiblingOf( location );
      }

      bool IPivotLocation.CanBePreviousSiblingOf( IMovableLocation location )
      {
        return this.CanBePreviousSiblingOf( location );
      }

      bool IPivotLocation.CanBeParentOf( IMovableLocation location )
      {
        return this.CanBeParentOf( location );
      }

      void IPivotLocation.SetHasNextSiblingOf( IMovableLocation location )
      {
        this.SetHasNextSiblingOf( location );
      }

      void IPivotLocation.SetHasPreviousSiblingOf( IMovableLocation location )
      {
        this.SetHasPreviousSiblingOf( location );
      }

      void IPivotLocation.SetHasParentOf( IMovableLocation location )
      {
        this.SetHasParentOf( location );
      }

      private readonly ColumnHierarchyModel<TColumn, TStatus> m_owner;
      private readonly int m_index;
    }

    #endregion

    #region StartLocation Private Class

    private sealed class StartLocation : Location
    {
      internal StartLocation( ColumnHierarchyModel<TColumn, TStatus> owner, int level, int index )
        : base( owner, index )
      {
        m_level = level;
      }

      protected override LocationType Type
      {
        get
        {
          return LocationType.Start;
        }
      }

      public override int GetHashCode()
      {
        return base.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as StartLocation;
        if( target == null )
          return false;

        return ( m_level == target.m_level )
            && ( base.Equals( target ) );
      }

      protected override bool CanBeNextSiblingOf( IMovableLocation location )
      {
        return location.CanMoveBefore( this );
      }

      protected override bool CanBePreviousSiblingOf( IMovableLocation location )
      {
        return location.CanMoveAfter( this );
      }

      protected override bool CanBeParentOf( IMovableLocation location )
      {
        return location.CanMoveUnder( this );
      }

      protected override void SetHasNextSiblingOf( IMovableLocation location )
      {
        location.MoveBefore( this );
      }

      protected override void SetHasPreviousSiblingOf( IMovableLocation location )
      {
        location.MoveAfter( this );
      }

      protected override void SetHasParentOf( IMovableLocation location )
      {
        location.MoveUnder( this );
      }

      internal override int EnsureLocation()
      {
        return this.Owner.EnsureStart( m_level, this.Index );
      }

      private readonly int m_level;
    }

    #endregion

    #region EndLocation Private Class

    private sealed class EndLocation : Location
    {
      internal EndLocation( ColumnHierarchyModel<TColumn, TStatus> owner, int level, int index )
        : base( owner, index )
      {
        m_level = level;
      }

      protected override LocationType Type
      {
        get
        {
          return LocationType.End;
        }
      }

      public override int GetHashCode()
      {
        return base.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as EndLocation;
        if( target == null )
          return false;

        return ( m_level == target.m_level )
            && ( base.Equals( target ) );
      }

      protected override bool CanBeNextSiblingOf( IMovableLocation location )
      {
        return location.CanMoveBefore( this );
      }

      protected override bool CanBePreviousSiblingOf( IMovableLocation location )
      {
        return location.CanMoveAfter( this );
      }

      protected override bool CanBeParentOf( IMovableLocation location )
      {
        return location.CanMoveUnder( this );
      }

      protected override void SetHasNextSiblingOf( IMovableLocation location )
      {
        location.MoveBefore( this );
      }

      protected override void SetHasPreviousSiblingOf( IMovableLocation location )
      {
        location.MoveAfter( this );
      }

      protected override void SetHasParentOf( IMovableLocation location )
      {
        location.MoveUnder( this );
      }

      internal override int EnsureLocation()
      {
        return this.Owner.EnsureEnd( m_level, this.Index );
      }

      private readonly int m_level;
    }

    #endregion

    #region SplitterLocation Private Class

    private sealed class SplitterLocation : Location
    {
      internal SplitterLocation( ColumnHierarchyModel<TColumn, TStatus> owner, int level, int index )
        : base( owner, index )
      {
        m_level = level;
      }

      protected override LocationType Type
      {
        get
        {
          return LocationType.Splitter;
        }
      }

      internal bool IsMovable
      {
        get
        {
          return ( m_level == this.Owner.LevelCount - 1 );
        }
      }

      public override int GetHashCode()
      {
        return base.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as SplitterLocation;
        if( target == null )
          return false;

        return ( m_level == target.m_level )
            && ( base.Equals( target ) );
      }

      protected override bool CanMoveBefore( ColumnLocation location )
      {
        return this.CanMoveSplitterBefore( location );
      }

      protected override bool CanMoveBefore( OrphanLocation location )
      {
        return this.CanMoveSplitterBefore( location );
      }

      protected override bool CanMoveAfter( ColumnLocation location )
      {
        return this.CanMoveSplitterAfter( location );
      }

      protected override bool CanMoveAfter( StartLocation location )
      {
        return this.CanMoveSplitterAfter( location );
      }

      protected override void MoveBefore( ColumnLocation location )
      {
        this.MoveSplitterBefore( location );
      }

      protected override void MoveBefore( OrphanLocation location )
      {
        this.MoveSplitterBefore( location );
      }

      protected override void MoveAfter( ColumnLocation location )
      {
        this.MoveSplitterAfter( location );
      }

      protected override void MoveAfter( StartLocation location )
      {
        this.MoveSplitterAfter( location );
      }

      protected override bool CanBeNextSiblingOf( IMovableLocation location )
      {
        return location.CanMoveBefore( this );
      }

      protected override bool CanBePreviousSiblingOf( IMovableLocation location )
      {
        return location.CanMoveAfter( this );
      }

      protected override bool CanBeParentOf( IMovableLocation location )
      {
        return location.CanMoveUnder( this );
      }

      protected override void SetHasNextSiblingOf( IMovableLocation location )
      {
        location.MoveBefore( this );
      }

      protected override void SetHasPreviousSiblingOf( IMovableLocation location )
      {
        location.MoveAfter( this );
      }

      protected override void SetHasParentOf( IMovableLocation location )
      {
        location.MoveUnder( this );
      }

      internal override int EnsureLocation()
      {
        return this.Owner.EnsureSplitter( m_level, this.Index );
      }

      private bool CanMoveSplitterBefore( ILocation location )
      {
        if( this.IsMovable )
          return true;

        // The current and target locations should not be on the top-most level.
        Debug.Assert( m_level < this.Owner.LevelCount - 1 );

        // The splitter may not move or a merged column is going to be splitted.
        if( location.GetPreviousSibling() != null )
          return false;

        var parentLocation = ( ( ILocation )this ).GetParent();
        Debug.Assert( parentLocation != null );

        return parentLocation.CanMoveBefore( location.GetParent() );
      }

      private bool CanMoveSplitterAfter( ILocation location )
      {
        if( this.IsMovable )
          return true;

        // The current and target locations should not be on the top-most level.
        Debug.Assert( m_level < this.Owner.LevelCount - 1 );

        // The splitter may not move or a merged column is going to be splitted.
        if( location.GetNextSibling() != null )
          return false;

        var parentLocation = ( ( ILocation )this ).GetParent();
        Debug.Assert( parentLocation != null );

        return parentLocation.CanMoveAfter( location.GetParent() );
      }

      private void MoveSplitterBefore( Location location )
      {
        if( this.IsMovable )
        {
          this.MoveBeforeCore( location );
        }
        else
        {
          var parentLocation = ( ( ILocation )this ).GetParent();
          Debug.Assert( parentLocation != null );

          parentLocation.MoveBefore( ( ( ILocation )location ).GetParent() );
        }
      }

      private void MoveSplitterAfter( Location location )
      {
        if( this.IsMovable )
        {
          this.MoveAfterCore( location );
        }
        else
        {
          var parentLocation = ( ( ILocation )this ).GetParent();
          Debug.Assert( parentLocation != null );

          parentLocation.MoveAfter( ( ( ILocation )location ).GetParent() );
        }
      }

      private readonly int m_level;
    }

    #endregion

    #region OrphanLocation Private Class

    private sealed class OrphanLocation : Location
    {
      internal OrphanLocation( ColumnHierarchyModel<TColumn, TStatus> owner, int level, int index )
        : base( owner, index )
      {
        m_level = level;
      }

      protected override LocationType Type
      {
        get
        {
          return LocationType.Orphan;
        }
      }

      public override int GetHashCode()
      {
        return base.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as OrphanLocation;
        if( target == null )
          return false;

        return ( m_level == target.m_level )
            && ( base.Equals( target ) );
      }

      protected override bool CanBeNextSiblingOf( IMovableLocation location )
      {
        return location.CanMoveBefore( this );
      }

      protected override bool CanBePreviousSiblingOf( IMovableLocation location )
      {
        return location.CanMoveAfter( this );
      }

      protected override bool CanBeParentOf( IMovableLocation location )
      {
        return location.CanMoveUnder( this );
      }

      protected override void SetHasNextSiblingOf( IMovableLocation location )
      {
        location.MoveBefore( this );
      }

      protected override void SetHasPreviousSiblingOf( IMovableLocation location )
      {
        location.MoveAfter( this );
      }

      protected override void SetHasParentOf( IMovableLocation location )
      {
        location.MoveUnder( this );
      }

      internal override int EnsureLocation()
      {
        return this.Owner.EnsureOrphan( m_level, this.Index );
      }

      private readonly int m_level;
    }

    #endregion

    #region ColumnLocation Private Class

    private sealed class ColumnLocation : Location, IColumnLocation
    {
      internal ColumnLocation( ColumnHierarchyModel<TColumn, TStatus> owner, TColumn column, int hashCode, int index )
        : base( owner, index )
      {
        m_column = column;
        m_hashCode = hashCode;
      }

      protected override LocationType Type
      {
        get
        {
          return LocationType.Column;
        }
      }

      TColumn IColumnLocation.Column
      {
        get
        {
          return m_column;
        }
      }

      TStatus IColumnLocation.Status
      {
        get
        {
          return this.GetStatus();
        }
        set
        {
          this.SetStatus( value );
        }
      }

      public override int GetHashCode()
      {
        return base.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as ColumnLocation;
        if( target == null )
          return false;

        return ( object.Equals( m_column, target.m_column ) )
            && ( base.Equals( target ) );
      }

      protected override bool CanMoveBefore( ColumnLocation location )
      {
        return true;
      }

      protected override bool CanMoveBefore( SplitterLocation location )
      {
        // The column may move only next to the the top-most splitter.
        return location.IsMovable;
      }

      protected override bool CanMoveBefore( OrphanLocation location )
      {
        return true;
      }

      protected override bool CanMoveAfter( ColumnLocation location )
      {
        return true;
      }

      protected override bool CanMoveAfter( SplitterLocation location )
      {
        // The column may move only next to the the top-most splitter.
        return location.IsMovable;
      }

      protected override bool CanMoveAfter( StartLocation location )
      {
        return true;
      }

      protected override bool CanMoveUnder( ColumnLocation location )
      {
        return true;
      }

      protected override bool CanMoveUnder( OrphanLocation location )
      {
        return true;
      }

      protected override void MoveBefore( ColumnLocation location )
      {
        this.MoveBeforeCore( location );
      }

      protected override void MoveBefore( SplitterLocation location )
      {
        this.MoveBeforeCore( location );
      }

      protected override void MoveBefore( OrphanLocation location )
      {
        this.MoveBeforeCore( location );
      }

      protected override void MoveAfter( ColumnLocation location )
      {
        this.MoveAfterCore( location );
      }

      protected override void MoveAfter( SplitterLocation location )
      {
        this.MoveAfterCore( location );
      }

      protected override void MoveAfter( StartLocation location )
      {
        this.MoveAfterCore( location );
      }

      protected override void MoveUnder( ColumnLocation location )
      {
        this.MoveUnderCore( location );
      }

      protected override void MoveUnder( OrphanLocation location )
      {
        this.MoveUnderCore( location );
      }

      protected override bool CanBeNextSiblingOf( IMovableLocation location )
      {
        return location.CanMoveBefore( this );
      }

      protected override bool CanBePreviousSiblingOf( IMovableLocation location )
      {
        return location.CanMoveAfter( this );
      }

      protected override bool CanBeParentOf( IMovableLocation location )
      {
        return location.CanMoveUnder( this );
      }

      protected override void SetHasNextSiblingOf( IMovableLocation location )
      {
        location.MoveBefore( this );
      }

      protected override void SetHasPreviousSiblingOf( IMovableLocation location )
      {
        location.MoveAfter( this );
      }

      protected override void SetHasParentOf( IMovableLocation location )
      {
        location.MoveUnder( this );
      }

      internal override int EnsureLocation()
      {
        return this.Owner.EnsureColumn( m_column, m_hashCode, this.Index );
      }

      private readonly TColumn m_column;
      private readonly int m_hashCode;
    }

    #endregion

    #region ColumnEntry Private Struct

    [DebuggerDisplay( "Column = {Column}, Next = {Next}" )]
    private struct ColumnEntry
    {
      internal ColumnEntry( TColumn column, int hashCode, int next )
      {
        m_column = column;
        m_hashCode = hashCode;
        m_next = next;
      }

      internal ColumnEntry( int next )
        : this( default( TColumn ), 0, next )
      {
      }

      internal TColumn Column
      {
        get
        {
          return m_column;
        }
      }

      internal int HashCode
      {
        get
        {
          return m_hashCode;
        }
      }

      internal int Next
      {
        get
        {
          return m_next;
        }
      }

      internal ColumnEntry SetNext( int next )
      {
        return new ColumnEntry( m_column, m_hashCode, next );
      }

      private readonly TColumn m_column;
      private readonly int m_hashCode;
      private readonly int m_next;
    }

    #endregion

    #region MarkEntry Private Struct

    [DebuggerDisplay( "Start = {Start}, End = {End}, Splitter = {Splitter}, Orphan = {Orphan}" )]
    private struct MarkEntry
    {
      internal static readonly MarkEntry Default = new MarkEntry( ColumnHierarchyModel<TColumn, TStatus>.None, ColumnHierarchyModel<TColumn, TStatus>.None, ColumnHierarchyModel<TColumn, TStatus>.None, ColumnHierarchyModel<TColumn, TStatus>.None );

      internal MarkEntry( int start, int end, int splitter, int orphan )
      {
        m_start = start;
        m_end = end;
        m_splitter = splitter;
        m_orphan = orphan;
      }

      internal int Start
      {
        get
        {
          return m_start;
        }
      }

      internal int End
      {
        get
        {
          return m_end;
        }
      }

      internal int Splitter
      {
        get
        {
          return m_splitter;
        }
      }

      internal int Orphan
      {
        get
        {
          return m_orphan;
        }
      }

      private readonly int m_start;
      private readonly int m_end;
      private readonly int m_splitter;
      private readonly int m_orphan;
    }

    #endregion

    #region RelationEntry Private Struct

    [DebuggerDisplay( "Parent = {Parent}, FirstChild = {FirstChild}, Previous = {Previous}, Next = {Next}" )]
    private struct RelationEntry
    {
      internal static readonly RelationEntry Default = new RelationEntry( ColumnHierarchyModel<TColumn, TStatus>.None, ColumnHierarchyModel<TColumn, TStatus>.None );

      internal RelationEntry( int parent, int child, int previous, int next )
      {
        m_previous = previous;
        m_next = next;
        m_parent = parent;
        m_child = child;
      }

      internal RelationEntry( int child, int previous, int next )
        : this( ColumnHierarchyModel<TColumn, TStatus>.None, child, previous, next )
      {
      }

      internal RelationEntry( int previous, int next )
        : this( ColumnHierarchyModel<TColumn, TStatus>.None, previous, next )
      {
      }

      internal int Parent
      {
        get
        {
          return m_parent;
        }
      }

      internal int FirstChild
      {
        get
        {
          return m_child;
        }
      }

      internal int Previous
      {
        get
        {
          return m_previous;
        }
      }

      internal int Next
      {
        get
        {
          return m_next;
        }
      }

      internal RelationEntry SetParent( int parent )
      {
        return new RelationEntry( parent, m_child, m_previous, m_next );
      }

      internal RelationEntry SetFirstChild( int child )
      {
        return new RelationEntry( m_parent, child, m_previous, m_next );
      }

      internal RelationEntry SetPrevious( int previous )
      {
        return new RelationEntry( m_parent, m_child, previous, m_next );
      }

      internal RelationEntry SetNext( int next )
      {
        return new RelationEntry( m_parent, m_child, m_previous, next );
      }

      private readonly int m_previous;
      private readonly int m_next;
      private readonly int m_parent;
      private readonly int m_child;
    }

    #endregion
  }
}
