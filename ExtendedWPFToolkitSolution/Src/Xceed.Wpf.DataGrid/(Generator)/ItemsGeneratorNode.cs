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

namespace Xceed.Wpf.DataGrid
{
  internal partial class ItemsGeneratorNode : CollectionGeneratorNode
  {
    internal ItemsGeneratorNode( IList list, GeneratorNode parent )
      : base( list, parent )
    {
    }

    internal override int ItemCount
    {
      get
      {
        var parentGroup = this.Parent as GroupGeneratorNode;
        if( ( parentGroup == null ) || parentGroup.IsExpanded )
          return this.Items.Count + this.ComputeDetailsCount();

        return 0;
      }
    }

    internal override void NotifyExpansionStateChanged( bool isParentExpanded )
    {
      base.NotifyExpansionStateChanged( isParentExpanded );

      if( this.Parent != null )
      {
        this.OnExpansionStateChanged( isParentExpanded, 0, this.Items.Count + this.ComputeDetailsCount() );

        if( isParentExpanded )
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

      var parentGroup = this.Parent as GroupGeneratorNode;
      if( ( parentGroup == null ) || parentGroup.IsExpanded )
        return delta;

      return 0;
    }

    public override object GetAt( int index )
    {
      if( index < 0 )
        return null;

      //If there are no details, call base (quicker)
      if( ( m_detailsMapping == null ) || ( m_detailsMapping.Count == 0 ) )
        return base.GetAt( index );

      int masterIndex;
      int detailIndex;
      int detailNodeIndex;

      var detailNode = this.GetDetailNodeForIndex( index, out masterIndex, out detailIndex, out detailNodeIndex );
      if( detailNode == null )
        return this.Items[ masterIndex ];

      return detailNode.DetailGenerator.ItemFromIndex( detailIndex );
    }

    public override int IndexOf( object item )
    {
      if( ( m_detailsMapping == null ) || ( m_detailsMapping.Count == 0 ) )
        return base.IndexOf( item );

      var tmpIndex = this.Items.IndexOf( item );
      if( tmpIndex >= 0 )
        return tmpIndex + this.CountDetailsBeforeDataIndex( tmpIndex );

      int runningDetailCount = 0;

      foreach( KeyValuePair<int, List<DetailGeneratorNode>> pair in m_detailsMapping )
      {
        var detailNodeList = pair.Value;
        var currentIndex = pair.Key;
        var runningIndex = ( currentIndex + runningDetailCount );

        var count = detailNodeList.Count;
        for( int i = 0; i < count; i++ )
        {
          var detailNode = detailNodeList[ i ];
          var detailIndex = detailNode.DetailGenerator.IndexFromItem( item );

          if( detailIndex >= 0 )
            return runningIndex + detailIndex + 1;

          var detailItemCount = detailNode.ItemCount;

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
        foreach( var detail in m_detailsMapping.Values )
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
      if( m_detailsMapping == null )
        return 0;

      var count = 0;

      foreach( var detailsList in m_detailsMapping.Values )
      {
        foreach( var detail in detailsList )
        {
          count += detail.ItemCount;
        }
      }

      return count;
    }

    internal int CountDetailsBeforeDataIndex( int dataIndex )
    {
      if( m_detailsMapping == null )
        return 0;

      var count = 0;

      foreach( KeyValuePair<int, List<DetailGeneratorNode>> indexToDetails in m_detailsMapping )
      {
        if( indexToDetails.Key < dataIndex )
        {
          foreach( var node in indexToDetails.Value )
          {
            count += node.ItemCount;
          }

          continue;
        }

        // Since the details list is now a SortedDictionary, we can assume
        // that if the key is greater or equal than the reference index,
        // there will be no more entry after this.
        break;
      }

      return count;
    }

    internal int CountDetailsBeforeGlobalIndex( int globalIndex )
    {
      if( ( m_detailsMapping == null ) || ( globalIndex == 0 ) )
        return 0;

      var retval = 0;

      foreach( KeyValuePair<int, List<DetailGeneratorNode>> indexToDetails in m_detailsMapping )
      {
        var key = indexToDetails.Key;

        // Since the details list is now a SortedDictionary, we can assume
        // that if the key is greater or equal than the reference index,
        // there will be no more entry after this.
        if( key >= globalIndex )
          break;

        foreach( var node in indexToDetails.Value )
        {
          var itemCount = node.ItemCount;
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
      int masterIndex;
      int detailIndex;
      int detailNodeIndex;

      return this.GetDetailNodeForIndex( targetIndex, out masterIndex, out detailIndex, out detailNodeIndex );
    }

    public DetailGeneratorNode GetDetailNodeForIndex( int targetIndex, out int masterIndex, out int detailIndex, out int detailNodeIndex )
    {
      masterIndex = -1;
      detailIndex = -1;
      detailNodeIndex = -1;

      if( ( m_detailsMapping == null ) || ( m_detailsMapping.Count == 0 ) )
      {
        masterIndex = targetIndex;
        return null;
      }

      var runningDetailCount = 0;

      foreach( KeyValuePair<int, List<DetailGeneratorNode>> pair in m_detailsMapping )
      {
        var detailNodeList = pair.Value;
        var currentIndex = pair.Key;
        var runningIndex = ( currentIndex + runningDetailCount );

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

        var count = detailNodeList.Count;
        for( int i = 0; i < count; i++ )
        {
          var detailNode = detailNodeList[ i ];
          var detailItemCount = detailNode.ItemCount;

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
