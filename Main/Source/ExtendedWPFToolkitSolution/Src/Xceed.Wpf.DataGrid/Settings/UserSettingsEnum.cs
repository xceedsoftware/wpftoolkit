/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xceed.Wpf.DataGrid.Settings
{
  [Flags]
  public enum UserSettings
  {
    None                    = 0x0000000,

    ColumnWidths            = 0x0000001,
    ColumnPositions         = 0x0000010,
    ColumnVisibilities      = 0x0000100, 

    FixedColumnCounts       = 0x0001000,
    Sorting                 = 0x0010000,
    Grouping                = 0x0100000,

    CardWidths              = 0x1000000,

    All                     = 0x1111111,
  }
}
