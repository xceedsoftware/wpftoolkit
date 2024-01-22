/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2023 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  internal static class FocusElementManager
  {
    #region Member

    private static List<DockingManager> _managers = new List<DockingManager>();
    private static FullWeakDictionary<ILayoutElement, IInputElement> _modelFocusedElement = new FullWeakDictionary<ILayoutElement, IInputElement>();
    private static WeakDictionary<ILayoutElement, IntPtr> _modelFocusedWindowHandle = new WeakDictionary<ILayoutElement, IntPtr>();
    private static WeakReference _lastFocusedElement;
    private static WindowHookHandler _windowHandler = null;
    private static DispatcherOperation _setFocusAsyncOperation;
    private static WeakReference _lastFocusedElementBeforeEnterMenuMode = null;

    #endregion

    #region Internal Methods

    internal static void SetupFocusManagement( DockingManager manager )
    {
      if( _managers.Count == 0 )
      {
        //InputManager.Current.EnterMenuMode += new EventHandler(InputManager_EnterMenuMode);
        //InputManager.Current.LeaveMenuMode += new EventHandler(InputManager_LeaveMenuMode);
        _windowHandler = new WindowHookHandler();
        _windowHandler.FocusChanged += new EventHandler<FocusChangeEventArgs>( WindowFocusChanging );
        //_windowHandler.Activate += new EventHandler<WindowActivateEventArgs>(WindowActivating);
        _windowHandler.Attach();

        if( Application.Current != null )
        {
          var currentDispatcher = Application.Current.Dispatcher;
          if( currentDispatcher != null )
          {
            if( currentDispatcher.CheckAccess() )
            {
              Application.Current.Exit += new ExitEventHandler( Current_Exit );
            }
            else
            {
              var disableProcessingCountFieldInfo = typeof( Dispatcher ).GetField( "_disableProcessingCount", BindingFlags.Instance | BindingFlags.NonPublic );
              if( disableProcessingCountFieldInfo != null )
              {
                var disableProcessingCountFieldInfoValue = disableProcessingCountFieldInfo.GetValue( currentDispatcher );
                if( ( disableProcessingCountFieldInfoValue != null ) && ( disableProcessingCountFieldInfoValue is int ) )
                {
                  var action = new Action( () => Application.Current.Exit += new ExitEventHandler( Current_Exit ) );

                  if( ( int )disableProcessingCountFieldInfoValue == 0 )
                  {
                    // in sync
                    currentDispatcher.Invoke( DispatcherPriority.Normal, action );
                  }
                  else
                  {
                    // in async.
                    currentDispatcher.BeginInvoke( DispatcherPriority.Normal, action );
                  }
                }
              }
            }
          }

        }
      }

      manager.PreviewGotKeyboardFocus += new KeyboardFocusChangedEventHandler( manager_PreviewGotKeyboardFocus );
      _managers.Add( manager );
    }

    internal static void FinalizeFocusManagement( DockingManager manager )
    {
      manager.PreviewGotKeyboardFocus -= new KeyboardFocusChangedEventHandler( manager_PreviewGotKeyboardFocus );
      _managers.Remove( manager );

      if( _managers.Count == 0 )
      {
        //InputManager.Current.EnterMenuMode -= new EventHandler(InputManager_EnterMenuMode);
        //InputManager.Current.LeaveMenuMode -= new EventHandler(InputManager_LeaveMenuMode);
        if( _windowHandler != null )
        {
          _windowHandler.FocusChanged -= new EventHandler<FocusChangeEventArgs>( WindowFocusChanging );
          //_windowHandler.Activate -= new EventHandler<WindowActivateEventArgs>(WindowActivating);
          _windowHandler.Detach();
          _windowHandler = null;
        }
      }

    }

    internal static IInputElement GetLastFocusedElement( ILayoutElement model )
    {
      IInputElement objectWithFocus;
      if( _modelFocusedElement.GetValue( model, out objectWithFocus ) )
        return objectWithFocus;

      return null;
    }


    internal static IntPtr GetLastWindowHandle( ILayoutElement model )
    {
      IntPtr handleWithFocus;
      if( _modelFocusedWindowHandle.GetValue( model, out handleWithFocus ) )
        return handleWithFocus;

      return IntPtr.Zero;
    }

    internal static void SetFocusOnLastElement( ILayoutElement model )
    {
      bool focused = false;
      IInputElement objectToFocus;
      if( _modelFocusedElement.GetValue( model, out objectToFocus ) )
      {
        focused = objectToFocus == Keyboard.Focus( objectToFocus );
      }

      IntPtr handleToFocus;
      if( _modelFocusedWindowHandle.GetValue( model, out handleToFocus ) )
        focused = IntPtr.Zero != Win32Helper.SetFocus( handleToFocus );


      if( focused )
      {
        _lastFocusedElement = new WeakReference( model );
      }

    }

    #endregion

    #region Private Methods

    private static void Current_Exit( object sender, ExitEventArgs e )
    {
      Application.Current.Exit -= new ExitEventHandler( Current_Exit );
      if( _windowHandler != null )
      {
        _windowHandler.FocusChanged -= new EventHandler<FocusChangeEventArgs>( WindowFocusChanging );
        //_windowHandler.Activate -= new EventHandler<WindowActivateEventArgs>(WindowActivating);
        _windowHandler.Detach();
        _windowHandler = null;
      }
    }

    private static void manager_PreviewGotKeyboardFocus( object sender, KeyboardFocusChangedEventArgs e )
    {
      var focusedElement = e.NewFocus as Visual;
      if( focusedElement != null &&
          !( focusedElement is LayoutAnchorableTabItem || focusedElement is LayoutDocumentTabItem ) )
      //Avoid tracking focus for elements like this
      {
        var parentAnchorable = focusedElement.FindVisualAncestor<LayoutAnchorableControl>();
        if( parentAnchorable != null )
        {
          _modelFocusedElement[ parentAnchorable.Model ] = e.NewFocus;
        }
        else
        {
          var parentDocument = focusedElement.FindVisualAncestor<LayoutDocumentControl>();
          if( parentDocument != null )
          {
            _modelFocusedElement[ parentDocument.Model ] = e.NewFocus;
          }
        }
      }
    }

    private static void WindowFocusChanging( object sender, FocusChangeEventArgs e )
    {
      foreach( var manager in _managers )
      {
        var hostContainingFocusedHandle = manager.FindLogicalChildren<HwndHost>().FirstOrDefault( hw => Win32Helper.IsChild( hw.Handle, e.GotFocusWinHandle ) );

        if( hostContainingFocusedHandle != null )
        {
          var parentAnchorable = hostContainingFocusedHandle.FindVisualAncestor<LayoutAnchorableControl>();
          if( parentAnchorable != null )
          {
            _modelFocusedWindowHandle[ parentAnchorable.Model ] = e.GotFocusWinHandle;
            if( parentAnchorable.Model != null )
              parentAnchorable.Model.IsActive = true;
          }
          else
          {
            var parentDocument = hostContainingFocusedHandle.FindVisualAncestor<LayoutDocumentControl>();
            if( parentDocument != null )
            {
              _modelFocusedWindowHandle[ parentDocument.Model ] = e.GotFocusWinHandle;
              if( parentDocument.Model != null )
                parentDocument.Model.IsActive = true;
            }
          }
        }
      }
    }

    private static void WindowActivating( object sender, WindowActivateEventArgs e )
    {
      if( Keyboard.FocusedElement == null &&
          _lastFocusedElement != null &&
          _lastFocusedElement.IsAlive )
      {
        var elementToSetFocus = _lastFocusedElement.Target as ILayoutElement;
        if( elementToSetFocus != null )
        {
          var manager = elementToSetFocus.Root.Manager;
          if( manager == null )
            return;

          IntPtr parentHwnd;
          if( !manager.GetParentWindowHandle( out parentHwnd ) )
            return;

          if( e.HwndActivating != parentHwnd )
            return;

          _setFocusAsyncOperation = Dispatcher.CurrentDispatcher.BeginInvoke( new Action( () =>
          {
            try
            {
              SetFocusOnLastElement( elementToSetFocus );
            }
            finally
            {
              _setFocusAsyncOperation = null;
            }
          } ), DispatcherPriority.Input );
        }
      }
    }

    private static void InputManager_EnterMenuMode( object sender, EventArgs e )
    {
      if( Keyboard.FocusedElement == null )
        return;

      var lastfocusDepObj = Keyboard.FocusedElement as DependencyObject;
      if( lastfocusDepObj.FindLogicalAncestor<DockingManager>() == null )
      {
        _lastFocusedElementBeforeEnterMenuMode = null;
        return;
      }

      _lastFocusedElementBeforeEnterMenuMode = new WeakReference( Keyboard.FocusedElement );
    }
    private static void InputManager_LeaveMenuMode( object sender, EventArgs e )
    {
      if( _lastFocusedElementBeforeEnterMenuMode != null &&
          _lastFocusedElementBeforeEnterMenuMode.IsAlive )
      {
        var lastFocusedInputElement = _lastFocusedElementBeforeEnterMenuMode.GetValueOrDefault<UIElement>();
        if( lastFocusedInputElement != null )
        {
          if( lastFocusedInputElement != Keyboard.Focus( lastFocusedInputElement ) )
            Debug.WriteLine( "Unable to activate the element" );
        }
      }
    }

    #endregion
  }
}
