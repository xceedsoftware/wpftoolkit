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
  internal class DocumentPaneDropAsAnchorableTarget : DropTarget<LayoutDocumentPaneControl>
  {
    #region Members

    private LayoutDocumentPaneControl _targetPane;
    int _tabIndex = -1;

    #endregion

    #region Constructors

    internal DocumentPaneDropAsAnchorableTarget( LayoutDocumentPaneControl paneControl, Rect detectionRect, DropTargetType type )
        : base( paneControl, detectionRect, type )
    {
      _targetPane = paneControl;
    }

    internal DocumentPaneDropAsAnchorableTarget( LayoutDocumentPaneControl paneControl, Rect detectionRect, DropTargetType type, int tabIndex )
        : base( paneControl, detectionRect, type )
    {
      _targetPane = paneControl;
      _tabIndex = tabIndex;
    }

    #endregion

    #region Overrides

    protected override void Drop( LayoutAnchorableFloatingWindow floatingWindow )
    {
      ILayoutDocumentPane targetModel = _targetPane.Model as ILayoutDocumentPane;
      LayoutDocumentPaneGroup parentGroup;
      LayoutPanel parentGroupPanel;
      FindParentLayoutDocumentPane( targetModel, out parentGroup, out parentGroupPanel );

      switch( Type )
      {
        case DropTargetType.DocumentPaneDockAsAnchorableBottom:
          #region DropTargetType.DocumentPaneDockAsAnchorableBottom
          {
            if( parentGroupPanel != null &&
                parentGroupPanel.ChildrenCount == 1 )
              parentGroupPanel.Orientation = System.Windows.Controls.Orientation.Vertical;

            if( parentGroupPanel != null &&
                parentGroupPanel.Orientation == System.Windows.Controls.Orientation.Vertical )
            {
              parentGroupPanel.Children.Insert(
                  parentGroupPanel.IndexOfChild( parentGroup != null ? parentGroup : targetModel ) + 1,
                  floatingWindow.RootPanel );
            }
            else if( parentGroupPanel != null )
            {
              var newParentPanel = new LayoutPanel() { Orientation = System.Windows.Controls.Orientation.Vertical };
              parentGroupPanel.ReplaceChild( parentGroup != null ? parentGroup : targetModel, newParentPanel );
              newParentPanel.Children.Add( parentGroup != null ? parentGroup : targetModel );
              newParentPanel.Children.Add( floatingWindow.RootPanel );
            }
            else
            {
              throw new NotImplementedException();
            }


          }
          break;
        #endregion
        case DropTargetType.DocumentPaneDockAsAnchorableTop:
          #region DropTargetType.DocumentPaneDockAsAnchorableTop
          {
            if( parentGroupPanel != null &&
                parentGroupPanel.ChildrenCount == 1 )
              parentGroupPanel.Orientation = System.Windows.Controls.Orientation.Vertical;

            if( parentGroupPanel != null &&
                parentGroupPanel.Orientation == System.Windows.Controls.Orientation.Vertical )
            {
              parentGroupPanel.Children.Insert(
                  parentGroupPanel.IndexOfChild( parentGroup != null ? parentGroup : targetModel ),
                  floatingWindow.RootPanel );
            }
            else if( parentGroupPanel != null )
            {
              var newParentPanel = new LayoutPanel() { Orientation = System.Windows.Controls.Orientation.Vertical };
              parentGroupPanel.ReplaceChild( parentGroup != null ? parentGroup : targetModel, newParentPanel );
              newParentPanel.Children.Add( parentGroup != null ? parentGroup : targetModel );
              newParentPanel.Children.Insert( 0, floatingWindow.RootPanel );
            }
            else
            {
              throw new NotImplementedException();
            }

          }
          break;
        #endregion
        case DropTargetType.DocumentPaneDockAsAnchorableLeft:
          #region DropTargetType.DocumentPaneDockAsAnchorableLeft
          {
            if( parentGroupPanel != null &&
                parentGroupPanel.ChildrenCount == 1 )
              parentGroupPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

            if( parentGroupPanel != null &&
                parentGroupPanel.Orientation == System.Windows.Controls.Orientation.Horizontal )
            {
              parentGroupPanel.Children.Insert(
                  parentGroupPanel.IndexOfChild( parentGroup != null ? parentGroup : targetModel ),
                  floatingWindow.RootPanel );
            }
            else if( parentGroupPanel != null )
            {
              var newParentPanel = new LayoutPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal };
              parentGroupPanel.ReplaceChild( parentGroup != null ? parentGroup : targetModel, newParentPanel );
              newParentPanel.Children.Add( parentGroup != null ? parentGroup : targetModel );
              newParentPanel.Children.Insert( 0, floatingWindow.RootPanel );
            }
            else
            {
              throw new NotImplementedException();
            }
          }
          break;
        #endregion
        case DropTargetType.DocumentPaneDockAsAnchorableRight:
          #region DropTargetType.DocumentPaneDockAsAnchorableRight
          {
            if( parentGroupPanel != null &&
                parentGroupPanel.ChildrenCount == 1 )
              parentGroupPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

            if( parentGroupPanel != null &&
                parentGroupPanel.Orientation == System.Windows.Controls.Orientation.Horizontal )
            {
              parentGroupPanel.Children.Insert(
                  parentGroupPanel.IndexOfChild( parentGroup != null ? parentGroup : targetModel ) + 1,
                  floatingWindow.RootPanel );
            }
            else if( parentGroupPanel != null )
            {
              var newParentPanel = new LayoutPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal };
              parentGroupPanel.ReplaceChild( parentGroup != null ? parentGroup : targetModel, newParentPanel );
              newParentPanel.Children.Add( parentGroup != null ? parentGroup : targetModel );
              newParentPanel.Children.Add( floatingWindow.RootPanel );
            }
            else
            {
              throw new NotImplementedException();
            }
          }
          break;
          #endregion
      }

      base.Drop( floatingWindow );
    }

    public override System.Windows.Media.Geometry GetPreviewPath( OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindowModel )
    {
      Rect targetScreenRect;
      ILayoutDocumentPane targetModel = _targetPane.Model as ILayoutDocumentPane;
      var manager = targetModel.Root.Manager;

      //ILayoutDocumentPane targetModel = _targetPane.Model as ILayoutDocumentPane;
      LayoutDocumentPaneGroup parentGroup;
      LayoutPanel parentGroupPanel;
      if( !FindParentLayoutDocumentPane( targetModel, out parentGroup, out parentGroupPanel ) )
        return null;

      //if (targetModel.Parent is LayoutDocumentPaneGroup)
      //{
      //    var parentGroup = targetModel.Parent as LayoutDocumentPaneGroup;
      //    var documentPaneGroupControl = manager.FindLogicalChildren<LayoutDocumentPaneGroupControl>().First(d => d.Model == parentGroup);
      //    targetScreenRect = documentPaneGroupControl.GetScreenArea();
      //}
      //else
      //{
      //    var documentPaneControl = manager.FindLogicalChildren<LayoutDocumentPaneControl>().First(d => d.Model == targetModel);
      //    targetScreenRect = documentPaneControl.GetScreenArea();
      //}

      //var parentPanel = targetModel.FindParent<LayoutPanel>();
      var documentPaneControl = manager.FindLogicalChildren<FrameworkElement>().OfType<ILayoutControl>().First( d => parentGroup != null ? d.Model == parentGroup : d.Model == parentGroupPanel ) as FrameworkElement;
      targetScreenRect = documentPaneControl.GetScreenArea();

      switch( Type )
      {
        case DropTargetType.DocumentPaneDockAsAnchorableBottom:
          {
            targetScreenRect.Offset( -overlayWindow.Left, -overlayWindow.Top );
            targetScreenRect.Offset( 0.0, targetScreenRect.Height - targetScreenRect.Height / 3.0 );
            targetScreenRect.Height /= 3.0;
            return new RectangleGeometry( targetScreenRect );
          }
        case DropTargetType.DocumentPaneDockAsAnchorableTop:
          {
            targetScreenRect.Offset( -overlayWindow.Left, -overlayWindow.Top );
            targetScreenRect.Height /= 3.0;
            return new RectangleGeometry( targetScreenRect );
          }
        case DropTargetType.DocumentPaneDockAsAnchorableRight:
          {
            targetScreenRect.Offset( -overlayWindow.Left, -overlayWindow.Top );
            targetScreenRect.Offset( targetScreenRect.Width - targetScreenRect.Width / 3.0, 0.0 );
            targetScreenRect.Width /= 3.0;
            return new RectangleGeometry( targetScreenRect );
          }
        case DropTargetType.DocumentPaneDockAsAnchorableLeft:
          {
            targetScreenRect.Offset( -overlayWindow.Left, -overlayWindow.Top );
            targetScreenRect.Width /= 3.0;
            return new RectangleGeometry( targetScreenRect );
          }
      }

      return null;
    }

    #endregion

    #region Private Methods

    private bool FindParentLayoutDocumentPane( ILayoutDocumentPane documentPane, out LayoutDocumentPaneGroup containerPaneGroup, out LayoutPanel containerPanel )
    {
      containerPaneGroup = null;
      containerPanel = null;

      if( documentPane.Parent is LayoutPanel )
      {
        containerPaneGroup = null;
        containerPanel = documentPane.Parent as LayoutPanel;
        return true;
      }
      else if( documentPane.Parent is LayoutDocumentPaneGroup )
      {
        var currentDocumentPaneGroup = documentPane.Parent as LayoutDocumentPaneGroup;
        while( !( currentDocumentPaneGroup.Parent is LayoutPanel ) )
        {
          currentDocumentPaneGroup = currentDocumentPaneGroup.Parent as LayoutDocumentPaneGroup;

          if( currentDocumentPaneGroup == null )
            break;
        }

        if( currentDocumentPaneGroup == null )
          return false;

        containerPaneGroup = currentDocumentPaneGroup;
        containerPanel = currentDocumentPaneGroup.Parent as LayoutPanel;
        return true;
      }

      return false;



    }

    #endregion
  }
}
