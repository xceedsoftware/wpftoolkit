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
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using Xceed.Wpf.DataGrid;
using System.Windows.Navigation;
using System.Windows.Controls.Primitives;

namespace Xceed.Utils.Wpf.DragDrop
{
  internal class DragSourceManager : DependencyObject, INotifyPropertyChanged
  {
    public DragSourceManager(
      UIElement draggedElement,
      AdornerLayer adornerLayerInsideDragContainer,
      UIElement dragContainer )
      : this( draggedElement,
              adornerLayerInsideDragContainer,
              dragContainer,
              true )
    {
    }

    public DragSourceManager(
      UIElement draggedElement,
      AdornerLayer adornerLayerInsideDragContainer,
      UIElement dragContainer,
      bool enableAutoScroll )
      : this( draggedElement,
              adornerLayerInsideDragContainer,
              dragContainer,
              enableAutoScroll,
              true )
    {
    }

    public DragSourceManager(
      UIElement draggedElement,
      AdornerLayer adornerLayerInsideDragContainer,
      UIElement dragContainer,
      bool enableAutoScroll,
      bool showDraggedElementGhost )
    {
      if( draggedElement == null )
        throw new ArgumentNullException( "draggedElement" );

      if( dragContainer == null )
        throw new ArgumentNullException( "dragContainer" );

      this.SetDraggedElement( draggedElement );
      this.DragContainer = dragContainer;
      this.ShowDraggedElementGhost = showDraggedElementGhost;

      // If the AdornerLayer was not provided to the DragSourceManager
      if( adornerLayerInsideDragContainer == null )
      {
        // Get the first AdornerLayer up the VisualTree of the dragContainer
        this.AdornerLayerInsideDragContainer = AdornerLayer.GetAdornerLayer( this.DragContainer );
      }
      else
      {
        this.AdornerLayerInsideDragContainer = adornerLayerInsideDragContainer;
      }

      if( enableAutoScroll )
      {
        DependencyObject element = draggedElement;
        ScrollViewer scrollViewer = null;

        while( element != null )
        {
          element = VisualTreeHelper.GetParent( element );
          scrollViewer = element as ScrollViewer;

          if( scrollViewer != null )
            break;
        }

        m_parentScrollViewer = scrollViewer;
        m_timer = new System.Windows.Threading.DispatcherTimer();
        m_timer.Interval = new TimeSpan( 0, 0, 0, 0, this.TimerInterval );
        m_timer.Tick += new EventHandler( this.OnAutoScrollTimer_Tick );
      }
    }

    #region AdornerLayerInsideDragContainer Read-Only Public Property

    public AdornerLayer AdornerLayerInsideDragContainer
    {
      get;
      private set;
    }

    #endregion

    #region DraggedElement Read-Only Public Property

    public UIElement DraggedElement
    {
      get;
      private set;
    }

    #endregion

    #region DraggedElementGhost Read-Only Internal Property

    internal Window DraggedElementGhost
    {
      get;
      private set;
    }

    #endregion

    #region DragContainer Read-Only Public Property

    public UIElement DragContainer
    {
      get;
      private set;
    }

    #endregion

    #region IsDragging Read-Only Public Property

    public bool IsDragging
    {
      get
      {
        return m_isDragging;
      }
      private set
      {
        if( m_isDragging == value )
          return;

        m_isDragging = value;

        this.NotifyPropertyChanged( "IsDragging" );
      }
    }

    private bool m_isDragging;

    #endregion

    #region InitialMousePositionToDraggedElement Read-Only Public Property

    internal Nullable<Point> InitialMousePositionToDraggedElement
    {
      get;
      private set;
    }

    #endregion

    #region InitialMousePositionToDragContainerAdorner Read-Only Public Property

    public Nullable<Point> InitialMousePositionToDragContainerAdorner
    {
      get;
      private set;
    }

    #endregion

    #region ShowDraggedElementGhost Public Property

    public bool ShowDraggedElementGhost
    {
      get;
      set;
    }

