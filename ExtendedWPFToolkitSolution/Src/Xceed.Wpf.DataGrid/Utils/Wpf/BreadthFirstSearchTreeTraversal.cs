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
using System.Windows;

namespace Xceed.Utils.Wpf
{
  internal sealed class BreadthFirstSearchTreeTraversal : ITreeTraversal
  {
    #region Current Property

    public DependencyObject Current
    {
      get
      {
        if( m_current == null )
          throw new InvalidOperationException();

        return m_current;
      }
    }

    private DependencyObject m_current; //null

    #endregion

    public bool MoveNext()
    {
      if( m_collection.Count > 0 )
      {
        m_current = m_collection.Dequeue();
        return true;
      }
      else
      {
        m_current = null;
        return false;
      }
    }

    public void VisitNodes( IEnumerable<DependencyObject> nodes )
    {
      if( nodes == null )
        return;

      foreach( var node in nodes )
      {
        m_collection.Enqueue( node );
      }
    }

    #region Private Fields

    private readonly Queue<DependencyObject> m_collection = new Queue<DependencyObject>();

    #endregion
  }
}
