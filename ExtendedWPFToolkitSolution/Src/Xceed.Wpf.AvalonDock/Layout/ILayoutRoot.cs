/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections.ObjectModel;

namespace Xceed.Wpf.AvalonDock.Layout
{
  public interface ILayoutRoot
  {
    DockingManager Manager
    {
      get;
    }

    LayoutPanel RootPanel
    {
      get;
    }

    LayoutAnchorSide TopSide
    {
      get;
    }
    LayoutAnchorSide LeftSide
    {
      get;
    }
    LayoutAnchorSide RightSide
    {
      get;
    }
    LayoutAnchorSide BottomSide
    {
      get;
    }

    LayoutContent ActiveContent
    {
      get; set;
    }

    ObservableCollection<LayoutFloatingWindow> FloatingWindows
    {
      get;
    }
    ObservableCollection<LayoutAnchorable> Hidden
    {
      get;
    }

    void CollectGarbage();
  }
}
