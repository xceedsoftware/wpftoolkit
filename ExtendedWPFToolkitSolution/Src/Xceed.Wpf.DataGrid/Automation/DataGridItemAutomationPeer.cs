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
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid.Automation
{
  public class DataGridItemAutomationPeer : AutomationPeer, ISelectionItemProvider, IVirtualizedItemProvider, IScrollItemProvider
  {
    public DataGridItemAutomationPeer( object item, DataGridContext dataGridContext, int index )
    {
      // if index = -1, it will be calculated later calling indexof on dataGridContext.Items.

      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      m_item = item;
      m_dataGridContext = dataGridContext;
      m_index = index;
      m_dataChildren = new Hashtable( 0 );
    }

    #region Item Property

    public object Item
    {
      get
      {
        return m_item;
      }
    }

    #endregion

    #region DataGridContext Property

    public DataGridContext DataGridContext
    {
      get
      {
        return m_dataGridContext;
      }
    }

    #endregion

    #region Index Property

    internal int Index
    {
      get
      {
        if( m_index == -1 )
          m_index = m_dataGridContext.Items.IndexOf( m_item );

        return m_index;
      }
    }

    internal void SetIndex( int value )
    {
      m_index = value;
    }

    #endregion

    public override object GetPattern( PatternInterface patternInterface )
    {
      if( ( patternInterface == PatternInterface.SelectionItem )
        || ( patternInterface == PatternInterface.VirtualizedItem )
        || ( patternInterface == PatternInterface.ScrollItem ) )
      {
        return this;
      }

      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetPattern( patternInterface );

      return null;
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
      return AutomationControlType.DataItem;
    }

    protected override string GetClassNameCore()
    {
      return "DataRow";
    }

    protected override string GetAcceleratorKeyCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetAcceleratorKey();

      return string.Empty;
    }

    protected override string GetAccessKeyCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetAccessKey();

      return string.Empty;
    }

    protected override string GetAutomationIdCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();
      string automationId = null;

      if( wrapperPeer != null )
        automationId = wrapperPeer.GetAutomationId();

      if( !string.IsNullOrEmpty( automationId ) )
        return automationId;

      return "Row_" + this.Index.ToString( CultureInfo.InvariantCulture );
    }

    protected override Rect GetBoundingRectangleCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetBoundingRectangle();

      return new Rect();
    }

    protected override List<AutomationPeer> GetChildrenCore()
    {
      DataRow dataRow = m_dataGridContext.CustomItemContainerGenerator.ContainerFromItem( m_item ) as DataRow;

      if( dataRow == null )
        return null;

      // Get child context ( Detail )
      DetailConfigurationCollection detailConfigurations = m_dataGridContext.DetailConfigurations;
      int detailConfigurationsCount = detailConfigurations.Count;

      ReadOnlyObservableCollection<ColumnBase> visibleColumns = m_dataGridContext.VisibleColumns;
      int visibleColumnsCount = visibleColumns.Count;

      if( visibleColumnsCount + detailConfigurationsCount <= 0 )
        return null;

      Hashtable oldDataChildren = m_dataChildren;
      List<AutomationPeer> list = new List<AutomationPeer>( visibleColumnsCount + detailConfigurationsCount );
      m_dataChildren = new Hashtable( visibleColumnsCount + detailConfigurationsCount );

      for( int i = 0; i < visibleColumnsCount; i++ )
      {
        ColumnBase column = visibleColumns[ i ];

        DataGridItemCellAutomationPeer cellAutomationPeer =
          oldDataChildren[ column ] as DataGridItemCellAutomationPeer;

        if( cellAutomationPeer == null )
          cellAutomationPeer = new DataGridItemCellAutomationPeer( this, column );

        // Always resetting the ColumnIndex since the visible position can have changed
        cellAutomationPeer.ColumnIndex = i;

        list.Add( cellAutomationPeer );
        m_dataChildren[ column ] = cellAutomationPeer;
      }

      for( int i = 0; i < detailConfigurationsCount; i++ )
      {
        DetailConfiguration detailConfiguration = detailConfigurations[ i ];
        DataGridContextAutomationPeer detailDataGridContextAutomationPeer = null;

        detailDataGridContextAutomationPeer =
          oldDataChildren[ detailConfiguration ] as DataGridContextAutomationPeer;

        if( detailDataGridContextAutomationPeer == null )
        {
          detailDataGridContextAutomationPeer = new DataGridContextAutomationPeer(
            m_dataGridContext.DataGridControl, m_dataGridContext, m_item, detailConfiguration );
        }

        list.Add( detailDataGridContextAutomationPeer );
        m_dataChildren[ detailConfiguration ] = detailDataGridContextAutomationPeer;
      }

      return list;
    }

    protected override Point GetClickablePointCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetClickablePoint();

      return new Point( double.NaN, double.NaN );
    }

    protected override string GetHelpTextCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetHelpText();

      return string.Empty;
    }

    protected override string GetItemStatusCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetItemStatus();

      return string.Empty;
    }

    protected override string GetItemTypeCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetItemType();

      return string.Empty;
    }

    protected override AutomationPeer GetLabeledByCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetLabeledBy();

      return null;
    }

    protected override string GetNameCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();
      string name = null;

      if( wrapperPeer != null )
        name = wrapperPeer.GetName();

      if( ( name == null ) && ( m_item is string ) )
        name = ( string )m_item;

      if( !string.IsNullOrEmpty( name ) )
        return name;

      Column mainColumn = this.DataGridContext.Columns.MainColumn as Column;

      if( ( mainColumn == null ) || ( !mainColumn.Visible ) )
        return string.Empty;

      BindingPathValueExtractor valueExtractor = this.DataGridContext.GetBindingPathExtractorForColumn( mainColumn, m_item );
      object mainColumnValue = valueExtractor.GetValueFromItem( m_item );

      if( mainColumnValue == null )
        return string.Empty;

      return string.Format( CultureInfo.CurrentCulture, "{0}", mainColumnValue );
    }

    protected override AutomationOrientation GetOrientationCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetOrientation();

      return AutomationOrientation.None;
    }

    protected override bool HasKeyboardFocusCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();
      return ( ( wrapperPeer != null ) && ( wrapperPeer.HasKeyboardFocus() ) );
    }

    protected override bool IsContentElementCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.IsContentElement();

      return true;
    }

    protected override bool IsControlElementCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.IsControlElement();

      return true;
    }

    protected override bool IsEnabledCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();
      return ( ( wrapperPeer == null ) || wrapperPeer.IsEnabled() );
    }

    protected override bool IsKeyboardFocusableCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();
      return ( ( wrapperPeer != null ) && wrapperPeer.IsKeyboardFocusable() );
    }

    protected override bool IsOffscreenCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.IsOffscreen();

      return true;
    }

    protected override bool IsPasswordCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();
      return ( ( wrapperPeer != null ) && ( wrapperPeer.IsPassword() ) );
    }

    protected override bool IsRequiredForFormCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();
      return ( ( wrapperPeer != null ) && ( wrapperPeer.IsRequiredForForm() ) );
    }

    protected override void SetFocusCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        wrapperPeer.SetFocus();
    }

    internal UIElement GetWrapper()
    {
      if( m_dataGridContext.DataGridControl.IsItemItsOwnContainer( m_item ) )
        return m_item as UIElement;

      return m_dataGridContext.CustomItemContainerGenerator.ContainerFromItem( m_item ) as UIElement;
    }

    internal AutomationPeer GetWrapperPeer()
    {
      UIElement wrapper = this.GetWrapper();

      if( wrapper == null )
        return null;

      AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement( wrapper );

      if( peer != null )
      {
        peer.EventsSource = this;
        return peer;
      }

      return null;
    }

    internal DataGridItemCellAutomationPeer GetDataGridItemCellPeer( ColumnBase column )
    {
      // Force the creation of the children caching (m_dataChildren).
      this.GetChildren();

      return m_dataChildren[ column ] as DataGridItemCellAutomationPeer;
    }

    internal DataGridContextAutomationPeer GetDetailPeer( DetailConfiguration detailConfig )
    {
      return m_dataChildren[ detailConfig ] as DataGridContextAutomationPeer;
    }

    #region ISelectionItemProvider Members

    bool ISelectionItemProvider.IsSelected
    {
      get
      {
        return m_dataGridContext.SelectedItemsStore.Contains( this.Index );
      }
    }

    IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
    {
      get
      {
        return this.ProviderFromPeer( m_dataGridContext.PeerSource );
      }
    }

    void ISelectionItemProvider.AddToSelection()
    {
      if( !this.IsEnabled() )
        throw new ElementNotEnabledException();

      DataGridControl dataGridControl = m_dataGridContext.DataGridControl;

      if( ( dataGridControl.SelectionModeToUse == SelectionMode.Single ) && ( dataGridControl.SelectedItem != null )
        && ( dataGridControl.SelectedItem != this.Item ) )
      {
        throw new InvalidOperationException( "An attempt was made to select more than 1 item." );
      }

      dataGridControl.SelectionChangerManager.Begin();

      try
      {
        dataGridControl.SelectionChangerManager.SelectItems(
          m_dataGridContext,
          new SelectionRangeWithItems( this.Index, m_item ) );
      }
      finally
      {
        dataGridControl.SelectionChangerManager.End( false, true, true );
      }
    }

    void ISelectionItemProvider.RemoveFromSelection()
    {
      if( !base.IsEnabled() )
        throw new ElementNotEnabledException();

      DataGridControl dataGridControl = m_dataGridContext.DataGridControl;
      dataGridControl.SelectionChangerManager.Begin();

      try
      {
        dataGridControl.SelectionChangerManager.UnselectItems(
          m_dataGridContext,
          new SelectionRangeWithItems( this.Index, m_item ) );
      }
      finally
      {
        dataGridControl.SelectionChangerManager.End( false, true, true );
      }
    }

    void ISelectionItemProvider.Select()
    {
      if( !this.IsEnabled() )
        throw new ElementNotEnabledException();

      DataGridControl dataGridControl = m_dataGridContext.DataGridControl;
      dataGridControl.SelectionChangerManager.Begin();

      try
      {
        dataGridControl.SelectionChangerManager.SelectJustThisItem( m_dataGridContext, this.Index, m_item );
      }
      finally
      {
        dataGridControl.SelectionChangerManager.End( false, true, true );
      }
    }

    internal void RaiseAutomationIsSelectedChanged( bool isSelected )
    {
      this.RaisePropertyChangedEvent( SelectionItemPatternIdentifiers.IsSelectedProperty, !isSelected, isSelected );
    }

    #endregion

    #region IVirtualizedItemProvider Members

    public void Realize()
    {
      this.ScrollIntoView();
    }

    #endregion

    #region IScrollItemProvider Members

    public void ScrollIntoView()
    {
      if( m_dataGridContext == null )
        return;

      DataGridControl dataGridControl = m_dataGridContext.DataGridControl;

      if( dataGridControl == null )
        return;

      if( m_dataGridContext.CustomItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated )
      {
        dataGridControl.BringItemIntoViewHelper( m_dataGridContext, m_item );
      }
      else
      {
        this.Dispatcher.BeginInvoke( DispatcherPriority.Loaded, new DispatcherOperationCallback( this.BringItemIntoViewHelper ), m_item );
      }
    }

    private object BringItemIntoViewHelper( object item )
    {
      if( m_dataGridContext == null )
        return null;

      DataGridControl dataGridControl = m_dataGridContext.DataGridControl;

      if( dataGridControl == null )
        return null;

      dataGridControl.BringItemIntoViewHelper( m_dataGridContext, item );
      return null;
    }

    #endregion

    private Hashtable m_dataChildren;
    private object m_item;
    private int m_index;
    private DataGridContext m_dataGridContext;
  }
}
