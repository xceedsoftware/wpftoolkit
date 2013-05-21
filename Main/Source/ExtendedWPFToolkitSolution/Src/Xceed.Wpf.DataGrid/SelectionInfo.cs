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
using System.Collections;
using System.Collections.ObjectModel;

namespace Xceed.Wpf.DataGrid
{
  public class SelectionInfo
  {
    internal SelectionInfo( 
      DataGridContext dataGridContext, 
      SelectedItemsStorage removedItems, 
      SelectedItemsStorage addedItems,
      SelectedCellsStorage removedCells,
      SelectedCellsStorage addedCells )
    {
      this.DataGridContext = dataGridContext;
      this.AddedItems = new ReadOnlyCollection<object>( new SelectionItemCollection( addedItems ) );
      this.RemovedItems = new ReadOnlyCollection<object>( new SelectionItemCollection( removedItems ) );
      this.AddedItemRanges = new ReadOnlyCollection<SelectionRange>( new SelectionItemRangeCollection( addedItems ) );
      this.RemovedItemRanges = new ReadOnlyCollection<SelectionRange>( new SelectionItemRangeCollection( removedItems ) );
      this.AddedCellRanges = new ReadOnlyCollection<SelectionCellRange>( new SelectionCellRangeCollection( addedCells ) );
      this.RemovedCellRanges = new ReadOnlyCollection<SelectionCellRange>( new SelectionCellRangeCollection( removedCells ) );
    }

    public DataGridContext DataGridContext
    {
      get;
      private set;
    }

    public ReadOnlyCollection<object> AddedItems
    {
      get;
      private set;
    }

    public ReadOnlyCollection<object> RemovedItems
    {
      get;
      private set;
    }

    public ReadOnlyCollection<SelectionRange> AddedItemRanges
    {
      get;
      private set;
    }

    public ReadOnlyCollection<SelectionRange> RemovedItemRanges
    {
      get;
      private set;
    }

    public ReadOnlyCollection<SelectionCellRange> AddedCellRanges
    {
      get;
      private set;
    }

    public ReadOnlyCollection<SelectionCellRange> RemovedCellRanges
    {
      get;
      private set;
    }

    internal bool IsEmpty
    {
      get
      {
        return 
          this.RemovedCellRanges.Count == 0
          && this.AddedCellRanges.Count == 0
          && this.RemovedItemRanges.Count == 0
          && this.AddedItemRanges.Count == 0;
      }
    }
  }
}
