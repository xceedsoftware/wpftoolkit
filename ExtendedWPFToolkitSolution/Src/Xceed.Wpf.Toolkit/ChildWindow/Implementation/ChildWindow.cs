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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit.Primitives;
using Xceed.Wpf.Toolkit.Core;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit
{
#pragma warning disable 0809
#pragma warning disable 0618 

  [TemplatePart( Name = PART_WindowRoot, Type = typeof( Grid ) )]  
  [TemplatePart( Name = PART_Root, Type = typeof( Grid ) )]   
  [TemplatePart( Name = PART_WindowControl, Type = typeof( WindowControl ) )]
  public class ChildWindow : WindowControl
  {
    private const string PART_WindowRoot = "PART_WindowRoot";  
    private const string PART_Root = "PART_Root";   
    private const string PART_WindowControl = "PART_WindowControl";
    private const int _horizontalOffset = 3;
    private const int _verticalOffset = 3;

    #region Private Members

    private Grid _root; 
    private TranslateTransform _moveTransform = new TranslateTransform();  
    private bool _startupPositionInitialized;  
    private FrameworkElement _parentContainer;  
    private Rectangle _modalLayer = new Rectangle();  
    private Canvas _modalLayerPanel = new Canvas();  
    private Grid _windowRoot;  
    private WindowControl _windowControl;
    private bool _ignorePropertyChanged;
    private bool _hasChildren;
    private bool _hasWindowContainer;  

    #endregion //Private Members

    #region Public Properties

    #region DialogResult

    private bool? _dialogResult;
    /// <summary>
    /// Gets or sets a value indicating whether the ChildWindow was accepted or canceled.
    /// </summary>
    /// <value>
    /// True if the child window was accepted; false if the child window was
    /// canceled. The default is null.
    /// </value>
    [TypeConverter( typeof( NullableBoolConverter ) )]
    public bool? DialogResult
    {
      get
      {
        return _dialogResult;
      }
      set
      {
        if( _dialogResult != value )
        {
          _dialogResult = value;
          this.Close();
        }
      }
    }

    #endregion //DialogResult

    #region DesignerWindowState

    public static readonly DependencyProperty DesignerWindowStateProperty = DependencyProperty.Register( "DesignerWindowState", typeof( WindowState ), typeof( ChildWindow ), new PropertyMetadata( WindowState.Closed, OnDesignerWindowStatePropertyChanged ) );
    public WindowState DesignerWindowState
    {
      get
      {
        return ( WindowState )GetValue( DesignerWindowStateProperty );
      }
      set
      {
        SetValue( DesignerWindowStateProperty, value );
      }
    }

    private static void OnDesignerWindowStatePropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ChildWindow childWindow = d as ChildWindow;
      if( childWindow != null )
        childWindow.OnDesignerWindowStatePropertyChanged( ( WindowState )e.OldValue, ( WindowState )e.NewValue );
    }

    protected virtual void OnDesignerWindowStatePropertyChanged( WindowState oldValue, WindowState newValue )
    {
      if( DesignerProperties.GetIsInDesignMode( this ) )
      {
        Visibility = newValue == Xceed.Wpf.Toolkit.WindowState.Open ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    #endregion //DesignerWindowState

    #region FocusedElement

    public static readonly DependencyProperty FocusedElementProperty = DependencyProperty.Register( "FocusedElement", typeof( FrameworkElement ), typeof( ChildWindow ), new UIPropertyMetadata( null ) );
    public FrameworkElement FocusedElement
    {
      get
      {
        return ( FrameworkElement )GetValue( FocusedElementProperty );
      }
      set
      {
        SetValue( FocusedElementProperty, value );
      }
    }

    #endregion

    #region IsModal

    public static readonly DependencyProperty IsModalProperty = DependencyProperty.Register( "IsModal", typeof( bool ), typeof( ChildWindow ), new UIPropertyMetadata( false, new PropertyChangedCallback( OnIsModalPropertyChanged ) ) );
    public bool IsModal
    {
      get
      {
        return ( bool )GetValue( IsModalProperty );
      }
      set
      {
        SetValue( IsModalProperty, value );
      }
    }

    private static void OnIsModalPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ChildWindow childWindow = d as ChildWindow;
      if( childWindow != null )
        childWindow.OnIsModalChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    internal event EventHandler<EventArgs> IsModalChanged;

    private void OnIsModalChanged( bool oldValue, bool newValue )
    {
      EventHandler<EventArgs> handler = IsModalChanged;
      if( handler != null )
      {
        handler( this, EventArgs.Empty );
      }

      if( !_hasWindowContainer )
      {
        if( newValue )
        {
          KeyboardNavigation.SetTabNavigation( this, KeyboardNavigationMode.Cycle );
          ShowModalLayer();
        }
        else
        {
          KeyboardNavigation.SetTabNavigation( this, KeyboardNavigationMode.Continue );
          HideModalLayer();
        }
      }
    }

    #endregion //IsModal

    #region OverlayBrush (Obsolete)

    [Obsolete( "This property is obsolete and should no longer be used. Use WindowContainer.ModalBackgroundBrushProperty instead." )]
    public static readonly DependencyProperty OverlayBrushProperty = DependencyProperty.Register( "OverlayBrush", typeof( Brush ), typeof( ChildWindow ), new PropertyMetadata( Brushes.Gray, OnOverlayBrushChanged ) );
    [Obsolete( "This property is obsolete and should no longer be used. Use WindowContainer.ModalBackgroundBrushProperty instead." )]
    public Brush OverlayBrush
    {
      get
      {
        return ( Brush )GetValue( OverlayBrushProperty );
      }
      set
      {
        SetValue( OverlayBrushProperty, value );
      }
    }

    private static void OnOverlayBrushChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ChildWindow childWindow = d as ChildWindow;
      if( childWindow != null )
        childWindow.OnOverlayBrushChanged( ( Brush )e.OldValue, ( Brush )e.NewValue );
    }

    [Obsolete( "This method is obsolete and should no longer be used. Use WindowContainer.ModalBackgroundBrushProperty instead." )]
    protected virtual void OnOverlayBrushChanged( Brush oldValue, Brush newValue )
    {
      _modalLayer.Fill = newValue;
    }

    #endregion //OverlayBrush

    #region OverlayOpacity (Obsolete)

    [Obsolete( "This property is obsolete and should no longer be used. Use WindowContainer.ModalBackgroundBrushProperty instead." )]
    public static readonly DependencyProperty OverlayOpacityProperty = DependencyProperty.Register( "OverlayOpacity", typeof( double ), typeof( ChildWindow ), new PropertyMetadata( 0.5, OnOverlayOpacityChanged ) );
    [Obsolete( "This property is obsolete and should no longer be used. Use WindowContainer.ModalBackgroundBrushProperty instead." )]
    public double OverlayOpacity
    {
      get
      {
        return ( double )GetValue( OverlayOpacityProperty );
      }
      set
      {
        SetValue( OverlayOpacityProperty, value );
      }
    }

    private static void OnOverlayOpacityChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ChildWindow childWindow = d as ChildWindow;
      if( childWindow != null )
        childWindow.OnOverlayOpacityChanged( ( double )e.OldValue, ( double )e.NewValue );
    }

    [Obsolete( "This method is obsolete and should no longer be used. Use WindowContainer.ModalBackgroundBrushProperty instead." )]
    protected virtual void OnOverlayOpacityChanged( double oldValue, double newValue )
    {
      _modalLayer.Opacity = newValue;
    }

    #endregion //OverlayOpacity  

    #region WindowStartupLocation

    public static readonly DependencyProperty WindowStartupLocationProperty = DependencyProperty.Register( "WindowStartupLocation", typeof( WindowStartupLocation ), typeof( ChildWindow ), new UIPropertyMetadata( WindowStartupLocation.Manual, OnWindowStartupLocationChanged ) );
    public WindowStartupLocation WindowStartupLocation
    {
      get
      {
        return ( WindowStartupLocation )GetValue( WindowStartupLocationProperty );
      }
      set
      {
        SetValue( WindowStartupLocationProperty, value );
      }
    }

    private static void OnWindowStartupLocationChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ChildWindow childWindow = o as ChildWindow;
      if( childWindow != null )
        childWindow.OnWindowStartupLocationChanged( ( WindowStartupLocation )e.OldValue, ( WindowStartupLocation )e.NewValue );
    }

    protected virtual void OnWindowStartupLocationChanged( WindowStartupLocation oldValue, WindowStartupLocation newValue )
    {
      // TODO: Add your property changed side-effects. Descendants can override as well.
    }

    #endregion //WindowStartupLocation

    #region WindowState

    public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register( "WindowState", typeof( WindowState ), typeof( ChildWindow ), new FrameworkPropertyMetadata( WindowState.Closed, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnWindowStatePropertyChanged ) );
    public WindowState WindowState
    {
      get
      {
        return ( WindowState )GetValue( WindowStateProperty );
      }
      set
      {
        SetValue( WindowStateProperty, value );
      }
    }

    private static void OnWindowStatePropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ChildWindow childWindow = d as ChildWindow;
      if( childWindow != null )
        childWindow.OnWindowStatePropertyChanged( ( WindowState )e.OldValue, ( WindowState )e.NewValue );
    }

    protected virtual void OnWindowStatePropertyChanged( WindowState oldValue, WindowState newValue )
    {
      if( !DesignerProperties.GetIsInDesignMode( this ) )
      {
        if( !_ignorePropertyChanged )
          SetWindowState( newValue );
      }
      else
      {
        Visibility = DesignerWindowState == Xceed.Wpf.Toolkit.WindowState.Open ? Visibility.Visible : System.Windows.Visibility.Collapsed;
      }
    }

    #endregion //WindowState

    #endregion //Public Properties

    #region Constructors

    static ChildWindow()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( ChildWindow ), new FrameworkPropertyMetadata( typeof( ChildWindow ) ) );
    }

    public ChildWindow()
    {
      DesignerWindowState = Xceed.Wpf.Toolkit.WindowState.Open;

      _modalLayer.Fill = OverlayBrush;
      _modalLayer.Opacity = OverlayOpacity;

      this.IsVisibleChanged += this.ChildWindow_IsVisibleChanged;
    }

    #endregion //Constructors

    #region Base Class Overrides

    internal override bool AllowPublicIsActiveChange
    {
      get { return false; }
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( _windowControl != null )
      {
        _windowControl.HeaderDragDelta -= ( o, e ) => this.OnHeaderDragDelta( e );
        _windowControl.HeaderIconDoubleClicked -= ( o, e ) => this.OnHeaderIconDoubleClick( e );
        _windowControl.CloseButtonClicked -= ( o, e ) => this.OnCloseButtonClicked( e );
      }
      _windowControl = this.GetTemplateChild( PART_WindowControl ) as WindowControl;
      if( _windowControl != null )
      {
        _windowControl.HeaderDragDelta += ( o, e ) => this.OnHeaderDragDelta( e );
        _windowControl.HeaderIconDoubleClicked += ( o, e ) => this.OnHeaderIconDoubleClick( e );
        _windowControl.CloseButtonClicked += ( o, e ) => this.OnCloseButtonClicked( e );
      }

      this.UpdateBlockMouseInputsPanel();

      _windowRoot = this.GetTemplateChild( PART_WindowRoot ) as Grid;
      if( _windowRoot != null )
      {
        _windowRoot.RenderTransform = _moveTransform;
      }
      _hasWindowContainer = ( VisualTreeHelper.GetParent( this ) as WindowContainer ) != null;

      if( !_hasWindowContainer )
      {
        _parentContainer = VisualTreeHelper.GetParent( this ) as FrameworkElement;
        if( _parentContainer != null )
        {
          _parentContainer.LayoutUpdated += ParentContainer_LayoutUpdated;
          _parentContainer.SizeChanged += ParentContainer_SizeChanged;

          //this is for XBAP applications only. When inside an XBAP the parent container has no height or width until it has loaded. Therefore
          //we need to handle the loaded event and reposition the window.
          if( System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted )
          {
            _parentContainer.Loaded += ( o, e ) =>
            {
              ExecuteOpen();
            };
          }
        }

        this.Unloaded += new RoutedEventHandler( ChildWindow_Unloaded );

        //initialize our modal background width/height
        _modalLayer.Height = _parentContainer.ActualHeight;
        _modalLayer.Width = _parentContainer.ActualWidth;

        _root = this.GetTemplateChild( PART_Root ) as Grid;

#if VS2008
      FocusVisualStyle = null;
#else
        Style focusStyle = ( _root != null ) ? _root.Resources[ "FocusVisualStyle" ] as Style : null;
        if( focusStyle != null )
        {
          Setter focusStyleDataContext = new Setter( Control.DataContextProperty, this );
          focusStyle.Setters.Add( focusStyleDataContext );
          FocusVisualStyle = focusStyle;
        }
#endif
        if( _root != null )
        {
          _root.Children.Add( _modalLayerPanel );
        }
      }
    }

    protected override void OnGotFocus( RoutedEventArgs e )
    {
      base.OnGotFocus( e );

      Action action = () =>
      {
        if( FocusedElement != null )
        {
          _hasChildren = true;
          FocusedElement.Focus();
        }
        else
        {
          //Focus first Focusable Child element of ChildWindow
          var focusableChild = TreeHelper.FindChild<FrameworkElement>( this.Content as DependencyObject, x => x.Focusable );
          if( focusableChild != null )
          {
            _hasChildren = true;
            focusableChild.Focus();
          }
          else
          {
            _hasChildren = false;
          }
        }
      };

      Dispatcher.BeginInvoke( DispatcherPriority.ApplicationIdle, action );
    }

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      base.OnPreviewKeyDown( e );

      if( this.IsModal )
      {
        // Prevent MenuItem shortcuts while ChildWindow is modal.
        if( Keyboard.IsKeyDown( Key.LeftAlt ) || Keyboard.IsKeyDown( Key.RightAlt ) )
        {
          e.Handled = true;
        }
        // Prevent Tab when no children
        else if( (e.Key == Key.Tab) && !_hasChildren )
        {
          e.Handled = true;
        }
      }
    }

    protected override void OnKeyDown( KeyEventArgs e )
    {
      base.OnKeyDown( e );

      if( WindowState == WindowState.Open )
      {
        switch( e.Key )
        {
          case Key.Left:
            this.Left -= _horizontalOffset;
            e.Handled = true;
            break;

          case Key.Right:
            this.Left += _horizontalOffset;
            e.Handled = true;
            break;

          case Key.Down:
            this.Top += _verticalOffset;
            e.Handled = true;
            break;

          case Key.Up:
            this.Top -= _verticalOffset;
            e.Handled = true;
            break;
        }
      }
    }

    protected override void OnLeftPropertyChanged( double oldValue, double newValue )
    {
      base.OnLeftPropertyChanged( oldValue, newValue );

      _hasWindowContainer = ( VisualTreeHelper.GetParent( this ) as WindowContainer ) != null;
      if( !_hasWindowContainer )
      {
          Left = GetRestrictedLeft();
        ProcessMove( newValue - oldValue, 0 );
      }
    }

    protected override void OnTopPropertyChanged( double oldValue, double newValue )
    {
      base.OnTopPropertyChanged( oldValue, newValue );

      _hasWindowContainer = ( VisualTreeHelper.GetParent( this ) as WindowContainer ) != null;
      if( !_hasWindowContainer )
      {
          Top = GetRestrictedTop();
        ProcessMove( 0, newValue - oldValue );
      }
    }

    internal override void UpdateBlockMouseInputsPanel()
    {
      if( _windowControl != null )
      {
        _windowControl.IsBlockMouseInputsPanelActive = this.IsBlockMouseInputsPanelActive;
      }
    }




    #endregion //Base Class Overrides

    #region Event Handlers

    protected virtual void OnHeaderDragDelta( DragDeltaEventArgs e )
    {
      if( !this.IsCurrentWindow( e.OriginalSource ) )
        return;

      e.Handled = true;

      DragDeltaEventArgs args = new DragDeltaEventArgs( e.HorizontalChange, e.VerticalChange );
      args.RoutedEvent = HeaderDragDeltaEvent;
      args.Source = this;
      this.RaiseEvent( args );

      if( !args.Handled )
      {
        if( object.Equals( e.OriginalSource, _windowControl ) )
        {
          double left = 0.0;

          if( this.FlowDirection == FlowDirection.RightToLeft )
            left = this.Left - e.HorizontalChange;
          else
            left = this.Left + e.HorizontalChange;

          this.Left = left;
          this.Top += e.VerticalChange;
        }
      }
    }

    protected virtual void OnHeaderIconDoubleClick( MouseButtonEventArgs e )
    {
      if( !this.IsCurrentWindow( e.OriginalSource ) )
        return;

      e.Handled = true;

      MouseButtonEventArgs args = new MouseButtonEventArgs( Mouse.PrimaryDevice, 0, MouseButton.Left );
      args.RoutedEvent = HeaderIconDoubleClickedEvent;
      args.Source = this;
      this.RaiseEvent( args );

      if( !args.Handled )
      {
        this.Close();
      }
    }

    protected virtual void OnCloseButtonClicked( RoutedEventArgs e )
    {
      if( !this.IsCurrentWindow( e.OriginalSource ) )
        return;

      e.Handled = true;

      RoutedEventArgs args = new RoutedEventArgs( CloseButtonClickedEvent, this );
      this.RaiseEvent( args );

      if( !args.Handled )
      {
        this.Close();
      }
    }













    [Obsolete( "This method is obsolete and should no longer be used." )]
    private void ParentContainer_LayoutUpdated( object sender, EventArgs e )
    {
      if( DesignerProperties.GetIsInDesignMode( this ) )
        return;

      //we only want to set the start position if this is the first time the control has bee initialized
      if( !_startupPositionInitialized )
      {
        ExecuteOpen();
        _startupPositionInitialized = true;
      }
    }

    [Obsolete( "This method is obsolete and should no longer be used." )]
    private void ChildWindow_Unloaded( object sender, RoutedEventArgs e )
    {
      if( _parentContainer != null )
      {
        _parentContainer.LayoutUpdated -= ParentContainer_LayoutUpdated;
        _parentContainer.SizeChanged -= ParentContainer_SizeChanged;        

        //this is for XBAP applications only. When inside an XBAP the parent container has no height or width until it has loaded. Therefore
        //we need to handle the loaded event and reposition the window.
        if( System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted )
        {
          _parentContainer.Loaded -= ( o, ev ) =>
          {
            ExecuteOpen();
          };
        }
      }
    }

    [Obsolete( "This method is obsolete and should no longer be used." )]
    void ParentContainer_SizeChanged( object sender, SizeChangedEventArgs e )
    {
      //resize our modal layer
      _modalLayer.Height = e.NewSize.Height;
      _modalLayer.Width = e.NewSize.Width;

      //reposition our window
      Left = GetRestrictedLeft();
      Top = GetRestrictedTop();
    }

    private void ChildWindow_IsVisibleChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      if( (bool)e.NewValue && this.IsModal )
      {
        this.Focus();
      }
    }



    #endregion //Event Handlers

    #region Methods

    #region Private



    [Obsolete( "This method is obsolete and should no longer be used. Use WindowContainer.GetRestrictedLeft() instead." )]
    private double GetRestrictedLeft()
    {
      if( Left < 0 )
        return 0;

      if( ( _parentContainer != null ) && (_windowRoot != null) )
      {
        if( Left + _windowRoot.ActualWidth > _parentContainer.ActualWidth && _parentContainer.ActualWidth != 0 )
        {
          double left = _parentContainer.ActualWidth - _windowRoot.ActualWidth;
          return left < 0 ? 0 : left;
        }
      }

      return Left;
    }

    [Obsolete( "This method is obsolete and should no longer be used. Use WindowContainer.GetRestrictedTop() instead." )]
    private double GetRestrictedTop()
    {
      if( Top < 0 )
        return 0;

      if( ( _parentContainer != null ) && ( _windowRoot != null ) )
      {
        if( Top + _windowRoot.ActualHeight > _parentContainer.ActualHeight && _parentContainer.ActualHeight != 0 )
        {
          double top = _parentContainer.ActualHeight - _windowRoot.ActualHeight;
          return top < 0 ? 0 : top;
        }
      }

      return Top;
    }

    private void SetWindowState( WindowState state )
    {
      switch( state )
      {
        case WindowState.Closed:
          {
            ExecuteClose();
            break;
          }
        case WindowState.Open:
          {
            ExecuteOpen();
            break;
          }
      }
    }

    private void ExecuteClose()
    {
      CancelEventArgs e = new CancelEventArgs();
      OnClosing( e );

      if( !e.Cancel )
      {
        if( !_dialogResult.HasValue )
          _dialogResult = false;

        OnClosed( EventArgs.Empty );
      }
      else
      {
        CancelClose();
      }
    }

    private void CancelClose()
    {
      _dialogResult = null; //when the close is cancelled, DialogResult should be null

      _ignorePropertyChanged = true;
      WindowState = WindowState.Open; //now reset the window state to open because the close was cancelled
      _ignorePropertyChanged = false;
    }

    private void ExecuteOpen()
    {
      _dialogResult = null; //reset the dialogResult to null each time the window is opened

      if( !_hasWindowContainer )
        if( WindowStartupLocation == Xceed.Wpf.Toolkit.WindowStartupLocation.Center )
          CenterChildWindow(); 

      if( !_hasWindowContainer )
        BringToFront();
    }

    private bool IsCurrentWindow( object windowtoTest )
    {
      return object.Equals( _windowControl, windowtoTest );
    }

    [Obsolete( "This method is obsolete and should no longer be used. Use WindowContainer.BringToFront() instead." )]
    private void BringToFront()
    {
      int index = 0;

      if( _parentContainer != null )
        index = ( int )_parentContainer.GetValue( Canvas.ZIndexProperty );

      SetValue( Canvas.ZIndexProperty, ++index );

      if( IsModal )
        Canvas.SetZIndex( _modalLayerPanel, index - 2 );
    }

    [Obsolete( "This method is obsolete and should no longer be used. Use WindowContainer.CenterChild() instead." )]
    private void CenterChildWindow()
    {
      if( ( _parentContainer != null ) && ( _windowRoot != null ) )
      {
        _windowRoot.UpdateLayout();

        Left = ( _parentContainer.ActualWidth - _windowRoot.ActualWidth ) / 2.0;
        Top = ( _parentContainer.ActualHeight - _windowRoot.ActualHeight ) / 2.0;
      }
    }

    [Obsolete( "This method is obsolete and should no longer be used." )]
    private void ShowModalLayer()
    {
      if( !DesignerProperties.GetIsInDesignMode( this ) )
      {
        if( !_modalLayerPanel.Children.Contains( _modalLayer ) )
          _modalLayerPanel.Children.Add( _modalLayer );

        _modalLayer.Visibility = System.Windows.Visibility.Visible;
      }
    }

    [Obsolete( "This method is obsolete and should no longer be used." )]
    private void HideModalLayer()
    {
      _modalLayer.Visibility = System.Windows.Visibility.Collapsed;
    }

    [Obsolete( "This method is obsolete and should no longer be used. Use the ChildWindow in a WindowContainer instead." )]
    private void ProcessMove( double x, double y )
    {
      _moveTransform.X += x;
      _moveTransform.Y += y;

      InvalidateArrange();
    }

    #endregion //Private

    #region Public

    public void Show()
    {
      WindowState = WindowState.Open;
    }

    public void Close()
    {
      WindowState = WindowState.Closed;
    }

    #endregion //Public

    #endregion //Methods

    #region Events

    /// <summary>
    /// Occurs when the ChildWindow is closed.
    /// </summary>
    public event EventHandler Closed;
    protected virtual void OnClosed( EventArgs e )
    {
      if( Closed != null )
        Closed( this, e );
    }

    /// <summary>
    /// Occurs when the ChildWindow is closing.
    /// </summary>
    public event EventHandler<CancelEventArgs> Closing;
    protected virtual void OnClosing( CancelEventArgs e )
    {
      if( Closing != null )
        Closing( this, e );
    }

    #endregion //Events

  }

#pragma warning restore 0809
#pragma warning restore 0618
}
