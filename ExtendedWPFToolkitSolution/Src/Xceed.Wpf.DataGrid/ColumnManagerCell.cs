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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Utils.Wpf.DragDrop;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  [TemplatePart( Name = "PART_ColumnResizerThumb", Type = typeof( Thumb ) )]
  [TemplatePart( Name = "PART_ColumnResizerThumbLeft", Type = typeof( Thumb ) )]
  public class ColumnManagerCell : Cell, IDropTarget
  {
    static ColumnManagerCell()
    {
      UIElement.FocusableProperty.OverrideMetadata( typeof( ColumnManagerCell ), new FrameworkPropertyMetadata( false ) );
      Cell.ReadOnlyProperty.OverrideMetadata( typeof( ColumnManagerCell ), new FrameworkPropertyMetadata( true ) );

      ColumnManagerCell.IsPressedProperty = ColumnManagerCell.IsPressedPropertyKey.DependencyProperty;

      ColumnManagerCell.IsBeingDraggedProperty = ColumnManagerCell.IsBeingDraggedPropertyKey.DependencyProperty;
    }

    public ColumnManagerCell()
    {
      this.ReadOnly = true;
    }

    #region IsPressed Read-Only Property

    private static readonly DependencyPropertyKey IsPressedPropertyKey =
      DependencyProperty.RegisterReadOnly( "IsPressed", typeof( bool ), typeof( ColumnManagerCell ), new PropertyMetadata( false ) );

    public static readonly DependencyProperty IsPressedProperty;

    public bool IsPressed
    {
      get
      {
        return ( bool )this.GetValue( ColumnManagerCell.IsPressedProperty );
      }
    }

    private void SetIsPressed( bool value )
    {
      this.SetValue( ColumnManagerCell.IsPressedPropertyKey, value );
    }

    #endregion IsPressed Read-Only Property

    #region IsBeingDragged Read-Only Property

    private static readonly DependencyPropertyKey IsBeingDraggedPropertyKey =
        DependencyProperty.RegisterReadOnly( "IsBeingDragged", typeof( bool ), typeof( ColumnManagerCell ), new PropertyMetadata( false ) );

    public static readonly DependencyProperty IsBeingDraggedProperty;

    public bool IsBeingDragged
    {
      get
      {
        return ( bool )this.GetValue( ColumnManagerCell.IsBeingDraggedProperty );
      }
    }

    private void SetIsBeingDragged( bool value )
    {
      this.SetValue( ColumnManagerCell.IsBeingDraggedPropertyKey, value );
    }

    #endregion IsBeingDragged Read-Only Property

    #region DataGridContext Internal Read-Only Property

    internal DataGridContext DataGridContext
    {
      get
      {
        return m_dataGridContext;
      }
    }

    #endregion

    #region DragSourceManager Property

    // This property is required by the ColumnManagerRow when performing AnimatedColumnReordering since it must
    // handle the Commit when the DraggedCell is dropped over the space already reserved for it in the ColumnManagerRow
    internal DragSourceManager DragSourceManager
    {
      get
      {
        return m_dragSourceManager;
      }
    }

    #endregion

    #region ShowResizeCursor Property

    internal bool ShowResizeCursor
    {
      get;
      set;
    }

    #endregion

    #region AllowResize Internal Read-Only Property

    internal virtual bool AllowResize
    {
      get
      {
        if( this.ParentColumn == null )
          return false;

        var dataGridContext = this.DataGridContext;
        if( dataGridContext == null )
          return false;

        // When details are flatten, only the column at the master level may be resized.
        if( dataGridContext.IsAFlattenDetail )
          return false;

        return true;
      }
    }

    #endregion

    #region CanBeCollapsed Property

    internal override bool CanBeCollapsed
    {
      get
      {
        // A ColumnManagerCell is always collapsible (except when it, or it's parent column, is being dragged), since it can't be edited.
        if( ( bool )this.GetValue( ColumnManagerCell.IsBeingDraggedProperty ) )
        {
          return false;
        }

        var parentColumn = this.ParentColumn;
        if( parentColumn == null )
          return true;

        return !TableflowView.GetIsBeingDraggedAnimated( parentColumn );

      }
    }

    #endregion

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      this.SetupColumnResizerThumb();
    }

    protected override void InitializeCore( DataGridContext dataGridContext, Row parentRow, ColumnBase parentColumn )
    {
      base.InitializeCore( dataGridContext, parentRow, parentColumn );

      this.SetColumnBinding( ColumnManagerCell.ContentProperty, "ParentColumn.Title" );
      this.SetColumnBinding( ColumnManagerCell.ContentTemplateProperty, "ParentColumn.TitleTemplate" );
      this.SetColumnBinding( ColumnManagerCell.ContentTemplateSelectorProperty, "ParentColumn.TitleTemplateSelector" );
    }


    protected internal override void PrepareDefaultStyleKey( ViewBase view )
    {
      var newThemeKey = view.GetDefaultStyleKey( typeof( ColumnManagerCell ) );
      if( object.Equals( this.DefaultStyleKey, newThemeKey ) )
        return;

      this.DefaultStyleKey = newThemeKey;
    }

    protected internal override void PrepareContainer( DataGridContext dataGridContext, object item )
    {
      base.PrepareContainer( dataGridContext, item );
      m_dataGridContext = dataGridContext;
    }

    protected internal override void ClearContainer()
    {
      m_dataGridContext = null;
      base.ClearContainer();
    }

    protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( this.CaptureMouse() )
      {
        this.SetIsPressed( true );

        // Ensure the DragSourceManager is created
        if( m_dragSourceManager == null )
        {
          this.SetupDragManager();
          Debug.Assert( m_dragSourceManager != null );
        }

        if( m_dragSourceManager != null )
        {
          if( dataGridContext != null )
          {
            // Update the DropOutsideCursor since it is defined on the View
            UIViewBase uiViewBase = dataGridContext.DataGridControl.GetView() as UIViewBase;

            m_dragSourceManager.DropOutsideCursor = ( uiViewBase != null ) ? uiViewBase.CannotDropDraggedElementCursor : UIViewBase.DefaultCannotDropDraggedElementCursor;
            m_dragSourceManager.DragStart( e );
          }
        }

        e.Handled = true;
      }

      base.OnMouseLeftButtonDown( e );
    }

    protected override void OnMouseMove( MouseEventArgs e )
    {
      if( ( this.IsMouseCaptured ) && ( e.LeftButton == MouseButtonState.Pressed ) )
      {
        if( m_dragSourceManager != null )
        {
          m_dragSourceManager.DragMove( e );
        }

        if( !this.IsBeingDragged )
        {
          Rect bounds = new Rect( 0d, 0d, this.ActualWidth, this.ActualHeight );
          this.SetIsPressed( bounds.Contains( e.GetPosition( this ) ) );
        }
        else
        {
          this.SetIsPressed( false );
        }

        e.Handled = true;
      }

      base.OnMouseMove( e );
    }

    protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
    {
      // m_dragSourceManager.ProcessMouseLeftButtonUp() will release the capture, so we need to check the IsMouseCaptured and IsPressed states before calling it.
      bool isMouseCaptured = this.IsMouseCaptured;
      bool isPressed = this.IsPressed;

      if( m_dragSourceManager != null )
      {
        m_dragSourceManager.Drop( e );
      }

      if( isMouseCaptured )
      {
        bool click = isPressed;

        this.ReleaseMouseCapture();
        this.SetIsPressed( false );

        if( click )
        {
          this.DoSort( ( ( Keyboard.Modifiers & ModifierKeys.Shift ) != ModifierKeys.Shift ) );
        }

        e.Handled = true;
      }

      // Focus must be done only on mouse up ( after the sort is done ... etc )
      // we have to focus the grid.

      // We don't need to set PreserveEditorFocus to true since clicking on another element will automatically
      // set the Cell/Row IsBeingEdited to false and try to make it leave edition.
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext != null )
      {
        DataGridControl dataGridControl = dataGridContext.DataGridControl;

        if( ( dataGridControl != null ) && ( !dataGridControl.IsKeyboardFocusWithin ) )
        {
          dataGridControl.Focus();
        }
      }

      base.OnMouseLeftButtonUp( e );
    }

    protected override void OnLostMouseCapture( MouseEventArgs e )
    {
      if( m_dragSourceManager != null )
      {
        m_dragSourceManager.DragCancel( e );
      }

      if( this.IsPressed )
      {
        this.SetIsPressed( false );
      }

      base.OnLostMouseCapture( e );
    }

    internal bool CanDoSort()
    {
      ColumnManagerRow parentRow = this.ParentRow as ColumnManagerRow;
      if( parentRow != null )
      {
        if( !parentRow.AllowSort )
          return false;
      }

      DataGridContext dataGridContext = this.DataGridContext;
      if( dataGridContext == null )
        return false;

      // When details are flatten, only the ColumnManagerCell at the master level may do the sort.
      if( dataGridContext.IsAFlattenDetail )
        return false;

      if( dataGridContext.SourceDetailConfiguration == null )
      {
        if( !dataGridContext.Items.CanSort )
          return false;
      }

      if( !this.IsEnabled )
        return false;

      ColumnBase parentColumn = this.ParentColumn;
      if( ( parentColumn == null ) || ( !parentColumn.AllowSort ) )
        return false;

      return true;
    }

    internal void DoSort( bool shiftUnpressed )
    {
      if( !this.CanDoSort() )
        return;

      DataGridContext dataGridContext = this.DataGridContext;
      ColumnBase column = this.ParentColumn;

      Debug.Assert( dataGridContext != null );

      var toggleColumnSortCommand = dataGridContext.ToggleColumnSortCommand;

      toggleColumnSortCommand.Execute( column, shiftUnpressed );
    }

    internal void DoResize( double newWidth, ColumnBase parentColumn )
    {
      if( !this.IsEnabled )
        return;

      if( !parentColumn.HasFixedWidth )
      {
        if( newWidth < MIN_WIDTH )
        {
          newWidth = MIN_WIDTH;
        }

        if( newWidth < parentColumn.MinWidth )
        {
          newWidth = parentColumn.MinWidth;
        }

        if( newWidth > parentColumn.MaxWidth )
        {
          newWidth = parentColumn.MaxWidth;
        }

        parentColumn.Width = newWidth;
      }
    }

    internal virtual void OnColumnResizerThumbDragStarted( DragStartedEventArgs e )
    {
      var parentColumn = this.ParentColumn;
      Debug.Assert( parentColumn != null );

      var dataGridContext = this.DataGridContext;
      Debug.Assert( dataGridContext != null );

      m_originalWidth = parentColumn.ActualWidth;

      if( TableView.GetRemoveColumnStretchingOnResize( dataGridContext ) )
      {
        dataGridContext.ColumnStretchingManager.DisableColumnStretching();
      }
    }

    internal virtual void OnColumnResizerThumbDragDelta( DragDeltaEventArgs e )
    {
      var parentColumn = this.ParentColumn;
      Debug.Assert( parentColumn != null );

      this.DoResize( parentColumn.ActualWidth + e.HorizontalChange, parentColumn );
    }

    internal virtual void OnColumnResizerThumbDragCompleted( DragCompletedEventArgs e )
    {
      if( e.Canceled )
      {
        this.ParentColumn.Width = m_originalWidth;
      }

      m_originalWidth = -1d;
    }

    internal void ShowDropMark( RelativePoint mousePosition )
    {
      if( m_dropMarkAdorner == null )
      {
        var dataGridContext = this.DataGridContext;
        var grid = ( dataGridContext != null ) ? dataGridContext.DataGridControl : null;

        Pen pen = UIViewBase.GetDropMarkPen( this );

        if( ( pen == null ) && ( grid != null ) )
        {
          UIViewBase uiViewBase = grid.GetView() as UIViewBase;
          pen = uiViewBase.DefaultDropMarkPen;
        }

        DropMarkOrientation orientation = UIViewBase.GetDropMarkOrientation( this );

        if( ( orientation == DropMarkOrientation.Default ) && ( grid != null ) )
        {
          UIViewBase uiViewBase = grid.GetView() as UIViewBase;
          orientation = uiViewBase.DefaultDropMarkOrientation;
        }

        m_dropMarkAdorner = new DropMarkAdorner( this, pen, orientation );

        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer( this );

        if( adornerLayer != null )
        {
          adornerLayer.Add( m_dropMarkAdorner );
        }
      }

      m_dropMarkAdorner.UpdateAlignment( mousePosition );
    }

    internal void HideDropMark()
    {
      if( m_dropMarkAdorner != null )
      {
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer( this );

        if( adornerLayer != null )
        {
          adornerLayer.Remove( m_dropMarkAdorner );
        }

        m_dropMarkAdorner = null;
      }
    }

    private void SetupColumnResizerThumb()
    {
      if( m_columnResizerThumb != null )
      {
        m_columnResizerThumb.DragStarted -= new DragStartedEventHandler( this.ColumnResizerThumb_DragStarted );
        m_columnResizerThumb.DragDelta -= new DragDeltaEventHandler( this.ColumnResizerThumb_DragDelta );
        m_columnResizerThumb.DragCompleted -= new DragCompletedEventHandler( this.ColumnResizerThumb_DragCompleted );
        m_columnResizerThumb.QueryCursor -= new QueryCursorEventHandler( this.ColumnResizerThumb_QueryCursor );
        m_columnResizerThumb.MouseDoubleClick -= new MouseButtonEventHandler( this.ColumnResizerThumb_MouseDoubleClick );

        m_columnResizerThumb = null;
      }

      if( m_columnResizerThumbLeft != null )
      {
        m_columnResizerThumbLeft.DragStarted -= new DragStartedEventHandler( this.ColumnResizerThumbLeft_DragStarted );
        m_columnResizerThumbLeft.DragDelta -= new DragDeltaEventHandler( this.ColumnResizerThumbLeft_DragDelta );
        m_columnResizerThumbLeft.DragCompleted -= new DragCompletedEventHandler( this.ColumnResizerThumbLeft_DragCompleted );
        m_columnResizerThumbLeft.QueryCursor -= new QueryCursorEventHandler( this.ColumnResizerThumbLeft_QueryCursor );
        m_columnResizerThumbLeft.MouseDoubleClick -= new MouseButtonEventHandler( this.ColumnResizerThumbLeft_MouseDoubleClick );

        m_columnResizerThumbLeft = null;
      }

      m_columnResizerThumb = this.GetTemplateChild( "PART_ColumnResizerThumb" ) as Thumb;
      m_columnResizerThumbLeft = this.GetTemplateChild( "PART_ColumnResizerThumbLeft" ) as Thumb;

      if( m_columnResizerThumb != null )
      {
        m_columnResizerThumb.DragStarted += new DragStartedEventHandler( this.ColumnResizerThumb_DragStarted );
        m_columnResizerThumb.DragDelta += new DragDeltaEventHandler( this.ColumnResizerThumb_DragDelta );
        m_columnResizerThumb.DragCompleted += new DragCompletedEventHandler( this.ColumnResizerThumb_DragCompleted );
        m_columnResizerThumb.QueryCursor += new QueryCursorEventHandler( this.ColumnResizerThumb_QueryCursor );
        m_columnResizerThumb.MouseDoubleClick += new MouseButtonEventHandler( this.ColumnResizerThumb_MouseDoubleClick );
      }

      if( m_columnResizerThumbLeft != null )
      {
        m_columnResizerThumbLeft.DragStarted += new DragStartedEventHandler( this.ColumnResizerThumbLeft_DragStarted );
        m_columnResizerThumbLeft.DragDelta += new DragDeltaEventHandler( this.ColumnResizerThumbLeft_DragDelta );
        m_columnResizerThumbLeft.DragCompleted += new DragCompletedEventHandler( this.ColumnResizerThumbLeft_DragCompleted );
        m_columnResizerThumbLeft.QueryCursor += new QueryCursorEventHandler( this.ColumnResizerThumbLeft_QueryCursor );
        m_columnResizerThumbLeft.MouseDoubleClick += new MouseButtonEventHandler( this.ColumnResizerThumbLeft_MouseDoubleClick );
      }
    }

    private void SetupDragManager()
    {
      // Prevent any drag operation if columns layout is changing.
      var dataGridContext = this.DataGridContext;
      if( ( dataGridContext == null ) || ( dataGridContext.ColumnManager.IsUpdateDeferred ) )
        return;

      // We do not support DragDrop when there are no AdornerLayer because there wouldn't be any visual feedback for the operation.
      AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer( this );
      if( adornerLayer == null )
      {
        // When virtualizing, the Cell is not yet in the VisualTree because it is the ParentRow's CellsHost which
        // is reponsible of putting them in the VisualTree. We try to get the adorner from the ParentRow instead
        if( this.ParentRow != null )
        {
          adornerLayer = AdornerLayer.GetAdornerLayer( this.ParentRow );
        }

        if( adornerLayer == null )
          return;
      }

      var dataGridControl = dataGridContext.DataGridControl;
      Debug.Assert( dataGridControl != null );

      if( m_dragSourceManager != null )
      {
        m_dragSourceManager.PropertyChanged -= this.DragSourceManager_PropertyChanged;
      }

      // The DataGridControl's AdornerDecoratorForDragAndDrop must be used for dragging in order to include the RenderTransform
      // the DataGridControl may performs. This AdornerDecorator is defined in the ControlTemplate as PART_DragDropAdornerDecorator
      if( ( dataGridControl.DragDropAdornerDecorator != null ) && ( dataGridControl.DragDropAdornerDecorator.AdornerLayer != null ) )
      {
        m_dragSourceManager = new ColumnReorderingDragSourceManager( this, dataGridControl.DragDropAdornerDecorator.AdornerLayer, dataGridControl, this.ParentRow.LevelCache );
      }
      else
      {
        m_dragSourceManager = new ColumnReorderingDragSourceManager( this, null, dataGridControl, this.ParentRow.LevelCache );
      }

      m_dragSourceManager.PropertyChanged += this.DragSourceManager_PropertyChanged;

      // Create bindings to ViewProperties for AutoScroll Properties
      Binding binding = new Binding();
      binding.Path = new PropertyPath( "(0).(1)", DataGridControl.DataGridContextProperty, TableView.AutoScrollIntervalProperty );
      binding.Mode = BindingMode.OneWay;
      binding.Source = this;
      binding.Converter = new MillisecondsConverter();
      BindingOperations.SetBinding( m_dragSourceManager, DragSourceManager.AutoScrollIntervalProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( "(0).(1)", DataGridControl.DataGridContextProperty, TableView.AutoScrollThresholdProperty );
      binding.Mode = BindingMode.OneWay;
      binding.Source = this;
      BindingOperations.SetBinding( m_dragSourceManager, DragSourceManager.AutoScrollThresholdProperty, binding );
    }

    private void DragSourceManager_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      if( e.PropertyName == "IsDragging" )
      {
        this.SetIsBeingDragged( m_dragSourceManager.IsDragging );
      }
    }

    private void ColumnResizerThumb_DragStarted( object sender, DragStartedEventArgs e )
    {
      if( !this.ShowResizeCursor || !this.AllowResize )
        return;

      this.OnColumnResizerThumbDragStarted( e );
    }

    private void ColumnResizerThumb_DragDelta( object sender, DragDeltaEventArgs e )
    {
      if( !this.ShowResizeCursor || !this.AllowResize )
        return;

      this.OnColumnResizerThumbDragDelta( e );
    }

    private void ColumnResizerThumb_DragCompleted( object sender, DragCompletedEventArgs e )
    {
      if( !this.ShowResizeCursor || !this.AllowResize )
        return;

      this.OnColumnResizerThumbDragCompleted( e );
    }

    private void ColumnResizerThumb_MouseDoubleClick( object sender, MouseButtonEventArgs e )
    {
      var parentColumn = this.ParentColumn;
      if( ( parentColumn == null ) || !this.ShowResizeCursor || !this.AllowResize )
        return;

      e.Handled = true;

      double fittedWidth = parentColumn.GetFittedWidth();
      if( fittedWidth != -1 )
      {
        parentColumn.Width = fittedWidth;
      }
    }

    private void ColumnResizerThumb_QueryCursor( object sender, QueryCursorEventArgs e )
    {
      var parentColumn = this.ParentColumn;
      if( parentColumn == null )
        return;

      var dataGridContext = this.DataGridContext;

      bool showResizeCursor = false;
      if( this.AllowResize )
      {
        // Don't disable resizing if ColumnStretching can be disabled by an end-user resize.
        showResizeCursor = ( !parentColumn.HasFixedWidth )
                        || ( ( dataGridContext != null ) && TableView.GetRemoveColumnStretchingOnResize( dataGridContext ) );
      }

      this.ShowResizeCursor = showResizeCursor;
      if( this.ShowResizeCursor )
      {
        UIViewBase viewBase = ( dataGridContext != null ) ? dataGridContext.DataGridControl.GetView() as UIViewBase : null;

        e.Cursor = ( viewBase != null ) ? viewBase.ColumnResizeWestEastCursor : UIViewBase.DefaultColumnResizeWestEastCursor;
        e.Handled = true;
      }
    }

    private ColumnManagerCell GetPreviousVisibleColumnManagerCell()
    {
      var parentColumn = this.ParentColumn;
      if( parentColumn == null )
        return null;

      var previousVisibleColumn = parentColumn.PreviousVisibleColumn;
      if( previousVisibleColumn == null )
        return null;

      return ( ColumnManagerCell )this.ParentRow.Cells[ previousVisibleColumn ];
    }

    private void ColumnResizerThumbLeft_QueryCursor( object sender, QueryCursorEventArgs e )
    {
      var previousColumnManagerCell = this.GetPreviousVisibleColumnManagerCell();
      if( previousColumnManagerCell == null )
        return;

      previousColumnManagerCell.ColumnResizerThumb_QueryCursor( previousColumnManagerCell, e );
    }

    private void ColumnResizerThumbLeft_DragStarted( object sender, DragStartedEventArgs e )
    {
      var previousColumnManagerCell = this.GetPreviousVisibleColumnManagerCell();
      if( previousColumnManagerCell == null )
        return;

      previousColumnManagerCell.ColumnResizerThumb_DragStarted( previousColumnManagerCell, e );
    }

    private void ColumnResizerThumbLeft_MouseDoubleClick( object sender, MouseButtonEventArgs e )
    {
      var previousColumnManagerCell = this.GetPreviousVisibleColumnManagerCell();
      if( previousColumnManagerCell == null )
        return;

      previousColumnManagerCell.ColumnResizerThumb_MouseDoubleClick( previousColumnManagerCell, e );
    }

    private void ColumnResizerThumbLeft_DragDelta( object sender, DragDeltaEventArgs e )
    {
      var previousColumnManagerCell = this.GetPreviousVisibleColumnManagerCell();
      if( previousColumnManagerCell == null )
        return;

      previousColumnManagerCell.ColumnResizerThumb_DragDelta( previousColumnManagerCell, e );
    }

    private void ColumnResizerThumbLeft_DragCompleted( object sender, DragCompletedEventArgs e )
    {
      var previousColumnManagerCell = this.GetPreviousVisibleColumnManagerCell();
      if( previousColumnManagerCell == null )
        return;

      previousColumnManagerCell.ColumnResizerThumb_DragCompleted( previousColumnManagerCell, e );
    }

    private void SetColumnBinding( DependencyProperty targetProperty, string sourceProperty )
    {
      if( BindingOperations.GetBinding( this, targetProperty ) != null )
        return;

      var binding = ColumnManagerCell.CreateColumnBinding( sourceProperty );
      if( binding != null )
      {
        BindingOperations.SetBinding( this, targetProperty, binding );
      }
      else
      {
        BindingOperations.ClearBinding( this, targetProperty );
      }
    }

    private static Binding CreateColumnBinding( string sourceProperty )
    {
      var binding = new Binding();
      binding.Path = new PropertyPath( sourceProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );
      binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

      return binding;
    }

    #region IDropTarget Members

    bool IDropTarget.CanDropElement( UIElement draggedElement, RelativePoint mousePosition )
    {
      var draggedCell = draggedElement as ColumnManagerCell;
      if( ( draggedCell == null ) || ( this == draggedCell ) )
        return false;

      var parentRow = this.ParentRow as ColumnManagerRow;
      if( ( parentRow == null ) || !parentRow.AllowColumnReorder )
        return false;

      var draggedDetailContext = draggedCell.DataGridContext;
      var sourceDetailContext = this.DataGridContext;
      Debug.Assert( ( draggedDetailContext != null ) && ( sourceDetailContext != null ) );

      if( ( sourceDetailContext.SourceDetailConfiguration != draggedDetailContext.SourceDetailConfiguration ) ||
          ( sourceDetailContext.GroupLevelDescriptions != draggedDetailContext.GroupLevelDescriptions ) )
        return false;

      if( !ColumnManagerCell.CanMove( draggedCell ) )
        return false;

      var manager = draggedCell.DragSourceManager as ColumnReorderingDragSourceManager;
      if( ( manager != null ) && ( manager.IsAnimatedColumnReorderingEnabled ) )
      {
        if( !manager.CanReorder( draggedCell, this, mousePosition ) )
          return false;
      }
      else
      {
        var relativePosition = mousePosition.GetPoint( this );
        var moveBefore = ( relativePosition.X <= this.ActualWidth / 2d );

        if( moveBefore )
        {
          if( !draggedCell.CanMoveBefore( this.ParentColumn ) )
            return false;
        }
        else
        {
          if( !draggedCell.CanMoveAfter( this.ParentColumn ) )
            return false;
        }
      }

      return true;
    }

    void IDropTarget.DragEnter( UIElement draggedElement )
    {
    }

    void IDropTarget.DragOver( UIElement draggedElement, RelativePoint mousePosition )
    {
      var draggedCell = draggedElement as ColumnManagerCell;
      if( draggedCell != null )
      {
        var manager = draggedCell.DragSourceManager as ColumnReorderingDragSourceManager;

        // No need for drop mark when performing animated Column reordering
        if( ( manager != null ) && ( manager.IsAnimatedColumnReorderingEnabled ) )
          return;
      }

      this.ShowDropMark( mousePosition );
    }

    void IDropTarget.DragLeave( UIElement draggedElement )
    {
      var draggedCell = draggedElement as ColumnManagerCell;
      if( draggedCell != null )
      {
        var manager = draggedCell.DragSourceManager as ColumnReorderingDragSourceManager;

        // No need for drop mark when performing animated Column reordering
        if( ( manager != null ) && ( manager.IsAnimatedColumnReorderingEnabled ) )
          return;
      }

      this.HideDropMark();
    }

    void IDropTarget.Drop( UIElement draggedElement, RelativePoint mousePosition )
    {
      var draggedCell = draggedElement as ColumnManagerCell;
      if( draggedCell == null )
        return;

      this.ProcessDrop( draggedCell, mousePosition );
    }

    internal void ProcessDrop( ColumnManagerCell draggedCell, RelativePoint mousePosition )
    {
      var manager = default( ColumnReorderingDragSourceManager );

      if( draggedCell != null )
      {
        manager = draggedCell.DragSourceManager as ColumnReorderingDragSourceManager;
      }

      if( ( manager != null ) && ( manager.IsAnimatedColumnReorderingEnabled ) )
      {
        manager.CommitReordering();
      }
      else
      {
        this.HideDropMark();

        var dataGridContext = this.DataGridContext;
        Debug.Assert( dataGridContext != null );

        if( dataGridContext != null )
        {
          var targetColumn = draggedCell.ParentColumn;
          var pivotColumn = this.ParentColumn;

          Debug.Assert( targetColumn != null );
          Debug.Assert( pivotColumn != null );

          var relativePosition = mousePosition.GetPoint( this );
          var offset = Point.Subtract( mousePosition.GetPoint( draggedCell ), relativePosition );
          var moveBefore = true;

          // We assumme the cells are layouted horizontally.
          if( Math.Abs( offset.X ) >= Math.Abs( offset.Y ) )
          {
            // Consider the case where the columns are layouted from left to right in reverse order.
            var reverse = ( ( offset.X > 0d ) == ( targetColumn.VisiblePosition >= pivotColumn.VisiblePosition ) );

            moveBefore = ( ( relativePosition.X < this.ActualWidth / 2d ) != reverse );
          }
          // We assume the cells are layouted vertically.
          else
          {
            // Consider the case where the columns are layouted from top to bottom in reverse order.
            var reverse = ( ( offset.Y > 0d ) == ( targetColumn.VisiblePosition >= pivotColumn.VisiblePosition ) );

            moveBefore = ( ( relativePosition.Y < this.ActualHeight / 2d ) != reverse );
          }

          var success = ( moveBefore )
                          ? dataGridContext.MoveColumnBefore( targetColumn, pivotColumn )
                          : dataGridContext.MoveColumnAfter( targetColumn, pivotColumn );

          Debug.Assert( success );
        }
      }
    }

    internal bool CanMoveBefore( ColumnBase column )
    {
      // A column that is locked cannot move.
      var parentColumn = this.ParentColumn;
      if( ( parentColumn == null ) || ( parentColumn.DraggableStatus != ColumnDraggableStatus.Draggable ) )
        return false;

      // The cell cannot move before a column that is locked in the first position.
      if( ( column == null ) || ( column.DraggableStatus == ColumnDraggableStatus.FirstUndraggable ) )
        return false;

      var dataGridContext = this.DataGridContext;
      if( dataGridContext == null )
        return false;

      if( dataGridContext.AreDetailsFlatten )
      {
        // Column reordering is not allowed for a ColumnManagerCell that is located at a detail level
        // when details are flatten.
        if( dataGridContext.SourceDetailConfiguration != null )
          return false;

        // The main column is always the first column when details are flatten.
        if( column.IsMainColumn )
          return false;
      }

      return true;
    }

    internal bool CanMoveAfter( ColumnBase column )
    {
      // A column that is locked cannot move.
      var parentColumn = this.ParentColumn;
      if( ( parentColumn == null ) || ( parentColumn.DraggableStatus != ColumnDraggableStatus.Draggable ) )
        return false;

      // The cell cannot move after a column that is locked in the last position.
      if( ( column == null ) || ( column.DraggableStatus == ColumnDraggableStatus.LastUndraggable ) )
        return false;

      var dataGridContext = this.DataGridContext;
      if( dataGridContext == null )
        return false;

      if( dataGridContext.AreDetailsFlatten )
      {
        // Column reordering is not allowed for a ColumnManagerCell that is located at a detail level
        // when details are flatten.
        if( dataGridContext.SourceDetailConfiguration != null )
          return false;

        // The main column is not allowed to be reordered when details are flatten.
        if( column.IsMainColumn )
          return false;
      }

      return true;
    }

    internal static bool CanMove( ColumnManagerCell cell )
    {
      if( cell == null )
        return false;

      var dataGridContext = cell.DataGridContext;
      if( dataGridContext == null )
        return false;

      // A column that is locked cannot move.
      var parentColumn = cell.ParentColumn;
      if( ( parentColumn == null ) || ( parentColumn.DraggableStatus != ColumnDraggableStatus.Draggable ) )
        return false;

      if( dataGridContext.AreDetailsFlatten )
      {
        // Column reordering is not allowed for a ColumnManagerCell that is located at a detail level
        // when details are flatten.
        if( dataGridContext.SourceDetailConfiguration != null )
          return false;

        // The main column is not allowed to be reordered when details are flatten.
        if( parentColumn.IsMainColumn )
          return false;
      }

      return true;
    }

    #endregion

    // Will remain null when no AdornerLayer is found.
    private ColumnReorderingDragSourceManager m_dragSourceManager;

    private DropMarkAdorner m_dropMarkAdorner;

    private DataGridContext m_dataGridContext; // = null;

    private const double MIN_WIDTH = 8d;
    private double m_originalWidth = -1d;
    private Thumb m_columnResizerThumb; // = null
    private Thumb m_columnResizerThumbLeft; // null

    #region MillisecondsConverter Private Class

    private sealed class MillisecondsConverter : IValueConverter
    {
      public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
      {
        return TimeSpan.FromMilliseconds( System.Convert.ToDouble( value ) );
      }

      public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
      {
        throw new NotSupportedException();
      }
    }

    #endregion
  }
}
