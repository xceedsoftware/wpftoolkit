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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

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

    private static readonly DependencyPropertyKey ColumnVirtualizationManagerPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "ColumnVirtualizationManager",
      typeof( ColumnVirtualizationManager ),
      typeof( ColumnVirtualizationManager ),
      new UIPropertyMetadata( null, new PropertyChangedCallback( ColumnVirtualizationManager.OnColumnVirtualizationManagerChanged ) ) );

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

    private static void OnColumnVirtualizationManagerChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as ColumnVirtualizationManager;
      if( self == null )
        return;

      var dataGridContext = self.DataGridContext;
      if( dataGridContext == null )
        return;

      var column = dataGridContext.Columns.MainColumn;
      if( column == null )
        return;

      column.RefreshDraggableStatus();
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

    private DataGridContext m_dataGridContext; //null;

    #endregion

    #region Version Proptety

    public int Version
    {
      get
      {
        return m_version;
      }
    }

    private int m_version; //0;

    #endregion

    public void Update()
    {
      if( !this.NeedsUpdate )
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
      PropertyChangedEventManager.AddListener( m_dataGridContext, this, string.Empty );
      ItemsSourceChangeCompletedEventManager.AddListener( m_dataGridContext.DataGridControl, this );
      ViewChangedEventManager.AddListener( m_dataGridContext.DataGridControl, this );
      ThemeChangedEventManager.AddListener( m_dataGridContext.DataGridControl, this );
      ColumnsLayoutChangingEventManager.AddListener( m_dataGridContext.ColumnManager, this );
      ColumnsLayoutChangedEventManager.AddListener( m_dataGridContext.ColumnManager, this );
    }

    // Called before detaching ColumnVirtualizationManager from DataGridContext
    protected virtual void Uninitialize()
    {
      this.ResetInternalState();

      PropertyChangedEventManager.RemoveListener( m_dataGridContext, this, string.Empty );
      ItemsSourceChangeCompletedEventManager.RemoveListener( m_dataGridContext.DataGridControl, this );
      ViewChangedEventManager.RemoveListener( m_dataGridContext.DataGridControl, this );
      ThemeChangedEventManager.RemoveListener( m_dataGridContext.DataGridControl, this );
      ColumnsLayoutChangingEventManager.RemoveListener( m_dataGridContext.ColumnManager, this );
      ColumnsLayoutChangedEventManager.RemoveListener( m_dataGridContext.ColumnManager, this );

      m_dataGridContext = null;
    }

    protected virtual void IncrementVersion( UpdateMeasureRequiredEventArgs e )
    {
      unchecked
      {
        m_version++;
      }
    }

    protected virtual void OnDataGridContextPropertyChanged( PropertyChangedEventArgs e )
    {
      Debug.Assert( !string.IsNullOrEmpty( e.PropertyName ) );

      if( string.IsNullOrEmpty( e.PropertyName ) )
        return;

      switch( e.PropertyName )
      {
        case "CurrentColumn":
          this.IncrementVersion( null );
          break;
      }
    }

    protected virtual void OnColumnsLayoutChanging()
    {
    }

    protected virtual void OnColumnsLayoutChanged()
    {
      this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.Unspecified ) );
    }

    protected virtual void OnDataGridControlViewChanged()
    {
      this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.Unspecified ) );
    }

    protected virtual void OnDataGridControlThemeChanged()
    {
      this.IncrementVersion( new UpdateMeasureRequiredEventArgs( UpdateMeasureTriggeredAction.Unspecified ) );
    }

    protected virtual void OnDataGridControlItemsSourceChanged()
    {
    }

    private int m_currentVersion; //0;

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      var detach = false;

      if( managerType == typeof( PropertyChangedEventManager ) )
      {
        if( sender == m_dataGridContext )
        {
          this.OnDataGridContextPropertyChanged( ( PropertyChangedEventArgs )e );
        }
      }
      else if( managerType == typeof( ColumnsLayoutChangingEventManager ) )
      {
        this.OnColumnsLayoutChanging();
      }
      else if( managerType == typeof( ColumnsLayoutChangedEventManager ) )
      {
        this.OnColumnsLayoutChanged();
      }
      else if( managerType == typeof( ViewChangedEventManager ) )
      {
        this.OnDataGridControlViewChanged();
        detach = true;
      }
      else if( managerType == typeof( ThemeChangedEventManager ) )
      {
        this.OnDataGridControlThemeChanged();
        detach = true;
      }
      else if( managerType == typeof( ItemsSourceChangeCompletedEventManager ) )
      {
        this.OnDataGridControlItemsSourceChanged();
        detach = true;
      }
      else
      {
        return false;
      }

      if( detach && ( m_dataGridContext != null ) )
      {
        // Detach the ColumnVirtualizationManager from the DataGridContext and detach from the DataGridContext
        ColumnVirtualizationManager.SetColumnVirtualizationManager( m_dataGridContext, null );

        this.Uninitialize();
      }

      return true;
    }

    #endregion
  }
}
