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
using System.Windows;

namespace Xceed.Utils.Wpf
{
  internal sealed class DepthFirstSearchTreeTraversal : ITreeTraversal
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
      var lastIndex = m_collection.Count - 1;
      if( lastIndex >= 0 )
      {
        m_current = m_collection[ lastIndex ];
        m_collection.RemoveAt( lastIndex );
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

      m_collection.AddRange( nodes.Reverse() );
    }

    #region Private Fields

    private readonly List<DependencyObject> m_collection = new List<DependencyObject>();

    #endregion
  }
}
