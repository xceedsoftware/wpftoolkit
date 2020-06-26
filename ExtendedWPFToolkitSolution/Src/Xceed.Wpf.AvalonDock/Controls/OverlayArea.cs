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
  public abstract class OverlayArea : IOverlayWindowArea
  {
    #region Members

    private IOverlayWindow _overlayWindow;
    private Rect? _screenDetectionArea;

    #endregion

    #region Constructors

    internal OverlayArea( IOverlayWindow overlayWindow )
    {
      _overlayWindow = overlayWindow;
    }

    #endregion

    #region Internal Methods

    protected void SetScreenDetectionArea( Rect rect )
    {
      _screenDetectionArea = rect;
    }

    #endregion

    #region IOverlayWindowArea

    Rect IOverlayWindowArea.ScreenDetectionArea
    {
      get
      {
        return _screenDetectionArea.Value;
      }
    }

    #endregion
  }
}
