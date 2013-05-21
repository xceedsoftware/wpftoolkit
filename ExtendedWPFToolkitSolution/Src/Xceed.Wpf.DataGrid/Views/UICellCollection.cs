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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid.Views
{
  // This class manages the various panel of a FixedCellPanel and the Logical/Visual 
  // Parent of the elements added to it. More precisely :
  // - The logical parent of all the elements added to this collection will be the 
  //   FixedCellPanel.
  // - An element added or inserted will be either added to the "fixed" sub panel or the
  //   "scrolling" sub panel, according to its index. Thus, the element's Visual Parent 
  //   will either be the fixed or the scrolling sub panel.
  // - When removing or inserting elements, some other elements may see their parent 
  //   change. From the fixed sub panel to the scrolling sub panel or vice versa.
  // - As long as the elements are added to FixedCellSubPanel objects (like our 
  //   m_fixedPanel and m_scrollingPanel), this Visual vs Logical parent management will 
  //   work.
  // - The collapsed elements are not considered when counting the number of fixed 
  //   elements, even 'though they can be found in the m_fixedPanel. This also means that
  //   for a FixedCellCount of 0, there can still be some (collapsed) element in m_fixedPanel.
  internal class UICellCollection : UIElementCollection
  {
    // The visualParent is passed to the base constructor because it is mandatory, but 
    // the element's visual parent will always be either the fixed or the scrolling sub 
    // panel. This is one of the reason why we don't use the base implementation of this
    // class functions.
    public UICellCollection( Panel fixedPanel, Panel scrollingPanel, FixedCellPanel parentFixedCellPanel )
      : base( parentFixedCellPanel, parentFixedCellPanel )
    {
      if( fixedPanel == null )
        throw new ArgumentNullException( "fixedPanel" );
      if( scrollingPanel == null )
        throw new ArgumentNullException( "scrollingPanel" );
      if( parentFixedCellPanel == null )
        throw new ArgumentNullException( "logicalParent" );

      m_fixedPanel = fixedPanel;
      m_scrollingPanel = scrollingPanel;
      m_parentFixedCellPanel = parentFixedCellPanel;
    }

    public override int Add( UIElement element )
    {
      int itemsCount = this.Count;

      if( this.GetFixedVisibleChildrenCount() < m_parentFixedCellPanel.FixedCellCount )
      {
        m_fixedPanel.Children.Add( element );
      }
      else
      {
        m_scrollingPanel.Children.Add( element );
      }

      this.SetLogicalParent( element );
      this.IncrementVersion();

      return itemsCount; // The old items count is necessarily the new element's index.
    }

    public override int Capacity
    {
      get
      {
        Debug.Fail( "Check to see if the caller does something meaningful with this. We don't want to implement this property." );
        return 0;
      }
      set
      {
        Debug.Fail( "Check to see if the caller does something meaningful with this. We don't want to implement this property." );
      }
    }

    public override void Clear()
    {
      int itemsCount = this.Count;

      if( itemsCount > 0 )
      {
        UIElement[] elementArray = new UIElement[ itemsCount ];
        this.CopyTo( elementArray, 0 );
        m_fixedPanel.Children.Clear();
        m_scrollingPanel.Children.Clear();

        // Only clear the logical parent once the elements are no longer attached.
        UIElement element = null;
        for( int i = 0; i < itemsCount; i++ )
        {
          element = elementArray[ i ];

          if( element != null )
            this.ClearLogicalParent( element );
        }

        this.IncrementVersion();
      }
    }

    public override bool Contains( UIElement element )
    {
      return ( ( m_scrollingPanel.Children.Contains( element ) ) || 
               ( m_fixedPanel.Children.Contains( element ) ) );
    }

    public override void CopyTo( Array array, int index )
    {
      int fixedItemsCount = m_fixedPanel.Children.Count;
      int scrollingItemsCount = m_scrollingPanel.Children.Count;

      if( array == null )
        throw new ArgumentNullException( "array" );

      if( array.Rank != 1 )
        throw new ArgumentException( "The destination array must be one-dimensional (Rank == 1)." );

      if( ( index < 0 ) || ( array.Length - index < fixedItemsCount + scrollingItemsCount ) )
        throw new ArgumentException( "The array size, given the provided index, cannot accommodate this collection element count." );

      for( int i = 0; i < fixedItemsCount; i++ )
      {
        array.SetValue( m_fixedPanel.Children[ i ], i + index );
      }

      for( int i = 0; i < scrollingItemsCount; i++ )
      {
        array.SetValue( m_scrollingPanel.Children[ i ], fixedItemsCount + i + index );
      }
    }

    public override void CopyTo( UIElement[] array, int index )
    {
      this.CopyTo( ( Array )array, index );
    }

    public override int Count
    {
      get
      {
        return m_fixedPanel.Children.Count + m_scrollingPanel.Children.Count;
      }
    }

    public override IEnumerator GetEnumerator()
    {
      return new CellEnumerator( this );
    }

    public override int IndexOf( UIElement element )
    {
      int index = m_scrollingPanel.Children.IndexOf( element );

      if( index >= 0 )
      {
        index += m_fixedPanel.Children.Count;
      }
      else
      {
        index = m_fixedPanel.Children.IndexOf( element );
      }

      return index;
    }

    public override void Insert( int index, UIElement element )
    {
      int itemsCount = this.Count;

      if( index > itemsCount )
        index = itemsCount;

      if( ( index < m_fixedPanel.Children.Count ) || ( itemsCount == m_fixedPanel.Children.Count ) )
      {
        m_fixedPanel.Children.Insert( index, element );
      }
      else
      {
        m_scrollingPanel.Children.Insert( index - m_fixedPanel.Children.Count, element );
      }

      if( element.Visibility != Visibility.Collapsed )
      {
        int fixedPanelLastChildIndex = m_fixedPanel.Children.Count - 1;
        int fixedCellCount = m_parentFixedCellPanel.FixedCellCount;

        // If necessary, move some elements (one visible and possibly some collapsed) 
        // from the fixed panel to the scrolling panel.
        while( this.GetFixedVisibleChildrenCount() > fixedCellCount )
        {
          UIElement bumpedElement = m_fixedPanel.Children[ fixedPanelLastChildIndex ];

          m_fixedPanel.Children.RemoveAt( fixedPanelLastChildIndex );
          m_scrollingPanel.Children.Insert( 0, bumpedElement );
          // The logical parent has not changed. We don't have to call SetLogicalParent.
          fixedPanelLastChildIndex--;
        }
      }

      this.SetLogicalParent( element );
      this.IncrementVersion();
    }

    public override bool IsSynchronized
    {
      get
      {
        return m_scrollingPanel.Children.IsSynchronized;
      }
    }

    public override object SyncRoot
    {
      get
      {
        return m_scrollingPanel.Children.SyncRoot;
      }
    }

    public override void Remove( UIElement element )
    {
      int indexOfElement = m_scrollingPanel.Children.IndexOf( element );

      if( indexOfElement >= 0 )
      {
        m_scrollingPanel.Children.RemoveAt( indexOfElement );
      }
      else
      {
        indexOfElement = m_fixedPanel.Children.IndexOf( element );

        if( indexOfElement >= 0 )
        {
          m_fixedPanel.Children.RemoveAt( indexOfElement );

          int scrollingPanelChildrenCount = m_scrollingPanel.Children.Count;

          // If necessary, move some elements (one visible and possibly some collapsed) 
          // from the scrolling panel to the fixed panel.
          if( scrollingPanelChildrenCount > 0 )
          {
            int fixedCellCount = m_parentFixedCellPanel.FixedCellCount;

            while( ( this.GetFixedVisibleChildrenCount() < fixedCellCount ) && ( scrollingPanelChildrenCount > 0 ) )
            {
              UIElement bumpedElement = m_scrollingPanel.Children[ 0 ];

              m_scrollingPanel.Children.RemoveAt( 0 );
              m_fixedPanel.Children.Add( bumpedElement );
              // The logical parent has not changed. We don't have to call SetLogicalParent.
              scrollingPanelChildrenCount--;
            }
          }
        }
        else
        {
          Debug.Assert( false, "Tried to remove an element that is not a child of the FixedCellPanel" );
        }
      }

      this.ClearLogicalParent( element );
      this.IncrementVersion();
    }

    public override void RemoveAt( int index )
    {
      if( ( index >= this.Count ) || ( index < 0 ) )
        throw new ArgumentOutOfRangeException( "index", index, "index must be less than count and greater than zero." ); 

      int fixedPanelChildrenCount = m_fixedPanel.Children.Count;

      if( index < fixedPanelChildrenCount )
      {
        this.Remove( m_fixedPanel.Children[ index ] );
      }
      else
      {
        this.Remove( m_scrollingPanel.Children[ index - fixedPanelChildrenCount ] );
      }
    }

    public override void RemoveRange( int index, int count )
    {
      Debug.Fail( "This method has not been optimized because it should never be called." );

      if( index < 0 )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero." ); 

      if( count < 0 )
        throw new ArgumentOutOfRangeException( "count", count, "count must be greater than or equal to zero." ); 

      if( this.Count - index < count )
        throw new ArgumentException( "The specified index and count are greater than the size of this collection." ); 

      for( int i = 0; i < count; i++ )
      {
        this.RemoveAt( index );
      }
    }

    public override UIElement this[ int index ]
    {
      get
      {
        if( index < 0 )
          throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero." ); 

        if( index >= this.Count )
          throw new ArgumentOutOfRangeException( "index", index, "index must be less than the count of elements in this collection." ); 

        int fixedPanelChildrenCount = m_fixedPanel.Children.Count;

        if( index < fixedPanelChildrenCount )
        {
          return m_fixedPanel.Children[ index ];
        }
        else
        {
          return m_scrollingPanel.Children[ index - fixedPanelChildrenCount ];
        }
      }

      set
      {
        if( value == null )
          throw new ArgumentNullException( "value" );

        if( index < 0 )
          throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero." );

        if( index >= this.Count )
          throw new ArgumentOutOfRangeException( "index", index, "index must be less than the count of elements in this collection." ); 

        int fixedPanelChildrenCount = m_fixedPanel.Children.Count;

        UIElement oldElement = null;

        if( index < fixedPanelChildrenCount )
        {
          oldElement = m_fixedPanel.Children[ index ];
          m_fixedPanel.Children[ index ] = value;
        }
        else
        {
          oldElement = m_scrollingPanel.Children[ index - fixedPanelChildrenCount ];
          m_scrollingPanel.Children[ index - fixedPanelChildrenCount ] = value;
        }

        this.SetLogicalParent( value );
        this.IncrementVersion();

        if( oldElement != null )
          this.ClearLogicalParent( oldElement );
      }
    }

    // Update the content of the two panels (fixed and scrolling) when the 
    // FixedColumnCount has changed.
    public void UpdatePanels()
    {
      int fixedVisibleChildrenCount = this.GetFixedVisibleChildrenCount();
      int fixedCellCount = m_parentFixedCellPanel.FixedCellCount;

      if( fixedVisibleChildrenCount > fixedCellCount )
      {
        UIElement element;

        while( fixedVisibleChildrenCount > fixedCellCount )
        {
          element = m_fixedPanel.Children[ m_fixedPanel.Children.Count - 1 ];
          m_fixedPanel.Children.RemoveAt( m_fixedPanel.Children.Count - 1 );
          m_scrollingPanel.Children.Insert( 0, element );
          // The logical parent has not changed. We don't have to call SetLogicalParent.
          if( element.Visibility != Visibility.Collapsed )
            fixedVisibleChildrenCount--;
        }

        this.IncrementVersion();
      }
      else if( ( fixedVisibleChildrenCount < fixedCellCount ) && ( m_scrollingPanel.Children.Count > 0 ) )
      {
        UIElement element;

        while( ( fixedVisibleChildrenCount < fixedCellCount ) && ( m_scrollingPanel.Children.Count > 0 ) )
        {
          element = m_scrollingPanel.Children[ 0 ];
          m_scrollingPanel.Children.RemoveAt( 0 );
          m_fixedPanel.Children.Add( element );
          // The logical parent has not changed. We don't have to call SetLogicalParent.
          if( element.Visibility != Visibility.Collapsed )
            fixedVisibleChildrenCount++;
        }

        this.IncrementVersion();
      }
    }

    private int GetFixedVisibleChildrenCount()
    {
      int fixedVisibleChildrenCount = 0;

      foreach( UIElement child in m_fixedPanel.Children )
      {
        if( child.Visibility != Visibility.Collapsed )
          fixedVisibleChildrenCount++;
      }

      return fixedVisibleChildrenCount;
    }

    private void IncrementVersion()
    {
      unchecked
      {
        m_version++;
      }
    }

    #region Private Class CellEnumerator

    // A IEnumerator class optimized to work with the UICellCollection.
    private class CellEnumerator : IEnumerator
    {
      public CellEnumerator( UICellCollection cellCollection )
      {
        if( cellCollection == null )
          throw new ArgumentNullException( "cellCollection" );

        m_cellCollection = cellCollection;
        m_version = cellCollection.m_version;
        m_itemsCount = cellCollection.Count;
        m_fixedCellCount = cellCollection.m_fixedPanel.Children.Count;
        this.Reset();
      }

      public object Current
      {
        get
        {
          if( m_index < 0 )
            throw new InvalidOperationException( "The enumerator has not been started." ); 

          if( m_index >= m_itemsCount )
            throw new InvalidOperationException( "The enumerator has reached the end." ); 

          return m_current;
        }
      }

      public bool MoveNext()
      {
        if( m_version != m_cellCollection.m_version )
          throw new InvalidOperationException( "The collection has changed." );

        bool result = false;

        if( m_index < m_itemsCount )
        {
          m_index++;

          if( m_index < m_itemsCount )
          {
            if( m_index < m_fixedCellCount )
            {
              m_current = m_cellCollection.m_fixedPanel.Children[ m_index ];
            }
            else
            {
              m_current = m_cellCollection.m_scrollingPanel.Children[ m_index - m_fixedCellCount ];
            }

            result = true;
          }
          else
          {
            m_current = null;
          }
        }

        return result;
      }

      public void Reset()
      {
        if( m_version != m_cellCollection.m_version )
          throw new InvalidOperationException( "The collection has changed." ); 

        m_index = -1;
        m_current = null;
      }

      private UICellCollection m_cellCollection;
      private int m_index;
      private UIElement m_current;
      private int m_version;
      private int m_itemsCount;
      private int m_fixedCellCount;
    }

    #endregion Private Class CellEnumerator

    private Panel m_fixedPanel;
    private Panel m_scrollingPanel;
    private FixedCellPanel m_parentFixedCellPanel;
    private int m_version;
  }
}
