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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Xceed.Wpf.DataGrid;

namespace Xceed.Utils.Wpf.DragDrop
{
  internal class DragSourceManager : DependencyObject, INotifyPropertyChanged
  {
    public DragSourceManager( UIElement draggedElement, AdornerLayer adornerLayerInsideDragContainer, UIElement dragContainer )
      : this( draggedElement, adornerLayerInsideDragContainer, dragContainer, true )
    {
    }

    public DragSourceManager( UIElement draggedElement, AdornerLayer adornerLayerInsideDragContainer, UIElement dragContainer, bool enableAutoScroll )
      : this( draggedElement, adornerLayerInsideDragContainer, dragContainer, enableAutoScroll, true )
    {
    }

    public DragSourceManager( UIElement draggedElement, AdornerLayer adornerLayerInsideDragContainer, UIElement dragContainer, bool enableAutoScroll, bool showDraggedElementGhost )
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

        if( scrollViewer != null )
        {
          m_autoScrollManager = new AutoScrollManager( scrollViewer );
        }
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

    #region MouseToScreenPositionFactor Read-Only Public Property

    internal Vector MouseToScreenPositionFactor
    {
      get
      {
        return m_mouseToScreenPositionFactor;
      }
      private set
      {
        m_mouseToScreenPositionFactor = value;
      }
    }

    private Vector m_mouseToScreenPositionFactor = new Vector( 1d, 1d );

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
        return ( m_autoScrollManager != null ) ? m_autoScrollManager.ScrollViewer : null;
      }
    }

    #endregion

    #region AutoScrollInterval Property

    public static readonly DependencyProperty AutoScrollIntervalProperty = DependencyProperty.Register(
      "AutoScrollInterval",
      typeof( int ),
      typeof( DragSourceManager ),
      new FrameworkPropertyMetadata( AutoScrollManager.AutoScrollInterval_DefaultValue, OnAutoScrollIntervalChanged ) );

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

    private static void OnAutoScrollIntervalChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
      var autoScrollManager = ( ( DragSourceManager )sender ).m_autoScrollManager;
      if( autoScrollManager != null )
      {
        autoScrollManager.AutoScrollInterval = ( int )args.NewValue;
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
      new FrameworkPropertyMetadata( AutoScrollManager.AutoScrollTreshold_DefaultValue, OnAutoScrollTresholdChanged ) );

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

    private static void OnAutoScrollTresholdChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args )
    {
      var autoScrollManager = ( ( DragSourceManager )sender ).m_autoScrollManager;
      if( autoScrollManager != null )
      {
        autoScrollManager.AutoScrollTreshold = ( int )args.NewValue;
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

      if( !this.DraggedElement.CaptureMouse() )
      {
        Debug.Fail( "The DragSourceManager was unable to capture the mouse on the dragged element." );
        return;
      }

      //Get the current window (which can be a popup) on which the drag is happening
      var parent = this.GetDraggedElementWindow();

      this.InitialMousePositionToDragContainerAdorner = e.GetPosition( this.AdornerLayerInsideDragContainer );
      this.InitialMousePositionToDraggedElement = e.GetPosition( this.DraggedElement );
      this.MouseToScreenPositionFactor = DragSourceManager.CalculateMouseToScreenPositionFactor( parent );
      this.ParentWindowIsPopup = ( parent is Popup );

      var draggedElementMousePosition = e.GetPosition( this.DraggedElement );
      m_draggedElementMouseOffset = new Vector( draggedElementMousePosition.X, draggedElementMousePosition.Y );
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

      if( ( this.InitialMousePositionToDragContainerAdorner.HasValue ) && ( e.LeftButton == MouseButtonState.Pressed ) )
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

      var ghost = this.DraggedElementGhost;
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

      //Find the new ghost position based on the current mouse position.
      var mousePosition = e.GetPosition( this.DragContainer );
      var draggedElementPosition = Point.Subtract( mousePosition, m_draggedElementMouseOffset );

      if( FrameworkElement.GetFlowDirection( this.DragContainer ) != FlowDirection.LeftToRight )
      {
        draggedElementPosition.X += this.DraggedElement.RenderSize.Width;
      }

      var draggedElementPositionOnScreen = this.DragContainer.PointToScreen( draggedElementPosition );

      var ghostPosition = new Point( draggedElementPositionOnScreen.X * m_mouseToScreenPositionFactor.X,
                                     draggedElementPositionOnScreen.Y * m_mouseToScreenPositionFactor.Y );

      ghost.Left = ghostPosition.X;
      ghost.Top = ghostPosition.Y;
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
      if( m_autoScrollManager != null )
      {
        m_autoScrollManager.StopAutoScroll();
      }
    }

    protected virtual void NotifyDragOutsideQueryCursor( object sender, QueryCursorEventArgs e )
    {
      e.Cursor = ( this.DropOutsideCursor != null ) ? this.DropOutsideCursor : Cursors.No;

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

      // Call this method after the drag is processed in case the ShowDraggedElementGhost property was modified by the GetDropTargetOnDrag
      this.UpdateDraggedElementGhostOnDrag( e );

      if( m_autoScrollManager != null )
      {
        m_autoScrollManager.ProcessMouseMove( e );
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

    private static Vector CalculateMouseToScreenPositionFactor( FrameworkElement element )
    {
      if( element == null )
        return new Vector( 1d, 1d );

      var source = PresentationSource.FromVisual( element );
      if( source == null )
      {
        var parent = DragSourceManager.GetPopupParent( element );
        while( parent != null )
        {
          source = PresentationSource.FromDependencyObject( parent );
          if( source != null )
            break;

          parent = DragSourceManager.GetPopupParent( parent );
        }
      }

      double x, y;

      if( source != null )
      {
        var deviceUnits = source.CompositionTarget.TransformToDevice;
        x = deviceUnits.M11;
        y = deviceUnits.M22;
      }
      else
      {
        using( var hwnd = new HwndSource( new HwndSourceParameters() ) )
        {
          var deviceUnits = hwnd.CompositionTarget.TransformToDevice;
          x = deviceUnits.M11;
          y = deviceUnits.M22;
        }
      }

      return new Vector( ( x == 0d ) ? 1d : 1d / x, ( y == 0d ) ? 1d : 1d / y );
    }

    private static DependencyObject GetPopupParent( DependencyObject element )
    {
      while( element != null )
      {
        var popup = element as Popup;
        if( popup != null )
        {
          var parent = popup.Parent;
          if( parent != null )
            return parent;

          var target = popup.PlacementTarget;
          if( target != null )
            return target;
        }

        element = VisualTreeHelper.GetParent( element );
      }

      return null;
    }

    private FrameworkElement GetDraggedElementWindow()
    {
      DependencyObject current = this.DraggedElement;
      while( current != null )
      {
        //If the grid is in a popup, this becomes the owning window, so return it.
        var popup = current as Popup;
        if( popup != null )
          return popup;

        var window = current as Window;
        if( window != null )
          return window;

        current = TreeHelper.GetParent( current );
      }

      // The dragged element's window was not found.
      return null;
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

    //This is used to position the DraggedElementGhost as such.
    private Vector m_draggedElementMouseOffset;

    private AutoScrollManager m_autoScrollManager;
  }
}
