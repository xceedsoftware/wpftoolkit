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
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Xceed.Wpf.Toolkit.Core
{
  public delegate void CancelRoutedEventHandler( object sender, CancelRoutedEventArgs e );

  /// <summary>
  /// An event data class that allows to inform the sender that the handler wants to cancel
  /// the ongoing action.
  /// 
  /// The handler can set the "Cancel" property to false to cancel the action.
  /// </summary>
  public class CancelRoutedEventArgs : RoutedEventArgs
  {
    public CancelRoutedEventArgs()
      : base()
    {
    }

    public CancelRoutedEventArgs( RoutedEvent routedEvent )
      : base( routedEvent )
    {
    }

    public CancelRoutedEventArgs( RoutedEvent routedEvent, object source )
      : base( routedEvent, source )
    {
    }

    #region Cancel Property

    public bool Cancel
    {
      get;
      set;
    }

    #endregion Cancel Property
  }
}
