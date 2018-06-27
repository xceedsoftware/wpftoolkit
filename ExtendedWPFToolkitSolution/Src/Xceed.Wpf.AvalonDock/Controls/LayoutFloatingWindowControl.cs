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
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Documents;
using Xceed.Wpf.AvalonDock.Themes;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public abstract class LayoutFloatingWindowControl : Window, ILayoutControl
  {
    #region Members

    private ResourceDictionary currentThemeResourceDictionary; // = null
    private bool _isInternalChange; //false
    private ILayoutElement _model;
    private bool _attachDrag = false;
    private HwndSource _hwndSrc;
    private HwndSourceHook _hwndSrcHook;
    private DragService _dragService = null;
    private bool _internalCloseFlag = false;

    #endregion

    #region Constructors

    static LayoutFloatingWindowControl()
    {
      LayoutFloatingWindowControl.ContentProperty.OverrideMetadata( typeof( LayoutFloatingWindowControl ), new FrameworkPropertyMetadata( null, null, new CoerceValueCallback( CoerceContentValue ) ) );
      AllowsTransparencyProperty.OverrideMetadata( typeof( LayoutFloatingWindowControl ), new FrameworkPropertyMetadata( false ) );
      ShowInTaskbarProperty.OverrideMetadata( typeof( LayoutFloatingWindowControl ), new FrameworkPropertyMetadata( false ) );
    }

    protected LayoutFloatingWindowControl( ILayoutElement model )
    {
      this.Loaded += new RoutedEventHandler( OnLoaded );
      this.Unloaded += new RoutedEventHandler( OnUnloaded );
      _model = model;
    }

    #endregion

    #region Properties

    #region Model

    public abstract ILayoutElement Model
    {
      get;
    }

    #endregion

    #region IsDragging

    /// <summary>
    /// IsDragging Read-Only Dependency Property
    /// </summary>
    private static readonly DependencyPropertyKey IsDraggingPropertyKey = DependencyProperty.RegisterReadOnly( "IsDragging", typeof( bool ), typeof( LayoutFloatingWindowControl ),
            new FrameworkPropertyMetadata( ( bool )false, new PropertyChangedCallback( OnIsDraggingChanged ) ) );

    public static readonly DependencyProperty IsDraggingProperty = IsDraggingPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the IsDragging property.  This dependency property 
    /// indicates that this floating window is being dragged.
    /// </summary>
    public bool IsDragging
    {
      get
      {
        return ( bool )GetValue( IsDraggingProperty );
      }
    }

    /// <summary>
    /// Provides a secure method for setting the IsDragging property.  
    /// This dependency property indicates that this floating window is being dragged.
    /// </summary>
    /// <param name="value">The new value for the property.</param>
    protected void SetIsDragging( bool value )
    {
      SetValue( IsDraggingPropertyKey, value );
    }

    /// <summary>
    /// Handles changes to the IsDragging property.
    /// </summary>
    private static void OnIsDraggingChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutFloatingWindowControl )d ).OnIsDraggingChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the IsDragging property.
    /// </summary>
    protected virtual void OnIsDraggingChanged( DependencyPropertyChangedEventArgs e )
    {
      if( ( bool )e.NewValue )
      {
        CaptureMouse();
      }
      else
      {
        ReleaseMouseCapture();
      }
    }

    #endregion

    #region CloseInitiatedByUser

    protected bool CloseInitiatedByUser
    {
      get
      {
        return !_internalCloseFlag;
      }
    }

    #endregion

    #region KeepContentVisibleOnClose

    internal bool KeepContentVisibleOnClose
    {
      get;
      set;
    }

    #endregion

    #region IsMaximized

    /// <summary>
    /// IsMaximized Dependency Property
    /// </summary>
    public static readonly DependencyProperty IsMaximizedProperty = DependencyProperty.Register( "IsMaximized", typeof( bool ), typeof( LayoutFloatingWindowControl ),
              new FrameworkPropertyMetadata( ( bool )false ) );

    /// <summary>
    /// Gets/sets the IsMaximized property.  This dependency property 
    /// indicates if the window is maximized.
    /// </summary>
    public bool IsMaximized
    {
      get
      {
        return ( bool )GetValue( IsMaximizedProperty );
      }
      private set
      {
        SetValue( IsMaximizedProperty, value );
        UpdatePositionAndSizeOfPanes();
      }
    }

    /// <summary>
    /// Provides a secure method for setting the IsMaximized property.  
    /// This dependency property indicates if the window is maximized.
    /// </summary>
    /// <param name="value">The new value for the property.</param>

    protected override void OnStateChanged( EventArgs e )
    {
      if( !_isInternalChange )
      {
        if( WindowState == WindowState.Maximized )
        {
          UpdateMaximizedState( true );
        }
        else
        {
          WindowState = IsMaximized ? WindowState.Maximized : WindowState.Normal;
        }
      }

      base.OnStateChanged( e );
    }

    #endregion

    #endregion

    #region Overrides

    protected override void OnClosed( EventArgs e )
    {
      if( Content != null )
      {
        var host = Content as FloatingWindowContentHost;
        host.Dispose();

        if( _hwndSrc != null )
        {
          _hwndSrc.RemoveHook( _hwndSrcHook );
          _hwndSrc.Dispose();
          _hwndSrc = null;
        }
      }

      base.OnClosed( e );
    }

    protected override void OnInitialized( EventArgs e )
    {
      CommandBindings.Add( new CommandBinding( Microsoft.Windows.Shell.SystemCommands.CloseWindowCommand,
          new ExecutedRoutedEventHandler( ( s, args ) => Microsoft.Windows.Shell.SystemCommands.CloseWindow( ( Window )args.Parameter ) ) ) );
      CommandBindings.Add( new CommandBinding( Microsoft.Windows.Shell.SystemCommands.MaximizeWindowCommand,
          new ExecutedRoutedEventHandler( ( s, args ) => Microsoft.Windows.Shell.SystemCommands.MaximizeWindow( ( Window )args.Parameter ) ) ) );
      CommandBindings.Add( new CommandBinding( Microsoft.Windows.Shell.SystemCommands.MinimizeWindowCommand,
          new ExecutedRoutedEventHandler( ( s, args ) => Microsoft.Windows.Shell.SystemCommands.MinimizeWindow( ( Window )args.Parameter ) ) ) );
      CommandBindings.Add( new CommandBinding( Microsoft.Windows.Shell.SystemCommands.RestoreWindowCommand,
          new ExecutedRoutedEventHandler( ( s, args ) => Microsoft.Windows.Shell.SystemCommands.RestoreWindow( ( Window )args.Parameter ) ) ) );
      //Debug.Assert(this.Owner != null);
      base.OnInitialized( e );
    }


    #endregion

    #region Internal Methods

    internal virtual void UpdateThemeResources( Theme oldTheme = null )
    {
      if( oldTheme != null )
      {
        if( oldTheme is DictionaryTheme )
        {
          if( currentThemeResourceDictionary != null )
          {
            Resources.MergedDictionaries.Remove( currentThemeResourceDictionary );
            currentThemeResourceDictionary = null;
          }
        }
        else
        {
          var resourceDictionaryToRemove =
              Resources.MergedDictionaries.FirstOrDefault( r => r.Source == oldTheme.GetResourceUri() );
          if( resourceDictionaryToRemove != null )
            Resources.MergedDictionaries.Remove(
                resourceDictionaryToRemove );
        }
      }

      var manager = _model.Root.Manager;
      if( manager.Theme != null )
      {
        if( manager.Theme is DictionaryTheme )
        {
          currentThemeResourceDictionary = ( ( DictionaryTheme )manager.Theme ).ThemeResourceDictionary;
          Resources.MergedDictionaries.Add( currentThemeResourceDictionary );
        }
        else
        {
          Resources.MergedDictionaries.Add( new ResourceDictionary() { Source = manager.Theme.GetResourceUri() } );
        }
      }
    }

    internal void AttachDrag( bool onActivated = true )
    {
      if( onActivated )
      {
        _attachDrag = true;
        this.Activated += new EventHandler( OnActivated );
      }
      else
      {
        IntPtr windowHandle = new WindowInteropHelper( this ).Handle;
        IntPtr lParam = new IntPtr( ( ( int )Left & ( int )0xFFFF ) | ( ( ( int )Top ) << 16 ) );
        Win32Helper.SendMessage( windowHandle, Win32Helper.WM_NCLBUTTONDOWN, new IntPtr( Win32Helper.HT_CAPTION ), lParam );
      }
    }

    protected virtual IntPtr FilterMessage( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
    {
      handled = false;

      switch( msg )
      {
        case Win32Helper.WM_ACTIVATE:
          if( ( ( int )wParam & 0xFFFF ) == Win32Helper.WA_INACTIVE )
          {
            if( lParam == this.GetParentWindowHandle() )
            {
              Win32Helper.SetActiveWindow( _hwndSrc.Handle );
              handled = true;
            }
          }
          break;
        case Win32Helper.WM_EXITSIZEMOVE:
          UpdatePositionAndSizeOfPanes();

          if( _dragService != null )
          {
            bool dropFlag;
            var mousePosition = this.TransformToDeviceDPI( Win32Helper.GetMousePosition() );
            _dragService.Drop( mousePosition, out dropFlag );
            _dragService = null;
            SetIsDragging( false );

            if( dropFlag )
              InternalClose();
          }

          break;
        case Win32Helper.WM_MOVING:
          {
            UpdateDragPosition();
            if( this.IsMaximized )
            {
              this.UpdateMaximizedState( false );
            }
          }
          break;
        case Win32Helper.WM_LBUTTONUP: //set as handled right button click on title area (after showing context menu)
          if( _dragService != null && Mouse.LeftButton == MouseButtonState.Released )
          {
            _dragService.Abort();
            _dragService = null;
            SetIsDragging( false );
          }
          break;
        case Win32Helper.WM_SYSCOMMAND:
          int command = ( int )wParam & 0xFFF0;
          if( command == Win32Helper.SC_MAXIMIZE || command == Win32Helper.SC_RESTORE )
          {
            UpdateMaximizedState( command == Win32Helper.SC_MAXIMIZE );
          }
          break;
      }



      return IntPtr.Zero;
    }

    internal void InternalClose()
    {
      _internalCloseFlag = true;
      Close();
    }

    #endregion

    #region Private Methods

    private static object CoerceContentValue( DependencyObject sender, object content )
    {
      return new FloatingWindowContentHost( sender as LayoutFloatingWindowControl ) { Content = content as UIElement };
    }

    private void OnLoaded( object sender, RoutedEventArgs e )
    {
      this.Loaded -= new RoutedEventHandler( OnLoaded );

      this.SetParentToMainWindowOf( Model.Root.Manager );

      _hwndSrc = HwndSource.FromDependencyObject( this ) as HwndSource;
      _hwndSrcHook = new HwndSourceHook( FilterMessage );
      _hwndSrc.AddHook( _hwndSrcHook );

      // Restore maximize state
      var maximized = Model.Descendents().OfType<ILayoutElementForFloatingWindow>().Any( l => l.IsMaximized );
      UpdateMaximizedState( maximized );
    }

    private void OnUnloaded( object sender, RoutedEventArgs e )
    {
      this.Unloaded -= new RoutedEventHandler( OnUnloaded );

      if( _hwndSrc != null )
      {
        _hwndSrc.RemoveHook( _hwndSrcHook );
        InternalClose();
      }
    }

    private void OnActivated( object sender, EventArgs e )
    {
      this.Activated -= new EventHandler( OnActivated );

      if( _attachDrag && Mouse.LeftButton == MouseButtonState.Pressed )
      {
        IntPtr windowHandle = new WindowInteropHelper( this ).Handle;
        var mousePosition = this.PointToScreenDPI( Mouse.GetPosition( this ) );
        var clientArea = Win32Helper.GetClientRect( windowHandle );
        var windowArea = Win32Helper.GetWindowRect( windowHandle );

        Left = mousePosition.X - windowArea.Width / 2.0;
        Top = mousePosition.Y - ( windowArea.Height - clientArea.Height ) / 2.0;
        _attachDrag = false;

        IntPtr lParam = new IntPtr( ( ( int )mousePosition.X & ( int )0xFFFF ) | ( ( ( int )mousePosition.Y ) << 16 ) );
        Win32Helper.SendMessage( windowHandle, Win32Helper.WM_NCLBUTTONDOWN, new IntPtr( Win32Helper.HT_CAPTION ), lParam );
      }
    }

    private void UpdatePositionAndSizeOfPanes()
    {
      foreach( var posElement in Model.Descendents().OfType<ILayoutElementForFloatingWindow>() )
      {
        posElement.FloatingLeft = Left;
        posElement.FloatingTop = Top;
        posElement.FloatingWidth = Width;
        posElement.FloatingHeight = Height;
      }
    }

    private void UpdateMaximizedState( bool isMaximized )
    {
      foreach( var posElement in Model.Descendents().OfType<ILayoutElementForFloatingWindow>() )
      {
        posElement.IsMaximized = isMaximized;
      }
      IsMaximized = isMaximized;
      _isInternalChange = true;
      WindowState = isMaximized ? WindowState.Maximized : WindowState.Normal;
      _isInternalChange = false;
    }

    private void UpdateDragPosition()
    {
      if( _dragService == null )
      {
        _dragService = new DragService( this );
        SetIsDragging( true );
      }

      var mousePosition = this.TransformToDeviceDPI( Win32Helper.GetMousePosition() );
      _dragService.UpdateMouseLocation( mousePosition );
    }

    #endregion

    #region Internal Classes

    protected internal class FloatingWindowContentHost : HwndHost
    {
      #region Members

      private LayoutFloatingWindowControl _owner;
      private HwndSource _wpfContentHost = null;
      private Border _rootPresenter = null;
      private DockingManager _manager = null;

      #endregion

      #region Constructors

      public FloatingWindowContentHost( LayoutFloatingWindowControl owner )
      {
        _owner = owner;
        var manager = _owner.Model.Root.Manager;
      }

      #endregion

      #region Properties

      #region RootVisual

      public Visual RootVisual
      {
        get
        {
          return _rootPresenter;
        }
      }

      #endregion

      #region Content

      /// <summary>
      /// Content Dependency Property
      /// </summary>
      public static readonly DependencyProperty ContentProperty = DependencyProperty.Register( "Content", typeof( UIElement ), typeof( FloatingWindowContentHost ),
              new FrameworkPropertyMetadata( ( UIElement )null, new PropertyChangedCallback( OnContentChanged ) ) );

      /// <summary>
      /// Gets or sets the Content property.  This dependency property 
      /// indicates ....
      /// </summary>
      public UIElement Content
      {
        get
        {
          return ( UIElement )GetValue( ContentProperty );
        }
        set
        {
          SetValue( ContentProperty, value );
        }
      }

      /// <summary>
      /// Handles changes to the Content property.
      /// </summary>
      private static void OnContentChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
      {
        ( ( FloatingWindowContentHost )d ).OnContentChanged( e );
      }

      /// <summary>
      /// Provides derived classes an opportunity to handle changes to the Content property.
      /// </summary>
      protected virtual void OnContentChanged( DependencyPropertyChangedEventArgs e )
      {
        if( _rootPresenter != null )
          _rootPresenter.Child = Content;
      }

      #endregion

      #endregion

      #region Overrides

      protected override System.Runtime.InteropServices.HandleRef BuildWindowCore( System.Runtime.InteropServices.HandleRef hwndParent )
      {
        _wpfContentHost = new HwndSource( new HwndSourceParameters()
        {
          ParentWindow = hwndParent.Handle,
          WindowStyle = Win32Helper.WS_CHILD | Win32Helper.WS_VISIBLE | Win32Helper.WS_CLIPSIBLINGS | Win32Helper.WS_CLIPCHILDREN,
          Width = 1,
          Height = 1
        } );

        _rootPresenter = new Border() { Child = new AdornerDecorator() { Child = Content }, Focusable = true };
        _rootPresenter.SetBinding( Border.BackgroundProperty, new Binding( "Background" ) { Source = _owner } );
        _wpfContentHost.RootVisual = _rootPresenter;
        _wpfContentHost.SizeToContent = SizeToContent.Manual;
        _manager = _owner.Model.Root.Manager;
        _manager.InternalAddLogicalChild( _rootPresenter );

        return new HandleRef( this, _wpfContentHost.Handle );
      }

      protected override void DestroyWindowCore( HandleRef hwnd )
      {
        _manager.InternalRemoveLogicalChild( _rootPresenter );
        if( _wpfContentHost != null )
        {
          _wpfContentHost.Dispose();
          _wpfContentHost = null;
        }
      }

      protected override Size MeasureOverride( Size constraint )
      {
        if( Content == null )
          return base.MeasureOverride( constraint );

        Content.Measure( constraint );
        return Content.DesiredSize;
      }

      #endregion
    }

    #endregion
  }
}
