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
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  public delegate void GroupExpansionChangingEventHandler( object sender, GroupExpansionChangingEventArgs e );

  public class GroupExpansionChangingEventArgs : RoutedEventArgs
  {
    public GroupExpansionChangingEventArgs( RoutedEvent routedEvent, Group group, CollectionViewGroup collectionViewGroup, DataGridContext dataGridContext, bool isExpanding )
      : base( routedEvent )
    {
      this.CollectionViewGroup = collectionViewGroup;
      this.DataGridContext = dataGridContext;
      this.Group = group;
      this.IsExpanding = isExpanding;
    }

    #region Cancel Property

    public bool Cancel
    {
      get;
      set;
    }

    #endregion

    #region CollectionViewGroup Property

    public CollectionViewGroup CollectionViewGroup
    {
      get;
      private set;
    }

    #endregion

    #region DataGridContext Property

    public DataGridContext DataGridContext
    {
      get;
      private set;
    }

    #endregion

    #region Group Property

    public Group Group
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
  }
}
