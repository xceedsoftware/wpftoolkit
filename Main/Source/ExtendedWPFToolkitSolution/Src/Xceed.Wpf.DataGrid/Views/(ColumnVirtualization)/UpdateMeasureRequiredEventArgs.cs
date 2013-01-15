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
