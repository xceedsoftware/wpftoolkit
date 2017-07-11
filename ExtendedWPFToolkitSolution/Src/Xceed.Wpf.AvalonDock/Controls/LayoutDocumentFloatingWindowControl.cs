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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.Windows.Shell;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class LayoutDocumentFloatingWindowControl : LayoutFloatingWindowControl
  {
    static LayoutDocumentFloatingWindowControl()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( LayoutDocumentFloatingWindowControl ), new FrameworkPropertyMetadata( typeof( LayoutDocumentFloatingWindowControl ) ) );
    }

    internal LayoutDocumentFloatingWindowControl( LayoutDocumentFloatingWindow model )
        : base( model )
    {
      _model = model;
      UpdateThemeResources();
    }


    LayoutDocumentFloatingWindow _model;

    public override ILayoutElement Model
    {
      get
      {
        return _model;
      }
    }

    public LayoutItem RootDocumentLayoutItem
    {
      get
      {
        return _model.Root.Manager.GetLayoutItemFromModel( _model.RootDocument );
      }
    }

    protected override void OnInitialized( EventArgs e )
    {
      base.OnInitialized( e );

      if( _model.RootDocument == null )
      {
        InternalClose();
      }
      else
      {
        var manager = _model.Root.Manager;

        Content = manager.CreateUIElementForModel( _model.RootDocument );

        _model.RootDocumentChanged += new EventHandler( _model_RootDocumentChanged );
      }
    }

    void _model_RootDocumentChanged( object sender, EventArgs e )
    {
      if( _model.RootDocument == null )
      {
        InternalClose();
      }
    }

    protected override IntPtr FilterMessage( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
    {
      switch( msg )
      {
        case Win32Helper.WM_NCLBUTTONDOWN: //Left button down on title -> start dragging over docking manager
          if( wParam.ToInt32() == Win32Helper.HT_CAPTION )
          {
            if( _model.RootDocument != null )
              _model.RootDocument.IsActive = true;
          }
          break;
        case Win32Helper.WM_NCRBUTTONUP:
          if( wParam.ToInt32() == Win32Helper.HT_CAPTION )
          {
            if( OpenContextMenu() )
              handled = true;
            if( _model.Root.Manager.ShowSystemMenu )
              WindowChrome.GetWindowChrome( this ).ShowSystemMenu = !handled;
            else
              WindowChrome.GetWindowChrome( this ).ShowSystemMenu = false;
          }
          break;

      }

      return base.FilterMessage( hwnd, msg, wParam, lParam, ref handled );
    }

    bool OpenContextMenu()
    {
      var ctxMenu = _model.Root.Manager.DocumentContextMenu;
      if( ctxMenu != null && RootDocumentLayoutItem != null )
      {
        ctxMenu.PlacementTarget = null;
        ctxMenu.Placement = PlacementMode.MousePoint;
        ctxMenu.DataContext = RootDocumentLayoutItem;
        ctxMenu.IsOpen = true;
        return true;
      }

      return false;
    }


    protected override void OnClosed( EventArgs e )
    {
      var root = Model.Root;
      root.Manager.RemoveFloatingWindow( this );
      root.CollectGarbage();

      base.OnClosed( e );

      if( !CloseInitiatedByUser )
      {
        root.FloatingWindows.Remove( _model );
      }

      _model.RootDocumentChanged -= new EventHandler( _model_RootDocumentChanged );
    }

  }
}
