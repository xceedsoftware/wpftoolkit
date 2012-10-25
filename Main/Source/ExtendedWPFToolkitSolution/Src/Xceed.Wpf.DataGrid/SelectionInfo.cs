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
