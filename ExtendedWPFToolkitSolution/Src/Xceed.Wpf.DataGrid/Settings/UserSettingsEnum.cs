/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

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
