/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  public delegate void DataGridCurrentChangedEventHandler( object sender, DataGridCurrentChangedEventArgs e );

  public class DataGridCurrentChangedEventArgs : RoutedEventArgs
  {
    internal DataGridCurrentChangedEventArgs( DataGridContext oldDataGridContext, object oldCurrent, DataGridContext newDataGridContext, object newCurrent )
      : base( DataGridControl.CurrentChangedEvent )
    {
      this.OldDataGridContext = oldDataGridContext;
      this.OldCurrent = oldCurrent;
      this.NewDataGridContext = newDataGridContext;
      this.NewCurrent = newCurrent;
    }

    public DataGridContext OldDataGridContext
    {
      get;
      private set;
    }

    public object OldCurrent
    {
      get;
      private set;
    }

    public DataGridContext NewDataGridContext
    {
      get;
      private set;
    }

    public object NewCurrent
    {
      get;
      private set;
    }
  }
}
