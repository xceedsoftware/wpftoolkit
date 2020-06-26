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
  public class AnchorablePaneControlOverlayArea : OverlayArea
  {
    #region Members

    private LayoutAnchorablePaneControl _anchorablePaneControl;

    #endregion

    #region constructors

    internal AnchorablePaneControlOverlayArea(
        IOverlayWindow overlayWindow,
        LayoutAnchorablePaneControl anchorablePaneControl )
        : base( overlayWindow )
    {

      _anchorablePaneControl = anchorablePaneControl;
      base.SetScreenDetectionArea( new Rect(
          _anchorablePaneControl.PointToScreenDPI( new Point() ),
          _anchorablePaneControl.TransformActualSizeToAncestor() ) );

    }

    #endregion
  }
}
