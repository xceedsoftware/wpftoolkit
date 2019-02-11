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
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal partial class HeadersFootersGeneratorNode : CollectionGeneratorNode
  {
    #region STATIC MEMBERS

    internal static HeadersFootersGeneratorNode GetSameLevelFirstHeaderNode( GroupGeneratorNode generatorNode )
    {
      HeadersFootersGeneratorNode headerGeneratorNode = null;

      GroupGeneratorNode parentGroupGeneratorNode = generatorNode.Parent as GroupGeneratorNode;

      if( parentGroupGeneratorNode == null )
      {
        GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( generatorNode, 0, 0 ); //index is not important

        if( nodeHelper.MoveToFirst() )
          headerGeneratorNode = nodeHelper.CurrentNode as HeadersFootersGeneratorNode;
      }
      else
      {
        headerGeneratorNode = parentGroupGeneratorNode.Child as HeadersFootersGeneratorNode;
      }

      return headerGeneratorNode;
    }

    #endregion STATIC MEMBERS

    public HeadersFootersGeneratorNode( IList list, GeneratorNode parent )
      : base( list, parent )
    {
    }

    internal override int ItemCount
    {
      get
      {
        GroupGeneratorNode parentGroup = this.Parent as GroupGeneratorNode;
        if( parentGroup == null || parentGroup.IsExpanded == true )
        {
          return this.Items.Count;
        }
        else
        {
          return this.ComputeCollapsedItemCount();
        }
      }
    }

    internal override void NotifyExpansionStateChanged( bool isParentExpanded )
    {
      base.NotifyExpansionStateChanged( isParentExpanded );

      bool sequenceStarted = false;

      int startIndex = 0;
      int sequenceCount = 0;
      int totalCount = 0;

      GroupHeaderFooterItemTemplate groupHeaderTemplate;

      //cycle through all of the items of the node
      for( int i = 0; i < this.Items.Count; i++ )
      {
        //determine if the DataTemplate for the node is a GroupHeaderFooterTemplate 
        groupHeaderTemplate = this.Items[ i ] as GroupHeaderFooterItemTemplate;

        //if the Node represent a GroupHeader of GroupFooter and the template is a GroupHeaderFooterTemplate which
        //is VisibleWhenCollapsed
        if( ( groupHeaderTemplate != null ) && ( groupHeaderTemplate.VisibleWhenCollapsed == true ) )
        {
          groupHeaderTemplate.Seal();
          //send any pending message to the Generator
          if( sequenceStarted == true )
          {
            this.OnExpansionStateChanged( isParentExpanded, startIndex, sequenceCount );

            //cumulate the total number of items collapsed/expanded
            totalCount += sequenceCount;

            //reset the sequence variables.
            startIndex = 0;
            sequenceCount = 0;
            sequenceStarted = false;
          }
        }
        //the node is to be collapsed/expanded
        else
        {
          //if no sequence were started yet
          if( sequenceStarted == false )
          {
            //start it.
            sequenceCount = 0;
            startIndex = i;
            sequenceStarted = true;
          }

          //cumulate the sequence count
          sequenceCount++;

        }

      }

      //if a sequence was started but not completed (event sent), then terminate it
      if( sequenceStarted == true )
      {
        this.OnExpansionStateChanged( isParentExpanded, startIndex, sequenceCount );

        //cumulate the total number of items collapsed/expanded
        totalCount += sequenceCount;
      }

      //finally, adjust the ItemCount
      this.Parent.AdjustItemCount( ( isParentExpanded == true ) ? totalCount : -totalCount );
    }

    public override int IndexOf( object item )
    {
      int retval = -1;

      if( item != null )
      {
        GroupGeneratorNode parentGroup = this.Parent as GroupGeneratorNode;

        //if the node is the child of a GroupGeneratorNode (then its a GroupHeader or GroupFooter).
        if( parentGroup != null )
        {
          //Debug.Assert( ( item.GetType() == typeof( GroupHeaderFooterItem ) ), "item must be a GroupHeaderFooterItem for HeadersFootersGeneratorNode whose parent is a GroupGeneratorNode" );

          if( item.GetType() == typeof( GroupHeaderFooterItem ) )
          {
            //only process further is the ParentGroup match the requested group.
            GroupHeaderFooterItem ghf = ( GroupHeaderFooterItem )item;
            if( ghf.Group == parentGroup.CollectionViewGroup )
            {
              retval = this.IndexOfHelper( parentGroup, ghf.Template );
            }
          }
          else
          {
            retval = this.IndexOfHelper( parentGroup, item );
          }

        }
        else
        {
          //if there is no parent group (or no parent at all), then process the item directly!
          retval = this.IndexOfHelper( parentGroup, item );
        }
      }

      return retval;

    }

    public override object GetAt( int index )
    {
      var parentGroup = this.Parent as GroupGeneratorNode;
      object item;

      if( ( parentGroup != null ) && !parentGroup.IsExpanded )
      {
        item = this.ComputeCollapsedGetAt( index );
      }
      else
      {
        item = base.GetAt( index );
      }

      if( ( parentGroup != null ) && ( item != null ) )
        return new GroupHeaderFooterItem( parentGroup.CollectionViewGroup, item );

      return item;
    }

    #region ImmediateUIGroups

    internal ReadOnlyCollection<Group> GetImmediateUIGroups( int generatorCurrentGeneration )
    {
      if( ( m_cachedGeneratorCurrentGeneration != generatorCurrentGeneration )
        || ( m_readOnlyImmediateUIGroups == null ) )
      {
        Debug.WriteLineIf( ( ( m_cachedGeneratorCurrentGeneration != generatorCurrentGeneration ) && ( m_readOnlyImmediateUIGroups == null ) ),
          "Creating Groups collection since generator version differs AND ReadOnlyCollection is null." );

        Debug.WriteLineIf( ( ( m_cachedGeneratorCurrentGeneration != generatorCurrentGeneration ) && ( m_readOnlyImmediateUIGroups != null ) ),
          "Creating Groups collection since generator version differs." );

        Debug.WriteLineIf( ( ( m_cachedGeneratorCurrentGeneration == generatorCurrentGeneration ) && ( m_readOnlyImmediateUIGroups == null ) ),
          "Creating Groups collection even if generator version is the same, since ReadOnlyCollection is null." );

        m_cachedGeneratorCurrentGeneration = generatorCurrentGeneration;

        // Ensure collections.
        if( m_immediateUIGroups == null )
        {
          Debug.Assert( m_readOnlyImmediateUIGroups == null );
          m_immediateUIGroups = new Collection<Group>();
          m_readOnlyImmediateUIGroups = new ReadOnlyCollection<Group>( m_immediateUIGroups );
        }
        else
        {
          Debug.Assert( m_readOnlyImmediateUIGroups != null );
          m_immediateUIGroups.Clear();
        }

        // Recalculate.
        GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( this, 0, 0 ); //index is not important

        while( nodeHelper.MoveToNext() )
        {
          GroupGeneratorNode groupGeneratorNode = nodeHelper.CurrentNode as GroupGeneratorNode;

          if( groupGeneratorNode == null )
            continue;

          m_immediateUIGroups.Add( groupGeneratorNode.UIGroup );
        }
      }

      return m_readOnlyImmediateUIGroups;
    }

    private int m_cachedGeneratorCurrentGeneration;
    private Collection<Group> m_immediateUIGroups;
    private ReadOnlyCollection<Group> m_readOnlyImmediateUIGroups;

    #endregion ImmediateUIGroups

    private int ComputeCollapsedItemCount()
    {
      int retval = 0;

      GroupHeaderFooterItemTemplate groupHeaderTemplate;

      for( int i = 0; i < this.Items.Count; i++ )
      {
        //determine if the DataTemplate for the node is a GroupHeaderFooterTemplate 
        groupHeaderTemplate = this.Items[ i ] as GroupHeaderFooterItemTemplate;

        //if the Node represent a GroupHeader of GroupFooter and the template is a GroupHeaderFooterTemplate which
        //is VisibleWhenCollapsed
        if( ( groupHeaderTemplate != null ) && ( groupHeaderTemplate.VisibleWhenCollapsed == true ) )
        {
          groupHeaderTemplate.Seal();
          retval++;
        }
      }

      return retval;
    }

    private int ComputeCollapsedIndexOf( object item )
    {
      int retval = -1;
      int count = -1;

      GroupHeaderFooterItemTemplate groupHeaderTemplate;

      for( int i = 0; i < this.Items.Count; i++ )
      {
        //determine if the DataTemplate for the node is a GroupHeaderFooterTemplate 
        groupHeaderTemplate = this.Items[ i ] as GroupHeaderFooterItemTemplate;

        //if the Node represent a GroupHeader of GroupFooter and the template is a GroupHeaderFooterTemplate which
        //is VisibleWhenCollapsed
        if( ( groupHeaderTemplate != null ) && ( groupHeaderTemplate.VisibleWhenCollapsed == true ) )
        {
          groupHeaderTemplate.Seal();
          count++;
        }

        if( this.Items[ i ] == item )
        {
          retval = count;
          break;
        }
      }

      return retval;
    }

    private object ComputeCollapsedGetAt( int index )
    {
      object retval = null;

      GroupHeaderFooterItemTemplate groupHeaderTemplate;
      int count = -1;

      for( int i = 0; i < this.Items.Count; i++ )
      {
        //determine if the DataTemplate for the node is a GroupHeaderFooterTemplate 
        groupHeaderTemplate = this.Items[ i ] as GroupHeaderFooterItemTemplate;

        //if the Node represent a GroupHeader of GroupFooter and the template is a GroupHeaderFooterTemplate which
        //is VisibleWhenCollapsed
        if( ( groupHeaderTemplate != null ) && ( groupHeaderTemplate.VisibleWhenCollapsed == true ) )
        {
          groupHeaderTemplate.Seal();
          count++;
        }

        if( count == index )
        {
          retval = this.Items[ i ];
          break;
        }
      }

      return retval;
    }

    private int IndexOfHelper( GroupGeneratorNode parentGroup, object template )
    {
      int retval = -1;

      if( parentGroup != null && parentGroup.IsExpanded == false )
      {
        retval = this.ComputeCollapsedIndexOf( template );
      }
      else
      {
        retval = base.IndexOf( template );
      }

      return retval;
    }
  }
}
