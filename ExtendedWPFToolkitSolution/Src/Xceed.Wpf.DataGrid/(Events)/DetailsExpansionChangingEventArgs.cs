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
  public delegate void DetailsExpansionChangingEventHandler( object sender, DetailsExpansionChangingEventArgs e );

  public class DetailsExpansionChangingEventArgs : RoutedEventArgs
  {
    public DetailsExpansionChangingEventArgs( RoutedEvent routedEvent, object dataItem, DataGridContext dataGridContext, bool isExpanding )
      : base( routedEvent )
    {
      this.DataGridContext = dataGridContext;
      this.IsExpanding = isExpanding;
      this.MasterItem = dataItem;
    }

    #region Cancel Property

    public bool Cancel
    {
      get;
      set;
    }

    #endregion

    #region DataGridContext Property

    public DataGridContext DataGridContext
    {
      get;
      private set;
    }

    #endregion

    #region IsExpanding Property 

    public bool IsExpanding
    {
      get;
      private set;
    }

    #endregion

    #region MasterItem Property

    public object MasterItem
    {
      get;
      private set;
    }

    #endregion
  }
}
