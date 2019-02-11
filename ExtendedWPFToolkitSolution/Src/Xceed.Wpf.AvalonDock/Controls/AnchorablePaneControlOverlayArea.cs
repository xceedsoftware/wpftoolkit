/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

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
