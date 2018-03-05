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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Xceed.Wpf.DataGrid.Utils;

namespace Xceed.Utils.Collections
{
  // This class implements a left-leaning red–black that is based on
  // the research paper "Left-leaning Red-Black Trees" by Robert Sedgewick.
  internal class LLRBTree<T> : IBinaryTree<T>
  {
    #region Static Fields

    internal static readonly string RootPropertyName = PropertyHelper.GetPropertyName( ( LLRBTree<T> t ) => t.Root );

    #endregion

    internal LLRBTree()
      : this( EqualityComparer<T>.Default )
    {
    }

    internal LLRBTree( IEqualityComparer<T> comparer )
    {
      if( comparer == null )
        throw new ArgumentNullException( "comparer" );

      m_comparer = comparer;
    }

    #region Root Property

    public IBinaryTreeNode<T> Root
    {
      get
      {
        return m_rootNode;
      }
    }

    #endregion

    public IBinaryTreeNode<T> Find( T value )
    {
      this.EnsureValue( value );

      var node = this.FindNode( value );
      if( ( node != null ) && ( node.Tree != this ) )
        return null;

      return node;
    }

    public IBinaryTreeNode<T> Insert( T value )
    {
      if( m_rootNode != null )
        throw new InvalidOperationException( "This method cannot be called when the tree has a root node." );

      this.EnsureNewValue( value );

      m_rootNode = this.CreateNode( value );

      if( m_rootNode == null )
        throw new InvalidOperationException( "A node was not created." );

      this.SetRootNodeColor();
      this.OnPostInsert( m_rootNode, null );

      return m_rootNode;
    }

    public IBinaryTreeNode<T> InsertBefore( T value, IBinaryTreeNode<T> node )
    {
      this.EnsureNewValue( value );
      this.EnsureCurrentNode( node );

      var pivotNode = ( Node )node;
      var rightmostNode = ( pivotNode.Left != null ) ? pivotNode.Left.GetRightmost() : null;
      var oldRootNode = m_rootNode;
      var parentNode = rightmostNode ?? pivotNode;
      var newNode = this.CreateNode( value );

      if( newNode == null )
        throw new InvalidOperationException( "A node was not created." );

      if( rightmostNode != null )
      {
        parentNode.Right = newNode;
      }
      else
      {
        parentNode.Left = newNode;
      }

      newNode.Parent = parentNode;

      this.BalanceUp( parentNode );
      this.SetRootNodeColor();
      this.OnPostInsert( newNode, oldRootNode );

      return newNode;
    }

    public IBinaryTreeNode<T> InsertAfter( T value, IBinaryTreeNode<T> node )
    {
      this.EnsureNewValue( value );
      this.EnsureCurrentNode( node );

      var pivotNode = ( Node )node;
      var leftmostNode = ( pivotNode.Right != null ) ? pivotNode.Right.GetLeftmost() : null;
      var oldRootNode = m_rootNode;
      var parentNode = leftmostNode ?? pivotNode;
      var newNode = this.CreateNode( value );

      if( newNode == null )
        throw new InvalidOperationException( "A node was not created." );

      if( leftmostNode != null )
      {
        parentNode.Left = newNode;
      }
      else
      {
        parentNode.Right = newNode;
      }

      newNode.Parent = parentNode;

      this.BalanceUp( parentNode );
      this.SetRootNodeColor();
      this.OnPostInsert( newNode, oldRootNode );

      return newNode;
    }

    public void Remove( T value )
    {
      this.EnsureValue( value );

      this.Remove( this.FindNode( value ) );
    }

    public void Remove( IBinaryTreeNode<T> node )
    {
      this.EnsureCurrentNode( node );

      var targetNode = ( Node )node;
      var path = new Stack<Node>();
      path.Push( targetNode );

      this.FindPath( path, m_rootNode );

      var oldValue = node.Value;
      var oldRootNode = m_rootNode;
      var oldParentNode = default( Node );

      m_rootNode = this.Remove( path, true, out oldParentNode );

      this.SetRootNodeColor();
      this.OnPostRemove( oldValue, oldParentNode, oldRootNode );
    }

    public void Clear()
    {
      if( m_rootNode == null )
        return;

      this.Clear( m_rootNode );

      m_rootNode = null;

      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
      this.OnPropertyChanged( LLRBTree<T>.RootPropertyName );
    }

    public IEnumerable<T> GetItems( bool reverse )
    {
      return this.GetNodes( reverse ).Select( node => node.Value );
    }

