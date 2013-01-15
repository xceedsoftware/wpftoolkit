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
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using Xceed.Utils.Math;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid.Views
{
  internal class ColumnVirtualizationManager : DependencyObject, IWeakEventListener
  {
    static ColumnVirtualizationManager()
    {
      ColumnVirtualizationManager.ColumnVirtualizationManagerProperty = ColumnVirtualizationManager.ColumnVirtualizationManagerPropertyKey.DependencyProperty;
    }

    public ColumnVirtualizationManager( DataGridContext dataGridContext )
    {
      Debug.Assert( dataGridContext != null );
      m_dataGridContext = dataGridContext;

      // Assign the ColumnVirtualizationManager to the DataGridContext
      ColumnVirtualizationManager.SetColumnVirtualizationManager( m_dataGridContext, this );

      this.Initialize();

      unchecked
      {
        m_version++;
      }
    }

    #region ColumnVirtualizationManager Property

    private static readonly DependencyPropertyKey ColumnVirtualizationManagerPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly( "ColumnVirtualizationManager", typeof( ColumnVirtualizationManager ), typeof( ColumnVirtualizationManager ), new UIPropertyMetadata( null ) );

    public static readonly DependencyProperty ColumnVirtualizationManagerProperty;

    public static ColumnVirtualizationManager GetColumnVirtualizationManager( DependencyObject obj )
    {
      return ( ColumnVirtualizationManager )obj.GetValue( ColumnVirtualizationManager.ColumnVirtualizationManagerProperty );
    }

    internal static void SetColumnVirtualizationManager( DependencyObject obj, ColumnVirtualizationManager value )
    {
      obj.SetValue( ColumnVirtualizationManager.ColumnVirtualizationManagerPropertyKey, value );
    }

    internal static void ClearColumnVirtualizationManager( DependencyObject obj )
    {
      obj.ClearValue( ColumnVirtualizationManager.ColumnVirtualizationManagerPropertyKey );
    }

    #endregion

    #region NeedsUpdate Property

    public bool NeedsUpdate
    {
      get
      {
        return ( m_currentVersion != m_version );
      }
    }

    #endregion

    #region DataGridContext Property

    public DataGridContext DataGridContext
    {
      get
      {
        return m_dataGridContext;
      }
    }

    #endregion

    #region Version Proptety

    public int Version
    {
      get
      {
        return m_version;
      }
    }

    #endregion

    public virtual void Update()
    {
      if( this.NeedsUpdate == false )
        return;

      this.PreUpdate();

      this.ResetInternalState();

      this.DoUpdate();

      this.PostUpdate();
    }

    public virtual void CleanManager()
    {
      this.Uninitialize();
    }

    protected virtual void PreUpdate()
    {
      // To be able to keep the previous state of the manager before updating
    }

    protected virtual void DoUpdate()
    {
    }

    // Called after PreUpdate in Update method
    protected virtual void ResetInternalState()
    {
    }

    protected virtual void PostUpdate()
    {
      // Update was completed correctly
      m_currentVersion = m_version;
    }

    // Called after attaching ColumnVirtualizationManager to DataGridContext
    protected virtual void Initialize()
    {
      m_dataGridContext.PropertyChanged += new PropertyChangedEventHandler( this.DataGridContext_PropertyChanged );
      ItemsSourceChangeCompletedEventManager.AddListener( m_dataGridContext.DataGridControl, this );
      ViewChangedEventManager.AddListener( m_dataGridContext.DataGridControl, this );
      ThemeChangedEventManager.AddListener( m_dataGridContext.DataGridControl, this );
      VisibleColumnsUpdatedEventManager.AddListener( m_dataGridContext.Columns, this );
    }

    // Called before detaching ColumnVirtualizationManager from DataGridContext
    protected virtual void Uninitialize()
    {
      this.ResetInternalState();

      m_dataGridContext.PropertyChanged -= new PropertyChangedEventHandler( this.DataGridContext_PropertyChanged );
      ItemsSourceChangeCompletedEventManager.RemoveListener( m_dataGridContext.DataGridControl, this );
      ViewChangedEventManager.RemoveListener( m_dataGridContext.DataGridControl, this );
      ThemeChangedEventManager.RemoveListener( m_dataGridContext.DataGridControl, this );
      VisibleColumnsUpdatedEventManager.RemoveListener( m_dataGridContext.Columns, this );

      m_dataGridContext = null;
    }

    protected virtual void IncrementVersion( object parameters )
    {
      unchecked
      {
        m_version++;
      }
    }

    protected virtual void DataGridContext_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      Debug.Assert( string.IsNullOrEmpty( e.PropertyName ) == false );

      if( string.IsNullOrEmpty( e.PropertyName ) == true )
        return;

      switch( e.PropertyName )
      {
        case "CurrentColumn":
          this.IncrementVersion( null );
          break;
      }
    }

    private int m_version; // = 0;
    private int m_currentVersion; // = 0;

    private DataGridContext m_dataGridContext; // = null;

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceivedWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceivedWeakEvent( Type managerType, object sender, EventArgs e )
    {
      bool handled = false;
      bool detachFromDataGridContext = false;

      if( managerType == typeof( VisibleColumnsUpdatedEventManager ) )
      {
        this.IncrementVersion( null );

        handled = true;
      }
      else if( managerType == typeof( ViewChangedEventManager ) )
      {
        detachFromDataGridContext = true;
        this.IncrementVersion( null );

        handled = true;
      }
      else if( managerType == typeof( ThemeChangedEventManager ) )
      {
        detachFromDataGridContext = true;
        this.IncrementVersion( null );

        handled = true;
      }
      else if( managerType == typeof( ItemsSourceChangeCompletedEventManager ) )
      {
        detachFromDataGridContext = true;
        handled = true;
      }

      if( detachFromDataGridContext == true )
      {
        if( m_dataGridContext != null )
        {
          // Detach the ColumnVirtualizationManager from the DataGridContext and detach from the DataGridContext
          ColumnVirtualizationManager.SetColumnVirtualizationManager( m_dataGridContext, null );

          this.Uninitialize();
        }
      }

      return handled;
    }

    #endregion
  }
}
