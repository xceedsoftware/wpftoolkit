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
using System.Text;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class HashedLinkedList<T> : ICollection<T>, ICollection
  {
    #region Constructors

    public HashedLinkedList()
      : this( null )
    {
    }

    public HashedLinkedList( IEnumerable<T> collection )
    {
      if( collection == null )
      {
        m_innerLinkedList = new LinkedList<T>();
      }
      else
      {
        m_innerLinkedList = new LinkedList<T>( collection );
      }

      m_nodeDictionary = new Dictionary<T, LinkedListNode<T>>( m_innerLinkedList.Count );

      var node = m_innerLinkedList.First;

      while( node != null )
      {
        var value = node.Value;

        this.ValidateValue( value );

        m_nodeDictionary.Add( value, node );
      }
    }

    #endregion

    #region Validation

    private void ValidateNewNode( LinkedListNode<T> node )
    {
      if( node == null )
        throw new ArgumentNullException( "node" );

      this.ValidateValue( node.Value );
    }

    private void ValidateExistingNode( LinkedListNode<T> node )
    {
      if( node == null )
        throw new ArgumentNullException( "node" );

      if( node.List != m_innerLinkedList )
        throw new InvalidOperationException( "The specified LinkedListNode does belong to this LinkedList." );
    }

    private void ValidateValue( T value )
    {
      if( value == null )
        throw new InvalidOperationException( "Null values are not supported by the HashedLinkedList" );

      if( m_nodeDictionary.ContainsKey( value ) )
        throw new ArgumentException( "The HashedLinkedList already contains this value.", "value" );
    }

    #endregion

    #region Public Methods

    public LinkedListNode<T> AddAfter( LinkedListNode<T> node, T value )
    {
      this.ValidateValue( value );

      var newNode = m_innerLinkedList.AddAfter( node, value );

      m_nodeDictionary.Add( value, newNode );

      return newNode;
    }

    public void AddAfter( LinkedListNode<T> node, LinkedListNode<T> newNode )
    {
      this.ValidateNewNode( newNode );

      m_innerLinkedList.AddAfter( node, newNode );

      m_nodeDictionary.Add( newNode.Value, newNode );
    }

    public LinkedListNode<T> AddBefore( LinkedListNode<T> node, T value )
    {
      this.ValidateValue( value );

      var newNode = m_innerLinkedList.AddBefore( node, value );

      m_nodeDictionary.Add( value, newNode );

      return newNode;
    }

    public void AddBefore( LinkedListNode<T> node, LinkedListNode<T> newNode )
    {
      this.ValidateNewNode( newNode );

      m_innerLinkedList.AddBefore( node, newNode );

      m_nodeDictionary.Add( newNode.Value, newNode );
    }

    public LinkedListNode<T> AddFirst( T value )
    {
      this.ValidateValue( value );

      var newNode = m_innerLinkedList.AddFirst( value );

      m_nodeDictionary.Add( value, newNode );

      return newNode;
    }

    public void AddFirst( LinkedListNode<T> node )
    {
      this.ValidateNewNode( node );

      m_innerLinkedList.AddFirst( node );

      m_nodeDictionary.Add( node.Value, node );
    }


    public LinkedListNode<T> AddLast( T value )
    {
      this.ValidateValue( value );

      var newNode = m_innerLinkedList.AddLast( value );

      m_nodeDictionary.Add( value, newNode );

      return newNode;
    }

    public void AddLast( LinkedListNode<T> node )
    {
      this.ValidateNewNode( node );

      m_innerLinkedList.AddLast( node );

      m_nodeDictionary.Add( node.Value, node );
    }

    public void Clear()
    {
      m_innerLinkedList.Clear();
      m_nodeDictionary.Clear();
    }

    public bool Contains( T value )
    {
      return m_nodeDictionary.ContainsKey( value );
    }

    public void CopyTo( T[] array, int index )
    {
      m_innerLinkedList.CopyTo( array, index );
    }

    public LinkedListNode<T> Find( T value )
    {
      if( value == null )
        return null;

      LinkedListNode<T> node = null;

      m_nodeDictionary.TryGetValue( value, out node );

      return node;
    }

    public LinkedList<T>.Enumerator GetEnumerator()
    {
      return m_innerLinkedList.GetEnumerator();
    }


    public bool Remove( T value )
    {
      LinkedListNode<T> node = this.Find( value );

      if( node != null )
      {
        this.Remove( node );
        return true;
      }

      return false;
    }

    public void Remove( LinkedListNode<T> node )
    {
      m_innerLinkedList.Remove( node );
      m_nodeDictionary.Remove( node.Value );
    }

    public void RemoveFirst()
    {
      this.Remove( this.First );
    }

    public void RemoveLast()
    {
      this.Remove( this.Last );
    }

    #endregion

    #region Count Property

    public int Count
    {
      get
      {
        return m_innerLinkedList.Count;
      }
    }

    #endregion

    #region First Property

    public LinkedListNode<T> First
    {
      get
      {
        return m_innerLinkedList.First;
      }
    }


    #endregion

    #region Last Property

    public LinkedListNode<T> Last
    {
      get
      {
        return m_innerLinkedList.Last;
      }
    }

    #endregion

    #region Private Fields

    private LinkedList<T> m_innerLinkedList;
    private Dictionary<T, LinkedListNode<T>> m_nodeDictionary;

    #endregion


    #region ICollection<T> Members

    void ICollection<T>.Add( T value )
    {
      this.AddLast( value );
    }

    void ICollection<T>.Clear()
    {
      this.Clear();
    }

    bool ICollection<T>.Contains( T value )
    {
      return this.Contains( value );
    }

    void ICollection<T>.CopyTo( T[] array, int arrayIndex )
    {
      this.CopyTo( array, arrayIndex );
    }

    int ICollection<T>.Count
    {
      get 
      {
        return this.Count;
      }
    }

    bool ICollection<T>.IsReadOnly
    {
      get 
      {
        return false;
      }
    }

    bool ICollection<T>.Remove( T value )
    {
      return this.Remove( value );
    }

    #endregion

    #region IEnumerable<T> Members

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion

    #region ICollection Members

    void ICollection.CopyTo( Array array, int index )
    {
      ( ( ICollection )m_innerLinkedList ).CopyTo( array, index );
    }

    int ICollection.Count
    {
      get 
      {
        return this.Count;
      }
    }

    bool ICollection.IsSynchronized
    {
      get 
      {
        return false; 
      }
    }

    object ICollection.SyncRoot
    {
      get 
      {
        return ( ( ICollection )m_innerLinkedList ).SyncRoot;
      }
    }

    #endregion
  }
}
