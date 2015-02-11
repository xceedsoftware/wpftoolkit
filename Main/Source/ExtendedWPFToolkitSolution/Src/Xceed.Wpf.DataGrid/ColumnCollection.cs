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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;

namespace Xceed.Wpf.DataGrid
{
  public class ColumnCollection : ObservableCollection<ColumnBase>
  {

    internal ColumnCollection( DataGridControl dataGridControl, DetailConfiguration parentDetailConfiguration )
      : base()
    {
      this.DataGridControl = dataGridControl;
      this.ParentDetailConfiguration = parentDetailConfiguration;
    }

    #region MainColumn Property

    public ColumnBase MainColumn
    {
      get
      {
        return m_mainColumn;
      }
      set
      {
        if( m_mainColumn == value )
          return;

        m_mainColumn = value;
        this.OnPropertyChanged( new PropertyChangedEventArgs( "MainColumn" ) );
      }
    }

    private ColumnBase m_mainColumn;

    #endregion

    #region FieldNameToColumnDictionary Property

    internal Dictionary<string, ColumnBase> FieldNameToColumnDictionary
    {
      get
      {
        return m_fieldNameToColumns;
      }
    }

    #endregion FieldNameToColumnDictionary Property

    #region DataGridControl Property

    internal DataGridControl DataGridControl
    {
      get
      {
        return m_dataGridControl;
      }
      set
      {
        m_dataGridControl = value;

        foreach( ColumnBase column in this )
        {
          column.NotifyDataGridControlChanged();
        }
      }
    }

    private DataGridControl m_dataGridControl;

    #endregion DataGridControl Property

    #region ParentDetailConfiguration Property

    internal DetailConfiguration ParentDetailConfiguration
    {
      get;
      set;
    }

    #endregion ParentDetailConfiguration Property

    #region ProcessingVisibleColumnsUpdate Property

    internal AutoResetFlag ProcessingVisibleColumnsUpdate
    {
      get
      {
        return m_processingVisibleColumnsUpdate;
      }
    }

    private readonly AutoResetFlag m_processingVisibleColumnsUpdate = AutoResetFlagFactory.Create();

    #endregion

    public ColumnBase this[ string fieldName ]
    {
      get
      {
        ColumnBase column = null;

        m_fieldNameToColumns.TryGetValue( fieldName, out column );

        return column;
      }
    }

    #region IsDeferVisibleColumnsUpdate Internal Property

    internal bool IsDeferVisibleColumnsUpdate
    {
      get
      {
        return ( m_deferVisibleColumnsUpdateCount > 0 );
      }
    }

    #endregion

    public IDisposable DeferVisibleColumnsUpdate()
    {
      return new DeferedColumnsVisibleUpdateDisposable( this );
    }

    protected override void RemoveItem( int index )
    {
      ColumnBase column = this[ index ];

      if( column != null )
      {
        this.UnregisterColumnChangedEvent( column );

        column.DetachFromContainingCollection();

        m_fieldNameToColumns.Remove( column.FieldName );
      }

      base.RemoveItem( index );
    }

    protected override void InsertItem( int index, ColumnBase item )
    {
      if( item != null )
      {
        if( ( string.IsNullOrEmpty( item.FieldName ) == true ) && ( !DesignerProperties.GetIsInDesignMode( item ) ) )
          throw new ArgumentException( "A column must have a fieldname.", "item" );

        if( m_fieldNameToColumns.ContainsKey( item.FieldName ) )
          throw new DataGridException( "A column with same field name already exists in collection." );

        m_fieldNameToColumns.Add( item.FieldName, item );

        item.AttachToContainingCollection( this );

        this.RegisterColumnChangedEvent( item );
      }

      base.InsertItem( index, item );
    }

    protected override void ClearItems()
    {
      foreach( ColumnBase column in this )
      {
        this.UnregisterColumnChangedEvent( column );

        column.DetachFromContainingCollection();
      }

      m_fieldNameToColumns.Clear();

      base.ClearItems();
    }

    protected override void SetItem( int index, ColumnBase item )
    {
      if( ( item != null ) && ( string.IsNullOrEmpty( item.FieldName ) == true ) )
        throw new ArgumentException( "A column must have a fieldname.", "item" );

      if( m_fieldNameToColumns.ContainsKey( item.FieldName ) )
        throw new DataGridException( "A column with same field name already exists in collection." );

      ColumnBase column = this[ index ];

      if( ( column != null ) && ( column != item ) )
      {
        this.UnregisterColumnChangedEvent( column );

        column.DetachFromContainingCollection();

        m_fieldNameToColumns.Remove( column.FieldName );
      }

      if( ( item != null ) && ( column != item ) )
      {
        m_fieldNameToColumns.Add( item.FieldName, item );

        item.AttachToContainingCollection( this );

        this.RegisterColumnChangedEvent( item );
      }

      base.SetItem( index, item );
    }

    protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      if( ( m_columnAdditionMessagesDeferedCount > 0 ) && ( e.Action == NotifyCollectionChangedAction.Add ) && ( e.NewStartingIndex == ( this.Count - e.NewItems.Count ) ) )
      {
        if( m_deferedColumnAdditionMessageStartIndex == -1 )
        {
          m_deferedColumnAdditionMessageStartIndex = e.NewStartingIndex;
        }

        foreach( object item in e.NewItems )
        {
          m_deferedColumns.Add( ( ColumnBase )item );
        }
      }
      else
      {
        base.OnCollectionChanged( e );
      }
    }

    internal IDisposable DeferColumnAdditionMessages()
    {
      return new DeferedColumnAdditionMessageDisposable( this );
    }

    internal void NotifyVisibleColumnsUpdating()
    {
      if( this.VisibleColumnsUpdating != null )
      {
        this.VisibleColumnsUpdating( this, EventArgs.Empty );
      }
    }

    internal event EventHandler VisibleColumnsUpdating;

    internal void DispatchNotifyVisibleColumnsUpdated()
    {
      //In all cases, make sure the ColumnVitualizationManager is flaged as outdate, but prevent to make updates to FixedCellPanels right away.
      this.NotifyVisibleColumnsUpdated( true );

      //In all cases, make sure we dipatch an operation to update the FixedCellPanels in case the update is not tiggered by other means (e.g. no scrolling, no DataRow in the grid).
      if( m_visibleColumnsUpdatedDispatcherOperation == null && m_dataGridControl != null )
      {
        ////# Will be null at first in the case of a detail, so process directly

        //This permits less updates = better performance.
        m_visibleColumnsUpdatedDispatcherOperation =
          m_dataGridControl.Dispatcher.BeginInvoke( DispatcherPriority.Render, new Action<bool>( this.NotifyVisibleColumnsUpdated ), false );
      }
    }

    internal event EventHandler VisibleColumnsUpdated;

    internal void OnRealizedContainersRequested( object sender, RealizedContainersRequestedEventArgs e )
    {
      if( this.RealizedContainersRequested != null )
      {
        this.RealizedContainersRequested( this, e );
      }
    }

    internal event RealizedContainersRequestedEventHandler RealizedContainersRequested;

    internal void OnDistinctValuesRequested( object sender, DistinctValuesRequestedEventArgs e )
    {
      if( this.DistinctValuesRequested != null )
      {
        this.DistinctValuesRequested( this, e );
      }
    }

    internal event DistinctValuesRequestedEventHandler DistinctValuesRequested;

    internal void OnActualWidthChanged( object sender, ColumnActualWidthChangedEventArgs e )
    {
      if( this.ActualWidthChanged != null )
      {
        this.ActualWidthChanged( this, e );
      }
    }

    internal event ColumnActualWidthChangedHandler ActualWidthChanged;

    internal event EventHandler ColumnVisibilityChanging;
    internal event EventHandler ColumnVisibilityChanged;

    private void RegisterColumnChangedEvent( ColumnBase column )
    {
      column.PropertyChanged += new PropertyChangedEventHandler( this.OnColumnPropertyChanged );
      column.TitleChanged += new EventHandler( this.OnColumnValueChanged );
      column.VisiblePositionChanging += new EventHandler( this.OnColumnVisibilityChanging );
      column.VisiblePositionChanged += new EventHandler( this.OnColumnValueChanged );
      column.VisiblePositionChanged += new EventHandler( this.OnColumnVisibilityChanged );
      column.VisibleChanged += new EventHandler( this.OnColumnVisibilityChanged );
    }

    private void UnregisterColumnChangedEvent( ColumnBase column )
    {
      column.PropertyChanged -= new PropertyChangedEventHandler( this.OnColumnPropertyChanged );
      column.TitleChanged -= new EventHandler( this.OnColumnValueChanged );
      column.VisiblePositionChanging -= new EventHandler( this.OnColumnVisibilityChanging );
      column.VisiblePositionChanged -= new EventHandler( this.OnColumnValueChanged );
      column.VisiblePositionChanged -= new EventHandler( this.OnColumnVisibilityChanged );
      column.VisibleChanged -= new EventHandler( this.OnColumnVisibilityChanged );
    }

