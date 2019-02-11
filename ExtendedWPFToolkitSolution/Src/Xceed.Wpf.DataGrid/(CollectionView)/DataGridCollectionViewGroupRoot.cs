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
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridCollectionViewGroupRoot : DataGridCollectionViewGroup
  {
    internal DataGridCollectionViewGroupRoot( DataGridCollectionView parentCollectionView )
      : base( 128 )
    {
      m_parentCollectionView = parentCollectionView;
    }

    internal DataGridCollectionViewGroupRoot( DataGridCollectionViewGroupRoot template )
      : base( template, null )
    {
      m_parentCollectionView = template.m_parentCollectionView;
    }

    protected override DataGridCollectionView GetCollectionView()
    {
      return m_parentCollectionView;
    }

    internal void SortRootRawItems( IList<SortDescriptionInfo> sortDescriptionInfos, List<RawItem> globalRawItems )
    {
      Debug.Assert( this.IsBottomLevel );

      var itemCount = m_sortedRawItems.Count;
      if( itemCount == 0 )
        return;

      var indexes = new int[ itemCount + 1 ];

      for( int i = 0; i < itemCount; i++ )
      {
        indexes[ i ] = m_sortedRawItems[ i ].Index;
      }

      // "Weak heap sort" sort array[0..NUM_ELEMENTS-1] to array[1..NUM_ELEMENTS]
      var collectionViewSort = new DataGridCollectionViewSort( indexes, sortDescriptionInfos );

      collectionViewSort.Sort( itemCount );
      var index = 0;

      for( int i = 1; i <= itemCount; i++ )
      {
        var newRawItem = globalRawItems[ indexes[ i ] ];
        newRawItem.SetSortedIndex( index );
        m_sortedRawItems[ index ] = newRawItem;
        index++;
      }
    }

    internal override void InsertRawItem( int index, RawItem rawItem )
    {
      Debug.Assert( this.IsBottomLevel );

      m_globalRawItemCount++;
      int count = m_sortedRawItems.Count;

      for( int i = index; i < count; i++ )
      {
        m_sortedRawItems[ i ].SetSortedIndex( i + 1 );
      }

      m_sortedRawItems.Insert( index, rawItem );
      rawItem.SetParentGroup( this );
      rawItem.SetSortedIndex( index );
    }

    internal override void RemoveRawItemAt( int index )
    {
      Debug.Assert( this.IsBottomLevel );
      Debug.Assert( m_sortedRawItems.Count > 0 );

      int count = m_sortedRawItems.Count;
      if( count == 0 )
        return;

      if( index != -1 )
      {
        m_globalRawItemCount--;

        for( int i = index + 1; i < count; i++ )
        {
          m_sortedRawItems[ i ].SetSortedIndex( i - 1 );
        }

        RawItem rawItem = m_sortedRawItems[ index ];
        rawItem.SetParentGroup( null );
        rawItem.SetSortedIndex( -1 );
        m_sortedRawItems.RemoveAt( index );
      }
    }

    internal override void MoveRawItem( int oldIndex, int newIndex )
    {
      Debug.Assert( this.IsBottomLevel );

      RawItem rawItem = m_sortedRawItems[ oldIndex ];

      m_sortedRawItems.RemoveAt( oldIndex );
      m_sortedRawItems.Insert( newIndex, rawItem );

      int startIndex = Math.Min( oldIndex, newIndex );
      int endIndex = Math.Max( oldIndex, newIndex );

      for( int i = startIndex; i <= endIndex; i++ )
      {
        m_sortedRawItems[ i ].SetSortedIndex( i );
      }
    }

    private DataGridCollectionView m_parentCollectionView;
  }
}