    protected virtual Node CreateNode( T value )
    {
      return new Node( this, value );
    }

    protected virtual void ClearNode( Node node )
    {
      if( node == null )
        return;

      node.Parent = null;
      node.Left = null;
      node.Right = null;
      node.IsRed = true;
    }

    protected virtual Node FindNode( T value )
    {
      foreach( var node in this.GetNodes( false ) )
      {
        if( m_comparer.Equals( value, node.Value ) )
          return node;
      }

      return null;
    }

    protected virtual void EnsureValue( T value )
    {
    }

    protected virtual void EnsureNewValue( T value )
    {
      this.EnsureValue( value );
    }

    private IEnumerable<Node> GetNodes( bool reverse )
    {
      if( m_rootNode == null )
        yield break;

      if( reverse )
      {
        for( var node = m_rootNode.GetRightmost() ?? m_rootNode; node != null; node = node.GetPrevious() )
        {
          yield return node;
        }
      }
      else
      {
        for( var node = m_rootNode.GetLeftmost() ?? m_rootNode; node != null; node = node.GetNext() )
        {
          yield return node;
        }
      }
    }

    private Node Remove( Stack<Node> path, bool destroy, out Node oldParentNode )
    {
      Debug.Assert( path != null );
      Debug.Assert( path.Count > 0 );

      var node = path.Pop();

      if( ( path.Count > 0 ) && ( node.Left == path.Peek() ) )
      {
        if( !this.IsRed( node.Left ) && !this.IsRed( node.Left.Left ) )
        {
          node = this.MoveRedLeft( node );

          this.CutPath( path );
          this.FindPath( path, node.Left );
        }

        node.Left = this.Remove( path, destroy, out oldParentNode );
      }
      else
      {
        if( this.IsRed( node.Left ) )
        {
          if( path.Count <= 0 )
          {
            path.Push( node );
          }

          node = this.RotateRight( node );

          Debug.Assert( path.Count > 0 );

          this.CutPath( path );
          this.FindPath( path, node.Right );
        }

        if( ( node.Right == null ) && ( path.Count == 0 ) )
        {
          Debug.Assert( node.Left == null );

          oldParentNode = node.Parent;

          if( destroy )
          {
            this.ClearNode( node );
          }

          return null;
        }

        if( !this.IsRed( node.Right ) && !this.IsRed( node.Right.Left ) )
        {
          if( path.Count <= 0 )
          {
            path.Push( node );
          }

          node = this.MoveRedRight( node );

          Debug.Assert( path.Count > 0 );

          if( path.Peek() == node )
          {
            path.Pop();
          }
          else
          {
            this.CutPath( path );
            this.FindPath( path, node.Right );
          }
        }

        if( path.Count == 0 )
        {
          Debug.Assert( node.Right != null );

          var child = default( Node );
          var target = node.Right.GetLeftmost();

          Debug.Assert( target != null );
          Debug.Assert( target.Left == null );

          if( target != node.Right )
          {
            path.Push( target );
            this.FindPath( path, node.Right );

            child = this.Remove( path, false, out oldParentNode );
          }
          else
          {
            oldParentNode = target;
          }

          target.Parent = node.Parent;
          target.Left = node.Left;
          target.Right = child;
          target.IsRed = node.IsRed;

          if( target.Left != null )
          {
            target.Left.Parent = target;
          }

          if( target.Right != null )
          {
            target.Right.Parent = target;
          }

          if( target.Parent != null )
          {
            if( target.Parent.Left == node )
            {
              target.Parent.Left = target;
            }
            else
            {
              target.Parent.Right = target;
            }
          }

          Debug.Assert( destroy );
          if( destroy )
          {
            this.ClearNode( node );
          }

          node = target;
        }
        else
        {
          node.Right = this.Remove( path, destroy, out oldParentNode );
        }
      }

      return this.Balance( node );
    }

    private void BalanceUp( Node node )
    {
      while( node != null )
      {
        node = this.Balance( node ).Parent;
      }
    }

    private Node Balance( Node node )
    {
      if( this.IsRed( node.Right ) && !this.IsRed( node.Left ) )
      {
        // Avoid right-leaning node
        node = this.RotateLeft( node );
      }

      if( this.IsRed( node.Left ) && this.IsRed( node.Left.Left ) )
      {
        node = this.RotateRight( node );
      }

      if( this.IsRed( node.Left ) && this.IsRed( node.Right ) )
      {
        this.FlipColor( node );
      }

      return node;
    }

