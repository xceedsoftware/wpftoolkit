/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Linq;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class LayoutAutoHideWindowControl : HwndHost, ILayoutControl
  {
    #region Members

    internal LayoutAnchorableControl _internalHost = null;

    private LayoutAnchorControl _anchor;
    private LayoutAnchorable _model;
    private HwndSource _internalHwndSource = null;
    private IntPtr parentWindowHandle;
    private ContentPresenter _internalHostPresenter = new ContentPresenter();
    private Grid _internalGrid = null;
    private AnchorSide _side;
    private LayoutGridResizerControl _resizer = null;
    private DockingManager _manager;
    private Border _resizerGhost = null;
    private Window _resizerWindowHost = null;
    private Vector _initialStartPoint;

    #endregion

    #region Constructors

    static LayoutAutoHideWindowControl()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( LayoutAutoHideWindowControl ), new FrameworkPropertyMetadata( typeof( LayoutAutoHideWindowControl ) ) );
      UIElement.FocusableProperty.OverrideMetadata( typeof( LayoutAutoHideWindowControl ), new FrameworkPropertyMetadata( true ) );
      Control.IsTabStopProperty.OverrideMetadata( typeof( LayoutAutoHideWindowControl ), new FrameworkPropertyMetadata( true ) );
      VisibilityProperty.OverrideMetadata( typeof( LayoutAutoHideWindowControl ), new FrameworkPropertyMetadata( Visibility.Hidden ) );
    }

    internal LayoutAutoHideWindowControl()
    {
    }

    #endregion

    #region Properties

    #region AnchorableStyle

    /// <summary>
    /// AnchorableStyle Dependency Property
    /// </summary>
    public static readonly DependencyProperty AnchorableStyleProperty = DependencyProperty.Register( "AnchorableStyle", typeof( Style ), typeof( LayoutAutoHideWindowControl ),
            new FrameworkPropertyMetadata( ( Style )null ) );

    /// <summary>
    /// Gets or sets the AnchorableStyle property. This dependency property 
    /// indicates the style to apply to the LayoutAnchorableControl hosted in this auto hide window.
    /// </summary>
    public Style AnchorableStyle
    {
      get
      {
        return ( Style )GetValue( AnchorableStyleProperty );
      }
      set
      {
        SetValue( AnchorableStyleProperty, value );
      }
    }

    #endregion

    #region Background

    /// <summary>
    /// Background Dependency Property
    /// </summary>
    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register( "Background", typeof( Brush ), typeof( LayoutAutoHideWindowControl ),
            new FrameworkPropertyMetadata( ( Brush )null ) );

    /// <summary>
    /// Gets or sets the Background property.  This dependency property 
    /// indicates background of the autohide childwindow.
    /// </summary>
    public Brush Background
    {
      get
      {
        return ( Brush )GetValue( BackgroundProperty );
      }
      set
      {
        SetValue( BackgroundProperty, value );
      }
    }

    #endregion

    #region Model

    public ILayoutElement Model
    {
      get
      {
        return _model;
      }
    }

    #endregion

    #region Resizer

    internal bool IsResizing
    {
      get;
      private set;
    }

    #endregion

    #endregion

    #region Overrides

    protected override System.Runtime.InteropServices.HandleRef BuildWindowCore( System.Runtime.InteropServices.HandleRef hwndParent )
    {
      parentWindowHandle = hwndParent.Handle;
      _internalHwndSource = new HwndSource( new HwndSourceParameters()
      {
        ParentWindow = hwndParent.Handle,
        WindowStyle = Win32Helper.WS_CHILD | Win32Helper.WS_VISIBLE | Win32Helper.WS_CLIPSIBLINGS | Win32Helper.WS_CLIPCHILDREN,
        Width = 0,
        Height = 0,
      } );

      _internalHwndSource.RootVisual = _internalHostPresenter;
      AddLogicalChild( _internalHostPresenter );
      Win32Helper.BringWindowToTop( _internalHwndSource.Handle );
      return new HandleRef( this, _internalHwndSource.Handle );
    }

    protected override void DestroyWindowCore( System.Runtime.InteropServices.HandleRef hwnd )
    {
      if( _internalHwndSource != null )
      {
        _internalHwndSource.Dispose();
        _internalHwndSource = null;
      }
    }

    protected override bool HasFocusWithinCore()
    {
      return false;
    }

    protected override System.Collections.IEnumerator LogicalChildren
    {
      get
      {
        if( _internalHostPresenter == null )
          return new UIElement[] { }.GetEnumerator();
        return new UIElement[] { _internalHostPresenter }.GetEnumerator();
      }
    }

    protected override Size MeasureOverride( Size constraint )
    {
      if( _internalHostPresenter == null )
        return base.MeasureOverride( constraint );

      _internalHostPresenter.Measure( constraint );
      //return base.MeasureOverride(constraint);
      return _internalHostPresenter.DesiredSize;
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
      if( _internalHostPresenter == null )
        return base.ArrangeOverride( finalSize );

      _internalHostPresenter.Arrange( new Rect( finalSize ) );
      return base.ArrangeOverride( finalSize );// new Size(_internalHostPresenter.ActualWidth, _internalHostPresenter.ActualHeight);
    }

    #endregion

    #region Internal Methods

    internal void Show( LayoutAnchorControl anchor )
    {
      if( _model != null )
        throw new InvalidOperationException();

      _anchor = anchor;
      _model = anchor.Model as LayoutAnchorable;
      _side = ( anchor.Model.Parent.Parent as LayoutAnchorSide ).Side;
      _manager = _model.Root.Manager;
      CreateInternalGrid();

      _model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler( _model_PropertyChanged );

      Visibility = System.Windows.Visibility.Visible;
      InvalidateMeasure();
      UpdateWindowPos();
      Win32Helper.BringWindowToTop( _internalHwndSource.Handle );
    }

    internal void Hide()
    {
      if( _model == null )
        return;

      _model.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler( _model_PropertyChanged );

      RemoveInternalGrid();
      _anchor = null;
      _model = null;
      _manager = null;
      Visibility = System.Windows.Visibility.Hidden;
    }

    internal bool IsWin32MouseOver
    {
      get
      {
        var ptMouse = new Win32Helper.Win32Point();
        if( !Win32Helper.GetCursorPos( ref ptMouse ) )
          return false;

        Point location = this.PointToScreenDPI( new Point() );

        Rect rectWindow = this.GetScreenArea();
        if( rectWindow.Contains( new Point( ptMouse.X, ptMouse.Y ) ) )
          return true;

        var manager = Model.Root.Manager;
        var anchor = manager.FindVisualChildren<LayoutAnchorControl>().Where( c => c.Model == Model ).FirstOrDefault();

        if( anchor == null )
          return false;

        location = anchor.PointToScreenDPI( new Point() );

        if( anchor.IsMouseOver )
          return true;

        return false;
      }
    }

    #endregion

    #region Private Methods

    private void _model_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
    {
      if( e.PropertyName == "IsAutoHidden" )
      {
        if( !_model.IsAutoHidden )
        {
          _manager.HideAutoHideWindow( _anchor );
        }
      }
    }

    private void CreateInternalGrid()
    {
      _internalGrid = new Grid() { FlowDirection = System.Windows.FlowDirection.LeftToRight };
      _internalGrid.SetBinding( Grid.BackgroundProperty, new Binding( "Background" ) { Source = this } );


      _internalHost = new LayoutAnchorableControl() { Model = _model, Style = AnchorableStyle };
      _internalHost.SetBinding( FlowDirectionProperty, new Binding( "Model.Root.Manager.FlowDirection" ) { Source = this } );

      KeyboardNavigation.SetTabNavigation( _internalGrid, KeyboardNavigationMode.Cycle );

      _resizer = new LayoutGridResizerControl();

      _resizer.DragStarted += new System.Windows.Controls.Primitives.DragStartedEventHandler( OnResizerDragStarted );
      _resizer.DragDelta += new System.Windows.Controls.Primitives.DragDeltaEventHandler( OnResizerDragDelta );
      _resizer.DragCompleted += new System.Windows.Controls.Primitives.DragCompletedEventHandler( OnResizerDragCompleted );

      if( _side == AnchorSide.Right )
      {
        _internalGrid.ColumnDefinitions.Add( new ColumnDefinition() { Width = new GridLength( double.IsNaN( _manager.GridSplitterWidth ) ? 6d : _manager.GridSplitterWidth ) } );
        _internalGrid.ColumnDefinitions.Add( new ColumnDefinition()
        {
          Width = _model.AutoHideWidth == 0.0 ? new GridLength( double.IsNaN( _model.AutoHideMinWidth ) ? 100d : _model.AutoHideMinWidth ) : new GridLength( double.IsNaN( _model.AutoHideWidth ) ? 0d : _model.AutoHideWidth, GridUnitType.Pixel )
        } );

        Grid.SetColumn( _resizer, 0 );
        Grid.SetColumn( _internalHost, 1 );

        _resizer.Cursor = Cursors.SizeWE;

        HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
        VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
      }
      else if( _side == AnchorSide.Left )
      {
        _internalGrid.ColumnDefinitions.Add( new ColumnDefinition()
        {
          Width = _model.AutoHideWidth == 0.0 ? new GridLength( double.IsNaN( _model.AutoHideMinWidth ) ? 100d : _model.AutoHideMinWidth ) : new GridLength( double.IsNaN( _model.AutoHideWidth ) ? 0d : _model.AutoHideWidth, GridUnitType.Pixel ),
        } );
        _internalGrid.ColumnDefinitions.Add( new ColumnDefinition() { Width = new GridLength( double.IsNaN( _manager.GridSplitterWidth ) ? 6d : _manager.GridSplitterWidth ) } );

        Grid.SetColumn( _internalHost, 0 );
        Grid.SetColumn( _resizer, 1 );

        _resizer.Cursor = Cursors.SizeWE;

        HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
      }
      else if( _side == AnchorSide.Top )
      {
        _internalGrid.RowDefinitions.Add( new RowDefinition()
        {
          Height = _model.AutoHideHeight == 0.0 ? new GridLength( double.IsNaN( _model.AutoHideMinHeight ) ? 100d : _model.AutoHideMinHeight ) : new GridLength( double.IsNaN( _model.AutoHideHeight ) ? 0d : _model.AutoHideHeight, GridUnitType.Pixel ),
        } );
        _internalGrid.RowDefinitions.Add( new RowDefinition() { Height = new GridLength( double.IsNaN( _manager.GridSplitterHeight ) ? 6d : _manager.GridSplitterHeight ) } );

        Grid.SetRow( _internalHost, 0 );
        Grid.SetRow( _resizer, 1 );

        _resizer.Cursor = Cursors.SizeNS;

        VerticalAlignment = System.Windows.VerticalAlignment.Top;
        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

      }
      else if( _side == AnchorSide.Bottom )
      {
        _internalGrid.RowDefinitions.Add( new RowDefinition() { Height = new GridLength( double.IsNaN( _manager.GridSplitterHeight ) ? 6d : _manager.GridSplitterHeight ) } );
        _internalGrid.RowDefinitions.Add( new RowDefinition()
        {
          Height = _model.AutoHideHeight == 0.0 ? new GridLength( double.IsNaN( _model.AutoHideMinHeight ) ? 100d : _model.AutoHideMinHeight ) : new GridLength( double.IsNaN( _model.AutoHideHeight ) ? 0d : _model.AutoHideHeight, GridUnitType.Pixel ),
        } );

        Grid.SetRow( _resizer, 0 );
        Grid.SetRow( _internalHost, 1 );

        _resizer.Cursor = Cursors.SizeNS;

        VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
      }


      _internalGrid.Children.Add( _resizer );
      _internalGrid.Children.Add( _internalHost );
      _internalHostPresenter.Content = _internalGrid;
    }

    private void RemoveInternalGrid()
    {
      _resizer.DragStarted -= new System.Windows.Controls.Primitives.DragStartedEventHandler( OnResizerDragStarted );
      _resizer.DragDelta -= new System.Windows.Controls.Primitives.DragDeltaEventHandler( OnResizerDragDelta );
      _resizer.DragCompleted -= new System.Windows.Controls.Primitives.DragCompletedEventHandler( OnResizerDragCompleted );

      _internalHostPresenter.Content = null;
    }

    private void ShowResizerOverlayWindow( LayoutGridResizerControl splitter )
    {
      _resizerGhost = new Border()
      {
        Background = splitter.BackgroundWhileDragging,
        Opacity = splitter.OpacityWhileDragging
      };

      var areaElement = _manager.GetAutoHideAreaElement();
      var modelControlActualSize = this._internalHost.TransformActualSizeToAncestor();

      Point ptTopLeftScreen = areaElement.PointToScreenDPIWithoutFlowDirection( new Point() );

      var managerSize = areaElement.TransformActualSizeToAncestor();

      Size windowSize;

      if( _side == AnchorSide.Right || _side == AnchorSide.Left )
      {
        windowSize = new Size(
            managerSize.Width - 25.0 + splitter.ActualWidth,
            managerSize.Height );

        _resizerGhost.Width = splitter.ActualWidth;
        _resizerGhost.Height = windowSize.Height;
        ptTopLeftScreen.Offset( 25, 0.0 );
      }
      else
      {
        windowSize = new Size(
            managerSize.Width,
            managerSize.Height - _model.AutoHideMinHeight - 25.0 + splitter.ActualHeight );

        _resizerGhost.Height = splitter.ActualHeight;
        _resizerGhost.Width = windowSize.Width;
        ptTopLeftScreen.Offset( 0.0, 25.0 );
      }

      _initialStartPoint = splitter.PointToScreenDPIWithoutFlowDirection( new Point() ) - ptTopLeftScreen;

      if( _side == AnchorSide.Right || _side == AnchorSide.Left )
      {
        Canvas.SetLeft( _resizerGhost, _initialStartPoint.X );
      }
      else
      {
        Canvas.SetTop( _resizerGhost, _initialStartPoint.Y );
      }

      Canvas panelHostResizer = new Canvas()
      {
        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
        VerticalAlignment = System.Windows.VerticalAlignment.Stretch
      };

      panelHostResizer.Children.Add( _resizerGhost );


      _resizerWindowHost = new Window()
      {
        ResizeMode = ResizeMode.NoResize,
        WindowStyle = System.Windows.WindowStyle.None,
        ShowInTaskbar = false,
        AllowsTransparency = true,
        Background = null,
        Width = windowSize.Width,
        Height = windowSize.Height,
        Left = ptTopLeftScreen.X,
        Top = ptTopLeftScreen.Y,
        ShowActivated = false,
        Owner = Window.GetWindow( this ),
        Content = panelHostResizer
      };

      _resizerWindowHost.Show();
    }

    private void HideResizerOverlayWindow()
    {
      if( _resizerWindowHost != null )
      {
        _resizerWindowHost.Close();
        _resizerWindowHost = null;
      }
    }

    private void OnResizerDragCompleted( object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e )
    {
      LayoutGridResizerControl splitter = sender as LayoutGridResizerControl;
      var rootVisual = this.FindVisualTreeRoot() as Visual;

      var trToWnd = TransformToAncestor( rootVisual );
      Vector transformedDelta = trToWnd.Transform( new Point( e.HorizontalChange, e.VerticalChange ) ) -
          trToWnd.Transform( new Point() );

      double delta;
      if( _side == AnchorSide.Right || _side == AnchorSide.Left )
        delta = Canvas.GetLeft( _resizerGhost ) - _initialStartPoint.X;
      else
        delta = Canvas.GetTop( _resizerGhost ) - _initialStartPoint.Y;

      if( _side == AnchorSide.Right )
      {
        if( _model.AutoHideWidth == 0.0 )
          _model.AutoHideWidth = _internalHost.ActualWidth - delta;
        else
          _model.AutoHideWidth -= delta;

        _internalGrid.ColumnDefinitions[ 1 ].Width = new GridLength( double.IsNaN( _model.AutoHideWidth ) ? 0d : _model.AutoHideWidth, GridUnitType.Pixel );
      }
      else if( _side == AnchorSide.Left )
      {
        if( _model.AutoHideWidth == 0.0 )
          _model.AutoHideWidth = _internalHost.ActualWidth + delta;
        else
          _model.AutoHideWidth += delta;

        _internalGrid.ColumnDefinitions[ 0 ].Width = new GridLength( double.IsNaN( _model.AutoHideWidth ) ? 0d : _model.AutoHideWidth, GridUnitType.Pixel );
      }
      else if( _side == AnchorSide.Top )
      {
        if( _model.AutoHideHeight == 0.0 )
          _model.AutoHideHeight = _internalHost.ActualHeight + delta;
        else
          _model.AutoHideHeight += delta;

        _internalGrid.RowDefinitions[ 0 ].Height = new GridLength( double.IsNaN( _model.AutoHideHeight ) ? 0d : _model.AutoHideHeight, GridUnitType.Pixel );
      }
      else if( _side == AnchorSide.Bottom )
      {
        if( _model.AutoHideHeight == 0.0 )
          _model.AutoHideHeight = _internalHost.ActualHeight - delta;
        else
          _model.AutoHideHeight -= delta;

        _internalGrid.RowDefinitions[ 1 ].Height = new GridLength( double.IsNaN( _model.AutoHideHeight ) ? 0d : _model.AutoHideHeight, GridUnitType.Pixel );
      }

      HideResizerOverlayWindow();

      IsResizing = false;
      InvalidateMeasure();
    }

    private void OnResizerDragDelta( object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e )
    {
      LayoutGridResizerControl splitter = sender as LayoutGridResizerControl;
      var rootVisual = this.FindVisualTreeRoot() as Visual;

      var trToWnd = TransformToAncestor( rootVisual );
      Vector transformedDelta = trToWnd.Transform( new Point( e.HorizontalChange, e.VerticalChange ) ) -
          trToWnd.Transform( new Point() );

      if( _side == AnchorSide.Right || _side == AnchorSide.Left )
      {
        if( FrameworkElement.GetFlowDirection( _internalHost ) == System.Windows.FlowDirection.RightToLeft )
          transformedDelta.X = -transformedDelta.X;
        Canvas.SetLeft( _resizerGhost, MathHelper.MinMax( _initialStartPoint.X + transformedDelta.X, 0.0, _resizerWindowHost.Width - _resizerGhost.Width ) );
      }
      else
      {
        Canvas.SetTop( _resizerGhost, MathHelper.MinMax( _initialStartPoint.Y + transformedDelta.Y, 0.0, _resizerWindowHost.Height - _resizerGhost.Height ) );
      }
    }

    private void OnResizerDragStarted( object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e )
    {
      var resizer = sender as LayoutGridResizerControl;
      ShowResizerOverlayWindow( resizer );
      IsResizing = true;
    }

    #endregion
  }
}
