/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

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
