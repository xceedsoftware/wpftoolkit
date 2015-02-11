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
  [Flags]
  internal enum AutoScrollCurrentItemSourceTriggers
  {
    None = 0x0000,
    Navigation = 0x0001,
    Editing = 0x0002,
    FocusChanged = 0x0004,
    SelectionChanged = 0x0008,
    SortChanged = 0x0010,
    GroupChanged = 0x0020,
    CurrentItemChanged = 0x0040,
    CurrentColumnChanged = 0x0080,
    CollectionViewCurrentItemChanged = 0x0100,
    ColumnsCollectionChanged = 0x0200,
    ItemsSourceChanged = 0x0400,
    ViewChanged = 0x0800,
    ThemeChanged = 0x1000,
  }
}
