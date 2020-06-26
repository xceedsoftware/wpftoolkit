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

using System.Windows;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class OverlayWindowDropTarget : IOverlayWindowDropTarget
  {
    #region Members

    private IOverlayWindowArea _overlayArea;
    private Rect _screenDetectionArea;
    private OverlayWindowDropTargetType _type;

    #endregion

    #region Constructors

    internal OverlayWindowDropTarget( IOverlayWindowArea overlayArea, OverlayWindowDropTargetType targetType, FrameworkElement element )
    {
      _overlayArea = overlayArea;
      _type = targetType;
      _screenDetectionArea = new Rect( element.TransformToDeviceDPI( new Point() ), element.TransformActualSizeToAncestor() );
    }

    #endregion


    #region IOverlayWindowDropTarget

    Rect IOverlayWindowDropTarget.ScreenDetectionArea
    {
      get
      {
        return _screenDetectionArea;
      }

    }

    OverlayWindowDropTargetType IOverlayWindowDropTarget.Type
    {
      get
      {
        return _type;
      }
    }

    #endregion
  }
}
