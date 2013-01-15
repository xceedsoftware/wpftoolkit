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

namespace Xceed.Wpf.DataGrid.Automation
{
  public class RowAutomationPeer : FrameworkElementAutomationPeer
  {
    public RowAutomationPeer( Row owner )
      : base( owner )
    {
    }

    #region Owner Property

    public new Row Owner
    {
      get
      {
        return base.Owner as Row;
      }
    }

    #endregion

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
      return AutomationControlType.Pane;
    }
  }
}
