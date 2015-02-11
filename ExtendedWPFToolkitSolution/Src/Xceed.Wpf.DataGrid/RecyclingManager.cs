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
using System.Diagnostics;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class RecyclingManager
  {
    #region RefCount Property

    public int RefCount
    {
      get
      {
        return m_generators.Count;
      }
    }

    #endregion

    public void EnqueueItemContainer( object item, DependencyObject container )
    {
      Debug.Assert( container != null );

      var entry = new ItemRecyclingPoolContainer( item, container );

      m_itemRecyclingPool.Add( entry, entry );
    }

    public DependencyObject DequeueItemContainer( object item )
    {
      if( m_itemRecyclingPool.Count <= 0 )
        return null;

      var retriever = new ItemRecyclingPoolContainerRetriever( item );

      ItemRecyclingPoolEntry value;
      if( !m_itemRecyclingPool.TryGetValue( retriever, out value ) )
      {
        value = m_itemRecyclingPool.First().Value;
      }

      m_itemRecyclingPool.Remove( value );

      return value.Container;
    }

    public void EnqueueHeaderFooterContainer( object key, DependencyObject container )
    {
      Stack<DependencyObject> headerFooterItemTypeStack;

      if( !m_headerRecyclingStacks.TryGetValue( key, out headerFooterItemTypeStack ) )
      {
        headerFooterItemTypeStack = new Stack<DependencyObject>();
        m_headerRecyclingStacks.Add( key, headerFooterItemTypeStack );
      }

      headerFooterItemTypeStack.Push( container );
    }

    public DependencyObject DequeueHeaderFooterContainer( object key )
    {
      Stack<DependencyObject> headerFooterItemTypeStack;

      if( m_headerRecyclingStacks.TryGetValue( key, out headerFooterItemTypeStack ) )
      {
        if( headerFooterItemTypeStack.Count > 0 )
          return headerFooterItemTypeStack.Pop();
      }

      return null;
    }

    public void EnqueueGroupHeaderFooterContainer( object key, object item, DependencyObject container )
    {
      Dictionary<object, Stack<DependencyObject>> headerFooterRecyclingStacks;

      if( !m_groupLevelRecyclingStacks.TryGetValue( key, out headerFooterRecyclingStacks ) )
      {
        headerFooterRecyclingStacks = new Dictionary<object, Stack<DependencyObject>>();
        m_groupLevelRecyclingStacks.Add( key, headerFooterRecyclingStacks );
      }

      if( item is GroupHeaderFooterItem )
      {
        item = ( ( GroupHeaderFooterItem )item ).Template;
      }

      Stack<DependencyObject> headerFooterItemTypeStack;

      if( !headerFooterRecyclingStacks.TryGetValue( item, out headerFooterItemTypeStack ) )
      {
        headerFooterItemTypeStack = new Stack<DependencyObject>();
        headerFooterRecyclingStacks.Add( item, headerFooterItemTypeStack );
      }

      headerFooterItemTypeStack.Push( container );
    }

    public DependencyObject DequeueGroupHeaderFooterContainer( object key, object item )
    {
      Dictionary<object, Stack<DependencyObject>> headerFooterRecyclingStacks;

      if( m_groupLevelRecyclingStacks.TryGetValue( key, out headerFooterRecyclingStacks ) )
      {
        if( item is GroupHeaderFooterItem )
        {
          item = ( ( GroupHeaderFooterItem )item ).Template;
        }

        Stack<DependencyObject> headerFooterItemTypeStack;

        if( headerFooterRecyclingStacks.TryGetValue( item, out headerFooterItemTypeStack ) )
        {
          if( headerFooterItemTypeStack.Count > 0 )
            return headerFooterItemTypeStack.Pop();
        }
      }

      return null;
    }

    public IList<DependencyObject> Clear()
    {
      var removedContainers = new List<DependencyObject>( ( from entry in m_itemRecyclingPool.Values
                                                            select entry.Container ) );

      m_itemRecyclingPool.Clear();

      foreach( Stack<DependencyObject> stack in m_headerRecyclingStacks.Values )
      {
        foreach( DependencyObject container in stack )
        {
          removedContainers.Add( container );
        }

        stack.Clear();
      }

      m_headerRecyclingStacks.Clear();

      foreach( Dictionary<object, Stack<DependencyObject>> stacks in m_groupLevelRecyclingStacks.Values )
      {
        foreach( Stack<DependencyObject> stack in stacks.Values )
        {
          foreach( DependencyObject container in stack )
          {
            removedContainers.Add( container );
          }

          stack.Clear();
        }

        stacks.Clear();
      }

      m_groupLevelRecyclingStacks.Clear();

      return removedContainers;
    }

    public void AddRef( CustomItemContainerGenerator reference )
    {
      Debug.Assert( reference != null );

      if( m_generators.Contains( reference ) )
        return;

      m_generators.Add( reference );
    }

    public void RemoveRef( CustomItemContainerGenerator reference )
    {
      Debug.Assert( reference != null );

      m_generators.Remove( reference );
    }

    #region Private Fields

    private readonly Dictionary<object, Dictionary<object, Stack<DependencyObject>>> m_groupLevelRecyclingStacks = new Dictionary<object, Dictionary<object, Stack<DependencyObject>>>();
    private readonly Dictionary<object, Stack<DependencyObject>> m_headerRecyclingStacks = new Dictionary<object, Stack<DependencyObject>>();
    private readonly Dictionary<ItemRecyclingPoolEntry, ItemRecyclingPoolEntry> m_itemRecyclingPool = new Dictionary<ItemRecyclingPoolEntry, ItemRecyclingPoolEntry>( new ItemRecyclingPoolEqualityComparer() );
    private readonly HashSet<CustomItemContainerGenerator> m_generators = new HashSet<CustomItemContainerGenerator>();

    #endregion

    #region ItemRecyclingPoolEntry Private Class

    private abstract class ItemRecyclingPoolEntry
    {
      internal ItemRecyclingPoolEntry( object dataItem )
      {
        m_key = ( dataItem != null ) ? dataItem.GetHashCode() : 0;
      }

      internal int Key
      {
        get
        {
          return m_key;
        }
      }

      // This property has been set here to remove the casts every time we want to
      // retrieve the container from an instance of ItemRecyclingPoolContainer.
      internal abstract DependencyObject Container
      {
        get;
      }

      public override int GetHashCode()
      {
        return m_key;
      }

      public override bool Equals( object obj )
      {
        var item = obj as ItemRecyclingPoolEntry;
        if( item == null )
          return false;

        return ( item.m_key == m_key );
      }

      private readonly int m_key;
    }

    #endregion

    #region ItemRecyclingPoolContainer Private Class

    private sealed class ItemRecyclingPoolContainer : ItemRecyclingPoolEntry
    {
      internal ItemRecyclingPoolContainer( object dataItem, DependencyObject container )
        : base( dataItem )
      {
        Debug.Assert( container != null );

        m_container = container;
      }

      internal override DependencyObject Container
      {
        get
        {
          return m_container;
        }
      }

      public override int GetHashCode()
      {
        return base.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        if( !base.Equals( obj ) )
          return false;

        if( obj == this )
          return true;

        var item = obj as ItemRecyclingPoolContainer;

        return ( item != null )
            && ( item.m_container == m_container );
      }

      private readonly DependencyObject m_container;
    }

    #endregion

    #region ItemRecyclingPoolContainerRetriever Private Class

    private sealed class ItemRecyclingPoolContainerRetriever : ItemRecyclingPoolEntry
    {
      internal ItemRecyclingPoolContainerRetriever( object dataItem )
        : base( dataItem )
      {
      }

      internal override DependencyObject Container
      {
        get
        {
          throw new NotSupportedException( "A retriever has no container." );
        }
      }
    }

    #endregion

    #region ItemRecyclingPoolEqualityComparer Private Class

    private sealed class ItemRecyclingPoolEqualityComparer : IEqualityComparer<ItemRecyclingPoolEntry>
    {
      public int GetHashCode( ItemRecyclingPoolEntry obj )
      {
        Debug.Assert( obj != null );

        return obj.GetHashCode();
      }

      public bool Equals( ItemRecyclingPoolEntry x, ItemRecyclingPoolEntry y )
      {
        if( object.ReferenceEquals( x, y ) )
          return true;

        if( object.ReferenceEquals( x, null ) || object.ReferenceEquals( y, null ) )
          return false;

        // If one of the entry is a retriever, invoke its Equals method so the retriever may do its job.
        if( x is ItemRecyclingPoolContainerRetriever )
          return x.Equals( y );

        return y.Equals( x );
      }
    }

    #endregion
  }
}
