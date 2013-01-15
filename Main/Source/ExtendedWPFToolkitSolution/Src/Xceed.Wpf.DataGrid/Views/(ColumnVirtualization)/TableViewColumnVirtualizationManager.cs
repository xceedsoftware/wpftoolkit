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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xceed.Utils.Math;
using System.ComponentModel;
using Xceed.Wpf.DataGrid.Markup;
using System.Windows.Data;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace Xceed.Wpf.DataGrid.Views
{
  internal class TableViewColumnVirtualizationManager : ColumnVirtualizationManager
  {
    static TableViewColumnVirtualizationManager()
    {
      // Register to ColumnManagerCell.ColumnReorderingEvent on FixedCellPanel type
      // to be notified the corret DataGridContext level
      EventManager.RegisterClassHandler(
          typeof( FixedCellPanel ),
          ColumnManagerCell.ColumnReorderingEvent,
          new ColumnReorderingEventHandler( TableViewColumnVirtualizationManager.OnColumnReordering ) );
    }

    public TableViewColumnVirtualizationManager( DataGridContext dataGridContext )
      : base( dataGridContext )
    {
    }

    #region UpdateMeasureRequired Public Event

    // This event will be use to notify every listener that the DataGridControl's layout
    // has changed and they must at least call MeasureOverride
    public event EventHandler UpdateMeasureRequired;

    public void NotifyUpdateMeasureRequired( UpdateMeasureRequiredEventArgs args )
    {
      if( this.UpdateMeasureRequired != null )
      {
        this.UpdateMeasureRequired( this, args );
      }
    }

    #endregion

    #region FieldNameToOffset Property

    public Dictionary<string, double> FieldNameToOffset
    {
      get
      {
        return m_fieldNameToOffset;
      }
    }

    private readonly Dictionary<string, double> m_fieldNameToOffset = new Dictionary<string, double>();

    #endregion

    #region FieldNameToWidth Property

    public Dictionary<string, double> FieldNameToWidth
    {
      get
      {
        return m_fieldNameToWidth;
      }
    }

    private readonly Dictionary<string, double> m_fieldNameToWidth = new Dictionary<string, double>();

    #endregion

    #region FieldNameToPosition Property

    public Dictionary<string, int> FieldNameToPosition
    {
      get
      {
        return m_fieldNameToPosition;
      }
    }

    private readonly Dictionary<string, int> m_fieldNameToPosition = new Dictionary<string, int>();

    #endregion

    #region FixedColumnsWidth Property

    public double FixedColumnsWidth
    {
      get;
      private set;
    }

    #endregion

    #region ScrollingColumnsWidth Property

    // The width of all visible columns in 
    public double ScrollingColumnsWidth
    {
      get;
      private set;
    }


    #endregion

    #region FixedColumnCount DependencyProperty

    public static readonly DependencyProperty FixedColumnCountProperty =
    DependencyProperty.Register( "FixedColumnCount", typeof( int ), typeof( TableViewColumnVirtualizationManager ), new UIPropertyMetadata( 0, new PropertyChangedCallback( TableViewColumnVirtualizationManager.OnFixedColumnCountChanged ) ) );

    private static void OnFixedColumnCountChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      TableViewColumnVirtualizationManager columnVirtualizationManager = o as TableViewColumnVirtualizationManager;

      if( columnVirtualizationManager == null )
        return;

      if( e.NewValue != null )
      {
        columnVirtualizationManager.m_fixedColumnCount = ( int )e.NewValue;

        // We want to be sure the cells won't be inserted in ScrollingPanel, but added to it
        columnVirtualizationManager.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ColumnReordering ) );
      }
    }

    public int FixedColumnCount
    {
      get
      {
        return m_fixedColumnCount;
      }
      set
      {
        this.SetValue( FixedColumnCountProperty, value );
      }
    }

    #endregion

    #region FirstColumnCompensationOffset Property

    // This field is set by the FixedCellPanel in OnCompensationOffsetChanged when the AttachedPropertyChanges
    public static readonly DependencyProperty FirstColumnCompensationOffsetProperty = DependencyProperty.Register(
      "FirstColumnCompensationOffset",
      typeof( double ),
      typeof( TableViewColumnVirtualizationManager ),
      new FrameworkPropertyMetadata(
        0d,
        new PropertyChangedCallback( TableViewColumnVirtualizationManager.OnFirstColumnCompensationOffsetChanged ) ) );

    public double FirstColumnCompensationOffset
    {
      get
      {
        return m_firstColumnCompensationOffset;
      }
      set
      {
        this.SetValue( TableViewColumnVirtualizationManager.FirstColumnCompensationOffsetProperty, value );
      }
    }

    private static void OnFirstColumnCompensationOffsetChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      TableViewColumnVirtualizationManager manager = sender as TableViewColumnVirtualizationManager;

      if( manager == null )
        return;

      manager.m_firstColumnCompensationOffset = ( double )e.NewValue;
      manager.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.Unspecified ) );
    }

    private double m_firstColumnCompensationOffset;

    #endregion

    #region IsVirtualizing DependencyProperty

    public static readonly DependencyProperty IsVirtualizingProperty =
    DependencyProperty.Register( "IsVirtualizing", typeof( bool ), typeof( TableViewColumnVirtualizationManager ), new UIPropertyMetadata( true, new PropertyChangedCallback( TableViewColumnVirtualizationManager.OnIsVirtualizingChanged ) ) );

    private static void OnIsVirtualizingChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      TableViewColumnVirtualizationManager columnVirtualizationManager = o as TableViewColumnVirtualizationManager;

      if( columnVirtualizationManager == null )
        return;

      if( e.NewValue != null )
      {
        columnVirtualizationManager.m_flags[ ( int )TableViewColumnVirtualizationManagerFlags.IsVirtualizing ] = ( bool )e.NewValue;
        columnVirtualizationManager.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.VirtualizationStateChanged ) );
      }
    }

    private bool IsVirtualizingCache
    {
      get
      {
        return m_flags[ ( int )TableViewColumnVirtualizationManagerFlags.IsVirtualizing ];
      }
      set
      {
        m_flags[ ( int )TableViewColumnVirtualizationManagerFlags.IsVirtualizing ] = value;
      }
    }

    public bool IsVirtualizing
    {
      get
      {
        return this.IsVirtualizingCache;
      }
      set
      {
        this.SetValue( IsVirtualizingProperty, value );
      }
    }

    #endregion

    #region IsCurrentColumnInViewPort Property

    public bool IsCurrentColumnInScrollingViewPort
    {
      get
      {
        return m_flags[ ( int )TableViewColumnVirtualizationManagerFlags.IsCurrentColumnInScrollingViewPort ];
      }
      private set
      {
        m_flags[ ( int )TableViewColumnVirtualizationManagerFlags.IsCurrentColumnInScrollingViewPort ] = value;
      }
    }

    #endregion

    #region FirstViewPortColumnFieldNameIndex Property

    public int FirstViewportColumnFieldNameIndex
    {
      get;
      private set;
    }

    #endregion

    #region FixedFieldNames Property

    public List<string> FixedFieldNames
    {
      get
      {
        return m_fixedFieldNames;
      }
    }

    private readonly List<string> m_fixedFieldNames = new List<string>();

    #endregion

    #region ScrollingFieldNames Property

    public List<string> ScrollingFieldNames
    {
      get
      {
        return m_scrollingFieldNames;
      }
    }

    private readonly List<string> m_scrollingFieldNames = new List<string>();

    #endregion

    #region FixedAndScrollingFieldNamesLookupDictionary

    // Contains all the FixedFieldNames and ScrollingFieldNames
    // but in a Dictionary to increase lookup speed
    public Dictionary<string, object> FixedAndScrollingFieldNamesLookupDictionary
    {
      get
      {
        return m_requiredFieldNames;
      }
    }

    private Dictionary<string, object> m_requiredFieldNames = new Dictionary<string, object>();

    #endregion

    #region VisibleColumnsTotalWidth Property

    // The width of all visible columns (fixed and scrolling ones)
    public double VisibleColumnsTotalWidth
    {
      get;
      private set;
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

      m_fixedColumnCount = TableView.GetFixedColumnCount( this.DataGridContext );

      Binding fixedColumnCountBindingBase = new Binding( "FixedColumnCount" );
      fixedColumnCountBindingBase.Source = this.DataGridContext;
      fixedColumnCountBindingBase.Mode = BindingMode.OneWay;

      BindingOperations.SetBinding( this,
        TableViewColumnVirtualizationManager.FixedColumnCountProperty,
        fixedColumnCountBindingBase );

      // We must get the value first since it is a bound property
      m_fixedColumnCount = ( int )this.GetValue( TableViewColumnVirtualizationManager.FixedColumnCountProperty );

      Binding isColumnVirtualizingEnabledBindingBase = new Binding();
      isColumnVirtualizingEnabledBindingBase.Source = this.DataGridContext;
      isColumnVirtualizingEnabledBindingBase.Path =
        new PropertyPath( TableView.IsColumnVirtualizationEnabledProperty );
      isColumnVirtualizingEnabledBindingBase.Mode = BindingMode.OneWay;

      BindingOperations.SetBinding( this,
        TableViewColumnVirtualizationManager.IsVirtualizingProperty,
        isColumnVirtualizingEnabledBindingBase );

      Binding compensationOffsetBinding = new Binding();
      compensationOffsetBinding.Path =
        new PropertyPath( TableView.CompensationOffsetProperty );
      compensationOffsetBinding.Source = this.DataGridContext;
      compensationOffsetBinding.Mode = BindingMode.OneWay;

      BindingOperations.SetBinding( this,
        TableViewColumnVirtualizationManager.FirstColumnCompensationOffsetProperty,
        compensationOffsetBinding );


      // We must get the value first since it is a bound property
      this.IsVirtualizingCache = ( bool )this.GetValue( TableViewColumnVirtualizationManager.IsVirtualizingProperty );
    }

    protected override void Uninitialize()
    {
      BindingOperations.ClearBinding( this,
        TableViewColumnVirtualizationManager.FixedColumnCountProperty );

      BindingOperations.ClearBinding( this,
        TableViewColumnVirtualizationManager.IsVirtualizingProperty );

      BindingOperations.ClearBinding( this,
       TableViewColumnVirtualizationManager.FirstColumnCompensationOffsetProperty );


      if( m_scrollViewer != null )
        m_scrollViewer.ScrollChanged -= new ScrollChangedEventHandler( this.ScrollViewer_ScrollChanged );

      ColumnActualWidthEventManager.RemoveListener( this.DataGridContext.Columns, this );
      DataGridControlTemplateChangedEventManager.RemoveListener( this.DataGridContext.DataGridControl, this );

      CollectionChangedEventManager.RemoveListener( this.DataGridContext.Items.SortDescriptions, this );
      CollectionChangedEventManager.RemoveListener( this.DataGridContext.Items.GroupDescriptions, this );


      base.Uninitialize();
    }

    protected override void PreUpdate()
    {
      base.PreUpdate();
    }

    protected override void ResetInternalState()
    {
      // Clear all references to columns
      this.FixedColumnsWidth = 0;
      this.VisibleColumnsTotalWidth = 0;
      this.ScrollingColumnsWidth = 0;

      this.FieldNameToOffset.Clear();
      this.FieldNameToWidth.Clear();
      this.FieldNameToPosition.Clear();

      this.FixedFieldNames.Clear();
      this.ScrollingFieldNames.Clear();
      this.FirstViewportColumnFieldNameIndex = -1;
      this.FixedAndScrollingFieldNamesLookupDictionary.Clear();
    }

    protected override void DoUpdate()
    {
      // We can't compute any values if Colums are not yet added to DataGrid
      if( this.DataGridContext.ColumnsByVisiblePosition.Count == 0 )
        return;

      LinkedListNode<ColumnBase> currentColumnNode = this.DataGridContext.ColumnsByVisiblePosition.First;
      int currentColumnIndex = 0;
      double currentTotalWidth = 0d;
      double currentColumnActualWidth = 0d;

      int currentFixedColumnCount = 0;
      ColumnBase dataGridContextCurrentColumn = this.DataGridContext.CurrentColumn;

      double horizontalOffset = this.GetHorizontalOffset();
      double viewPortWidth = this.GetViewPortWidth();

      double fixedFieldNamesWidth = 0;

      if( this.FixedColumnCount > 0 )
      {
        while( currentColumnNode != null )
        {
          if( this.FixedColumnCount == currentFixedColumnCount )
          {
            // All Fixed Columns were found
            break;
          }

          ColumnBase column = currentColumnNode.Value;

          if( column != null )
          {
            string fieldName = column.FieldName;

            // Ensure we keep the correct visible offset for every columns even if they are Collpased
            this.FieldNameToOffset.Add( fieldName, currentTotalWidth );

            // The Column is not visible
            if( !column.Visible )
            {
              // Move next
              currentColumnNode = currentColumnNode.Next;
              currentColumnIndex++;
              continue;
            }

            this.FieldNameToPosition.Add( fieldName, currentColumnIndex );
            currentColumnActualWidth = column.ActualWidth;

            fixedFieldNamesWidth += currentColumnActualWidth;
            currentTotalWidth += currentColumnActualWidth;
            this.FieldNameToWidth.Add( fieldName, currentColumnActualWidth );

            // We found a FixedCell
            if( this.FixedColumnCount > currentFixedColumnCount )
            {
              // The DataGridContext.CurrentColumn is fixed, so in Viewport
              if( dataGridContextCurrentColumn == column )
              {
                this.IsCurrentColumnInScrollingViewPort = true;
              }

              this.FixedColumnsWidth += currentColumnActualWidth;

              this.FixedFieldNames.Add( fieldName );

              currentColumnNode = currentColumnNode.Next;
              currentColumnIndex++;
              currentFixedColumnCount++;
              continue;
            }
          }
        }
      }

      if( this.IsVirtualizing )
      {
        // We must reduce the possible viewPortWidth only when virtualizing since
        viewPortWidth = Math.Max( 0d, viewPortWidth - fixedFieldNamesWidth );

        // We increment the horizontalOffset to take the fixed Columns width into consideration
        horizontalOffset += fixedFieldNamesWidth;
      }

      // We must consider the fixedFieldNamesWidth width when calculating visible indexes in ViewPort
      double viewPortMinimalOffset = horizontalOffset;
      double viewPortMaximumOffset = horizontalOffset + viewPortWidth;

      bool visibleColumnFieldNameMustBeAdded = false;
      bool firstVisibleScrollingColumn = true;

      while( currentColumnNode != null )
      {
        ColumnBase column = currentColumnNode.Value;
        string fieldName = column.FieldName;

        // Ensure we keep the correct visible offset for every columns even if they are Collpased
        this.FieldNameToOffset.Add( fieldName, currentTotalWidth );

        if( column.Visible )
        {
          visibleColumnFieldNameMustBeAdded = false;
          this.FieldNameToPosition.Add( fieldName, currentColumnIndex );
          currentColumnActualWidth = column.ActualWidth;

          double columnLeftEdgeOffset = currentTotalWidth;
          double columnRightEdgeOffset = columnLeftEdgeOffset + currentColumnActualWidth;

          currentTotalWidth += currentColumnActualWidth;

          this.ScrollingColumnsWidth += currentColumnActualWidth;

          this.FieldNameToWidth.Add( fieldName, currentColumnActualWidth );

          if( this.IsVirtualizing )
          {
            // The cell is before the ViewPort
            // Cell .... | --- ViewPort --- |
            bool beforeViewPort = ( horizontalOffset > columnRightEdgeOffset );

            // The cell is after the ViewPort
            // | --- ViewPort --- | .... Cell
            bool afterViewPort = ( viewPortMaximumOffset < columnLeftEdgeOffset );

            // Columns are in the ViewPort
            if( !beforeViewPort && !afterViewPort )
            {
              // The DataGridContext.CurrentColumn is scrolling and in Viewport
              if( dataGridContextCurrentColumn == column )
              {
                this.IsCurrentColumnInScrollingViewPort = true;
              }

              // We keep the index of the first viewport column index to ease the FixedColumn splitter change
              if( this.FirstViewportColumnFieldNameIndex == -1 )
              {
                this.FirstViewportColumnFieldNameIndex = this.ScrollingFieldNames.Count;
              }

              visibleColumnFieldNameMustBeAdded = true;
            }

            // If the current is the first visible scrolling Column, force the insertion to the list
            // We need it to be there for the case we are tabbing from the Last column of the row.
            visibleColumnFieldNameMustBeAdded |= firstVisibleScrollingColumn;
            firstVisibleScrollingColumn = false;
          }
          else
          {
            visibleColumnFieldNameMustBeAdded = true;
          }

          if( ( visibleColumnFieldNameMustBeAdded ) && ( !this.FixedFieldNames.Contains( fieldName ) ) && ( !this.ScrollingFieldNames.Contains( fieldName ) ) )
          {
            this.ScrollingFieldNames.Add( fieldName );
          }
        }

        currentColumnNode = currentColumnNode.Next;
        currentColumnIndex++;
      }

      string firstColumnFieldName = this.GetFirstVisibleFocusableColumnFieldName();
      if( ( !string.IsNullOrEmpty( firstColumnFieldName ) ) && ( !this.ScrollingFieldNames.Contains( firstColumnFieldName ) ) )
      {
        this.ScrollingFieldNames.Add( firstColumnFieldName );
      }

      string lastColumnFieldName = this.GetLastVisibleFocusableColumnFieldName();
      if( ( !string.IsNullOrEmpty( lastColumnFieldName ) ) && ( !this.ScrollingFieldNames.Contains( lastColumnFieldName ) ) )
      {
        this.ScrollingFieldNames.Add( lastColumnFieldName );
      }

      this.VisibleColumnsTotalWidth = currentTotalWidth;
    }

    protected override void PostUpdate()
    {
      base.PostUpdate();

      foreach( string fieldName in this.FixedFieldNames )
      {
        this.FixedAndScrollingFieldNamesLookupDictionary.Add( fieldName, null );
      }

      foreach( string fieldName in this.ScrollingFieldNames )
      {
        this.FixedAndScrollingFieldNamesLookupDictionary.Add( fieldName, null );
      }
    }

    protected override void IncrementVersion( object parameters )
    {
      base.IncrementVersion( parameters );

      this.NotifyUpdateMeasureRequired( parameters as UpdateMeasureRequiredEventArgs );
    }

    protected override void DataGridContext_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      base.DataGridContext_PropertyChanged( sender, e );

      Debug.Assert( string.IsNullOrEmpty( e.PropertyName ) == false );

      if( string.IsNullOrEmpty( e.PropertyName ) == true )
        return;

      switch( e.PropertyName )
      {
        case "FixedHeaderFooterViewPortSize":
          this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ViewPortWidthChanged ) );
          break;

        case "CurrentItem":
          // No need to increment version, only notify the current item to invalidate measure
          this.NotifyUpdateMeasureRequired( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.CurrentItemChanged ) );
          break;
      }
    }

    private string GetPreviousVisibleFocusableColumnFieldName( LinkedListNode<ColumnBase> currentColumnNode )
    {
      ColumnBase currentColumn = currentColumnNode.Value;

      while( ( currentColumn != null )
        && ( ( currentColumn.ReadOnly )
        && ( !currentColumn.CanBeCurrentWhenReadOnly )
        || ( this.FixedFieldNames.Contains( currentColumn.FieldName ) )
        || ( !currentColumn.Visible ) ) )
      {
        currentColumnNode = currentColumnNode.Previous;
        currentColumn = ( currentColumnNode != null ) ? currentColumnNode.Value : null;
      }

      string nextFocusableFieldName = ( currentColumn != null ) ? currentColumn.FieldName : string.Empty;
      return nextFocusableFieldName;
    }

    private string GetNextVisibleFocusableColumnFieldName( LinkedListNode<ColumnBase> currentColumnNode )
    {
      ColumnBase currentColumn = currentColumnNode.Value;

      while( ( currentColumn != null )
        && ( ( currentColumn.ReadOnly )
        && ( !currentColumn.CanBeCurrentWhenReadOnly )
        || ( this.FixedFieldNames.Contains( currentColumn.FieldName ) )
        || ( !currentColumn.Visible ) ) )
      {
        currentColumnNode = currentColumnNode.Next;
        currentColumn = ( currentColumnNode != null ) ? currentColumnNode.Value : null;
      }

      string nextFocusableFieldName = ( currentColumn != null ) ? currentColumn.FieldName : string.Empty;
      return nextFocusableFieldName;
    }

    private string GetFirstVisibleFocusableColumnFieldName()
    {
      string fieldName;

      if( this.DataGridContext != null )
      {
        fieldName = this.GetNextVisibleFocusableColumnFieldName( this.DataGridContext.ColumnsByVisiblePosition.First );
      }
      else
      {
        fieldName = string.Empty;
      }

      return fieldName;
    }

    private string GetLastVisibleFocusableColumnFieldName()
    {
      string fieldName;

      if( this.DataGridContext != null )
      {
        fieldName = this.GetPreviousVisibleFocusableColumnFieldName( this.DataGridContext.ColumnsByVisiblePosition.Last );
      }
      else
      {
        fieldName = string.Empty;
      }

      return fieldName;
    }

    private double GetHorizontalOffset()
    {
      double horizontalOffset = 0;

      ScrollViewer parentScrollViewer = this.DataGridContext.DataGridControl.ScrollViewer;

      if( parentScrollViewer != null )
        horizontalOffset = parentScrollViewer.HorizontalOffset - this.FirstColumnCompensationOffset;

      return horizontalOffset;
    }

    private double GetViewPortWidth()
    {
      double viewPortWidth = 0;

      IScrollInfo vStackPanel = null;

      // Viewport is only required when UI Virtualization is on
      if( this.IsVirtualizing )
      {
        try
        {
          vStackPanel = this.DataGridContext.DataGridControl.ItemsHost as IScrollInfo;
        }
        catch( Exception )
        {
          // Ignore exceptions since it may be called before the ItemsHost Template is applied
        }

        if( vStackPanel != null )
        {
          // Get the ViewPortWidth
          viewPortWidth = vStackPanel.ViewportWidth;
        }

        // We use the HeaderFooterItem width when greater than the viewport width
        if( ( viewPortWidth == 0 ) || ( this.DataGridContext.FixedHeaderFooterViewPortSize.Width > viewPortWidth ) )
        {
          viewPortWidth = this.DataGridContext.FixedHeaderFooterViewPortSize.Width;
        }
      }

      return viewPortWidth;
    }

    private void UpdateScrollViewer()
    {
      if( this.DataGridContext == null )
        return;

      ScrollViewer newScrollViewer = this.DataGridContext.DataGridControl.ScrollViewer;

      Debug.Assert( ( newScrollViewer != null ) || ( DesignerProperties.GetIsInDesignMode( this ) ) );

      if( newScrollViewer != m_scrollViewer )
      {
        if( m_scrollViewer != null )
          m_scrollViewer.ScrollChanged -= new ScrollChangedEventHandler( this.ScrollViewer_ScrollChanged );

        m_scrollViewer = newScrollViewer;

        if( m_scrollViewer != null )
          m_scrollViewer.ScrollChanged += new ScrollChangedEventHandler( this.ScrollViewer_ScrollChanged );
      }
    }

    private static void OnColumnReordering( object sender, ColumnReorderingEventArgs e )
    {
      FixedCellPanel panel = sender as FixedCellPanel;

      if( panel == null )
        return;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( panel );

      if( dataGridContext == null )
        return;

      TableViewColumnVirtualizationManager columnVirtualizationManager =
          dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;

      if( columnVirtualizationManager == null )
        return;

      int currentFixedColumnCount = columnVirtualizationManager.FixedColumnCount;

      // We must be sure the VisiblePosition is converted to visible index since some Columns can be Visible = false
      int oldVisiblePosition = FixedCellPanel.CalculateVisibleIndex( e.OldVisiblePosition, dataGridContext );
      int newVisiblePosition = FixedCellPanel.CalculateVisibleIndex( e.NewVisiblePosition, dataGridContext );

      if( ( oldVisiblePosition < columnVirtualizationManager.FixedColumnCount ) && ( newVisiblePosition >= columnVirtualizationManager.FixedColumnCount ) )
      {
        // A column was moved from the fixedPanel to the scrollingPanel

        // Do not increment version, it will be done by the FixedColumnCount changed
        TableView.SetFixedColumnCount( columnVirtualizationManager.DataGridContext, columnVirtualizationManager.FixedColumnCount - 1 );
      }
      else if( ( oldVisiblePosition >= columnVirtualizationManager.FixedColumnCount ) && ( newVisiblePosition < columnVirtualizationManager.FixedColumnCount ) )
      {
        // A column was moved from the scrollingPanel to the fixedPanel

        // Do not increment version, it will be done by the FixedColumnCount changed
        TableView.SetFixedColumnCount( columnVirtualizationManager.DataGridContext, columnVirtualizationManager.FixedColumnCount + 1 );
      }
      else
      {
        columnVirtualizationManager.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ColumnReordering ) );
      }

      // This must be done to stop progation of the event even
      // if only 1 ColumnManagerCell will raise it.
      e.Handled = true;
    }

    private void ScrollViewer_ScrollChanged( object sender, System.Windows.Controls.ScrollChangedEventArgs e )
    {
      if( e.OriginalSource != this.DataGridContext.DataGridControl.ScrollViewer )
        return;

      this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ScrollViewerChanged ) );
    }

    #region IWeakEventListener Members

    protected override bool OnReceivedWeakEvent( Type managerType, object sender, EventArgs e )
    {
      bool handled = false;
      bool detachFromDataGridContext = false;

      if( managerType == typeof( VisibleColumnsUpdatedEventManager ) )
      {
        this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ColumnReordering ) );

        handled = true;
      }
      else if( managerType == typeof( CollectionChangedEventManager ) )
      {
        if( sender == this.DataGridContext.Items.SortDescriptions )
          this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.SortingChanged ) );

        if( sender == this.DataGridContext.Items.GroupDescriptions )
          this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.GroupingChanged ) );

        handled = true;
      }
      else if( managerType == typeof( ColumnActualWidthEventManager ) )
      {
        ColumnActualWidthChangedEventArgs args = e as ColumnActualWidthChangedEventArgs;

        // We pass the delta between the old and new value to tell the Panel to reduce the
        // horizontal offset when a column is auto-resized to a smaller value
        if( args != null )
        {
          double delta = args.OldValue - args.NewValue;
          this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ColumnActualWidthChanged, delta ) );
        }
        else
        {
          this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.ColumnActualWidthChanged ) );
        }

        handled = true;
      }
      else if( managerType == typeof( ViewChangedEventManager ) )
      {
        detachFromDataGridContext = true;
        this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.Unspecified ) );

        handled = true;
      }
      else if( managerType == typeof( ThemeChangedEventManager ) )
      {
        detachFromDataGridContext = true;
        this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.Unspecified ) );

        handled = true;
      }
      else if( managerType == typeof( DataGridControlTemplateChangedEventManager ) )
      {
        this.UpdateScrollViewer();
        this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.Unspecified ) );

        handled = true;
      }
      else if( managerType == typeof( ItemsSourceChangeCompletedEventManager ) )
      {
        detachFromDataGridContext = true;
        handled = true;
      }

      if( detachFromDataGridContext == true )
      {
        if( this.DataGridContext != null )
        {
          // Detach the ColumnVirtualizationManager from the DataGridContext and detach from the DataGridContext
          ColumnVirtualizationManager.SetColumnVirtualizationManager( this.DataGridContext, null );

          this.Uninitialize();
        }
      }

      return handled;
    }

    #endregion

    private BitVector32 m_flags = new BitVector32();
    private int m_fixedColumnCount; // = 0;
    private ScrollViewer m_scrollViewer; // = null;

    [Flags]
    private enum TableViewColumnVirtualizationManagerFlags
    {
      IsVirtualizing = 1,
      IsCurrentColumnInScrollingViewPort = 2,
    }
  }
}

