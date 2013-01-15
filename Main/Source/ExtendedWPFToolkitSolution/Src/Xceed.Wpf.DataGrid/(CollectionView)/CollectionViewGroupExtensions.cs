/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace Xceed.Wpf.DataGrid
{
  public static class CollectionViewGroupExtensions
  {
    public static IList<object> GetItems( this CollectionViewGroup collectionViewGroup )
    {
      DataGridVirtualizingCollectionViewGroupBase dataGridVirtualizingCollectionViewGroupBase = collectionViewGroup as DataGridVirtualizingCollectionViewGroupBase;
      if( dataGridVirtualizingCollectionViewGroupBase != null )
      {
        return dataGridVirtualizingCollectionViewGroupBase.VirtualItems;
      }

      DataGridCollectionViewGroup dataGridCollectionViewGroup = collectionViewGroup as DataGridCollectionViewGroup;
      if( dataGridCollectionViewGroup != null )
      {
        // The Items property of the DataGridCollectionViewGroup has been optimized
        // to allow faster IndexOf and Contains. Since this property we could not
        // override the property, we had to new it thus, we need to cast the CollectionViewGroup
        // in the right type before acecssing the property.
        return dataGridCollectionViewGroup.Items;
      }

      return collectionViewGroup.Items;
    }

    public static int GetItemCount( this CollectionViewGroup collectionViewGroup )
    {
      if( collectionViewGroup is DataGridVirtualizingCollectionViewGroupBase )
        return ( ( DataGridVirtualizingCollectionViewGroupBase )collectionViewGroup ).VirtualItemCount;

      return collectionViewGroup.ItemCount;
    }
  }
}
