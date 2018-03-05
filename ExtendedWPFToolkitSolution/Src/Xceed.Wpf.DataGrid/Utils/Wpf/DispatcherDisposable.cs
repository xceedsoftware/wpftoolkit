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
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Xceed.Utils.Wpf
{
  internal abstract class DispatcherDisposable : DependencyObject, IDisposable
  {
    #region Static Fields

    private static readonly object IsAlive = new object();

    #endregion

    public void Dispose()
    {
      this.Dispose( true );
      GC.SuppressFinalize( this );
    }

    protected void Dispose( bool disposing )
    {
      // Make sure we are only disposing once.
      if( Interlocked.Exchange( ref m_isAlive, null ) == null )
        return;

      // We must not try to use managed objects since we have no clue whatever they have been
      // garbage collected before the current object or not.
      if( !disposing )
        return;

      // We ask the derived class for a delegate instead of calling a protected method directly
      // because the current object could be garbage collected before the dispatcher calls the delegate.
      // We want to avoid calling a method on a object that is no longer valid.  Instead, we ask
      // the derived class to generate a delegate that will do the dispose.  For example, the delegate
      // could be a lambda that encapsulate the fields in a closure.
      var d = this.GetDisposeDelegate();
      if( d == null )
        return;

      if( object.ReferenceEquals( d.Target, this ) )
        throw new InvalidOperationException( "The dispose delegate must not target the disposable object." );

      var dispatcher = this.GetDisposeDispatcher() ?? this.Dispatcher;
      if( ( dispatcher == null ) || dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished )
        return;

      if( dispatcher.CheckAccess() )
      {
        d.Invoke( disposing );
      }
      else
      {
        dispatcher.BeginInvoke( d, DispatcherPriority.Send, disposing );
      }
    }

    protected abstract Action<bool> GetDisposeDelegate();

    protected virtual Dispatcher GetDisposeDispatcher()
    {
      return this.Dispatcher;
    }

    // This class does not cleanup unmanaged resource, so it does not need a finalizer.
    //~DispatcherDisposable()
    //{
    //  this.Dispose( false );
    //}

    private object m_isAlive = DispatcherDisposable.IsAlive;
  }
}
