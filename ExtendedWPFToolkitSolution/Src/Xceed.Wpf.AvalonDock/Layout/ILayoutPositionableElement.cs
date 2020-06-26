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

namespace Xceed.Wpf.AvalonDock.Layout
{
  internal interface ILayoutPositionableElement : ILayoutElement, ILayoutElementForFloatingWindow
  {
    GridLength DockWidth
    {
      get;
      set;
    }

    GridLength DockHeight
    {
      get;
      set;
    }

    double DockMinWidth
    {
      get; set;
    }
    double DockMinHeight
    {
      get; set;
    }

    bool AllowDuplicateContent
    {
      get; set;
    }

    bool IsVisible
    {
      get;
    }
  }


  internal interface ILayoutPositionableElementWithActualSize
  {
    double ActualWidth
    {
      get; set;
    }
    double ActualHeight
    {
      get; set;
    }
  }

  internal interface ILayoutElementForFloatingWindow
  {
    double FloatingWidth
    {
      get; set;
    }
    double FloatingHeight
    {
      get; set;
    }
    double FloatingLeft
    {
      get; set;
    }
    double FloatingTop
    {
      get; set;
    }
    bool IsMaximized
    {
      get; set;
    }
  }
}
