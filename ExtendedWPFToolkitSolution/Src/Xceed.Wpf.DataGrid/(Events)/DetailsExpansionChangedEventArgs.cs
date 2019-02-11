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
  public delegate void DetailsExpansionChangedEventHandler( object sender, DetailsExpansionChangedEventArgs e );

  public class DetailsExpansionChangedEventArgs : RoutedEventArgs
  {
    public DetailsExpansionChangedEventArgs( RoutedEvent routedEvent, object dataItem, DataGridContext dataGridContext, bool isExpanded )
      : base( routedEvent )
    {
      this.DataGridContext = dataGridContext;
      this.IsExpanded = isExpanded;
      this.MasterItem = dataItem;
    }

    #region DataGridContext Property

    public DataGridContext DataGridContext
    {
      get;
      private set;
    }

    #endregion

    #region IsExpanded Property 

    public bool IsExpanded
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
