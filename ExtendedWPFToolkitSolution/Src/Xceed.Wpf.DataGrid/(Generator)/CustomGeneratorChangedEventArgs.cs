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
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal delegate void CustomGeneratorChangedEventHandler( object sender, CustomGeneratorChangedEventArgs e );

  internal class CustomGeneratorChangedEventArgs : EventArgs
  {
    internal CustomGeneratorChangedEventArgs( NotifyCollectionChangedAction action, GeneratorPosition position, int index, int itemCount, int itemUICount )
      : this( action, position, index, new GeneratorPosition( -1, 0 ), -1, itemCount, itemUICount, null )
    {

    }

    internal CustomGeneratorChangedEventArgs( NotifyCollectionChangedAction action, GeneratorPosition position, int index, int itemCount, int itemUICount, IList<DependencyObject> removedContainers )
      : this( action, position, index, new GeneratorPosition( -1, 0 ), -1, itemCount, itemUICount, removedContainers )
    {

    }

    internal CustomGeneratorChangedEventArgs( NotifyCollectionChangedAction action, GeneratorPosition position, int index, GeneratorPosition oldPosition, int oldIndex, int itemCount, int itemUICount, IList<DependencyObject> removedContainers )
    {
      _action = action;
      _position = position;
      _oldPosition = oldPosition;
      _itemCount = itemCount;
      _itemUICount = itemUICount;
      _index = index;
      _oldIndex = oldIndex;
      m_removedContainers = removedContainers;
    }

    public NotifyCollectionChangedAction Action
    {
      get
      {
        return _action;
      }
    }

    public int ItemCount
    {
      get
      {
        return _itemCount;
      }
    }

    public int ItemUICount
    {
      get
      {
        return _itemUICount;
      }
    }

    public GeneratorPosition OldPosition
    {
      get
      {
        return _oldPosition;
      }
    }

    public GeneratorPosition Position
    {
      get
      {
        return _position;
      }
    }

    public IList<DependencyObject> RemovedContainers
    {
      get
      {
        return m_removedContainers;
      }
    }

    public int Index
    {
      get
      {
        return _index;
      }
    }

    public int OldIndex
    {
      get
      {
        return _oldIndex;
      }
    }

    private readonly NotifyCollectionChangedAction _action;
    private readonly int _itemCount;
    private readonly int _itemUICount;
    private readonly GeneratorPosition _oldPosition;
    private readonly GeneratorPosition _position;
    private readonly IList<DependencyObject> m_removedContainers;
    private readonly int _index;
    private readonly int _oldIndex;

  }
}
