/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid
{
  public class AsyncQueryInfo : IDisposable
  {
    #region CONSTRUCTORS

    internal AsyncQueryInfo(
      Dispatcher dispatcher,
      Action<AsyncQueryInfo> beginQueryDelegate,
      Action<AsyncQueryInfo> abortQueryDelegate,
      Action<AsyncQueryInfo, object[]> endQueryDelegate,
      Action<AsyncQueryInfo> queryErrorChangedDelegate,
      Action<AsyncQueryInfo> builtInAbortDelegate,
      int startIndex,
      int requestedItemCount )
    {
      m_dispatcher = dispatcher;
      m_builtInAbortDelegate = builtInAbortDelegate;
      m_beginQueryDelegate = beginQueryDelegate;
      m_abortQueryDelegate = abortQueryDelegate;
      m_endQueryDelegate = endQueryDelegate;

      m_queryErrorChangedDelegate = queryErrorChangedDelegate;

      m_startIndex = startIndex;
      m_requestedItemCount = requestedItemCount;
    }

    #endregion CONSTRUCTORS

    #region BeginQueryDelegateInvoked Private Property

    private bool BeginQueryDelegateInvoked
    {
      get
      {
        return m_flags[ ( int )AsyncQueryInfoFlags.BeginQueryDelegateInvoked ];
      }
      set
      {
        m_flags[ ( int )AsyncQueryInfoFlags.BeginQueryDelegateInvoked ] = value;
      }
    }

    #endregion

    #region EndQueryDelegateDispatcherOperation Private Property

    private DispatcherOperation EndQueryDelegateDispatcherOperation
    {
      get;
      set;
    }

    #endregion

    #region StartIndex Property

    public int StartIndex
    {
      get
      {
        return m_startIndex;
      }
    }

    #endregion StartIndex Property

    #region RequestedItemCount Property

    public int RequestedItemCount
    {
      get
      {
        return m_requestedItemCount;
      }
    }

    #endregion RequestedItemCount  Property

    #region ShouldAbort Property

    public bool ShouldAbort
    {
      get
      {
        return m_flags[ ( int )AsyncQueryInfoFlags.ShouldAbort ];
      }
      private set
      {
        m_flags[ ( int )AsyncQueryInfoFlags.ShouldAbort ] = value;
      }
    }

    #endregion ShouldAbort Property

    #region AsyncState Property

    public object AsyncState
    {
      get;
      set;
    }

    #endregion AsyncState Property

    #region Error Property

    public object Error
    {
      get
      {
        return m_error;
      }
      set
      {
        Debug.Assert( !this.IsDisposed );

        if( m_error != value )
        {
          m_error = value;

          if( m_queryErrorChangedDelegate != null )
          {
            m_dispatcher.Invoke(
              DispatcherPriority.Send,
              m_queryErrorChangedDelegate,
              this );
          }
        }
      }
    }

    #endregion Error Property

    #region IsDisposed Internal Property

    internal bool IsDisposed
    {
      get
      {
        return m_flags[ ( int )AsyncQueryInfoFlags.IsDisposed ];
      }
      private set
      {
        m_flags[ ( int )AsyncQueryInfoFlags.IsDisposed ] = value;
      }
    }

    #endregion

    #region PUBLIC METHODS

    public void EndQuery( object[] items )
    {
      // Ignore this query when disposed
      if( this.IsDisposed )
        return;

      if( this.EndQueryDelegateDispatcherOperation != null )
        throw new InvalidOperationException( "An attempt was made to call EndQuery when it has already been called for this AsyncQueryInfo." );

      if( this.ShouldAbort )
        return;

      this.EndQueryDelegateDispatcherOperation = 
        m_dispatcher.BeginInvoke( DispatcherPriority.Background, m_endQueryDelegate,
        this, new object[] { items } );
    }

    #endregion PUBLIC METHODS

    #region INTERNAL METHODS

    internal void QueueQuery()
    {
      Debug.Assert( !this.IsDisposed, "The AsyncQueryInfo is disposed" );

      this.ShouldAbort = false;
      this.BeginQueryDelegateInvoked = false;

      var endQueryDelegateOperation = this.EndQueryDelegateDispatcherOperation;
      if( endQueryDelegateOperation != null )
      {
        this.EndQueryDelegateDispatcherOperation.Abort();
        this.EndQueryDelegateDispatcherOperation = null;
      }

#if SILVERLIGHT
      m_dispatcher.BeginInvoke( new Action( this.BeginQuery ) );
#else
      m_beginQueryDispatcherOperation = m_dispatcher.BeginInvoke( DispatcherPriority.Background,
        new Action( this.BeginQueryDispatcherCallback ) );
#endif
    }

    private void BeginQueryDispatcherCallback()
    {
      m_beginQueryDispatcherOperation = null;

      Debug.Assert( !this.IsDisposed );

      if( this.ShouldAbort )
      {
        Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "BUILT-IN BeginQueryDispatcherCallback Aborted! (" + this.GetHashCode() + ") For page at starting index: " + m_startIndex.ToString() );
        m_builtInAbortDelegate( this );
        return;
      }

      m_beginQueryDelegate( this );
      this.BeginQueryDelegateInvoked = true;
    }

    internal void AbortQuery()
    {
      Debug.Assert( !this.IsDisposed );

      // Already dispatcher on the UI Thread the method that will fill the page.  
      // If it did have one, we would have invoked instead of BeginInvoked the endQuery delegate.
      var endQueryDelegateOperation = this.EndQueryDelegateDispatcherOperation;
      if( endQueryDelegateOperation != null )
      {
        this.EndQueryDelegateDispatcherOperation.Abort();
        this.EndQueryDelegateDispatcherOperation = null;
      }

      if( this.ShouldAbort )
      {
        Debug.Assert( m_beginQueryDispatcherOperation == null );
        m_builtInAbortDelegate( this );
        return;
      }

      this.ShouldAbort = true;

      // If the BeginQuery already had time to execute, then we know the user was notified of the query request.  
      // Notify him to cancel the query as soon as possible.
      if( this.BeginQueryDelegateInvoked )
      {
        Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "CALLING Abort Query! For page at starting index: " + m_startIndex.ToString() );
        m_dispatcher.BeginInvoke( DispatcherPriority.Send, m_abortQueryDelegate, this );
      }
      else
      {
        // BeginQueryDispatcherCallback was dispatched
        // but not executed yet, abort it and return
        if( m_beginQueryDispatcherOperation != null )
        {
          m_beginQueryDispatcherOperation.Abort();
          m_beginQueryDispatcherOperation = null;
        }

        m_builtInAbortDelegate( this );
      }
    }

    #endregion INTERNAL METHODS

    #region PRIVATE FIELDS

    private int m_startIndex;
    private int m_requestedItemCount;

    private BitVector32 m_flags = new BitVector32();

    private object m_error;

    private Dispatcher m_dispatcher;

    private DispatcherOperation m_beginQueryDispatcherOperation;
    private Action<AsyncQueryInfo> m_builtInAbortDelegate;
    private Action<AsyncQueryInfo> m_beginQueryDelegate;
    private Action<AsyncQueryInfo> m_abortQueryDelegate;
    private Action<AsyncQueryInfo, object[]> m_endQueryDelegate;
    private Action<AsyncQueryInfo> m_queryErrorChangedDelegate;

    #endregion PRIVATE FIELDS

    #region AsyncQueryInfoFlags Private Enum

    [Flags]
    private enum AsyncQueryInfoFlags
    {
      ShouldAbort = 1,
      EndQueryDelegateInvoked = 2,
      BeginQueryDelegateInvoked = 4,
      IsDisposed = 8,
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      m_dispatcher = null;

      Debug.Assert( m_beginQueryDispatcherOperation == null, "A BeginQueryDispatcherCallback is still pending on the Dispatcher" );

      m_beginQueryDispatcherOperation = null;
      m_builtInAbortDelegate = null;
      m_abortQueryDelegate = null;
      m_beginQueryDelegate = null;
      m_endQueryDelegate = null;
      m_queryErrorChangedDelegate = null;

      this.IsDisposed = true;
    }

    #endregion
  }
}
