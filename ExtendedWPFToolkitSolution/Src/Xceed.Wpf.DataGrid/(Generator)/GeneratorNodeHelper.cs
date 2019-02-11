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
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class GeneratorNodeHelper
  {
    internal GeneratorNodeHelper( GeneratorNode startNode, int index, int sourceDataIndex )
    {
      if( startNode == null )
        throw new ArgumentNullException( "startNode" );

      m_state = new State( startNode, index, sourceDataIndex );
    }

    internal GeneratorNode CurrentNode
    {
      get
      {
        return m_state.Node;
      }
    }

    internal int Index
    {
      get
      {
        return m_state.Index;
      }
    }

    internal int SourceDataIndex
    {
      get
      {
        return m_state.DataIndex;
      }
    }

    internal bool MoveToNext()
    {
      var success = GeneratorNodeHelper.MoveToNext( ref m_state );

      this.EnsureState();

      return success;
    }

    internal bool MoveToNextBy( int count )
    {
      var success = GeneratorNodeHelper.MoveToNext( ref m_state, count );

      this.EnsureState();

      return success;
    }

    internal bool MoveToPrevious()
    {
      var success = GeneratorNodeHelper.MoveToPrevious( ref m_state );

      this.EnsureState();

      return success;
    }

    internal bool MoveToFirst()
    {
      GeneratorNodeHelper.MoveToFirst( ref m_state );

      this.EnsureState();

      return true;
    }

    internal bool MoveToEnd()
    {
      // We call MoveToNext instead of MoveToEnd because MoveToEnd includes the indexes of the last node.
      while( GeneratorNodeHelper.MoveToNext( ref m_state ) )
      {
      }

      this.EnsureState();

      return true;
    }

    internal bool MoveToChild( bool skipItemLessGroupNodes )
    {
      var success = GeneratorNodeHelper.MoveToChild( ref m_state, skipItemLessGroupNodes );

      this.EnsureState();

      return success;
    }

    internal bool MoveToParent()
    {
      var success = GeneratorNodeHelper.MoveToParent( ref m_state );

      this.EnsureState();

      return success;
    }

    internal bool InsertAfter( GeneratorNode insert )
    {
      if( insert == null )
        throw new DataGridInternalException( "GeneratorNode is null." );

      var insertionCount = default( int );
      var chainLength = default( int );
      var insertLast = GeneratorNodeHelper.EvaluateChain( insert, out insertionCount, out chainLength );

      var nextNode = m_state.Node.Next;
      if( nextNode != null )
      {
        nextNode.Previous = insertLast;
      }

      insertLast.Next = nextNode;
      insert.Previous = m_state.Node;
      m_state.Node.Next = insert;

      // Move the current node to the last node inserted
      if( !this.MoveToNextBy( chainLength ) )
        throw new DataGridInternalException( "Unable to move to the requested generator index." );

      return true;
    }

    internal bool InsertBefore( GeneratorNode insert )
    {
      if( insert == null )
        throw new DataGridInternalException( "GeneratorNode is null" );

      var insertionCount = default( int );
      var chainLength = default( int );
      var insertLast = GeneratorNodeHelper.EvaluateChain( insert, out insertionCount, out chainLength );

      var previousNode = m_state.Node.Previous;
      if( previousNode != null )
      {
        previousNode.Next = insert;
      }

      insert.Previous = previousNode;
      m_state.Node.Previous = insertLast;
      insertLast.Next = m_state.Node;

      var parentGroup = insert.Parent as GroupGeneratorNode;
      if( parentGroup != null )
      {
        if( parentGroup.Child == m_state.Node )
        {
          parentGroup.Child = insert;
        }
      }

      // Move the current to the first item inserted.
      // No need to update the indexes since they will still be with the correct value.
      m_state.Node = insert;

      this.EnsureState();

      return true;
    }

    internal int FindItem( object item )
    {
      var current = m_state;

      while( true )
      {
        var itemsNode = current.Node as ItemsGeneratorNode;
        if( itemsNode != null )
        {
          var index = itemsNode.Items.IndexOf( item );
          if( index >= 0 )
          {
            index += itemsNode.CountDetailsBeforeDataIndex( index );

            // Item is directly from this items node... then return the appropriate index!
            m_state = current;

            this.EnsureState();

            return index + current.Index;
          }

          // If the item is from a detail, then I don't want to "use" it!!!
          // but continue looping.... to find occurances of this item somewhere else in the tree.
        }
        else
        {
          var collectionNode = current.Node as CollectionGeneratorNode;
          if( collectionNode != null )
          {
            var index = collectionNode.IndexOf( item );
            if( index >= 0 )
            {
              m_state = current;

              this.EnsureState();

              return index + current.Index;
            }
          }
        }

        // If we reach this point, it's because the item we are looking
        // for is not in this node... Try to access the child
        if( !GeneratorNodeHelper.MoveToChild( ref current ) )
        {
          // Try "advancing" to the next item.
          if( !GeneratorNodeHelper.MoveToFollowing( ref current ) )
            break;
        }
      }

      return -1;
    }

    internal object FindIndex( int index )
    {
      var current = m_state;

      while( true )
      {
        if( index < current.Index + current.Node.ItemCount )
        {
          var itemsNode = current.Node as CollectionGeneratorNode;
          if( itemsNode != null )
          {
            m_state = current;

            this.EnsureState();

            return itemsNode.GetAt( index - current.Index );
          }

          if( !GeneratorNodeHelper.MoveToChild( ref current ) )
            break;
        }
        else
        {
          // If we reach this point, it's because the item we are looking for is not in this node... Try to access the child.
          // No need to check for childs, since the condition above would catch it (childs part of ItemCount).
          if( !GeneratorNodeHelper.MoveToFollowing( ref current ) )
            break;
        }
      }

      return null;
    }

    internal bool FindNode( GeneratorNode node )
    {
      if( node == null )
        throw new ArgumentNullException( "node" );

      var success = GeneratorNodeHelper.FindNode( ref m_state, node );

      this.EnsureState();

      return success;
    }

    internal bool FindNodeForIndex( int index )
    {
      var success = GeneratorNodeHelper.FindNodeForIndex( ref m_state, index );

      this.EnsureState();

      return success;
    }

    //Note: this function will not check for the presence of the item in the details for Items nodes.
    internal bool AbsoluteFindItem( object item )
    {
      // This method will search through nodes, even those collapsed for the item.
      var current = m_state;

      while( true )
      {
        var itemsNode = current.Node as CollectionGeneratorNode;
        if( ( itemsNode != null ) && itemsNode.Items.Contains( item ) )
          break;

        // If we reach this point, it's because the item we are looking
        // for is not in this node... Try to access the child
        if( !GeneratorNodeHelper.MoveToChild( ref current, false ) )
        {
          // Try "advancing" to the next item.
          if( !GeneratorNodeHelper.MoveToFollowing( ref current ) )
            return false;
        }
      }

      m_state = current;

      this.EnsureState();

      return true;
    }

    //Note: this function will not check for the presence of the group in the details for Items nodes.
    internal bool FindGroup( CollectionViewGroup group )
    {
      if( group == null )
        return false;

      var current = m_state;

      while( true )
      {
        var groupNode = current.Node as GroupGeneratorNode;
        if( groupNode != null )
        {
          if( groupNode.CollectionViewGroup == group )
            break;

          if( !groupNode.CollectionViewGroup.IsBottomLevel )
          {
            if( !GeneratorNodeHelper.MoveToChild( ref current, false ) )
              return false;
          }
          else
          {
            if( !GeneratorNodeHelper.MoveToFollowing( ref current ) )
              return false;
          }
        }
        else
        {
          // There is nothing under a non GroupGeneratorNode, try to move Next node in the list.
          if( !GeneratorNodeHelper.MoveToFollowing( ref current ) )
            return false;
        }
      }

      m_state = current;

      this.EnsureState();

      return true;
    }

    internal void ReverseCalculateIndex()
    {
      m_state = GeneratorNodeHelper.FindNodeLocation( m_state.Node );

      this.EnsureState();
    }

    internal bool MoveForward()
    {
      var startNode = m_state.Node;

      while( true )
      {
        if( !( m_state.Node is GroupGeneratorNode ) && ( m_state.Node != startNode ) && ( m_state.Node.ItemCount != 0 ) )
          break;

        if( !GeneratorNodeHelper.MoveToChild( ref m_state ) )
        {
          if( !GeneratorNodeHelper.MoveToFollowing( ref m_state ) )
            return false;
        }
      }

      this.EnsureState();

      return true;
    }

    internal bool MoveBackward()
    {
      var startNode = m_state.Node;

      while( true )
      {
        if( !( m_state.Node is GroupGeneratorNode ) && ( m_state.Node != startNode ) && ( m_state.Node.ItemCount != 0 ) )
          break;

        if( !GeneratorNodeHelper.MoveToChild( ref m_state ) )
        {
          if( !GeneratorNodeHelper.MoveToPreceding( ref m_state ) )
            return false;
        }
        else
        {
          GeneratorNodeHelper.MoveToEnd( ref m_state );
        }
      }

      this.EnsureState();

      return true;
    }

    // This method cannot be used for groups.
    // This method will search for items independently of the Expanded/Collapsed status of GroupGeneratorNodes.
    internal bool Contains( object item )
    {
      var current = m_state;
      var found = false;
      var skipCollectionGeneratorNodeCheck = false;

      while( true )
      {
        skipCollectionGeneratorNodeCheck = false;

        var headersFootersNode = current.Node as HeadersFootersGeneratorNode;
        if( ( headersFootersNode != null ) && ( item is GroupHeaderFooterItem ) )
        {
          var groupHeaderFooterItem = ( GroupHeaderFooterItem )item;
          var parentGroup = headersFootersNode.Parent as GroupGeneratorNode;

          if( ( parentGroup != null ) && ( parentGroup.CollectionViewGroup == groupHeaderFooterItem.Group ) && headersFootersNode.Items.Contains( groupHeaderFooterItem.Template ) )
          {
            found = true;
            break;
          }

          // If there is no parent node, then its because the current HeadersFootersGeneratorNode is not a GroupHeaders/Footers node (FixedHeaders/Footers or Headers/Footers).
          skipCollectionGeneratorNodeCheck = true; // force skip CollectionGeneratorNode verification, this is to limit amount of job done by loop body.
        }

        if( !skipCollectionGeneratorNodeCheck )
        {
          var collectionNode = current.Node as CollectionGeneratorNode;
          if( collectionNode != null )
          {
            // When dealing with a DataView, the DataView's IList's Contains implementation will return false 
            // for a dataRowView which is in edition and was modified even though it is really part of the collection.
            // Therefore, we must use a for loop of Object.Equals method calls.
            var dataRowViewItem = item as System.Data.DataRowView;
            if( dataRowViewItem != null )
            {
              var items = collectionNode.Items;
              var itemsCount = items.Count;
              var itemDataRow = dataRowViewItem.Row;

              for( int i = 0; i < itemsCount; i++ )
              {
                var currentDataRowView = items[ i ] as System.Data.DataRowView;
                if( ( currentDataRowView != null ) && ( itemDataRow == currentDataRowView.Row ) )
                {
                  found = true;
                  break;
                }
              }

              if( found )
                break;
            }
            else
            {
              // Since the GetAt() methods can be overriden to compensate for the Expand/Collapse status of Groups
              // AND the details features, accessing the collection directly prevent pre-processing of the content of the collection node.
              if( collectionNode.Items.Contains( item ) )
              {
                found = true;
                break;
              }
            }
          }
        }

        if( GeneratorNodeHelper.MoveToChild( ref current, false ) )
          continue;

        if( GeneratorNodeHelper.MoveToFollowing( ref current ) )
          continue;

        break;
      }

      m_state = current;

      this.EnsureState();

      return found;
    }

    internal void ProcessVisit(
      DataGridContext sourceContext,
      int minIndex,
      int maxIndex,
      IDataGridContextVisitor visitor,
      DataGridContextVisitorType visitorType,
      bool visitDetails,
      out bool visitWasStopped )
    {
      visitWasStopped = false;

      // This is used only for DataGridContextVisitorType.ItemsBlock
      var startSourceDataItemIndex = -1;
      var endSourceDataItemIndex = -1;

      if( minIndex < 0 )
        throw DataGridException.Create<ArgumentException>( "The minimum index must be greater than or equal to zero.", sourceContext.DataGridControl, "minIndex" );

      if( ( visitorType & DataGridContextVisitorType.DataGridContext ) == DataGridContextVisitorType.DataGridContext )
      {
        visitor.Visit( sourceContext, ref visitWasStopped );

        if( visitWasStopped )
          return;
      }

      //Take a shortcut, if the visit is made only for contexts, and there is no child contexts
      //return right away.
      var containsDetails = false;

      foreach( var childContext in sourceContext.GetChildContextsCore() )
      {
        containsDetails = true;
        break;
      }

      var processed = false;
      var current = m_state;

      while( true )
      {
        processed = false;

        var itemCount = current.Node.ItemCount;

        //If the whole current node is below the minIndex, jump over it.
        if( ( current.Index + ( itemCount - 1 ) ) < minIndex )
        {
          processed = true;
        }

        //when the index to visit exceeds the range defined, exit the loop.
        if( current.Index > maxIndex )
          break;

        var minForNode = Math.Max( 0, minIndex - current.Index ); // this will give the base offset within the node where to start the visitating!
        var maxForNode = Math.Min( itemCount - 1, maxIndex - current.Index ); //this will five the max offset within this node to visit (protected against overlfow )

        if( !processed )
        {
          var headersNode = current.Node as HeadersFootersGeneratorNode;
          if( headersNode != null )
          {
            var isHeaderFooter = ( headersNode.Parent == null );

            //If the node is a Headers or Footers node AND the visitorType does not contain HeadersFooters
            if( ( isHeaderFooter ) && ( ( visitorType & DataGridContextVisitorType.HeadersFooters ) == DataGridContextVisitorType.HeadersFooters ) )
            {
              GeneratorNodeHelper.ProcessHeadersNodeVisit( headersNode, sourceContext, minForNode, maxForNode, visitor, ref visitWasStopped );
            }
            else if( ( !isHeaderFooter ) && ( ( visitorType & DataGridContextVisitorType.GroupHeadersFooters ) == DataGridContextVisitorType.GroupHeadersFooters ) )
            {
              GeneratorNodeHelper.ProcessHeadersNodeVisit( headersNode, sourceContext, minForNode, maxForNode, visitor, ref visitWasStopped );
            }

            processed = true;
          }
        }

        if( !processed )
        {
          var itemsNode = current.Node as ItemsGeneratorNode;
          if( itemsNode != null )
          {
            if( ( visitorType & DataGridContextVisitorType.ItemsBlock ) == DataGridContextVisitorType.ItemsBlock )
            {
              GeneratorNodeHelper.ProcessItemsNodeBlockVisit(
                itemsNode, sourceContext,
                minForNode, maxForNode,
                visitor, visitorType, visitDetails, containsDetails, current.DataIndex,
                ref startSourceDataItemIndex, ref endSourceDataItemIndex, ref visitWasStopped );
            }
            else if( ( ( visitDetails ) && ( containsDetails ) )
              || ( ( visitorType & DataGridContextVisitorType.Items ) == DataGridContextVisitorType.Items ) )
            {
              GeneratorNodeHelper.ProcessItemsNodeVisit(
                itemsNode, sourceContext,
                minForNode, maxForNode,
                visitor, visitorType, visitDetails, current.DataIndex, ref visitWasStopped );
            }

            processed = true;
          }
        }

        if( !processed )
        {
          var groupNode = current.Node as GroupGeneratorNode;
          if( groupNode != null )
          {
            if( ( visitorType & DataGridContextVisitorType.Groups ) == DataGridContextVisitorType.Groups )
            {
              visitor.Visit(
                sourceContext,
                groupNode.CollectionViewGroup,
                groupNode.NamesTree,
                groupNode.Level,
                groupNode.IsExpanded,
                groupNode.IsComputedExpanded,
                ref visitWasStopped );
            }

            processed = true;
          }
        }

        if( !processed )
          throw DataGridException.Create<DataGridInternalException>( "Unable to process the visit.", sourceContext.DataGridControl );

        if( visitWasStopped )
          break;

        if( GeneratorNodeHelper.MoveToChild( ref current ) )
          continue;

        if( GeneratorNodeHelper.MoveToFollowing( ref current ) )
          continue;

        break;
      }

      if( ( visitorType & DataGridContextVisitorType.ItemsBlock ) == DataGridContextVisitorType.ItemsBlock )
      {
        if( startSourceDataItemIndex != -1 )
        {
          var stopVisit = false;
          visitor.Visit( sourceContext, startSourceDataItemIndex, endSourceDataItemIndex, ref stopVisit );
          visitWasStopped = visitWasStopped || stopVisit;
        }
      }

      m_state = current;

      this.EnsureState();
    }

    internal static GeneratorNode EvaluateChain( GeneratorNode startNode, out int totalChildCount, out int chainLength )
    {
      if( startNode == null )
        throw new ArgumentNullException( "startNode" );

      var current = new State( startNode, 0, 0 );

      totalChildCount = startNode.ItemCount;
      chainLength = 1;

      while( true )
      {
        if( !GeneratorNodeHelper.MoveToNext( ref current ) )
          break;

        totalChildCount += current.Node.ItemCount;
        chainLength++;
      }

      return current.Node;
    }

    private void EnsureState()
    {
      if( m_state.Node == null )
        throw new DataGridInternalException();
    }

    private static void ProcessItemsNodeBlockVisit(
      ItemsGeneratorNode itemsNode,
      DataGridContext sourceContext,
      int minIndex,
      int maxIndex,
      IDataGridContextVisitor visitor,
      DataGridContextVisitorType visitorType,
      bool visitDetails,
      bool containsDetails,
      int sourceDataItemIndex,
      ref int startSourceDataItemIndex,
      ref int endSourceDataItemIndex,
      ref bool stopVisit )
    {
      if( maxIndex < minIndex )
        return;

      int runningIndex = minIndex;
      sourceDataItemIndex += minIndex - itemsNode.CountDetailsBeforeGlobalIndex( minIndex );

      if( !containsDetails )
      {
        // If we contains no detail, we take a quick way out of it.
        if( startSourceDataItemIndex == -1 )
        {
          startSourceDataItemIndex = sourceDataItemIndex;
        }
        else
        {
          if( ( endSourceDataItemIndex + 1 ) != sourceDataItemIndex )
          {
            visitor.Visit( sourceContext, startSourceDataItemIndex, endSourceDataItemIndex, ref stopVisit );

            if( stopVisit )
              return;

            startSourceDataItemIndex = sourceDataItemIndex;
          }
        }

        endSourceDataItemIndex = sourceDataItemIndex + Math.Min( maxIndex - minIndex, itemsNode.Items.Count - 1 );
        return;
      }

      int masterIndex;
      int detailStartIndex;
      int detailNodeIndex;

      while( runningIndex <= maxIndex )
      {
        DetailGeneratorNode detailNode = itemsNode.GetDetailNodeForIndex( runningIndex, out masterIndex, out detailStartIndex, out detailNodeIndex );

        if( detailNode != null )
        {
          int detailEndIndex = Math.Min( detailNode.ItemCount - 1, detailStartIndex + ( maxIndex - runningIndex ) );
          sourceDataItemIndex -= detailStartIndex;
          bool visitWasStopped;

          ( ( IDataGridContextVisitable )detailNode.DetailGenerator ).AcceptVisitor(
            detailStartIndex, detailEndIndex, visitor, visitorType, visitDetails, out visitWasStopped );

          stopVisit = stopVisit || visitWasStopped;

          if( stopVisit )
            break;

          runningIndex += detailNode.ItemCount - detailStartIndex - 1;
        }
        else
        {
          // set the first data index that will be visited for that items block
          if( startSourceDataItemIndex == -1 )
          {
            startSourceDataItemIndex = sourceDataItemIndex;
            endSourceDataItemIndex = sourceDataItemIndex;
          }
          else
          {
            if( ( endSourceDataItemIndex + 1 ) != sourceDataItemIndex )
            {
              visitor.Visit( sourceContext, startSourceDataItemIndex, endSourceDataItemIndex, ref stopVisit );

              if( stopVisit )
                break;

              startSourceDataItemIndex = sourceDataItemIndex;
            }

            endSourceDataItemIndex = sourceDataItemIndex;
          }

          sourceDataItemIndex++;
        }

        runningIndex++;
      }
    }

    private static void ProcessItemsNodeVisit(
      ItemsGeneratorNode itemsNode,
      DataGridContext sourceContext,
      int minIndex,
      int maxIndex,
      IDataGridContextVisitor visitor,
      DataGridContextVisitorType visitorType,
      bool visitDetails,
      int sourceDataItemIndex,
      ref bool stopVisit )
    {
      var runningIndex = minIndex;
      sourceDataItemIndex += minIndex;

      int masterIndex;
      int detailStartIndex;
      int detailNodeIndex;

      while( runningIndex <= maxIndex )
      {
        var detailNode = itemsNode.GetDetailNodeForIndex( runningIndex, out masterIndex, out detailStartIndex, out detailNodeIndex );

        if( detailNode != null )
        {
          var detailEndIndex = Math.Min( detailNode.ItemCount - 1, detailStartIndex + ( maxIndex - runningIndex ) );
          sourceDataItemIndex -= detailStartIndex;

          if( visitDetails )
          {
            bool visitWasStopped;

            ( ( IDataGridContextVisitable )detailNode.DetailGenerator ).AcceptVisitor( detailStartIndex, detailEndIndex, visitor, visitorType, visitDetails, out visitWasStopped );

            stopVisit = stopVisit || visitWasStopped;

            if( stopVisit )
              break;

            runningIndex += detailNode.ItemCount - detailStartIndex - 1;
          }
        }
        else
        {
          if( ( visitorType & DataGridContextVisitorType.Items ) == DataGridContextVisitorType.Items )
          {
            object dataItem = itemsNode.GetAt( runningIndex );
            visitor.Visit( sourceContext, sourceDataItemIndex, dataItem, ref stopVisit );

            if( stopVisit )
              break;
          }

          sourceDataItemIndex++;
        }

        runningIndex++;
      }
    }

    private static void ProcessHeadersNodeVisit( HeadersFootersGeneratorNode headersNode, DataGridContext sourceContext, int minIndex, int maxIndex, IDataGridContextVisitor visitor, ref bool stopVisit )
    {
      for( int i = minIndex; i <= maxIndex; i++ )
      {
        var template = headersNode.GetAt( i );

        var groupNode = headersNode.Parent as GroupGeneratorNode;
        if( groupNode != null )
        {
          visitor.Visit( sourceContext, ( GroupHeaderFooterItem )template, ref stopVisit );
        }
        else
        {
          visitor.Visit( sourceContext, ( DataTemplate )template, ref stopVisit );
        }

        if( stopVisit )
          break;
      }
    }

    private static bool MoveToNext( ref State state )
    {
      return GeneratorNodeHelper.MoveToNext( ref state, 1 );
    }

    private static bool MoveToNext( ref State state, int count )
    {
      if( count <= 0 )
        return true;

      var node = state.Node;
      var index = state.Index;
      var dataIndex = state.DataIndex;

      for( int i = 0; i < count; i++ )
      {
        var nextNode = node.Next;
        if( nextNode == null )
          return false;

        var groupNode = node as GroupGeneratorNode;
        if( groupNode != null )
        {
          dataIndex += groupNode.TotalLeafCount;
        }

        index += node.ItemCount;
        node = nextNode;
      }

      state.Node = node;
      state.Index = index;
      state.DataIndex = dataIndex;

      return true;
    }

    private static bool MoveToPrevious( ref State state )
    {
      var node = state.Node.Previous;
      if( node == null )
        return false;

      state.Node = node;
      state.Index -= node.ItemCount;

      var groupNode = node as GroupGeneratorNode;
      if( groupNode != null )
      {
        state.DataIndex -= groupNode.TotalLeafCount;
      }

      return true;
    }

    private static void MoveToFirst( ref State state )
    {
      var node = state.Node.Previous;
      if( node == null )
        return;

      var index = state.Index;
      var dataIndex = state.DataIndex;

      while( true )
      {
        var groupNode = node as GroupGeneratorNode;
        if( groupNode != null )
        {
          dataIndex -= groupNode.TotalLeafCount;
        }

        index -= node.ItemCount;

        var target = node.Previous;
        if( target == null )
          break;

        node = target;
      }

      state.Node = node;
      state.Index = index;
      state.DataIndex = dataIndex;
    }

    private static void MoveToEnd( ref State state )
    {
      var node = state.Node;
      if( node.Next == null )
        return;

      var index = state.Index;
      var dataIndex = state.DataIndex;

      while( true )
      {
        var groupNode = node as GroupGeneratorNode;
        if( groupNode != null )
        {
          dataIndex += groupNode.TotalLeafCount;
        }

        index += node.ItemCount;

        var target = node.Next;
        if( target == null )
          break;

        node = target;
      }

      state.Node = node;
      state.Index = index;
      state.DataIndex = dataIndex;
    }

    private static bool MoveToChild( ref State state )
    {
      return GeneratorNodeHelper.MoveToChild( ref state, true );
    }

    private static bool MoveToChild( ref State state, bool skipItemLessGroupNodes )
    {
      var groupNode = state.Node as GroupGeneratorNode;
      if( ( groupNode == null ) || ( groupNode.Child == null ) )
        return false;

      if( ( skipItemLessGroupNodes ) && ( groupNode.ItemCount <= 0 ) )
        return false;

      state.Node = groupNode.Child;

      return true;
    }

    private static bool MoveToParent( ref State state )
    {
      if( state.Node.Level == 0 )
        return false;

      GeneratorNodeHelper.MoveToFirst( ref state );

      state.Node = state.Node.Parent;
      Debug.Assert( state.Node != null );

      return true;
    }

    private static bool MoveToFollowing( ref State state )
    {
      while( !GeneratorNodeHelper.MoveToNext( ref state ) )
      {
        if( !GeneratorNodeHelper.MoveToParent( ref state ) )
          return false;
      }

      return true;
    }

    private static bool MoveToPreceding( ref State state )
    {
      while( !GeneratorNodeHelper.MoveToPrevious( ref state ) )
      {
        if( !GeneratorNodeHelper.MoveToParent( ref state ) )
          return false;
      }

      return true;
    }

    private static State FindNodeLocation( GeneratorNode node )
    {
      var state = new State( node, 0, 0 );

      while( true )
      {
        GeneratorNodeHelper.MoveToFirst( ref state );

        if( !GeneratorNodeHelper.MoveToParent( ref state ) )
          break;
      }

      Debug.Assert( state.Index <= 0 );
      Debug.Assert( state.DataIndex <= 0 );

      return new State( node, -state.Index, -state.DataIndex );
    }

    private static bool FindNode( ref State state, GeneratorNode node )
    {
      if( state.Node == node )
        return true;

      var from = state;
      var fromLevel = state.Node.Level;
      var to = new State( node, 0, 0 );
      var toLevel = node.Level;

      while( from.Node != to.Node )
      {
        if( fromLevel > toLevel )
        {
          if( !GeneratorNodeHelper.MoveToParent( ref from ) )
            return false;

          fromLevel = from.Node.Level;
        }
        else
        {
          if( !GeneratorNodeHelper.MoveToPrevious( ref to ) )
          {
            if( !GeneratorNodeHelper.MoveToParent( ref to ) )
              return false;

            toLevel = to.Node.Level;
          }
        }
      }

      state.Node = node;
      state.Index = from.Index - to.Index;
      state.DataIndex = from.DataIndex - to.DataIndex;

      return true;
    }

    private static bool FindNodeForIndex( ref State state, int index )
    {
      var current = state;

      // The algo is a single drill-down... for optimized performance..
      // It first tries to go horizontally in the tree and then drills-down if it can.
      while( true )
      {
        var itemCount = current.Node.ItemCount;

        // Verify if the current node contains ( directly or its children ) the required index
        if( index < current.Index + itemCount )
        {
          // A Group node is the only node that can have children is by definition empty... let's dig deeper.
          // If we cannot travel deeper, then this node is the closest we get.
          if( !GeneratorNodeHelper.MoveToChild( ref current ) )
            break;
        }
        else if( ( index == current.Index ) && ( itemCount != 0 ) )
        {
          break;
        }
        else
        {
          if( !GeneratorNodeHelper.MoveToNext( ref current ) )
            return false;
        }
      }

      state.Node = current.Node;
      state.Index = current.Index;
      state.DataIndex = current.DataIndex;

      return true;
    }

    private State m_state;

    #region State Private Struct

    private struct State
    {
      internal State( GeneratorNode node, int index, int dataIndex )
      {
        Debug.Assert( node != null );

        this.Node = node;
        this.Index = index;
        this.DataIndex = dataIndex;
      }

      internal GeneratorNode Node;
      internal int Index;
      internal int DataIndex;
    }

    #endregion
  }
}
