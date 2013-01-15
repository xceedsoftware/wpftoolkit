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
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Utils.Wpf;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using Xceed.Wpf.DataGrid.Converters;
using Xceed.Utils.Math;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace Xceed.Wpf.DataGrid.Views
{
  public class DataGridScrollViewer : ScrollViewer
  {
    #region CONSTRUCTORS

    static DataGridScrollViewer()
    {
      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( DataGridScrollViewer ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnParentDataGridChanged ) ) );
    }

    internal DataGridScrollViewer()
    {
      this.Loaded += new RoutedEventHandler( OnLoaded );
      this.Unloaded += new RoutedEventHandler( OnUnloaded );
    }

    #endregion

    #region SynchronizedScrollViewerPosition Attached Property

    public static readonly DependencyProperty SynchronizedScrollViewerPositionProperty =
        DependencyProperty.RegisterAttached( "SynchronizedScrollViewerPosition", typeof( SynchronizedScrollViewerPosition ), typeof( TableViewScrollViewer ), new UIPropertyMetadata( SynchronizedScrollViewerPosition.None ) );

    public static SynchronizedScrollViewerPosition GetSynchronizedScrollViewerPosition( DependencyObject obj )
    {
      return ( SynchronizedScrollViewerPosition )obj.GetValue( TableViewScrollViewer.SynchronizedScrollViewerPositionProperty );
    }

    public static void SetSynchronizedScrollViewerPosition( DependencyObject obj, SynchronizedScrollViewerPosition value )
    {
      obj.SetValue( TableViewScrollViewer.SynchronizedScrollViewerPositionProperty, value );
    }

    #endregion SynchronizedScrollViewerPosition Attached Property

    #region INTERNAL PROPERTIES

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

    internal IEnumerable<SynchronizedScrollViewer> SynchronizedScrollViewers
    {
      get
      {
        return m_childScrollViewers;
      }
    }

    private static readonly DependencyProperty SynchronizedScrollViewerExtentProperty =
        DependencyProperty.Register( "SynchronizedScrollViewerExtent", typeof( double ), typeof( DataGridScrollViewer ), new FrameworkPropertyMetadata( new PropertyChangedCallback( DataGridScrollViewer.OnSynchronizedScrollViewerExtentPropertyChanged ) ) );

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

    #region PROTECTED PROPERTIES

    protected internal ScrollBar HorizontalScrollBar
    {
      get
      {
        return m_horizontalScrollBar;
      }
    }

    protected internal ScrollBar VerticalScrollBar
    {
      get
      {
        return m_verticalScrollBar;
      }
    }

    #endregion

    #region PUBLIC METHODS

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      this.ClearScrollViewerList();
      m_deferableChilds.Clear();

      this.PopulateScrollViewerList( this, m_childScrollViewers );
      this.FindAllDeferableChildren( this, m_deferableChilds );

      m_horizontalScrollBar = this.GetTemplateChild( "PART_HorizontalScrollBar" ) as ScrollBar;
      m_verticalScrollBar = this.GetTemplateChild( "PART_VerticalScrollBar" ) as ScrollBar;

      this.UpdateChildScrollViewers();

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return;

      if( m_verticalScrollBar != null )
      {
        if( dataGridContext.DataGridControl.ScrollViewer != null )
        {
          // Assert the Template as been applied on the ScrollBar to get access to the ScrollThumb
          if( m_verticalScrollBar.Track == null )
            m_verticalScrollBar.ApplyTemplate();

          Debug.Assert( m_verticalScrollBar.Track != null );

          if( m_verticalScrollBar.Track != null )
            m_verticalScrollThumb = m_verticalScrollBar.Track.Thumb;

          if( m_verticalScrollThumb != null )
          {
            // Register to IsMouseCaptureChanged to know when this ScrollThumb is clicked to display the ScrollTip if required
            m_verticalScrollThumb.IsMouseCapturedChanged += new DependencyPropertyChangedEventHandler( this.ScrollThumb_IsMouseCapturedChanged );
          }
        }
      }

      if( m_horizontalScrollBar != null )
      {
        if( dataGridContext.DataGridControl.ScrollViewer != null )
        {
          // Assert the Template as been applied on the ScrollBar to get access to the ScrollThumb
          if( m_horizontalScrollBar.Track == null )
            m_horizontalScrollBar.ApplyTemplate();

          Debug.Assert( m_horizontalScrollBar.Track != null );

          if( m_horizontalScrollBar.Track != null )
            m_horizontalScrollThumb = m_horizontalScrollBar.Track.Thumb;

          if( m_horizontalScrollThumb != null )
          {
            // Register to IsMouseCaptureChanged to know when this ScrollThumb is clicked to display the ScrollTip if required
            m_horizontalScrollThumb.IsMouseCapturedChanged += new DependencyPropertyChangedEventHandler( this.ScrollThumb_IsMouseCapturedChanged );
          }
        }
      }
    }

    #endregion

    #region INTERNAL METHODS

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

    #region GetVisibleColumn


    internal static int GetFirstVisibleFocusableColumnIndex( DataGridContext currentDataGridContext )
    {
      return DataGridScrollViewer.GetFirstVisibleFocusableColumnIndex( currentDataGridContext, null );
    }

    internal static int GetFirstVisibleFocusableColumnIndex( DataGridContext currentDataGridContext, Row newRow )
    {
      ReadOnlyColumnCollection visibleColumnsCollection = ( ReadOnlyColumnCollection )currentDataGridContext.VisibleColumns;

      if( visibleColumnsCollection == null )
        return -1;

      int firstVisiblefocusableColumnIndex;
      int visibleColumnsCount = visibleColumnsCollection.Count - 1;

      Row currentRow = ( currentDataGridContext.CurrentRow != null ) ? currentDataGridContext.CurrentRow : newRow;

      if( currentRow != null )
      {
        for( firstVisiblefocusableColumnIndex = 0; firstVisiblefocusableColumnIndex <= visibleColumnsCount; firstVisiblefocusableColumnIndex++ )
        {
          if( currentRow.Cells[ visibleColumnsCollection[ firstVisiblefocusableColumnIndex ] ].GetCalculatedCanBeCurrent() )
          {
            return firstVisiblefocusableColumnIndex;
          }
        }
      }

      return -1;
    }

    internal static int GetLastVisibleFocusableColumnIndex( DataGridContext currentDataGridContext )
    {
      ReadOnlyColumnCollection visibleColumnsCollection = ( ReadOnlyColumnCollection )currentDataGridContext.VisibleColumns;

      if( visibleColumnsCollection == null )
        return -1;

      int lastVisiblefocusableColumnIndex;
      int visibleColumnsCount = visibleColumnsCollection.Count - 1;

      Row currentRow = currentDataGridContext.CurrentRow;

      if( currentRow != null )
      {
        for( lastVisiblefocusableColumnIndex = visibleColumnsCount; lastVisiblefocusableColumnIndex >= 0; lastVisiblefocusableColumnIndex-- )
        {
          if( currentRow.Cells[ visibleColumnsCollection[ lastVisiblefocusableColumnIndex ] ].GetCalculatedCanBeCurrent() )
          {
            return lastVisiblefocusableColumnIndex;
          }
        }
      }
      return -1;
    }

    internal static int GetNextVisibleFocusableColumnIndex( DataGridContext currentDataGridContext )
    {
      ColumnBase currentColumn = currentDataGridContext.CurrentColumn;

      return DataGridScrollViewer.GetNextVisibleFocusableColumnIndex( currentDataGridContext, currentColumn );
    }

    internal static int GetNextVisibleFocusableColumnIndex( DataGridContext currentDataGridContext, ColumnBase currentColumn )
    {
      ReadOnlyColumnCollection visibleColumnsCollection = ( ReadOnlyColumnCollection )currentDataGridContext.VisibleColumns;

      if( visibleColumnsCollection == null )
        return -1;

      int visibleColumnsCollectionCount = visibleColumnsCollection.Count;

      Row currentRow = currentDataGridContext.CurrentRow;

      if( currentRow != null )
      {
        int currentColumnIndex = currentDataGridContext.VisibleColumns.IndexOf( currentColumn );
        int NextVisibleFocusableColumnIndex;

        for( NextVisibleFocusableColumnIndex = currentColumnIndex + 1; NextVisibleFocusableColumnIndex < visibleColumnsCollectionCount; NextVisibleFocusableColumnIndex++ )
        {
          if( currentRow.Cells[ visibleColumnsCollection[ NextVisibleFocusableColumnIndex ] ].GetCalculatedCanBeCurrent() )
          {
            return NextVisibleFocusableColumnIndex;
          }
        }
      }

      return -1;
    }

    internal static int GetPreviousVisibleFocusableColumnIndex( DataGridContext currentDataGridContext, ColumnBase currentColumn )
    {
      ReadOnlyColumnCollection visibleColumnsCollection = ( ReadOnlyColumnCollection )currentDataGridContext.VisibleColumns;
      int visibleColumnsCollectionCount = visibleColumnsCollection.Count;

      if( currentColumn != null )
      {
        Row currentRow = currentDataGridContext.CurrentRow;

        int currentColumnIndex = currentDataGridContext.VisibleColumns.IndexOf( currentColumn );
        int previousVisibleFocusableColumnIndex;

        for( previousVisibleFocusableColumnIndex = currentColumnIndex - 1; previousVisibleFocusableColumnIndex >= 0; previousVisibleFocusableColumnIndex-- )
        {
          if( currentRow.Cells[ visibleColumnsCollection[ previousVisibleFocusableColumnIndex ] ].GetCalculatedCanBeCurrent() )
          {
            return previousVisibleFocusableColumnIndex;
          }
        }
      }
      return DataGridScrollViewer.GetLastVisibleFocusableColumnIndex( currentDataGridContext );
    }

    internal static int GetPreviousVisibleFocusableColumnIndex( DataGridContext currentDataGridContext )
    {
      ColumnBase currentColumn = currentDataGridContext.CurrentColumn;

      return DataGridScrollViewer.GetPreviousVisibleFocusableColumnIndex( currentDataGridContext, currentColumn );
    }

    #endregion GetVisibleColumn

    internal static void ProcessHomeKey( DataGridContext dataGridContext )
    {
      ReadOnlyColumnCollection visibleColumnsCollection = ( ReadOnlyColumnCollection )dataGridContext.VisibleColumns;

      int visibleColumnsCount = visibleColumnsCollection.Count;

      // Set the CurrentColumn only if there are VisibleColumns
      if( visibleColumnsCount > 0 )
      {
        int firstVisiblefocusableColumnIndex = DataGridScrollViewer.GetFirstVisibleFocusableColumnIndex( dataGridContext );

        if( firstVisiblefocusableColumnIndex >= 0 )
        {
          try
          {
            dataGridContext.SetCurrentColumnAndChangeSelection( visibleColumnsCollection[ firstVisiblefocusableColumnIndex ] );
          }
          catch( DataGridException )
          {
            // We swallow the exception if it occurs because of a validation error or Cell was read-only or
            // any other GridException.
          }
        }
      }
    }

    internal static void ProcessEndKey( DataGridContext dataGridContext )
    {
      ReadOnlyColumnCollection visibleColumnsCollection = ( ReadOnlyColumnCollection )dataGridContext.VisibleColumns;
      int visibleColumnsCount = visibleColumnsCollection.Count;

      // Set the CurrentColumn only if there are VisibleColumns
      if( visibleColumnsCount > 0 )
      {
        int lastVisiblefocusableColumnIndex = DataGridScrollViewer.GetLastVisibleFocusableColumnIndex( dataGridContext );

        if( lastVisiblefocusableColumnIndex >= 0 )
        {
          try
          {
            dataGridContext.SetCurrentColumnAndChangeSelection( visibleColumnsCollection[ lastVisiblefocusableColumnIndex ] );
          }
          catch( DataGridException )
          {
            // We swallow the exception if it occurs because of a validation error or Cell was read-only or
            // any other GridException.
          }
        }
      }
    }

    #endregion

    #region PROTECTED METHODS

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

      if( ( ( navigationBehavior == NavigationBehavior.CellOnly )
        || ( navigationBehavior == NavigationBehavior.RowOrCell ) )
        && ( currentColumn != null ) )
      {

        int oldCurrentIndex = currentColumn.Index;

        DataGridScrollViewer.ProcessHomeKey( dataGridContext );
        TableViewColumnVirtualizationManager columnVirtualizationManager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;

        //if the first focusable column is is within the viewport, scroll so 0d offset, otherwise, bringIntoView
        bool isFixedColumn = ( columnVirtualizationManager == null ) ?
          false : columnVirtualizationManager.FixedFieldNames.Contains( currentColumn.FieldName );

        if( ( ( this.IsCellsOffsetNeedReset( dataGridContext ) )
          && ( oldCurrentIndex == currentColumn.Index ) )
          || ( isFixedColumn ) )
          this.Dispatcher.BeginInvoke( DispatcherPriority.Normal, new Action( this.ScrollToLeftEnd ) );

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
            this.HandlePageUpNavigation();
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

      if( ( ( navigationBehavior == NavigationBehavior.CellOnly )
        || ( navigationBehavior == NavigationBehavior.RowOrCell ) )
        && ( CurrentColumn != null ) )
      {
        int oldCurrentIndex = CurrentColumn.Index;

        DataGridScrollViewer.ProcessEndKey( dataGridContext );

        //if the last focusable column is is within the viewport, scroll to Extended offset, otherwise, bringIntoView

        if( ( this.IsCellsOffsetNeedReset( dataGridContext ) ) && ( oldCurrentIndex == CurrentColumn.Index ) )
          this.Dispatcher.BeginInvoke( DispatcherPriority.Normal, new Action( this.ScrollToRightEnd ) );

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
            this.HandlePageDownNavigation();
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
      TableViewColumnVirtualizationManager columnVirtualizationManager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;

      if( columnVirtualizationManager == null )
        return false;

      Cell targetCell = dataGridContext.CurrentCell;

      // if the targetCell is null, it means we are on a header or.. (a non focusable item)
      // we want to scroll as the "no navigation" behavior
      if( targetCell == null )
        return true;

      FixedCellPanel fixedCellPanel = targetCell.ParentRow.CellsHostPanel as FixedCellPanel;
      DataGridItemsHost itemsHost = dataGridContext.DataGridControl.ItemsHost as DataGridItemsHost;

      double viewportWidth = this.ViewportWidth;
      double fixedColumnWidth = columnVirtualizationManager.FixedColumnsWidth;
      double cellsWidth = targetCell.ParentColumn.Width;

      Point cellToScrollingCellsDecorator = targetCell.TranslatePoint( new Point(), fixedCellPanel.ScrollingCellsDecorator );

      bool leftEdgeOutOfViewPort;

      // verify if the cell's left edge is visible in the viewPort
      leftEdgeOutOfViewPort = ( cellToScrollingCellsDecorator.X < 0 ) ? true : false;

      // Verify if the Cell's right edge is visible in the ViewPort
      Point cellToItemsHost = targetCell.TranslatePoint( new Point( cellsWidth, 0 ),
        itemsHost );

      // If the right edge is out of Viewport, ensure not to call the resetHorizontalOffset
      bool rightEdgeOutOfViewPort = ( ( cellToItemsHost.X - viewportWidth ) > 0 ) ? true : false;

      bool resetHorizontalOffset = false;

      // if the cell is inside the viewPort or if one of the edges of the cell are outside the portView but the
      // the viewPort is not wide enough to contain the cell
      if( ( ( !rightEdgeOutOfViewPort ) && ( !leftEdgeOutOfViewPort ) )
        || ( ( ( rightEdgeOutOfViewPort )
        || ( leftEdgeOutOfViewPort ) )
        && ( ( cellsWidth - Math.Abs( cellToScrollingCellsDecorator.X ) ) >= ( viewportWidth - fixedColumnWidth ) ) ) )
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

    #endregion

    #region PRIVATE METHODS

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

    private void ScrollThumb_IsMouseCapturedChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      Thumb scrollThumb = sender as Thumb;

      if( scrollThumb == null )
        return;

      if( scrollThumb.IsMouseCaptured == false )
        return;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return;

      // Register to LostMouseCapture to be notified when to DeferScroll
      if( scrollThumb == m_horizontalScrollThumb )
      {
        m_horizontalScrollThumb.LostMouseCapture += new MouseEventHandler( this.ScrollThumb_LostMouseCapture );
      }
      else if( scrollThumb == m_verticalScrollThumb )
      {
        m_verticalScrollThumb.LostMouseCapture += new MouseEventHandler( this.ScrollThumb_LostMouseCapture );
      }
      else
      {
        Debug.Fail( "Unknown thumb used for scrolling." );
        return;
      }

      DataGridControl parentGridControl = dataGridContext.DataGridControl;

      if( ( parentGridControl != null ) && ( parentGridControl.ItemScrollingBehavior == ItemScrollingBehavior.Deferred ) )
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

          ScrollBar scrollBar = scrollThumb.TemplatedParent as ScrollBar;

          if( scrollBar == m_horizontalScrollBar )
          {
            m_deferScrollInfoRefresh = panel.DeferScrollInfoRefresh( Orientation.Horizontal );
          }
          else if( scrollBar == m_verticalScrollBar )
          {
            m_deferScrollInfoRefresh = panel.DeferScrollInfoRefresh( Orientation.Vertical );
          }
          else
          {
            Debug.Fail( "Unknown scroll bar used for DeferredScrolling." );
            return;
          }

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

    private void ScrollThumb_LostMouseCapture( object sender, MouseEventArgs e )
    {
      Thumb scrollBarThumb = sender as Thumb;

      Debug.Assert( scrollBarThumb != null );

      if( scrollBarThumb != null )
        scrollBarThumb.LostMouseCapture -= new MouseEventHandler( this.ScrollThumb_LostMouseCapture );

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
      int childCount = VisualTreeHelper.GetChildrenCount( reference );

      for( int i = 0; i < childCount; i++ )
      {
        DependencyObject child = VisualTreeHelper.GetChild( reference, i );

        SynchronizedScrollViewer synchronizedScrollViewer = child as SynchronizedScrollViewer;

        if( synchronizedScrollViewer != null )
        {
          string propertyName = String.Empty;

          if( synchronizedScrollViewer.ScrollOrientation == ScrollOrientation.Horizontal )
          {
            propertyName = "ExtentWidth";
          }
          else
          {
            propertyName = "ExtentHeight";
          }

          Binding binding = new Binding();
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
      int childCount = VisualTreeHelper.GetChildrenCount( reference );

      for( int i = 0; i < childCount; i++ )
      {
        DependencyObject child = VisualTreeHelper.GetChild( reference, i );

        IDeferableScrollChanged deferable = child as IDeferableScrollChanged;

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

    #endregion

    #region PRIVATE FIELDS

    private List<SynchronizedScrollViewer> m_childScrollViewers = new List<SynchronizedScrollViewer>();
    private List<IDeferableScrollChanged> m_deferableChilds = new List<IDeferableScrollChanged>();

    ScrollBar m_horizontalScrollBar; // = null
    Thumb m_horizontalScrollThumb; // = null
    ScrollBar m_verticalScrollBar; // = null
    Thumb m_verticalScrollThumb; // = null
    IDisposable m_deferScrollInfoRefresh; // = null 
    int m_measureVersion;

    #endregion

    #region INTERNAL INTERFACE IDeferableScrollChanged

    internal interface IDeferableScrollChanged
    {
      bool DeferScrollChanged
      {
        get;
        set;
      }
    }

    #endregion
  }
}
