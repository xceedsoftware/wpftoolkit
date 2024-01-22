/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2023 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;
using System.Windows.Controls;

namespace Xceed.Wpf.Toolkit
{
  public class TabItemEventArgs : RoutedEventArgs
  {
    public TabItem TabItem
    {
      get;
      private set;
    }


    public TabItemEventArgs( TabItem tabItem )
      : base()
    {
      this.TabItem = tabItem;
    }
  }
}