    #endregion

    #region CurrentDropTarget Protected Property

    protected IDropTarget CurrentDropTarget
    {
      get;
      set;
    }

    #endregion

    #region CurrentDropTargetPosition

    protected Nullable<Point> CurrentDropTargetToDragContainerPosition
    {
      get;
      set;
    }

    #endregion

    #region ParentScrollViewer Protected Property

    protected ScrollViewer ParentScrollViewer
    {
      get
      {
        return m_parentScrollViewer;
      }
    }

    #endregion

    #region AutoScrollInterval Property

    public static readonly DependencyProperty AutoScrollIntervalProperty = DependencyProperty.Register(
      "AutoScrollInterval",
      typeof( int ),
      typeof( DragSourceManager ),
      new FrameworkPropertyMetadata( 50 ) );

    public int AutoScrollInterval
    {
      get
      {
        return ( int )this.GetValue( DragSourceManager.AutoScrollIntervalProperty );
      }
      set
      {
        this.SetValue( DragSourceManager.AutoScrollIntervalProperty, value );
      }
    }

    #endregion

    #region TimerInterval Property

    internal int TimerInterval
    {
      get;
      set;
    }

    #endregion

    #region AutoScrollTreshold Property

    public static readonly DependencyProperty AutoScrollTresholdProperty = DependencyProperty.Register(
      "AutoScrollTreshold",
      typeof( int ),
      typeof( DragSourceManager ),
      new FrameworkPropertyMetadata( 5 ) );

    public int AutoScrollTreshold
    {
      get
      {
        return ( int )this.GetValue( DragSourceManager.AutoScrollTresholdProperty );
      }
      set
      {
        this.SetValue( DragSourceManager.AutoScrollTresholdProperty, value );
      }
    }

    #endregion

    #region DropOutsideCursor Property

    public Cursor DropOutsideCursor
    {
      get;
      set;
    }

    #endregion

    #region ParentIsPopup Property

    internal bool ParentWindowIsPopup
    {
      get;
      private set;
    }

    #endregion

    public event QueryCursorEventHandler DragOutsideQueryCursor;

    public event EventHandler DroppedOutside;

    public virtual void SetDraggedElement( UIElement newDraggedElement )
    {
      if( newDraggedElement != this.DraggedElement )
      {
        if( this.DraggedElement != null )
        {
          this.DraggedElement.QueryCursor -= this.OnDraggedElement_QueryCursor;
        }

        if( newDraggedElement != null )
        {
          this.DraggedElement = newDraggedElement;
          newDraggedElement.QueryCursor += this.OnDraggedElement_QueryCursor;
        }
      }
    }

    public virtual void ProcessMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      if( e.Handled )
        return;

#if DEBUG
      this.CheckMouseEventArgs( e );
#endif

      bool captured = this.DraggedElement.CaptureMouse();

      if( captured )
      {
        this.InitialMousePositionToDragContainerAdorner = e.GetPosition( this.AdornerLayerInsideDragContainer );

        this.InitialMousePositionToDraggedElement = e.GetPosition( this.DraggedElement );

        //Get the current window (which can be a popup) on which the drag is happening
        Visual parent = this.GetDraggedElementWindow();

        //Set the flag because the dragged ghost in the case of a popup is different, and return because DraggedElementGhost will not be used.
        if( parent is Popup )
        {
          this.ParentWindowIsPopup = true;
          return;
        }

        //Get the starting position of the gost window, so it can be used to correct the mouse position while dragging it.  
        Point ghostPostion = new Point( -this.InitialMousePositionToDraggedElement.Value.X, -this.InitialMousePositionToDraggedElement.Value.Y );
        if( parent != null )
        {
          m_ghostInitialScreenPositon = parent.PointToScreen( ghostPostion );
        }
        else
        {
          m_ghostInitialScreenPositon = ghostPostion;
        }
      }
      else
      {
        Debug.Fail( "The DragSourceManager was unable to capture the mouse on the dragged element." );
      }
    }

