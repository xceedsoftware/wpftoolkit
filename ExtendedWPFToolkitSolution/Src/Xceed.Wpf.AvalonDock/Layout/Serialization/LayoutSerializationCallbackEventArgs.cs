/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.ComponentModel;

namespace Xceed.Wpf.AvalonDock.Layout.Serialization
{
  public class LayoutSerializationCallbackEventArgs : CancelEventArgs
  {
    #region constructor

    public LayoutSerializationCallbackEventArgs( LayoutContent model, object previousContent )
    {
      Cancel = false;
      Model = model;
      Content = previousContent;
    }

    #endregion

    #region Properties

    public LayoutContent Model
    {
      get; private set;
    }

    public object Content
    {
      get; set;
    }

    #endregion
  }
}
