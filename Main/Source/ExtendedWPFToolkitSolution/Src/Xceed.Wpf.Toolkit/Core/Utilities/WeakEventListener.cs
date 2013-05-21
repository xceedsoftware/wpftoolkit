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
using System.Linq;
using System.Text;
using System.Windows;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  internal class WeakEventListener<TArgs> : IWeakEventListener where TArgs : EventArgs
  {
    private Action<object,TArgs> _callback;

    public WeakEventListener(Action<object,TArgs> callback)
    {
      if( callback == null )
         throw new ArgumentNullException( "callback" );

      _callback = callback;
    }

    public bool ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      _callback(sender, (TArgs)e);
      return true;
    }
  }
}
