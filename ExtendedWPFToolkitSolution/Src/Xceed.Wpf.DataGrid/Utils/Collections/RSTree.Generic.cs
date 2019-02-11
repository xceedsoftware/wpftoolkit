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
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Xceed.Utils.Collections
{
  internal class RSTree<TRegion, TItem> : ICollection<RSTree<TRegion, TItem>.Entry>, ICollection
  {
    #region Static Fields

    private const double OverflowReInsertionRatio = 0.3d;

    #endregion

    internal RSTree( RSTreeHelper<TRegion> helper )
      : this( helper, 3, 8 )
    {
    }

    internal RSTree( RSTreeHelper<TRegion> helper, int minNodeSize, int maxNodeSize )
    {
      if( helper == null )
        throw new ArgumentNullException( "helper" );

      var dimensions = helper.GetDimensions();
      if( dimensions < 1 )
        throw new ArgumentException( "The number of dimensions must be greater than or equal to one.", "helper" );

      if( minNodeSize < 2 )
        throw new ArgumentOutOfRangeException( "minNodeSize", minNodeSize, "minNodeSize must be greater than or equal to 2." );

      if( maxNodeSize < minNodeSize * 2 )
        throw new ArgumentException( "maxNodeSize must be greater than or equal to minNodeSize * 2.", "maxNodeSize" );

      m_helper = helper;
      m_minNodeSize = minNodeSize;
      m_maxNodeSize = maxNodeSize;
    }

    #region Count Property

    public int Count
    {
      get
      {
        return m_count;
      }
    }

    #endregion

    public void Add( TRegion region, TItem item )
    {
      this.Add( new Entry( region, item ) );
    }

    public bool Remove( TRegion region, TItem item )
    {
      return this.Remove( new Entry( region, item ) );
    }

    public bool Contains( TRegion region, TItem item )
    {
      return this.Contains( new Entry( region, item ) );
    }

    public bool Overlaps( TRegion region )
    {
      return this.Overlaps( m_rootNode, region );
    }

    public IEnumerable<RSTree<TRegion, TItem>.Entry> GetEntriesWithin( TRegion region )
    {
      if( ( m_rootNode == null ) || m_helper.IsEmptyRegion( region ) )
        return Enumerable.Empty<RSTree<TRegion, TItem>.Entry>();

      return new EntryEnumerable( this, region );
    }

    private static int FindLevel( Node node )
    {
      Debug.Assert( node != null );

      if( node.Parent == null )
        return 0;

      return RSTree<TRegion, TItem>.FindLevel( node.Parent ) + 1;
    }

    private void Insert( Entry entry )
    {
      Debug.Assert( !m_helper.IsEmptyRegion( entry.Region ) );

      if( m_rootNode == null )
      {
        m_rootNode = new LeafNode( m_helper, m_maxNodeSize );
      }

      var leafNode = ( LeafNode )m_rootNode.FindNearest( entry.Region );
      Debug.Assert( leafNode != null );

      leafNode.Add( entry );

      if( leafNode.Count > m_maxNodeSize )
      {
        this.ManageOverflow( leafNode );
      }
    }

    private bool Overlaps( Node node, TRegion region )
    {
      if( ( node == null ) || !m_helper.IsOverlapping( node.Region, region ) )
        return false;

      var leafNode = node as LeafNode;
      if( leafNode != null )
      {
        var count = leafNode.Count;
        for( int i = 0; i < count; i++ )
        {
          var entry = leafNode[ i ];
          if( m_helper.IsOverlapping( entry.Region, region ) )
            return true;
        }
      }
      else
      {
        var internalNode = ( InternalNode )node;
        var count = internalNode.Count;

        for( int i = 0; i < count; i++ )
        {
          if( this.Overlaps( internalNode[ i ], region ) )
            return true;
        }
      }

      return false;
    }

    private void Insert( Node node, int level )
    {
      Debug.Assert( ( node != null ) && ( level > 0 ) );
      Debug.Assert( m_rootNode != null );

      var internalNode = ( InternalNode )m_rootNode.FindNearest( node.Region, level - 1 );
      Debug.Assert( internalNode != null );

      internalNode.Add( node );

      if( internalNode.Count > m_maxNodeSize )
      {
        this.ManageOverflow( internalNode );
      }
    }

    private IEnumerable<Node> FindNodes( TRegion region, int level )
    {
      Debug.Assert( !m_helper.IsEmptyRegion( region ) );

      if( m_rootNode == null )
        yield break;

      var nodeInfos = new Queue<NodeInfo>();
      nodeInfos.Enqueue( new NodeInfo( m_rootNode, 0 ) );

      while( nodeInfos.Count > 0 )
      {
        var nodeInfo = nodeInfos.Dequeue();
        var node = nodeInfo.Node;
        var nodeLevel = nodeInfo.Level;

        if( !m_helper.IsOverlapping( node.Region, region ) )
          continue;

        if( nodeLevel == level )
        {
          yield return node;
        }
        else if( ( level < 0 ) || ( nodeLevel < level ) )
        {
          var leafNode = node as LeafNode;
          if( leafNode != null )
          {
            if( level < 0 )
              yield return leafNode;
          }
          else
          {
            foreach( var child in ( InternalNode )node )
            {
              nodeInfos.Enqueue( new NodeInfo( child, nodeLevel + 1 ) );
            }
          }
        }
      }
    }

    private void ManageOverflow( Node node )
    {
      while( node != null )
      {
        node = ( node is LeafNode )
                 ? this.ManageOverflowCore( ( LeafNode )node )
                 : this.ManageOverflowCore( ( InternalNode )node );
      }
    }

    private Node ManageOverflowCore( LeafNode node )
    {
      Debug.Assert( node.Count > m_maxNodeSize );

      // It makes no sense to reinsert the entry in the root node.
      if( node.Parent != null )
      {
        node = this.Reinsert( node );
        if( node == null )
          return null;
      }

      // We have no choice but to split the node.
      var newNode = node.Split( m_minNodeSize );
      Debug.Assert( newNode != null );

      // We have splited the root node.  A new root is required.
      if( node == m_rootNode )
      {
        var newRootNode = new InternalNode( m_helper, m_maxNodeSize );
        newRootNode.Add( node );
        newRootNode.Add( newNode );

        m_rootNode = newRootNode;
      }
      // Add the new node to the same parent node.
      else
      {
        var parentNode = ( InternalNode )node.Parent;
        Debug.Assert( parentNode != null );

        parentNode.Add( newNode );

        // Return the parent node if it is overflowing to process it.
        if( parentNode.Count > m_maxNodeSize )
          return parentNode;
      }

      return null;
    }

    private Node ManageOverflowCore( InternalNode node )
    {
      Debug.Assert( node.Count > m_maxNodeSize );

      // It makes no sense to reinsert child nodes in the root node.
      if( node.Parent != null )
      {
        node = this.Reinsert( node );
        if( node == null )
          return null;
      }

      // We have no choice but to split the node.
      var newNode = node.Split( m_minNodeSize );
      Debug.Assert( newNode != null );

      // We have splited the root node.  A new root is required.
      if( node == m_rootNode )
      {
        var newRootNode = new InternalNode( m_helper, m_maxNodeSize );
        newRootNode.Add( node );
        newRootNode.Add( newNode );

        m_rootNode = newRootNode;
      }
      // Add the new node to the same parent node.
      else
      {
        var parentNode = ( InternalNode )node.Parent;
        Debug.Assert( parentNode != null );

        parentNode.Add( newNode );

        // Return the parent node if it is overflowing to process it.
        if( parentNode.Count > m_maxNodeSize )
          return parentNode;
      }

      return null;
    }

    private LeafNode Reinsert( LeafNode node )
    {
      Debug.Assert( node != null );
      Debug.Assert( node.Count > m_maxNodeSize );

      var reinsertCount = System.Math.Min( node.Count - m_minNodeSize, ( int )System.Math.Ceiling( m_maxNodeSize * RSTree<TRegion, TItem>.OverflowReInsertionRatio ) );
      if( reinsertCount <= 0 )
        return node;

      Debug.Assert( ( m_minNodeSize <= node.Count - reinsertCount ) && ( reinsertCount <= m_maxNodeSize ) );

      // Identify the entries that are at the "edge" of current region.
      var center = new KPoint( m_helper.CenterOf( node.Region ) );
      var candidates = node.Select( ( entry, entryIndex ) => new
                             {
                               Entry = entry,
                               Index = entryIndex,
                               Distance = KPoint.Distance( center, new KPoint( m_helper.CenterOf( entry.Region ) ) )
                             } )
                           .OrderByDescending( item => item.Distance )
                           .ThenBy( item => item.Index )
                           .Take( reinsertCount ).ToList();

      // The entries must be removed from last to first to prevent an index shift.
      foreach( var item in candidates.OrderByDescending( x => x.Index ) )
      {
        node.RemoveAt( item.Index );
      }

      var splitNode = node;
      var reinsert = 0;

      while( reinsert < candidates.Count )
      {
        var entry = candidates[ reinsert ].Entry;
        var newLeafNode = ( LeafNode )m_rootNode.FindNearest( entry.Region );
        Debug.Assert( newLeafNode != null );

        newLeafNode.Add( entry );
        reinsert++;

        // If a new overflow occur while reinserting entries, stop reinserting in order to split the node.
        if( newLeafNode.Count > m_maxNodeSize )
        {
          splitNode = newLeafNode;
          break;
        }
        // If the best leaf node for this entry is the original node, so does the remaining entries.
        else if( newLeafNode == node )
        {
          break;
        }
      }

      Debug.Assert( ( ( candidates.Count - reinsert ) + node.Count <= m_maxNodeSize )
                 || ( ( ( candidates.Count - reinsert ) + node.Count > m_maxNodeSize ) && ( splitNode == node ) ) );

      // Reinsert the remaining entries in the original node.
      for( ; reinsert < candidates.Count; reinsert++ )
      {
        node.Add( candidates[ reinsert ].Entry );
      }

      if( splitNode.Count > m_maxNodeSize )
        return splitNode;

      return null;
    }

    private InternalNode Reinsert( InternalNode node )
    {
      Debug.Assert( node != null );
      Debug.Assert( node.Count > m_maxNodeSize );

      var reinsertCount = System.Math.Min( node.Count - m_minNodeSize, ( int )System.Math.Ceiling( m_maxNodeSize * RSTree<TRegion, TItem>.OverflowReInsertionRatio ) );
      if( reinsertCount <= 0 )
        return node;

      Debug.Assert( ( m_minNodeSize <= node.Count - reinsertCount ) && ( reinsertCount <= m_maxNodeSize ) );

      // Identify the child nodes that are at the "edge" of current region.
      var center = new KPoint( m_helper.CenterOf( node.Region ) );
      var candidates = node.Select( ( childNode, childNodeIndex ) => new
                             {
                               Node = childNode,
                               Index = childNodeIndex,
                               Distance = KPoint.Distance( center, new KPoint( m_helper.CenterOf( childNode.Region ) ) )
                             } )
                           .OrderByDescending( item => item.Distance )
                           .ThenBy( item => item.Index )
                           .Take( reinsertCount ).ToList();

      // The child nodes must be removed from last to first to prevent an index shift.
      foreach( var item in candidates.OrderByDescending( x => x.Index ) )
      {
        node.RemoveAt( item.Index );
      }

      var splitNode = node;
      var nodeLevel = RSTree<TRegion, TItem>.FindLevel( node );
      var reinsert = 0;

      while( reinsert < candidates.Count )
      {
        var childNode = candidates[ reinsert ].Node;
        var newInternalNode = ( InternalNode )m_rootNode.FindNearest( childNode.Region, nodeLevel );
        Debug.Assert( newInternalNode != null );

        newInternalNode.Add( childNode );
        reinsert++;

        // If a new overflow occur while reinserting child nodes, stop reinserting in order to split the node.
        if( newInternalNode.Count > m_maxNodeSize )
        {
          splitNode = newInternalNode;
          break;
        }
        // If the best internal node for this child node is the original node, so does the remaining child nodes.
        else if( newInternalNode == node )
        {
          break;
        }
      }

      Debug.Assert( ( ( candidates.Count - reinsert ) + node.Count <= m_maxNodeSize )
                 || ( ( ( candidates.Count - reinsert ) + node.Count > m_maxNodeSize ) && ( splitNode == node ) ) );

      // Reinsert the remaining child nodes in the original node.
      for( ; reinsert < candidates.Count; reinsert++ )
      {
        node.Add( candidates[ reinsert ].Node );
      }

      if( splitNode.Count > m_maxNodeSize )
        return splitNode;

      return null;
    }

    private void ManageUnderflow( Node node )
    {
      while( node != null )
      {
        node = ( node is LeafNode )
                 ? this.ManageUnderflowCore( ( LeafNode )node )
                 : this.ManageUnderflowCore( ( InternalNode )node );
      }
    }

    private Node ManageUnderflowCore( LeafNode node )
    {
      Debug.Assert( node.Count < m_minNodeSize );

      if( node == m_rootNode )
      {
        if( node.Count == 0 )
        {
          m_rootNode = null;
        }
      }
      else
      {
        var parent = ( InternalNode )node.Parent;
        Debug.Assert( parent != null );

        // Remove the current node from the parent node to prevent entries to
        // be reinserted within it.
        var removed = parent.Remove( node );
        Debug.Assert( removed );

        // Reinsert the entries in the remaining nodes.
        for( int i = node.Count - 1; i >= 0; i-- )
        {
          var entry = node[ i ];
          node.RemoveAt( i );

          this.Insert( entry );
        }

        // Return the parent node if it is underflowing to process it.
        if( parent.Count < m_minNodeSize )
          return parent;
      }

      return null;
    }

    private Node ManageUnderflowCore( InternalNode node )
    {
      Debug.Assert( node.Count < m_minNodeSize );

      if( node == m_rootNode )
      {
        Debug.Assert( node.Count > 0 );

        // Remove an entire level.
        if( node.Count == 1 )
        {
          var child = node[ 0 ];
          node.RemoveAt( 0 );

          m_rootNode = child;
        }
      }
      else
      {
        var level = RSTree<TRegion, TItem>.FindLevel( node );
        var parent = ( InternalNode )node.Parent;
        Debug.Assert( ( parent != null ) && ( level > 0 ) );

        // Remove the current node from the parent node to prevent child nodes to
        // be reinserted within it.
        var removed = parent.Remove( node );
        Debug.Assert( removed );

        // Reinsert the child nodes in the remaining nodes of the same level.
        for( int i = node.Count - 1; i >= 0; i-- )
        {
          var child = node[ i ];
          node.RemoveAt( i );

          this.Insert( child, level + 1 );
        }

        // Return the parent node if it is underflowing to process it.
        if( parent.Count < m_minNodeSize )
          return parent;
      }

      return null;
    }

    private void IncrementVersion()
    {
      unchecked
      {
        m_version++;
      }
    }

    #region ICollection<> Members

    int ICollection<RSTree<TRegion, TItem>.Entry>.Count
    {
      get
      {
        return this.Count;
      }
    }

    bool ICollection<RSTree<TRegion, TItem>.Entry>.IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public void Add( Entry entry )
    {
      if( m_helper.IsEmptyRegion( entry.Region ) )
        throw new ArgumentException( "The region must not be empty.", "entry" );

      this.Insert( entry );

      m_count++;
      this.IncrementVersion();
    }

    public void Clear()
    {
      m_count = 0;
      m_rootNode = null;

      this.IncrementVersion();
    }

    public bool Contains( Entry entry )
    {
      if( ( m_rootNode == null ) || m_helper.IsEmptyRegion( entry.Region ) )
        return false;

      return ( from candidate in this.GetEntriesWithin( entry.Region )
               where ( m_helper.AreEquivalent( entry.Region, candidate.Region ) )
                  && ( object.Equals( entry.Item, candidate.Item ) )
               select candidate ).Any();
    }

    public bool Remove( Entry entry )
    {
      if( ( m_rootNode == null ) || m_helper.IsEmptyRegion( entry.Region ) )
        return false;

      // Get the first LeafNode where the entry was found.
      var leafNode = ( from candidate in this.FindNodes( entry.Region, -1 ).Cast<LeafNode>()
                       where candidate.Remove( entry )
                       select candidate ).FirstOrDefault();
      if( leafNode == null )
        return false;

      if( leafNode.Count < m_minNodeSize )
      {
        this.ManageUnderflow( leafNode );
      }

      m_count--;
      this.IncrementVersion();

      return true;
    }

    void ICollection<RSTree<TRegion, TItem>.Entry>.CopyTo( Entry[] array, int index )
    {
      ( ( ICollection )this ).CopyTo( array, index );
    }

    #endregion

    #region ICollection Members

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
        if( m_syncRoot == null )
        {
          Interlocked.CompareExchange( ref m_syncRoot, new object(), null );
        }

        return m_syncRoot;
      }
    }

    void ICollection.CopyTo( Array array, int index )
    {
      if( array == null )
        throw new ArgumentNullException( "array" );

      if( index < 0 )
        throw new ArgumentOutOfRangeException( "index" );

      if( array.Rank != 1 )
        throw new ArgumentException( "Multi-dimensional array is not supported.", "array" );

      if( ( array.Length - index ) < this.Count )
        throw new ArgumentException( "The number of elements is greater than the available space from index to the end of the destination array.", "array" );

      foreach( var entry in this )
      {
        array.SetValue( entry, index++ );
      }
    }

    #endregion

    #region IEnumerable<> Members

    public IEnumerator<Entry> GetEnumerator()
    {
      return new EntryEnumerator( this );
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion

    private readonly RSTreeHelper<TRegion> m_helper;
    private readonly int m_minNodeSize;
    private readonly int m_maxNodeSize;

    private Node m_rootNode; //null
    private int m_count; //0
    private int m_version; //0
    private object m_syncRoot; //null

    #region Entry Internal Struct

    internal struct Entry
    {
      public Entry( TRegion region, TItem item )
      {
        m_region = region;
        m_item = item;
      }

      public TRegion Region
      {
        get
        {
          return m_region;
        }
      }

      public TItem Item
      {
        get
        {
          return m_item;
        }
      }

      public override int GetHashCode()
      {
        return ( !object.ReferenceEquals( m_item, null ) ) ? m_item.GetHashCode() : 0;
      }

      public override bool Equals( object obj )
      {
        if( !( obj is Entry ) )
          return false;

        var target = ( Entry )obj;

        return ( object.Equals( target.m_region, m_region ) )
            && ( object.Equals( target.m_item, m_item ) );
      }

      private readonly TRegion m_region;
      private readonly TItem m_item;
    }

    #endregion

    #region EntryEnumerable Private Class

    private sealed class EntryEnumerable : IEnumerable<RSTree<TRegion, TItem>.Entry>
    {
      internal EntryEnumerable( RSTree<TRegion, TItem> owner, TRegion region )
      {
        Debug.Assert( owner != null );

        m_owner = owner;
        m_region = region;
      }

      public IEnumerator<RSTree<TRegion, TItem>.Entry> GetEnumerator()
      {
        return new EntryEnumerator( m_owner, m_region );
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return this.GetEnumerator();
      }

      private readonly RSTree<TRegion, TItem> m_owner;
      private readonly TRegion m_region;
    }

    #endregion

    #region EntryEnumerator Private Class

    private sealed class EntryEnumerator : IEnumerator<RSTree<TRegion, TItem>.Entry>
    {
      internal EntryEnumerator( RSTree<TRegion, TItem> owner, TRegion region )
        : this( owner )
      {
        Debug.Assert( !owner.m_helper.IsEmptyRegion( region ) );

        m_filter = ( TRegion r ) => m_owner.m_helper.IsOverlapping( region, r );
      }

      internal EntryEnumerator( RSTree<TRegion, TItem> owner )
      {
        Debug.Assert( owner != null );

        m_owner = owner;
        m_version = owner.m_version;

        if( owner.m_rootNode != null )
        {
          m_nodes.Enqueue( owner.m_rootNode );
        }
      }

      public Entry Current
      {
        get
        {
          if( m_entries.Count == 0 )
            throw new InvalidOperationException();

          return m_entries.Peek();
        }
      }

      object IEnumerator.Current
      {
        get
        {
          return this.Current;
        }
      }

      public bool MoveNext()
      {
        this.CheckVersion();

        if( m_entries.Count > 0 )
        {
          m_entries.Dequeue();

          if( m_entries.Count > 0 )
            return true;
        }

        Debug.Assert( m_entries.Count == 0 );

        while( m_nodes.Count > 0 )
        {
          var node = m_nodes.Dequeue();
          if( ( m_filter != null ) && !m_filter.Invoke( node.Region ) )
            continue;

          var leafNode = node as LeafNode;
          if( leafNode != null )
          {
            var count = leafNode.Count;

            for( int i = 0; i < count; i++ )
            {
              var entry = leafNode[ i ];
              if( ( m_filter != null ) && !m_filter.Invoke( entry.Region ) )
                continue;

              m_entries.Enqueue( entry );
            }

            if( m_entries.Count > 0 )
              return true;
          }
          else
          {
            var internalNode = ( InternalNode )node;
            var count = internalNode.Count;

            for( int i = 0; i < count; i++ )
            {
              m_nodes.Enqueue( internalNode[ i ] );
            }
          }
        }

        Debug.Assert( m_nodes.Count == 0 );
        Debug.Assert( m_entries.Count == 0 );

        return false;
      }

      public void Reset()
      {
        this.CheckVersion();

        m_entries.Clear();
        m_nodes.Clear();

        if( m_owner.m_rootNode != null )
        {
          m_nodes.Enqueue( m_owner.m_rootNode );
        }
      }

      void IDisposable.Dispose()
      {
      }

      private void CheckVersion()
      {
        if( m_version != m_owner.m_version )
          throw new InvalidOperationException();
      }

      private readonly RSTree<TRegion, TItem> m_owner;
      private readonly Queue<Node> m_nodes = new Queue<Node>();
      private readonly Queue<Entry> m_entries = new Queue<Entry>();
      private readonly Func<TRegion, bool> m_filter; //null
      private int m_version;
    }

    #endregion

    #region Node Private Class

    protected abstract class Node : ICollection
    {
      public abstract int Count
      {
        get;
      }

      internal abstract TRegion Region
      {
        get;
      }

      internal Node Parent
      {
        get
        {
          return m_parent;
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
          throw new NotSupportedException();
        }
      }

      protected abstract void CopyToCore( Array array, int index );
      protected abstract IEnumerator GetEnumeratorCore();

      protected void SetParent( Node child, Node parent )
      {
        if( ( child == null ) || ( child.m_parent == parent ) )
          return;

        if( child.m_parent != null )
        {
          child.m_parent.SliceRegion( child.Region );
        }

        child.m_parent = parent;

        if( parent != null )
        {
          parent.MergeRegion( child.Region );
        }
      }

      protected internal abstract void MergeRegion( TRegion region );
      protected internal abstract void SliceRegion( TRegion region );
      protected internal abstract void InvalidateRegion();

      internal abstract Node FindNearest( TRegion region, int level );

      internal Node FindNearest( TRegion region )
      {
        return this.FindNearest( region, -1 );
      }

      internal abstract Node Split( int minNodeSize );

      void ICollection.CopyTo( Array array, int index )
      {
        this.CopyToCore( array, index );
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return this.GetEnumeratorCore();
      }

      private Node m_parent;
    }

    #endregion

    #region InternalNode Private Class

    [DebuggerDisplay( "Internal (Count = {Count})" )]
    private sealed class InternalNode : Node, IList<Node>
    {
      internal InternalNode( RSTreeHelper<TRegion> helper, int size )
      {
        Debug.Assert( helper != null );
        Debug.Assert( size > 0 );

        m_helper = helper;
        m_nodes = new Node[ size + 1 ]; // We reserve one extra space for overflow.
      }

      public override int Count
      {
        get
        {
          return m_count;
        }
      }

      internal override TRegion Region
      {
        get
        {
          if( !m_regionValid )
          {
            m_region = m_helper.GetEmptyRegion();

            for( int i = 0; i < m_count; i++ )
            {
              m_region = m_helper.Union( m_region, m_nodes[ i ].Region );
            }

            m_regionValid = true;
          }

          return m_region;
        }
      }

      public Node this[ int index ]
      {
        get
        {
          if( ( index < 0 ) || ( index >= m_count ) )
            throw new ArgumentOutOfRangeException( "index" );

          return m_nodes[ index ];
        }
        set
        {
          this.SetAt( index, value );
        }
      }

      bool ICollection<Node>.IsReadOnly
      {
        get
        {
          return false;
        }
      }

      public void Add( Node node )
      {
        this.InsertAt( m_count, node );
      }

      public void Insert( int index, Node node )
      {
        this.InsertAt( index, node );
      }

      public void RemoveAt( int index )
      {
        if( ( index < 0 ) || ( index >= m_count ) )
          throw new ArgumentOutOfRangeException( "index" );

        var node = m_nodes[ index ];

        for( int i = index; i < m_count - 1; i++ )
        {
          m_nodes[ i ] = m_nodes[ i + 1 ];
        }

        m_nodes[ m_count - 1 ] = null;
        m_count--;

        this.SetParent( node, null );
      }

      public bool Remove( Node node )
      {
        var index = this.IndexOf( node );
        if( index < 0 )
          return false;

        this.RemoveAt( index );
        return true;
      }

      public void Clear()
      {
        var count = m_count;
        var nodes = new Node[ m_nodes.Length ];

        Array.Copy( m_nodes, 0, nodes, 0, m_count );
        Array.Clear( m_nodes, 0, m_count );
        m_count = 0;
        m_regionValid = false;

        for( int i = 0; i < count; i++ )
        {
          this.SetParent( nodes[ i ], null );
        }
      }

      public int IndexOf( Node node )
      {
        if( node == null )
          return -1;

        return Array.IndexOf( m_nodes, node, 0, m_count );
      }

      public bool Contains( Node node )
      {
        return ( this.IndexOf( node ) >= 0 );
      }

      public IEnumerator<Node> GetEnumerator()
      {
        for( int i = 0; i < m_count; i++ )
        {
          yield return m_nodes[ i ];
        }
      }

      protected override void CopyToCore( Array array, int index )
      {
        Array.Copy( m_nodes, 0, array, index, m_count );
      }

      protected override IEnumerator GetEnumeratorCore()
      {
        return this.GetEnumerator();
      }

      protected internal override void MergeRegion( TRegion region )
      {
        if( !m_regionValid )
          return;

        var newRegion = m_helper.Union( m_region, region );
        if( m_helper.AreEquivalent( m_region, newRegion ) )
          return;

        m_region = newRegion;
        this.InvalidateParentRegion();
      }

      protected internal override void SliceRegion( TRegion region )
      {
        if( !m_regionValid || m_helper.IsWithinBounds( m_region, region ) )
          return;

        m_regionValid = false;
        this.InvalidateParentRegion();
      }

      protected internal override void InvalidateRegion()
      {
        if( !m_regionValid )
          return;

        m_regionValid = false;
        this.InvalidateParentRegion();
      }

      internal override Node FindNearest( TRegion region, int level )
      {
        if( m_count <= 0 )
          throw new InvalidOperationException( "An internal node must have children." );

        this.CheckChildNodesOfSameType();

        if( level == 0 )
          return this;

        var childNode = ( m_nodes[ 0 ] is LeafNode )
                          ? this.FindLeastOverlapEnlargement( m_nodes, m_count, region )
                          : this.FindLeastAreaEnlargement( m_nodes, m_count, region );

        Debug.Assert( childNode != null );

        return childNode.FindNearest( region, ( level < 0 ) ? level : level - 1 );
      }

      internal override Node Split( int minNodeSize )
      {
        Debug.Assert( m_count == m_nodes.Length );

        var nodes = ( Node[] )m_nodes.Clone();
        var axis = this.FindSplitAxis( nodes, minNodeSize );
        var index = this.FindSplitIndex( nodes, minNodeSize, axis );

        this.Clear();

        for( int i = 0; i < index; i++ )
        {
          this.Add( nodes[ i ] );
        }

        var newNode = new InternalNode( m_helper, m_nodes.Length - 1 );
        Debug.Assert( newNode.m_nodes.Length == m_nodes.Length );

        for( int i = index; i < nodes.Length; i++ )
        {
          newNode.Add( nodes[ i ] );
        }

        return newNode;
      }

      [Conditional( "DEBUG" )]
      private void CheckChildNodesOfSameType()
      {
        if( m_count <= 0 )
          return;

        var isLeaf = m_nodes[ 0 ] is LeafNode;

        for( int i = 1; i < m_count; i++ )
        {
          Debug.Assert( ( m_nodes[ i ] is LeafNode ) == isLeaf );
        }
      }

      private void SetAt( int index, Node node )
      {
        if( node == null )
          throw new ArgumentNullException( "node" );

        if( ( index < 0 ) || ( index >= m_count ) )
          throw new ArgumentOutOfRangeException( "index" );

        if( m_count >= m_nodes.Length )
          throw new InvalidOperationException();

        var oldNode = m_nodes[ index ];
        m_nodes[ index ] = node;

        this.SetParent( node, this );
        this.SetParent( oldNode, null );
      }

      private void InsertAt( int index, Node node )
      {
        if( node == null )
          throw new ArgumentNullException( "node" );

        if( ( index < 0 ) || ( index > m_count ) )
          throw new ArgumentOutOfRangeException( "index" );

        if( m_count >= m_nodes.Length )
          throw new InvalidOperationException();

        for( int i = m_count; i > index; i-- )
        {
          m_nodes[ i ] = m_nodes[ i - 1 ];
        }

        m_nodes[ index ] = node;
        m_count++;

        this.SetParent( node, this );
      }

      private void InvalidateParentRegion()
      {
        var parent = this.Parent;
        if( parent == null )
          return;

        parent.InvalidateRegion();
      }

      private Node FindLeastOverlapEnlargement( IList<Node> nodes, int count, TRegion region )
      {
        Debug.Assert( ( nodes != null ) && ( nodes.Count > 0 ) );
        Debug.Assert( nodes.Count >= count );

        if( count == 1 )
          return nodes[ 0 ];

        var candidates = new List<Node>( 1 );
        var minimum = double.PositiveInfinity;

        for( int i = 0; i < count; i++ )
        {
          var candidate = nodes[ i ];
          var newRegion = m_helper.Union( candidate.Region, region );
          var value = 0d;

          for( int j = 0; j < count; j++ )
          {
            if( i == j )
              continue;

            value += m_helper.CalculateOverlapEnlargement( newRegion, nodes[ j ].Region );
          }

          if( value < minimum )
          {
            candidates.Clear();
            candidates.Add( candidate );
            minimum = value;
          }
          else if( value == minimum )
          {
            candidates.Add( candidate );
          }
        }

        if( candidates.Count > 1 )
          return this.FindLeastAreaEnlargement( candidates, candidates.Count, region );

        return candidates[ 0 ];
      }

      private Node FindLeastAreaEnlargement( IList<Node> nodes, int count, TRegion region )
      {
        Debug.Assert( ( nodes != null ) && ( nodes.Count > 0 ) );
        Debug.Assert( nodes.Count >= count );

        if( count == 1 )
          return nodes[ 0 ];

        var candidates = new List<Node>( 1 );
        var minimum = double.PositiveInfinity;

        for( int i = 0; i < count; i++ )
        {
          var candidate = nodes[ i ];
          var value = m_helper.CalculateAreaEnlargement( candidate.Region, region );

          if( value < minimum )
          {
            candidates.Clear();
            candidates.Add( candidate );
            minimum = value;
          }
          else if( value == minimum )
          {
            candidates.Add( candidate );
          }
        }

        if( candidates.Count > 1 )
          return this.FindSmallestArea( candidates, candidates.Count );

        return candidates[ 0 ];
      }

      private Node FindSmallestArea( IList<Node> nodes, int count )
      {
        Debug.Assert( ( nodes != null ) && ( nodes.Count > 0 ) );
        Debug.Assert( nodes.Count >= count );

        if( count == 1 )
          return nodes[ 0 ];

        var index = 0;
        var minimum = double.PositiveInfinity;

        for( int i = 0; i < count; i++ )
        {
          var value = m_helper.CalculateArea( nodes[ i ].Region );
          if( value < minimum )
          {
            index = i;
            minimum = value;
          }
        }

        return nodes[ index ];
      }

      private int FindSplitAxis( Node[] nodes, int minNodeSize )
      {
        Debug.Assert( ( nodes != null ) && ( nodes.Length == m_count ) );

        var axis = 0;
        var minimum = double.PositiveInfinity;
        var dimensions = m_helper.GetDimensions();

        for( int i = 0; i < dimensions; i++ )
        {
          Array.Sort( nodes, new LowerBoundComparer( m_helper, i ) );

          var value = this.CalculateMargin( nodes, minNodeSize );
          if( value < minimum )
          {
            minimum = value;
            axis = i;
          }

          Array.Sort( nodes, new UpperBoundComparer( m_helper, i ) );

          value = this.CalculateMargin( nodes, minNodeSize );
          if( value < minimum )
          {
            minimum = value;
            axis = i;
          }
        }

        return axis;
      }

      private int FindSplitIndex( Node[] nodes, int minNodeSize, int dimension )
      {
        Array.Sort( nodes, new LowerBoundComparer( m_helper, dimension ) );

        var lowerOverlap = double.PositiveInfinity;
        var lowerArea = double.PositiveInfinity;
        var lowerIndex = this.FindSplitIndex( nodes, minNodeSize, out lowerOverlap, out lowerArea );

        Array.Sort( nodes, new UpperBoundComparer( m_helper, dimension ) );

        var upperOverlap = double.PositiveInfinity;
        var upperArea = double.PositiveInfinity;
        var upperIndex = this.FindSplitIndex( nodes, minNodeSize, out upperOverlap, out upperArea );

        if( ( upperOverlap < lowerOverlap ) || ( ( upperOverlap == lowerOverlap ) && ( upperArea < lowerArea ) ) )
          return upperIndex;

        Array.Sort( nodes, new LowerBoundComparer( m_helper, dimension ) );
        return lowerIndex;
      }

      private int FindSplitIndex( Node[] nodes, int minSize, out double overlap, out double area )
      {
        var splitIndex = 0;

        overlap = double.PositiveInfinity;
        area = double.PositiveInfinity;

        var maxSize = nodes.Length - minSize;

        for( int i = minSize; i < maxSize; i++ )
        {
          var firstRegion = m_helper.GetEmptyRegion();
          for( int j = 0; j < i; j++ )
          {
            firstRegion = m_helper.Union( firstRegion, nodes[ j ].Region );
          }

          var secondRegion = m_helper.GetEmptyRegion();
          for( int j = i; j < nodes.Length; j++ )
          {
            secondRegion = m_helper.Union( secondRegion, nodes[ j ].Region );
          }

          var currentOverlap = m_helper.CalculateArea( m_helper.Intersect( firstRegion, secondRegion ) );
          var currentArea = m_helper.CalculateArea( firstRegion ) + m_helper.CalculateArea( secondRegion );

          if( ( currentOverlap < overlap ) || ( ( currentOverlap == overlap ) && ( currentArea < area ) ) )
          {
            overlap = currentOverlap;
            area = currentArea;
            splitIndex = i;
          }
        }

        return splitIndex;
      }

      private double CalculateMargin( IList<Node> nodes, int minSize )
      {
        var maxSize = nodes.Count - minSize;
        var margin = 0d;

        for( int i = minSize; i < maxSize; i++ )
        {
          var region = m_helper.GetEmptyRegion();
          for( int j = 0; j < i; j++ )
          {
            region = m_helper.Union( region, nodes[ j ].Region );
          }

          margin += m_helper.CalculateMargin( region );

          region = m_helper.GetEmptyRegion();
          for( int j = i; j < nodes.Count; j++ )
          {
            region = m_helper.Union( region, nodes[ j ].Region );
          }

          margin += m_helper.CalculateMargin( region );
        }

        return margin;
      }

      void ICollection<Node>.CopyTo( Node[] array, int index )
      {
        this.CopyToCore( array, index );
      }

      private readonly RSTreeHelper<TRegion> m_helper;
      private readonly Node[] m_nodes;
      private int m_count;
      private TRegion m_region;
      private bool m_regionValid; //false
    }

    #endregion

    #region LeafNode Private Class

    [DebuggerDisplay( "Leaf (Count = {Count})" )]
    private sealed class LeafNode : Node, IList<Entry>
    {
      internal LeafNode( RSTreeHelper<TRegion> helper, int size )
      {
        Debug.Assert( helper != null );
        Debug.Assert( size > 0 );

        m_helper = helper;
        m_values = new Entry[ size + 1 ]; // We reserve one extra space for overflow.
      }

      public override int Count
      {
        get
        {
          return m_count;
        }
      }

      internal override TRegion Region
      {
        get
        {
          if( !m_regionValid )
          {
            m_region = m_helper.GetEmptyRegion();

            for( int i = 0; i < m_count; i++ )
            {
              m_region = m_helper.Union( m_region, m_values[ i ].Region );
            }

            m_regionValid = true;
          }

          return m_region;
        }
      }

      public Entry this[ int index ]
      {
        get
        {
          if( ( index < 0 ) || ( index >= m_count ) )
            throw new ArgumentOutOfRangeException( "index" );

          return m_values[ index ];
        }
        set
        {
          this.SetAt( index, value );
        }
      }

      bool ICollection<Entry>.IsReadOnly
      {
        get
        {
          return false;
        }
      }

      public void Add( Entry value )
      {
        this.InsertAt( m_count, value );
      }

      public void Insert( int index, Entry value )
      {
        this.InsertAt( index, value );
      }

      public void RemoveAt( int index )
      {
        if( ( index < 0 ) || ( index >= m_count ) )
          throw new ArgumentOutOfRangeException( "index" );

        var value = m_values[ index ];

        for( int i = index; i < m_count - 1; i++ )
        {
          m_values[ i ] = m_values[ i + 1 ];
        }

        m_values[ m_count - 1 ] = default( Entry );
        m_count--;

        this.PostRemove( value );
      }

      public bool Remove( Entry value )
      {
        var index = this.IndexOf( value );
        if( index < 0 )
          return false;

        this.RemoveAt( index );
        return true;
      }

      public void Clear()
      {
        Array.Clear( m_values, 0, m_count );
        m_count = 0;
        m_regionValid = false;
      }

      public int IndexOf( Entry value )
      {
        return Array.IndexOf( m_values, value, 0, m_count );
      }

      public bool Contains( Entry value )
      {
        return ( this.IndexOf( value ) >= 0 );
      }

      public IEnumerator<Entry> GetEnumerator()
      {
        for( int i = 0; i < m_count; i++ )
        {
          yield return m_values[ i ];
        }
      }

      protected override void CopyToCore( Array array, int index )
      {
        Array.Copy( m_values, 0, array, index, m_count );
      }

      protected override IEnumerator GetEnumeratorCore()
      {
        return this.GetEnumerator();
      }

      protected internal override void MergeRegion( TRegion region )
      {
        if( !m_regionValid )
          return;

        var newRegion = m_helper.Union( m_region, region );
        if( m_helper.AreEquivalent( m_region, newRegion ) )
          return;

        m_region = newRegion;
        this.InvalidateParentRegion();
      }

      protected internal override void SliceRegion( TRegion region )
      {
        if( !m_regionValid || m_helper.IsWithinBounds( m_region, region ) )
          return;

        m_regionValid = false;
        this.InvalidateParentRegion();
      }

      protected internal override void InvalidateRegion()
      {
        if( !m_regionValid )
          return;

        m_regionValid = false;
        this.InvalidateParentRegion();
      }

      internal override Node FindNearest( TRegion region, int level )
      {
        return this;
      }

      internal override Node Split( int minNodeSize )
      {
        Debug.Assert( m_count == m_values.Length );

        var entries = ( Entry[] )m_values.Clone();
        var axis = this.FindSplitAxis( entries, minNodeSize );
        var index = this.FindSplitIndex( entries, minNodeSize, axis );

        this.Clear();

        for( int i = 0; i < index; i++ )
        {
          this.Add( entries[ i ] );
        }

        var newNode = new LeafNode( m_helper, m_values.Length - 1 );
        Debug.Assert( newNode.m_values.Length == m_values.Length );

        for( int i = index; i < entries.Length; i++ )
        {
          newNode.Add( entries[ i ] );
        }

        return newNode;
      }

      private void SetAt( int index, Entry value )
      {
        if( ( index < 0 ) || ( index >= m_count ) )
          throw new ArgumentOutOfRangeException( "index" );

        if( m_count >= m_values.Length )
          throw new InvalidOperationException();

        var oldValue = m_values[ index ];
        m_values[ index ] = value;

        this.PostAdd( value );
        this.PostRemove( oldValue );
      }

      private void InsertAt( int index, Entry value )
      {
        if( ( index < 0 ) || ( index > m_count ) )
          throw new ArgumentOutOfRangeException( "index" );

        if( m_count >= m_values.Length )
          throw new InvalidOperationException();

        for( int i = m_count; i > index; i-- )
        {
          m_values[ i ] = m_values[ i - 1 ];
        }

        m_values[ index ] = value;
        m_count++;

        this.PostAdd( value );
      }

      private void PostAdd( Entry entry )
      {
        this.MergeRegion( entry.Region );
      }

      private void PostRemove( Entry entry )
      {
        this.SliceRegion( entry.Region );
      }

      private void InvalidateParentRegion()
      {
        var parent = this.Parent;
        if( parent == null )
          return;

        parent.InvalidateRegion();
      }

      private int FindSplitAxis( Entry[] entries, int minNodeSize )
      {
        Debug.Assert( ( entries != null ) && ( entries.Length == m_count ) );

        var axis = 0;
        var minimum = double.PositiveInfinity;
        var dimensions = m_helper.GetDimensions();

        for( int i = 0; i < dimensions; i++ )
        {
          Array.Sort( entries, new LowerBoundComparer( m_helper, i ) );

          var value = this.CalculateMargin( entries, minNodeSize );
          if( value < minimum )
          {
            minimum = value;
            axis = i;
          }

          Array.Sort( entries, new UpperBoundComparer( m_helper, i ) );

          value = this.CalculateMargin( entries, minNodeSize );
          if( value < minimum )
          {
            minimum = value;
            axis = i;
          }
        }

        return axis;
      }

      private int FindSplitIndex( Entry[] entries, int minNodeSize, int dimension )
      {
        Array.Sort( entries, new LowerBoundComparer( m_helper, dimension ) );

        var lowerOverlap = double.PositiveInfinity;
        var lowerArea = double.PositiveInfinity;
        var lowerIndex = this.FindSplitIndex( entries, minNodeSize, out lowerOverlap, out lowerArea );

        Array.Sort( entries, new UpperBoundComparer( m_helper, dimension ) );

        var upperOverlap = double.PositiveInfinity;
        var upperArea = double.PositiveInfinity;
        var upperIndex = this.FindSplitIndex( entries, minNodeSize, out upperOverlap, out upperArea );

        if( ( upperOverlap < lowerOverlap ) || ( ( upperOverlap == lowerOverlap ) && ( upperArea < lowerArea ) ) )
          return upperIndex;

        Array.Sort( entries, new LowerBoundComparer( m_helper, dimension ) );
        return lowerIndex;
      }

      private int FindSplitIndex( Entry[] entries, int minSize, out double overlap, out double area )
      {
        var splitIndex = 0;

        overlap = double.PositiveInfinity;
        area = double.PositiveInfinity;

        var maxSize = entries.Length - minSize;

        for( int i = minSize; i < maxSize; i++ )
        {
          var firstRegion = m_helper.GetEmptyRegion();
          for( int j = 0; j < i; j++ )
          {
            firstRegion = m_helper.Union( firstRegion, entries[ j ].Region );
          }

          var secondRegion = m_helper.GetEmptyRegion();
          for( int j = i; j < entries.Length; j++ )
          {
            secondRegion = m_helper.Union( secondRegion, entries[ j ].Region );
          }

          var currentOverlap = m_helper.CalculateArea( m_helper.Intersect( firstRegion, secondRegion ) );
          var currentArea = m_helper.CalculateArea( firstRegion ) + m_helper.CalculateArea( secondRegion );

          if( ( currentOverlap < overlap ) || ( ( currentOverlap == overlap ) && ( currentArea < area ) ) )
          {
            overlap = currentOverlap;
            area = currentArea;
            splitIndex = i;
          }
        }

        return splitIndex;
      }

      private double CalculateMargin( IList<Entry> entries, int minSize )
      {
        var maxSize = entries.Count - minSize;
        var margin = 0d;

        for( int i = minSize; i < maxSize; i++ )
        {
          var region = m_helper.GetEmptyRegion();
          for( int j = 0; j < i; j++ )
          {
            region = m_helper.Union( region, entries[ j ].Region );
          }

          margin += m_helper.CalculateMargin( region );

          region = m_helper.GetEmptyRegion();
          for( int j = i; j < entries.Count; j++ )
          {
            region = m_helper.Union( region, entries[ j ].Region );
          }

          margin += m_helper.CalculateMargin( region );
        }

        return margin;
      }

      void ICollection<Entry>.CopyTo( Entry[] array, int index )
      {
        this.CopyToCore( array, index );
      }

      private readonly RSTreeHelper<TRegion> m_helper;
      private readonly Entry[] m_values;
      private int m_count;
      private TRegion m_region;
      private bool m_regionValid; //false
    }

    #endregion

    #region NodeInfo Private Struct

    private struct NodeInfo
    {
      internal NodeInfo( Node node, int level )
      {
        this.Node = node;
        this.Level = level;
      }

      internal readonly Node Node;
      internal readonly int Level;
    }

    #endregion

    #region KPoint Private Struct

    private struct KPoint
    {
      internal KPoint( params double[] values )
      {
        if( values == null )
          throw new ArgumentNullException( "values" );

        m_values = values;
      }

      public int Count
      {
        get
        {
          return m_values.Length;
        }
      }

      public double this[ int index ]
      {
        get
        {
          if( ( index < 0 ) || ( index >= m_values.Length ) )
            throw new ArgumentOutOfRangeException( "index" );

          return m_values[ index ];
        }
      }

      public static double Distance( KPoint x, KPoint y )
      {
        var xs = x.m_values;
        var ys = y.m_values;

        if( xs.Length != ys.Length )
          throw new ArgumentException( "The points must have the same number of values.", "y" );

        if( ( xs == ys ) || ( xs.Length == 0 ) )
          return 0d;

        if( xs.Length == 1 )
          return System.Math.Abs( xs[ 0 ] - ys[ 0 ] );

        var squareSum = 0d;
        for( int i = 0; i < xs.Length; i++ )
        {
          var delta = xs[ i ] - ys[ i ];
          squareSum += delta * delta;
        }

        return System.Math.Sqrt( squareSum );
      }

      public override int GetHashCode()
      {
        var hashCode = 0;

        foreach( var value in m_values )
        {
          hashCode *= 13;
          hashCode += value.GetHashCode();
        }

        return hashCode;
      }

      public override bool Equals( object obj )
      {
        if( !( obj is KPoint ) )
          return false;

        var values = ( ( KPoint )obj ).m_values;
        if( values == m_values )
          return true;

        if( values.Length != m_values.Length )
          return false;

        for( int i = 0; i < m_values.Length; i++ )
        {
          if( values[ i ] != m_values[ i ] )
            return false;
        }

        return true;
      }

      private readonly double[] m_values;
    }

    #endregion

    #region LowerBoundComparer Private Class

    private sealed class LowerBoundComparer : IComparer<Node>, IComparer<Entry>
    {
      internal LowerBoundComparer( RSTreeHelper<TRegion> helper, int dimension )
      {
        Debug.Assert( helper != null );

        m_helper = helper;
        m_dimension = dimension;
      }

      int IComparer<Node>.Compare( Node x, Node y )
      {
        return this.GetValue( x.Region ).CompareTo( this.GetValue( y.Region ) );
      }

      int IComparer<Entry>.Compare( Entry x, Entry y )
      {
        return this.GetValue( x.Region ).CompareTo( this.GetValue( y.Region ) );
      }

      private double GetValue( TRegion region )
      {
        Debug.Assert( !m_helper.IsEmptyRegion( region ) );

        return m_helper.GetValueOf( region, m_dimension );
      }

      private readonly RSTreeHelper<TRegion> m_helper;
      private readonly int m_dimension;
    }

    #endregion

    #region UpperBoundComparer Private Class

    private sealed class UpperBoundComparer : IComparer<Node>, IComparer<Entry>
    {
      internal UpperBoundComparer( RSTreeHelper<TRegion> helper, int dimension )
      {
        Debug.Assert( helper != null );

        m_helper = helper;
        m_dimension = dimension;
      }

      int IComparer<Node>.Compare( Node x, Node y )
      {
        return this.GetValue( x.Region ).CompareTo( this.GetValue( y.Region ) );
      }

      int IComparer<Entry>.Compare( Entry x, Entry y )
      {
        return this.GetValue( x.Region ).CompareTo( this.GetValue( y.Region ) );
      }

      private double GetValue( TRegion region )
      {
        Debug.Assert( !m_helper.IsEmptyRegion( region ) );

        return m_helper.GetValueOf( region, m_dimension ) + m_helper.GetSizeOf( region, m_dimension );
      }

      private readonly RSTreeHelper<TRegion> m_helper;
      private readonly int m_dimension;
    }

    #endregion
  }
}
