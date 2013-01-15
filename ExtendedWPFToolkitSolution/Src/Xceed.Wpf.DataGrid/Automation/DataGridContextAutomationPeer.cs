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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections;
using System.Windows;
using System.Windows.Automation;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid.Automation
{
  public class DataGridContextAutomationPeer : AutomationPeer, ISelectionProvider, ITableProvider, IExpandCollapseProvider, IItemContainerProvider
  {
    internal DataGridContextAutomationPeer(
      DataGridControl dataGridControl,
      DataGridContext parentDataGridContext,
      object parentItem,
      DetailConfiguration detailConfiguration )
    {
      if( dataGridControl == null )
        throw new ArgumentNullException( "dataGridControl" );

      if( parentDataGridContext == null )
      {
        m_dataGridContext = dataGridControl.DataGridContext;
      }
      else
      {
        m_dataGridContext = parentDataGridContext.GetChildContext( parentItem, detailConfiguration );
      }

      if( m_dataGridContext != null )
        m_dataGridContext.Peer = this;

      m_dataGridControl = dataGridControl;
      m_parentDataGridContext = parentDataGridContext;
      m_parentItem = parentItem;
      m_detailConfiguration = detailConfiguration;

      m_dataChildren = new Hashtable( 0 );
      m_headerFooterChildren = new Hashtable( 0 );
    }

    #region DataGridContext Property

    public DataGridContext DataGridContext
    {
      get
      {
        return m_dataGridContext;
      }
      internal set
      {
        if( m_dataGridContext == value )
          return;

        if( ( m_oldColumnCount != -1 ) && ( m_dataGridContext != null ) )
        {
          // We only remove unsubscribe from the event, since it is when we get the child list that we 
          // subscribe to be aware of the column modification.
          m_dataGridContext.Columns.VisibleColumnsUpdated -= new EventHandler( Columns_VisibleColumnsUpdated );
          m_oldColumnCount = -1;
        }

        m_dataGridContext = value;

        this.RaiseAutomationEvent( AutomationEvents.StructureChanged );
      }
    }

    #endregion

    #region ItemPeers Property

    internal Hashtable ItemPeers
    {
      get
      {
        return m_dataChildren;
      }
    }

    #endregion

    public override object GetPattern( PatternInterface patternInterface )
    {
      switch( patternInterface )
      {
        case PatternInterface.Table:
        case PatternInterface.Grid:
        case PatternInterface.Selection:
        case PatternInterface.ExpandCollapse:
        case PatternInterface.ItemContainer:
          return this;

        default:
          return null;
      }
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
      return AutomationControlType.DataGrid;
    }

    protected virtual DataGridItemAutomationPeer CreateItemAutomationPeer( object item, int index )
    {
      if( m_dataGridContext == null )
        return null;

      return new DataGridItemAutomationPeer( item, m_dataGridContext, index );
    }

    protected override List<AutomationPeer> GetChildrenCore()
    {
      if( m_dataGridContext == null )
        return null;

      if( m_oldColumnCount == -1 )
      {
        m_oldColumnCount = m_dataGridContext.VisibleColumns.Count;
        m_dataGridContext.Columns.VisibleColumnsUpdated += new EventHandler( Columns_VisibleColumnsUpdated );
      }

      CustomItemContainerGenerator itemGenerator = m_dataGridContext.CustomItemContainerGenerator;
      IEnumerable items;
      int itemsCount;
      bool itemsIsAllItems = false;

      // The way Microsoft check if we are called by an automation that support Virtualization.
      if( ( ItemContainerPatternIdentifiers.Pattern != null ) && ( m_dataGridControl.ItemsHost is DataGridItemsHost ) )
      {
        List<object> realizedItems = itemGenerator.GetRealizedDataItems();
        items = realizedItems;
        itemsCount = realizedItems.Count;
      }
      else
      {
        CollectionView viewItems = m_dataGridContext.Items;
        items = viewItems;
        itemsCount = viewItems.Count;
        itemsIsAllItems = true;
      }

      Hashtable oldDataChildren = m_dataChildren;
      Hashtable oldHeaderFooterChildren = m_headerFooterChildren;

      // Get header count
      HeadersFootersGeneratorNode headerNode = itemGenerator.Header;
      int headersCount = ( headerNode == null ) ? 0 : headerNode.ItemCount;

      // Get footer count
      HeadersFootersGeneratorNode footerNode = itemGenerator.Footer;
      int footersCount = ( footerNode == null ) ? 0 : footerNode.ItemCount;

      int childrenCount = headersCount + footersCount;

      if( m_parentDataGridContext == null )
      {
        // Add the fixed header / footer count to the children
        Panel fixedPanel = m_dataGridControl.FixedHeadersHostPanel;

        if( fixedPanel != null )
        {
          childrenCount += fixedPanel.Children.Count;
        }

        fixedPanel = m_dataGridControl.FixedFootersHostPanel;

        if( fixedPanel != null )
        {
          childrenCount += fixedPanel.Children.Count;
        }
      }

      ReadOnlyObservableCollection<object> groups = m_dataGridContext.Items.Groups;

      if( groups != null )
      {
        // Add the group count to the children
        childrenCount += groups.Count;
      }
      else
      {
        childrenCount += itemsCount;
      }

      m_dataChildren = new Hashtable( itemsCount );
      m_headerFooterChildren = new Hashtable( headersCount + footersCount );

      if( childrenCount == 0 )
        return null;

      List<AutomationPeer> list = new List<AutomationPeer>( childrenCount );
      this.AddChildrenHeaders( headerNode, oldHeaderFooterChildren, list );

      if( groups != null )
      {
        CustomItemContainerGenerator customItemContainerGenerator = m_dataGridContext.CustomItemContainerGenerator;
        DataGridGroupAutomationPeer peer;
        itemsCount = groups.Count;

        for( int i = 0; i < itemsCount; i++ )
        {
          CollectionViewGroup collectionViewGroup = groups[ i ] as CollectionViewGroup;

          if( collectionViewGroup != null )
          {
            Group uiGroup =
              customItemContainerGenerator.GetGroupFromCollectionViewGroup( null, collectionViewGroup );

            if( uiGroup != null )
            {
              peer = uiGroup.CreateAutomationPeer() as DataGridGroupAutomationPeer;

              if( peer != null )
                list.Add( peer );
            }
          }
        }
      }
      else
      {
        int index = 0;

        foreach( object item in ( IEnumerable )items )
        {
          DataGridItemAutomationPeer itemAutomationPeer =
            oldDataChildren[ item ] as DataGridItemAutomationPeer;

          if( itemAutomationPeer == null )
          {
            if( itemsIsAllItems )
            {
              itemAutomationPeer = this.CreateItemAutomationPeer( item, index );
              index++;
            }
            else
            {
              itemAutomationPeer = this.CreateItemAutomationPeer( item, -1 );
            }
          }
          else
          {
            if( itemsIsAllItems )
            {
              itemAutomationPeer.SetIndex( index );
              index++;
            }
            else
            {
              itemAutomationPeer.SetIndex( -1 );
            }
          }

          // Force EventsSource to be updated
          itemAutomationPeer.GetWrapperPeer();

          if( m_dataChildren[ item ] == null )
          {
            list.Add( itemAutomationPeer );
            m_dataChildren[ item ] = itemAutomationPeer;
          }
        }
      }

      this.AddChildrenFooters( footerNode, oldHeaderFooterChildren, list );
      return list;
    }

    protected override string GetAcceleratorKeyCore()
    {
      return string.Empty;
    }

    protected override string GetAccessKeyCore()
    {
      return string.Empty;
    }

    protected override string GetAutomationIdCore()
    {
      string automationId = null;

      if( m_dataGridContext != null )
      {
        QueryAutomationIdRoutedEventArgs args = new QueryAutomationIdRoutedEventArgs(
          AutomationQueryEvents.QueryAutomationIdForDetailEvent, m_dataGridContext );

        m_dataGridControl.RaiseEvent( args );

        if( args.Handled )
          automationId = args.AutomationId;
      }

      if( automationId == null )
        return "Detail_" + m_detailConfiguration.RelationName;

      return automationId;
    }

    protected override Rect GetBoundingRectangleCore()
    {
      return Rect.Empty;
    }

    protected override string GetClassNameCore()
    {
      return "DataGridContext";
    }

    protected override Point GetClickablePointCore()
    {
      return new Point( double.NaN, double.NaN );
    }

    protected override string GetHelpTextCore()
    {
      if( m_dataGridContext == null )
        return string.Empty;

      QueryHelpTextRoutedEventArgs args = new QueryHelpTextRoutedEventArgs(
        AutomationQueryEvents.QueryHelpTextForDetailEvent, m_dataGridContext );

      m_dataGridControl.RaiseEvent( args );
      string helpText = null;

      if( args.Handled )
        helpText = args.HelpText;

      return ( helpText ?? string.Empty );
    }

    protected override string GetItemStatusCore()
    {
      if( m_dataGridContext == null )
        return string.Empty;

      QueryItemStatusRoutedEventArgs args = new QueryItemStatusRoutedEventArgs(
        AutomationQueryEvents.QueryItemStatusForDetailEvent, m_dataGridContext );

      m_dataGridControl.RaiseEvent( args );
      string itemStatus = null;

      if( args.Handled )
        itemStatus = args.ItemStatus;

      return ( itemStatus ?? string.Empty );
    }

    protected override string GetItemTypeCore()
    {
      if( m_dataGridContext == null )
        return string.Empty;

      QueryItemTypeRoutedEventArgs args = new QueryItemTypeRoutedEventArgs(
        AutomationQueryEvents.QueryItemTypeForDetailEvent, m_dataGridContext );

      m_dataGridControl.RaiseEvent( args );
      string itemType = null;

      if( args.Handled )
        itemType = args.ItemType;

      return ( itemType ?? string.Empty );
    }

    protected override AutomationPeer GetLabeledByCore()
    {
      return null;
    }

    protected override string GetNameCore()
    {
      if( m_dataGridContext == null )
        return string.Empty;

      QueryNameRoutedEventArgs args = new QueryNameRoutedEventArgs(
        AutomationQueryEvents.QueryNameForDetailEvent, m_dataGridContext );

      m_dataGridControl.RaiseEvent( args );
      string name = null;

      if( args.Handled )
      {
        name = args.Name;
      }
      else
      {
        name = m_dataGridContext.SourceDetailConfiguration.Title as string;
      }

      return ( name ?? string.Empty );
    }

    protected override AutomationOrientation GetOrientationCore()
    {
      return AutomationOrientation.None;
    }

    protected override bool HasKeyboardFocusCore()
    {
      return false;
    }

    protected override bool IsContentElementCore()
    {
      return true;
    }

    protected override bool IsControlElementCore()
    {
      return true;
    }

    protected override bool IsEnabledCore()
    {
      return true;
    }

    protected override bool IsKeyboardFocusableCore()
    {
      return false;
    }

    protected override bool IsOffscreenCore()
    {
      if( m_dataGridContext == null )
        return true;

      if( m_parentDataGridContext == null )
        return false;

      return ( m_parentDataGridContext.CustomItemContainerGenerator.ContainerFromItem( m_dataGridContext.ParentItem ) == null );
    }

    protected override bool IsPasswordCore()
    {
      return false;
    }

    protected override bool IsRequiredForFormCore()
    {
      return false;
    }

    protected override void SetFocusCore()
    {
    }

    internal DataGridItemAutomationPeer FindOrCreateItemAutomationPeer( object item, int index )
    {
      DataGridItemAutomationPeer itemAutomationPeer = m_dataChildren[ item ] as DataGridItemAutomationPeer;

      if( itemAutomationPeer == null )
      {
        itemAutomationPeer = this.CreateItemAutomationPeer( item, index );
      }
      else
      {
        itemAutomationPeer.SetIndex( index );
      }

      // Force EventsSource to be updated
      itemAutomationPeer.GetWrapperPeer();
      return itemAutomationPeer;
    }

    internal IRawElementProviderSimple GetRawElementProviderSimple()
    {
      return this.ProviderFromPeer( this );
    }

    internal DataGridItemAutomationPeer GetItemAutomationPeer( object item )
    {
      // Force the creation of the Children.
      this.GetChildren();
      return m_dataChildren[ item ] as DataGridItemAutomationPeer;
    }

    internal DataGridItemAutomationPeer CreateItemAutomationPeerInternal( object item, int index )
    {
      return this.CreateItemAutomationPeer( item, index );
    }

    internal void RaiseSelectionEvents( IList<object> unselectedItems, IList<object> selectedItems )
    {
      if( m_dataChildren.Count == 0 )
      {
        this.RaiseAutomationEvent( AutomationEvents.SelectionPatternOnInvalidated );
        return;
      }

      int currentSelectedItemCount = ( m_dataGridContext == null ) ? 0 : m_dataGridContext.SelectedItemsStore.Count;
      int selectedItemsCount = selectedItems.Count;
      int unselectedItemsCount = unselectedItems.Count;

      if( currentSelectedItemCount == 1 )
      {
        object item;

        try
        {
          // GetItem() can throw if the DataVirtualizing source don't have that item anymore.
          item = m_dataGridContext.SelectedItemsStore[ 0 ].GetItem( m_dataGridContext, 0 );
        }
        catch
        {
          item = null;
        }

        if( item != null )
        {
          DataGridItemAutomationPeer peer = m_dataChildren[ item ] as DataGridItemAutomationPeer;

          if( peer != null )
            peer.RaiseAutomationEvent( AutomationEvents.SelectionItemPatternOnElementSelected );
        }
        else
        {
          this.RaiseAutomationEvent( AutomationEvents.SelectionPatternOnInvalidated );
        }
      }
      else if( ( selectedItemsCount + unselectedItemsCount ) > 20 )
      {
        this.RaiseAutomationEvent( AutomationEvents.SelectionPatternOnInvalidated );
      }
      else
      {
        for( int i = 0; i < selectedItemsCount; i++ )
        {
          object item;

          try
          {
            // Underlying GetItem() can throw if the DataVirtualizing source don't have that item anymore.
            item = selectedItems[ i ];
          }
          catch
          {
            item = null;
          }

          if( item != null )
          {
            DataGridItemAutomationPeer peer = m_dataChildren[ item ] as DataGridItemAutomationPeer;

            if( peer != null )
              peer.RaiseAutomationEvent( AutomationEvents.SelectionItemPatternOnElementAddedToSelection );
          }
          else
          {
            this.RaiseAutomationEvent( AutomationEvents.SelectionPatternOnInvalidated );
            return;
          }
        }

        for( int i = 0; i < unselectedItemsCount; i++ )
        {
          object item;

          try
          {
            // Underlying GetItem() can throw if the DataVirtualizing source don't have that item anymore.
            item = unselectedItems[ i ];
          }
          catch
          {
            item = null;
          }

          if( item != null )
          {
            DataGridItemAutomationPeer peer = m_dataChildren[ item ] as DataGridItemAutomationPeer;

            if( peer != null )
              peer.RaiseAutomationEvent( AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection );
          }
          else
          {
            this.RaiseAutomationEvent( AutomationEvents.SelectionPatternOnInvalidated );
            return;
          }
        }
      }
    }

    internal DataGridContextAutomationPeer GetDetailPeer( object parentItem, DetailConfiguration detailConfig )
    {
      DataGridItemAutomationPeer itemAutomationPeer = m_dataChildren[ parentItem ] as DataGridItemAutomationPeer;

      if( itemAutomationPeer == null )
        return null;

      return itemAutomationPeer.GetDetailPeer( detailConfig );
    }

    internal List<AutomationPeer> GetColumnHeadersPeer()
    {
      if( m_dataGridContext == null )
        return null;

      List<AutomationPeer> list = new List<AutomationPeer>( 16 );
      this.AddChildrenHeaders( m_dataGridContext.CustomItemContainerGenerator.Header, m_headerFooterChildren, list );
      this.AddChildrenFooters( m_dataGridContext.CustomItemContainerGenerator.Footer, m_headerFooterChildren, list );
      int listCount = list.Count;

      List<AutomationPeer> columnHeaders = new List<AutomationPeer>( listCount );

      for( int i = 0; i < listCount; i++ )
      {
        HeaderFooterItemAutomationPeer headerFooterItemPeer = list[ i ] as HeaderFooterItemAutomationPeer;

        if( headerFooterItemPeer == null )
          continue;

        if( headerFooterItemPeer.GetAutomationControlType() == AutomationControlType.Header )
          columnHeaders.Add( headerFooterItemPeer );
      }

      return columnHeaders;
    }

    private void Columns_VisibleColumnsUpdated( object sender, EventArgs e )
    {
      if( m_dataGridContext == null )
        return;

      int newCount = m_dataGridContext.VisibleColumns.Count;

      if( newCount != m_oldColumnCount )
        this.RaisePropertyChangedEvent( GridPatternIdentifiers.ColumnCountProperty, m_oldColumnCount, newCount );

      m_oldColumnCount = newCount;

      // Invalidate the cell peer of all the row to resync with the new column.
      foreach( AutomationPeer peer in m_dataChildren.Values )
      {
        peer.InvalidatePeer();
      }
    }

    private void AddChildrenHeaders(
      HeadersFootersGeneratorNode headerNode,
      Hashtable oldHeaderFooterChildren,
      List<AutomationPeer> list )
    {
      if( m_parentDataGridContext == null )
      {
        // Add fixed header to the children list
        Panel fixedHeaderPanel = m_dataGridControl.FixedHeadersHostPanel;

        if( fixedHeaderPanel != null )
        {
          int index = 0;

          foreach( DependencyObject headerFooter in fixedHeaderPanel.Children )
          {
            HeaderFooterItem headerFooterItem = headerFooter as HeaderFooterItem;

            if( headerFooterItem != null )
            {
              HeaderFooterItemAutomationPeer automationPeer = oldHeaderFooterChildren[ headerFooter ] as HeaderFooterItemAutomationPeer;

              if( automationPeer == null )
              {
                automationPeer = new HeaderFooterItemAutomationPeer( m_dataGridContext, headerFooter );
              }

              automationPeer.SetExtraInfo( HeaderFooterItemAutomationPeer.HeaderFooterType.FixedHeader, index );
              m_headerFooterChildren[ headerFooter ] = automationPeer;
              list.Add( automationPeer );
            }

            index++;
          }
        }
      }

      if( headerNode != null )
      {
        DataGridGroupAutomationPeer.AddHeaderPeer( m_dataGridContext, headerNode, list, m_headerFooterChildren, oldHeaderFooterChildren );
      }
    }

    private void AddChildrenFooters(
      HeadersFootersGeneratorNode footerNode,
      Hashtable oldHeaderFooterChildren,
      List<AutomationPeer> list )
    {
      if( m_parentDataGridContext == null )
      {
        // Add fixed header to the children list
        Panel fixedFooterPanel = m_dataGridControl.FixedFootersHostPanel;

        if( fixedFooterPanel != null )
        {
          int index = 0;

          foreach( DependencyObject headerFooter in fixedFooterPanel.Children )
          {
            HeaderFooterItem headerFooterItem = headerFooter as HeaderFooterItem;

            if( headerFooterItem != null )
            {
              HeaderFooterItemAutomationPeer automationPeer = oldHeaderFooterChildren[ headerFooter ] as HeaderFooterItemAutomationPeer;

              if( automationPeer == null )
              {
                automationPeer = new HeaderFooterItemAutomationPeer( m_dataGridContext, headerFooter );
              }

              automationPeer.SetExtraInfo( HeaderFooterItemAutomationPeer.HeaderFooterType.FixedFooter, index );
              m_headerFooterChildren[ headerFooter ] = automationPeer;
              list.Add( automationPeer );
            }

            index++;
          }
        }
      }

      if( footerNode != null )
      {
        DataGridGroupAutomationPeer.AddFooterPeer( m_dataGridContext, footerNode, list, m_headerFooterChildren, oldHeaderFooterChildren );
      }
    }

    #region ISelectionProvider Members

    bool ISelectionProvider.CanSelectMultiple
    {
      get
      {
        return m_dataGridControl.SelectionModeToUse != SelectionMode.Single;
      }
    }

    IRawElementProviderSimple[] ISelectionProvider.GetSelection()
    {
      if( m_dataGridContext == null )
        return null;

      IList<object> selectedItems = m_dataGridContext.SelectedItems;
      int count = m_dataGridContext.SelectedItems.Count;

      if( ( count <= 0 ) || ( m_dataChildren.Count <= 0 ) )
        return null;

      List<IRawElementProviderSimple> list = new List<IRawElementProviderSimple>( count );

      for( int i = 0; i < count; i++ )
      {
        DataGridItemAutomationPeer peer =
          m_dataChildren[ selectedItems[ i ] ] as DataGridItemAutomationPeer;

        if( peer != null )
          list.Add( this.ProviderFromPeer( peer ) );
      }

      return list.ToArray();
    }

    bool ISelectionProvider.IsSelectionRequired
    {
      get
      {
        return false;
      }
    }

    #endregion ISelectionProvider Members

    #region ITableProvider Members

    IRawElementProviderSimple[] ITableProvider.GetColumnHeaders()
    {
      List<AutomationPeer> list = this.GetColumnHeadersPeer();
      IRawElementProviderSimple[] rawElementProviderSimples = new IRawElementProviderSimple[ list.Count ];

      for( int i = 0; i < rawElementProviderSimples.Length; i++ )
      {
        rawElementProviderSimples[ i ] = this.ProviderFromPeer( list[ i ] );
      }

      return rawElementProviderSimples;
    }

    IRawElementProviderSimple[] ITableProvider.GetRowHeaders()
    {
      return new IRawElementProviderSimple[ 0 ];
    }

    RowOrColumnMajor ITableProvider.RowOrColumnMajor
    {
      get
      {
        return RowOrColumnMajor.RowMajor;
      }
    }

    #endregion

    #region IGridProvider Members

    int IGridProvider.ColumnCount
    {
      get
      {
        if( m_dataGridContext == null )
          return 0;

        return m_dataGridContext.VisibleColumns.Count;
      }
    }

    IRawElementProviderSimple IGridProvider.GetItem( int row, int column )
    {
      if( m_dataGridContext == null )
        return null;

      // Here we only consider data item.  No header / footer are part of the IGridProvider.
      CollectionView items = m_dataGridContext.Items;

      if( ( row < 0 ) || ( row >= items.Count ) )
        throw new ArgumentOutOfRangeException( "row" );

      ReadOnlyObservableCollection<ColumnBase> visibleColumns = m_dataGridContext.VisibleColumns;

      if( ( column < 0 ) || ( column >= visibleColumns.Count ) )
        throw new ArgumentOutOfRangeException( "column" );

      object item = m_dataGridContext.Items.GetItemAt( row );
      DataGridItemAutomationPeer itemPeer = m_dataChildren[ item ] as DataGridItemAutomationPeer;

      if( itemPeer == null )
      {
        // If the item is not found in the cache, try to force is creation in the caching.
        CollectionViewGroup[] groups;
        this.GetParentGroupsContainingItemIndex( row, out groups );

        this.ForceChildrenCaching( groups );
        itemPeer = m_dataChildren[ item ] as DataGridItemAutomationPeer;
      }

      if( itemPeer == null )
        return null;

      DataGridItemCellAutomationPeer cellPeer = itemPeer.GetDataGridItemCellPeer( visibleColumns[ column ] );

      if( cellPeer == null )
        return null;

      return this.ProviderFromPeer( cellPeer );
    }

    int IGridProvider.RowCount
    {
      get
      {
        if( m_dataGridContext == null )
          return 0;

        return m_dataGridContext.Items.Count;
      }
    }

    private void ForceChildrenCaching( CollectionViewGroup[] groups )
    {
      List<AutomationPeer> subAutomationPeers = this.GetChildren();

      foreach( CollectionViewGroup group in groups )
      {
        Debug.Assert( !( group is DataGridCollectionViewGroupRoot ) );
        bool groupFound = false;

        foreach( AutomationPeer automationPeer in subAutomationPeers )
        {
          DataGridGroupAutomationPeer groupAutomationPeer = automationPeer as DataGridGroupAutomationPeer;

          if( groupAutomationPeer == null )
            continue;

          if( groupAutomationPeer.Owner.CollectionViewGroup == group )
          {
            subAutomationPeers = groupAutomationPeer.GetChildren();
            groupFound = true;
            break;
          }
        }

        if( !groupFound )
          break;
      }
    }

    private void GetParentGroupsContainingItemIndex( int index, out CollectionViewGroup[] parentGroups )
    {
      DataGridCollectionView collectionView = m_dataGridContext.ItemsSourceCollection as DataGridCollectionView;

      if( collectionView != null )
      {
        RawItem rawItem = collectionView.GetRawItemAt( index );
        List<CollectionViewGroup> groupList = new List<CollectionViewGroup>( 16 );
        DataGridCollectionViewGroup parentGroup = rawItem.ParentGroup;

        while( ( parentGroup != null ) && !( parentGroup is DataGridCollectionViewGroupRoot ) )
        {
          groupList.Add( parentGroup );
          parentGroup = parentGroup.Parent;
        }

        groupList.Reverse();
        parentGroups = groupList.ToArray();
      }
      else
      {
        CollectionView items = m_dataGridContext.Items;
        ReadOnlyObservableCollection<object> groups = items.Groups;

        if( groups != null )
        {
          List<CollectionViewGroup> groupList = new List<CollectionViewGroup>( 16 );
          this.GetParentGroupsContainingItemIndex( index, groups, groupList );
          parentGroups = groupList.ToArray();
        }
        else
        {
          parentGroups = new DataGridCollectionViewGroup[ 0 ];
        }
      }
    }

    private void GetParentGroupsContainingItemIndex(
      int indexOffset,
      ReadOnlyObservableCollection<object> rootGroups,
      List<CollectionViewGroup> parentGroups )
    {
      int oldCurrentCount = 0;
      int currentCount = 0;

      foreach( CollectionViewGroup group in rootGroups )
      {
        currentCount += group.GetItemCount();

        if( currentCount > indexOffset )
        {
          parentGroups.Add( group );

          if( !group.IsBottomLevel )
          {
            this.GetParentGroupsContainingItemIndex(
              indexOffset - oldCurrentCount,
              ( ReadOnlyObservableCollection<object> )group.GetItems(),
              parentGroups );
          }

          return;
        }

        oldCurrentCount = currentCount;
      }
    }

    #endregion

    #region IExpandCollapseProvider Members

    void IExpandCollapseProvider.Collapse()
    {
      if( m_parentDataGridContext == null )
        return;

      m_parentDataGridContext.CollapseDetails( m_parentItem );
      this.DataGridContext = null;
    }

    void IExpandCollapseProvider.Expand()
    {
      if( m_parentDataGridContext == null )
        return;

      m_parentDataGridContext.ExpandDetails( m_parentItem );
      Debug.Assert( m_dataGridContext != null );
    }

    ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
    {
      get
      {
        if( m_parentDataGridContext == null )
          return ExpandCollapseState.Expanded;

        return ( m_dataGridContext == null ) ?
          ExpandCollapseState.Collapsed : ExpandCollapseState.Expanded;
      }
    }

    #endregion

    #region IItemContainerProvider Members

    public IRawElementProviderSimple FindItemByProperty( IRawElementProviderSimple startAfter, int propertyId, object value )
    {
      if( ( propertyId != 0 ) && ( !DataGridContextAutomationPeer.IsPropertySupportedForFindItem( propertyId ) ) )
      {
        throw new ArgumentException( "Property not supported" );
      }

      if( m_dataGridContext == null )
        return null;

      CollectionView items = m_dataGridContext.Items;
      int itemsCount = ( items == null ) ? 0 : items.Count;

      DataGridItemAutomationPeer startAfterItemPeer = null;
      HeaderFooterItemAutomationPeer startAfterHeaderFooterPeer = null;

      if( startAfter != null )
      {
        AutomationPeer startAfterPeer = this.PeerFromProvider( startAfter );

        if( startAfterPeer == null )
          return null;

        startAfterItemPeer = startAfterPeer as DataGridItemAutomationPeer;
        startAfterHeaderFooterPeer = startAfterPeer as HeaderFooterItemAutomationPeer;
      }

      int startIndex = 0;

      // Get header count
      HeadersFootersGeneratorNode headerNode = m_dataGridContext.CustomItemContainerGenerator.Header;
      int headersCount = ( headerNode == null ) ? 0 : headerNode.ItemCount;
      int fixedHeadersCount = 0;
      Panel fixedHeadersPanel = null;

      // Get footer count
      HeadersFootersGeneratorNode footerNode = m_dataGridContext.CustomItemContainerGenerator.Footer;
      int footersCount = ( footerNode == null ) ? 0 : footerNode.ItemCount;
      int fixedFootersCount = 0;
      Panel fixedFootersPanel = null;

      if( m_parentDataGridContext == null )
      {
        // Add the fixed header / footer count to the children
        fixedHeadersPanel = m_dataGridControl.FixedHeadersHostPanel;
        fixedHeadersCount = ( fixedHeadersPanel != null ) ? fixedHeadersPanel.Children.Count : 0;
        fixedFootersPanel = m_dataGridControl.FixedFootersHostPanel;
        fixedFootersCount = ( fixedFootersPanel != null ) ? fixedFootersPanel.Children.Count : 0;
      }

      int childrenCount = headersCount + fixedHeadersCount + footersCount + fixedFootersCount + itemsCount;

      if( startAfterItemPeer != null )
      {
        startIndex = fixedHeadersCount + headersCount + startAfterItemPeer.Index + 1;
      }
      else if( startAfterHeaderFooterPeer != null )
      {
        startIndex = startAfterHeaderFooterPeer.Index + 1;

        switch( startAfterHeaderFooterPeer.Type )
        {
          case HeaderFooterItemAutomationPeer.HeaderFooterType.FixedHeader:
            {
              break;
            }
          case HeaderFooterItemAutomationPeer.HeaderFooterType.Header:
            {
              startIndex += fixedHeadersCount;
              break;
            }
          case HeaderFooterItemAutomationPeer.HeaderFooterType.Footer:
            {
              startIndex += fixedHeadersCount + headersCount + itemsCount;
              break;
            }
          case HeaderFooterItemAutomationPeer.HeaderFooterType.FixedFooter:
            {
              startIndex += fixedHeadersCount + headersCount + footersCount + itemsCount;
              break;
            }
        }
      }

      // Force a children refresh and update our inner caching
      this.GetChildren();

      if( propertyId == 0 )
      {
        AutomationPeer peer = this.GetPeerForChildrenIndex( startIndex );

        if( peer == null )
          return null;

        return this.ProviderFromPeer( peer );
      }

      object propertyValue = null;

      // Search in footer/Fixed footer first
      if( footersCount + fixedFootersCount > 0 )
      {
        int footerStartIndex = Math.Max( startIndex, childrenCount - footersCount - fixedFootersCount );

        for( int i = startIndex; i < childrenCount; i++ )
        {
          AutomationPeer peer = this.GetPeerForChildrenIndex( i );

          if( peer != null )
          {
            try
            {
              propertyValue = peer.GetPropertyValue( propertyId );
            }
            catch( Exception exception )
            {
              if( exception is ElementNotAvailableException )
              {
                continue;
              }
            }

            if( object.Equals( value, propertyValue ) )
            {
              return this.ProviderFromPeer( peer );
            }
          }
        }

        childrenCount -= footersCount + fixedFootersCount;
      }

      // Search in the header/Fixed header and data item
      for( int i = startIndex; i < childrenCount; i++ )
      {
        AutomationPeer peer = this.GetPeerForChildrenIndex( i );

        if( peer != null )
        {
          try
          {
            propertyValue = peer.GetPropertyValue( propertyId );
          }
          catch( Exception exception )
          {
            if( exception is ElementNotAvailableException )
            {
              continue;
            }
          }

          if( object.Equals( value, propertyValue ) )
          {
            return this.ProviderFromPeer( peer );
          }
        }
      }

      return null;
    }

    private AutomationPeer GetPeerForChildrenIndex( int index )
    {
      int fixedHeadersCount = 0;
      Panel fixedHeadersPanel = null;
      int fixedFootersCount = 0;
      Panel fixedFootersPanel = null;

      if( m_parentDataGridContext == null )
      {
        fixedHeadersPanel = m_dataGridControl.FixedHeadersHostPanel;
        fixedHeadersCount = ( fixedHeadersPanel != null ) ? fixedHeadersPanel.Children.Count : 0;
        fixedFootersPanel = m_dataGridControl.FixedFootersHostPanel;
        fixedFootersCount = ( fixedFootersPanel != null ) ? fixedFootersPanel.Children.Count : 0;
      }

      if( index < fixedHeadersCount )
      {
        UIElement element = fixedHeadersPanel.Children[ index ];
        AutomationPeer automationPeer = m_headerFooterChildren[ element ] as AutomationPeer;
        Debug.Assert( automationPeer != null );

        if( automationPeer == null )
          return null;

        return automationPeer;
      }
      else
      {
        index -= fixedHeadersCount;
        HeadersFootersGeneratorNode headerNode = m_dataGridContext.CustomItemContainerGenerator.Header;
        int headersCount = ( headerNode == null ) ? 0 : headerNode.ItemCount;

        if( index < headersCount )
        {
          object item = headerNode.GetAt( index );
          AutomationPeer automationPeer = m_headerFooterChildren[ item ] as AutomationPeer;
          Debug.Assert( automationPeer != null );
          return automationPeer;
        }
        else
        {
          index -= headersCount;
          CollectionView items = m_dataGridContext.Items;
          int itemsCount = ( items == null ) ? 0 : items.Count;

          if( index < itemsCount )
          {
            return this.FindOrCreateItemAutomationPeer( items.GetItemAt( index ), index );
          }
          else
          {
            index -= itemsCount;
            HeadersFootersGeneratorNode footerNode = m_dataGridContext.CustomItemContainerGenerator.Footer;
            int footersCount = ( footerNode == null ) ? 0 : footerNode.ItemCount;

            if( index < footersCount )
            {
              object item = footerNode.GetAt( index );
              AutomationPeer automationPeer = m_headerFooterChildren[ item ] as AutomationPeer;
              Debug.Assert( automationPeer != null );
              return automationPeer;
            }
            else
            {
              index -= footersCount;

              if( index < fixedFootersCount )
              {
                UIElement element = fixedFootersPanel.Children[ index ];
                AutomationPeer automationPeer = m_headerFooterChildren[ element ] as AutomationPeer;
                Debug.Assert( automationPeer != null );
                return automationPeer;
              }
            }
          }
        }
      }

      return null;
    }

    private static bool IsPropertySupportedForFindItem( int id )
    {
      return ( ( AutomationElementIdentifiers.NameProperty.Id == id )
        || ( AutomationElementIdentifiers.AutomationIdProperty.Id == id )
        || ( AutomationElementIdentifiers.ControlTypeProperty.Id == id )
        || ( SelectionItemPatternIdentifiers.IsSelectedProperty.Id == id ) );
    }

    #endregion

    private DataGridControl m_dataGridControl;
    private DataGridContext m_dataGridContext;
    private DataGridContext m_parentDataGridContext;
    private object m_parentItem;
    private DetailConfiguration m_detailConfiguration;

    private Hashtable m_dataChildren;
    private Hashtable m_headerFooterChildren;
    private int m_oldColumnCount = -1;
  }
}
