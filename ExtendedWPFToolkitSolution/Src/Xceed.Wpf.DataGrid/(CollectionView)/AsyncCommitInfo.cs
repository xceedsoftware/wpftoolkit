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
using System.Windows.Threading;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  public class AsyncCommitInfo : IDisposable
  {
    #region STATIC MEMBERS


    #endregion STATIC MEMBERS

    #region CONSTRUCTORS

    internal AsyncCommitInfo(
      Dispatcher dispatcher,
      Action<AsyncCommitInfo> beginCommitDelegate,
      Action<AsyncCommitInfo> endCommitDelegate,
      Action<AsyncCommitInfo> commitErrorChangedDelegate,
      VirtualizedItemInfo[] virtualizedItemInfos )
    {
      m_dispatcher = dispatcher;

      m_beginCommitDelegate = beginCommitDelegate;
      m_endCommitDelegate = endCommitDelegate;
      m_commitErrorChangedDelegate = commitErrorChangedDelegate;

      m_virtualizedItemInfos = virtualizedItemInfos;
    }

    #endregion CONSTRUCTORS


    #region Items Property

    public VirtualizedItemInfo[] VirtualizedItemInfos
    {
      get
      {
        return m_virtualizedItemInfos;
      }
    }

    #endregion Items Property

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

          if( m_commitErrorChangedDelegate != null )
          {
            m_dispatcher.Invoke(
              DispatcherPriority.Send,
              m_commitErrorChangedDelegate,
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
        return m_isDisposed;
      }
    }

    #endregion


    #region PUBLIC METHODS

    public void EndCommit()
    {
      Debug.Assert( !this.IsDisposed );

      if( m_commitEndedDispatched )
        throw new InvalidOperationException( "An attempt was made to call EndCommit when it has already been called for this AsyncCommitInfo." );

      m_dispatcher.BeginInvoke(
        DispatcherPriority.Send,
        m_endCommitDelegate,
        this );

      m_commitEndedDispatched = true;
    }

    #endregion PUBLIC METHODS

    #region INTERNAL METHODS

    internal void BeginCommit()
    {
      Debug.Assert( !this.IsDisposed );

      m_dispatcher.BeginInvoke(
        DispatcherPriority.Background,
        m_beginCommitDelegate,
        this );
    }

    #endregion INTERNAL METHODS

    #region PRIVATE FIELDS

    private VirtualizedItemInfo[] m_virtualizedItemInfos;
    private bool m_commitEndedDispatched;
    private bool m_isDisposed;

    private object m_error;

    private Dispatcher m_dispatcher;
    private Action<AsyncCommitInfo> m_beginCommitDelegate;
    private Action<AsyncCommitInfo> m_endCommitDelegate;
    private Action<AsyncCommitInfo> m_commitErrorChangedDelegate;

    #endregion PRIVATE FIELDS

    #region IDisposable Members

    public void Dispose()
    {
      if( m_isDisposed )
      {
        Debug.Assert( false, "The AsyncQueryInfo is disposed" );
        return;
      }

      m_dispatcher = null;

      m_beginCommitDelegate = null;
      m_endCommitDelegate = null;
      m_commitErrorChangedDelegate = null;

      m_isDisposed = true;
    }

    #endregion
  }
}
