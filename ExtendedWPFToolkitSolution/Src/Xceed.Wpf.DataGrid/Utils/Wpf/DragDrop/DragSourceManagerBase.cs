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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.DataGrid;

namespace Xceed.Utils.Wpf.DragDrop
{
  internal class DragSourceManagerBase : DependencyObject, INotifyPropertyChanged
  {
    internal DragSourceManagerBase( UIElement draggedElement, AdornerLayer adornerLayer, UIElement container )
      : this( draggedElement, adornerLayer, container, true )
    {
    }

    internal DragSourceManagerBase( UIElement draggedElement, AdornerLayer adornerLayer, UIElement container, bool enableAutoScroll )
      : this( draggedElement, adornerLayer, container, enableAutoScroll, true )
    {
    }

    internal DragSourceManagerBase( UIElement draggedElement, AdornerLayer adornerLayer, UIElement container, bool enableAutoScroll, bool showGhost )
    {
      if( draggedElement == null )
        throw new ArgumentNullException( "draggedElement" );

      if( container == null )
        throw new ArgumentNullException( "container" );

      m_draggedElement = draggedElement;
      m_container = container;
      m_adornerLayer = ( adornerLayer != null ) ? adornerLayer : AdornerLayer.GetAdornerLayer( container );
      m_showGhost = showGhost;

      if( enableAutoScroll )
      {
        var scrollViewer = DragSourceManagerBase.GetScrollViewer( draggedElement );
        if( scrollViewer != null )
        {
          m_autoScrollManager = new AutoScrollManager( scrollViewer );
        }
      }
    }

    #region IsDragging Property

    internal bool IsDragging
    {
      get
      {
        return m_isDragging;
      }
      private set
      {
        if( value == m_isDragging )
          return;

        m_isDragging = value;

        this.OnPropertyChanged( "IsDragging" );
      }
    }

    private bool m_isDragging;

    #endregion

    #region IsDragStarted Property

    internal bool IsDragStarted
    {
      get
      {
        return m_isDragStarted;
      }
      private set
      {
        if( value == m_isDragStarted )
          return;

        m_isDragStarted = value;

        this.OnPropertyChanged( "IsDragStarted" );
      }
    }

    private bool m_isDragStarted;

    #endregion

    #region ShowGhost Property

    internal bool ShowGhost
    {
      get
      {
        return m_showGhost;
      }
      set
      {
        if( value == m_showGhost )
          return;

        m_showGhost = value;

        this.OnPropertyChanged( "ShowGhost" );
      }
    }

    private bool m_showGhost;

    #endregion

    #region AutoScrollInterval Property

    internal static readonly DependencyProperty AutoScrollIntervalProperty = DependencyProperty.Register(
      "AutoScrollInterval",
      typeof( TimeSpan ),
      typeof( DragSourceManagerBase ),
      new FrameworkPropertyMetadata( AutoScrollManager.AutoScrollInterval_DefaultValue, DragSourceManagerBase.OnAutoScrollIntervalChanged ) );

    internal TimeSpan AutoScrollInterval
    {
      get
      {
        return ( TimeSpan )this.GetValue( DragSourceManagerBase.AutoScrollIntervalProperty );
      }
      set
      {
        this.SetValue( DragSourceManagerBase.AutoScrollIntervalProperty, value );
      }
    }

    private void OnAutoScrollIntervalChanged( TimeSpan oldValue, TimeSpan newValue )
    {
      if( m_autoScrollManager == null )
        return;

      m_autoScrollManager.AutoScrollInterval = newValue;
    }

    private static void OnAutoScrollIntervalChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DragSourceManagerBase;
      if( self == null )
        return;

      self.OnAutoScrollIntervalChanged( ( TimeSpan )e.OldValue, ( TimeSpan )e.NewValue );
    }

    #endregion

    #region AutoScrollThreshold Property

    public static readonly DependencyProperty AutoScrollThresholdProperty = DependencyProperty.Register(
      "AutoScrollThreshold",
      typeof( int ),
      typeof( DragSourceManagerBase ),
      new FrameworkPropertyMetadata( AutoScrollManager.AutoScrollThreshold_DefaultValue, DragSourceManagerBase.OnAutoScrollThresholdChanged ) );

