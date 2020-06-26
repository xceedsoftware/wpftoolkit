/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections.Generic;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Attributes
{
  public interface IItemsSource
  {
    ItemCollection GetValues();
  }

  public class Item
  {
    public string DisplayName
    {
      get;
      set;
    }
    public object Value
    {
      get;
      set;
    }
  }

  public class ItemCollection : List<Item>
  {
    public void Add( object value )
    {
      Item item = new Item();
      item.DisplayName = value.ToString();
      item.Value = value;
      base.Add( item );
    }

    public void Add( object value, string displayName )
    {
      Item newItem = new Item();
      newItem.DisplayName = displayName;
      newItem.Value = value;
      base.Add( newItem );
    }
  }
}
