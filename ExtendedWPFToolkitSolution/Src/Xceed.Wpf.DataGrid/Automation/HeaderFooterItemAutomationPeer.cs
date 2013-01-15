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
using System.Windows;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid.Automation
{
  internal class HeaderFooterItemAutomationPeer : AutomationPeer, IVirtualizedItemProvider, IScrollItemProvider
  {
    public HeaderFooterItemAutomationPeer( DataGridContext dataGridContext, object owner )
    {
      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      if( owner == null )
        throw new ArgumentNullException( "owner" );

      Debug.Assert( ( owner is GroupHeaderFooterItem ) || ( owner is DataTemplate ) || ( owner is HeaderFooterItem ) );

      m_owner = owner;
      m_dataGridContext = dataGridContext;
    }

    #region Owner Property

    public object Owner
    {
      get
      {
        return m_owner;
      }
    }

    #endregion

    #region Type Property

    internal HeaderFooterType Type
    {
      get
      {
        return m_headerFooterType;
      }
    }

    #endregion

    #region Index Property

    internal int Index
    {
      get
      {
        return m_index;
      }
    }

    #endregion

    public override object GetPattern( PatternInterface patternInterface )
    {
      if( ( patternInterface == PatternInterface.VirtualizedItem )
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
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetAutomationControlType();

      return AutomationControlType.Custom;
    }

    protected override string GetClassNameCore()
    {
      return "HeaderFooterItem";
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
      switch( m_headerFooterType )
      {
        case HeaderFooterType.FixedHeader:
          return "FixedHeader_" + m_index.ToString( CultureInfo.InvariantCulture );

        case HeaderFooterType.Header:
          return "Header_" + m_index.ToString( CultureInfo.InvariantCulture );

        case HeaderFooterType.FixedFooter:
          return "FixedFooter_" + m_index.ToString( CultureInfo.InvariantCulture );

        default: // HeaderFooterType.Footer
          return "Footer_" + m_index.ToString( CultureInfo.InvariantCulture );
      }
    }

    protected override Rect GetBoundingRectangleCore()
    {
      AutomationPeer wrapperPeer = this.GetWrapperPeer();

      if( wrapperPeer != null )
        return wrapperPeer.GetBoundingRectangle();

      return Rect.Empty;
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

      if( wrapperPeer != null )
        return wrapperPeer.GetName();

      return string.Empty;
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

    internal void SetExtraInfo( HeaderFooterType headerFooterType, int index )
    {
      m_headerFooterType = headerFooterType;
      m_index = index;
    }

    internal UIElement GetWrapper()
    {
      HeaderFooterItem headerFooterItem = m_owner as HeaderFooterItem;

      if( headerFooterItem == null )
      {
        headerFooterItem =
          m_dataGridContext.GetContainerFromItem( m_owner ) as HeaderFooterItem;
      }

      if( headerFooterItem == null )
        return null;

      IDataGridItemContainer dataGridItemContainer = HeaderFooterItem.FindIDataGridItemContainerInChildren(
        headerFooterItem, headerFooterItem.AsVisual() );

      if( dataGridItemContainer != null )
        return dataGridItemContainer as UIElement;

      return headerFooterItem;
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
        dataGridControl.BringItemIntoViewHelper( m_dataGridContext, m_owner );
      }
      else
      {
        this.Dispatcher.BeginInvoke( DispatcherPriority.Loaded, new DispatcherOperationCallback( this.BringItemIntoViewHelper ), m_owner );
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

    private DataGridContext m_dataGridContext;
    private object m_owner;
    private HeaderFooterType m_headerFooterType;
    private int m_index;

    public enum HeaderFooterType
    {
      FixedHeader,
      Header,
      Footer,
      FixedFooter
    }
  }
}
