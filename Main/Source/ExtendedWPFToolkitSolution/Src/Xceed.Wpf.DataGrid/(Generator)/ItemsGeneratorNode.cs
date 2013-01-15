/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid
{
  internal partial class ItemsGeneratorNode : CollectionGeneratorNode
  {
    public ItemsGeneratorNode( IList list, GeneratorNode parent )
      : base( list, parent )
    {

    }

    internal override int ItemCount
    {
      get
      {
        GroupGeneratorNode parentGroup = this.Parent as GroupGeneratorNode;
        if( parentGroup != null )
        {
          int totalItemCount = 0;

          if( parentGroup.IsExpanded == true )
          {
            totalItemCount = this.Items.Count + this.ComputeDetailsCount();
          }

          return totalItemCount;
        }

        return this.Items.Count + this.ComputeDetailsCount();
      }
    }

    internal override void NotifyExpansionStateChanged( bool isParentExpanded )
    {
      base.NotifyExpansionStateChanged( isParentExpanded );

      if( this.Parent != null )
      {
        this.OnExpansionStateChanged( isParentExpanded, 0, this.Items.Count + this.ComputeDetailsCount() );

        if( isParentExpanded == true )
        {
          this.Parent.AdjustItemCount( this.Items.Count + this.ComputeDetailsCount() );
        }
        else
        {
          this.Parent.AdjustItemCount( -( this.Items.Count + this.ComputeDetailsCount() ) );
        }
      }
    }

    protected override int AdjustItemCountOverride( int delta )
    {
      base.AdjustItemCountOverride( delta );

      GroupGeneratorNode parentGroup = this.Parent as GroupGeneratorNode;
      if( ( parentGroup != null ) && ( parentGroup.IsExpanded == false ) )
      {
        return 0;
      }

      return delta;
    }

    public override object GetAt( int index )
    {
      if( index < 0 )
        return null;

      if( ( m_detailsMapping == null )
        || ( m_detailsMapping.Count == 0 ) )
      {
        //If there are no details, call base (quicker)
        return base.GetAt( index );
      }

      int masterIndex;
      int detailIndex;
      int detailNodeIndex;
      DetailGeneratorNode detailNode = this.GetDetailNodeForIndex( index, out masterIndex, out detailIndex, out detailNodeIndex );

      if( detailNode == null )
      {
        return this.Items[ masterIndex ];
      }

      return detailNode.DetailGenerator.ItemFromIndex( detailIndex );
    }

    public override int IndexOf( object item )
    {
      if( ( m_detailsMapping == null )
        || ( m_detailsMapping.Count == 0 ) )
      {
        return base.IndexOf( item );
      }

      int tmpIndex = this.Items.IndexOf( item );
      if( tmpIndex != -1 )
      {
        return tmpIndex + this.CountDetailsBeforeDataIndex( tmpIndex );
      }

      int runningDetailCount = 0;

      foreach( KeyValuePair<int, List<DetailGeneratorNode>> pair in m_detailsMapping )
      {
        List<DetailGeneratorNode> detailNodeList = pair.Value;

        int currentIndex = pair.Key;
        int runningIndex = ( currentIndex + runningDetailCount );

        int count = detailNodeList.Count;
        for( int i = 0; i < count; i++ )
        {
          DetailGeneratorNode detailNode = detailNodeList[ i ];
          int detailIndex = detailNode.DetailGenerator.IndexFromItem( item );

          if( detailIndex > -1 )
            return runningIndex + detailIndex + 1;

          int detailItemCount = detailNode.ItemCount;

          runningDetailCount += detailItemCount;
          runningIndex += detailItemCount;
        }
      }

      return -1;
    }

    internal override void CleanGeneratorNode()
    {
      base.CleanGeneratorNode();

      if( m_detailsMapping != null )
      {
        foreach( List<DetailGeneratorNode> detail in m_detailsMapping.Values )
        {
          detail.Clear();
        }
        m_detailsMapping.Clear();
        m_detailsMapping = null;
      }
    }

    #region Master/Detail Specific Stuff

    public SortedDictionary<int, List<DetailGeneratorNode>> Details
    {
      get
      {
        return m_detailsMapping;
      }
      set
      {
        m_detailsMapping = value;
      }
    }

    private int ComputeDetailsCount()
    {
      int retval = 0;
      if( m_detailsMapping != null )
      {
        foreach( List<DetailGeneratorNode> detailsList in m_detailsMapping.Values )
        {
          foreach( DetailGeneratorNode detail in detailsList )
          {
            retval += detail.ItemCount;
          }
        }
      }
      return retval;
    }

    internal int CountDetailsBeforeDataIndex( int dataIndex )
    {
      if( this.Details == null )
        return 0;

      int retval = 0;

      foreach( KeyValuePair<int, List<DetailGeneratorNode>> indexToDetails in m_detailsMapping )
      {
        if( indexToDetails.Key < dataIndex )
        {
          foreach( DetailGeneratorNode node in indexToDetails.Value )
          {
            retval += node.ItemCount;
          }

          continue;
        }

        // Since the details list is now a SortedDictionary, we can assume
        // that if the key is greater or equal than the reference index,
        // there will be no more entry after this.
        break;
      }

      return retval;
    }

    internal int CountDetailsBeforeGlobalIndex( int globalIndex )
    {
      if( ( m_detailsMapping == null ) || ( globalIndex == 0 ) )
        return 0;

      int retval = 0;

      foreach( KeyValuePair<int, List<DetailGeneratorNode>> indexToDetails in m_detailsMapping )
      {
        int key = indexToDetails.Key;

        // Since the details list is now a SortedDictionary, we can assume
        // that if the key is greater or equal than the reference index,
        // there will be no more entry after this.
        if( key >= globalIndex )
          break;

        foreach( DetailGeneratorNode node in indexToDetails.Value )
        {
          int itemCount = node.ItemCount;
          globalIndex -= itemCount;

          if( key >= globalIndex )
            break;

          retval += itemCount;
        }
      }

      return retval;
    }

    public DetailGeneratorNode GetDetailNodeForIndex( int targetIndex )
    {
      int masterIndex = -1;
      int detailIndex = -1;
      int detailNodeIndex = -1;
      return this.GetDetailNodeForIndex( targetIndex, out masterIndex, out detailIndex, out detailNodeIndex );
    }

    public DetailGeneratorNode GetDetailNodeForIndex( int targetIndex, out int masterIndex, out int detailIndex, out int detailNodeIndex )
    {
      masterIndex = -1;
      detailIndex = -1;
      detailNodeIndex = -1;

      if( ( m_detailsMapping == null )
        || ( m_detailsMapping.Count == 0 ) )
      {
        masterIndex = targetIndex;
        return null;
      }

      int runningDetailCount = 0;

      foreach( KeyValuePair<int, List<DetailGeneratorNode>> pair in m_detailsMapping )
      {
        List<DetailGeneratorNode> detailNodeList = pair.Value;
        int currentIndex = pair.Key;

        int runningIndex = ( currentIndex + runningDetailCount );

        if( targetIndex < runningIndex )
        {
          masterIndex = targetIndex - runningDetailCount;
          return null;
        }

        if( runningIndex == targetIndex )
        {
          masterIndex = currentIndex;
          return null;
        }

        int count = detailNodeList.Count;
        for( int i = 0; i < count; i++ )
        {
          DetailGeneratorNode detailNode = detailNodeList[ i ];
          int detailItemCount = detailNode.ItemCount;

          if( targetIndex <= ( runningIndex + detailItemCount ) )
          {
            masterIndex = currentIndex;
            detailIndex = targetIndex - runningIndex - 1;
            detailNodeIndex = i;
            return detailNode;
          }
          else
          {
            runningDetailCount += detailItemCount;
            runningIndex += detailItemCount;
          }
        }
      }

      masterIndex = targetIndex - runningDetailCount;
      return null;
    }

    private SortedDictionary<int, List<DetailGeneratorNode>> m_detailsMapping; // null

    #endregion
  }
}