    private void FlipColor( Node node )
    {
      Debug.Assert( node != null );
      Debug.Assert( node.Left != null );
      Debug.Assert( node.Right != null );

      node.Left.IsRed = !node.Left.IsRed;
      node.Right.IsRed = !node.Right.IsRed;
      node.IsRed = !node.IsRed;
    }

    private void SetRootNodeColor()
    {
      if( m_rootNode == null )
        return;

      m_rootNode.IsRed = false;
    }

    private void Clear( Node node )
    {
      if( node == null )
        return;

      this.Clear( node.Left );
      this.Clear( node.Right );
      this.ClearNode( node );
    }

    private Node RotateLeft( Node node )
    {
      Debug.Assert( node != null );

      var right = node.Right;
      Debug.Assert( right != null );

      node.Right = right.Left;
      if( node.Right != null )
      {
        node.Right.Parent = node;
      }

      var parentNode = node.Parent;

      right.Parent = parentNode;
      right.Left = node;
      right.IsRed = node.IsRed;

      node.Parent = right;
      node.IsRed = true;

      if( parentNode != null )
      {
        if( parentNode.Left == node )
        {
          parentNode.Left = right;
        }
        else
        {
          parentNode.Right = right;
        }
      }
      else
      {
        m_rootNode = right;
      }

      return right;
    }

    private Node RotateRight( Node node )
    {
      Debug.Assert( node != null );

      var left = node.Left;
      Debug.Assert( left != null );

      node.Left = left.Right;
      if( node.Left != null )
      {
        node.Left.Parent = node;
      }

      var parentNode = node.Parent;

      left.Parent = parentNode;
      left.Right = node;
      left.IsRed = node.IsRed;

      node.Parent = left;
      node.IsRed = true;

      if( parentNode != null )
      {
        if( parentNode.Left == node )
        {
          parentNode.Left = left;
        }
        else
        {
          parentNode.Right = left;
        }
      }
      else
      {
        m_rootNode = left;
      }

      return left;
    }

    private Node MoveRedLeft( Node node )
    {
      Debug.Assert( node != null );
      Debug.Assert( node.Right != null );

      this.FlipColor( node );

      if( this.IsRed( node.Right.Left ) )
      {
        node.Right = this.RotateRight( node.Right );
        node = this.RotateLeft( node );

        this.FlipColor( node );
      }

      return node;
    }

    private Node MoveRedRight( Node node )
    {
      Debug.Assert( node != null );
      Debug.Assert( node.Left != null );

      this.FlipColor( node );

      if( this.IsRed( node.Left.Left ) )
      {
        node = this.RotateRight( node );

        this.FlipColor( node );
      }

      return node;
    }

    private void FindPath( Stack<Node> path, Node upTo )
    {
      Debug.Assert( path != null );

      if( path.Count <= 0 )
        throw new ArgumentException( "The path must contain at least one element.", "path" );

      if( path.Peek() != upTo )
      {
        for( var parentNode = path.Peek().Parent; parentNode != null; parentNode = parentNode.Parent )
        {
          path.Push( parentNode );

          if( parentNode == upTo )
            break;
        }

        if( path.Peek() != upTo )
          throw new InvalidOperationException();
      }
    }

    private void CutPath( Stack<Node> path )
    {
      for( int i = 0; i < 2; i++ )
      {
        if( path.Count <= 1 )
          break;

        path.Pop();
      }
    }

    private void OnPostInsert( Node newNode, Node oldRootNode )
    {
      Debug.Assert( newNode != null );

      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, newNode.Value ) );

