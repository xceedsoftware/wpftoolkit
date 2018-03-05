/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections.Generic;

namespace Xceed.Wpf.DataGrid
{
  public delegate void DataGridSelectionChangingEventHandler( object sender, DataGridSelectionChangingEventArgs e );

  public class DataGridSelectionChangingEventArgs : CancelRoutedEventArgs
  {
    internal DataGridSelectionChangingEventArgs( IList<SelectionInfo> selectionInfos, bool isCancelable )
      : base( DataGridControl.SelectionChangingEvent )
    {
      this.SelectionInfos = selectionInfos;
      this.IsCancelable = isCancelable;
    }

    public bool IsCancelable
    {
      get;
      private set;
    }

    public IList<SelectionInfo> SelectionInfos
    {
      get;
      private set;
    }
  }
}
