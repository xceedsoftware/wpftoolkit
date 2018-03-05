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
using System.Windows.Threading;

namespace Xceed.Utils.Wpf
{
  internal sealed class DeferredDisposable : DispatcherDisposable
  {
    internal DeferredDisposable( DeferredDisposableState state )
    {
      if( state == null )
        throw new ArgumentNullException( "state" );

      m_state = state;
      m_state.Initialize();
    }

    protected override Dispatcher GetDisposeDispatcher()
    {
      var dispatcher = m_state.GetDispatcher();
      if( dispatcher != null )
        return dispatcher;

      return base.GetDisposeDispatcher();
    }

    protected override Action<bool> GetDisposeDelegate()
    {
      // We store the member in a local variable to create a closure.
      // We do not want the returned delegate to target the current object.
      var state = m_state;

      return ( disposing ) => state.Dispose( disposing );
    }

    private readonly DeferredDisposableState m_state;
  }
}
