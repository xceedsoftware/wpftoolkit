/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation.Peers;

namespace Xceed.Wpf.DataGrid.Automation
{
  public class ColumnManagerRowAutomationPeer : RowAutomationPeer
  {
    public ColumnManagerRowAutomationPeer( ColumnManagerRow owner )
      : base( owner )
    {
    }

    public new ColumnManagerRow Owner
    {
      get
      {
        return base.Owner as ColumnManagerRow;
      }
    }

    protected override bool IsContentElementCore()
    {
      return false;
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
      return AutomationControlType.Header;
    }
  }
}
