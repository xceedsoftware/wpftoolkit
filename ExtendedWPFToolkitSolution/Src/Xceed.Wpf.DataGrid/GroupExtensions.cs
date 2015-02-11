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
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  public static class GroupExtensions
  {
    public static IList<object> GetItems( this Group group )
    {
      CollectionViewGroup collectionViewGroup = group.CollectionViewGroup;

      if( collectionViewGroup == null )
        return null;

      return collectionViewGroup.GetItems();
    }

    public static IEnumerable<object> GetLeafItems( this Group group )
    {
      CollectionViewGroup collectionViewGroup = group.CollectionViewGroup;

      if( collectionViewGroup == null )
        return new object[0];

      return collectionViewGroup.GetLeafItems();
    }

    public static SelectionRange GetRange( this Group group )
    {
      CollectionViewGroup collectionViewGroup = group.CollectionViewGroup;

      if( collectionViewGroup == null )
        return SelectionRange.Empty;

      return collectionViewGroup.GetRange( group.DataGridContext );
    }
  }
}
