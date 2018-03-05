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
using System.Collections.Specialized;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal delegate void CustomGeneratorChangedEventHandler( object sender, CustomGeneratorChangedEventArgs e );

  internal sealed class CustomGeneratorChangedEventArgs : EventArgs
  {
    private CustomGeneratorChangedEventArgs( NotifyCollectionChangedAction action, int count, IList<DependencyObject> containers )
    {
      m_action = action;
      m_count = count;
      m_containers = containers;
    }

    internal NotifyCollectionChangedAction Action
    {
      get
      {
        return m_action;
      }
    }

    internal int Count
    {
      get
      {
        return m_count;
      }
    }

    internal IList<DependencyObject> Containers
    {
      get
      {
        return m_containers;
      }
    }

    internal static CustomGeneratorChangedEventArgs Add( int count )
    {
      return new CustomGeneratorChangedEventArgs( NotifyCollectionChangedAction.Add, count, null );
    }

    internal static CustomGeneratorChangedEventArgs Remove( int count, IList<DependencyObject> containers )
    {
      return new CustomGeneratorChangedEventArgs( NotifyCollectionChangedAction.Remove, count, containers );
    }

    internal static CustomGeneratorChangedEventArgs Reset()
    {
      return new CustomGeneratorChangedEventArgs( NotifyCollectionChangedAction.Reset, 0, null );
    }

    private readonly NotifyCollectionChangedAction m_action;
    private readonly int m_count;
    private readonly IList<DependencyObject> m_containers;
  }
}
