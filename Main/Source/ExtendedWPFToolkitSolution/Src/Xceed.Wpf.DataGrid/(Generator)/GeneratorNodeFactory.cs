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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Xceed.Wpf.DataGrid.Utils;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal class GeneratorNodeFactory : IWeakEventListener
  {
    public GeneratorNodeFactory( NotifyCollectionChangedEventHandler itemsChangedHandler,
                                 NotifyCollectionChangedEventHandler groupsChangedHandler,
                                 EventHandler<ExpansionStateChangedEventArgs> expansionStateChangedHandler,
                                 EventHandler isExpandedChangingHandler,
                                 EventHandler isExpandedChangedHandler )
    {
      if( itemsChangedHandler == null )
      {
        throw new ArgumentNullException( "itemsChangedHandler" );
      }

      if( groupsChangedHandler == null )
      {
        throw new ArgumentNullException( "groupsChangedHandler" );
      }

      if( expansionStateChangedHandler == null )
      {
        throw new ArgumentNullException( "expansionStateChangedHandler" );
      }

      if( isExpandedChangingHandler == null )
      {
        throw new ArgumentNullException( "isExpandedChangingHandler" );
      }

      if( isExpandedChangedHandler == null )
      {
        throw new ArgumentNullException( "isExpandedChangedHandler" );
      }

      m_itemsChangedHandler = itemsChangedHandler;
      m_groupsChangedHandler = groupsChangedHandler;
      m_expansionStateChangedHandler = expansionStateChangedHandler;
      m_isExpandedChangingHandler = isExpandedChangingHandler;
      m_isExpandedChangedHandler = isExpandedChangedHandler;
    }

    #region IWeakEventListener Members

    public bool ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        NotifyCollectionChangedEventHandler handler;
        m_nodesCollectionChangedEventHandlers.TryGetValue( sender, out handler );

        if( handler != null )
        {
          handler.Invoke( sender, ( NotifyCollectionChangedEventArgs )e );
        }

        return true;
      }

      return false;
    }

    #endregion IWeakEventListener Members

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

      node.IsExpanded = groupConfig.InitiallyExpanded;

      if( collectionViewGroup.IsBottomLevel == false )
      {
        IList<object> subItems = collectionViewGroup.GetItems();

        this.RegisterNodeCollectionChanged(
          ( INotifyCollectionChanged )subItems,
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
      GeneratorNode next,
      CustomItemContainerGenerator generator )
    {
      Debug.Assert( collection != null, "collection cannot be null for CreateItemsGeneratorNode()" );
      Debug.Assert( generator != null );

      INotifyCollectionChanged notifyCollection = collection as INotifyCollectionChanged;

      Debug.Assert( notifyCollection != null, "collection must be a INotifyCollectionChanged for CreateItemsGeneratorNode()" );

      //case 113904: If the item source for the ItemsGeneratorNode is an ItemCollection, then
      //check if the underlying SourceCollection is a DataGridCollectionView.
      //This particular exception case is there to handle messaging quirks in the case 
      //of Master Detail edition. Refer to case for more details.
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

      this.RegisterNodeCollectionChanged( 
        ( INotifyCollectionChanged )newNode.Items,
        new NotifyCollectionChangedEventHandler( newNode.OnCollectionChanged ) );

      newNode.CollectionChanged += m_itemsChangedHandler;
      newNode.ExpansionStateChanged += m_expansionStateChangedHandler;

      if( parent != null )
      {
        newNode.AdjustItemCount( newNode.ItemCount );
      }
    }

    private void ConfigureHeadersFootersNotification( INotifyCollectionChanged notifyCollection, HeadersFootersGeneratorNode node )
    {
      List<HeadersFootersGeneratorNode> nodeList;
      bool keyFound = m_headersFootersMapping.TryGetValue( notifyCollection, out nodeList );

      if( keyFound == false )
      {
        nodeList = new List<HeadersFootersGeneratorNode>();

        notifyCollection.CollectionChanged += OnHeadersFootersCollectionChanged;

        m_headersFootersMapping.Add( notifyCollection, nodeList );
      }

      nodeList.Add( node );
    }

    private void CleanHeadersFootersNotification( HeadersFootersGeneratorNode node )
    {
      INotifyCollectionChanged notifyCollection = node.Items as INotifyCollectionChanged;
      if( notifyCollection != null )
      {
        try
        {
          List<HeadersFootersGeneratorNode> nodeList = m_headersFootersMapping[ notifyCollection ];

          nodeList.Remove( node );

          if( nodeList.Count == 0 )
          {
            notifyCollection.CollectionChanged -= OnHeadersFootersCollectionChanged;
            m_headersFootersMapping.Remove( notifyCollection );
          }
        }
        catch( Exception e )
        {
          throw new DataGridInternalException( e );
        }
      }
    }

    private void OnHeadersFootersCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      INotifyCollectionChanged collectionChanged = sender as INotifyCollectionChanged;

      if( collectionChanged != null )
      {
        try
        {
          List<HeadersFootersGeneratorNode> nodeList = m_headersFootersMapping[ collectionChanged ];

          foreach( HeadersFootersGeneratorNode node in nodeList )
          {
            m_itemsChangedHandler.Invoke( node, e );
          }
        }
        catch( Exception ex )
        {
          throw new DataGridInternalException( ex );
        }
      }
    }

    private void RegisterNodeCollectionChanged( INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler )
    {
      m_nodesCollectionChangedEventHandlers.Add( source, handler );
      CollectionChangedEventManager.AddListener( source, this );
    }

    private void UnregisterNodeCollectionChanged( INotifyCollectionChanged source )
    {
      bool existed = m_nodesCollectionChangedEventHandlers.Remove( source );

      if( existed )
      {
        CollectionChangedEventManager.RemoveListener( source, this );
      }
    }

    private readonly NotifyCollectionChangedEventHandler m_itemsChangedHandler; // = null
    private readonly NotifyCollectionChangedEventHandler m_groupsChangedHandler; // = null
    private readonly EventHandler<ExpansionStateChangedEventArgs> m_expansionStateChangedHandler; // = null
    private readonly EventHandler m_isExpandedChangedHandler; // = null
    private readonly EventHandler m_isExpandedChangingHandler; // = null

    private readonly Dictionary<INotifyCollectionChanged, List<HeadersFootersGeneratorNode>> m_headersFootersMapping = new Dictionary<INotifyCollectionChanged, List<HeadersFootersGeneratorNode>>(32);

    private readonly Dictionary<object, NotifyCollectionChangedEventHandler> m_nodesCollectionChangedEventHandlers = new Dictionary<object, NotifyCollectionChangedEventHandler>(32);
  }
}
