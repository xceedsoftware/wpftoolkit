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
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class DataGridCollectionViewBaseDataProvider : DataSourceProvider, IWeakEventListener
  {
    internal DataGridCollectionViewBaseDataProvider( DataGridCollectionViewSourceBase parentSource )
      : base()
    {
      if( parentSource == null )
        throw new ArgumentNullException( "parentSource" );

      m_parentSource = parentSource;
    }

    #region CurrentView Property

    public DataGridCollectionViewBase CurrentView
    {
      get
      {
        return m_currentView;
      }
    }

    private DataGridCollectionViewBase m_currentView;

    #endregion

    #region ParentSource Internal Property

    internal DataGridCollectionViewSourceBase ParentSource
    {
      get
      {
        return m_parentSource;
      }
    }

    private readonly DataGridCollectionViewSourceBase m_parentSource;

    #endregion

    public void DelayRefresh( Dispatcher dispatcher, DispatcherPriority priority )
    {
      // No need to call Refresh again since there is already one that is pending.
      if( m_delayedRefreshPending )
        return;

      // Call Refresh on the dispatcher with the specified priority.
      var operation = dispatcher.BeginInvoke(
        priority,
        new Action( delegate
        {
          this.Refresh();
        } ) );

      // If we're not already completed, set the internal flag to prevent
      // another Refresh from being stacked on register to be notified
      // when the operation complete.
      if( operation.Status != DispatcherOperationStatus.Completed )
      {
        m_delayedRefreshPending = true;
        operation.Completed += new EventHandler( this.OnDelayedRefreshCompleted );
      }
    }

    protected override void BeginQuery()
    {
      var queryException = default( Exception );

      try
      {
        this.EnsureDataGridCollectionViewBase();
      }
      catch( Exception exception )
      {
        queryException = exception;
      }

      this.OnQueryFinished( m_currentView, queryException, null, null );
    }

    internal abstract DataGridCollectionViewBase EnsureDataGridCollectionViewBaseCore();

    private void EnsureDataGridCollectionViewBase()
    {
      var success = false;

      try
      {
        var newView = this.EnsureDataGridCollectionViewBaseCore();
        if( newView != m_currentView )
        {
          this.ClearView();

          m_currentView = newView;

          if( m_currentView != null )
          {
            m_currentView.ParentCollectionViewSourceBase = m_parentSource;
          }
        }

        Debug.Assert( ( m_currentView == null ) || ( m_currentView.ParentCollectionViewSourceBase == m_parentSource ) );

        success = true;
      }
      finally
      {
        if( !success )
        {
          this.ClearView();
        }
      }
    }

    private void ClearView()
    {
      if( m_currentView == null )
        return;

      var view = m_currentView;
      m_currentView = null;

      view.Dispose();
    }

    private void OnDelayedRefreshCompleted( object sender, EventArgs e )
    {
      m_delayedRefreshPending = false;
    }

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( DataChangedEventManager ) )
      {
        this.Refresh();
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    private bool m_delayedRefreshPending;
  }
}
