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
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace Xceed.Wpf.DataGrid
{
  public static class CollectionViewGroupExtensions
  {

    internal static SelectionRange GetRange( this CollectionViewGroup collectionViewGroup, DataGridContext dataGridContext )
    {
      int startIndex = -1;
      int endIndex = -1;

      DataGridVirtualizingCollectionViewGroupBase dataGridVirtualizingCollectionViewGroupBase = collectionViewGroup as DataGridVirtualizingCollectionViewGroupBase;
      DataGridCollectionViewGroup dataGridCollectionViewGroup = collectionViewGroup as DataGridCollectionViewGroup;

      if( dataGridVirtualizingCollectionViewGroupBase != null )
      {
        startIndex = dataGridVirtualizingCollectionViewGroupBase.StartGlobalIndex;
        endIndex = startIndex + dataGridVirtualizingCollectionViewGroupBase.VirtualItemCount - 1;
      }
      else if( dataGridCollectionViewGroup != null )
      {
        startIndex = dataGridCollectionViewGroup.GetFirstRawItemGlobalSortedIndex();
        endIndex = startIndex + dataGridCollectionViewGroup.GlobalRawItemCount - 1;
      }
      else if( collectionViewGroup.ItemCount > 0 )
      {
        if( dataGridContext == null )
          throw new DataGridInternalException( "This collectionViewGroup require a DataGridContext instance" );

        var firstItem = collectionViewGroup.GetFirstLeafItem();
        var lastItem = collectionViewGroup.GetLastLeafItem();

        if( firstItem != null && lastItem != null )
        {
          startIndex = dataGridContext.Items.IndexOf( firstItem );
          endIndex = dataGridContext.Items.IndexOf( lastItem );
        }
      }

      return ( startIndex >= 0 ) && ( startIndex <= endIndex )
        ? new SelectionRange( startIndex, endIndex )
        : SelectionRange.Empty;
    }

    public static IList<object> GetItems( this CollectionViewGroup collectionViewGroup )
    {
      var dataGridVirtualizingCollectionViewGroupBase = collectionViewGroup as DataGridVirtualizingCollectionViewGroupBase;
      if( dataGridVirtualizingCollectionViewGroupBase != null )
        return dataGridVirtualizingCollectionViewGroupBase.VirtualItems;

      // The Items property of the DataGridCollectionViewGroup has been optimized to allow faster IndexOf and Contains. Since this property we could not
      // override the property, we had to new it thus, we need to cast the CollectionViewGroup in the right type before acecssing the property.
      var dataGridCollectionViewGroup = collectionViewGroup as DataGridCollectionViewGroup;
      if( dataGridCollectionViewGroup != null )
        return dataGridCollectionViewGroup.Items;

      return collectionViewGroup.Items;
    }

    public static int GetItemCount( this CollectionViewGroup collectionViewGroup )
    {
      var dataGridVirtualizingCollectionViewGroupBase = collectionViewGroup as DataGridVirtualizingCollectionViewGroupBase;
      if( dataGridVirtualizingCollectionViewGroupBase != null )
        return dataGridVirtualizingCollectionViewGroupBase.VirtualItemCount;

      return collectionViewGroup.ItemCount;
    }

    internal static IEnumerable<object> GetLeafItems( this CollectionViewGroup collectionViewGroup )
    {
      foreach( var item in collectionViewGroup.GetItems() )
      {
        if( item is CollectionViewGroup )
        {
          foreach( var subItem in ( ( CollectionViewGroup )item ).GetLeafItems() )
          {
            yield return subItem;
          }
        }
        else
        {
          yield return item;
        }
      }
    }

    internal static object GetFirstLeafItem( this CollectionViewGroup collectionViewGroup )
    {
      IList<object> items = collectionViewGroup.GetItems();

      if( items.Count == 0 )
        return null;

      var item = items[ 0 ];
      var subGroup = item as CollectionViewGroup;

      return ( subGroup != null )
        ? subGroup.GetFirstLeafItem()
        : item;
    }

    internal static object GetLastLeafItem( this CollectionViewGroup collectionViewGroup )
    {
      IList<object> items = collectionViewGroup.GetItems();

      if( items.Count == 0 )
        return null;

      var item = items[ items.Count - 1 ];
      var subGroup = item as CollectionViewGroup;

      return ( subGroup != null )
        ? subGroup.GetLastLeafItem()
        : item;
    }


    internal static IEnumerable<CollectionViewGroup> GetSubGroups( this CollectionViewGroup collectionViewGroup )
    {
      foreach( var item in collectionViewGroup.GetItems() )
      {
        var subGroup = item as CollectionViewGroup;

        //No need to check every items, they will be all Groups or not.
        if( subGroup == null )
          break;

        yield return subGroup;

        foreach( var subSubgroup in subGroup.GetSubGroups() )
        {
          yield return subGroup;
        }
      }
    }
  }
}
