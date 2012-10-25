/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System.Collections;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal class CollectionGeneratorNode : NotifyCollectionChangedGeneratorNode
  {
    protected CollectionGeneratorNode( IList list, GeneratorNode parent )
      : base( parent )
    {
      Debug.Assert( list != null, "list cannot be null for CollectionGeneratorNode" );

      m_items = list;
    }

    public IList Items
    {
      get
      {
        return m_items;
      }
    }

    public virtual int IndexOf( object item )
    {
      return m_items.IndexOf( item );
    }

    public virtual object GetAt( int index )
    {
      return m_items[ index ];
    }

    private readonly IList m_items;
  }
}
