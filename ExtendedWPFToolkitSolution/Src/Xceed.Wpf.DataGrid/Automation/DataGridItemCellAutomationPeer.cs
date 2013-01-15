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
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid.Automation
{
  internal class DataGridItemCellAutomationPeer : AutomationPeer, ITableItemProvider
  {
    public DataGridItemCellAutomationPeer( DataGridItemAutomationPeer itemAutomationPeer, ColumnBase column )
    {

      if( itemAutomationPeer == null )
        throw new ArgumentNullException( "itemAutomationPeer" );

      if( column == null )
        throw new ArgumentNullException( "column" );

      m_itemAutomationPeer = itemAutomationPeer;
      m_column = column;

      // Call the GetWrapperPeer since it will force the events of the wrapped peer to be rerooted to us.
      AutomationPeer wrapperPeer = this.GetWrapperPeer();
    }

    #region ItemAutomationPeer Property

    public DataGridItemAutomationPeer ItemAutomationPeer
    {
      get
      {
        return m_itemAutomationPeer;
      }
    }

    #endregion

    #region Column Property

    public ColumnBase Column
    {
      get
      {
        return m_column;
      }
    }

    #endregion

    #region ColumnIndex Property

    internal int ColumnIndex
    {
      get;
      set;
    }

    #endregion

    public override object GetPattern( PatternInterface patternInterface )
    {
      switch( patternInterface )
      {
        case PatternInterface.GridItem:
        case PatternInterface.TableItem:
          return this;
      }

      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetPattern( patternInterface );

      return null;
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
      return AutomationControlType.Pane;
    }

    protected override string GetClassNameCore()
    {
      return "Cell";
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

      if( automationId == null )
        automationId = string.Empty;

      return automationId;
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
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetChildren();

      return null;
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

      if( name == null )
        name = string.Empty;

      return name;
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
      if( m_itemAutomationPeer == null )
        return null;

      Row row = m_itemAutomationPeer.GetWrapper() as Row;

      if( row == null )
        return null;

      return row.Cells[ m_column ];
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

    #region IGridItemProvider Members

    int IGridItemProvider.Column
    {
      get
      {
        return this.ColumnIndex;
      }
    }

    int IGridItemProvider.ColumnSpan
    {
      get
      {
        return 1;
      }
    }

    IRawElementProviderSimple IGridItemProvider.ContainingGrid
    {
      get
      {
        return this.ProviderFromPeer( this.ItemAutomationPeer.DataGridContext.PeerSource );
      }
    }

    int IGridItemProvider.Row
    {
      get
      {
        return m_itemAutomationPeer.Index;
      }
    }

    int IGridItemProvider.RowSpan
    {
      get
      {
        return 1;
      }
    }

    #endregion

    #region ITableItemProvider Members

    IRawElementProviderSimple[] ITableItemProvider.GetColumnHeaderItems()
    {
      DataGridItemAutomationPeer dataGridItemAutomationPeer = m_itemAutomationPeer;

      if( dataGridItemAutomationPeer == null )
        return new IRawElementProviderSimple[ 0 ];

      DataGridContext dataGridContext = dataGridItemAutomationPeer.DataGridContext;

      if( dataGridContext == null )
        return new IRawElementProviderSimple[ 0 ];

      List<AutomationPeer> headersFootersPeer = dataGridContext.Peer.GetColumnHeadersPeer();

      if( headersFootersPeer == null )
        return new IRawElementProviderSimple[ 0 ];

      int headersFootersPeerCount = headersFootersPeer.Count;
      List<IRawElementProviderSimple> rawElementProviderSimples = new List<IRawElementProviderSimple>( headersFootersPeerCount );

      for( int i = 0; i < headersFootersPeerCount; i++ )
      {
        RowAutomationPeer rowPeer = headersFootersPeer[ i ] as RowAutomationPeer;

        if( rowPeer != null )
        {
          Cell cell = rowPeer.Owner.Cells[ m_column ];

          if( cell != null )
          {
            AutomationPeer automationPeer = FrameworkElementAutomationPeer.CreatePeerForElement( cell );
            Debug.Assert( automationPeer != null );

            if( automationPeer != null )
            {
              rawElementProviderSimples.Add( this.ProviderFromPeer( automationPeer ) );
            }
          }
        }
      }

      return rawElementProviderSimples.ToArray();
    }

    IRawElementProviderSimple[] ITableItemProvider.GetRowHeaderItems()
    {
      return new IRawElementProviderSimple[ 0 ];
    }

    #endregion ITableItemProvider Members

    private DataGridItemAutomationPeer m_itemAutomationPeer;
    private ColumnBase m_column;
  }
}