      if( oldRootNode != m_rootNode )
      {
        this.OnPropertyChanged( LLRBTree<T>.RootPropertyName );
      }
    }

    private void OnPostRemove( T value, Node parentNode, Node oldRootNode )
    {
      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, value ) );

      if( oldRootNode != m_rootNode )
      {
        this.OnPropertyChanged( LLRBTree<T>.RootPropertyName );
      }
    }

    private bool IsRed( Node node )
    {
      // A null node is "black".
      return ( node != null )
          && ( node.IsRed );
    }

    private void EnsureNode( IBinaryTreeNode<T> node )
    {
      if( node == null )
        throw new ArgumentNullException( "node" );

      if( !( node is Node ) )
        throw new ArgumentException( "Unexpected node type.", "node" );
    }

    private void EnsureCurrentNode( IBinaryTreeNode<T> node )
    {
      this.EnsureNode( node );

      if( node.Tree != this )
        throw new ArgumentException( "The node is not within the tree.", "node" );
    }

    #region INotifyCollectionChanged Members

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    protected virtual void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      var handler = this.CollectionChanged;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged( string propertyName )
    {
      this.OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
    }

    protected virtual void OnPropertyChanged( PropertyChangedEventArgs e )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    #region IEnumerable<> Members

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return this.GetItems( false ).GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ( ( IEnumerable<T> )this ).GetEnumerator();
    }

    #endregion

    private readonly IEqualityComparer<T> m_comparer;

    private Node m_rootNode;

    #region Node Protected Class

    protected class Node : IBinaryTreeNode<T>
    {
      public Node( LLRBTree<T> owner, T value )
      {
        Debug.Assert( owner != null );
        Debug.Assert( value != null );

        m_owner = owner;
        m_value = value;
        m_isRed = true;
      }

      public T Value
      {
        get
        {
          return m_value;
        }
      }

      public LLRBTree<T> Tree
      {
        get
        {
          return m_owner;
        }
      }

      public Node Parent
      {
        get
        {
          return m_parent;
        }
        internal set
        {
          if( value == m_parent )
            return;

          var oldNode = m_parent;

          m_parent = value;

          this.OnParentChanged( oldNode, m_parent );
        }
      }

      public Node Left
      {
        get
        {
          return m_left;
        }
        internal set
        {
          if( value == m_left )
            return;

          var oldNode = m_left;

          m_left = value;

          this.OnLeftChanged( oldNode, m_left );
        }
      }

      public Node Right
      {
        get
        {
          return m_right;
        }
        internal set
        {
          if( value == m_right )
            return;

          var oldNode = m_right;

          m_right = value;

          this.OnRightChanged( oldNode, m_right );
        }
      }

      internal bool IsRed
      {
        get
        {
          return m_isRed;
        }
        set
        {
          m_isRed = value;
        }
      }

      public Node GetPrevious()
      {
        if( m_left != null )
          return m_left.GetRightmost();

        var child = this;
        var parent = m_parent;

        while( parent != null )
        {
          if( parent.Right == child )
            return parent;

          child = parent;
          parent = parent.Parent;
        }

        return null;
      }

      public Node GetNext()
      {
        if( m_right != null )
          return m_right.GetLeftmost();

        var child = this;
        var parent = m_parent;

        while( parent != null )
        {
          if( parent.Left == child )
            return parent;

          child = parent;
          parent = parent.Parent;
        }

        return null;
      }

      public Node GetLeftmost()
      {
        var child = default( Node );

        for( var current = this; current != null; current = current.Left )
        {
          child = current;
        }

        return child;
      }

      public Node GetRightmost()
      {
        var child = default( Node );

        for( var current = this; current != null; current = current.Right )
        {
          child = current;
        }

        return child;
      }

      protected virtual void OnParentChanged( Node oldNode, Node newNode )
      {
      }

      protected virtual void OnLeftChanged( Node oldNode, Node newNode )
      {
      }

      protected virtual void OnRightChanged( Node oldNode, Node newNode )
      {
      }

      IBinaryTree<T> IBinaryTreeNode<T>.Tree
      {
        get
        {
          return this.Tree;
        }
      }

      IBinaryTreeNode<T> IBinaryTreeNode<T>.Parent
      {
        get
        {
          return this.Parent;
        }
      }

      IBinaryTreeNode<T> IBinaryTreeNode<T>.Left
      {
        get
        {
          return this.Left;
        }
      }

      IBinaryTreeNode<T> IBinaryTreeNode<T>.Right
      {
        get
        {
          return this.Right;
        }
      }

      IBinaryTreeNode<T> IBinaryTreeNode<T>.GetPrevious()
      {
        return this.GetPrevious();
      }

      IBinaryTreeNode<T> IBinaryTreeNode<T>.GetNext()
      {
        return this.GetNext();
      }

      IBinaryTreeNode<T> IBinaryTreeNode<T>.GetLeftmost()
      {
        return this.GetLeftmost();
      }

      IBinaryTreeNode<T> IBinaryTreeNode<T>.GetRightmost()
      {
        return this.GetRightmost();
      }

      private readonly T m_value;
      private readonly LLRBTree<T> m_owner;
      private Node m_parent; // null
      private Node m_left; // null
      private Node m_right; // null
      private bool m_isRed;
    }

    #endregion
  }
}
