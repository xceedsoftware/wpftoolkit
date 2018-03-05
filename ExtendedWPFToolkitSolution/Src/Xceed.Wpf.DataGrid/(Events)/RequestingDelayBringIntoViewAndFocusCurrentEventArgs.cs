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

namespace Xceed.Wpf.DataGrid
{
  internal class RequestingDelayBringIntoViewAndFocusCurrentEventArgs : EventArgs
  {
    internal RequestingDelayBringIntoViewAndFocusCurrentEventArgs( AutoScrollCurrentItemSourceTriggers trigger )
    {
      m_trigger = trigger;
    }

    #region Trigger Property

    internal AutoScrollCurrentItemSourceTriggers Trigger
    {
      get
      {
        return m_trigger;
      }
    }

    private readonly AutoScrollCurrentItemSourceTriggers m_trigger;

    #endregion
  }
}
