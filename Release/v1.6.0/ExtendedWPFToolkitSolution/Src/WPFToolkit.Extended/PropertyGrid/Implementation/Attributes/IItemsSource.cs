/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/
using System.Collections.Generic;

namespace Microsoft.Windows.Controls.PropertyGrid.Attributes
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
