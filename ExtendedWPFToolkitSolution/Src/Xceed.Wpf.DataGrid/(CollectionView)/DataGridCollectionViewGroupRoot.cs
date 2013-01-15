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
using System.Text;
using System.Windows.Data;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridCollectionViewGroupRoot : DataGridCollectionViewGroup
  {
    #region CONSTRUCTORS

    public DataGridCollectionViewGroupRoot( DataGridCollectionView parentCollectionView )
      : base( null, null, 0 )
    {
      if( parentCollectionView == null )
        throw new ArgumentNullException( "parentCollectionView" );

      m_parentCollectionView = parentCollectionView;
    }

    internal DataGridCollectionViewGroupRoot( DataGridCollectionViewGroupRoot template )
      : base( template, null )
    {
      m_parentCollectionView = template.m_parentCollectionView;
    }

    #endregion CONSTRUCTORS

    #region PROTECTED METHODS

    protected override DataGridCollectionView GetCollectionView()
    {
      return m_parentCollectionView;
    }

    #endregion PROTECTED METHODS

    #region INTERNAL METHODS

    internal void SortRootRawItems( SortDescriptionInfo[] sortDescriptionInfos, List<RawItem> globalRawItems )
    {
      Debug.Assert( this.IsBottomLevel );
      List<RawItem> rawItems = this.RawItems;

      if( rawItems == null )
        return;

      int itemCount = rawItems.Count;

      if( itemCount == 0 )
        return;

      int[] indexes;

      indexes = new int[ itemCount + 1 ];

      for( int i = 0; i < itemCount; i++ )
      {
        indexes[ i ] = rawItems[ i ].Index;
      }

      // "Weak heap sort" sort array[0..NUM_ELEMENTS-1] to array[1..NUM_ELEMENTS]
      DataGridCollectionViewSort collectionViewSort =
        new DataGridCollectionViewSort( indexes, sortDescriptionInfos );

      collectionViewSort.Sort( itemCount );
      int index = 0;

      for( int i = 1; i <= itemCount; i++ )
      {
        RawItem newRawItem = globalRawItems[ indexes[ i ] ];
        newRawItem.SetSortedIndex( index );
        rawItems[ index ] = newRawItem;
        index++;
      }
    }

    #endregion INTERNAL METHODS

    #region RawItem management for BottomLevel

    internal override void InsertRawItem( int index, RawItem rawItem )
    {
      Debug.Assert( this.IsBottomLevel );

      if( m_sortedRawItems == null )
        m_sortedRawItems = new List<RawItem>( 128 );

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

      if( index != -1 )
      {
        m_globalRawItemCount--;
        int count = m_sortedRawItems.Count;

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

    #endregion RawItem management for BottomLevel

    #region PRIVATE FILEDS

    private DataGridCollectionView m_parentCollectionView;

    #endregion PRIVATE FIELDS
  }
}
