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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid.Views
{
  internal abstract class TableViewColumnVirtualizationManagerBase : ColumnVirtualizationManager
  {
    protected TableViewColumnVirtualizationManagerBase( DataGridContext dataGridContext )
      : base( dataGridContext )
    {
    }

    #region VirtualizationMode Property

    protected static readonly DependencyProperty VirtualizationModeProperty = DependencyProperty.Register(
      "VirtualizationMode",
      typeof( ColumnVirtualizationMode ),
      typeof( TableViewColumnVirtualizationManagerBase ),
      new UIPropertyMetadata( ColumnVirtualizationMode.Recycling, new PropertyChangedCallback( TableViewColumnVirtualizationManagerBase.OnVirtualizationModeChanged ) ) );

    public ColumnVirtualizationMode VirtualizationMode
    {
      get
      {
        return ( ColumnVirtualizationMode )this.GetValue( TableViewColumnVirtualizationManagerBase.VirtualizationModeProperty );
      }
      set
      {
        this.SetValue( TableViewColumnVirtualizationManagerBase.VirtualizationModeProperty, value );
      }
    }

    protected virtual void OnVirtualizationModeChanged( ColumnVirtualizationMode oldValue, ColumnVirtualizationMode newValue )
    {
      this.OnVirtualizingCellCollectionUpdateRequired(
             new VirtualizingCellCollectionUpdateRequiredEventArgs( VirtualizingCellCollectionUpdateTriggeredAction.VirtualizationModeChanged ) );
    }

    private static void OnVirtualizationModeChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var manager = sender as TableViewColumnVirtualizationManagerBase;
      if( manager == null )
        return;

      manager.OnVirtualizationModeChanged( ( ColumnVirtualizationMode )e.OldValue, ( ColumnVirtualizationMode )e.NewValue );
    }

    #endregion

    #region FirstColumnCompensationOffset Property

    // This field is set by the FixedCellPanel in OnCompensationOffsetChanged when the AttachedPropertyChanges
    protected static readonly DependencyProperty FirstColumnCompensationOffsetProperty = DependencyProperty.Register(
      "FirstColumnCompensationOffset",
      typeof( double ),
      typeof( TableViewColumnVirtualizationManagerBase ),
      new FrameworkPropertyMetadata(
        0d,
        new PropertyChangedCallback( TableViewColumnVirtualizationManagerBase.OnFirstColumnCompensationOffsetChanged ) ) );

    public double FirstColumnCompensationOffset
    {
      get
      {
        return ( double )this.GetValue( TableViewColumnVirtualizationManagerBase.FirstColumnCompensationOffsetProperty );
      }
      set
      {
        this.SetValue( TableViewColumnVirtualizationManagerBase.FirstColumnCompensationOffsetProperty, value );
      }
    }

    protected virtual void OnFirstColumnCompensationOffsetChanged( double oldValue, double newValue )
    {
      this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.Unspecified ) );
    }

    private static void OnFirstColumnCompensationOffsetChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var manager = sender as TableViewColumnVirtualizationManagerBase;
      if( manager == null )
        return;

      manager.OnFirstColumnCompensationOffsetChanged( ( double )e.OldValue, ( double )e.NewValue );
    }

    #endregion

    #region FixedColumnCount Protected Property

    protected static readonly DependencyProperty FixedColumnCountProperty = DependencyProperty.Register(
      "FixedColumnCount",
      typeof( int ),
      typeof( TableViewColumnVirtualizationManagerBase ),
      new UIPropertyMetadata( 0, new PropertyChangedCallback( TableViewColumnVirtualizationManagerBase.OnFixedColumnCountChanged ),
                                 new CoerceValueCallback( TableViewColumnVirtualizationManagerBase.CoerceFixedColumnCount ) ) );

    // Only used to back the dp.  The dp itself is used as the API for the user to change the fixed column count (through TableView binding),
    // and the PropertyChangedCallback redirect the value to the top most manager row.
    protected int FixedColumnCount
    {
      get
      {
        return ( int )this.GetValue( TableViewColumnVirtualizationManagerBase.FixedColumnCountProperty );
      }
      set
      {
        this.SetValue( TableViewColumnVirtualizationManagerBase.FixedColumnCountProperty, value );
      }
    }

    private static void OnFixedColumnCountChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as TableViewColumnVirtualizationManagerBase;
      if( self == null )
        return;

      self.OnFixedColumnCountChanged( ( int )e.OldValue, ( int )e.NewValue );
    }

    private static object CoerceFixedColumnCount( DependencyObject sender, object value )
    {
      var self = sender as TableViewColumnVirtualizationManagerBase;
      var count = ( int )value;

      if( ( count == 0 ) || ( self == null ) )
        return value;

      var dataGridContext = self.DataGridContext;
      if( dataGridContext == null )
        return value;

      if( count < 0 )
        return 0;

      var columns = dataGridContext.VisibleColumns;
      var max = columns.Count;

      if( count > max )
        return max;

      return value;
    }

    private void OnFixedColumnCountChanged( int oldValue, int newValue )
    {
      var dataGridContext = this.DataGridContext;
      if( dataGridContext == null )
        return;

      var columnManager = dataGridContext.ColumnManager;
      columnManager.SetFixedColumnCount( newValue );
    }

    #endregion

    #region FixedColumnsWidth Internal Property

    internal abstract double FixedColumnsWidth
    {
      get;
    }

    protected abstract void SetFixedColumnsWidth( double value );

    #endregion

    #region ScrollingColumnsWidth Internal Property

    // The width of all visible columns in 
    internal abstract double ScrollingColumnsWidth
    {
      get;
    }

    protected abstract void SetScrollingColumnsWidth( double value );

    #endregion

    #region VisibleColumnsWidth Internal Property

    // The width of all visible columns (fixed and scrolling ones)
    internal abstract double VisibleColumnsWidth
    {
      get;
    }

    protected abstract void SetVisibleColumnsWidth( double value );

    #endregion

    #region FieldNameToOffset Internal Property

    internal abstract IColumnInfoCollection<double> FieldNameToOffset
    {
      get;
    }

    #endregion

    #region FieldNameToWidth Internal Property

    internal abstract IColumnInfoCollection<double> FieldNameToWidth
    {
      get;
    }

    #endregion

    #region FieldNameToPosition Internal Property

    internal abstract IColumnInfoCollection<int> FieldNameToPosition
    {
      get;
    }

    #endregion

    #region MergedNameToOffset Internal Property

    internal abstract IList<IColumnInfoCollection<double>> MergedNameToOffset
    {
      get;
    }

    #endregion

    #region MergedNameToWidth Internal Property

    internal abstract IList<IColumnInfoCollection<double>> MergedNameToWidth
    {
      get;
    }

    #endregion

    #region MergedNameToPostion Internal Property

    internal abstract IList<IColumnInfoCollection<int>> MergedNameToPostion
    {
      get;
    }

    #endregion

    #region FixedFieldNames Internal Property

    internal abstract IColumnNameCollection FixedFieldNames
    {
      get;
    }

    #endregion

    #region ScrollingFieldNames Internal Property

    internal abstract IColumnNameCollection ScrollingFieldNames
    {
      get;
    }

    #endregion

    #region VisibleFieldNames Internal Property

    internal IColumnNameCollection VisibleFieldNames
    {
      get
      {
        return new ColumnNameLookup( new IColumnNameCollection[] { this.FixedFieldNames, this.ScrollingFieldNames } );
      }
    }

    #endregion

    #region FixedMergedNames Internal Property

    internal abstract IList<IColumnNameCollection> FixedMergedNames
    {
      get;
    }

    #endregion

    #region ScrollingMergedNames Internal Property

    internal abstract IList<IColumnNameCollection> ScrollingMergedNames
    {
      get;
    }

    #endregion

    #region VisibleMergedFieldNames Internal Property

    internal IList<IColumnNameCollection> VisibleMergedFieldNames
    {
      get
      {
        var fixedFieldNames = this.FixedMergedNames;
        var scrollingFieldNames = this.ScrollingMergedNames;

        Debug.Assert( fixedFieldNames != null );
        Debug.Assert( scrollingFieldNames != null );
        Debug.Assert( fixedFieldNames.Count == scrollingFieldNames.Count );

        var count = fixedFieldNames.Count;
        var retval = new List<IColumnNameCollection>( count );

        for( int i = 0; i < count; i++ )
        {
          retval.Add( new ColumnNameLookup( new IColumnNameCollection[] { fixedFieldNames[ i ], scrollingFieldNames[ i ] } ) );
        }

        return retval.AsReadOnly();
      }
    }

    #endregion

    #region ScrollViewer Private Property

    private ScrollViewer ScrollViewer
    {
      get
      {
        return m_scrollViewer;
      }
      set
      {
        if( value == m_scrollViewer )
          return;

        if( m_scrollViewer != null )
        {
          m_scrollViewer.ScrollChanged -= new ScrollChangedEventHandler( this.ScrollViewer_ScrollChanged );
        }

        m_scrollViewer = value;

        if( m_scrollViewer != null )
        {
          m_scrollViewer.ScrollChanged += new ScrollChangedEventHandler( this.ScrollViewer_ScrollChanged );
        }
      }
    }

    private void ScrollViewer_ScrollChanged( object sender, ScrollChangedEventArgs e )
    {
      //If only scrolling vertically, there is no changes to columns in view, thus no need to update this manager, and therefore no need to make FixedCellPanels update.
      if( e.VerticalChange != 0 )
      {
        if( ( e.ExtentHeightChange == 0 ) && ( e.ExtentWidthChange == 0 ) && ( e.HorizontalChange == 0 ) &&
            ( e.ViewportHeightChange == 0 ) && ( e.ViewportWidthChange == 0 ) )
          return;
      }

      var dataGridContext = this.DataGridContext;
      if( dataGridContext == null )
        return;

      var dataGridControl = dataGridContext.DataGridControl;
      if( dataGridControl == null )
        return;

      if( e.OriginalSource != dataGridControl.ScrollViewer )
        return;

      this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ScrollViewerChanged ) );
    }

    private void UpdateScrollViewer()
    {
      var dataGridContext = this.DataGridContext;

      this.ScrollViewer = ( dataGridContext != null ) ? dataGridContext.DataGridControl.ScrollViewer : null;
    }

    private ScrollViewer m_scrollViewer; //null;

    #endregion

    #region UpdateMeasureRequired Public Event

    // This event will be use to notify every listener that the DataGridControl's layout has changed and they must at least call MeasureOverride
    public event EventHandler UpdateMeasureRequired;

    protected void OnUpdateMeasureRequired( UpdateMeasureRequiredEventArgs e )
    {
      // If the manager is detached, it means it will be soon collected, thus don't send update notifications to FixedCellPanels that can still be hook to it (but being recycled).
      if( this.DataGridContext == null )
        return;

      var handler = this.UpdateMeasureRequired;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    #region VirtualizingCellCollectionUpdateRequired Public Event

    // This event is raised so FixedCellPanels can update their cell collections accordingly to changes in columns in the ColumnVirtualizingMode they are currently in.
    public event EventHandler VirtualizingCellCollectionUpdateRequired;

    protected void OnVirtualizingCellCollectionUpdateRequired( VirtualizingCellCollectionUpdateRequiredEventArgs e )
    {
      // If the manager is detached, it means it will be soon collected, thus don't send update notifications to FixedCellPanels that can still be hook to it (but being recycled).
      if( this.DataGridContext == null )
        return;

      var handler = this.VirtualizingCellCollectionUpdateRequired;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    protected override void Initialize()
    {
      base.Initialize();

      CollectionChangedEventManager.AddListener( this.DataGridContext.Items.SortDescriptions, this );
      CollectionChangedEventManager.AddListener( this.DataGridContext.Items.GroupDescriptions, this );
      ColumnActualWidthEventManager.AddListener( this.DataGridContext.Columns, this );
      DataGridControlTemplateChangedEventManager.AddListener( this.DataGridContext.DataGridControl, this );

      this.UpdateScrollViewer();

      var fixedColumnCountBindingBase = new Binding( TableView.FixedColumnCountProperty.Name );
      fixedColumnCountBindingBase.Source = this.DataGridContext;
      fixedColumnCountBindingBase.Mode = BindingMode.TwoWay;
      BindingOperations.SetBinding( this, TableViewColumnVirtualizationManagerBase.FixedColumnCountProperty, fixedColumnCountBindingBase );

      var compensationOffsetBinding = new Binding();
      compensationOffsetBinding.Path = new PropertyPath( TableView.CompensationOffsetProperty );
      compensationOffsetBinding.Source = this.DataGridContext;
      compensationOffsetBinding.Mode = BindingMode.OneWay;
      BindingOperations.SetBinding( this, TableViewColumnVirtualizationManagerBase.FirstColumnCompensationOffsetProperty, compensationOffsetBinding );

      var columnVirtualizationModeBindingBase = new Binding();
      columnVirtualizationModeBindingBase.Source = this.DataGridContext;
      columnVirtualizationModeBindingBase.Path = new PropertyPath( TableView.ColumnVirtualizationModeProperty.Name );
      columnVirtualizationModeBindingBase.Mode = BindingMode.OneWay;
      BindingOperations.SetBinding( this, TableViewColumnVirtualizationManagerBase.VirtualizationModeProperty, columnVirtualizationModeBindingBase );
    }

    protected override void Uninitialize()
    {
      BindingOperations.ClearBinding( this, TableViewColumnVirtualizationManagerBase.VirtualizationModeProperty );
      BindingOperations.ClearBinding( this, TableViewColumnVirtualizationManagerBase.FirstColumnCompensationOffsetProperty );
      BindingOperations.ClearBinding( this, TableViewColumnVirtualizationManagerBase.FixedColumnCountProperty );

      this.ScrollViewer = null;

      CollectionChangedEventManager.RemoveListener( this.DataGridContext.Items.SortDescriptions, this );
      CollectionChangedEventManager.RemoveListener( this.DataGridContext.Items.GroupDescriptions, this );
      ColumnActualWidthEventManager.RemoveListener( this.DataGridContext.Columns, this );
      DataGridControlTemplateChangedEventManager.RemoveListener( this.DataGridContext.DataGridControl, this );

      base.Uninitialize();
    }

    protected override void ResetInternalState()
    {
      this.SetFixedColumnsWidth( 0d );
      this.SetScrollingColumnsWidth( 0d );
      this.SetVisibleColumnsWidth( 0d );

      this.FieldNameToOffset.Clear();
      this.FieldNameToWidth.Clear();
      this.FieldNameToPosition.Clear();
      this.MergedNameToOffset.Clear();
      this.MergedNameToPostion.Clear();
      this.MergedNameToWidth.Clear();

      this.FixedFieldNames.Clear();
      this.ScrollingFieldNames.Clear();
      this.FixedMergedNames.Clear();
      this.ScrollingMergedNames.Clear();
    }

    protected override void DoUpdate()
    {
      var dataGridContext = this.DataGridContext;
      Debug.Assert( dataGridContext != null );

      var columnManager = dataGridContext.ColumnManager;
      Debug.Assert( columnManager != null );

      // Make sure the fixed column count is up-to-date.
      this.CoerceValue( TableViewColumnVirtualizationManagerBase.FixedColumnCountProperty );

      //It can happen that the columnManager has not been initialized yet (e.g. when creating a grid in code behind), so make sure markers are not null.
      var levelMarkers = columnManager.GetLevelMarkersFor( dataGridContext.Columns );
      if( levelMarkers == null )
        return;

      var currentLocation = levelMarkers.Start;
      Debug.Assert( currentLocation != null );
      Debug.Assert( currentLocation.Type == LocationType.Start );

      var columnPosition = 0;
      var visibleColumnsTotalWidth = 0d;

      // Fill the data structures for the fixed columns.
      while( currentLocation.Type != LocationType.Splitter )
      {
        var columnLocation = currentLocation as ColumnHierarchyManager.IColumnLocation;
        if( columnLocation != null )
        {
          var column = columnLocation.Column;
          Debug.Assert( column != null );

          // Ensure we keep the correct visible offset for all columns even if they are collpased
          this.FieldNameToOffset[ column ] = visibleColumnsTotalWidth;

          if( column.Visible )
          {
            var width = column.ActualWidth;

            this.FixedFieldNames.Add( column );
            this.FieldNameToPosition[ column ] = columnPosition;
            this.FieldNameToWidth[ column ] = width;

            visibleColumnsTotalWidth += width;
          }

          columnPosition++;
        }
        else
        {
          Debug.Assert( currentLocation.Type != LocationType.Column );
        }

        currentLocation = currentLocation.GetNextSiblingOrCousin();
        Debug.Assert( currentLocation != null );
      }

      this.SetFixedColumnsWidth( visibleColumnsTotalWidth );

      var horizontalOffset = this.GetHorizontalOffset();
      var viewportWidth = this.GetViewportWidth();

      if( this.VirtualizationMode != ColumnVirtualizationMode.None )
      {
        // We increment the horizontalOffset to take the fixed columns width into consideration.
        horizontalOffset += visibleColumnsTotalWidth;
        viewportWidth = Math.Max( 0d, viewportWidth - visibleColumnsTotalWidth );
      }

      // We must consider the fixed columns width when calculating visible indexes in viewport
      var viewportMaximumOffset = horizontalOffset + viewportWidth;
      var visibleScrollingColumnsTotalWidth = 0d;

      // Fill the data structures for the scrolling columns.
      while( currentLocation.Type != LocationType.Orphan )
      {
        var columnLocation = currentLocation as ColumnHierarchyManager.IColumnLocation;
        if( columnLocation != null )
        {
          var column = columnLocation.Column;
          Debug.Assert( column != null );

          // Ensure we keep the correct visible offset for all columns even if they are collapsed
          this.FieldNameToOffset[ column ] = visibleColumnsTotalWidth;

          if( column.Visible )
          {
            var width = column.ActualWidth;
            var leftEdgeOffset = visibleColumnsTotalWidth;
            var rightEdgeOffset = leftEdgeOffset + width;

            this.FieldNameToPosition[ column ] = columnPosition;
            this.FieldNameToWidth[ column ] = width;

            visibleColumnsTotalWidth += width;
            visibleScrollingColumnsTotalWidth += width;

            if( this.VirtualizationMode != ColumnVirtualizationMode.None )
            {
              // The column is in the viewport.
              if( ( leftEdgeOffset < viewportMaximumOffset ) && ( rightEdgeOffset > horizontalOffset ) )
              {
                this.ScrollingFieldNames.Add( column );
              }
            }
            else
            {
              this.ScrollingFieldNames.Add( column );
            }
          }

          columnPosition++;
        }
        else
        {
          Debug.Assert( currentLocation.Type != LocationType.Column );
        }

        currentLocation = currentLocation.GetNextSiblingOrCousin();
        Debug.Assert( currentLocation != null );
      }

      this.SetScrollingColumnsWidth( visibleScrollingColumnsTotalWidth );
      this.SetVisibleColumnsWidth( visibleColumnsTotalWidth );
    }

    protected override void IncrementVersion( UpdateMeasureRequiredEventArgs e )
    {
      base.IncrementVersion( e );

      this.OnUpdateMeasureRequired( e );
    }

    protected override void OnDataGridContextPropertyChanged( PropertyChangedEventArgs e )
    {
      base.OnDataGridContextPropertyChanged( e );

      Debug.Assert( !string.IsNullOrEmpty( e.PropertyName ) );

      if( string.IsNullOrEmpty( e.PropertyName ) )
        return;

      switch( e.PropertyName )
      {
        case "FixedHeaderFooterViewPortSize":
          this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ViewPortWidthChanged ) );
          break;

        case "CurrentItem":
          // No need to increment version, only notify the current item to invalidate measure
          this.OnUpdateMeasureRequired( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.CurrentItemChanged ) );
          break;
      }
    }

    protected override void OnColumnsLayoutChanging()
    {
      m_columnsVisibilitySnapshot.Clear();

      var dataGridContext = this.DataGridContext;
      if( dataGridContext != null )
      {
        // We consult the VisibleColumns collection to figure if a column was visible or not instead
        // of the ColumnBase.Visible property because the property may already be tainted by a change
        // at this step.
        var visibleColumns = ( ReadOnlyColumnCollection )dataGridContext.VisibleColumns;

        foreach( var column in dataGridContext.Columns )
        {
          // This line of code is equivalent to
          //   m_columnsVisibilitySnapshot[ column ] = visibleColumns.Contains( column );
          // However, the result is found in O(1) instead of O(n).
          m_columnsVisibilitySnapshot[ column ] = ( visibleColumns[ column.FieldName ] == column );
        }
      }

      base.OnColumnsLayoutChanging();
    }

    protected override void OnColumnsLayoutChanged()
    {
      var columnsVisibilitySnapshot = new Dictionary<ColumnBase, bool>( m_columnsVisibilitySnapshot );
      m_columnsVisibilitySnapshot.Clear();

      var dataGridContext = this.DataGridContext;
      if( dataGridContext != null )
      {
        // When details are flatten, the detail DataGridContext's FixedColumnCount property is bound to the 
        // DataGridControl's FixedColumnCount property.  Allowing the value to be changed here would destroy the binding.
        if( !dataGridContext.IsAFlattenDetail )
        {
          var columnManager = dataGridContext.ColumnManager;
          var fixedColumnCount = columnManager.GetFixedColumnCount();

          Debug.Assert( fixedColumnCount >= 0 );

          this.FixedColumnCount = fixedColumnCount;
        }

        this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ColumnReordering ) );

        var columns = dataGridContext.Columns;

        if( this.VirtualizationMode == ColumnVirtualizationMode.Virtualizing )
        {
          // Notify the FixedCellPanel that some cells need to be created for new columns.
          if( ( columnsVisibilitySnapshot.Count < columns.Count ) || ( columns.Any( c => !columnsVisibilitySnapshot.ContainsKey( c ) ) ) )
          {
            this.OnVirtualizingCellCollectionUpdateRequired( new VirtualizingCellCollectionUpdateRequiredEventArgs( VirtualizingCellCollectionUpdateTriggeredAction.VisibleColumnsAdded ) );
          }
        }

        // Figure out the columns that have changed visibility
        if( columnsVisibilitySnapshot.Count > 0 )
        {
          var columnChanges = new List<ColumnBase>();

          foreach( var column in columns )
          {
            bool visible;

            if( columnsVisibilitySnapshot.TryGetValue( column, out visible ) && ( column.Visible != visible ) )
            {
              columnChanges.Add( column );
            }
          }

          if( columnChanges.Count > 0 )
          {
            this.OnVirtualizingCellCollectionUpdateRequired( new VirtualizingCellCollectionUpdateRequiredEventArgs( columnChanges ) );
          }
        }
      }

      base.OnColumnsLayoutChanged();
    }

    protected virtual void OnSortDescriptionsChanged( NotifyCollectionChangedEventArgs e )
    {
      this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.SortingChanged ) );
    }

    protected virtual void OnGroupDescriptionsChanged( NotifyCollectionChangedEventArgs e )
    {
      this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.GroupingChanged ) );
    }

    protected virtual void OnColumnActualWidthChanged( ColumnActualWidthChangedEventArgs e )
    {
      // We pass the delta between the old and new value to tell the Panel to reduce the horizontal offset when a column is auto-resized to a smaller value
      if( e != null )
      {
        this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ColumnActualWidthChanged, e.OldValue - e.NewValue ) );
      }
      else
      {
        this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ColumnActualWidthChanged ) );
      }
    }

    protected virtual void OnDataGridControlTemplateChanged()
    {
      this.UpdateScrollViewer();
      this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.Unspecified ) );
    }

    internal IColumnInfoCollection<double> GetFieldNameToOffset()
    {
      return this.GetFieldNameToOffset( -1 );
    }

    internal IColumnInfoCollection<double> GetFieldNameToOffset( int level )
    {
      if( level < 0 )
        return this.FieldNameToOffset;

      var collection = this.MergedNameToOffset;
      if( collection.Count > 0 )
        return collection[ level ];

      //If the list is empty, it means there is no column in the grid right now, so return an empty collection, as does the FieldNameToOffset property.
      return new ColumnInfoCollection<double>();
    }

    internal IColumnInfoCollection<double> GetFieldNameToWidth()
    {
      return this.GetFieldNameToWidth( -1 );
    }

    internal IColumnInfoCollection<double> GetFieldNameToWidth( int level )
    {
      if( level < 0 )
        return this.FieldNameToWidth;

      var collection = this.MergedNameToWidth;
      if( collection.Count > 0 )
        return collection[ level ];

      //If the list is empty, it means there is no column in the grid right now, so return an empty collection, as does the FieldNameToWidth property.
      return new ColumnInfoCollection<double>();
    }

    internal IColumnInfoCollection<int> GetFieldNameToPosition()
    {
      return this.GetFieldNameToPosition( -1 );
    }

    internal IColumnInfoCollection<int> GetFieldNameToPosition( int level )
    {
      if( level < 0 )
        return this.FieldNameToPosition;

      var collection = this.MergedNameToPostion;
      if( collection.Count > 0 )
        return collection[ level ];

      //If the list is empty, it means there is no column in the grid right now, so return an empty collection, as does the FieldNameToPosition property.
      return new ColumnInfoCollection<int>();
    }

    internal IColumnNameCollection GetFixedFieldNames()
    {
      return this.GetFixedFieldNames( -1 );
    }

    internal IColumnNameCollection GetFixedFieldNames( int level )
    {
      if( level < 0 )
        return this.FixedFieldNames;

      var collection = this.FixedMergedNames;
      if( collection.Count > 0 )
        return collection[ level ];

      //If the list is empty, it means there is no column in the grid right now, so return an empty collection, as does the FixedFieldNames property.
      return new ColumnNameCollection();
    }

    internal IColumnNameCollection GetScrollingFieldNames()
    {
      return this.GetScrollingFieldNames( -1 );
    }

    internal IColumnNameCollection GetScrollingFieldNames( int level )
    {
      if( level < 0 )
        return this.ScrollingFieldNames;

      var collection = this.ScrollingMergedNames;
      if( collection.Count > 0 )
        return collection[ level ];

      //If the list is empty, it means there is no column in the grid right now, so return an empty collection, as does the ScrollingFieldNames property.
      return new ColumnNameCollection();
    }

    internal IColumnNameCollection GetVisibleFieldNames()
    {
      return this.GetVisibleFieldNames( -1 );
    }

    internal IColumnNameCollection GetVisibleFieldNames( int level )
    {
      if( level < 0 )
        return this.VisibleFieldNames;

      var collection = this.VisibleMergedFieldNames;
      if( collection.Count > 0 )
        return collection[ level ];

      //If the list is empty, it means there is no column in the grid right now, so return an empty collection, as does the VisibleFieldNames property.
      return new ColumnNameCollection();
    }

    private string FindFirstVisibleFocusableColumnFieldName( LinkedListNode<ColumnBase> startNode, Func<LinkedListNode<ColumnBase>, LinkedListNode<ColumnBase>> getNextNodeHandler )
    {
      if( getNextNodeHandler == null )
        throw new ArgumentNullException( "getNextNodeHandler" );

      if( startNode == null )
        return null;

      ColumnBase currentColumn = startNode.Value;

      while( ( currentColumn != null )
        && ( ( currentColumn.ReadOnly )
          && ( !currentColumn.CanBeCurrentWhenReadOnly )
          || ( this.FixedFieldNames.Contains( currentColumn.FieldName ) )
          || ( !currentColumn.Visible ) ) )
      {
        startNode = getNextNodeHandler.Invoke( startNode );
        currentColumn = ( startNode != null ) ? startNode.Value : null;
      }

      return ( currentColumn != null ) ? currentColumn.FieldName : null;
    }

    private string GetFirstVisibleFocusableColumnFieldName()
    {
      var dataGridContext = this.DataGridContext;
      if( dataGridContext == null )
        return null;

      return this.FindFirstVisibleFocusableColumnFieldName(
               dataGridContext.ColumnsByVisiblePosition.First,
               ( node ) => node.Next );
    }

    private string GetLastVisibleFocusableColumnFieldName()
    {
      var dataGridContext = this.DataGridContext;
      if( dataGridContext == null )
        return null;

      return this.FindFirstVisibleFocusableColumnFieldName(
               dataGridContext.ColumnsByVisiblePosition.Last,
               ( node ) => node.Previous );
    }

    private double GetHorizontalOffset()
    {
      var scrollViewer = this.ScrollViewer;
      if( scrollViewer == null )
        return 0d;

      return scrollViewer.HorizontalOffset - this.FirstColumnCompensationOffset;
    }

    private double GetViewportWidth()
    {
      // Viewport is only required when UI Virtualization is on.
      if( this.VirtualizationMode == ColumnVirtualizationMode.None )
        return 0d;

      IScrollInfo scrollInfo = null;

      try
      {
        var dataGridContext = this.DataGridContext;
        if( dataGridContext != null )
        {
          scrollInfo = this.DataGridContext.DataGridControl.ItemsHost as IScrollInfo;
        }
      }
      catch( Exception )
      {
        // Ignore exceptions since it may be called before the ItemsHost Template is applied
      }

      double width = ( scrollInfo != null ) ? scrollInfo.ViewportWidth : 0d;

      // We use the HeaderFooterItem width when greater than the viewport width.
      return Math.Max( width, this.DataGridContext.FixedHeaderFooterViewPortSize.Width );
    }

    private IList<ColumnBase> GetVisibleColumns( int level )
    {
      return this.DataGridContext.VisibleColumns;
    }

    private ColumnCollection GetColumnCollectionForLevel( int level )
    {
      var dataGridContext = this.DataGridContext;
      if( dataGridContext == null )
        return null;

      if( level < 0 )
        return dataGridContext.Columns;

      return null;
    }

    private IEnumerable<ColumnHierarchyManager.ILocation> GetChildLocations( ColumnHierarchyManager.ILocation location )
    {
      if( location == null )
        yield break;

      location = location.GetFirstChild();

      while( location != null )
      {
        yield return location;

        location = location.GetNextSibling();
      }
    }

    private IEnumerable<ColumnHierarchyManager.IColumnLocation> GetColumnLocations( IEnumerable<ColumnHierarchyManager.ILocation> locations )
    {
      if( locations == null )
        return Enumerable.Empty<ColumnHierarchyManager.IColumnLocation>();

      return ( from location in locations
               let columnLocation = location as ColumnHierarchyManager.IColumnLocation
               where ( columnLocation != null )
               select columnLocation );
    }

    private double GetVisibleChildColumnsWidth( ColumnHierarchyManager.ILocation parentLocation )
    {
      var width = 0d;

      foreach( var columnLocation in this.GetColumnLocations( this.GetChildLocations( parentLocation ) ) )
      {
        var column = columnLocation.Column;
        if( column.Visible )
        {
          width += column.ActualWidth;
        }
      }

      return width;
    }

    #region IWeakEventListener Members

    protected override bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      var handled = true;
      var dataGridContext = this.DataGridContext;

      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        if( sender == dataGridContext.Items.SortDescriptions )
        {
          this.OnSortDescriptionsChanged( ( NotifyCollectionChangedEventArgs )e );
        }
        else if( sender == dataGridContext.Items.GroupDescriptions )
        {
          this.OnGroupDescriptionsChanged( ( NotifyCollectionChangedEventArgs )e );
        }
      }
      else if( managerType == typeof( ColumnActualWidthEventManager ) )
      {
        this.OnColumnActualWidthChanged( e as ColumnActualWidthChangedEventArgs );
      }
      else if( managerType == typeof( DataGridControlTemplateChangedEventManager ) )
      {
        this.OnDataGridControlTemplateChanged();
      }
      else
      {
        handled = false;
      }

      if( !base.OnReceiveWeakEvent( managerType, sender, e ) )
        return handled;

      return true;
    }

    #endregion

    private readonly Dictionary<ColumnBase, bool> m_columnsVisibilitySnapshot = new Dictionary<ColumnBase, bool>();

    internal sealed class ColumnNameCollection : IColumnNameCollection
    {
      public int Count
      {
        get
        {
          return m_list.Count;
        }
      }

      public string this[ int index ]
      {
        get
        {
          return m_list[ index ];
        }
      }

      public void Clear()
      {
        m_list.Clear();
        m_list.TrimExcess();

        m_lookup.Clear();
        m_lookup.TrimExcess();
      }

      public void Add( string fieldName )
      {
        if( fieldName == null )
          return;

        if( m_lookup.Contains( fieldName ) )
          return;

        m_lookup.Add( fieldName );
        m_list.Add( fieldName );
      }

      public void Add( ColumnBase column )
      {
        if( column == null )
          return;

        this.Add( column.FieldName );
      }

      public void Remove( string fieldName )
      {
        if( fieldName == null )
          return;

        if( !m_lookup.Contains( fieldName ) )
          return;

        m_lookup.Remove( fieldName );
        m_list.Remove( fieldName );
      }

      public void Remove( ColumnBase column )
      {
        if( column == null )
          return;

        this.Remove( column.FieldName );
      }

      public bool Contains( string fieldName )
      {
        if( fieldName == null )
          return false;

        return m_lookup.Contains( fieldName );
      }

      public bool Contains( ColumnBase column )
      {
        if( column == null )
          return false;

        return this.Contains( column.FieldName );
      }

      public IEnumerator<string> GetEnumerator()
      {
        return m_list.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return this.GetEnumerator();
      }

      private readonly List<string> m_list = new List<string>();
      private readonly HashSet<string> m_lookup = new HashSet<string>();
    }

    internal sealed class ColumnInfoCollection<T> : IColumnInfoCollection<T>
    {
      public void Clear()
      {
        m_data.Clear();
      }

      public T this[ string fieldName ]
      {
        get
        {
          return m_data[ fieldName ];
        }
        set
        {
          if( fieldName == null )
            return;

          m_data[ fieldName ] = value;
        }
      }

      public T this[ ColumnBase column ]
      {
        get
        {
          return this[ column.FieldName ];
        }
        set
        {
          if( column == null )
            return;

          this[ column.FieldName ] = value;
        }
      }

      public void Reset( string fieldName )
      {
        if( fieldName == null )
          return;

        m_data.Remove( fieldName );
      }

      public void Reset( ColumnBase column )
      {
        if( column == null )
          return;

        this.Reset( column.FieldName );
      }

      public bool Contains( string fieldName )
      {
        if( fieldName == null )
          return false;

        return m_data.ContainsKey( fieldName );
      }

      public bool Contains( ColumnBase column )
      {
        if( column == null )
          return false;

        return this.Contains( column.FieldName );
      }

      public bool TryGetValue( string fieldName, out T value )
      {
        if( fieldName != null )
          return m_data.TryGetValue( fieldName, out value );

        value = default( T );
        return false;
      }

      public bool TryGetValue( ColumnBase column, out T value )
      {
        if( column != null )
          return this.TryGetValue( column.FieldName, out value );

        value = default( T );
        return false;
      }

      private readonly Dictionary<string, T> m_data = new Dictionary<string, T>();
    }

    private sealed class ColumnNameLookup : IColumnNameCollection
    {
      internal ColumnNameLookup( IEnumerable<IColumnNameCollection> collection )
      {
        m_collection = ( collection != null ) ? collection.ToList() : new List<IColumnNameCollection>( 0 );
      }

      public int Count
      {
        get
        {
          return m_collection.Sum( item => item.Count );
        }
      }

      public string this[ int index ]
      {
        get
        {
          foreach( var collection in m_collection )
          {
            int count = collection.Count;

            if( index < count )
              return collection[ index ];

            index -= count;
          }

          throw new ArgumentOutOfRangeException( "index" );
        }
      }

      public void Clear()
      {
        throw new NotSupportedException();
      }

      public void Add( string fieldName )
      {
        throw new NotSupportedException();
      }

      public void Add( ColumnBase column )
      {
        throw new NotSupportedException();
      }

      public void Remove( string fieldName )
      {
        throw new NotSupportedException();
      }

      public void Remove( ColumnBase column )
      {
        throw new NotSupportedException();
      }

      public bool Contains( string fieldName )
      {
        if( fieldName == null )
          return false;

        return ( from list in m_collection
                 where list.Contains( fieldName )
                 select true ).Any();
      }

      public bool Contains( ColumnBase column )
      {
        if( column == null )
          return false;

        return this.Contains( column.FieldName );
      }

      public IEnumerator<string> GetEnumerator()
      {
        if( m_collection.Count == 0 )
          return Enumerable.Empty<string>().GetEnumerator();

        IEnumerable<string> result = m_collection[ 0 ];
        foreach( var more in m_collection.Skip( 1 ) )
        {
          result = result.Concat( more );
        }

        return result.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return this.GetEnumerator();
      }

      private readonly List<IColumnNameCollection> m_collection;
    }
  }
}
