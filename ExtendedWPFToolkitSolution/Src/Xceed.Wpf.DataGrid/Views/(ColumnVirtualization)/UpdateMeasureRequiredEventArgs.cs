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

namespace Xceed.Wpf.DataGrid.Views
{
  internal class UpdateMeasureRequiredEventArgs : EventArgs
  {
    public UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction action )
    {
      this.TriggeredAction = action;
    }

    public UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction action, object parameters )
      : this( action )
    {
      this.Parameters = parameters;
    }

    public UpdateMeasureTriggeredAction TriggeredAction
    {
      get;
      private set;
    }

    public object Parameters
    {
      get;
      private set;
    }
  }

  internal enum UpdateMeasureTriggeredAction
  {
    Unspecified,
    ColumnActualWidthChanged,
    ColumnReordering, // FixedColumns, Columns drag and drop
    CurrentItemChanged,
    GroupingChanged,
    ScrollViewerChanged,
    SortingChanged,
    VirtualizationStateChanged, // On / Off Virtualization
    ViewPortWidthChanged, // Parent viewport resized
  }
}
