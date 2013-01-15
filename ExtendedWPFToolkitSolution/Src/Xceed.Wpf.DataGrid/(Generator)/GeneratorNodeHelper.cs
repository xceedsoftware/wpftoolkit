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
using System.Windows.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class GeneratorNodeHelper
  {
    public GeneratorNodeHelper( GeneratorNode initialPointer, int index, int sourceDataIndex )
    {
      if( initialPointer == null )
        throw new ArgumentNullException( "initialPointer" );

      m_currentNode = initialPointer;
      m_index = index;
      m_sourceDataIndex = sourceDataIndex;
    }

    //-------------
    // Properties

    public GeneratorNode CurrentNode
    {
      get
      {
        return m_currentNode;
      }
    }

    public int Index
    {
      get
      {
        return m_index;
      }
    }

    public int SourceDataIndex
    {
      get
      {
        return m_sourceDataIndex;
      }
    }

    //-------------
    // Methods

    public bool MoveToNext()
    {
      if( m_currentNode.Next != null )
      {
        GroupGeneratorNode groupNode = m_currentNode as GroupGeneratorNode;

        if( groupNode != null )
        {
          m_sourceDataIndex += groupNode.TotalLeafCount;
        }

        m_index += m_currentNode.ItemCount;
        m_currentNode = m_currentNode.Next;
        return true;
      }

      return false;
    }

    public bool MoveToNextBy( int count )
    {
      GeneratorNode originalNode = m_currentNode;
      int originalIndex = m_index;
      int originalSourceDataIndex = m_sourceDataIndex;

      for( int i = 0; i < count; i++ )
      {
        //if we are not capable of moving forward to the item specified, problem.... throw.
        if( !this.MoveToNext() )
        {
          m_currentNode = originalNode;
          m_index = originalIndex;
          m_sourceDataIndex = originalSourceDataIndex;
          return false;
        }
      }

      return true;
    }

    //internal use only
    public bool MoveToPrevious()
    {
      if( m_currentNode.Previous != null )
      {
        m_currentNode = m_currentNode.Previous;
        GroupGeneratorNode groupNode = m_currentNode as GroupGeneratorNode;

        if( groupNode != null )
        {
          m_sourceDataIndex -= groupNode.TotalLeafCount;
        }

        m_index -= m_currentNode.ItemCount;
        return true;
      }

      return false;
    }

    public bool MoveToFirst()
    {
      //this function stays at the same level and moves to the first node
      while( this.MoveToPrevious() )
      {
      }

      return true;
    }

    public bool MoveToEnd()
    {
      //this function stays at the same level and moves to the end
      while( this.MoveToNext() )
      {
      }

      return true;
    }

    public bool MoveToChild()
    {
      return this.MoveToChild( true );
    }

    public bool MoveToChild( bool skipItemLessGroupNodes )
    {
      GroupGeneratorNode groupNode = m_currentNode as GroupGeneratorNode;

      if( ( groupNode == null ) || ( groupNode.Child == null ) )
        return false;

      if( ( skipItemLessGroupNodes ) && ( !( groupNode.ItemCount > 0 ) ) )
        return false;

      m_currentNode = groupNode.Child;
      return true;
    }

    public bool MoveToParent()
    {
      GeneratorNode originalNode = m_currentNode;
      int originalIndex = m_index;
      int originalSourceDataIndex = m_sourceDataIndex;

      //move until we arrive at the first of the linked list.
      while( this.MoveToPrevious() )
      {
      }

      //if the first does not have a parent, then... error.
      if( m_currentNode.Parent != null )
      {
        m_currentNode = m_currentNode.Parent;
        return true;
      }

      //if there was an error, revert to the original node;
      m_currentNode = originalNode;
      m_index = originalIndex;
      m_sourceDataIndex = originalSourceDataIndex;
      return false;
    }

    public bool MoveToFollowing()
    {
      bool retval = false;
      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_currentNode, m_index, m_sourceDataIndex );

      while( !retval )
      {
        retval = nodeHelper.MoveToNext();

        if( !retval )
        {
          if( !nodeHelper.MoveToParent() )
          {
            //cannot move to parent and could not move to next, this is the end of the chain.
            break;
          }
        }
      }

      if( retval )
      {
        m_currentNode = nodeHelper.CurrentNode;
        m_index = nodeHelper.Index;
        m_sourceDataIndex = nodeHelper.SourceDataIndex;
      }

      return retval;
    }

    public bool InsertAfter( GeneratorNode insert )
    {
      if( insert == null )
        throw new DataGridInternalException();

      int insertionCount;
      int chainLength;
      GeneratorNode insertLast = GeneratorNodeHelper.EvaluateChain( insert, out insertionCount, out chainLength );

      if( m_currentNode.Next != null )
      {
        m_currentNode.Next.Previous = insertLast;
      }

      insertLast.Next = m_currentNode.Next;
      insert.Previous = m_currentNode;
      m_currentNode.Next = insert;

      // Move the current node to the last node inserted
      if( !this.MoveToNextBy( chainLength ) )
        throw new DataGridInternalException();

      return true;
    }

    public bool InsertBefore( GeneratorNode insert )
    {
      if( insert == null )
        throw new DataGridInternalException();

      int insertionCount;
      int chainLength;
      GeneratorNode insertLast = GeneratorNodeHelper.EvaluateChain( insert, out insertionCount, out chainLength );
      GeneratorNode previous = m_currentNode.Previous;

      if( previous != null )
      {
        previous.Next = insert;
      }

      insert.Previous = previous;
      m_currentNode.Previous = insertLast;
      insertLast.Next = m_currentNode;

      GroupGeneratorNode parentGroup = insert.Parent as GroupGeneratorNode;

      if( parentGroup != null )
      {
        if( parentGroup.Child == m_currentNode )
        {
          parentGroup.Child = insert;
        }
      }

      // Move the current to the first item inserted.
      // No need to change m_index, m_sourceDataIndex since they will still be with the correct value.
      m_currentNode = insert;
      return true;
    }

    public static GeneratorNode EvaluateChain( GeneratorNode chainStart, out int totalChildCount, out int chainLength )
    {
      //if we insert a chain of nodes, this GeneratorNodeHelper will help us
      GeneratorNodeHelper newHelper = new GeneratorNodeHelper( chainStart, 0, 0 );

      //first determine the total number of childs from this "node"
      totalChildCount = 0;
      chainLength = 0;

      do
      {
        totalChildCount += newHelper.CurrentNode.ItemCount;
        chainLength++;
      }
      while( newHelper.MoveToNext() );

      //then, since we moved at the end of the "chain"
      return newHelper.CurrentNode;
    }

    //FindItem skips over itemless nodes.
    public int FindItem( object item )
    {
      //finding items can only be done in "forward" direction
      int retval = -1;

      GeneratorNode originalNode = m_currentNode;
      int originalIndex = m_index;
      int originalSourceDataIndex = m_sourceDataIndex;

      while( retval == -1 )
      {
        ItemsGeneratorNode itemsNode = m_currentNode as ItemsGeneratorNode;

        if( itemsNode != null )
        {
          int tmpIndex = itemsNode.Items.IndexOf( item );
          if( tmpIndex > -1 )
          {
            tmpIndex += itemsNode.CountDetailsBeforeDataIndex( tmpIndex );
            //item is directly from this items node... then return the appropriate index!
            retval = m_index + tmpIndex;
            break;
          }
          else
          {
            //if the item is from a detail, then I don't want to "use" it!!!
            retval = -1;
            //but continue looping.... to find occurances of this item somewhere else in the tree
          }
        }
        else
        {
          CollectionGeneratorNode collectionNode = m_currentNode as CollectionGeneratorNode;

          if( collectionNode != null )
          {
            int tmpIndex = collectionNode.IndexOf( item );

            if( tmpIndex != -1 )
            {
              retval = m_index + tmpIndex;
              break;
            }
          }
        }

        //if we reach this point, it's because the item we are looking
        //for is not in this node... Try to access the child
        if( this.MoveToChild() )
          continue;

        //if we reach this point, it's because we have no child...
        if( this.MoveToNext() )
          continue;

        //final try, try "advancing" to the next item.
        if( this.MoveToFollowing() )
          continue;

        //up to this, we are in an endpoint, we failed.
        break;
      }

      if( retval == -1 )
      {
        m_currentNode = originalNode;
        m_index = originalIndex;
        m_sourceDataIndex = originalSourceDataIndex;
      }

      return retval;
    }

    public object FindIndex( int index )
    {
      //WARNING: this method only searches forward

      GeneratorNode originalNode = m_currentNode;
      int originalIndex = m_index;
      int originalSourceDataIndex = m_sourceDataIndex;

      do
      {
        if( index < ( m_index + m_currentNode.ItemCount ) )
        {
          CollectionGeneratorNode itemsNode = m_currentNode as CollectionGeneratorNode;

          if( itemsNode != null )
          {
            return itemsNode.GetAt( index - m_index );
          }
          else // not an Items Node, then try to move to child...
          {
            //if it fails, then quit loop with failure
            if( !this.MoveToChild() )
            {
              break;
            }
          }
        }
        else
        {
          //if we reach this point, it's because the item we are looking
          //for is not in this node... Try to access the child

          //No need to check for childs, since the condition above would catch it (childs part of ItemCount).

          if( this.MoveToNext() )
            continue;

          //final try, try "advancing" to the next item.
          if( this.MoveToFollowing() )
            continue;

          //up to this, we are in an endpoint, we failed.
          break;
        }
      }
      while( true );

      m_currentNode = originalNode;
      m_index = originalIndex;
      m_sourceDataIndex = originalSourceDataIndex;
      return null;
    }

    public bool ReverseCalculateIndex()
    {
      // index need to be 0, as I will use the value from the index once I backtracked all the way to the root.
      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( this.CurrentNode, 0, 0 );

      while( ( nodeHelper.CurrentNode.Previous != null ) || ( nodeHelper.CurrentNode.Parent != null ) )
      {
        if( !nodeHelper.MoveToPrevious() )
        {
          nodeHelper.MoveToParent();
        }
      }

      m_index = Math.Abs( nodeHelper.Index );
      m_sourceDataIndex = Math.Abs( nodeHelper.SourceDataIndex );
      return true;
    }

    public bool FindNodeForIndex( int index )
    {
      GeneratorNode originalNode = m_currentNode;
      int originalIndex = m_index;
      int originalSourceDataIndex = m_sourceDataIndex;

      //The algo is a single drill-down... for optimized performance..
      //It first tries to go Horizontally in the tree and then drills-down it can.

      do
      {
        int itemCount = m_currentNode.ItemCount;

        //verify if the current node contains ( directly or its children ) the required index
        if( index < ( m_index + itemCount ) )
        {
          //a Group node is the only node that can Have Children is by definition empty... let's dig deeper.
          //If we try to dig deeper and fail... 
          if( !this.MoveToChild() )
          {
            //if we cannot travel deeper, then this node is the closest we get...
            return true;
          }
        }
        else if( ( index == m_index ) && ( itemCount != 0 ) )
        {
          return true;
        }
        else
        {
          //Move to the NextNode in the list... 
          if( !this.MoveToNext() )
            break;
        }
      }
      while( true );

      m_currentNode = originalNode;
      m_index = originalIndex;
      m_sourceDataIndex = originalSourceDataIndex;
      return false;
    }

    public bool MoveForward()
    {
      bool recurseGroup = true;
      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_currentNode, m_index, m_sourceDataIndex );

      do
      {
        GroupGeneratorNode groupNode = nodeHelper.CurrentNode as GroupGeneratorNode;

        if( ( groupNode == null ) && ( nodeHelper.CurrentNode != m_currentNode ) && ( nodeHelper.CurrentNode.ItemCount != 0 ) )
        {
          m_currentNode = nodeHelper.CurrentNode;
          m_index = nodeHelper.Index;
          m_sourceDataIndex = nodeHelper.SourceDataIndex;
          return true;
        }

        if( ( recurseGroup ) && ( nodeHelper.MoveToChild() ) )
          continue;

        recurseGroup = true;

        if( nodeHelper.MoveToNext() )
          continue;

        if( nodeHelper.MoveToParent() )
        {
          recurseGroup = false;
          continue;
        }

        break;
      }
      while( true );

      return false;
    }

    public bool MoveBackward()
    {
      bool recurseGroup = true;
      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_currentNode, m_index, m_sourceDataIndex );

      do
      {
        GroupGeneratorNode groupNode = nodeHelper.CurrentNode as GroupGeneratorNode;

        if( ( groupNode == null ) && ( nodeHelper.CurrentNode != m_currentNode ) && ( nodeHelper.CurrentNode.ItemCount != 0 ) )
        {
          m_currentNode = nodeHelper.CurrentNode;
          m_index = nodeHelper.Index;
          m_sourceDataIndex = nodeHelper.SourceDataIndex;
          return true;
        }

        if( ( recurseGroup ) && ( nodeHelper.MoveToChild() ) )
        {
          nodeHelper.MoveToEnd();
          continue;
        }

        recurseGroup = true;

        if( nodeHelper.MoveToPrevious() )
          continue;


        if( nodeHelper.MoveToParent() )
        {
          recurseGroup = false;
          continue;
        }

        break;
      }
      while( true );

      return false;
    }

    //Note: this function will not check for the presence of the item in the details for Items nodes.
    public bool AbsoluteFindItem( object item )
    {
      //this method will search through nodes, even those collapsed for the item 
      //finding items can only be done in "forward" direction
      GeneratorNode originalNode = m_currentNode;
      int originalIndex = m_index;
      int originalSourceDataIndex = m_sourceDataIndex;

      do
      {
        CollectionGeneratorNode itemsNode = m_currentNode as CollectionGeneratorNode;

        if( itemsNode != null )
        {
          if( itemsNode.Items.Contains( item ) )
            return true;
        }

        //if we reach this point, it's because the item we are looking
        //for is not in this node... Try to access the child
        if( this.MoveToChild() )
          continue;

        //if we reach this point, it's because we have no child...
        if( this.MoveToNext() )
          continue;

        //final try, try "advancing" to the next item.
        if( this.MoveToFollowing() )
          continue;

        //up to this, we are in an endpoint, we failed.
        break;
      } while( true );

      m_currentNode = originalNode;
      m_index = originalIndex;
      m_sourceDataIndex = originalSourceDataIndex;
      return false;
    }

    //Note: this function will not check for the presence of the group in the details for Items nodes.
    public bool FindGroup( CollectionViewGroup group )
    {
      if( group == null )
        return false;

      GeneratorNode originalNode = m_currentNode;
      int originalIndex = m_index;
      int originalSourceDataIndex = m_sourceDataIndex;

      do
      {
        GroupGeneratorNode groupNode = m_currentNode as GroupGeneratorNode;
        if( groupNode != null )
        {
          if( groupNode.CollectionViewGroup == group )
            return true;

          if( !groupNode.CollectionViewGroup.IsBottomLevel )
          {
            if( !this.MoveToChild( false ) )
            {
              break;
            }
          }
          else
          {
            if( !this.MoveToFollowing() )
            {
              break;
            }
          }
        }
        else
        {
          //There can be nothing under a non-GroupGeneratorNode, try to move Next node in the list.
          if( !this.MoveToFollowing() )
          {
            break;
          }
        }
      } while( true );

      m_currentNode = originalNode;
      m_index = originalIndex;
      m_sourceDataIndex = originalSourceDataIndex;
      return false;
    }

    //This method cannot be used for groups.
    //This method will search for items independently of the Expanded/Collpased status of GroupGeneratorNodes
    public bool Contains( object item )
    {
      bool skipCollectionGeneratorNodeCheck = false;

      do
      {
        HeadersFootersGeneratorNode headersFootersNode = m_currentNode as HeadersFootersGeneratorNode;
        skipCollectionGeneratorNodeCheck = false;

        //If the node is a HeadersFootersGeneratorNode, do some specific handling.
        if( headersFootersNode != null )
        {
          //If the item passed to the function is a GroupHeaderFooterItem, then its because we are looking for a GroupHeader/Footer       
          if( item.GetType() == typeof( GroupHeaderFooterItem ) )
          {
            GroupHeaderFooterItem groupHeaderFooterItem = ( GroupHeaderFooterItem )item;

            //Determine the parent node/collectionViewGroup
            GroupGeneratorNode parentGroup = headersFootersNode.Parent as GroupGeneratorNode;

            if( parentGroup != null )
            {
              if( groupHeaderFooterItem.Group == parentGroup.CollectionViewGroup )
              {
                if( headersFootersNode.Items.Contains( groupHeaderFooterItem.Template ) )
                  return true;
              }
            }
            //If there is no parent node, then its because the current HeadersFootersGeneratorNode is not a GroupHeaders/Footers node (FixedHeaders/Fotoers or Headers/Footers).

            skipCollectionGeneratorNodeCheck = true; //force skip CollectionGeneratorNode verification, this is to limit amount of job done by loop body.
          }

          //If the item passed is not a GroupHeaderFooterItem, not need to do specific processing, reverting to "Common" algo.
        }

        if( !skipCollectionGeneratorNodeCheck )
        {
          CollectionGeneratorNode collectionNode = m_currentNode as CollectionGeneratorNode;

          if( collectionNode != null )
          {
            // When dealing with a DataView, the DataView's IList's Contains implementation will return false 
            // for a dataRowView which is in edition and was modified even though it is really part of the collection.
            // Therefore, we must use a for loop of Object.Equals method calls.
            System.Data.DataRowView dataRowViewItem = item as System.Data.DataRowView;

            if( dataRowViewItem != null )
            {
              IList items = collectionNode.Items;
              int itemsCount = items.Count;

              System.Data.DataRow itemDataRow = dataRowViewItem.Row;

              for( int i = 0; i < itemsCount; i++ )
              {
                System.Data.DataRowView currentDataRowView = items[ i ] as System.Data.DataRowView;

                if( ( currentDataRowView != null ) && ( itemDataRow == currentDataRowView.Row ) )
                  return true;
              }
            }
            else
            {
              //Since the GetAt() methods can be overriden to compensate for the Expand/Collapse status of Groups
              // AND the details features, accessing the collection directly prevent pre-processing of the content of the collection node.
              if( collectionNode.Items.Contains( item ) )
              {
                //if the item is from a detail, then I don't want to "use" it!!!
                return true;
              }
            }
          }
        }

        //if we reach this point, it's because the item we are looking
        //for is not in this node... Try to access the child
        //Note: Since I want to search independently of the Expand/Collapse status,
        //pass false to the method to systematically visit childs.
        if( this.MoveToChild( false ) )
          continue;

        //if we reach this point, it's because we have no child...
        if( this.MoveToNext() )
          continue;

        //final try, try "advancing" to the next item.
        if( this.MoveToFollowing() )
          continue;

        //up to this, we are in an endpoint, we failed.
        break;
      } while( true );

      return false;
    }

    //-------------
    // Data Members

    private GeneratorNode m_currentNode; // = null
    private int m_index;
    private int m_sourceDataIndex;

    public void ProcessVisit(
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
      int startSourceDataItemIndex = -1;
      int endSourceDataItemIndex = -1;

      if( minIndex < 0 )
        throw new ArgumentException( "The minimum index must be greater than or equal to zero." );

      if( ( visitorType & DataGridContextVisitorType.DataGridContext ) == DataGridContextVisitorType.DataGridContext )
      {
        visitor.Visit( sourceContext, ref visitWasStopped );

        if( visitWasStopped )
          return;
      }

      //Take a shortcut, if the visit is made only for contexts, and there is no child contexts
      //return right away.
      bool containsDetails = false;

      foreach( DataGridContext childContext in sourceContext.GetChildContexts() )
      {
        containsDetails = true;
        break;
      }

      bool processed = false;

      do
      {
        //resets the flag that indicates if the node was already processed
        processed = false;

        int itemCount = this.CurrentNode.ItemCount;

        //If the whole current node is below the minIndex, jump over it.
        if( ( this.Index + ( itemCount - 1 ) ) < minIndex )
        {
          processed = true;
        }

        //when the index to visit exceeds the range defined, exit the loop.
        if( this.Index > maxIndex )
          break;

        int minForNode = Math.Max( 0, minIndex - this.Index ); // this will give the base offset within the node where to start the visitating!
        int maxForNode = Math.Min( itemCount - 1, maxIndex - this.Index ); //this will five the max offset within this node to visit (protected against overlfow )

        if( !processed )
        {
          HeadersFootersGeneratorNode headersNode = this.CurrentNode as HeadersFootersGeneratorNode;

          if( headersNode != null )
          {
            bool isHeaderFooter = ( headersNode.Parent == null );

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
          ItemsGeneratorNode itemsNode = this.CurrentNode as ItemsGeneratorNode;

          if( itemsNode != null )
          {
            if( ( visitorType & DataGridContextVisitorType.ItemsBlock ) == DataGridContextVisitorType.ItemsBlock )
            {
              GeneratorNodeHelper.ProcessItemsNodeBlockVisit(
                itemsNode, sourceContext,
                minForNode, maxForNode,
                visitor, visitorType, visitDetails, containsDetails, m_sourceDataIndex,
                ref startSourceDataItemIndex, ref endSourceDataItemIndex, ref visitWasStopped );
            }
            else if( ( ( visitDetails ) && ( containsDetails ) )
              || ( ( visitorType & DataGridContextVisitorType.Items ) == DataGridContextVisitorType.Items ) )
            {
              GeneratorNodeHelper.ProcessItemsNodeVisit(
                itemsNode, sourceContext,
                minForNode, maxForNode,
                visitor, visitorType, visitDetails, m_sourceDataIndex, ref visitWasStopped );
            }

            processed = true;
          }
        }

        if( !processed )
        {
          GroupGeneratorNode groupNode = this.CurrentNode as GroupGeneratorNode;

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
          throw new DataGridInternalException();

        if( visitWasStopped )
          break;

        if( this.MoveToChild() )
          continue;

        if( this.MoveToFollowing() )
          continue;

        break;
      }
      while( true ); //loop is controled by continue and break statements.


      if( ( visitorType & DataGridContextVisitorType.ItemsBlock ) == DataGridContextVisitorType.ItemsBlock )
      {
        if( startSourceDataItemIndex != -1 )
        {
          bool stopVisit = false;
          visitor.Visit( sourceContext, startSourceDataItemIndex, endSourceDataItemIndex, ref stopVisit );
          visitWasStopped |= stopVisit;
        }
      }
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

          stopVisit |= visitWasStopped;

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
      int runningIndex = minIndex;
      sourceDataItemIndex += minIndex;

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

          if( visitDetails )
          {
            bool visitWasStopped;

            ( ( IDataGridContextVisitable )detailNode.DetailGenerator ).AcceptVisitor( 
              detailStartIndex, detailEndIndex, visitor, visitorType, visitDetails, out visitWasStopped );

            stopVisit |= visitWasStopped;

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
        object template = headersNode.GetAt( i );

        GroupGeneratorNode groupNode = headersNode.Parent as GroupGeneratorNode;
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

  }
}
