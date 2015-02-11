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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class ColumnSynchronizationManager : IWeakEventListener
  {
    #region Static Fields

    private static readonly string DetailConfigurationDataGridControlPropertyName = "DataGridControl";
    private static readonly string DetailConfigurationIsAttachedToDetailDescriptionPropertyName = "IsAttachedToDetailDescription";
    private static readonly string ColumnCollectionMainColumnPropertyName = "MainColumn";

    #endregion

    #region Constructor

    internal ColumnSynchronizationManager( DetailConfiguration configuration )
    {
      if( configuration == null )
        throw new ArgumentNullException( "configuration" );

      m_detailConfiguration = configuration;
      m_itemPropertyMap = configuration.ItemPropertyMap;

      PropertyChangedEventManager.AddListener( configuration, this, ColumnSynchronizationManager.DetailConfigurationDataGridControlPropertyName );
      PropertyChangedEventManager.AddListener( configuration, this, ColumnSynchronizationManager.DetailConfigurationIsAttachedToDetailDescriptionPropertyName );
      CollectionChangedEventManager.AddListener( ( INotifyCollectionChanged )m_itemPropertyMap, this );

      using( this.DeferSynchronization() )
      {
        this.DataGridControl = configuration.DataGridControl;
        this.DetailColumns = configuration.Columns;

        Debug.Assert( this.DetailColumns != null );
      }
    }

    #endregion

    #region DetailConfiguration Private Property

    private DetailConfiguration DetailConfiguration
    {
      get
      {
        return m_detailConfiguration;
      }
    }

    private readonly DetailConfiguration m_detailConfiguration;

    #endregion

    #region DataGridControl Private Property

    private DataGridControl DataGridControl
    {
      get
      {
        return m_dataGridControl;
      }
      set
      {
        if( value == m_dataGridControl )
          return;

        using( this.DeferSynchronization() )
        {
          if( m_dataGridControl != null )
          {
            ViewChangedEventManager.RemoveListener( m_dataGridControl, this );
          }

          m_dataGridControl = value;

          if( m_dataGridControl != null )
          {
            ViewChangedEventManager.AddListener( m_dataGridControl, this );

            this.MasterColumns = m_dataGridControl.Columns;
          }
          else
          {
            this.MasterColumns = null;
          }

          this.InvalidateSynchronization();
        }
      }
    }

    private DataGridControl m_dataGridControl;

    #endregion

    #region ItemPropertyMap Private Property

    private FieldNameMap ItemPropertyMap
    {
      get
      {
        return m_itemPropertyMap;
      }
    }

    private readonly FieldNameMap m_itemPropertyMap;

    #endregion

    #region MasterColumns Private Property

    private ColumnCollection MasterColumns
    {
      get
      {
        return m_masterColumns;
      }
      set
      {
        if( value == m_masterColumns )
          return;

        if( m_masterColumns != null )
        {
          CollectionChangedEventManager.RemoveListener( m_masterColumns, this );
          PropertyChangedEventManager.RemoveListener( m_masterColumns, this, ColumnSynchronizationManager.ColumnCollectionMainColumnPropertyName );
          VisibleColumnsUpdatedEventManager.RemoveListener( m_masterColumns, this );
        }

        m_masterColumns = value;

        if( m_masterColumns != null )
        {
          CollectionChangedEventManager.AddListener( m_masterColumns, this );
          PropertyChangedEventManager.AddListener( m_masterColumns, this, ColumnSynchronizationManager.ColumnCollectionMainColumnPropertyName );
          VisibleColumnsUpdatedEventManager.AddListener( m_masterColumns, this );
        }
      }
    }

    private ColumnCollection m_masterColumns;

    #endregion

    #region DetailColumns Private Property

    private ColumnCollection DetailColumns
    {
      get
      {
        return m_detailColumns;
      }
      set
      {
        if( value == m_detailColumns )
          return;

        if( m_detailColumns != null )
        {
          CollectionChangedEventManager.RemoveListener( m_detailColumns, this );
          VisibleColumnsUpdatedEventManager.RemoveListener( m_detailColumns, this );
        }

        m_detailColumns = value;

        if( m_detailColumns != null )
        {
          CollectionChangedEventManager.AddListener( m_detailColumns, this );
          VisibleColumnsUpdatedEventManager.AddListener( m_detailColumns, this );
        }
      }
    }

    private ColumnCollection m_detailColumns;

    #endregion

    #region PairedColumns Private Property

    private Dictionary<SynchronizationKey, SynchronizationEntry> PairedColumns
    {
      get
      {
        return m_pairedColumns;
      }
    }

    private readonly Dictionary<SynchronizationKey, SynchronizationEntry> m_pairedColumns = new Dictionary<SynchronizationKey, SynchronizationEntry>( 0 );

    #endregion

    #region IsSynchronizationReady Private Property

    private bool IsSynchronizationReady
    {
      get
      {
        return ( this.MasterColumns != null )
            && ( this.DetailConfiguration.IsAttachedToDetailDescription );
      }
    }

    #endregion

    #region IsUpdatingColumns Private Property

    private bool IsUpdatingColumns
    {
      get
      {
        return m_isUpdatingColumns;
      }
    }

    private bool m_isUpdatingColumns;

    #endregion

    #region AreDetailsFlatten Private Property

    private bool AreDetailsFlatten
    {
      get
      {
        var dataGridControl = this.DataGridControl;

        return ( dataGridControl != null )
            && ( dataGridControl.AreDetailsFlatten );
      }
    }

    #endregion

    #region DeferSynchronization Methods

    private IDisposable DeferSynchronization()
    {
      return new DeferSynchronizationHelper( this );
    }

    private bool IsSynchronizationDefered
    {
      get
      {
        return ( Interlocked.CompareExchange( ref m_deferSynchronizationCount, 0, 0 ) != 0 );
      }
    }

    private bool IsSynchronizationValid
    {
      get
      {
        return m_isSynchronizationValid;
      }
    }

    private void InvalidateSynchronization()
    {
      m_isSynchronizationValid = false;
    }

    private int m_deferSynchronizationCount; //0
    private bool m_isSynchronizationValid = true;

    #endregion

    private void Refresh()
    {
      if( this.IsUpdatingColumns )
        return;

      this.InvalidateSynchronization();
      this.UpdateColumns();
    }

    private void UpdateColumns()
    {
      if( this.IsSynchronizationDefered || this.IsSynchronizationValid || this.IsUpdatingColumns )
        return;

      try
      {
        m_isUpdatingColumns = true;
        m_isSynchronizationValid = true;

        if( this.IsSynchronizationReady && this.AreDetailsFlatten )
        {
          var keys = new HashSet<SynchronizationKey>( this.GetNewKeys() );

          foreach( var key in this.PairedColumns.Keys.Except( keys ).ToArray() )
          {
            this.Desynchronize( key );
          }

          foreach( var key in keys.OrderBy( item => item.OrderKey ) )
          {
            this.Synchronize( key );
          }

          this.SetMainColumn();
        }
        else
        {
          this.Desynchronize();
        }
      }
      finally
      {
        m_isUpdatingColumns = false;
      }
    }

    private void Synchronize( SynchronizationKey key )
    {
      if( key == null )
        return;

      var collection = this.PairedColumns;

      SynchronizationEntry pair;
      if( !collection.TryGetValue( key, out pair ) )
      {
        if( key.MasterColumn != null )
        {
          pair = new BoundColumn( key );
        }
        else
        {
          pair = new UnboundColumn( key );
        }

        collection.Add( key, pair );
      }

      pair.Synchronize();
    }

    private void Desynchronize( SynchronizationKey key )
    {
      if( key == null )
        return;

      var collection = this.PairedColumns;

      SynchronizationEntry pair;
      if( !collection.TryGetValue( key, out pair ) )
        return;

      collection.Remove( key );
      pair.Desynchronize();
    }

    private void Desynchronize()
    {
      var collection = this.PairedColumns;
      while( collection.Count > 0 )
      {
        this.Desynchronize( collection.First().Key );
      }
    }

    private void SetMainColumn()
    {
      if( !this.IsSynchronizationReady || !this.AreDetailsFlatten )
        return;

      var detailColumns = this.DetailColumns;
      ColumnBase detailColumn = null;

      var mainColumn = this.MasterColumns.MainColumn;
      if( mainColumn != null )
      {
        string targetName;
        if( this.ItemPropertyMap.TryGetItemPropertyName( mainColumn, out targetName ) )
        {
          detailColumn = detailColumns[ targetName ];
        }
      }

      detailColumns.MainColumn = detailColumn;
    }

    private IEnumerable<SynchronizationKey> GetNewKeys()
    {
      var detailConfig = this.DetailConfiguration;
      var map = this.ItemPropertyMap;

      foreach( var detailColumn in this.DetailColumns )
      {
        ColumnBase masterColumn;
        string targetName;

        if( map.TryGetColumnFieldName( detailColumn, out targetName ) )
        {
          masterColumn = this.MasterColumns[ targetName ];
        }
        else
        {
          masterColumn = null;
        }

        yield return new SynchronizationKey( masterColumn, detailColumn, detailConfig );
      }
    }

    private void OnViewChanged()
    {
      this.Refresh();
    }

    private void OnUserSettingsLoaded()
    {
      this.Refresh();
    }

    private void OnMasterColumnsPropertyChanged( PropertyChangedEventArgs e )
    {
      var propertyName = e.PropertyName;
      bool mayHaveChanged = string.IsNullOrEmpty( propertyName );

      if( mayHaveChanged || ( propertyName == ColumnSynchronizationManager.ColumnCollectionMainColumnPropertyName ) )
      {
        this.SetMainColumn();
      }
    }

    private void OnDetailConfigurationPropertyChanged( PropertyChangedEventArgs e )
    {
      var propertyName = e.PropertyName;
      bool mayHaveChanged = string.IsNullOrEmpty( propertyName );

      if( mayHaveChanged || ( propertyName == ColumnSynchronizationManager.DetailConfigurationDataGridControlPropertyName ) )
      {
        this.DataGridControl = this.DetailConfiguration.DataGridControl;
      }

      if( mayHaveChanged || ( propertyName == ColumnSynchronizationManager.DetailConfigurationIsAttachedToDetailDescriptionPropertyName ) )
      {
        this.Refresh();
      }
    }

    private void OnItemPropertyMapCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      if( e.Action == NotifyCollectionChangedAction.Move )
        return;

      this.Refresh();
    }

    private void OnMasterColumnsCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      if( e.Action == NotifyCollectionChangedAction.Move )
        return;

      this.Refresh();
    }

    private void OnDetailColumnsCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      if( e.Action == NotifyCollectionChangedAction.Move )
        return;

      this.Refresh();
    }

    private void OnMasterVisibleColumnsUpdated( EventArgs e )
    {
      this.Refresh();
    }

    private void OnDetailVisibleColumnsUpdated( EventArgs e )
    {
      this.Refresh();
    }

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        if( sender == this.MasterColumns )
        {
          Debug.Assert( sender != null );

          this.OnMasterColumnsCollectionChanged( ( NotifyCollectionChangedEventArgs )e );
        }
        else if( sender == this.DetailColumns )
        {
          Debug.Assert( sender != null );

          this.OnDetailColumnsCollectionChanged( ( NotifyCollectionChangedEventArgs )e );
        }
        else if( sender == this.ItemPropertyMap )
        {
          Debug.Assert( sender != null );

          this.OnItemPropertyMapCollectionChanged( ( NotifyCollectionChangedEventArgs )e );
        }

        return true;
      }
      else if( managerType == typeof( PropertyChangedEventManager ) )
      {
        if( sender == this.MasterColumns )
        {
          Debug.Assert( sender != null );

          this.OnMasterColumnsPropertyChanged( ( PropertyChangedEventArgs )e );
        }
        else if( sender == this.DetailConfiguration )
        {
          Debug.Assert( sender != null );

          this.OnDetailConfigurationPropertyChanged( ( PropertyChangedEventArgs )e );
        }

        return true;
      }
      else if( managerType == typeof( VisibleColumnsUpdatedEventManager ) )
      {
        if( sender == this.MasterColumns )
        {
          Debug.Assert( sender != null );

          this.OnMasterVisibleColumnsUpdated( e );
        }
        else if( sender == this.DetailColumns )
        {
          Debug.Assert( sender != null );

          this.OnDetailVisibleColumnsUpdated( e );
        }

        return true;
      }
      else if( managerType == typeof( ViewChangedEventManager ) )
      {
        if( sender == this.DataGridControl )
        {
          Debug.Assert( sender != null );

          this.OnViewChanged();
        }

        return true;
      }

      return false;
    }

    #endregion

    #region DeferSynchronizationHelper Private Class

    private sealed class DeferSynchronizationHelper : IDisposable
    {
      internal DeferSynchronizationHelper( ColumnSynchronizationManager owner )
      {
        if( owner == null )
          throw new ArgumentNullException( "owner" );

        m_owner = owner;

        Interlocked.Increment( ref owner.m_deferSynchronizationCount );
      }

      void IDisposable.Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        Debug.Assert( disposing );

        var owner = m_owner;
        if( Interlocked.CompareExchange<ColumnSynchronizationManager>( ref m_owner, null, owner ) == null )
          return;

        if( Interlocked.Decrement( ref owner.m_deferSynchronizationCount ) != 0 )
          return;

        owner.UpdateColumns();
      }

      ~DeferSynchronizationHelper()
      {
        this.Dispose( false );
      }

      #region Private Fields

      private ColumnSynchronizationManager m_owner;

      #endregion
    }

    #endregion

    #region SynchronizationKey Private Class

    private sealed class SynchronizationKey
    {
      internal SynchronizationKey(
        ColumnBase masterColumn,
        ColumnBase detailColumn,
        DetailConfiguration detailConfig )
      {
        if( detailColumn == null )
          throw new ArgumentNullException( "detailColumn" );

        if( detailConfig == null )
          throw new ArgumentNullException( "detailConfig" );

        m_masterColumn = masterColumn;
        m_detailColumn = detailColumn;
        m_detailConfiguration = detailConfig;
      }

      #region MasterColumn Property

      internal ColumnBase MasterColumn
      {
        get
        {
          return m_masterColumn;
        }
      }

      private readonly ColumnBase m_masterColumn;

      #endregion

      #region DetailColumn Property

      internal ColumnBase DetailColumn
      {
        get
        {
          return m_detailColumn;
        }
      }

      private readonly ColumnBase m_detailColumn;

      #endregion

      #region DetailConfiguration Property

      internal DetailConfiguration DetailConfiguration
      {
        get
        {
          return m_detailConfiguration;
        }
      }

      private readonly DetailConfiguration m_detailConfiguration;

      #endregion

      #region OrderKey Property

      internal int OrderKey
      {
        get
        {
          var column = this.MasterColumn;
          if( column != null )
            return column.VisiblePosition;

          return int.MaxValue;
        }
      }

      #endregion

      public override int GetHashCode()
      {
        if( m_masterColumn == null )
          return m_detailColumn.GetHashCode();

        return ( m_masterColumn.GetHashCode() )
             ^ ( m_detailColumn.GetHashCode() );
      }

      public override bool Equals( object obj )
      {
        var key = obj as SynchronizationKey;
        if( key == null )
          return false;

        if( key == this )
          return true;

        return ( key.m_masterColumn == m_masterColumn )
            && ( key.m_detailColumn == m_detailColumn )
            && ( key.m_detailConfiguration == m_detailConfiguration );
      }
    }

    #endregion

    #region SynchronizationEntry Private Class

    private abstract class SynchronizationEntry
    {
      protected SynchronizationEntry( SynchronizationKey key )
      {
        if( key == null )
          throw new ArgumentNullException( "key" );

        m_key = key;
      }

      #region MasterColumn Protected Property

      protected ColumnBase MasterColumn
      {
        get
        {
          return m_key.MasterColumn;
        }
      }

      #endregion

      #region DetailColumn Protected Property

      protected ColumnBase DetailColumn
      {
        get
        {
          return m_key.DetailColumn;
        }
      }

      #endregion

      #region DetailConfiguration Protected Property

      protected DetailConfiguration DetailConfiguration
      {
        get
        {
          return m_key.DetailConfiguration;
        }
      }

      #endregion

      #region Key Private Property

      private SynchronizationKey Key
      {
        get
        {
          return m_key;
        }
      }

      private readonly SynchronizationKey m_key;

      #endregion

      public abstract void Synchronize();
      public abstract void Desynchronize();
    }

    #endregion

    #region BoundColumn Private Class

    private sealed class BoundColumn : SynchronizationEntry
    {
      internal BoundColumn( SynchronizationKey key )
        : base( key )
      {
        if( key.MasterColumn == null )
          throw new ArgumentException( "The master column is not set.", "key" );

        m_bindings = BoundColumn.CreateBindings( key.MasterColumn ).ToArray();
      }

      #region IsListeningEvents Private Property

      private bool IsListeningEvents
      {
        get
        {
          return m_isListeningEvents;
        }
        set
        {
          m_isListeningEvents = value;
        }
      }

      private bool m_isListeningEvents; //false

      #endregion

      public override void Synchronize()
      {
        this.SubscribeEvents();

        var targetColumn = this.DetailColumn;

        foreach( var entry in m_bindings )
        {
          var dp = entry.Key;
          var binding = entry.Value;

          // The appropriate binding is still in place.
          var currentBinding = BindingOperations.GetBindingBase( targetColumn, dp );
          if( currentBinding == binding )
            continue;

          if( currentBinding != null )
          {
            m_localValues[ dp ] = currentBinding;
          }
          else
          {
            var localValue = targetColumn.ReadLocalValue( dp );
            if( localValue != DependencyProperty.UnsetValue )
            {
              m_localValues[ dp ] = localValue;
            }
            else
            {
              m_localValues.Remove( dp );
            }
          }

          BindingOperations.SetBinding( targetColumn, dp, binding );
        }

        var visiblePosition = this.GetTargetVisiblePosition();
        if( visiblePosition.HasValue )
        {
          targetColumn.VisiblePosition = visiblePosition.Value;
        }
        else
        {
          targetColumn.ClearValue( ColumnBase.VisiblePositionProperty );
        }
      }

      public override void Desynchronize()
      {
        this.UnsubscribeEvents();

        var targetColumn = this.DetailColumn;

        foreach( var entry in m_bindings )
        {
          var dp = entry.Key;
          var binding = entry.Value;

          // The appropriate binding is still in place.
          var currentBinding = BindingOperations.GetBindingBase( targetColumn, dp );
          if( currentBinding == binding )
          {
            object oldValue;

            if( m_localValues.TryGetValue( dp, out oldValue ) )
            {
              var oldBinding = oldValue as BindingBase;
              if( oldBinding != null )
              {
                BindingOperations.SetBinding( targetColumn, dp, oldBinding );
              }
              else
              {
                targetColumn.SetValue( dp, oldValue );
              }
            }
            else
            {
              BindingOperations.ClearBinding( targetColumn, dp );
            }
          }

          m_localValues.Remove( dp );
        }

        targetColumn.ClearValue( ColumnBase.VisiblePositionProperty );
      }

      private static IEnumerable<KeyValuePair<DependencyProperty, BindingBase>> CreateBindings( ColumnBase source )
      {
        yield return BoundColumn.CreateBinding( ColumnBase.WidthProperty, source );
        yield return BoundColumn.CreateBinding( ColumnBase.MinWidthProperty, source );
        yield return BoundColumn.CreateBinding( ColumnBase.MaxWidthProperty, source );
        yield return BoundColumn.CreateBinding( ColumnBase.DesiredWidthProperty, source );
        yield return BoundColumn.CreateBinding( ColumnBase.VisibleProperty, source );
        yield return BoundColumn.CreateBinding( ColumnReorderingDragSourceManager.AnimatedColumnReorderingTranslationProperty, source );
        yield return BoundColumn.CreateBinding( TableflowView.IsBeingDraggedAnimatedProperty, source );
        yield return BoundColumn.CreateBinding( TableflowView.ColumnReorderingDragSourceManagerProperty, source );
      }

      private static KeyValuePair<DependencyProperty, BindingBase> CreateBinding( DependencyProperty dp, ColumnBase source )
      {
        if( dp == null )
          throw new ArgumentNullException( "property" );

        if( source == null )
          throw new ArgumentNullException( "source" );

        var binding = new Binding();
        binding.Path = new PropertyPath( dp );
        binding.Source = source;

        return new KeyValuePair<DependencyProperty, BindingBase>( dp, binding );
      }

      private Nullable<int> GetTargetVisiblePosition()
      {
        var detailConfig = this.DetailConfiguration;

        var dataGridControl = detailConfig.DataGridControl;
        if( dataGridControl == null )
          return null;

        var masterColumn = this.MasterColumn;
        int visiblePosition = masterColumn.VisiblePosition;
        if( visiblePosition < 0 )
          return null;

        var masterColumnNode = dataGridControl.ColumnsByVisiblePosition.Find( masterColumn );
        if( masterColumnNode == null )
          return null;

        var map = this.DetailConfiguration.ItemPropertyMap;

        // Remove master columns that are not linked to a detail column from the VisiblePosition count.
        masterColumnNode = masterColumnNode.Previous;
        while( masterColumnNode != null )
        {
          string detailColumnName;
          if( !map.TryGetItemPropertyName( masterColumnNode.Value, out detailColumnName ) )
          {
            visiblePosition--;
          }

          masterColumnNode = masterColumnNode.Previous;
        }

        Debug.Assert( visiblePosition >= 0 );

        return visiblePosition;
      }

      private void SubscribeEvents()
      {
        if( this.IsListeningEvents )
          return;

        this.MasterColumn.FittedWidthRequested += new FittedWidthRequestedEventHandler( this.OnMasterColumnFittedWidthRequested );
        this.IsListeningEvents = true;
      }

      private void UnsubscribeEvents()
      {
        if( !this.IsListeningEvents )
          return;

        this.MasterColumn.FittedWidthRequested -= new FittedWidthRequestedEventHandler( this.OnMasterColumnFittedWidthRequested );
        this.IsListeningEvents = false;
      }

      private void OnMasterColumnFittedWidthRequested( object sender, FittedWidthRequestedEventArgs e )
      {
        Debug.Assert( ( ColumnBase )sender == this.MasterColumn );

        var fittedWidth = this.DetailColumn.GetFittedWidth();
        if( fittedWidth < 0d )
          return;

        e.SetValue( fittedWidth );
      }

      #region Private Fields

      private readonly KeyValuePair<DependencyProperty, BindingBase>[] m_bindings;
      private readonly Dictionary<DependencyProperty, object> m_localValues = new Dictionary<DependencyProperty, object>( 0 );

      #endregion
    }

    #endregion

    #region UnboundColumn Private Class

    private sealed class UnboundColumn : SynchronizationEntry
    {
      internal UnboundColumn( SynchronizationKey key )
        : base( key )
      {
        if( key.MasterColumn != null )
          throw new ArgumentException( "The master column should not be set.", "key" );
      }

      public override void Synchronize()
      {
        var targetColumn = this.DetailColumn;
        var dp = ColumnBase.VisibleProperty;

        var currentBinding = BindingOperations.GetBindingBase( targetColumn, dp );
        if( currentBinding != null )
        {
          m_localValue = currentBinding;
          BindingOperations.ClearBinding( targetColumn, dp );
        }
        else
        {
          var localValue = targetColumn.ReadLocalValue( dp );

          // The appropriate value is still in place.
          if( object.Equals( localValue, false ) )
            return;

          m_localValue = localValue;
        }

        // Hide the column.
        targetColumn.SetValue( dp, false );
      }

      public override void Desynchronize()
      {
        var targetColumn = this.DetailColumn;
        var dp = ColumnBase.VisibleProperty;
        var oldLocalValue = m_localValue;

        m_localValue = DependencyProperty.UnsetValue;

        if( BindingOperations.GetBindingBase( targetColumn, dp ) != null )
          return;

        if( !object.Equals( targetColumn.ReadLocalValue( dp ), false ) )
          return;

        var oldBinding = oldLocalValue as BindingBase;
        if( oldBinding != null )
        {
          BindingOperations.SetBinding( targetColumn, dp, oldBinding );
        }
        else if( oldLocalValue != DependencyProperty.UnsetValue )
        {
          targetColumn.SetValue( dp, oldLocalValue );
        }
        else
        {
          targetColumn.ClearValue( dp );
        }
      }

      #region Private Fields

      private object m_localValue = DependencyProperty.UnsetValue;

      #endregion
    }

    #endregion
  }
}
