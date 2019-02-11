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
  //   elements, even though they can be found in the m_fixedPanel. This also means that
  //   for a FixedCellCount of 0, there can still be some (collapsed) element in m_fixedPanel.
  // - The scrolling panel only contains the elements presently in ViewPort
  internal class VirtualizingUICellCollection : UIElementCollection
  {
    // The visualParent is passed to the base constructor because it is mandatory, but 
    // the element's visual parent will always be either the fixed or the scrolling sub 
    // panel. This is one of the reason why we don't use the base implementation of this
    // class functions.
    public VirtualizingUICellCollection( Panel fixedPanel, Panel scrollingPanel, FixedCellPanel parentFixedCellPanel )
      : base( parentFixedCellPanel, parentFixedCellPanel )
    {
      if( fixedPanel == null )
        throw new ArgumentNullException( "fixedPanel" );

      if( scrollingPanel == null )
        throw new ArgumentNullException( "scrollingPanel" );

      if( parentFixedCellPanel == null )
        throw new ArgumentNullException( "parentFixedCellPanel" );

      m_fixedPanel = fixedPanel;
      m_scrollingPanel = scrollingPanel;
      m_parentVirtualizingCellsHost = parentFixedCellPanel as IVirtualizingCellsHost;
    }

    public override int Capacity
    {
      get
      {
        return 0;
      }
      set
      {
        Debug.Fail( "Check to see if the caller does something meaningful with this. We don't want to implement this property." );
      }
    }

    public override void Clear()
    {
      //Empty both sub panels
      m_fixedPanel.Children.Clear();
      m_scrollingPanel.Children.Clear();
    }

    public override bool Contains( UIElement element )
    {
      //check for presence in both sub panels
      return m_fixedPanel.Children.Contains( element ) || m_scrollingPanel.Children.Contains( element );
    }

    public override void CopyTo( Array array, int index )
    {
      throw new NotSupportedException();
    }

    public override void CopyTo( UIElement[] array, int index )
    {
      throw new NotSupportedException();
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
      return new CombinedEnumerator( this );
    }

    public override int IndexOf( UIElement element )
    {
      //get the index of the element in the fixed sub panel first.
      int fixedIndex = m_fixedPanel.Children.IndexOf( element );

      //if found, return this index as it is valid
      if( fixedIndex != -1 )
        return fixedIndex;

      //then determine the index of the element in the scrolling sub panel.
      int scrollingIndex = m_scrollingPanel.Children.IndexOf( element );

      //if item not found in the second sub panel, return -1 right away.
      if( scrollingIndex == -1 )
        return -1;

      int fixedCount = m_fixedPanel.Children.Count;

      //finally, item was found in second sub panel, return a combined value
      return fixedCount + scrollingIndex;
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

    public override int Add( UIElement element )
    {
      throw new NotSupportedException( "VirtualizingUICellCollection.Add" );
    }

    public override void Insert( int index, UIElement element )
    {
      throw new NotSupportedException( "VirtualizingUICellCollection.Insert" );
    }

    public override void Remove( UIElement element )
    {
      throw new NotSupportedException( "VirtualizingUICellCollection.Remove" );
    }

    public override void RemoveAt( int index )
    {
      throw new NotSupportedException( "VirtualizingUICellCollection.RemoveAt" );
    }

    public override void RemoveRange( int index, int count )
    {
      throw new NotSupportedException( "VirtualizingUICellCollection.RemoveRange" );
    }

    public override UIElement this[ int index ]
    {
      get
      {
        //check if the index is out of the valid range
        if( ( index < 0 ) || ( index >= this.Count ) )
          throw new ArgumentOutOfRangeException( "index" );

        int fixedCount = m_fixedPanel.Children.Count;

        if( index < fixedCount )
        {
          return m_fixedPanel.Children[ index ];
        }

        return m_scrollingPanel.Children[ index - fixedCount ];
      }
      set
      {
        throw new NotSupportedException( "The VirtualizingUICellCollection does not support direct modification of its children." );
      }
    }

    public void ClearCellLogicalParent( UIElement targetElement )
    {
      if( ( m_parentVirtualizingCellsHost != null ) && ( m_parentVirtualizingCellsHost.CanModifyLogicalParent == false ) )
      {
        this.ClearLogicalParent( targetElement );
      }
    }

    public void SetCellLogicalParent( UIElement targetElement )
    {
      if( ( m_parentVirtualizingCellsHost != null ) && ( m_parentVirtualizingCellsHost.CanModifyLogicalParent == false ) )
      {
        this.SetLogicalParent( targetElement );
      }
    }

    private void IncrementVersion()
    {
      unchecked
      {
        m_version++;
      }
    }

    private Panel m_fixedPanel;
    private Panel m_scrollingPanel;
    private IVirtualizingCellsHost m_parentVirtualizingCellsHost;
    private int m_version;

    private class CombinedEnumerator : IEnumerator
    {
      public CombinedEnumerator( VirtualizingUICellCollection cellCollection )
      {
        if( cellCollection == null )
          throw new ArgumentNullException( "cellCollection" );

        m_cellCollection = cellCollection;
      }

      #region IEnumerator Members

      public object Current
      {
        get
        {
          //Enumerator not started.
          if( m_fixedEnumerator == null )
            throw new InvalidOperationException();

          //If enumerating through fixed panel
          if( m_fixed == true )
            return m_fixedEnumerator.Current;

          //otherwise, use the scrolling enumerator
          return m_scrollingEnumerator.Current;
        }
      }

      public bool MoveNext()
      {
        //If the Enumerator is not started, 
        if( m_fixedEnumerator == null )
        {
          m_fixedEnumerator = m_cellCollection.m_fixedPanel.Children.GetEnumerator();
          m_scrollingEnumerator = m_cellCollection.m_scrollingPanel.Children.GetEnumerator();

          //problems creating one of the enumerator
          if( ( m_fixedEnumerator == null ) || ( m_scrollingEnumerator == null ) )
          {
            m_fixedEnumerator = null;
            m_scrollingEnumerator = null;

            //could not move to first item.
            return false;
          }

          //flag to the CombinedEnumerator to use the Fixed enumerator.
          m_fixed = true;
        }

        //If Fixed enumerator is in use
        if( m_fixed == true )
        {
          //try to move the Fixed enumerator
          bool fixedRetval = m_fixedEnumerator.MoveNext();
          if( fixedRetval == true )
          {
            //success, return true
            return true;
          }
          else
          {
            //failure, reached the end of enumerator empty
            m_fixed = false;

            //continue with execution so that scrolling enumerator gets started
          }
        }

        //If it returns false, then scrolling panel is empty, or we reaced the end.
        return m_scrollingEnumerator.MoveNext();
      }

      public void Reset()
      {
        m_fixedEnumerator = null;
        m_scrollingEnumerator = null;
      }

      #endregion

      private VirtualizingUICellCollection m_cellCollection;
      private bool m_fixed;
      private IEnumerator m_fixedEnumerator;
      private IEnumerator m_scrollingEnumerator;
    }
  }
}
