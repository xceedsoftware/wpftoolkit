/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2025 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;

namespace Xceed.Wpf.Toolkit.Core
{
  public delegate void CancelRoutedEventHandler( object sender, CancelRoutedEventArgs e );

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