    public int AutoScrollThreshold
    {
      get
      {
        return ( int )this.GetValue( DragSourceManagerBase.AutoScrollThresholdProperty );
      }
      set
      {
        this.SetValue( DragSourceManagerBase.AutoScrollThresholdProperty, value );
      }
    }

    private void OnAutoScrollThresholdChanged( int oldValue, int newValue )
    {
      if( m_autoScrollManager == null )
        return;

      m_autoScrollManager.AutoScrollThreshold = newValue;
    }

    private static void OnAutoScrollThresholdChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DragSourceManagerBase;
      if( self == null )
        return;

      self.OnAutoScrollThresholdChanged( ( int )e.OldValue, ( int )e.NewValue );
    }

    #endregion

    #region AdornerLayer Protected Property

    protected AdornerLayer AdornerLayer
    {
      get
      {
        return m_adornerLayer;
      }
    }

    private readonly AdornerLayer m_adornerLayer;

    #endregion

    #region DraggedElement Protected Property

    protected UIElement DraggedElement
    {
      get
      {
        return m_draggedElement;
      }
    }

    private readonly UIElement m_draggedElement;

    #endregion

    #region Container Protected Property

    protected UIElement Container
    {
      get
      {
        return m_container;
      }
    }

    private readonly UIElement m_container;

    #endregion

    #region IsPopup Protected Property

    protected bool IsPopup
    {
      get
      {
        return m_isPopup;
      }
    }

    private bool m_isPopup;

    #endregion

    #region InitialMousePositionToDraggedElement Protected Property

    protected Point? InitialMousePositionToDraggedElement
    {
      get
      {
        return m_initialMousePositionToDraggedElement;
      }
    }

    private Point? m_initialMousePositionToDraggedElement;

    #endregion

    #region InitialMousePositionToAdorner Protected Property

    protected Point? InitialMousePositionToAdorner
    {
      get
      {
        return m_initialMousePositionToAdorner;
      }
    }

    private Point? m_initialMousePositionToAdorner;

    #endregion

    protected virtual void OnDragStart( Func<IInputElement, Point> getPosition )
    {
    }

    protected virtual void OnDragEnd( Func<IInputElement, Point> getPosition, bool drop )
    {
      if( drop )
      {
        this.OnDrop( getPosition );
      }
      else
      {
        this.OnDragCancel( getPosition );
      }
    }

    protected virtual void OnDragCancel( Func<IInputElement, Point> getPosition )
    {
    }

    protected virtual void OnDragMove( Func<IInputElement, Point> getPosition )
    {
    }

    protected virtual void OnDrop( Func<IInputElement, Point> getPosition )
    {
    }

    protected virtual void UpdateGhost( Func<IInputElement, Point> getPosition )
    {
      if( m_isPopup || ( m_draggedElementGhost == null ) )
        return;

      if( m_showGhost != m_draggedElementGhost.IsVisible )
      {
        if( m_showGhost )
        {
          m_draggedElementGhost.Show();
        }
        else
        {
          m_draggedElementGhost.Hide();
        }
      }

      //Find the new ghost position based on the current mouse position.
      var mousePosition = getPosition.Invoke( m_container );
      var draggedElementPosition = Point.Subtract( mousePosition, m_draggedElementMouseOffset );

      if( FrameworkElement.GetFlowDirection( m_container ) != FlowDirection.LeftToRight )
      {
        draggedElementPosition.X += m_draggedElement.RenderSize.Width;
      }

      var draggedElementPositionOnScreen = m_container.PointToScreen( draggedElementPosition );

      // When Windows fonts are magnified (via the Control Panel), the position returned by PointToScreen doesn't match perfectly to the coordinate used
      // to translate a Window object.  We need to multiply the result by a zoom factor in order to place the Window at the mouse location.
      var ghostPosition = new Point( draggedElementPositionOnScreen.X * m_mouseToScreenPositionFactor.X,
                                     draggedElementPositionOnScreen.Y * m_mouseToScreenPositionFactor.Y );

      m_draggedElementGhost.Left = ghostPosition.X;
      m_draggedElementGhost.Top = ghostPosition.Y;
    }

    [Conditional( "DEBUG" )]
    protected virtual void ValidateMouseEventArgs( MouseEventArgs e )
    {
    }

    internal void DragStart( MouseEventArgs e )
    {
      if( e.Handled || m_isDragStarted || m_isReentrant || ( e.LeftButton != MouseButtonState.Pressed ) )
        return;

      m_isReentrant = true;

      try
      {
        this.ValidateMouseEventArgs( e );
        this.DragStart( e.GetPosition );
      }
      finally
      {
        m_isReentrant = false;
      }
    }

    internal void DragCancel( MouseEventArgs e )
    {
      if( e.Handled || !m_isDragStarted || m_isReentrant )
        return;

      m_isReentrant = true;

      try
      {
        this.ValidateMouseEventArgs( e );
        this.DragEnd( e.GetPosition, false );
      }
      finally
      {
        m_isReentrant = false;
      }
    }

    internal void Drop( MouseEventArgs e )
    {
      if( e.Handled || !m_isDragStarted || m_isReentrant )
        return;

      m_isReentrant = true;

      try
      {
        this.ValidateMouseEventArgs( e );
        this.DragEnd( e.GetPosition, true );
      }
      finally
      {
        m_isReentrant = false;
      }
    }

    internal void DragMove( MouseEventArgs e )
    {
      this.DragMove( e, false );
    }

    internal void DragMove( MouseEventArgs e, bool isCreatingCopy )
    {
      if( e.Handled || !m_isDragStarted || m_isReentrant || ( e.LeftButton != MouseButtonState.Pressed ) )
        return;

      m_isReentrant = true;

      try
      {
        this.ValidateMouseEventArgs( e );
        this.DragMove( e.GetPosition, isCreatingCopy );

        if( m_autoScrollManager != null )
        {
          m_autoScrollManager.HandleMouseMove( e );
        }
      }
      finally
      {
        m_isReentrant = false;
      }
    }

    private static ScrollViewer GetScrollViewer( DependencyObject from )
    {
      while( from != null )
      {
        var scrollViewer = from as ScrollViewer;
        if( scrollViewer != null )
          return scrollViewer;

        from = VisualTreeHelper.GetParent( from );
      }

      return null;
    }

    private static FrameworkElement GetPopupOrWindow( DependencyObject from )
    {
      while( from != null )
      {
        var popup = from as Popup;
        if( popup != null )
          return popup;

        var window = from as Window;
        if( window != null )
          return window;

        from = TreeHelper.GetParent( from );
      }

      return null;
    }

    private static DependencyObject GetPopupParent( DependencyObject from )
    {
      while( from != null )
      {
        var popup = from as Popup;
        if( popup != null )
        {
          var parent = popup.Parent;
          if( parent != null )
            return parent;

          var target = popup.PlacementTarget;
          if( target != null )
            return target;
        }

        from = VisualTreeHelper.GetParent( from );
      }

      return null;
    }

    private static Vector CalculateMouseToScreenPositionFactor( FrameworkElement element )
    {
      if( element == null )
        return new Vector( 1d, 1d );

      var source = PresentationSource.FromVisual( element );
      if( source == null )
      {
        var parent = DragSourceManagerBase.GetPopupParent( element );
        while( parent != null )
        {
          source = PresentationSource.FromDependencyObject( parent );
          if( source != null )
            break;

          parent = DragSourceManagerBase.GetPopupParent( parent );
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

    private static Window CreateDraggedElementGhost( UIElement element, bool isCreatingCopy )
    {
      if( element == null )
        return null;

      TileBrush brush = null;

      // Draw a copy of the element to make sure it won't be modified. 
      // Used in MergedColumnEditor when recycling is active while dragging a cell far away.
      if( isCreatingCopy )
      {
        var drawingVisual = new DrawingVisual();
        using( var drawingContext = drawingVisual.RenderOpen() )
        {
          var visualBrush = new VisualBrush( element );
          drawingContext.DrawRectangle( visualBrush, null, new Rect( new Point(), new Size( element.RenderSize.Width, element.RenderSize.Height ) ) );
        }

        float dpiX, dpiY;
        using( System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd( IntPtr.Zero ) )
        {
          dpiX = graphics.DpiX;
          dpiY = graphics.DpiY;
        }
        var bitmapTarget = new RenderTargetBitmap( (int)element.RenderSize.Width, (int)element.RenderSize.Height, dpiX, dpiY, PixelFormats.Default );
        bitmapTarget.Render( drawingVisual );
        brush = new ImageBrush( bitmapTarget );
      }
      else
      {
        brush = new VisualBrush( element );
      }

      brush.Opacity = 0.75d;
      brush.Stretch = Stretch.None;
      brush.AlignmentX = AlignmentX.Left;
      brush.AlignmentY = AlignmentY.Top;

      var rectangle = new Rectangle();
      rectangle.Width = element.DesiredSize.Width;
      rectangle.Height = element.DesiredSize.Height;
      rectangle.Fill = brush;

      var window = new Window();
      window.Topmost = true;
      window.WindowStyle = WindowStyle.None;
      window.AllowsTransparency = true;
      window.Background = null;
      window.SizeToContent = SizeToContent.WidthAndHeight;
      window.ShowInTaskbar = false;
      window.IsHitTestVisible = false;
      window.ShowActivated = false;
      window.Focusable = false;
      window.Left = 0;
      window.Top = 0;
      window.Content = rectangle;

      //This will make sure the window follows what has been set on the parent grid.
      var dataGridContext = DataGridControl.GetDataGridContext( element );
      if( dataGridContext != null )
      {
        window.FlowDirection = dataGridContext.DataGridControl.FlowDirection;
      }

      return window;
    }

    private void DragStart( Func<IInputElement, Point> getPosition )
    {
      var parent = DragSourceManagerBase.GetPopupOrWindow( m_draggedElement );
      var draggedElementPosition = getPosition.Invoke( m_draggedElement );

      m_isPopup = ( parent is Popup );
      m_initialMousePositionToAdorner = ( m_adornerLayer != null ) ? getPosition.Invoke( m_adornerLayer ) : default( Point? );
      m_initialMousePositionToDraggedElement = draggedElementPosition;
      m_mouseToScreenPositionFactor = DragSourceManagerBase.CalculateMouseToScreenPositionFactor( parent );
      m_draggedElementMouseOffset = new Vector( draggedElementPosition.X, draggedElementPosition.Y );

      if( m_autoScrollManager != null )
      {
        m_autoScrollManager.Start();
      }

      this.IsDragStarted = true;
    }

    private void DragEnd( Func<IInputElement, Point> getPosition, bool drop )
    {
      if( m_isDragging )
      {
        this.OnDragEnd( getPosition, drop );
        this.IsDragging = false;
      }

      if( m_draggedElementGhost != null )
      {
        m_draggedElementGhost.Hide();
        m_draggedElementGhost.Content = null;
        m_draggedElementGhost.Close();
        m_draggedElementGhost = null;
      }

      if( m_autoScrollManager != null )
      {
        m_autoScrollManager.Stop();
      }

      m_initialMousePositionToAdorner = default( Point? );
      m_initialMousePositionToDraggedElement = default( Point? );

      this.IsDragStarted = false;
    }

    private void DragMove( Func<IInputElement, Point> getPosition, bool isCreatingCopy )
    {
      if( !m_initialMousePositionToAdorner.HasValue )
        return;

      if( !m_isDragging )
      {
        var position = getPosition.Invoke( m_adornerLayer );

        if( DragDropHelper.IsMouseMoveDrag( m_initialMousePositionToAdorner.Value, position ) )
        {
          m_draggedElementGhost = DragSourceManagerBase.CreateDraggedElementGhost( m_draggedElement, isCreatingCopy );

          this.OnDragStart( getPosition );
          this.IsDragging = true;
        }
      }

      if( m_isDragging )
      {
        this.OnDragMove( getPosition );
        this.UpdateGhost( getPosition );
      }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged( string propertyName )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    protected sealed override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnPropertyChanged( e );
      this.OnPropertyChanged( e.Property.Name );
    }

    #endregion

    private readonly AutoScrollManager m_autoScrollManager;
    private Window m_draggedElementGhost; //null
    private Vector m_mouseToScreenPositionFactor = new Vector( 1d, 1d );
    private Vector m_draggedElementMouseOffset;
    private bool m_isReentrant; //false
  }
}
