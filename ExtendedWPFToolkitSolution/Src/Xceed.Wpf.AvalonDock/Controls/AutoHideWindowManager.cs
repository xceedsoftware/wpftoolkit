/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2024 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  internal class AutoHideWindowManager
  {
    #region Members

    private DockingManager _manager;
    private WeakReference _currentAutohiddenAnchor = null;
    private DispatcherTimer _closingTimer = null;
    private DispatcherTimer _closeTimer = null;

    #endregion

    #region Constructors

    internal AutoHideWindowManager( DockingManager manager )
    {
      _manager = manager;
      this.SetupClosingTimer();
      this.SetupCloseTimer();
    }

    internal void UpdateCloseTimerInterval( int newValue )
    {
      _closeTimer.Interval = TimeSpan.FromMilliseconds( newValue );
    }

    #endregion

    #region Private Methods

    public void ShowAutoHideWindow( LayoutAnchorControl anchor )
    {
      if( _currentAutohiddenAnchor.GetValueOrDefault<LayoutAnchorControl>() != anchor )
      {
        this.StopClosingTimer();
        this.StopCloseTimer();
        _currentAutohiddenAnchor = new WeakReference( anchor );
        _manager.AutoHideWindow.Show( anchor );
        this.StartClosingTimer();
      }
    }

    public void HideAutoWindow( LayoutAnchorControl anchor = null )
    {
      if( anchor == null ||
          anchor == _currentAutohiddenAnchor.GetValueOrDefault<LayoutAnchorControl>() )
      {
        this.StopClosingTimer();
        this.StopCloseTimer();
      }
      else
        System.Diagnostics.Debug.Assert( false );
    }

    private void SetupClosingTimer()
    {
      _closingTimer = new DispatcherTimer( DispatcherPriority.Background );
      _closingTimer.Interval = TimeSpan.FromMilliseconds( 50 );
      _closingTimer.Tick += ( s, e ) =>
      {
        if( _manager.AutoHideWindow.IsWin32MouseOver
          || ( ( LayoutAnchorable )_manager.AutoHideWindow.Model ).IsActive
          || _manager.AutoHideWindow.IsResizing )
          return;

        this.StopClosingTimer();
        this.StartCloseTimer();
      };
    }

    private void StartClosingTimer()
    {
      _closingTimer.Start();
    }

    private void StopClosingTimer()
    {
      _closingTimer.Stop();
    }

    private void SetupCloseTimer()
    {
      _closeTimer = new DispatcherTimer( DispatcherPriority.Background );
      _closeTimer.Interval = TimeSpan.FromMilliseconds( _manager.AutoHideWindowClosingTimer );
      _closeTimer.Tick += ( s, e ) =>
      {
        if( _manager.AutoHideWindow.IsWin32MouseOver
          || ( ( LayoutAnchorable )_manager.AutoHideWindow.Model ).IsActive
          || _manager.AutoHideWindow.IsResizing )
        {
          _closeTimer.Stop();
          this.StartClosingTimer();
          return;
        }

        this.StopCloseTimer();
      };
    }

    private void StartCloseTimer()
    {
      _closeTimer.Start();
    }

    private void StopCloseTimer()
    {
      _closeTimer.Stop();
      _manager.AutoHideWindow.Hide();
      _currentAutohiddenAnchor = null;
    }

    #endregion
  }
}
