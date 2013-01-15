/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System.Collections.Generic;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal class RecyclingManager
  {
    public void EnqueueItemContainer( DependencyObject container )
    {
      m_itemRecyclingStack.Push( container );
    }

    public DependencyObject DequeueItemContainer()
    {
      if( m_itemRecyclingStack.Count > 0 )
        return m_itemRecyclingStack.Pop();

      return null;
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
      List<DependencyObject> removedContainers = new List<DependencyObject>();

      foreach( DependencyObject container in m_itemRecyclingStack )
      {
        removedContainers.Add( container );
      }

      m_itemRecyclingStack.Clear();

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
      if( m_refList.Contains( reference ) == false )
      {
        m_refList.Add( reference );
      }
    }

    public void RemoveRef( CustomItemContainerGenerator reference )
    {
      if( m_refList.Contains( reference ) == true )
      {
        m_refList.Remove( reference );
      }
    }

    public int RefCount
    {
      get
      {
        return m_refList.Count;
      }
    }

    private readonly Dictionary<object, Dictionary<object, Stack<DependencyObject>>> m_groupLevelRecyclingStacks = new Dictionary<object, Dictionary<object, Stack<DependencyObject>>>();
    private readonly Dictionary<object, Stack<DependencyObject>> m_headerRecyclingStacks = new Dictionary<object, Stack<DependencyObject>>();
    private readonly Stack<DependencyObject> m_itemRecyclingStack = new Stack<DependencyObject>();
    private readonly List<CustomItemContainerGenerator> m_refList = new List<CustomItemContainerGenerator>();

  }
}
