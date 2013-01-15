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
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Automation;
using System.Globalization;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid.Automation
{
  public class DataGridGroupAutomationPeer : AutomationPeer, IExpandCollapseProvider, IItemContainerProvider
  {
    public DataGridGroupAutomationPeer( Group uiGroupOwner )
    {
      if( uiGroupOwner == null )
        throw new ArgumentNullException( "uiGroupOwner" );

      m_uiGroupOwner = uiGroupOwner;
      m_dataChildren = new Hashtable( 0 );
      m_headerFooterChildren = new Hashtable( 0 );
    }

    #region Owner Property

    public Group Owner
    {
      get
      {
        return m_uiGroupOwner;
      }
    }

    #endregion

    public override object GetPattern( PatternInterface patternInterface )
    {
      if( patternInterface == PatternInterface.ExpandCollapse )
        return this;

      return null;
    }

    protected override string GetAcceleratorKeyCore()
    {
      return string.Empty;
    }

    protected override string GetAccessKeyCore()
    {
      return string.Empty;
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
      return AutomationControlType.Group;
    }

    protected override string GetAutomationIdCore()
    {
      QueryAutomationIdRoutedEventArgs args = new QueryAutomationIdRoutedEventArgs(
        AutomationQueryEvents.QueryAutomationIdForGroupEvent, m_uiGroupOwner );

      m_uiGroupOwner.DataGridContext.DataGridControl.RaiseEvent( args );

      string automationId = null;

      if( args.Handled )
        automationId = args.AutomationId;

      if( automationId == null )
      {
        if( m_uiGroupOwner.Value != null )
        {
          return string.Format(
            CultureInfo.InvariantCulture,
            "Group_{0}:{1}_{2}",
            m_uiGroupOwner.Level, m_uiGroupOwner.GroupBy, m_uiGroupOwner.Value );
        }

        return string.Empty;
      }

      return automationId;
    }

    protected override Rect GetBoundingRectangleCore()
    {
      return Rect.Empty;
    }

    protected override List<AutomationPeer> GetChildrenCore()
    {
      Group owner = this.Owner;
      GroupGeneratorNode generatorNode = owner.GeneratorNode;

      if( generatorNode == null )
        return null;

      DataGridContext dataGridContext = owner.DataGridContext;

      if( dataGridContext == null )
        return null;

      CustomItemContainerGenerator customItemContainerGenerator = dataGridContext.CustomItemContainerGenerator;
      IList<object> items;
      bool itemsIsAllItems = false;

      // The way Microsoft check if we are called by an automation that support Virtualization.
      if( ( ItemContainerPatternIdentifiers.Pattern != null ) && ( owner.IsBottomLevel ) && ( dataGridContext.DataGridControl.ItemsHost is DataGridItemsHost ) )
      {
        items = customItemContainerGenerator.GetRealizedDataItemsForGroup( generatorNode );
      }
      else
      {
        items = owner.GetItems();
        itemsIsAllItems = true;
      }

      int itemsCount = ( items == null ) ? 0 : items.Count;
      Hashtable oldDataChildren = m_dataChildren;
      Hashtable oldHeaderFooterChildren = m_headerFooterChildren;

      // Get header count
      HeadersFootersGeneratorNode headerNode = generatorNode.GetHeaderNode();
      int headersCount = ( headerNode == null ) ? 0 : headerNode.ItemCount;

      // Get footer count
      HeadersFootersGeneratorNode footerNode = generatorNode.GetFooterNode();
      int footersCount = ( footerNode == null ) ? 0 : footerNode.ItemCount;

      int childrenCount = itemsCount + headersCount + footersCount;
      m_dataChildren = new Hashtable( itemsCount );
      m_headerFooterChildren = new Hashtable( headersCount + footersCount );

      if( childrenCount == 0 )
        return null;

      DataGridContextAutomationPeer dataGridContextAutomationPeer = dataGridContext.Peer;
      Hashtable dataGridContextItemsPeer = dataGridContextAutomationPeer.ItemPeers;

      DataGridGroupAutomationPeer groupAutomationPeer;
      DataGridItemAutomationPeer itemAutomationPeer;
      List<AutomationPeer> list = new List<AutomationPeer>( childrenCount );

      if( headerNode != null )
      {
        DataGridGroupAutomationPeer.AddHeaderPeer( dataGridContext, headerNode, list, m_headerFooterChildren, oldHeaderFooterChildren );
      }

      int index = 0;

      for( int i = 0; i < itemsCount; i++ )
      {
        Object item = items[ i ];
        CollectionViewGroup collectionViewGroup = item as CollectionViewGroup;

        if( collectionViewGroup == null )
        {
          if( ( i == 0 ) && ( itemsIsAllItems ) )
          {
            GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( generatorNode, 0, 0 );
            nodeHelper.ReverseCalculateIndex();
            index = nodeHelper.SourceDataIndex;
          }

          itemAutomationPeer = oldDataChildren[ item ] as DataGridItemAutomationPeer;

          if( itemAutomationPeer == null )
          {
            if( itemsIsAllItems )
            {
              itemAutomationPeer = dataGridContextAutomationPeer.CreateItemAutomationPeerInternal( item, index + i );
            }
            else
            {
              itemAutomationPeer = dataGridContextAutomationPeer.CreateItemAutomationPeerInternal( item, -1 );
            }
          }
          else
          {
            if( itemsIsAllItems )
            {
              itemAutomationPeer.SetIndex( index + i );
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
            Debug.Assert( itemAutomationPeer != null );
            list.Add( itemAutomationPeer );
            m_dataChildren[ item ] = itemAutomationPeer;

            if( dataGridContextItemsPeer[ item ] == null )
              dataGridContextItemsPeer[ item ] = itemAutomationPeer;
          }
        }
        else
        {
          Group uiGroup =
            customItemContainerGenerator.GetGroupFromCollectionViewGroup( this.Owner, collectionViewGroup );

          if( uiGroup != null )
          {
            groupAutomationPeer = uiGroup.CreateAutomationPeer() as DataGridGroupAutomationPeer;

            if( groupAutomationPeer != null )
              list.Add( groupAutomationPeer );
          }
        }
      }

      if( footerNode != null )
      {
        DataGridGroupAutomationPeer.AddFooterPeer( dataGridContext, footerNode, list, m_headerFooterChildren, oldHeaderFooterChildren );
      }

      return list;
    }

    protected override string GetClassNameCore()
    {
      return "DataGridCollectionViewGroup";
    }

    protected override Point GetClickablePointCore()
    {
      return new Point( double.NaN, double.NaN );
    }

    protected override string GetHelpTextCore()
    {
      QueryHelpTextRoutedEventArgs args = new QueryHelpTextRoutedEventArgs(
        AutomationQueryEvents.QueryHelpTextForGroupEvent, m_uiGroupOwner );

      m_uiGroupOwner.DataGridContext.DataGridControl.RaiseEvent( args );

      string helpText = null;

      if( args.Handled )
        helpText = args.HelpText;

      return ( helpText ?? string.Empty );
    }

    protected override string GetItemStatusCore()
    {
      QueryItemStatusRoutedEventArgs args = new QueryItemStatusRoutedEventArgs(
        AutomationQueryEvents.QueryItemStatusForGroupEvent, m_uiGroupOwner );

      m_uiGroupOwner.DataGridContext.DataGridControl.RaiseEvent( args );

      string itemStatus = null;

      if( args.Handled )
        itemStatus = args.ItemStatus;

      return ( itemStatus ?? string.Empty );
    }

    protected override string GetItemTypeCore()
    {
      QueryItemTypeRoutedEventArgs args = new QueryItemTypeRoutedEventArgs(
        AutomationQueryEvents.QueryItemTypeForGroupEvent, m_uiGroupOwner );

      m_uiGroupOwner.DataGridContext.DataGridControl.RaiseEvent( args );

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
      QueryNameRoutedEventArgs args = new QueryNameRoutedEventArgs(
        AutomationQueryEvents.QueryNameForGroupEvent, m_uiGroupOwner );

      m_uiGroupOwner.DataGridContext.DataGridControl.RaiseEvent( args );

      string name = null;

      if( args.Handled )
        name = args.Name;

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
      return false;
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
      throw new NotSupportedException();
    }

    static internal void AddHeaderPeer(
      DataGridContext dataGridContext,
      HeadersFootersGeneratorNode node,
      List<AutomationPeer> list,
      Hashtable children,
      Hashtable oldChildren )
    {
      int headersCount = node.ItemCount;

      for( int i = 0; i < headersCount; i++ )
      {
        // We use GetAt since it does not return the same thing as .Items.
        // We need to get a GroupHeaderFooterItem.
        object item = node.GetAt( i );

        if( item != null )
        {
          HeaderFooterItemAutomationPeer automationPeer =
            oldChildren[ item ] as HeaderFooterItemAutomationPeer;

          if( automationPeer == null )
          {
            automationPeer = new HeaderFooterItemAutomationPeer( dataGridContext, item );
          }

          Debug.Assert( automationPeer != null );

          // we set ExtraInfo even if the header was already created since the position can change.
          automationPeer.SetExtraInfo( HeaderFooterItemAutomationPeer.HeaderFooterType.Header, i );

          // Force EventsSource to be updated
          automationPeer.GetWrapperPeer();

          list.Add( automationPeer );
          children[ item ] = automationPeer;
        }
      }
    }

    static internal void AddFooterPeer(
      DataGridContext dataGridContext,
      HeadersFootersGeneratorNode node,
      List<AutomationPeer> list,
      Hashtable children,
      Hashtable oldChildren )
    {
      int headersCount = node.ItemCount;

      for( int i = 0; i < headersCount; i++ )
      {
        // We use GetAt since it does not return the same thing as .Items.
        // We need to get a GroupHeaderFooterItem.
        object item = node.GetAt( i );

        if( item != null )
        {
          HeaderFooterItemAutomationPeer automationPeer =
            oldChildren[ item ] as HeaderFooterItemAutomationPeer;

          if( automationPeer == null )
          {
            automationPeer = new HeaderFooterItemAutomationPeer( dataGridContext, item );
          }

          Debug.Assert( automationPeer != null );

          // we set ExtraInfo even if the header was already created since the position can change.
          automationPeer.SetExtraInfo( HeaderFooterItemAutomationPeer.HeaderFooterType.Footer, i );

          // Force EventsSource to be updated
          automationPeer.GetWrapperPeer();

          list.Add( automationPeer );
          children[ item ] = automationPeer;
        }
      }
    }

    #region IExpandCollapseProvider Members

    void IExpandCollapseProvider.Collapse()
    {
      m_uiGroupOwner.IsExpanded = false;
    }

    void IExpandCollapseProvider.Expand()
    {
      m_uiGroupOwner.IsExpanded = true;
    }

    ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
    {
      get
      {
        return ( m_uiGroupOwner.IsExpanded ) ?
          ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
      }
    }

    #endregion

    #region IItemContainerProvider Members

    public IRawElementProviderSimple FindItemByProperty( IRawElementProviderSimple startAfter, int propertyId, object value )
    {
      if( ( propertyId != 0 ) && ( !DataGridGroupAutomationPeer.IsPropertySupportedForFindItem( propertyId ) ) )
      {
        throw new ArgumentException( "Property not supported" );
      }

      Group owner = this.Owner;
      GroupGeneratorNode generatorNode = owner.GeneratorNode;

      if( generatorNode == null )
        return null;

      DataGridContext dataGridContext = owner.DataGridContext;

      if( dataGridContext == null )
        return null;

      this.ResetChildrenCache();
      IList<object> items = owner.GetItems();
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
      HeadersFootersGeneratorNode headerNode = generatorNode.GetHeaderNode();
      int headersCount = ( headerNode == null ) ? 0 : headerNode.ItemCount;

      // Get footer count
      HeadersFootersGeneratorNode footerNode = generatorNode.GetFooterNode();
      int footersCount = ( footerNode == null ) ? 0 : footerNode.ItemCount;

      int childrenCount = headersCount + footersCount + itemsCount;

      if( startAfterItemPeer != null )
      {
        startIndex = headersCount + startAfterItemPeer.Index + 1;
      }
      else if( startAfterHeaderFooterPeer != null )
      {
        startIndex = startAfterHeaderFooterPeer.Index + 1;

        switch( startAfterHeaderFooterPeer.Type )
        {
          case HeaderFooterItemAutomationPeer.HeaderFooterType.Header:
            {
              break;
            }
          case HeaderFooterItemAutomationPeer.HeaderFooterType.Footer:
            {
              startIndex += headersCount + itemsCount;
              break;
            }
        }
      }

      if( propertyId == 0 )
      {
        AutomationPeer peer = this.GetPeerForChildrenIndex( startIndex );

        if( peer == null )
          return null;

        return this.ProviderFromPeer( peer );
      }

      object propertyValue = null;

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
      Group owner = this.Owner;
      GroupGeneratorNode generatorNode = owner.GeneratorNode;

      if( generatorNode == null )
        return null;

      DataGridContext dataGridContext = owner.DataGridContext;

      if( dataGridContext == null )
        return null;

      DataGridContextAutomationPeer dataGridContextAutomationPeer = dataGridContext.Peer;
      HeadersFootersGeneratorNode headerNode = generatorNode.GetHeaderNode();
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
        IList<object> items = owner.GetItems();
        int itemsCount = ( items == null ) ? 0 : items.Count;

        if( index < itemsCount )
        {
          return dataGridContextAutomationPeer.FindOrCreateItemAutomationPeer( items[ index ], -1 );
        }
        else
        {
          index -= itemsCount;
          HeadersFootersGeneratorNode footerNode = generatorNode.GetFooterNode();
          int footersCount = ( footerNode == null ) ? 0 : footerNode.ItemCount;

          if( index < footersCount )
          {
            object item = footerNode.GetAt( index );
            AutomationPeer automationPeer = m_headerFooterChildren[ item ] as AutomationPeer;
            Debug.Assert( automationPeer != null );
            return automationPeer;
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

    private Group m_uiGroupOwner;
    private Hashtable m_dataChildren;
    private Hashtable m_headerFooterChildren;
  }
}
