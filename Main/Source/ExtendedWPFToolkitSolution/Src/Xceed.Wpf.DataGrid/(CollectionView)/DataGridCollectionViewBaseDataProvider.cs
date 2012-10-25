﻿/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class DataGridCollectionViewBaseDataProvider : DataSourceProvider, IWeakEventListener
  {
    #region CONSTRUCTORS

    internal DataGridCollectionViewBaseDataProvider( DataGridCollectionViewSourceBase parentSource )
      : base()
    {
      if( parentSource == null )
        throw new ArgumentNullException( "parentSource" );

      m_parentSource = parentSource;
    }

    #endregion CONSTRUCTORS

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( DataChangedEventManager ) )
      {
        this.Refresh();
        return true;
      }

      return false;
    }

    #endregion

    #region CurrentView Property

    public DataGridCollectionViewBase CurrentView
    {
      get
      {
        return m_currentView;
      }
    }

    private DataGridCollectionViewBase m_currentView;

    #endregion CurrentView Property

    #region PUBLIC METHODS

    public void DelayRefresh( Dispatcher dispatcher, DispatcherPriority priority )
    {
      // No need to call Refresh again since there is already one that is pending.
      if( m_delayedRefreshPending )
        return;

      // Call Refresh on the dispatcher with the specified priority.
      DispatcherOperation operation = dispatcher.BeginInvoke(
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

    #endregion PUBLIC METHODS

    #region PROTECTED METHODS

    protected override void BeginQuery()
    {
      Exception queryException = null;

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

    #endregion PROTECTED METHODS

    #region INTERNAL PROPERTIES

    internal DataGridCollectionViewSourceBase ParentSource
    {
      get
      {
        return m_parentSource;
      }
    }

    #endregion INTERNAL PROPERTIES

    #region PRIVATE METHODS

    internal abstract DataGridCollectionViewBase EnsureDataGridCollectionViewBaseCore();

    private void EnsureDataGridCollectionViewBase()
    {
      try
      {
        DataGridCollectionViewBase view = this.EnsureDataGridCollectionViewBaseCore();

        if( view == null )
        {
          if( m_currentView != null )
          {
            m_currentView.ParentCollectionViewSourceBase = null;
            m_currentView = null;
          }
        }

        if( m_currentView != view )
        {
          DataGridCollectionViewBase oldView = m_currentView;

          m_currentView = view;

          if( ( oldView != null ) && ( oldView.ItemProperties != null ) )
          {
            // Ensure to unregister the ItemsProperties collection from 
            // events of the ItemProperties it contains to avoid memory leaks.
            // The ItemProperties are reused when a new DataGridCollectionViewBase
            // is generated by the DataGridCollectionViewBaseDataProvider and 
            // the ItemProperties collection of this new DataGridCollectionViewBase
            // will also register to those events. 
            // We can safely unregister from those here wince the old 
            // DataGridCollectionViewBase will not be reused anywhere else.
            oldView.ItemProperties.UnregisterDataGridItemPropertiesEvents();
          }
        }

        if( m_currentView != null )
          m_currentView.ParentCollectionViewSourceBase = m_parentSource;
      }
      catch
      {
        m_currentView.ParentCollectionViewSourceBase = null;
        m_currentView = null;
        throw;
      }
    }

    private void OnDelayedRefreshCompleted( object sender, EventArgs e )
    {
      m_delayedRefreshPending = false;
    }

    #endregion PRIVATE METHODS

    #region PRIVATE FIELDS

    private bool m_delayedRefreshPending;
    private DataGridCollectionViewSourceBase m_parentSource;

    #endregion
  }
}
