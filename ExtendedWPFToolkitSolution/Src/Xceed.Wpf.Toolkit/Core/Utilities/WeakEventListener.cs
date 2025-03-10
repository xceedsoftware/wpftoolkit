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
using System.Windows;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  internal class WeakEventListener<TArgs> : IWeakEventListener where TArgs : EventArgs
  {
    private Action<object, TArgs> _callback;

    public WeakEventListener( Action<object, TArgs> callback )
    {
      if( callback == null )
        throw new ArgumentNullException( "callback" );

      _callback = callback;
    }

    public bool ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      _callback( sender, ( TArgs )e );
      return true;
    }
  }
}
