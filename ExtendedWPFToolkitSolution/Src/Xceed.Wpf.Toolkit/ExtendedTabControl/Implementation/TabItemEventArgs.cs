/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2022 Xceed Software Inc.

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
  /// <summary>
  /// Provides data for the ExtendedTabControl.TabItemAdded and ExtendedTabControl.TabItemRemoved events.
  /// </summary>
  /// <QualityBand>Preview</QualityBand>
  public class TabItemEventArgs : RoutedEventArgs
  {
    /// <summary>
    /// Gets the TabItem that is passed in TabItemAdded/TabItemRemoved events. 
    /// </summary>
    public TabItem TabItem
    {
      get;
      private set;
    }


    /// <summary>
    /// Initializes a new instance of the TabItemEventArgs class.
    /// </summary>
    /// <param name="tabItem">TabItem to add or to remove.</param>
    public TabItemEventArgs( TabItem tabItem )
      : base()
    {
      this.TabItem = tabItem;
    }
  }
}

