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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Wpf.DataGrid.Converters;

namespace Xceed.Wpf.DataGrid.Views
{
  public class DataGridScrollViewer : ScrollViewer
  {
    static DataGridScrollViewer()
    {
      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( DataGridScrollViewer ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnParentDataGridChanged ) ) );
    }

    internal DataGridScrollViewer()
    {
      this.Loaded += new RoutedEventHandler( OnLoaded );
      this.Unloaded += new RoutedEventHandler( OnUnloaded );
      m_templateHelper = new ScrollViewerTemplateHelper( this, this.DragScrollBegin, this.DragScrollEnd );
    }

    #region SynchronizedScrollViewerPosition Attached Property

    public static readonly DependencyProperty SynchronizedScrollViewerPositionProperty = DependencyProperty.RegisterAttached(
      "SynchronizedScrollViewerPosition",
      typeof( SynchronizedScrollViewerPosition ),
      typeof( TableViewScrollViewer ),
      new UIPropertyMetadata( SynchronizedScrollViewerPosition.None ) );

    public static SynchronizedScrollViewerPosition GetSynchronizedScrollViewerPosition( DependencyObject obj )
    {
      return ( SynchronizedScrollViewerPosition )obj.GetValue( TableViewScrollViewer.SynchronizedScrollViewerPositionProperty );
    }

    public static void SetSynchronizedScrollViewerPosition( DependencyObject obj, SynchronizedScrollViewerPosition value )
    {
      obj.SetValue( TableViewScrollViewer.SynchronizedScrollViewerPositionProperty, value );
    }

    #endregion SynchronizedScrollViewerPosition Attached Property

    #region SynchronizedScrollViewersWidth Property

    internal double SynchronizedScrollViewersWidth
    {
      get
      {
        double maxValue = 0d;

        foreach( SynchronizedScrollViewer ssv in m_childScrollViewers )
        {
          if( ssv.ScrollOrientation == ScrollOrientation.Horizontal )
          {
            if( ssv.ExtentWidth > maxValue )
            {
              maxValue = ssv.ExtentWidth;
            }
          }
        }

        return maxValue;
      }
    }

    #endregion

    #region SynchronizedScrollViewersHeight Property

    internal double SynchronizedScrollViewersHeight
    {
      get
      {
        double maxValue = 0d;

        foreach( SynchronizedScrollViewer ssv in m_childScrollViewers )
        {
          if( ssv.ScrollOrientation == ScrollOrientation.Vertical )
          {
            if( ssv.ExtentHeight > maxValue )
            {
              maxValue = ssv.ExtentHeight;
            }
          }
        }

        return maxValue;
      }
    }

    #endregion

    #region SynchronizedScrollViewers Property

    internal IEnumerable<SynchronizedScrollViewer> SynchronizedScrollViewers
    {
      get
      {
        return m_childScrollViewers;
      }
    }

    private static readonly DependencyProperty SynchronizedScrollViewerExtentProperty = DependencyProperty.Register(
      "SynchronizedScrollViewerExtent",
      typeof( double ),
      typeof( DataGridScrollViewer ),
      new FrameworkPropertyMetadata( new PropertyChangedCallback( DataGridScrollViewer.OnSynchronizedScrollViewerExtentPropertyChanged ) ) );

    private static void OnSynchronizedScrollViewerExtentPropertyChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      DataGridScrollViewer dataGridScrollViewer = o as DataGridScrollViewer;

      if( dataGridScrollViewer != null )
      {
        FrameworkElement panel = dataGridScrollViewer.ScrollInfo as FrameworkElement;

        if( panel != null )
        {
          panel.InvalidateMeasure();
        }
      }
    }

    internal int MeasureVersion
    {
      get
      {
        return m_measureVersion;
      }
    }

    #endregion

    #region HorizontalScrollBar Property

    protected internal ScrollBar HorizontalScrollBar
    {
      get
      {
        return m_templateHelper.HorizontalScrollBar;
      }
    }

    #endregion

    #region VerticalScrollBar Property

    protected internal ScrollBar VerticalScrollBar
    {
      get
      {
        return m_templateHelper.VerticalScrollBar;
      }
    }

    #endregion

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      this.ClearScrollViewerList();
      m_deferableChilds.Clear();

      this.PopulateScrollViewerList( this, m_childScrollViewers );
      this.FindAllDeferableChildren( this, m_deferableChilds );

      m_templateHelper.RefreshTemplate();

      this.UpdateChildScrollViewers();
    }

    internal void HandlePageUpNavigation()
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return;

      DataGridControl dataGridControl = dataGridContext.DataGridControl;
      FrameworkElement hostPanel = dataGridControl.ItemsHost;

      if( hostPanel != null )
      {
        // First thing to do here is the force the relayout of the ItemsHostPanel so that the target element gets "visible"
        hostPanel.UpdateLayout();

        FrameworkElement firstVisibleItem = ScrollViewerHelper.GetFirstVisibleContainer(
          dataGridControl, hostPanel, this );

        if( firstVisibleItem != null )
        {
          DataGridContext firstVisibleItemDataGridContext = DataGridControl.GetDataGridContext( firstVisibleItem );

          object oldItem = dataGridContext.InternalCurrentItem;

          bool focused;

          // Make the first visible item the current and focused.
          // in order to cancel the current column in mixed mode when paged-Up
          if( dataGridControl.NavigationBehavior == NavigationBehavior.RowOrCell )
            focused = dataGridControl.SetFocusHelper(
            firstVisibleItem, null, true, true );
          else
          {
            focused = dataGridControl.SetFocusHelper(
              firstVisibleItem, firstVisibleItemDataGridContext.CurrentColumn, true, true );
          }

          if( ( ( !focused ) || ( oldItem == firstVisibleItemDataGridContext.InternalCurrentItem ) )
            && ( !dataGridControl.HasValidationError ) )
          {
            // The current item was already the first item, move focus up.
            DataGridItemsHost.ProcessMoveFocus( Key.Up );
          }
        }
      }
    }

    internal void HandlePageDownNavigation()
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return;

      DataGridControl dataGridControl = dataGridContext.DataGridControl;
      FrameworkElement hostPanel = dataGridControl.ItemsHost;

      if( hostPanel != null )
      {
        // First thing to do here is the force the relayout of the ItemsHostPanel so that the target element gets "visible"
        hostPanel.UpdateLayout();

        // Then retrieve the last visible item in this panel
        FrameworkElement lastVisibleItem = ScrollViewerHelper.GetLastVisibleContainer(
          dataGridControl, hostPanel, this ) as FrameworkElement;

        if( lastVisibleItem != null )
        {
          DataGridContext lastVisibleItemDataGridContext = DataGridControl.GetDataGridContext( lastVisibleItem );

          object oldItem = dataGridContext.InternalCurrentItem;

          bool focused;

          // Make the last visible item the current and focused.
          // in order to cancel the current column in mixed mode when paged-Down
          if( dataGridControl.NavigationBehavior == NavigationBehavior.RowOrCell )
            focused = dataGridControl.SetFocusHelper(
            lastVisibleItem, null, true, true );
          else
          {
            focused = dataGridControl.SetFocusHelper(
              lastVisibleItem, lastVisibleItemDataGridContext.CurrentColumn, true, true );
          }

          if( ( ( !focused ) || ( oldItem == lastVisibleItemDataGridContext.InternalCurrentItem ) )
            && ( !dataGridControl.HasValidationError ) )
          {
            // The current item was already the first item, move focus down.
            DataGridItemsHost.ProcessMoveFocus( Key.Down );
          }
        }
      }
    }

    protected override void OnKeyDown( KeyEventArgs e )
    {
      base.OnKeyDown( e );

      if( e.Handled )
        return;

      switch( e.Key )
      {
        // Handle the System key definition (basically with ALT key pressed)
        case Key.System:
          this.HandleSystemKey( e );
          break;

        case Key.PageUp:
          this.HandlePageUpKey( e );
          break;

        case Key.PageDown:
          this.HandlePageDownKey( e );
          break;

        case Key.Home:
          this.HandlePageHomeKey( e );
          break;

        case Key.End:
          this.HandlePageEndKey( e );
          break;

        case Key.Up:
          this.HandleUpKey( e );
          break;

        case Key.Down:
          this.HandleDownKey( e );
          break;

        case Key.Left:
          this.HandleLeftKey( e );
          break;

        case Key.Right:
          this.HandleRightKey( e );
          break;

        default:
          base.OnKeyDown( e );
          break;
      }
    }

    protected virtual void HandleSystemKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      DataGridContext dataGridContext = null;

      if( e.OriginalSource != null )
      {
        dataGridContext = DataGridControl.GetDataGridContext( e.OriginalSource as DependencyObject );
      }
      else
      {
        dataGridContext = DataGridControl.GetDataGridContext( this );
      }

      if( dataGridContext == null )
        return;

      PagingBehavior pagingBehavior = dataGridContext.DataGridControl.PagingBehavior;

      // If Alt-PageDown was pressed
      if( e.SystemKey == Key.PageDown )
      {
        // Process Paging in opposed axis
        if( pagingBehavior == PagingBehavior.TopToBottom )
        {
          this.PageRight();
        }
        else
        {
          this.PageDown();
        }

        e.Handled = true;
      }
      // If Alt-PageUp was pressed
      else if( e.SystemKey == Key.PageUp )
      {
        // Process scrolling in opposed axis
        if( pagingBehavior == PagingBehavior.TopToBottom )
        {
          this.PageLeft();
        }
        else
        {
          this.PageUp();
        }

        e.Handled = true;
      }
      else
      {
        // If neither Alt-PageUp or alt-PageDown, flag the input as non-processed.
        e.Handled = false;
      }
    }

    protected virtual void HandlePageUpKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      DataGridContext dataGridContext = null;

      if( e.OriginalSource != null )
      {
        dataGridContext = DataGridControl.GetDataGridContext( e.OriginalSource as DependencyObject );
      }
      else
      {
        dataGridContext = DataGridControl.GetDataGridContext( this );
      }

      if( dataGridContext == null )
        return;

      DataGridControl dataGridControl = dataGridContext.DataGridControl;
      PagingBehavior pagingBehavior = dataGridControl.PagingBehavior;
      NavigationBehavior navigationBehavior = dataGridControl.NavigationBehavior;

      //If the Ctrl Modifier was held at the same time
      if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
      {
        //scroll to absolute end of the scroll viewer
        if( pagingBehavior == PagingBehavior.TopToBottom )
        {
          this.ScrollToTop();
        }
        else
        {
          this.ScrollToLeftEnd();
        }

        // Then handle new selection!
        if( navigationBehavior != NavigationBehavior.None )
          this.HandlePageUpNavigation();
      }
      // No special modifiers were held
      else
      {
        // Retrieve first visible item
        FrameworkElement firstVisibleItem = ScrollViewerHelper.GetFirstVisibleContainer(
          dataGridControl, dataGridControl.ItemsHost, this );

        if( firstVisibleItem != null )
        {
          // There is an identified weakness with the IsKeyboardFocusWithin property where 
          // it cannot tell if the focus is within a Popup which is within the element
          // This has been identified, and only the places where it caused problems 
          // were fixed... This comment is only here to remind developpers of the flaw

          // If the item has keyboard focus
          if( ( firstVisibleItem.IsKeyboardFocusWithin ) || ( firstVisibleItem.IsKeyboardFocused )
              || ( navigationBehavior == NavigationBehavior.None ) )
          {
            // Then scroll
            if( pagingBehavior == PagingBehavior.TopToBottom )
            {
              this.PageUp();
            }
            else
            {
              this.PageLeft();
            }
          }

          // And process new selection
          if( navigationBehavior != NavigationBehavior.None )
            this.HandlePageUpNavigation();
        }
        else
        {
          // Normaly for when we are in dataGridControl.NavigationBehavior == NavigationBehavior.None
          if( pagingBehavior == PagingBehavior.TopToBottom )
          {
            this.PageUp();
          }
          else
          {
            this.PageLeft();
          }
        }
      }

      e.Handled = true;
    }

    protected virtual void HandlePageDownKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      DataGridContext dataGridContext = null;

      if( e.OriginalSource != null )
      {
        dataGridContext = DataGridControl.GetDataGridContext( e.OriginalSource as DependencyObject );
      }
      else
      {
        dataGridContext = DataGridControl.GetDataGridContext( this );
      }

      if( dataGridContext == null )
        return;

      DataGridControl dataGridControl = dataGridContext.DataGridControl;
      PagingBehavior pagingBehavior = dataGridControl.PagingBehavior;
      NavigationBehavior navigationBehavior = dataGridControl.NavigationBehavior;

      // If the Ctrl Modifier was held at the same time
      if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
      {
        // Scroll to absolute end of the scroll viewer
        if( pagingBehavior == PagingBehavior.TopToBottom )
        {
          this.ScrollToBottom();
        }
        else
        {
          this.ScrollToRightEnd();
        }

        // Then handle new selection!
        if( navigationBehavior != NavigationBehavior.None )
          this.HandlePageDownNavigation();
      }
      //No special modifiers were held
      else
      {
        FrameworkElement lastVisibleContainer = ScrollViewerHelper.GetLastVisibleContainer(
          dataGridControl, dataGridControl.ItemsHost, this );

        if( lastVisibleContainer != null )
        {
          // There is an identified weakness with the IsKeyboardFocusWithin property where 
          // it cannot tell if the focus is within a Popup which is within the element
          // This has been identified, and only the places where it caused problems 
          // were fixed... This comment is only here to remind developpers of the flaw

          // If the item has keyboard focus
          if( ( lastVisibleContainer.IsKeyboardFocusWithin ) || ( lastVisibleContainer.IsKeyboardFocused )
              || ( navigationBehavior == NavigationBehavior.None ) )
          {
            // Then scroll
            if( pagingBehavior == PagingBehavior.TopToBottom )
            {
              this.PageDown();
            }
            else
            {
              this.PageRight();
            }
          }

          //and process new selection
          if( navigationBehavior != NavigationBehavior.None )
            this.HandlePageDownNavigation();
        }
        else
        {
          // Normaly for when we are in dataGridControl.NavigationBehavior == NavigationBehavior.None
          if( pagingBehavior == PagingBehavior.TopToBottom )
          {
            this.PageDown();
          }
          else
          {
            this.PageRight();
          }
        }
      }

      e.Handled = true;
    }

    protected virtual void HandlePageHomeKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      DataGridContext dataGridContext = null;

      if( e.OriginalSource != null )
      {
        dataGridContext = DataGridControl.GetDataGridContext( e.OriginalSource as DependencyObject );
      }
      else
      {
        dataGridContext = DataGridControl.GetDataGridContext( this );
      }

      if( dataGridContext == null )
        return;

      PagingBehavior pagingBehavior = dataGridContext.DataGridControl.PagingBehavior;
      NavigationBehavior navigationBehavior = dataGridContext.DataGridControl.NavigationBehavior;

      ColumnBase currentColumn = dataGridContext.CurrentColumn;

      if( ( ( navigationBehavior == NavigationBehavior.CellOnly ) || ( navigationBehavior == NavigationBehavior.RowOrCell ) ) && ( currentColumn != null ) )
      {
        int oldCurrentIndex = currentColumn.Index;

        NavigationHelper.MoveFocusToFirstVisibleColumn( dataGridContext );

        var columnVirtualizationManager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManagerBase;

        //if the first focusable column is is within the viewport, scroll so 0d offset, otherwise, bringIntoView
        bool isFixedColumn = ( columnVirtualizationManager == null ) ? false : columnVirtualizationManager.GetFixedFieldNames().Contains( currentColumn.FieldName );

        if( ( ( this.IsCellsOffsetNeedReset( dataGridContext ) ) && ( oldCurrentIndex == currentColumn.Index ) ) || ( isFixedColumn ) )
        {
          this.Dispatcher.BeginInvoke( DispatcherPriority.Normal, new Action( this.ScrollToLeftEnd ) );
        }

        if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
        {
          if( pagingBehavior == PagingBehavior.TopToBottom )
          {
            this.ScrollToTop();
          }
          else
          {
            this.ScrollToLeftEnd();
          }

          this.HandlePageUpNavigation();
        }
      }
      else
      {
        if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
        {
          this.ScrollToTop();
          this.ScrollToLeftEnd();

          //than handle new selection!
          if( navigationBehavior != NavigationBehavior.None )
          {
            this.HandlePageUpNavigation();
          }
        }
        else
        {
          if( pagingBehavior == PagingBehavior.TopToBottom )
          {
            this.ScrollToLeftEnd();
          }
          else
          {
            this.ScrollToTop();
          }
        }
      }

      e.Handled = true;
    }

    protected virtual void HandlePageEndKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      DataGridContext dataGridContext = null;

      if( e.OriginalSource != null )
      {
        dataGridContext = DataGridControl.GetDataGridContext( e.OriginalSource as DependencyObject );
      }
      else
      {
        dataGridContext = DataGridControl.GetDataGridContext( this );
      }

      if( dataGridContext == null )
        return;

      PagingBehavior pagingBehavior = dataGridContext.DataGridControl.PagingBehavior;
      NavigationBehavior navigationBehavior = dataGridContext.DataGridControl.NavigationBehavior;

      ColumnBase CurrentColumn = dataGridContext.CurrentColumn;

      if( ( ( navigationBehavior == NavigationBehavior.CellOnly ) || ( navigationBehavior == NavigationBehavior.RowOrCell ) ) && ( CurrentColumn != null ) )
      {
        int oldCurrentIndex = CurrentColumn.Index;

        NavigationHelper.MoveFocusToLastVisibleColumn( dataGridContext );

        //if the last focusable column is is within the viewport, scroll to Extended offset, otherwise, bringIntoView
        if( ( this.IsCellsOffsetNeedReset( dataGridContext ) ) && ( oldCurrentIndex == CurrentColumn.Index ) )
        {
          this.Dispatcher.BeginInvoke( DispatcherPriority.Normal, new Action( this.ScrollToRightEnd ) );
        }

        if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
        {
          if( pagingBehavior == PagingBehavior.TopToBottom )
          {
            this.ScrollToBottom();
          }
          else
          {
            this.ScrollToRightEnd();
          }

          this.HandlePageDownNavigation();
        }
      }
      else
      {
        if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
        {
          this.ScrollToBottom();
          this.ScrollToRightEnd();

          // Than handle new selection!
          if( navigationBehavior != NavigationBehavior.None )
          {
            this.HandlePageDownNavigation();
          }
        }
        else
        {
          if( pagingBehavior == PagingBehavior.TopToBottom )
          {
            this.ScrollToRightEnd();
          }
          else
          {
            this.ScrollToBottom();
          }
        }
      }

      e.Handled = true;
    }

    private bool IsCellsOffsetNeedReset( DataGridContext dataGridContext )
    {
      var columnVirtualizationManager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManagerBase;
      if( columnVirtualizationManager == null )
        return false;

      Cell targetCell = dataGridContext.CurrentCell;

      // if the targetCell is null, it means we are on a header or a non focusable item; thus we want to scroll as the "no navigation" behavior
      if( targetCell == null )
        return true;

      FixedCellPanel fixedCellPanel = targetCell.ParentRow.CellsHostPanel as FixedCellPanel;
      DataGridItemsHost itemsHost = dataGridContext.DataGridControl.ItemsHost as DataGridItemsHost;

      double viewportWidth = this.ViewportWidth;
      double fixedColumnWidth = columnVirtualizationManager.FixedColumnsWidth;
      double fixedColumnSplitterWidth = 0;
      double cellsWidth = targetCell.ParentColumn.Width;

      Point cellToScrollingCellsDecorator = targetCell.TranslatePoint( new Point(), fixedCellPanel.ScrollingCellsDecorator );

      bool leftEdgeOutOfViewPort;

      // verify if the cell's left edge is visible in the viewPort
      leftEdgeOutOfViewPort = ( cellToScrollingCellsDecorator.X < 0 ) ? true : false;

      // Verify if the Cell's right edge is visible in the ViewPort
      Point cellToItemsHost = targetCell.TranslatePoint( new Point( cellsWidth, 0 ), itemsHost );

      // If the right edge is out of Viewport, ensure not to call the resetHorizontalOffset
      bool rightEdgeOutOfViewPort = ( ( cellToItemsHost.X - viewportWidth ) > 0 ) ? true : false;

      bool resetHorizontalOffset = false;

      // if the cell is inside the viewPort or if one of the edges of the cell are outside the portView but the the viewPort is not wide enough to contain the cell
      if( ( ( !rightEdgeOutOfViewPort ) && ( !leftEdgeOutOfViewPort ) )
        || ( ( ( rightEdgeOutOfViewPort ) || ( leftEdgeOutOfViewPort ) )
        && ( ( cellsWidth - Math.Abs( cellToScrollingCellsDecorator.X ) ) >= ( viewportWidth - fixedColumnWidth - fixedColumnSplitterWidth ) ) ) )
      {
        resetHorizontalOffset = true;
      }

      return resetHorizontalOffset;
    }

    protected virtual void HandleUpKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      DataGridContext dataGridContext = null;

      if( e.OriginalSource != null )
      {
        dataGridContext = DataGridControl.GetDataGridContext( e.OriginalSource as DependencyObject );
      }
      else
      {
        dataGridContext = DataGridControl.GetDataGridContext( this );
      }

      if( dataGridContext == null )
        return;

      bool hasValidationError = dataGridContext.DataGridControl.HasValidationError;

      if( !hasValidationError )
      {
        this.LineUp();
      }

      e.Handled = true;
    }

    protected virtual void HandleDownKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      DataGridContext dataGridContext = null;

      if( e.OriginalSource != null )
      {
        dataGridContext = DataGridControl.GetDataGridContext( e.OriginalSource as DependencyObject );
      }
      else
      {
        dataGridContext = DataGridControl.GetDataGridContext( this );
      }

      if( dataGridContext == null )
        return;

      bool hasValidationError = dataGridContext.DataGridControl.HasValidationError;

      if( !hasValidationError )
      {
        this.LineDown();
      }

      e.Handled = true;
    }

    protected virtual void HandleLeftKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      DataGridContext dataGridContext = null;

      if( e.OriginalSource != null )
      {
        dataGridContext = DataGridControl.GetDataGridContext( e.OriginalSource as DependencyObject );
      }
      else
      {
        dataGridContext = DataGridControl.GetDataGridContext( this );
      }

      if( dataGridContext == null )
        return;

      FlowDirection flowDirection = dataGridContext.DataGridControl.FlowDirection;
      bool hasValidationError = dataGridContext.DataGridControl.HasValidationError;

      if( !hasValidationError )
      {
        if( flowDirection == FlowDirection.LeftToRight )
        {
          this.LineLeft();
        }
        else
        {
          this.LineRight();
        }
      }

      e.Handled = true;
    }

    protected virtual void HandleRightKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      DataGridContext dataGridContext = null;

      if( e.OriginalSource != null )
      {
        dataGridContext = DataGridControl.GetDataGridContext( e.OriginalSource as DependencyObject );
      }
      else
      {
        dataGridContext = DataGridControl.GetDataGridContext( this );
      }

      if( dataGridContext == null )
        return;

      FlowDirection flowDirection = dataGridContext.DataGridControl.FlowDirection;
      bool hasValidationError = dataGridContext.DataGridControl.HasValidationError;

      if( !hasValidationError )
      {
        if( flowDirection == FlowDirection.LeftToRight )
        {
          this.LineRight();
        }
        else
        {
          this.LineLeft();
        }
      }

      e.Handled = true;
    }

    protected override void OnScrollChanged( ScrollChangedEventArgs e )
    {
      base.OnScrollChanged( e );

      this.UpdateChildScrollViewers();
    }

    protected override Size MeasureOverride( Size constraint )
    {
      unchecked
      {
        m_measureVersion++;
      }

      return base.MeasureOverride( constraint );
    }

    private static void OnParentDataGridChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridScrollViewer scrollViewer = sender as DataGridScrollViewer;
      if( scrollViewer != null )
      {
        if( e.NewValue == null )
        {
          scrollViewer.ClearScrollViewerList();
        }
      }
    }

    private void OnLoaded( object sender, RoutedEventArgs e )
    {
      //Placing the "Populate" call here is redundant from the OnApplyTemplate() call, but still required to prevent any possible case
      //where the DataGridScrollViewer is removed/added from/to its visual parent without having its template re-applied.
      //the if statement is there to ensure that we are not doing the job 2 times 
      //(if the list if not empty in the loaded event, its because OnApplyTemplate was called)
      if( m_childScrollViewers.Count == 0 )
      {
        this.PopulateScrollViewerList( this, m_childScrollViewers );
      }
    }

    private void OnUnloaded( object sender, RoutedEventArgs e )
    {
      //when the ScrollViewer gets unloaded, clear the events (we don't have other hooks that indicate the ScrollViewer 
      //is deconnected from the DAtaGridControl's Template)
      this.ClearScrollViewerList();
    }

    private void DragScrollBegin( Orientation orientation )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return;

      DataGridControl parentGridControl = dataGridContext.DataGridControl;

      if( parentGridControl == null || dataGridContext.DataGridControl.ScrollViewer == null )
        return;

      if( parentGridControl.ItemScrollingBehavior == ItemScrollingBehavior.Deferred )
      {
        IDeferableScrollInfoRefresh panel = this.ScrollInfo as IDeferableScrollInfoRefresh;

        if( panel != null )
        {
          if( m_deferScrollInfoRefresh != null )
          {
            foreach( IDeferableScrollChanged deferable in m_deferableChilds )
            {
              deferable.DeferScrollChanged = false;
            }

            m_deferScrollInfoRefresh.Dispose();
            m_deferScrollInfoRefresh = null;
          }

          m_deferScrollInfoRefresh = panel.DeferScrollInfoRefresh( orientation );

          if( m_deferScrollInfoRefresh != null )
          {
            foreach( IDeferableScrollChanged deferable in m_deferableChilds )
            {
              deferable.DeferScrollChanged = true;
            }
          }
        }
      }
    }

    private void DragScrollEnd( Orientation orientation )
    {
      if( m_deferScrollInfoRefresh != null )
      {
        foreach( IDeferableScrollChanged deferable in m_deferableChilds )
        {
          deferable.DeferScrollChanged = false;
        }

        m_deferScrollInfoRefresh.Dispose();
        m_deferScrollInfoRefresh = null;
      }
    }

    private void ClearScrollViewerList()
    {
      BindingOperations.ClearBinding( this, DataGridScrollViewer.SynchronizedScrollViewerExtentProperty );
      m_childScrollViewers.Clear();
    }

    private void PopulateScrollViewerList( DependencyObject reference, List<SynchronizedScrollViewer> list )
    {
      MultiBinding multiBinding = new MultiBinding();
      multiBinding.Converter = new SynchronizedScrollViewerMultiConverter();

      this.PopulateScrollViewerListHelper( reference, list, multiBinding );

      BindingOperations.SetBinding( this, DataGridScrollViewer.SynchronizedScrollViewerExtentProperty, multiBinding );
    }

    private void PopulateScrollViewerListHelper( DependencyObject reference, List<SynchronizedScrollViewer> list, MultiBinding multiBinding )
    {
      if( reference == null )
        return;

      var childCount = VisualTreeHelper.GetChildrenCount( reference );

      for( int i = 0; i < childCount; i++ )
      {
        var child = VisualTreeHelper.GetChild( reference, i );
        var synchronizedScrollViewer = child as SynchronizedScrollViewer;

        if( synchronizedScrollViewer != null )
        {
          var propertyName = String.Empty;

          if( synchronizedScrollViewer.ScrollOrientation == ScrollOrientation.Horizontal )
          {
            propertyName = "ExtentWidth";
          }
          else
          {
            propertyName = "ExtentHeight";
          }

          var binding = new Binding();
          binding.Path = new PropertyPath( propertyName );
          binding.Mode = BindingMode.OneWay;
          binding.Source = synchronizedScrollViewer;

          multiBinding.Bindings.Add( binding );

          list.Add( synchronizedScrollViewer );
        }
        else
        {
          this.PopulateScrollViewerListHelper( child, list, multiBinding );
        }
      }
    }

    private void FindAllDeferableChildren( DependencyObject reference, List<IDeferableScrollChanged> list )
    {
      if( reference == null )
        return;

      var childCount = VisualTreeHelper.GetChildrenCount( reference );

      for( int i = 0; i < childCount; i++ )
      {
        var child = VisualTreeHelper.GetChild( reference, i );
        var deferable = child as IDeferableScrollChanged;

        if( deferable != null )
        {
          list.Add( deferable );
        }
        else
        {
          this.FindAllDeferableChildren( child, list );
        }
      }
    }

    private void UpdateChildScrollViewers()
    {
      bool isAtTop = ( this.VerticalOffset == 0 );
      bool isAtLeft = ( this.HorizontalOffset == 0 );
      bool isAtBottom = ( ( this.VerticalOffset + this.ViewportHeight ) == this.ExtentHeight );
      bool isAtRight = ( ( this.HorizontalOffset + this.ViewportWidth ) == this.ExtentWidth );

      foreach( SynchronizedScrollViewer scrollViewer in m_childScrollViewers )
      {
        SynchronizedScrollViewerPosition position = TableViewScrollViewer.GetSynchronizedScrollViewerPosition( scrollViewer );

        bool handled = false;

        switch( position )
        {
          case SynchronizedScrollViewerPosition.Top:
            if( isAtTop == true )
            {
              KeyboardNavigation.SetDirectionalNavigation( scrollViewer, KeyboardNavigationMode.Continue );
              KeyboardNavigation.SetTabNavigation( scrollViewer, KeyboardNavigationMode.Continue );
              handled = true;
            }
            break;
          case SynchronizedScrollViewerPosition.Bottom:
            if( isAtBottom == true )
            {
              KeyboardNavigation.SetDirectionalNavigation( scrollViewer, KeyboardNavigationMode.Continue );
              KeyboardNavigation.SetTabNavigation( scrollViewer, KeyboardNavigationMode.Continue );
              handled = true;
            }
            break;
          case SynchronizedScrollViewerPosition.Left:
            if( isAtLeft == true )
            {
              KeyboardNavigation.SetDirectionalNavigation( scrollViewer, KeyboardNavigationMode.Continue );
              KeyboardNavigation.SetTabNavigation( scrollViewer, KeyboardNavigationMode.Continue );
              handled = true;
            }
            break;
          case SynchronizedScrollViewerPosition.Right:
            if( isAtRight == true )
            {
              KeyboardNavigation.SetDirectionalNavigation( scrollViewer, KeyboardNavigationMode.Continue );
              KeyboardNavigation.SetTabNavigation( scrollViewer, KeyboardNavigationMode.Continue );
              handled = true;
            }
            break;

          case SynchronizedScrollViewerPosition.None:
          default:
            break;
        }

        if( handled == false )
        {
          KeyboardNavigation.SetDirectionalNavigation( scrollViewer, KeyboardNavigationMode.None );
          KeyboardNavigation.SetTabNavigation( scrollViewer, KeyboardNavigationMode.None );
        }
      }
    }

    private List<SynchronizedScrollViewer> m_childScrollViewers = new List<SynchronizedScrollViewer>();
    private List<IDeferableScrollChanged> m_deferableChilds = new List<IDeferableScrollChanged>();

    ScrollViewerTemplateHelper m_templateHelper;
    IDisposable m_deferScrollInfoRefresh; // = null 
    int m_measureVersion;

    internal interface IDeferableScrollChanged
    {
      bool DeferScrollChanged
      {
        get;
        set;
      }
    }
  }
}
