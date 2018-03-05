/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Threading;
using System.Windows.Threading;

namespace Xceed.Utils.Wpf
{
  internal abstract class DeferredDisposableState
  {
    protected virtual object SyncRoot
    {
      get
      {
        return null;
      }
    }

    protected virtual Dispatcher Dispatcher
    {
      get
      {
        return null;
      }
    }

    protected abstract bool IsDeferred
    {
      get;
    }

    protected abstract void Increment();
    protected abstract void Decrement();

    protected virtual void OnDeferEnding( bool disposing )
    {
    }

    protected abstract void OnDeferEnded( bool disposing );

    internal Dispatcher GetDispatcher()
    {
      return this.Dispatcher;
    }

    internal void Initialize()
    {
      var syncRoot = this.SyncRoot;
      if( syncRoot != null )
      {
        Monitor.Enter( syncRoot );
      }

      try
      {
        this.Increment();
      }
      finally
      {
        if( syncRoot != null )
        {
          Monitor.Exit( syncRoot );
        }
      }
    }

    internal void Dispose( bool disposing )
    {
      var syncRoot = this.SyncRoot;
      if( syncRoot != null )
      {
        Monitor.Enter( syncRoot );
      }

      try
      {
        this.Decrement();

        if( this.IsDeferred )
          return;

        this.OnDeferEnding( disposing );
      }
      finally
      {
        if( syncRoot != null )
        {
          Monitor.Exit( syncRoot );
        }
      }

      this.OnDeferEnded( disposing );
    }
  }
}
