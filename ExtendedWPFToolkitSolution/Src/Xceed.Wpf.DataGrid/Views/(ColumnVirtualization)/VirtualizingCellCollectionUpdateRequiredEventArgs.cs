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
using System.Collections.Generic;

namespace Xceed.Wpf.DataGrid.Views
{
  internal sealed class VirtualizingCellCollectionUpdateRequiredEventArgs : EventArgs
  {
    internal VirtualizingCellCollectionUpdateRequiredEventArgs( VirtualizingCellCollectionUpdateTriggeredAction action )
    {
      if( action == VirtualizingCellCollectionUpdateTriggeredAction.VisibleColumnsChanged )
        throw new ArgumentException( "The action must be anything but VirtualizingCellCollectionUpdateTriggeredAction.VisibleColumnsChanged.", "action" );

      this.TriggeredAction = action;
    }

    internal VirtualizingCellCollectionUpdateRequiredEventArgs( IList<ColumnBase> columns )
    {
      if( columns == null )
        throw new ArgumentNullException( "columns" );

      if( columns.Count <= 0 )
        throw new ArgumentException( "The column count must be greater than zero.", "columns" );

      this.TriggeredAction = VirtualizingCellCollectionUpdateTriggeredAction.VisibleColumnsChanged;
      this.Columns = columns;
    }

    internal VirtualizingCellCollectionUpdateTriggeredAction TriggeredAction
    {
      get;
      private set;
    }

    internal IList<ColumnBase> Columns
    {
      get;
      private set;
    }
  }

  internal enum VirtualizingCellCollectionUpdateTriggeredAction
  {
    VirtualizationModeChanged, //Virtualizing and recycling cells.
    VisibleColumnsAdded,
    VisibleColumnsChanged
  }
}
