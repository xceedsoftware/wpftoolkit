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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.Toolkit.Core;
using System.Windows;

namespace Xceed.Wpf.Toolkit
{
  public class ItemAddingEventArgs : CancelRoutedEventArgs
  {
    #region Constructor

    public ItemAddingEventArgs( RoutedEvent itemAddingEvent, object itemAdding )
      : base( itemAddingEvent )
    {
      Item = itemAdding;
    }

    #endregion

    #region Properties

    #region Item Property

    public object Item
    {
      get;
      set;
    }

    #endregion

    #endregion //Properties
  }
}