    private void OnColumnPropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      if( string.IsNullOrEmpty( e.PropertyName ) || e.PropertyName == "IsMainColumn" )
      {
        // The ColumnBase.IsMainColumn property impacts the columns visibility when details are flatten.
        var dataGridControl = this.DataGridControl;
        if( ( dataGridControl != null ) && ( dataGridControl.AreDetailsFlatten ) )
        {
          this.OnColumnVisibilityChanged( sender, EventArgs.Empty );
        }
      }
    }

    private void OnColumnVisibilityChanging( object sender, EventArgs e )
    {
      if( this.ColumnVisibilityChanging != null )
      {
        this.ColumnVisibilityChanging( this, new WrappedEventEventArgs( sender, e ) );
      }
    }

    private void OnColumnVisibilityChanged( object sender, EventArgs e )
    {
      if( this.ColumnVisibilityChanged != null )
      {
        this.ColumnVisibilityChanged( this, new WrappedEventEventArgs( sender, e ) );
      }
    }

    private void OnColumnValueChanged( object sender, EventArgs e )
    {
      ColumnBase column = ( ColumnBase )sender;
    }

    private void NotifyVisibleColumnsUpdated( bool onlyIncrementFlag )
    {
      if( this.VisibleColumnsUpdated != null )
      {
        this.VisibleColumnsUpdated( this, new VisibleColumnsUpdatedEventArgs( onlyIncrementFlag ) );
      }

      //If only updating the flag, don't reset the DispatcherOperation, since if it is still alive, another call will be made which will reset it.
      if( onlyIncrementFlag )
        return;

      m_visibleColumnsUpdatedDispatcherOperation = null;
    }

    private int m_columnAdditionMessagesDeferedCount = 0;
    private int m_deferVisibleColumnsUpdateCount = 0;
    private int m_deferedColumnAdditionMessageStartIndex = -1;
    private List<ColumnBase> m_deferedColumns = new List<ColumnBase>();
    private Dictionary<string, ColumnBase> m_fieldNameToColumns = new Dictionary<string, ColumnBase>(); // To optimize indexing speed for FieldName
    private DispatcherOperation m_visibleColumnsUpdatedDispatcherOperation;

    private sealed class DeferedColumnAdditionMessageDisposable : IDisposable
    {
      public DeferedColumnAdditionMessageDisposable( ColumnCollection columns )
      {
        if( columns == null )
          throw new ArgumentNullException( "columns" );

        m_columns = columns;

        m_columns.m_columnAdditionMessagesDeferedCount++;
      }

      #region IDisposable Members

      public void Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      #endregion

      private void Dispose( bool disposing )
      {
        m_columns.m_columnAdditionMessagesDeferedCount--;

        if( m_columns.m_columnAdditionMessagesDeferedCount == 0 )
        {
          if( m_columns.m_deferedColumns.Count > 0 )
          {
            m_columns.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, m_columns.m_deferedColumns, m_columns.m_deferedColumnAdditionMessageStartIndex ) );
          }

          m_columns.m_deferedColumnAdditionMessageStartIndex = -1;
          m_columns.m_deferedColumns.Clear();
        }
      }

      ~DeferedColumnAdditionMessageDisposable()
      {
        this.Dispose( false );
      }

      private ColumnCollection m_columns; // = null
    }

    private sealed class DeferedColumnsVisibleUpdateDisposable : IDisposable
    {
      public DeferedColumnsVisibleUpdateDisposable( ColumnCollection columns )
      {
        if( columns == null )
          throw new ArgumentNullException( "columns" );

        m_columns = columns;

        Interlocked.Increment( ref m_columns.m_deferVisibleColumnsUpdateCount );
      }

      #region IDisposable Members

      public void Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      #endregion

      private void Dispose( bool disposing )
      {
        if( Interlocked.Decrement( ref m_columns.m_deferVisibleColumnsUpdateCount ) != 0 )
          return;

        m_columns.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
      }

      ~DeferedColumnsVisibleUpdateDisposable()
      {
        this.Dispose( false );
      }

      private ColumnCollection m_columns; // = null
    }

    internal class WrappedEventEventArgs : EventArgs
    {
      public WrappedEventEventArgs( object sender, EventArgs eventArgs )
      {
        if( sender == null )
          throw new ArgumentNullException( "sender" );

        if( eventArgs == null )
          throw new ArgumentNullException( "eventArgs" );

        this.WrappedSender = sender;
        this.WrappedEventArgs = eventArgs;
      }

      public object WrappedSender
      {
        get;
        private set;
      }

      public object WrappedEventArgs
      {
        get;
        private set;
      }
    }
  }
}
