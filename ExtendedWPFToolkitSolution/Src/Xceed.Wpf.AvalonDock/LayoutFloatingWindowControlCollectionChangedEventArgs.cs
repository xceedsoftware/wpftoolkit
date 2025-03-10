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

using System;
using System.Collections.Specialized;

namespace Xceed.Wpf.AvalonDock
{
  public class LayoutFloatingWindowControlCollectionChangedEventArgs : EventArgs
  {
    public LayoutFloatingWindowControlCollectionChangedEventArgs( NotifyCollectionChangedEventArgs collectionChangedEventArgs )
    {
      CollectionChangedEventArgs = collectionChangedEventArgs;
    }

    public NotifyCollectionChangedEventArgs CollectionChangedEventArgs
    {
      get;
      private set;
    }
  }
}
