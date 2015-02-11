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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridCollectionViewEnumerator : IEnumerator
  {
    public DataGridCollectionViewEnumerator( DataGridCollectionView collectionView )
    {
      m_collectionView = collectionView;
      m_version = m_collectionView.SortedItemVersion;

      this.Reset();
    }

    #region IEnumerator Members

    public object Current
    {
      get
      {
        if( m_beforeStart == true )
          throw new InvalidOperationException( "MoveNext must be called first." );

        if( m_current == null )
          throw new InvalidOperationException( "The index is past the end of the list." );

        return m_current.DataItem;
      }
    }

    public bool MoveNext()
    {
      bool retval = false;

      if( m_version != m_collectionView.SortedItemVersion )
        throw new InvalidOperationException( "The list of items has changed." );

      if( m_beforeStart == true )
      {
        m_beforeStart = false;

        if( ( m_currentGroup != null ) && ( m_currentGroup.RawItems.Count == 0 ) )
        {
          // This should only occur if the first leaf group encountered after the Reset call was empty.
          this.MoveToNextNonEmptyLeafGroup();
        }
      }

      if( m_afterEnd == true )
      {
        m_current = null;
      }
      else
      {
        if( ( m_currentGroup == null ) || ( m_currentGroup.RawItems.Count == 0 ) )
        {
          m_afterEnd = true;
        }
        else
        {
          //check indexes
          if( m_currentItemIndex < m_currentGroup.RawItems.Count )
          {
            m_current = m_currentGroup.RawItems[ m_currentItemIndex ];
            m_currentItemIndex++;

            if( m_currentItemIndex >= m_currentGroup.RawItems.Count )
            {
              m_currentItemIndex = 0;
              m_afterEnd = !this.MoveToNextNonEmptyLeafGroup();
            }

            retval = true;
          }
        }
      }

      return retval;
    }

    private bool MoveToNextNonEmptyLeafGroup()
    {
      bool foundNonEmptyLeafGroup = false;

      while( foundNonEmptyLeafGroup == false )
      {
        if( m_currentGroupIndex.Count == 0 )
          break;

        m_currentGroup = m_currentGroup.Parent;
        int index = m_currentGroupIndex.Pop();
        index++;

        if( index < m_currentGroup.ItemCount )
        {
          m_currentGroup = this.MoveToFirstLeafGroup( m_currentGroup, index );

          foundNonEmptyLeafGroup = ( m_currentGroup.RawItems.Count > 0 );
        }
      }

      return foundNonEmptyLeafGroup;
    }

    private DataGridCollectionViewGroup MoveToFirstLeafGroup( DataGridCollectionViewGroup referenceGroup, int index )
    {
      while( !referenceGroup.IsBottomLevel )
      {
        referenceGroup = referenceGroup.Items[ index ] as DataGridCollectionViewGroup;
        m_currentGroupIndex.Push( index );

        if( index != 0 )
          index = 0;
      }

      return referenceGroup;
    }

    public void Reset()
    {
      m_beforeStart = true;
      m_afterEnd = false;

      m_current = null;
      m_currentItemIndex = 0;
      m_currentGroupIndex.Clear();

      m_currentGroup = ( m_collectionView.Count == 0 ) ? null : this.MoveToFirstLeafGroup( m_collectionView.RootGroup, 0 );
    }

    #endregion

    private DataGridCollectionViewGroup m_currentGroup;

    private RawItem m_current;
    private Stack<int> m_currentGroupIndex = new Stack<int>( 16 );
    private int m_currentItemIndex;
    private bool m_beforeStart;
    private bool m_afterEnd;
    private int m_version;
    private DataGridCollectionView m_collectionView;
  }
}
