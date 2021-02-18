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
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  internal class DockingManagerDropTarget : DropTarget<DockingManager>
  {
    #region Members

    private DockingManager _manager;

    #endregion

    #region Constructors

    internal DockingManagerDropTarget( DockingManager manager, Rect detectionRect, DropTargetType type )
        : base( manager, detectionRect, type )
    {
      _manager = manager;
    }

    #endregion

    #region Overrides

    protected override void Drop( LayoutAnchorableFloatingWindow floatingWindow )
    {
      switch( Type )
      {
        case DropTargetType.DockingManagerDockLeft:
          #region DropTargetType.DockingManagerDockLeft
          {
            if( _manager.Layout.RootPanel.Orientation != System.Windows.Controls.Orientation.Horizontal &&
                _manager.Layout.RootPanel.Children.Count == 1 )
              _manager.Layout.RootPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

            if( _manager.Layout.RootPanel.Orientation == System.Windows.Controls.Orientation.Horizontal )
            {
              var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
              if( layoutAnchorablePaneGroup != null &&
                  layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal )
              {
                var childrenToTransfer = layoutAnchorablePaneGroup.Children.ToArray();
                for( int i = 0; i < childrenToTransfer.Length; i++ )
                  _manager.Layout.RootPanel.Children.Insert( i, childrenToTransfer[ i ] );
              }
              else
                _manager.Layout.RootPanel.Children.Insert( 0, floatingWindow.RootPanel );
            }
            else
            {
              var newOrientedPanel = new LayoutPanel()
              {
                Orientation = System.Windows.Controls.Orientation.Horizontal
              };

              var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
              if( layoutAnchorablePaneGroup != null &&
                  layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal )
              {
                var childrenToTransfer = layoutAnchorablePaneGroup.Children.ToArray();
                for( int i = 0; i < childrenToTransfer.Length; i++ )
                  newOrientedPanel.Children.Insert( i, childrenToTransfer[ i ] );
              }
              else
              {
                newOrientedPanel.Children.Add( floatingWindow.RootPanel );
              }
              newOrientedPanel.Children.Add( _manager.Layout.RootPanel );

              _manager.Layout.RootPanel = newOrientedPanel;
            }
          }
          break;
        #endregion
        case DropTargetType.DockingManagerDockRight:
          #region DropTargetType.DockingManagerDockRight
          {
            if( _manager.Layout.RootPanel.Orientation != System.Windows.Controls.Orientation.Horizontal &&
                _manager.Layout.RootPanel.Children.Count == 1 )
              _manager.Layout.RootPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

            if( _manager.Layout.RootPanel.Orientation == System.Windows.Controls.Orientation.Horizontal )
            {
              var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
              if( layoutAnchorablePaneGroup != null &&
                  layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal )
              {
                var childrenToTransfer = layoutAnchorablePaneGroup.Children.ToArray();
                for( int i = 0; i < childrenToTransfer.Length; i++ )
                  _manager.Layout.RootPanel.Children.Add( childrenToTransfer[ i ] );
              }
              else
                _manager.Layout.RootPanel.Children.Add( floatingWindow.RootPanel );
            }
            else
            {
              var newOrientedPanel = new LayoutPanel()
              {
                Orientation = System.Windows.Controls.Orientation.Horizontal
              };

              var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
              if( layoutAnchorablePaneGroup != null &&
                  layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal )
              {
                var childrenToTransfer = layoutAnchorablePaneGroup.Children.ToArray();
                for( int i = 0; i < childrenToTransfer.Length; i++ )
                  newOrientedPanel.Children.Add( childrenToTransfer[ i ] );
              }
              else
              {
                newOrientedPanel.Children.Add( floatingWindow.RootPanel );
              }

              newOrientedPanel.Children.Insert( 0, _manager.Layout.RootPanel );


              _manager.Layout.RootPanel = newOrientedPanel;
            }
          }
          break;
        #endregion
        case DropTargetType.DockingManagerDockTop:
          #region DropTargetType.DockingManagerDockTop
          {
            if( _manager.Layout.RootPanel.Orientation != System.Windows.Controls.Orientation.Vertical &&
                _manager.Layout.RootPanel.Children.Count == 1 )
              _manager.Layout.RootPanel.Orientation = System.Windows.Controls.Orientation.Vertical;

            if( _manager.Layout.RootPanel.Orientation == System.Windows.Controls.Orientation.Vertical )
            {
              var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
              if( layoutAnchorablePaneGroup != null &&
                  layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical )
              {
                var childrenToTransfer = layoutAnchorablePaneGroup.Children.ToArray();
                for( int i = 0; i < childrenToTransfer.Length; i++ )
                  _manager.Layout.RootPanel.Children.Insert( i, childrenToTransfer[ i ] );
              }
              else
                _manager.Layout.RootPanel.Children.Insert( 0, floatingWindow.RootPanel );
            }
            else
            {
              var newOrientedPanel = new LayoutPanel()
              {
                Orientation = System.Windows.Controls.Orientation.Vertical
              };

              var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
              if( layoutAnchorablePaneGroup != null &&
                  layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical )
              {
                var childrenToTransfer = layoutAnchorablePaneGroup.Children.ToArray();
                for( int i = 0; i < childrenToTransfer.Length; i++ )
                  newOrientedPanel.Children.Insert( i, childrenToTransfer[ i ] );
              }
              else
              {
                newOrientedPanel.Children.Add( floatingWindow.RootPanel );
              }
              newOrientedPanel.Children.Add( _manager.Layout.RootPanel );

              _manager.Layout.RootPanel = newOrientedPanel;
            }
          }
          break;
        #endregion
        case DropTargetType.DockingManagerDockBottom:
          #region DropTargetType.DockingManagerDockBottom
          {
            if( _manager.Layout.RootPanel.Orientation != System.Windows.Controls.Orientation.Vertical &&
                _manager.Layout.RootPanel.Children.Count == 1 )
              _manager.Layout.RootPanel.Orientation = System.Windows.Controls.Orientation.Vertical;

            if( _manager.Layout.RootPanel.Orientation == System.Windows.Controls.Orientation.Vertical )
            {
              var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
              if( layoutAnchorablePaneGroup != null &&
                  layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical )
              {
                var childrenToTransfer = layoutAnchorablePaneGroup.Children.ToArray();
                for( int i = 0; i < childrenToTransfer.Length; i++ )
                  _manager.Layout.RootPanel.Children.Add( childrenToTransfer[ i ] );

              }
              else
                _manager.Layout.RootPanel.Children.Add( floatingWindow.RootPanel );
            }
            else
            {
              var newOrientedPanel = new LayoutPanel()
              {
                Orientation = System.Windows.Controls.Orientation.Vertical
              };

              var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
              if( layoutAnchorablePaneGroup != null &&
                  layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical )
              {
                var childrenToTransfer = layoutAnchorablePaneGroup.Children.ToArray();
                for( int i = 0; i < childrenToTransfer.Length; i++ )
                  newOrientedPanel.Children.Add( childrenToTransfer[ i ] );
              }
              else
              {
                newOrientedPanel.Children.Add( floatingWindow.RootPanel );
              }
              newOrientedPanel.Children.Insert( 0, _manager.Layout.RootPanel );

              _manager.Layout.RootPanel = newOrientedPanel;
            }
          }
          break;
          #endregion
      }


      base.Drop( floatingWindow );
    }

    public override System.Windows.Media.Geometry GetPreviewPath( OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindowModel )
    {
      var anchorableFloatingWindowModel = floatingWindowModel as LayoutAnchorableFloatingWindow;
      var layoutAnchorablePane = anchorableFloatingWindowModel.RootPanel as ILayoutPositionableElement;
      var layoutAnchorablePaneWithActualSize = anchorableFloatingWindowModel.RootPanel as ILayoutPositionableElementWithActualSize;

      var targetScreenRect = TargetElement.GetScreenArea();

      switch( Type )
      {
        case DropTargetType.DockingManagerDockLeft:
          {
            var desideredWidth = layoutAnchorablePane.DockWidth.IsAbsolute ? layoutAnchorablePane.DockWidth.Value : layoutAnchorablePaneWithActualSize.ActualWidth;
            var previewBoxRect = new Rect(
                targetScreenRect.Left - overlayWindow.Left,
                targetScreenRect.Top - overlayWindow.Top,
                Math.Min( desideredWidth, targetScreenRect.Width / 2.0 ),
                targetScreenRect.Height );

            return new RectangleGeometry( previewBoxRect );
          }
        case DropTargetType.DockingManagerDockTop:
          {
            var desideredHeight = layoutAnchorablePane.DockHeight.IsAbsolute ? layoutAnchorablePane.DockHeight.Value : layoutAnchorablePaneWithActualSize.ActualHeight;
            var previewBoxRect = new Rect(
                targetScreenRect.Left - overlayWindow.Left,
                targetScreenRect.Top - overlayWindow.Top,
                targetScreenRect.Width,
                Math.Min( desideredHeight, targetScreenRect.Height / 2.0 ) );

            return new RectangleGeometry( previewBoxRect );
          }
        case DropTargetType.DockingManagerDockRight:
          {
            var desideredWidth = layoutAnchorablePane.DockWidth.IsAbsolute ? layoutAnchorablePane.DockWidth.Value : layoutAnchorablePaneWithActualSize.ActualWidth;
            var previewBoxRect = new Rect(
                targetScreenRect.Right - overlayWindow.Left - Math.Min( desideredWidth, targetScreenRect.Width / 2.0 ),
                targetScreenRect.Top - overlayWindow.Top,
                Math.Min( desideredWidth, targetScreenRect.Width / 2.0 ),
                targetScreenRect.Height );

            return new RectangleGeometry( previewBoxRect );
          }
        case DropTargetType.DockingManagerDockBottom:
          {
            var desideredHeight = layoutAnchorablePane.DockHeight.IsAbsolute ? layoutAnchorablePane.DockHeight.Value : layoutAnchorablePaneWithActualSize.ActualHeight;
            var previewBoxRect = new Rect(
                targetScreenRect.Left - overlayWindow.Left,
                targetScreenRect.Bottom - overlayWindow.Top - Math.Min( desideredHeight, targetScreenRect.Height / 2.0 ),
                targetScreenRect.Width,
                Math.Min( desideredHeight, targetScreenRect.Height / 2.0 ) );

            return new RectangleGeometry( previewBoxRect );
          }
      }


      throw new InvalidOperationException();
    }

    #endregion
  }
}
