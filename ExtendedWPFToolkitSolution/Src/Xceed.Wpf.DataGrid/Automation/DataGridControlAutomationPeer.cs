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
using System.Windows.Controls;
using System.Collections;
using System.Windows.Data;
using System.Windows.Automation.Provider;
using System.Collections.ObjectModel;
using System.Windows;

namespace Xceed.Wpf.DataGrid.Automation
{
  public class DataGridControlAutomationPeer : FrameworkElementAutomationPeer
  {
    public DataGridControlAutomationPeer( DataGridControl owner )
      : base( owner )
    {
      m_innerDataGridContextAutomationPeer = new DataGridContextAutomationPeer( owner, null, null, null );
      m_innerDataGridContextAutomationPeer.EventsSource = this;
    }

    #region DataGridControl Property

    public DataGridControl DataGridControl
    {
      get
      {
        return this.Owner as DataGridControl;
      }
    }

    #endregion

    public override object GetPattern( PatternInterface patternInterface )
    {
      if( patternInterface == PatternInterface.Scroll )
      {
        DataGridControl owner = this.DataGridControl;
        ScrollViewer scrollViewer = owner.ScrollViewer;

        if( scrollViewer != null )
        {
          AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement( scrollViewer );

          if( ( peer != null ) && ( peer is IScrollProvider ) )
          {
            peer.EventsSource = this;
            return peer;
          }
        }
      }

      return m_innerDataGridContextAutomationPeer.GetPattern( patternInterface );
    }

    protected override List<AutomationPeer> GetChildrenCore()
    {
      return m_innerDataGridContextAutomationPeer.GetChildren();
    }

    protected override string GetClassNameCore()
    {
      return "DataGridControl";
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
      return m_innerDataGridContextAutomationPeer.GetAutomationControlType();
    }

    protected override string GetItemTypeCore()
    {
      return m_innerDataGridContextAutomationPeer.GetItemType();
    }

    protected override string GetItemStatusCore()
    {
      return m_innerDataGridContextAutomationPeer.GetItemStatus();
    }

    protected override string GetAutomationIdCore()
    {
      string automationId = null;
      automationId = base.GetAutomationIdCore();

      if( string.IsNullOrEmpty( automationId ) )
      {
        automationId = "DataGridControl";
      }

      return automationId;
    }

    private DataGridContextAutomationPeer m_innerDataGridContextAutomationPeer;
  }
}
