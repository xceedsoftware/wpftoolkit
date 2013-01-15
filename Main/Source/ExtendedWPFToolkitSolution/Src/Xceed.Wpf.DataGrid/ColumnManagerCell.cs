/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Utils.Wpf.DragDrop;
using Xceed.Wpf.DataGrid.Views;
using System.Collections;
using Xceed.Utils.Collections;
using System.Collections.Specialized;
using System;
using System.Windows.Automation.Peers;
using Xceed.Wpf.DataGrid.Automation;

namespace Xceed.Wpf.DataGrid
{
  [TemplatePart( Name = "PART_ColumnResizerThumb", Type = typeof( Thumb ) )]
  [TemplatePart( Name = "PART_ColumnResizerThumbLeft", Type = typeof( Thumb ) )]
  public class ColumnManagerCell : Cell, IDropTarget
  {
    static ColumnManagerCell()
    {
      FocusableProperty.OverrideMetadata( typeof( ColumnManagerCell ), new FrameworkPropertyMetadata( false ) );
      ReadOnlyProperty.OverrideMetadata( typeof( ColumnManagerCell ), new FrameworkPropertyMetadata( true ) );

      ColumnManagerCell.IsPressedProperty =
        ColumnManagerCell.IsPressedPropertyKey.DependencyProperty;

      ColumnManagerCell.IsBeingDraggedProperty =
        ColumnManagerCell.IsBeingDraggedPropertyKey.DependencyProperty;
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

    #region DataGridContext Property

    private DataGridContext DataGridContext
    {
      get
      {
        Debug.Assert( m_dataGridContext != null );

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

    #region ColumnReordering Internal Event

    internal static readonly RoutedEvent ColumnReorderingEvent =
          EventManager.RegisterRoutedEvent( "ColumnReordering",
               RoutingStrategy.Bubble,
             typeof( ColumnReorderingEventHandler ),
             typeof( ColumnManagerCell ) );

    #endregion

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      this.SetupColumnResizerThumb();
    }

    protected override void InitializeCore( DataGridContext dataGridContext, Row parentRow, ColumnBase parentColumn )
    {
      base.InitializeCore( dataGridContext, parentRow, parentColumn );

      this.SetContent( parentColumn );
      this.SetContentTemplate( parentColumn );
      this.SetContentTemplateSelector( parentColumn );
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
      return new ColumnManagerCellAutomationPeer( this );
    }

    protected internal override void PrepareDefaultStyleKey( ViewBase view )
    {
      object currentThemeKey = view.GetDefaultStyleKey( typeof( ColumnManagerCell ) );

      if( currentThemeKey.Equals( this.DefaultStyleKey ) == false )
      {
        this.DefaultStyleKey = currentThemeKey;
      }
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

            m_dragSourceManager.DropOutsideCursor = ( uiViewBase != null )
             ? uiViewBase.CannotDropDraggedElementCursor
             : UIViewBase.DefaultCannotDropDraggedElementCursor;

            m_dragSourceManager.ProcessMouseLeftButtonDown( e );
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
          m_dragSourceManager.ProcessMouseMove( e );
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
      // m_dragSourceManager.ProcessMouseLeftButtonUp() will release the capture,
      // so we need to check the IsMouseCaptured and IsPressed states before calling it.
      bool isMouseCaptured = this.IsMouseCaptured;
      bool isPressed = this.IsPressed;

      if( m_dragSourceManager != null )
      {
        m_dragSourceManager.ProcessMouseLeftButtonUp( e );
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
        m_dragSourceManager.ProcessLostMouseCapture( e );

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

      Debug.Assert( dataGridContext != null );

      SortingHelper.ToggleColumnSort(
        dataGridContext, dataGridContext.Items.SortDescriptions,
        dataGridContext.Columns, this.ParentColumn, shiftUnpressed );
    }

    internal void DoResize( double newWidth )
    {
      if( !this.IsEnabled )
        return;

      ColumnBase parentColumn = this.ParentColumn;

      if( parentColumn == null )
        return;

      if( !parentColumn.HasFixedWidth )
      {
        if( newWidth < MIN_WIDTH )
          newWidth = MIN_WIDTH;

        if( newWidth < parentColumn.MinWidth )
          newWidth = parentColumn.MinWidth;

        if( newWidth > parentColumn.MaxWidth )
          newWidth = parentColumn.MaxWidth;

        parentColumn.Width = newWidth;
      }
    }

    private void SetContent( ColumnBase parentColumn )
    {
      Binding binding = new Binding();
      binding.Path = new PropertyPath( ColumnBase.TitleProperty );
      binding.Mode = BindingMode.OneWay;
      binding.Source = parentColumn;
      binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding( this, ColumnManagerCell.ContentProperty, binding );
    }

    private void SetContentTemplate( ColumnBase parentColumn )
    {
      Binding binding = new Binding();
      binding.Path = new PropertyPath( ColumnBase.TitleTemplateProperty );
      binding.Mode = BindingMode.OneWay;
      binding.Source = parentColumn;
      binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding( this, ColumnManagerCell.ContentTemplateProperty, binding );
    }

    private void SetContentTemplateSelector( ColumnBase parentColumn )
    {
      Binding binding = new Binding();
      binding.Path = new PropertyPath( ColumnBase.TitleTemplateSelectorProperty );
      binding.Mode = BindingMode.OneWay;
      binding.Source = parentColumn;
      binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding( this, ColumnManagerCell.ContentTemplateSelectorProperty, binding );
    }

    private void SetupDragManager()
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      // We do not support DragDrop when there are no AdornerLayer because there wouldn't 
      // be any visual feedback for the operation.
      AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer( this );

      if( adornerLayer == null )
      {
        // When virtualizing, the Cell is not yet in the VisualTree because it is
        // the ParentRow's CellsHost which is reponsible of putting them in the 
        // VisualTree. We try to get the adorner from the ParentRow instead
        if( this.ParentRow != null )
          adornerLayer = AdornerLayer.GetAdornerLayer( this.ParentRow );

        if( adornerLayer == null )
          return;
      }

      // Can be null in design-time (edition of a style TargetType ColumnManagerCell).
      if( dataGridContext == null )
        return;

      DataGridControl dataGridControl = dataGridContext.DataGridControl;

      if( dataGridControl == null )
        return;


      if( m_dragSourceManager != null )
      {
        m_dragSourceManager.PropertyChanged -= this.DragSourceManager_PropertyChanged;
      }

      // The DataGridControl's AdornerDecoratorForDragAndDrop must be used for dragging in order to include the 
      // RenderTransform the DataGridControl may performs. This AdornerDecorator is defined in the ControlTemplate
      // as PART_DragDropAdornerDecorator
      if( ( dataGridControl.DragDropAdornerDecorator != null )
          && ( dataGridControl.DragDropAdornerDecorator.AdornerLayer != null ) )
      {
        m_dragSourceManager = new ColumnReorderingDragSourceManager( this,
                                                                     dataGridControl.DragDropAdornerDecorator.AdornerLayer,
                                                                     dataGridControl );
      }
      else
      {
        m_dragSourceManager = new ColumnReorderingDragSourceManager( this,
                                                                     null,
                                                                     dataGridControl );
      }

      m_dragSourceManager.PropertyChanged += this.DragSourceManager_PropertyChanged;

      // Create bindings to ViewProperties for AutoScroll Properties
      Binding binding = new Binding();
      binding.Path = new PropertyPath( "(0).(1)",
        DataGridControl.DataGridContextProperty,
        TableView.AutoScrollIntervalProperty );

      binding.Mode = BindingMode.OneWay;
      binding.Source = this;

      BindingOperations.SetBinding( m_dragSourceManager, DragSourceManager.AutoScrollIntervalProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( "(0).(1)", DataGridControl.DataGridContextProperty, TableView.AutoScrollTresholdProperty );
      binding.Mode = BindingMode.OneWay;
      binding.Source = this;

      BindingOperations.SetBinding( m_dragSourceManager, DragSourceManager.AutoScrollTresholdProperty, binding );
    }

    private void DragSourceManager_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      if( e.PropertyName == "IsDragging" )
      {
        this.SetIsBeingDragged( m_dragSourceManager.IsDragging );
      }
    }

    private void ShowDropMark( Point mousePosition )
    {
      if( m_dropMarkAdorner == null )
      {
        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

        DataGridControl grid = ( dataGridContext != null )
          ? dataGridContext.DataGridControl
          : null;

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
          adornerLayer.Add( m_dropMarkAdorner );
      }

      m_dropMarkAdorner.UpdateAlignment( mousePosition );
    }

    private void HideDropMark()
    {
      if( m_dropMarkAdorner != null )
      {
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer( this );

        if( adornerLayer != null )
          adornerLayer.Remove( m_dropMarkAdorner );

        m_dropMarkAdorner = null;
      }
    }

    private void SetupColumnResizerThumb()
    {
      if( m_columnResizerThumb != null )
      {
        m_columnResizerThumb.DragStarted -= new DragStartedEventHandler( ColumnResizerThumb_DragStarted );
        m_columnResizerThumb.DragDelta -= new DragDeltaEventHandler( ColumnResizerThumb_DragDelta );
        m_columnResizerThumb.DragCompleted -= new DragCompletedEventHandler( ColumnResizerThumb_DragCompleted );
        m_columnResizerThumb.QueryCursor -= new QueryCursorEventHandler( ColumnResizerThumb_QueryCursor );
        m_columnResizerThumb.MouseDoubleClick -= new MouseButtonEventHandler( ColumnResizerThumb_MouseDoubleClick );

        m_columnResizerThumb = null;
      }

      if( m_columnResizerThumbLeft != null )
      {
        m_columnResizerThumbLeft.DragStarted -= new DragStartedEventHandler( ColumnResizerThumbLeft_DragStarted );
        m_columnResizerThumbLeft.DragDelta -= new DragDeltaEventHandler( ColumnResizerThumbLeft_DragDelta );
        m_columnResizerThumbLeft.DragCompleted -= new DragCompletedEventHandler( ColumnResizerThumbLeft_DragCompleted );
        m_columnResizerThumbLeft.QueryCursor -= new QueryCursorEventHandler( ColumnResizerThumbLeft_QueryCursor );
        m_columnResizerThumbLeft.MouseDoubleClick -= new MouseButtonEventHandler( ColumnResizerThumbLeft_MouseDoubleClick );

        m_columnResizerThumbLeft = null;
      }

      m_columnResizerThumb = this.GetTemplateChild( "PART_ColumnResizerThumb" ) as Thumb;
      m_columnResizerThumbLeft = this.GetTemplateChild( "PART_ColumnResizerThumbLeft" ) as Thumb;

      if( m_columnResizerThumb != null )
      {
        m_columnResizerThumb.DragStarted += new DragStartedEventHandler( ColumnResizerThumb_DragStarted );
        m_columnResizerThumb.DragDelta += new DragDeltaEventHandler( ColumnResizerThumb_DragDelta );
        m_columnResizerThumb.DragCompleted += new DragCompletedEventHandler( ColumnResizerThumb_DragCompleted );
        m_columnResizerThumb.QueryCursor += new QueryCursorEventHandler( ColumnResizerThumb_QueryCursor );
        m_columnResizerThumb.MouseDoubleClick += new MouseButtonEventHandler( ColumnResizerThumb_MouseDoubleClick );
      }

      if( m_columnResizerThumbLeft != null )
      {
        m_columnResizerThumbLeft.DragStarted += new DragStartedEventHandler( ColumnResizerThumbLeft_DragStarted );
        m_columnResizerThumbLeft.DragDelta += new DragDeltaEventHandler( ColumnResizerThumbLeft_DragDelta );
        m_columnResizerThumbLeft.DragCompleted += new DragCompletedEventHandler( ColumnResizerThumbLeft_DragCompleted );
        m_columnResizerThumbLeft.QueryCursor += new QueryCursorEventHandler( ColumnResizerThumbLeft_QueryCursor );
        m_columnResizerThumbLeft.MouseDoubleClick += new MouseButtonEventHandler( ColumnResizerThumbLeft_MouseDoubleClick );
      }
    }

    private void ColumnResizerThumb_MouseDoubleClick( object sender, MouseButtonEventArgs e )
    {
      ColumnBase parentColumn = this.ParentColumn;

      if( parentColumn == null )
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
      ColumnBase parentColumn = this.ParentColumn;

      if( parentColumn == null )
        return;

      DataGridContext dataGridContext = this.DataGridContext;

      bool showResizeCursor = !parentColumn.HasFixedWidth;

      if( !showResizeCursor )
      {
        // Don't disable resizing if ColumnStretching can be disabled by an end-user resize.
        showResizeCursor = ( dataGridContext != null ) && ( TableView.GetRemoveColumnStretchingOnResize( dataGridContext ) );
      }

      if( showResizeCursor )
      {
        if( dataGridContext != null )
        {
          UIViewBase uiViewBase = dataGridContext.DataGridControl.GetView() as UIViewBase;

          e.Cursor = ( uiViewBase != null )
            ? uiViewBase.ColumnResizeWestEastCursor
            : UIViewBase.DefaultColumnResizeWestEastCursor;
        }
        else
        {
          e.Cursor = UIViewBase.DefaultColumnResizeWestEastCursor;
        }

        e.Handled = true;
      }
    }

    private void ColumnResizerThumb_DragStarted( object sender, DragStartedEventArgs e )
    {
      ColumnBase parentColumn = this.ParentColumn;

      if( parentColumn == null )
        return;

      m_originalWidth = parentColumn.ActualWidth;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( ( dataGridContext != null ) && ( TableView.GetRemoveColumnStretchingOnResize( dataGridContext ) ) )
      {
        dataGridContext.ColumnStretchingManager.DisableColumnStretching();
      }
    }

    private void ColumnResizerThumb_DragDelta( object sender, DragDeltaEventArgs e )
    {
      ColumnBase parentColumn = this.ParentColumn;

      if( parentColumn != null )
      {
        this.DoResize( parentColumn.ActualWidth + e.HorizontalChange );
      }
    }

    private void ColumnResizerThumb_DragCompleted( object sender, DragCompletedEventArgs e )
    {
      ColumnBase parentColumn = this.ParentColumn;

      if( parentColumn == null )
        return;

      if( e.Canceled || parentColumn.HasFixedWidth )
        parentColumn.Width = m_originalWidth;

      m_originalWidth = -1d;
    }

    private ColumnManagerCell GetPreviousVisibleColumnManagerCell()
    {
      var previousVisibleColumn = this.ParentColumn.PreviousVisibleColumn;

      if( previousVisibleColumn == null )
        return null;

      return ( ColumnManagerCell )this.ParentRow.Cells[ previousVisibleColumn ];
    }

    private void ColumnResizerThumbLeft_QueryCursor( object sender, QueryCursorEventArgs e )
    {
      var previousColumnManagerCell = this.GetPreviousVisibleColumnManagerCell();

      if( previousColumnManagerCell != null )
        previousColumnManagerCell.ColumnResizerThumb_QueryCursor( previousColumnManagerCell, e );
    }

    private void ColumnResizerThumbLeft_DragStarted( object sender, DragStartedEventArgs e )
    {
      var previousColumnManagerCell = this.GetPreviousVisibleColumnManagerCell();

      if( previousColumnManagerCell != null )
        previousColumnManagerCell.ColumnResizerThumb_DragStarted( previousColumnManagerCell, e );
    }

    private void ColumnResizerThumbLeft_MouseDoubleClick( object sender, MouseButtonEventArgs e )
    {
      var previousColumnManagerCell = this.GetPreviousVisibleColumnManagerCell();

      if( previousColumnManagerCell != null )
        previousColumnManagerCell.ColumnResizerThumb_MouseDoubleClick( previousColumnManagerCell, e );
    }

    private void ColumnResizerThumbLeft_DragDelta( object sender, DragDeltaEventArgs e )
    {
      var previousColumnManagerCell = this.GetPreviousVisibleColumnManagerCell();

      if( previousColumnManagerCell != null )
        previousColumnManagerCell.ColumnResizerThumb_DragDelta( previousColumnManagerCell, e );
    }

    private void ColumnResizerThumbLeft_DragCompleted( object sender, DragCompletedEventArgs e )
    {
      var previousColumnManagerCell = this.GetPreviousVisibleColumnManagerCell();

      if( previousColumnManagerCell != null )
        previousColumnManagerCell.ColumnResizerThumb_DragCompleted( previousColumnManagerCell, e );
    }

    #region IDropTarget Members

    bool IDropTarget.CanDropElement( UIElement draggedElement )
    {
      bool allowColumnReorder = true;
      ColumnManagerRow parentRow = this.ParentRow as ColumnManagerRow;

      if( parentRow != null )
        allowColumnReorder = parentRow.AllowColumnReorder;

      DataGridContext sourceDetailContext = this.DataGridContext;
      Debug.Assert( sourceDetailContext != null );
      DetailConfiguration sourceDetailConfig = ( sourceDetailContext != null ) ? sourceDetailContext.SourceDetailConfiguration : null;

      DataGridContext draggedDetailContext = DataGridControl.GetDataGridContext( draggedElement );
      Debug.Assert( draggedDetailContext != null );
      DetailConfiguration draggedDetailConfig = ( draggedDetailContext != null ) ? draggedDetailContext.SourceDetailConfiguration : null;


      return ( ( sourceDetailConfig == draggedDetailConfig ) &&
             ( sourceDetailContext != null ) &&
             ( draggedDetailContext != null ) &&
             ( sourceDetailContext.GroupLevelDescriptions == draggedDetailContext.GroupLevelDescriptions ) &&
             ( allowColumnReorder ) &&
             ( draggedElement is ColumnManagerCell ) &&
             ( draggedElement != this ) );
    }

    void IDropTarget.DragEnter( UIElement draggedElement )
    {
    }

    void IDropTarget.DragOver( UIElement draggedElement, Point mousePosition )
    {
      ColumnManagerCell draggedCell = draggedElement as ColumnManagerCell;

      if( draggedCell != null )
      {
        ColumnReorderingDragSourceManager manager =
          draggedCell.DragSourceManager as ColumnReorderingDragSourceManager;

        // No need for drop mark when performing animated Column reordering
        if( ( manager != null ) && ( manager.IsAnimatedColumnReorderingEnabled ) )
          return;
      }

      this.ShowDropMark( mousePosition );
    }

    void IDropTarget.DragLeave( UIElement draggedElement )
    {
      ColumnManagerCell draggedCell = draggedElement as ColumnManagerCell;

      if( draggedCell != null )
      {
        ColumnReorderingDragSourceManager manager =
          draggedCell.DragSourceManager as ColumnReorderingDragSourceManager;

        // No need for drop mark when performing animated Column reordering
        if( ( manager != null ) && ( manager.IsAnimatedColumnReorderingEnabled ) )
          return;
      }

      this.HideDropMark();
    }

    void IDropTarget.Drop( UIElement draggedElement )
    {
      ColumnManagerCell draggedCell = draggedElement as ColumnManagerCell;

      if( draggedCell == null )
        return;

      ColumnReorderingDragSourceManager manager = null;

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
        int oldPosition = draggedCell.ParentColumn.VisiblePosition;
        int newPosition = this.ParentColumn.VisiblePosition;

        if( m_dropMarkAdorner != null )
        {
          DropMarkAlignment alignment = m_dropMarkAdorner.Alignment;

          this.HideDropMark();

          // This will force every Rows to update there layout

          ColumnReorderingEventArgs e = new ColumnReorderingEventArgs(
                    ColumnManagerCell.ColumnReorderingEvent,
                    oldPosition,
                    newPosition );

          this.RaiseEvent( e );

          if( draggedCell.ParentColumn.VisiblePosition < newPosition )
          {
            if( alignment == DropMarkAlignment.Near )
            {
              draggedCell.ParentColumn.VisiblePosition = newPosition - 1;
            }
            else
            {
              draggedCell.ParentColumn.VisiblePosition = newPosition;
            }
          }
          else
          {
            if( alignment == DropMarkAlignment.Near )
            {
              draggedCell.ParentColumn.VisiblePosition = newPosition;
            }
            else
            {
              draggedCell.ParentColumn.VisiblePosition = newPosition + 1;
            }
          }
        }
      }
    }

    #endregion IDropTarget Members

    // Will remain null when no AdornerLayer is found.
    private ColumnReorderingDragSourceManager m_dragSourceManager;
    private DropMarkAdorner m_dropMarkAdorner;

    private DataGridContext m_dataGridContext; // = null;

    private const double MIN_WIDTH = 8d;
    private double m_originalWidth = -1d;
    private Thumb m_columnResizerThumb; // = null
    private Thumb m_columnResizerThumbLeft; // null
  }
}
