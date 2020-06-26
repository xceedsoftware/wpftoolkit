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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  internal class DragService
  {
    #region Members

    private DockingManager _manager;
    private LayoutFloatingWindowControl _floatingWindow;
    private List<IOverlayWindowHost> _overlayWindowHosts = new List<IOverlayWindowHost>();
    private IOverlayWindowHost _currentHost;
    private IOverlayWindow _currentWindow;
    private List<IDropArea> _currentWindowAreas = new List<IDropArea>();
    private IDropTarget _currentDropTarget;

    #endregion

    #region Public Methods

    public DragService( LayoutFloatingWindowControl floatingWindow )
    {
      _floatingWindow = floatingWindow;
      _manager = floatingWindow.Model.Root.Manager;


      GetOverlayWindowHosts();
    }

    public void UpdateMouseLocation( Point dragPosition )
    {
      var floatingWindowModel = _floatingWindow.Model as LayoutFloatingWindow;

      var newHost = _overlayWindowHosts.FirstOrDefault( oh => oh.HitTest( dragPosition ) );

      if( _currentHost != null || _currentHost != newHost )
      {
        //is mouse still inside current overlay window host?
        if( ( _currentHost != null && !_currentHost.HitTest( dragPosition ) ) ||
            _currentHost != newHost )
        {
          //esit drop target
          if( _currentDropTarget != null )
            _currentWindow.DragLeave( _currentDropTarget );
          _currentDropTarget = null;

          //exit area
          _currentWindowAreas.ForEach( a =>
               _currentWindow.DragLeave( a ) );
          _currentWindowAreas.Clear();

          //hide current overlay window
          if( _currentWindow != null )
            _currentWindow.DragLeave( _floatingWindow );
          if( _currentHost != null )
            _currentHost.HideOverlayWindow();
          _currentHost = null;
        }

        if( _currentHost != newHost )
        {
          _currentHost = newHost;
          _currentWindow = _currentHost.ShowOverlayWindow( _floatingWindow );
          _currentWindow.DragEnter( _floatingWindow );
        }
      }

      if( _currentHost == null )
        return;

      if( _currentDropTarget != null &&
          !_currentDropTarget.HitTest( dragPosition ) )
      {
        _currentWindow.DragLeave( _currentDropTarget );
        _currentDropTarget = null;
      }

      List<IDropArea> areasToRemove = new List<IDropArea>();
      _currentWindowAreas.ForEach( a =>
      {
        //is mouse still inside this area?
        if( !a.DetectionRect.Contains( dragPosition ) )
        {
          _currentWindow.DragLeave( a );
          areasToRemove.Add( a );
        }
      } );

      areasToRemove.ForEach( a =>
           _currentWindowAreas.Remove( a ) );


      var areasToAdd =
          _currentHost.GetDropAreas( _floatingWindow ).Where( cw => !_currentWindowAreas.Contains( cw ) && cw.DetectionRect.Contains( dragPosition ) ).ToList();

      _currentWindowAreas.AddRange( areasToAdd );

      areasToAdd.ForEach( a =>
           _currentWindow.DragEnter( a ) );

      if( _currentDropTarget == null )
      {
        _currentWindowAreas.ForEach( wa =>
        {
          if( _currentDropTarget != null )
            return;

          _currentDropTarget = _currentWindow.GetTargets().FirstOrDefault( dt => dt.HitTest( dragPosition ) );
          if( _currentDropTarget != null )
          {
            _currentWindow.DragEnter( _currentDropTarget );
            return;
          }
        } );
      }

    }

    public void Drop( Point dropLocation, out bool dropHandled )
    {
      dropHandled = false;

      UpdateMouseLocation( dropLocation );

      var floatingWindowModel = _floatingWindow.Model as LayoutFloatingWindow;
      var root = floatingWindowModel.Root;

      if( _currentHost != null )
        _currentHost.HideOverlayWindow();

      if( _currentDropTarget != null )
      {
        _currentWindow.DragDrop( _currentDropTarget );
        root.CollectGarbage();
        dropHandled = true;
      }


      _currentWindowAreas.ForEach( a => _currentWindow.DragLeave( a ) );

      if( _currentDropTarget != null )
        _currentWindow.DragLeave( _currentDropTarget );
      if( _currentWindow != null )
        _currentWindow.DragLeave( _floatingWindow );
      _currentWindow = null;

      _currentHost = null;
    }

    #endregion

    #region Internal Methods

    internal void Abort()
    {
      var floatingWindowModel = _floatingWindow.Model as LayoutFloatingWindow;

      _currentWindowAreas.ForEach( a => _currentWindow.DragLeave( a ) );

      if( _currentDropTarget != null )
        _currentWindow.DragLeave( _currentDropTarget );
      if( _currentWindow != null )
        _currentWindow.DragLeave( _floatingWindow );
      _currentWindow = null;
      if( _currentHost != null )
        _currentHost.HideOverlayWindow();
      _currentHost = null;
    }

    #endregion

    #region Private Methods

    private void GetOverlayWindowHosts()
    {
      _overlayWindowHosts.AddRange( _manager.GetFloatingWindowsByZOrder().OfType<LayoutAnchorableFloatingWindowControl>().Where( fw => fw != _floatingWindow && fw.IsVisible ) );
      _overlayWindowHosts.Add( _manager );
    }

    #endregion    
  }
}
