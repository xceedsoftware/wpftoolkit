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
  public class DocumentPaneControlOverlayArea : OverlayArea
  {
    #region Members

    private LayoutDocumentPaneControl _documentPaneControl;

    #endregion

    #region Constructors

    internal DocumentPaneControlOverlayArea(
        IOverlayWindow overlayWindow,
        LayoutDocumentPaneControl documentPaneControl )
        : base( overlayWindow )
    {
      _documentPaneControl = documentPaneControl;
      base.SetScreenDetectionArea( new Rect(  _documentPaneControl.PointToScreenDPI( new Point() ),  _documentPaneControl.TransformActualSizeToAncestor() ) );
    }

    #endregion
  }
}
