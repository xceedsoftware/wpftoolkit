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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal class GeneratorNodeFactory : IWeakEventListener
  {
    public GeneratorNodeFactory( NotifyCollectionChangedEventHandler itemsChangedHandler,
                                 NotifyCollectionChangedEventHandler groupsChangedHandler,
                                 NotifyCollectionChangedEventHandler headersFootersChangedHandler,
                                 EventHandler<ExpansionStateChangedEventArgs> expansionStateChangedHandler,
                                 EventHandler isExpandedChangingHandler,
                                 EventHandler isExpandedChangedHandler,
                                 DataGridControl dataGridControl )
    {
      if( itemsChangedHandler == null )
        throw new ArgumentNullException( "itemsChangedHandler" );

      if( groupsChangedHandler == null )
        throw new ArgumentNullException( "groupsChangedHandler" );

      if( headersFootersChangedHandler == null )
        throw new ArgumentNullException( "headersFootersChangedHandler" );

      if( expansionStateChangedHandler == null )
        throw new ArgumentNullException( "expansionStateChangedHandler" );

      if( isExpandedChangingHandler == null )
        throw new ArgumentNullException( "isExpandedChangingHandler" );

      if( isExpandedChangedHandler == null )
        throw new ArgumentNullException( "isExpandedChangedHandler" );

      m_itemsChangedHandler = itemsChangedHandler;
      m_groupsChangedHandler = groupsChangedHandler;
      m_headersFootersChangedHandler = headersFootersChangedHandler;
      m_expansionStateChangedHandler = expansionStateChangedHandler;
      m_isExpandedChangingHandler = isExpandedChangingHandler;
      m_isExpandedChangedHandler = isExpandedChangedHandler;

      if( dataGridControl != null )
      {
        m_dataGridControl = new WeakReference( dataGridControl );
      }
    }

    #region DataGridControl Private Property

    private DataGridControl DataGridControl
    {
      get
      {
        if( m_dataGridControl == null )
          return null;

        return m_dataGridControl.Target as DataGridControl;
      }
    }

    //For exception purposes only.
    private readonly WeakReference m_dataGridControl; //null

    #endregion

    public GeneratorNode CreateGroupGeneratorNode(
      CollectionViewGroup collectionViewGroup,
      GeneratorNode parent,
      GeneratorNode previous,
      GeneratorNode next,
      GroupConfiguration groupConfig )
    {
      Debug.Assert( collectionViewGroup != null, "collectionViewGroup cannot be null for CreateGroupGeneratorNode()" );

      GroupGeneratorNode node = new GroupGeneratorNode( collectionViewGroup, parent, groupConfig );

      if( previous != null )
      {
        previous.Next = node;
      }
      node.Previous = previous;

      if( next != null )
      {
        next.Previous = node;
      }
      node.Next = next;

      node.SetIsExpandedAtInitialization( groupConfig.InitiallyExpanded );

      if( !collectionViewGroup.IsBottomLevel )
      {
        this.RegisterNodeCollectionChanged(
          ( INotifyCollectionChanged )collectionViewGroup.GetItems(),
          new NotifyCollectionChangedEventHandler( node.OnCollectionChanged ) );

        node.CollectionChanged += m_groupsChangedHandler;
      }

      node.ExpansionStateChanged += m_expansionStateChangedHandler;
      node.IsExpandedChanging += m_isExpandedChangingHandler;
      node.IsExpandedChanged += m_isExpandedChangedHandler;

      node.AdjustItemCount( node.ItemCount );

      node.BuildNamesTree();

      return node;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "generator" )]
    public GeneratorNode CreateItemsGeneratorNode(
      IList collection,
      GeneratorNode parent,
      GeneratorNode previous,
      GeneratorNode next )
    {
      Debug.Assert( collection != null, "collection cannot be null for CreateItemsGeneratorNode()" );

      INotifyCollectionChanged notifyCollection = collection as INotifyCollectionChanged;

      Debug.Assert( notifyCollection != null, "collection must be a INotifyCollectionChanged for CreateItemsGeneratorNode()" );

      ItemCollection itemCollection = collection as ItemCollection;
      if( itemCollection != null )
      {
        DataGridCollectionView dgcv = itemCollection.SourceCollection as DataGridCollectionView;
        if( dgcv != null )
        {
          collection = dgcv;
        }
      }

      ItemsGeneratorNode node = new ItemsGeneratorNode( collection, parent );

      this.SetupCollectionGeneratorNode( node, parent, previous, next );

      node.AdjustLeafCount( node.Items.Count );

      return node;
    }

    public HeadersFootersGeneratorNode CreateHeadersFootersGeneratorNode(
      IList collection,
      GeneratorNode parent,
      GeneratorNode previous,
      GeneratorNode next )
    {
      Debug.Assert( collection != null, "collection cannot be null for CreateHeadersFootersGeneratorNode()" );

      INotifyCollectionChanged notifyCollection = collection as INotifyCollectionChanged;

      Debug.Assert( notifyCollection != null, "collection must be a INotifyCollectionChanged for CreateHeadersFootersGeneratorNode()" );

      HeadersFootersGeneratorNode node = new HeadersFootersGeneratorNode( collection, parent );

      if( previous != null )
      {
        previous.Next = node;
      }
      node.Previous = previous;

      if( next != null )
      {
        next.Previous = node;
      }
      node.Next = next;

      node.ExpansionStateChanged += m_expansionStateChangedHandler;

      this.ConfigureHeadersFootersNotification( notifyCollection, node );

      if( parent != null )
      {
        node.AdjustItemCount( node.ItemCount );
      }

      return node;
    }

    public void CleanGeneratorNodeTree( GeneratorNode node )
    {
      if( node.Parent != null )
      {
        node.Parent.AdjustItemCount( -node.ItemCount );

        GroupGeneratorNode parentGroupNode = node.Parent as GroupGeneratorNode;
        if( ( parentGroupNode != null ) && ( parentGroupNode.Child == node ) )
        {
          parentGroupNode.Child = null;
        }
      }

      GeneratorNode child;
      GeneratorNode next;

      do
      {
        GroupGeneratorNode groupNode = node as GroupGeneratorNode;

        next = node.Next;
        child = ( groupNode != null ) ? groupNode.Child : null;

        this.CleanGeneratorNode( node );

        //this recursive function cleans up the tree of nodes
        if( child != null )
        {
          this.CleanGeneratorNodeTree( child );
        }

        node = next;
      }
      while( node != null );
    }

    public void CleanGeneratorNode( GeneratorNode node )
    {
      HeadersFootersGeneratorNode headersFootersNode = node as HeadersFootersGeneratorNode;
      if( headersFootersNode != null )
      {
        this.CleanHeadersFootersNotification( headersFootersNode );
      }
      else
      {
        ItemsGeneratorNode itemsNode = node as ItemsGeneratorNode;
        if( itemsNode != null )
        {
          this.UnregisterNodeCollectionChanged( ( INotifyCollectionChanged )itemsNode.Items );
          itemsNode.CollectionChanged -= m_itemsChangedHandler;
        }
        else
        {
          GroupGeneratorNode groupNode = node as GroupGeneratorNode;
          if( groupNode != null )
          {
            IList<object> subItems = groupNode.CollectionViewGroup.GetItems();

            this.UnregisterNodeCollectionChanged( ( INotifyCollectionChanged )subItems );

            groupNode.CollectionChanged -= m_groupsChangedHandler;
            groupNode.IsExpandedChanging -= m_isExpandedChangingHandler;
            groupNode.IsExpandedChanged -= m_isExpandedChangedHandler;
          }
        }
      }

      node.ExpansionStateChanged -= m_expansionStateChangedHandler;

      node.CleanGeneratorNode();
    }

    private void SetupCollectionGeneratorNode( CollectionGeneratorNode newNode, GeneratorNode parent, GeneratorNode previous, GeneratorNode next )
    {
      if( previous != null )
      {
        previous.Next = newNode;
      }
      newNode.Previous = previous;

      if( next != null )
      {
        next.Previous = newNode;
      }
      newNode.Next = next;

      this.RegisterNodeCollectionChanged( ( INotifyCollectionChanged )newNode.Items, new NotifyCollectionChangedEventHandler( newNode.OnCollectionChanged ) );

      newNode.CollectionChanged += m_itemsChangedHandler;
      newNode.ExpansionStateChanged += m_expansionStateChangedHandler;

      if( parent != null )
      {
        newNode.AdjustItemCount( newNode.ItemCount );
      }
    }

    private void ConfigureHeadersFootersNotification( INotifyCollectionChanged collection, HeadersFootersGeneratorNode node )
    {
      ICollection<HeadersFootersGeneratorNode> nodeList;
      if( !m_headersFootersMapping.TryGetValue( collection, out nodeList ) )
      {
        nodeList = new HashSet<HeadersFootersGeneratorNode>();

        m_headersFootersMapping.Add( collection, nodeList );
        CollectionChangedEventManager.AddListener( collection, this );
      }

      nodeList.Add( node );
    }

    private void CleanHeadersFootersNotification( HeadersFootersGeneratorNode node )
    {
      var collection = node.Items as INotifyCollectionChanged;
      if( collection == null )
        return;

      try
      {
        var nodeList = m_headersFootersMapping[ collection ];
        nodeList.Remove( node );

        if( nodeList.Count == 0 )
        {
          CollectionChangedEventManager.RemoveListener( collection, this );
          m_headersFootersMapping.Remove( collection );
        }
      }
      catch( Exception e )
      {
        throw new DataGridInternalException( e.Message, e, this.DataGridControl );
      }
    }

    private void RegisterNodeCollectionChanged( INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler )
    {
      m_nodesCollectionChangedEventHandlers.Add( source, handler );
      CollectionChangedEventManager.AddListener( source, this );
    }

    private void UnregisterNodeCollectionChanged( INotifyCollectionChanged source )
    {
      if( m_nodesCollectionChangedEventHandlers.Remove( source ) )
      {
        CollectionChangedEventManager.RemoveListener( source, this );
      }
    }

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        this.OnCollectionChanged( ( INotifyCollectionChanged )sender, ( NotifyCollectionChangedEventArgs )e );
      }
      else
      {
        return false;
      }

      return true;
    }

    private void OnCollectionChanged( INotifyCollectionChanged source, NotifyCollectionChangedEventArgs e )
    {
      NotifyCollectionChangedEventHandler handler;
      ICollection<HeadersFootersGeneratorNode> nodeList;

      if( m_nodesCollectionChangedEventHandlers.TryGetValue( source, out handler ) )
      {
        handler.Invoke( source, e );
      }
      else if( m_headersFootersMapping.TryGetValue( source, out nodeList ) )
      {
        Debug.Assert( nodeList.Count > 0 );

        m_headersFootersChangedHandler.Invoke( nodeList, e );
      }
    }

    #endregion

    #region Private Fields

    private readonly NotifyCollectionChangedEventHandler m_itemsChangedHandler;
    private readonly NotifyCollectionChangedEventHandler m_groupsChangedHandler;
    private readonly NotifyCollectionChangedEventHandler m_headersFootersChangedHandler;
    private readonly EventHandler<ExpansionStateChangedEventArgs> m_expansionStateChangedHandler;
    private readonly EventHandler m_isExpandedChangedHandler;
    private readonly EventHandler m_isExpandedChangingHandler;

    private readonly Dictionary<INotifyCollectionChanged, ICollection<HeadersFootersGeneratorNode>> m_headersFootersMapping = new Dictionary<INotifyCollectionChanged, ICollection<HeadersFootersGeneratorNode>>( 32 );
    private readonly Dictionary<object, NotifyCollectionChangedEventHandler> m_nodesCollectionChangedEventHandlers = new Dictionary<object, NotifyCollectionChangedEventHandler>( 32 );

    #endregion
  }
}
