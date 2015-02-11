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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid.Views
{
  internal abstract class TableViewColumnVirtualizationManagerBase : ColumnVirtualizationManager
  {
    protected TableViewColumnVirtualizationManagerBase( DataGridContext dataGridContext )
      : base( dataGridContext )
    {
      RoutedEventManager.Instance.Register();
    }

    #region IsVirtualizing Property

    protected static readonly DependencyProperty IsVirtualizingProperty = DependencyProperty.Register(
      "IsVirtualizing",
      typeof( bool ),
      typeof( TableViewColumnVirtualizationManagerBase ),
      new UIPropertyMetadata(
        true,
        new PropertyChangedCallback( TableViewColumnVirtualizationManagerBase.OnIsVirtualizingChanged ) ) );

    public bool IsVirtualizing
    {
      get
      {
        return ( bool )this.GetValue( TableViewColumnVirtualizationManagerBase.IsVirtualizingProperty );
      }
      set
      {
        this.SetValue( TableViewColumnVirtualizationManagerBase.IsVirtualizingProperty, value );
      }
    }

    protected virtual void OnIsVirtualizingChanged( bool oldValue, bool newValue )
    {
      this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.VirtualizationStateChanged ) );
    }

    private static void OnIsVirtualizingChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var manager = sender as TableViewColumnVirtualizationManagerBase;
      if( manager == null )
        return;

      manager.OnIsVirtualizingChanged( ( bool )e.OldValue, ( bool )e.NewValue );
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
                                 new CoerceValueCallback( TableViewColumnVirtualizationManagerBase.FixedColumnCountCoerceCallback ) ) );

    // Only used to back the dp.  The dp itself is used as the API for the user to change the fixed column count (through TableView binding),
    // and the PropertyChangedCallback redirect the value to the top most manager row.  FixedColumnCountInternal reflects the value for the regular CMR (-1 level).
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

    private void OnFixedColumnCountChanged( int oldValue, int newValue )
    {
      // To prevent reentrance
      if( !m_updatingFixedColumnCount.IsSet )
      {
        using( m_updatingFixedColumnCount.Set() )
        {
          int level = -1;
          int correctionValue = ( newValue > 0 ) ? 1 : 0;

          this.SetFixedColumnCount( level, newValue, correctionValue );
        }
      }

      // We want to be sure the cells won't be inserted in ScrollingPanel, but added to it
      if( m_incrementVersionDispatcherOperation == null )
      {
        m_incrementVersionDispatcherOperation = this.Dispatcher.BeginInvoke(
                                                  DispatcherPriority.Render,
                                                  new Action<UpdateMeasureRequiredEventArgs>( this.IncrementVersion ),
                                                  new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ColumnReordering ) );
      }
    }

    private static void OnFixedColumnCountChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var manager = ( TableViewColumnVirtualizationManagerBase )sender;

      manager.OnFixedColumnCountChanged( ( int )e.OldValue, ( int )e.NewValue );
    }

    private static object FixedColumnCountCoerceCallback( DependencyObject sender, object value )
    {
      var fixedColumnCount = ( int )value;
      if( fixedColumnCount == 0 )
        return value;

      if( fixedColumnCount < 0 )
        return 0;

      var manager = ( TableViewColumnVirtualizationManagerBase )sender;
      var dataGridContext = manager.DataGridContext;
      int maxValue = 0;

      if( dataGridContext != null )
      {
        var columns = dataGridContext.VisibleColumns;
        if( columns.Any() )
        {
          maxValue = columns.Count;
        }
      }

      if( fixedColumnCount > maxValue )
        return maxValue;

      return value;
    }

    private int FixedColumnCountInternal
    {
      get;
      set;
    }

    private void UpdateFixedColumnCount()
    {
      this.CoerceValue( TableViewColumnVirtualizationManagerBase.FixedColumnCountProperty );
    }

    private readonly AutoResetFlag m_updatingFixedColumnCount = AutoResetFlagFactory.Create();
    private DispatcherOperation m_incrementVersionDispatcherOperation;

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

    #region FirstViewportColumnFieldNameIndex Internal Property

    internal abstract int FirstViewportColumnFieldNameIndex
    {
      get;
    }

    protected abstract void SetFirstViewportColumnFieldNameIndex( int value );

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
      DataGridContext dataGridContext = this.DataGridContext;

      if( dataGridContext == null )
      {
        this.ScrollViewer = null;
      }
      else
      {
        var scrollViewer = dataGridContext.DataGridControl.ScrollViewer;

        Debug.Assert( ( scrollViewer != null ) || ( DesignerProperties.GetIsInDesignMode( this ) ) );

        this.ScrollViewer = scrollViewer;
      }
    }

    private ScrollViewer m_scrollViewer; //null;

    #endregion

    #region UpdateMeasureRequired Public Event

    // This event will be use to notify every listener that the DataGridControl's layout has changed and they must at least call MeasureOverride
    public event EventHandler UpdateMeasureRequired;

    protected void RaiseUpdateMeasureRequired( UpdateMeasureRequiredEventArgs args )
    {
      if( this.UpdateMeasureRequired != null )
      {
        this.UpdateMeasureRequired( this, args );
      }
    }

    #endregion

    protected override void Initialize()
    {
      base.Initialize();

      CollectionChangedEventManager.AddListener( this.DataGridContext.Items.SortDescriptions, this );
      CollectionChangedEventManager.AddListener( this.DataGridContext.Items.GroupDescriptions, this );
      ColumnActualWidthEventManager.AddListener( this.DataGridContext.Columns, this );
      DataGridControlTemplateChangedEventManager.AddListener( this.DataGridContext.DataGridControl, this );
      if( this.DataGridContext.SourceDetailConfiguration != null )
      {
        FixedColumnCountInfoEventManager.AddListener( this.DataGridContext.SourceDetailConfiguration, this );
      }

      this.UpdateScrollViewer();

      Binding fixedColumnCountBindingBase = new Binding( "FixedColumnCount" );
      fixedColumnCountBindingBase.Source = this.DataGridContext;
      fixedColumnCountBindingBase.Mode = BindingMode.TwoWay;
      BindingOperations.SetBinding( this, TableViewColumnVirtualizationManagerBase.FixedColumnCountProperty, fixedColumnCountBindingBase );

      Binding isColumnVirtualizingEnabledBindingBase = new Binding();
      isColumnVirtualizingEnabledBindingBase.Source = this.DataGridContext;
      isColumnVirtualizingEnabledBindingBase.Path = new PropertyPath( TableView.IsColumnVirtualizationEnabledProperty );
      isColumnVirtualizingEnabledBindingBase.Mode = BindingMode.OneWay;
      BindingOperations.SetBinding( this, TableViewColumnVirtualizationManagerBase.IsVirtualizingProperty, isColumnVirtualizingEnabledBindingBase );

      Binding compensationOffsetBinding = new Binding();
      compensationOffsetBinding.Path = new PropertyPath( TableView.CompensationOffsetProperty );
      compensationOffsetBinding.Source = this.DataGridContext;
      compensationOffsetBinding.Mode = BindingMode.OneWay;
      BindingOperations.SetBinding( this, TableViewColumnVirtualizationManagerBase.FirstColumnCompensationOffsetProperty, compensationOffsetBinding );
    }

    protected override void Uninitialize()
    {
      BindingOperations.ClearBinding( this, TableViewColumnVirtualizationManagerBase.FixedColumnCountProperty );

      BindingOperations.ClearBinding( this, TableViewColumnVirtualizationManagerBase.IsVirtualizingProperty );
      BindingOperations.ClearBinding( this, TableViewColumnVirtualizationManagerBase.FirstColumnCompensationOffsetProperty );

      this.ScrollViewer = null;

      CollectionChangedEventManager.RemoveListener( this.DataGridContext.Items.SortDescriptions, this );
      CollectionChangedEventManager.RemoveListener( this.DataGridContext.Items.GroupDescriptions, this );
      ColumnActualWidthEventManager.RemoveListener( this.DataGridContext.Columns, this );
      DataGridControlTemplateChangedEventManager.RemoveListener( this.DataGridContext.DataGridControl, this );

      if( this.DataGridContext.SourceDetailConfiguration != null )
      {
        FixedColumnCountInfoEventManager.RemoveListener( this.DataGridContext.SourceDetailConfiguration, this );
      }

      base.Uninitialize();
    }

    protected override void ResetInternalState()
    {
      this.SetFixedColumnsWidth( 0d );
      this.SetScrollingColumnsWidth( 0d );
      this.SetVisibleColumnsWidth( 0d );
      this.SetFirstViewportColumnFieldNameIndex( -1 );

      this.FieldNameToOffset.Clear();
      this.FieldNameToWidth.Clear();
      this.FieldNameToPosition.Clear();

      this.FixedFieldNames.Clear();
      this.ScrollingFieldNames.Clear();
    }

    protected override void DoUpdate()
    {
      this.UpdateFixedColumnCount();

      var dataGridContext = this.DataGridContext;
      var columnsByVisiblePosition = dataGridContext.ColumnsByVisiblePosition;

      // We can't compute any values if Colums are not yet added to DataGrid
      if( columnsByVisiblePosition.Count == 0 )
        return;

      LinkedListNode<ColumnBase> currentColumnNode = columnsByVisiblePosition.First;
      int currentColumnIndex = 0;
      double currentTotalWidth = 0d;
      double currentColumnActualWidth = 0d;

      int currentFixedColumnCount = 0;
      ColumnBase currentColumnDataGridContext = dataGridContext.CurrentColumn;

      double fixedFieldNamesWidth = 0d;
      double fixedColumnsWidth = 0d;

      if( this.FixedColumnCountInternal > 0 )
      {
        while( currentColumnNode != null )
        {
          // If all Fixed Columns were found
          if( this.FixedColumnCountInternal == currentFixedColumnCount )
            break;

          ColumnBase column = currentColumnNode.Value;

          // Ensure we keep the correct visible offset for every columns even if they are Collpased
          this.FieldNameToOffset[ column ] = currentTotalWidth;

          // The Column is not visible
          if( !column.Visible )
          {
            // Move next
            currentColumnNode = currentColumnNode.Next;
            currentColumnIndex++;
          }
          else
          {
            this.FieldNameToPosition[ column ] = currentColumnIndex;

            currentColumnActualWidth = column.ActualWidth;

            fixedFieldNamesWidth += currentColumnActualWidth;
            currentTotalWidth += currentColumnActualWidth;

            this.FieldNameToWidth[ column ] = currentColumnActualWidth;

            // We found a FixedCell
            if( this.FixedColumnCountInternal > currentFixedColumnCount )
            {
              fixedColumnsWidth += currentColumnActualWidth;

              this.FixedFieldNames.Add( column );

              currentColumnNode = currentColumnNode.Next;
              currentColumnIndex++;
              currentFixedColumnCount++;
            }
            else
            {
              Debug.Assert( this.FixedColumnCount == currentFixedColumnCount );
            }
          }
        }
      }

      this.SetFixedColumnsWidth( fixedColumnsWidth );

      double horizontalOffset = this.GetHorizontalOffset();
      double viewportWidth = this.GetViewportWidth();

      if( this.IsVirtualizing )
      {
        // We increment the horizontalOffset to take the fixed columns width into consideration.
        horizontalOffset += fixedFieldNamesWidth;
        viewportWidth = Math.Max( 0d, viewportWidth - fixedFieldNamesWidth );
      }

      // We must consider the fixedFieldNamesWidth width when calculating visible indexes in Viewport
      double viewportMaximumOffset = horizontalOffset + viewportWidth;
      double scrollingColumnsWidth = 0d;

      while( currentColumnNode != null )
      {
        ColumnBase column = currentColumnNode.Value;

        // Ensure we keep the correct visible offset for every columns even if they are collapsed
        this.FieldNameToOffset[ column ] = currentTotalWidth;

        if( column.Visible )
        {
          bool visibleColumnFieldNameMustBeAdded = false;

          this.FieldNameToPosition[ column ] = currentColumnIndex;
          currentColumnActualWidth = column.ActualWidth;

          double columnLeftEdgeOffset = currentTotalWidth;
          double columnRightEdgeOffset = columnLeftEdgeOffset + currentColumnActualWidth;

          currentTotalWidth += currentColumnActualWidth;
          scrollingColumnsWidth += currentColumnActualWidth;

          this.FieldNameToWidth[ column ] = currentColumnActualWidth;

          if( this.IsVirtualizing )
          {
            // The cell is before the ViewPort
            // Cell .... | --- ViewPort --- |
            bool beforeViewPort = ( horizontalOffset >= columnRightEdgeOffset );

            // The cell is after the ViewPort
            // | --- ViewPort --- | .... Cell
            bool afterViewPort = ( viewportMaximumOffset <= columnLeftEdgeOffset );

            // Columns are in the ViewPort
            if( !beforeViewPort && !afterViewPort )
            {
              // We keep the index of the first viewport column index to ease the FixedColumn splitter change
              if( this.FirstViewportColumnFieldNameIndex == -1 )
              {
                this.SetFirstViewportColumnFieldNameIndex( this.ScrollingFieldNames.Count );
              }

              visibleColumnFieldNameMustBeAdded = true;
            }
          }
          else
          {
            visibleColumnFieldNameMustBeAdded = true;
          }

          if( visibleColumnFieldNameMustBeAdded && ( !this.FixedFieldNames.Contains( column ) ) && ( !this.ScrollingFieldNames.Contains( column ) ) )
          {
            this.ScrollingFieldNames.Add( column );
          }
        }

        currentColumnNode = currentColumnNode.Next;
        currentColumnIndex++;
      }

      this.SetScrollingColumnsWidth( scrollingColumnsWidth );
      this.SetVisibleColumnsWidth( currentTotalWidth );
    }

    protected override void IncrementVersion( object parameters )
    {
      // If the manager is detached, it means it will be soon collected, thus don't send update notifications to FixedCellPanels that can still be hook to it (but being recycled).
      if( !m_detached )
      {
        base.IncrementVersion( parameters );
        this.RaiseUpdateMeasureRequired( parameters as UpdateMeasureRequiredEventArgs );
      }

      m_incrementVersionDispatcherOperation = null;
    }

    protected override void DataGridContext_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      base.DataGridContext_PropertyChanged( sender, e );

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
          this.RaiseUpdateMeasureRequired( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.CurrentItemChanged ) );
          break;
      }
    }

    internal int GetFixedColumnCount()
    {
      return this.GetFixedColumnCount( -1 );
    }

    internal int GetFixedColumnCount( int level )
    {
      if( level < 0 )
        return this.FixedColumnCountInternal;

      //If the list is empty, it means there is no column in the grid right now, so return 0.
      return 0;
    }

    internal IColumnInfoCollection<double> GetFieldNameToOffset()
    {
      return this.GetFieldNameToOffset( -1 );
    }

    internal IColumnInfoCollection<double> GetFieldNameToOffset( int level )
    {
      if( level < 0 )
        return this.FieldNameToOffset;

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

      //If the list is empty, it means there is no column in the grid right now, so return an empty collection, as does the VisibleFieldNames property.
      return new ColumnNameCollection();
    }

    internal void SetFixedColumnCount( int level, int fixedColumnCount, int correctionValue )
    {
      var dataGridContext = this.DataGridContext;

      // When details are flatten, the detail DataGridContext's FixedColumnCount property is bound to the 
      // DataGridControl's FixedColumnCount property.  Allowing the value to be changed here would destroy the binding.
      if( dataGridContext.IsAFlattenDetail )
        return;

      if( level == -1 )
      {
        this.FixedColumnCountInternal = fixedColumnCount;
      }

      //Set the top level FixedColumnCount property (at this point, either there is no MergedHeaders so level = -1, or fixedColumnCount has been calculated for level 0).
      if( !m_updatingFixedColumnCount.IsSet )
      {
        using( m_updatingFixedColumnCount.Set() )
        {
          this.FixedColumnCount = fixedColumnCount;
        }
      }
    }

    internal int GetFirstViewportColumnFieldNameIndex()
    {
      return this.GetFirstViewportColumnFieldNameIndex( -1 );
    }

    internal int GetFirstViewportColumnFieldNameIndex( int level )
    {
      if( level < 0 )
        return this.FirstViewportColumnFieldNameIndex;

      //If the list is empty, it means there is no merged column in the grid right now, so return 0.
      return 0;
    }

    internal void UpdateFixedColumnCountInfo( FixedColumnCountInfoEventArgs e )
    {
      this.DataGridContext.IsSettingFixedColumnCount = true;

      //Gather the necessary info to update the FixedColumnCount at each MergedHeader level.
      m_fixedColumnCountInfo = new FixedColumnCountInfo();

      ColumnBase triggeringColumn = e.TriggeringColumn;
      if( triggeringColumn == null )
      {
        m_fixedColumnCountInfo.Level = e.Level;
      }
      else
      {
        m_fixedColumnCountInfo.Level = -1;
      }

      switch( e.UpdateType )
      {
        case FixedColumnUpdateType.Hide:
        case FixedColumnUpdateType.Remove:
          {
            m_fixedColumnCountInfo.FixedColumnCount = this.GetFixedColumnCount( m_fixedColumnCountInfo.Level ) - 1;
            m_fixedColumnCountInfo.CorrectionValue = m_fixedColumnCountInfo.FixedColumnCount >= ( this.GetVisibleColumns( m_fixedColumnCountInfo.Level ).Count - 1 ) ? 1 : 0;
            break;
          }

        case FixedColumnUpdateType.Show:
          {
            m_fixedColumnCountInfo.FixedColumnCount = this.GetFixedColumnCount( m_fixedColumnCountInfo.Level ) + 1;
            m_fixedColumnCountInfo.CorrectionValue = m_fixedColumnCountInfo.FixedColumnCount > 0 ? 1 : 0;
            break;
          }

        case FixedColumnUpdateType.Update:
          {
            m_fixedColumnCountInfo.FixedColumnCount = this.GetFixedColumnCount( m_fixedColumnCountInfo.Level );
            m_fixedColumnCountInfo.CorrectionValue = m_fixedColumnCountInfo.FixedColumnCount >= ( this.GetVisibleColumns( m_fixedColumnCountInfo.Level ).Count - 1 ) ? 1 : 0;
            break;
          }
      }
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
      if( !this.IsVirtualizing )
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

      if( level != -1 )
        throw new ArgumentException( "Not expecting merged headers in this version of the datagrid" );

      return this.DataGridContext.VisibleColumns;
    }

    private void OnColumnReordering( FixedCellPanel panel, ColumnReorderingEventArgs e )
    {
      // We must be sure the VisiblePosition is converted to visible index since some Columns can be Visible = false
      int oldVisiblePosition = FixedCellPanel.CalculateVisibleIndex( e.OldVisiblePosition, panel.ColumnsByVisiblePosition );
      int newVisiblePosition = FixedCellPanel.CalculateVisibleIndex( e.NewVisiblePosition, panel.ColumnsByVisiblePosition );

      int correctionValue = 0;
      int level = panel.ParentRow.LevelCache;
      int columnCount = this.GetFixedColumnCount( level );

      if( ( oldVisiblePosition < columnCount ) && ( newVisiblePosition >= columnCount ) )
      {
        // A column has moved from the fixedPanel to the scrollingPanel.  Do not increment version, it will be done by the FixedColumnCount changed
        columnCount--;
        if( columnCount > 0 )
        {
          correctionValue = 1;
        }

        //Set fixed column count at all merged header levels.
        this.SetFixedColumnCount( level, columnCount, correctionValue );

      }
      else if( ( oldVisiblePosition >= columnCount ) && ( newVisiblePosition < columnCount ) )
      {
        // A column has moved from the scrollingPanel to the fixedPanel.  Do not increment version, it will be done by the FixedColumnCount changed
        columnCount++;
        if( columnCount == this.GetVisibleColumns( level ).Count )
        {
          correctionValue = 1;
        }

        //Set fixed column count at all merged header levels.
        this.SetFixedColumnCount( level, columnCount, correctionValue );
      }
      else
      {
        this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ColumnReordering ) );
      }

      // This must be done to stop progation of the event even if only one ColumnManagerCell will raise it.
      e.Handled = true;
    }

    #region IWeakEventListener Members

    protected override bool OnReceivedWeakEvent( Type managerType, object sender, EventArgs e )
    {
      bool handled = true;
      bool detach = false;
      DataGridContext dataGridContext = this.DataGridContext;

      if( managerType == typeof( VisibleColumnsUpdatedEventManager ) )
      {
        var eventArgs = e as VisibleColumnsUpdatedEventArgs;

        //If flag is true, no need to update all the FixedCellPanel's at this time, but make sure it will be update before it is used again.
        if( eventArgs != null && eventArgs.OnlyIncrementFlag )
        {
          base.IncrementVersion( null );
        }
        else
        {
          var detailConfig = this.DataGridContext.SourceDetailConfiguration;

          //Make sure the FixedColumnCount is valid at each MergedHeader level
          if( this.FixedColumnCount != 0 && ( this.DataGridContext.IsSettingFixedColumnCount || ( detailConfig != null && detailConfig.IsSettingFixedColumnCount ) ) )
          {
            if( m_fixedColumnCountInfo != null )
            {
              this.SetFixedColumnCount( m_fixedColumnCountInfo.Level, m_fixedColumnCountInfo.FixedColumnCount, m_fixedColumnCountInfo.CorrectionValue );
              m_fixedColumnCountInfo = null;
            }

            this.DataGridContext.IsSettingFixedColumnCount = false;
            if( detailConfig != null )
            {
              detailConfig.IsSettingFixedColumnCount = false;
            }
          }

          this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ColumnReordering ) );
        }
      }
      else if( managerType == typeof( CollectionChangedEventManager ) )
      {
        if( sender == dataGridContext.Items.SortDescriptions )
        {
          this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.SortingChanged ) );
        }
        else if( sender == dataGridContext.Items.GroupDescriptions )
        {
          this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.GroupingChanged ) );
        }
      }
      else if( managerType == typeof( ColumnActualWidthEventManager ) )
      {
        var args = e as ColumnActualWidthChangedEventArgs;

        // We pass the delta between the old and new value to tell the Panel to reduce the horizontal offset when a column is auto-resized to a smaller value
        if( args != null )
        {
          double delta = args.OldValue - args.NewValue;
          this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ColumnActualWidthChanged, delta ) );
        }
        else
        {
          this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ColumnActualWidthChanged ) );
        }
      }
      else if( managerType == typeof( FixedColumnCountInfoEventManager ) )
      {
        var infoEventArgs = e as FixedColumnCountInfoEventArgs;
        this.UpdateFixedColumnCountInfo( infoEventArgs );
      }
      else if( managerType == typeof( ViewChangedEventManager ) )
      {
        this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.Unspecified ) );

        detach = true;
      }
      else if( managerType == typeof( ThemeChangedEventManager ) )
      {
        this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.Unspecified ) );

        detach = true;
      }
      else if( managerType == typeof( DataGridControlTemplateChangedEventManager ) )
      {
        this.UpdateScrollViewer();
        this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.Unspecified ) );
      }
      else if( managerType == typeof( ItemsSourceChangeCompletedEventManager ) )
      {
        detach = true;
      }
      else
      {
        handled = false;
      }

      if( detach && ( dataGridContext != null ) )
      {
        // Detach the ColumnVirtualizationManager from the DataGridContext.
        ColumnVirtualizationManager.SetColumnVirtualizationManager( dataGridContext, null );
        m_detached = true;

        this.Uninitialize();
      }

      return handled;
    }

    #endregion

    private FixedColumnCountInfo m_fixedColumnCountInfo;
    private bool m_detached;

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

    private sealed class RoutedEventManager
    {
      private static RoutedEventManager Singleton;
      private static int Registered; //0

      private RoutedEventManager()
      {
      }

      internal static RoutedEventManager Instance
      {
        get
        {
          if( RoutedEventManager.Singleton == null )
          {
            Interlocked.CompareExchange<RoutedEventManager>( ref RoutedEventManager.Singleton, new RoutedEventManager(), null );
          }

          return RoutedEventManager.Singleton;
        }
      }

      internal void Register()
      {
        if( Interlocked.CompareExchange( ref RoutedEventManager.Registered, 1, 0 ) == 1 )
          return;

        // Register to ColumnManagerCell.ColumnReorderingEvent on FixedCellPanel type
        // to be notified the corret DataGridContext level
        EventManager.RegisterClassHandler(
          typeof( FixedCellPanel ),
          ColumnManagerCell.ColumnReorderingEvent,
          new ColumnReorderingEventHandler( RoutedEventManager.OnColumnReordering ) );
      }

      private static void OnColumnReordering( object sender, ColumnReorderingEventArgs e )
      {
        var panel = sender as FixedCellPanel;
        if( panel == null )
          return;

        var dataGridContext = DataGridControl.GetDataGridContext( panel );
        if( dataGridContext == null )
          return;

        var manager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManagerBase;
        if( manager == null )
          return;

        manager.OnColumnReordering( panel, e );
      }
    }

    private class FixedColumnCountInfo
    {
      internal int Level
      {
        get;
        set;
      }

      internal int FixedColumnCount
      {
        get;
        set;
      }

      internal int CorrectionValue
      {
        get;
        set;
      }
    }
  }
}
