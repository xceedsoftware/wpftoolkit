/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xceed.Utils.Collections
{
  internal interface IBinaryTree<T> : INotifyCollectionChanged, INotifyPropertyChanged, IEnumerable<T>, IEnumerable
  {
    IBinaryTreeNode<T> Root
    {
      get;
    }

    IBinaryTreeNode<T> Find( T value );

    IBinaryTreeNode<T> Insert( T value );
    IBinaryTreeNode<T> InsertBefore( T value, IBinaryTreeNode<T> node );
    IBinaryTreeNode<T> InsertAfter( T value, IBinaryTreeNode<T> node );

    void Remove( T value );
    void Remove( IBinaryTreeNode<T> node );

    void Clear();

    IEnumerable<T> GetItems( bool reverse );
  }
}
