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
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal partial class GroupGeneratorNode : NotifyCollectionChangedGeneratorNode
  {
    public GroupGeneratorNode( CollectionViewGroup group, GeneratorNode parent, GroupConfiguration groupConfig )
      : base( parent )
    {
      Debug.Assert( group != null, "group cannot be null for GroupGeneratorNode" );
      Debug.Assert( groupConfig != null );

      m_group = group;
      m_groupConfig = groupConfig;
    }

    public event EventHandler TotalLeafCountChanged;
    public event EventHandler IsExpandedChanged;
    public event EventHandler IsExpandedChanging;

    #region CollectionViewGroup Property

    public CollectionViewGroup CollectionViewGroup
    {
      get
      {
        return m_group;
      }
    }

    #endregion

    #region UIGroup Property

    public Group UIGroup
    {
      get
      {
        return m_uiGroup;
      }
      set
      {
        m_uiGroup = value;
      }
    }

    #endregion

    #region GroupConfiguration Property

    public GroupConfiguration GroupConfiguration
    {
      get
      {
        return m_groupConfig;
      }
    }

    #endregion

    #region TotalLeafCount Property

    public int TotalLeafCount
    {
      get
      {
        return m_leafCount;
      }
    }

    #endregion

    #region ItemCount Property

    internal override int ItemCount
    {
      get
      {
        GroupGeneratorNode parentGroup = this.Parent as GroupGeneratorNode;
        if( parentGroup != null )
        {
          return ( parentGroup.IsExpanded == true ) ? m_itemCount : 0;
        }
        return m_itemCount;
      }
    }

    #endregion

    #region IsExpanded Property

    public bool IsExpanded
    {
      get
      {
        return m_isExpanded;
      }
      set
      {
        //if the expanded status changed
        if( m_isExpanded != value )
        {
          var dataGridContext = m_uiGroup.DataGridContext;
          var groupExpansionChangingEventArgs = default( GroupExpansionChangingEventArgs );
          if( value )
          {
            groupExpansionChangingEventArgs = new GroupExpansionChangingEventArgs( DataGridControl.GroupExpandingEvent, m_uiGroup, m_group, dataGridContext, true );
            dataGridContext.DataGridControl.RaiseGroupExpanding( groupExpansionChangingEventArgs );
          }
          else
          {
            groupExpansionChangingEventArgs = new GroupExpansionChangingEventArgs( DataGridControl.GroupCollapsingEvent, m_uiGroup, m_group, dataGridContext, false );
            dataGridContext.DataGridControl.RaiseGroupCollapsing( groupExpansionChangingEventArgs );
          }

          if( groupExpansionChangingEventArgs.Cancel )
            return;

          var handler = this.IsExpandedChanging;
          if( handler != null )
          {
            handler.Invoke( this, EventArgs.Empty );
          }

          this.NotifyImmediateChildren( value );

          m_isExpanded = value;

          handler = this.IsExpandedChanged;
          if( handler != null )
          {
            handler.Invoke( this, EventArgs.Empty );
          }

          if( m_isExpanded )
          {
            dataGridContext.DataGridControl.RaiseGroupExpanded( new GroupExpansionChangedEventArgs( DataGridControl.GroupExpandedEvent, m_uiGroup, m_group, dataGridContext, true ) );
          }
          else
          {
            dataGridContext.DataGridControl.RaiseGroupCollapsed( new GroupExpansionChangedEventArgs( DataGridControl.GroupCollapsedEvent, m_uiGroup, m_group, dataGridContext, false ) );
          }
        }
      }
    }

    internal void SetIsExpandedAtInitialization( bool isExpanded )
    {
      m_isExpanded = isExpanded;
    }

    #endregion

    #region NamesTree Property

    public object[] NamesTree
    {
      get
      {
        return m_namesTree;
      }
    }

    #endregion

    #region Child Property

    public GeneratorNode Child
    {
      get;
      set;
    }

    #endregion

    #region IsComputedExpandedOverride Property

    protected override bool IsComputedExpandedOverride
    {
      get
      {
        return this.IsExpanded;
      }
    }

    #endregion

    public HeadersFootersGeneratorNode GetHeaderNode()
    {
      return this.Child as HeadersFootersGeneratorNode;
    }

    public HeadersFootersGeneratorNode GetFooterNode()
    {
      GeneratorNode generatorNode = this.Child;
      HeadersFootersGeneratorNode footerGeneratorNode = null;

      if( generatorNode == null )
        return null;

      do
      {
        generatorNode = generatorNode.Next;
        footerGeneratorNode = generatorNode as HeadersFootersGeneratorNode;
      } while( ( footerGeneratorNode == null ) && ( generatorNode != null ) );

      return footerGeneratorNode;
    }

    protected override int AdjustItemCountOverride( int delta )
    {
      delta = base.AdjustItemCountOverride( delta );

      m_itemCount += delta;

      GroupGeneratorNode parentGroup = this.Parent as GroupGeneratorNode;
      if( ( parentGroup != null ) && ( parentGroup.IsExpanded == false ) )
      {
        return 0;
      }

      return delta;
    }

    protected override int AdjustLeafCountOverride( int delta )
    {
      delta = base.AdjustLeafCountOverride( delta );

      if( delta != 0 )
      {
        m_leafCount += delta;

        if( this.TotalLeafCountChanged != null )
        {
          this.TotalLeafCountChanged( this, EventArgs.Empty );
        }
      }

      return delta;
    }

    internal override void NotifyExpansionStateChanged( bool isParentExpanded )
    {
      base.NotifyExpansionStateChanged( isParentExpanded );

      if( this.Parent != null )
      {
        this.OnExpansionStateChanged( isParentExpanded, 0, m_itemCount );

        if( isParentExpanded == true )
        {
          this.Parent.AdjustItemCount( m_itemCount );
        }
        else
        {
          this.Parent.AdjustItemCount( -m_itemCount );
        }
      }
    }

    internal override void CleanGeneratorNode()
    {
      base.CleanGeneratorNode();

      this.Child = null;
      this.UIGroup.ClearGroup();
      this.UIGroup = null;

      m_group = null;
    }

    internal void BuildNamesTree()
    {
      int level = this.Level;

      m_namesTree = new object[ level + 1 ];

      m_namesTree[ level ] = this.CollectionViewGroup.Name;

      GroupGeneratorNode parentNode = this.Parent as GroupGeneratorNode;
      for( int i = level - 1; i >= 0; i-- )
      {
        m_namesTree[ i ] = parentNode.CollectionViewGroup.Name;
        parentNode = parentNode.Parent as GroupGeneratorNode;
      }
    }

    private void NotifyImmediateChildren( bool value )
    {
      if( this.Child != null )
      {
        GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( this.Child, 0, 0 ); //index not important
        nodeHelper.MoveToEnd();
        do
        {
          nodeHelper.CurrentNode.NotifyExpansionStateChanged( value );
        }
        while( nodeHelper.MoveToPrevious() == true );
      }
    }

    private GroupConfiguration m_groupConfig;
    private bool m_isExpanded = true;
    private int m_leafCount; //this is the storage for the total number of "leaf" items that are child of this group. (no counting the groups, headers and footers)
    private int m_itemCount; //this is the storage for the total number of items that are "child" of this group node.
    private CollectionViewGroup m_group;
    private Group m_uiGroup;
    private object[] m_namesTree;
  }
}
