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
using System.Windows;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal class UnboundDataItem
  {
    #region CONSTRUCTORS

    private UnboundDataItem( object dataItem )
    {
      if( dataItem == null )
      {
        m_weakDataItem = null;
      }
      else
      {
        m_weakDataItem = new WeakReference( dataItem );
      }
    }

    #endregion CONSTRUCTORS

    public static void FreeUnboundDataItem( UnboundDataItemNode unboundDataItemNode )
    {
      lock( UnboundDataItem.UnboundDataItems )
      {
        UnboundDataItems.Remove( unboundDataItemNode );
      }
    }

    public static UnboundDataItemNode GetUnboundDataItemNode( object dataItem, out UnboundDataItem unboundDataItem )
    {
      lock( UnboundDataItem.UnboundDataItems )
      {
        UnboundDataItemNode current = UnboundDataItem.UnboundDataItems.FirstNode;

        while( current != null )
        {
          object currentDataItem = current.DataItem;
          unboundDataItem = current.UnboundDataItem;

          if( !current.IsAlive )
          {
            UnboundDataItemNode next = current.Next;
            UnboundDataItem.UnboundDataItems.Remove( current );
            current = next;
            continue;
          }

          if( object.Equals( currentDataItem, dataItem ) )
            return current;

          current = current.Next;
        }

        unboundDataItem = new UnboundDataItem( dataItem );
        UnboundDataItemNode unboundDataItemNode = new UnboundDataItemNode( unboundDataItem );
        UnboundDataItem.UnboundDataItems.Add( unboundDataItemNode );
        return unboundDataItemNode;
      }
    }

    public object DataItem
    {
      get
      {
        return ( m_weakDataItem == null ) ? null : m_weakDataItem.Target;
      }
    }

    private WeakReference m_weakDataItem;

    private static UnboundDataItemList UnboundDataItems = new UnboundDataItemList();

    internal class UnboundDataItemNode
    {
      public UnboundDataItemNode( UnboundDataItem unboundDataItem )
      {
        if( unboundDataItem == null )
          throw new ArgumentNullException( "unboundDataItem" );

        m_weakUnboundDataItem = new WeakReference( unboundDataItem );
      }

      public object DataItem
      {
        get
        {
          UnboundDataItem unboundDataItem = m_weakUnboundDataItem.Target as UnboundDataItem;

          if( unboundDataItem == null )
            return null;

          return unboundDataItem.DataItem;
        }
      }

      public UnboundDataItem UnboundDataItem
      {
        get
        {
          if( m_weakUnboundDataItem == null )
            return null;

          return m_weakUnboundDataItem.Target as UnboundDataItem;
        }
      }

      public UnboundDataItemNode Next
      {
        get;
        set;
      }

      public UnboundDataItemNode Previous
      {
        get;
        set;
      }

      public bool IsAlive
      {
        get
        {
          UnboundDataItem unboundDataItem = m_weakUnboundDataItem.Target as UnboundDataItem;

          if( unboundDataItem == null )
            return false;

          WeakReference weakDataItem = unboundDataItem.m_weakDataItem;

          return ( ( weakDataItem == null ) || ( weakDataItem.IsAlive ) )
            && ( m_weakUnboundDataItem.IsAlive );
        }
      }

      private WeakReference m_weakUnboundDataItem;
    }

    private class UnboundDataItemList
    {
      public void Add( UnboundDataItemNode newNode )
      {
        newNode.Next = this.FirstNode;

        if( this.FirstNode != null )
          this.FirstNode.Previous = newNode;

        this.FirstNode = newNode;

        if( this.LastNode == null )
          this.LastNode = this.FirstNode;

        m_count++;
      }

      public void Remove( UnboundDataItemNode unboundDataItemNode )
      {
        bool removed = false;

        if( this.LastNode == unboundDataItemNode )
        {
          this.LastNode = unboundDataItemNode.Previous;
          removed = true;
        }

        if( this.FirstNode == unboundDataItemNode )
        {
          this.FirstNode = unboundDataItemNode.Next;
          removed = true;
        }

        UnboundDataItemNode previous = unboundDataItemNode.Previous;

        if( previous != null )
        {
          previous.Next = unboundDataItemNode.Next;
          removed = true;
        }

        UnboundDataItemNode next = unboundDataItemNode.Next;

        if( next != null )
        {
          next.Previous = unboundDataItemNode.Previous;
          removed = true;
        }

        if( removed )
        {
          unboundDataItemNode.Previous = null;
          unboundDataItemNode.Next = null;
          m_count--;
        }
      }

      public UnboundDataItemNode FirstNode
      {
        get;
        private set;
      }

      public UnboundDataItemNode LastNode
      {
        get;
        private set;
      }

      public int Count
      {
        get
        {
          return m_count;
        }
      }

      private int m_count;
    }
  }
}
