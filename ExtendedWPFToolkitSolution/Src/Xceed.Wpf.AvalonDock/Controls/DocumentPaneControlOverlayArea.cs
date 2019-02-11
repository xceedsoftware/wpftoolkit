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