    public virtual void ProcessPreviewMouseRightButtonDown( MouseButtonEventArgs e )
    {
    }

    public virtual void ProcessMouseMove( MouseEventArgs e )
    {
      if( e.Handled )
        return;

#if DEBUG
      this.CheckMouseEventArgs( e );
#endif

      if( ( this.InitialMousePositionToDragContainerAdorner.HasValue )
          && ( e.LeftButton == MouseButtonState.Pressed ) )
      {
        if( !this.IsDragging )
        {
          Point currentPosition = e.GetPosition( this.AdornerLayerInsideDragContainer );

          if( DragDropHelper.IsMouseMoveDrag( this.InitialMousePositionToDraggedElement.Value, currentPosition ) )
          {
            this.BeginDrag();
          }
        }

        if( this.IsDragging )
        {
          this.Drag( e );
        }
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e" )]
    public virtual void ProcessMouseLeftButtonUp( MouseButtonEventArgs e )
    {
#if DEBUG
      this.CheckMouseEventArgs( e );
#endif
      this.EndDrag( true );
      this.DraggedElement.ReleaseMouseCapture();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e" )]
    public virtual void ProcessLostMouseCapture( MouseEventArgs e )
    {
#if DEBUG
      this.CheckMouseEventArgs( e );
#endif
      if( this.IsDragging )
      {
        this.EndDrag( false );
      }
    }

    protected virtual void UpdateDraggedElementGhostOnDrag( MouseEventArgs e )
    {
      //DraggedElementGhost is not used in the case of a popup window.
      if( this.ParentWindowIsPopup )
        return;

      Window ghost = this.DraggedElementGhost;

      if( ghost == null )
        return;

      if( this.ShowDraggedElementGhost && !ghost.IsVisible )
      {
        ghost.Show();
      }
      else if( !this.ShowDraggedElementGhost && ghost.IsVisible )
      {
        ghost.Hide();
      }

      //Get the current position of the mouse
      Point currentMousePosition = e.GetPosition( null );

      //And set the ghost position by correcting the mouse position.
      ghost.Left = currentMousePosition.X + m_ghostInitialScreenPositon.X;
      ghost.Top = currentMousePosition.Y + m_ghostInitialScreenPositon.Y;
    }

    protected virtual IDropTarget GetDropTargetOnDrag( MouseEventArgs e, out Nullable<Point> dropTargetPosition, out IDropTarget lastFoundDropTarget )
    {
      IDropTarget dropTarget = DragDropHelper.GetDropTargetAtPoint( this.DraggedElement, this.DragContainer, e, out dropTargetPosition, out lastFoundDropTarget );

      // ColumnManagerRow was defined as IDropTarget only because Animated Column Reordering required it, ignore it in base class
      if( dropTarget is ColumnManagerRow )
      {
        dropTarget = null;
        dropTargetPosition = null;
      }

      return dropTarget;
    }

    protected virtual void ProcessDragOnCurrentDropTarget( IDropTarget dropTarget )
    {
      if( dropTarget != this.CurrentDropTarget )
      {
        if( this.CurrentDropTarget != null )
        {
          this.CurrentDropTarget.DragLeave( this.DraggedElement );
          this.CurrentDropTarget = null;
        }

        if( dropTarget != null )
        {
          if( dropTarget.CanDropElement( this.DraggedElement ) )
          {
            this.CurrentDropTarget = dropTarget;
            this.CurrentDropTarget.DragEnter( this.DraggedElement );
          }
        }
      }

      if( this.CurrentDropTarget != null )
      {
        // Always use the Mouse Position relative to DraggedElement and not the DraggedContainer
        this.CurrentDropTarget.DragOver( this.DraggedElement, Mouse.GetPosition( this.CurrentDropTarget as IInputElement ) );
      }
    }

    protected virtual void ProcessCurrentDropTargetDrop( bool wasDragging, bool drop )
    {
      if( this.CurrentDropTarget != null )
      {
        if( drop )
        {
          this.CurrentDropTarget.Drop( this.DraggedElement );
        }
        else
        {
          this.CurrentDropTarget.DragLeave( this.DraggedElement );
        }

        this.CurrentDropTarget = null;
      }
      else if( wasDragging )
      {
        this.NotifyDropOuside();
      }
    }

    protected virtual void StopAutoScroll()
    {
      if( m_timer != null )
      {
        m_timer.Stop();
      }
    }

    protected virtual void PerformAutoScroll()
    {
      if( ( m_timer != null ) && ( m_timer.IsEnabled ) )
      {
        TimeSpan timeSpanSinceLastScroll = ( TimeSpan )( DateTime.Now - m_lastAutoScrollTime );

        // This method may be called before its time (on the MouseMove event). We make
        // sure that the AutoScroll is not performed before the desired time span has elapsed.
        if( timeSpanSinceLastScroll.Milliseconds >= AutoScrollInterval )
        {
          double scrollOffset = m_parentScrollViewer.HorizontalOffset + m_scrollAmount;

          if( ( m_autoScrollDirection & AutoScrollDirection.Left ) == AutoScrollDirection.Left )
          {
            if( m_scrollPage )
            {
              m_parentScrollViewer.PageLeft();
            }
            else
            {
              //Make sure the grid stops scrolling.
              if( scrollOffset < 0 )
              {
                scrollOffset = 0;
              }
              m_parentScrollViewer.ScrollToHorizontalOffset( scrollOffset );
            }
          }
          else if( ( m_autoScrollDirection & AutoScrollDirection.Right ) == AutoScrollDirection.Right )
          {
            if( m_scrollPage )
            {
              m_parentScrollViewer.PageRight();
            }
            else
            {
              //Make sure the grid does not scroll pass the last right column.
              if( scrollOffset > m_maxHorizontalScrollOffset )
              {
                scrollOffset = m_maxHorizontalScrollOffset;
              }
              m_parentScrollViewer.ScrollToHorizontalOffset( scrollOffset );
            }
          }
          else if( ( m_autoScrollDirection & AutoScrollDirection.Up ) == AutoScrollDirection.Up )
          {
            m_parentScrollViewer.LineUp();
          }
          else if( ( m_autoScrollDirection & AutoScrollDirection.Down ) == AutoScrollDirection.Down )
          {
            m_parentScrollViewer.LineDown();
          }

          m_lastAutoScrollTime = DateTime.Now;
        }
      }
    }

    protected virtual void NotifyDragOutsideQueryCursor( object sender, QueryCursorEventArgs e )
    {
      e.Cursor = ( this.DropOutsideCursor != null )
        ? this.DropOutsideCursor
        : Cursors.No;

      if( this.DragOutsideQueryCursor != null )
      {
        this.DragOutsideQueryCursor( sender, e );
      }

      e.Handled = true;
    }

    protected virtual void NotifyDropOuside()
    {
      if( this.DroppedOutside != null )
      {
        this.DroppedOutside( this, EventArgs.Empty );
      }
    }

    protected virtual void BeginDrag()
    {
      this.IsDragging = true;

      this.DraggedElementGhost = this.CreateDraggedElementGhost( this.DraggedElement );
    }

    protected virtual Window CreateDraggedElementGhost( UIElement draggedElement )
    {
      VisualBrush brush = new VisualBrush( draggedElement );
      brush.Opacity = 0.75;
      brush.Stretch = Stretch.None;
      brush.AlignmentX = AlignmentX.Left;
      brush.AlignmentY = AlignmentY.Top;

      Rectangle rectangle = new Rectangle();
      rectangle.Width = draggedElement.DesiredSize.Width;
      rectangle.Height = draggedElement.DesiredSize.Height;
      rectangle.Fill = brush;

      Window draggedWindow = new Window();
      //Make sure it stay on top of all other windows.
      draggedWindow.Topmost = true;
      //This will hide borders
      draggedWindow.WindowStyle = WindowStyle.None;
      //This will hide everything except what is explicitly set as content, that is, the rectangle defined up here.
      draggedWindow.AllowsTransparency = true;
      draggedWindow.Background = null;
      draggedWindow.SizeToContent = SizeToContent.WidthAndHeight;
      draggedWindow.ShowInTaskbar = false;
      draggedWindow.IsHitTestVisible = false;
      draggedWindow.ShowActivated = false;
      draggedWindow.Focusable = false;
      draggedWindow.Left = 0;
      draggedWindow.Top = 0;
      draggedWindow.Content = rectangle;
      //This will make sure the window follows what has been set on the parent grid.
      DataGridContext context = DataGridControl.GetDataGridContext( draggedElement );
      if( context != null )
      {
        draggedWindow.FlowDirection = context.DataGridControl.FlowDirection;
      }

      return draggedWindow;
    }

    protected virtual void Drag( MouseEventArgs e )
    {
      Nullable<Point> currentDropTarget = null;
      IDropTarget lastFoundDropTarget = null;
      IDropTarget dropTarget = this.GetDropTargetOnDrag( e, out currentDropTarget, out lastFoundDropTarget );

      this.CurrentDropTargetToDragContainerPosition = currentDropTarget;

      this.ProcessDragOnCurrentDropTarget( dropTarget );

      // Call this method after the drag is processed in case
      // the ShowDraggedElementGhost property was modified by the GetDropTargetOnDrag
      this.UpdateDraggedElementGhostOnDrag( e );

      if( m_parentScrollViewer != null )
      {
        this.CheckForAutoScroll( e.GetPosition( m_parentScrollViewer ) );
      }
    }

    protected virtual void EndDrag( bool drop )
    {
      bool wasDragging = this.IsDragging;

      this.IsDragging = false;
      this.InitialMousePositionToDragContainerAdorner = null;
      this.InitialMousePositionToDraggedElement = null;
      this.StopAutoScroll();

      this.ProcessCurrentDropTargetDrop( wasDragging, drop );

      //Hide the ghost only after doing the drop, so it will be visible if the operation takes a while.
      if( this.DraggedElementGhost != null )
      {
        this.DraggedElementGhost.Hide();
        this.DraggedElementGhost.Content = null;
        this.DraggedElementGhost.Close();
        this.DraggedElementGhost = null;
      }
    }

    private Visual GetDraggedElementWindow()
    {
      DependencyObject current = this.DraggedElement;
      while( current != null )
      {
        Popup popup = current as Popup;

        //If the grid is in a popup, this becomes the owning window, so return it.
        if( popup != null )
          return popup;

        Window window = current as Window;

        if( window != null )
          return window;

        current = TreeHelper.GetParent( current );
      }

      // The dragged element's window was not found.
      return null;
    }

    private void CheckForAutoScroll( Point clientMousePosition )
    {
      if( m_timer == null )
        return; // AutoScroll is not active

      Size scrollViewerRenderSize = m_parentScrollViewer.RenderSize;

      m_autoScrollDirection = AutoScrollDirection.None;
      if( m_parentScrollViewer.ScrollableWidth > 0 )
      {
        //Calculate the right edge maximum scroll offset
        m_maxHorizontalScrollOffset = m_parentScrollViewer.ExtentWidth - scrollViewerRenderSize.Width;

        //Scrolling left
        if( ( clientMousePosition.X < AutoScrollTreshold ) &&
            ( m_parentScrollViewer.HorizontalOffset > 0 ) )
        {
          m_autoScrollDirection |= AutoScrollDirection.Left;
          m_scrollAmount = -1;

          if( clientMousePosition.X < -1 )
          {
            m_scrollAmount = clientMousePosition.X;
          }

          //Scroll to a maximum of one page at a time.
          if( m_scrollAmount < -scrollViewerRenderSize.Width )
          {
            m_scrollPage = true;
          }
          else
          {
            m_scrollPage = false;
          }
        }
        //Scrolling right
        else if( ( clientMousePosition.X > scrollViewerRenderSize.Width - AutoScrollTreshold )
                 && ( m_parentScrollViewer.HorizontalOffset < m_maxHorizontalScrollOffset ) )
        {
          m_autoScrollDirection |= AutoScrollDirection.Right;
          double rightEdgeMouseDelta = clientMousePosition.X - scrollViewerRenderSize.Width;
          m_scrollAmount = 1;

          if( rightEdgeMouseDelta > 1 )
          {
            m_scrollAmount = rightEdgeMouseDelta;
          }

          //Scroll to a maximum of one page at a time.
          if( m_scrollAmount > scrollViewerRenderSize.Width )
          {
            m_scrollPage = true;
          }
          else
          {
            m_scrollPage = false;
          }
        }
      }

      if( m_parentScrollViewer.ScrollableHeight > 0 )
      {
        if( ( clientMousePosition.Y < AutoScrollTreshold )
            && ( m_parentScrollViewer.VerticalOffset > 0 ) )
        {
          m_autoScrollDirection |= AutoScrollDirection.Up;
        }
        else if( ( clientMousePosition.Y > scrollViewerRenderSize.Height - AutoScrollTreshold )
            && ( m_parentScrollViewer.VerticalOffset < m_parentScrollViewer.ExtentHeight - scrollViewerRenderSize.Height ) )
        {
          m_autoScrollDirection |= AutoScrollDirection.Down;
        }
      }

      if( m_autoScrollDirection == AutoScrollDirection.None )
      {
        this.StopAutoScroll();
      }
      else
      {
        // The DispatcherTimer is not a priority event. The Tick event won't fire while 
        // the user moves the mouse pointer. That's why we manually call the 
        // PerformAutoScroll method (on each MouseMove, i.e. Drag). When the user doesn't 
        // move the mouse pointer, the timer will take over to do the AutoScroll.
        if( !m_timer.IsEnabled )
        {
          m_timer.Start();
        }

        this.PerformAutoScroll();
      }
    }

    private void OnAutoScrollTimer_Tick( object sender, EventArgs e )
    {
      this.PerformAutoScroll();
    }

    private void OnDraggedElement_QueryCursor( object sender, QueryCursorEventArgs e )
    {
      if( this.IsDragging )
      {
        if( ( this.CurrentDropTarget == null ) || ( !this.CurrentDropTarget.CanDropElement( this.DraggedElement ) ) )
        {
          this.NotifyDragOutsideQueryCursor( sender, e );
        }
      }
    }

#if DEBUG
    private void CheckMouseEventArgs( MouseEventArgs e )
    {
      UIElement sourceElement = e.Source as UIElement;

      while( ( sourceElement != null ) && ( sourceElement != this.DraggedElement ) )
      {
        sourceElement = VisualTreeHelper.GetParent( sourceElement ) as UIElement;
      }

      if( sourceElement == null )
        Debug.Fail( "The Source of the " + e.RoutedEvent.Name + " event is NOT the UIElement that was passed to the ctor of this DragSourceManager OR one of its children." );
    }
#endif

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected void NotifyPropertyChanged( string propertyName )
    {
      if( string.IsNullOrEmpty( propertyName ) )
        throw new ArgumentException( "The property name cannot be null (Nothing in Visual Basic) or an empty string.", "propertyName" );

      if( this.PropertyChanged != null )
      {
        this.PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
      }
    }

    #endregion

    private AutoScrollDirection m_autoScrollDirection = AutoScrollDirection.None;
    private DispatcherTimer m_timer = null;
    private DateTime m_lastAutoScrollTime;
    private double m_scrollAmount;
    private double m_maxHorizontalScrollOffset;
    private bool m_scrollPage;

    //This is used to position the DraggedElementGhost as such.
    private Point m_ghostInitialScreenPositon;

    private ScrollViewer m_parentScrollViewer;

    [Flags()]
    private enum AutoScrollDirection
    {
      None = 0,
      Left = 1,
      Right = 2,
      Up = 4,
      Down = 8
    }
  }
}
