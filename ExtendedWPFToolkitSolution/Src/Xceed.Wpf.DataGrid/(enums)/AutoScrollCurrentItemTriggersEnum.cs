/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;

namespace Xceed.Wpf.DataGrid
{
  // Defines the events that trigger the bring into view for the current item.
  [Flags]
  public enum AutoScrollCurrentItemTriggers
  {
    None = 0x00,

    CurrentChanged = 0x01,
    ItemsSourceChanged = 0x02,
    FocusChanged = 0x04,
    SortChanged = 0x08,
    GroupChanged = 0x10,
    ViewChanged = 0x20,
    ThemeChanged = 0x40,

    All = ~0x00
  }
}
