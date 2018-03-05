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
using System.Windows;
using System.Windows.Data;
using Xceed.Utils.Wpf;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class ColumnSynchronizationManager : IWeakEventListener
  {
    #region Constructor

    internal ColumnSynchronizationManager( DetailConfiguration configuration )
    {
      if( configuration == null )
        throw new ArgumentNullException( "configuration" );

      m_detailConfiguration = configuration;
      m_itemPropertyMap = configuration.ItemPropertyMap;

      PropertyChangedEventManager.AddListener( configuration, this, string.Empty );
      MappingChangedEventManager.AddListener( m_itemPropertyMap, this );

      using( this.DeferSynchronization() )
      {
        this.DataGridControl = configuration.DataGridControl;
        this.DetailColumnManager = configuration.ColumnManager;

        Debug.Assert( m_detailColumnManager != null );
      }
    }

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

            this.MasterColumnManager = m_dataGridControl.DataGridContext.ColumnManager;
          }
          else
          {
            this.MasterColumnManager = null;
          }

          m_isSynchronizationValid = false;
        }
      }
    }

    private DataGridControl m_dataGridControl;

    #endregion

    #region MasterColumnManager Private Property

    private ColumnHierarchyManager MasterColumnManager
    {
      get
      {
        return m_masterColumnManager;
      }
      set
      {
        if( value == m_masterColumnManager )
          return;

        if( m_masterColumnManager != null )
        {
          ColumnsLayoutChangedEventManager.RemoveListener( m_masterColumnManager, this );
          CollectionChangedEventManager.RemoveListener( m_masterColumnManager.Columns, this );
          PropertyChangedEventManager.RemoveListener( m_masterColumnManager.Columns, this, ColumnCollection.MainColumnPropertyName );
        }

        m_masterColumnManager = value;

        if( m_masterColumnManager != null )
        {
          ColumnsLayoutChangedEventManager.AddListener( m_masterColumnManager, this );
          CollectionChangedEventManager.AddListener( m_masterColumnManager.Columns, this );
          PropertyChangedEventManager.AddListener( m_masterColumnManager.Columns, this, ColumnCollection.MainColumnPropertyName );
        }
      }
    }

    private ColumnHierarchyManager m_masterColumnManager;

    #endregion

    #region DetailColumnManager Private Property

    private ColumnHierarchyManager DetailColumnManager
    {
      get
      {
        return m_detailColumnManager;
      }
      set
      {
        if( value == m_detailColumnManager )
          return;

        if( m_detailColumnManager != null )
        {
          ColumnsLayoutChangedEventManager.RemoveListener( m_detailColumnManager, this );
          CollectionChangedEventManager.RemoveListener( m_detailColumnManager.Columns, this );
        }

        m_detailColumnManager = value;

        if( m_detailColumnManager != null )
        {
          ColumnsLayoutChangedEventManager.AddListener( m_detailColumnManager, this );
          CollectionChangedEventManager.AddListener( m_detailColumnManager.Columns, this );
        }
      }
    }

    private ColumnHierarchyManager m_detailColumnManager;

    #endregion

    #region IsSynchronizationReady Private Property

    private bool IsSynchronizationReady
    {
      get
      {
        return ( m_masterColumnManager != null )
            && ( m_detailConfiguration.DetailDescription != null );
      }
    }

    #endregion

    #region AreDetailsFlatten Private Property

    private bool AreDetailsFlatten
    {
      get
      {
        return ( m_dataGridControl != null )
            && ( m_dataGridControl.AreDetailsFlatten );
      }
    }

    #endregion

    #region DeferSynchronization Methods

    private IDisposable DeferSynchronization()
    {
      return new DeferredDisposable( new DeferState( this ) );
    }

    private bool IsSynchronizationDeferred
    {
      get
      {
        return ( m_deferSynchronizationCount != 0 );
      }
    }

    private int m_deferSynchronizationCount; //0
    private bool m_isSynchronizationValid = true;

    #endregion

    internal void Refresh()
    {
      if( m_isUpdatingColumns )
        return;

      m_isSynchronizationValid = false;

      this.UpdateColumns();
    }

    private void UpdateColumns()
    {
      if( this.IsSynchronizationDeferred || m_isSynchronizationValid || m_isUpdatingColumns )
        return;

      try
      {
        m_isUpdatingColumns = true;
        m_isSynchronizationValid = true;

        if( this.IsSynchronizationReady && this.AreDetailsFlatten )
        {
          Debug.Assert( m_detailColumnManager != null );

          using( m_detailColumnManager.DeferUpdate() )
          {
            var keys = new HashSet<SynchronizationKey>( this.GetNewKeys() );

            foreach( var key in m_pairedColumns.Keys.Except( keys ).ToArray() )
            {
              this.Desynchronize( key );
            }

            foreach( var key in keys.OrderBy( item => item.OrderKey ) )
            {
              this.Synchronize( key );
            }

            this.SetMainColumn();
          }
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

      var collection = m_pairedColumns;

      SynchronizationEntry pair;
      if( !collection.TryGetValue( key, out pair ) )
      {
        if( key.MasterColumn != null )
        {
          pair = new BoundColumn( this, key );
        }
        else
        {
          pair = new UnboundColumn( this, key );
        }

        collection.Add( key, pair );
      }

      pair.Synchronize();
    }

    private void Desynchronize( SynchronizationKey key )
    {
      if( key == null )
        return;

      var collection = m_pairedColumns;

      SynchronizationEntry pair;
      if( !collection.TryGetValue( key, out pair ) )
        return;

      collection.Remove( key );
      pair.Desynchronize();
    }

    private void Desynchronize()
    {
      var collection = m_pairedColumns;
      while( collection.Count > 0 )
      {
        this.Desynchronize( collection.First().Key );
      }
    }

    private void SetMainColumn()
    {
      if( !this.IsSynchronizationReady || !this.AreDetailsFlatten )
        return;

      Debug.Assert( m_detailColumnManager != null );

      var detailColumns = m_detailColumnManager.Columns;
      var detailColumn = default( ColumnBase );

      var mainColumn = m_masterColumnManager.Columns.MainColumn;
      if( mainColumn != null )
      {
        if( !DataGridItemPropertyMapHelper.TryGetDetailColumn( m_itemPropertyMap, detailColumns, mainColumn, out detailColumn ) )
        {
          detailColumn = null;
        }
      }

      detailColumns.MainColumn = detailColumn;
    }

    private IEnumerable<SynchronizationKey> GetNewKeys()
    {
      foreach( var detailColumn in m_detailColumnManager.Columns )
      {
        var masterColumn = default( ColumnBase );

        if( !DataGridItemPropertyMapHelper.TryGetMasterColumn( m_itemPropertyMap, m_masterColumnManager.Columns, detailColumn, out masterColumn ) )
        {
          masterColumn = null;
        }

        yield return new SynchronizationKey( masterColumn, detailColumn, m_detailConfiguration );
      }
    }

    private void OnViewChanged()
    {
      this.Refresh();
    }

    private void OnMasterColumnsPropertyChanged( PropertyChangedEventArgs e )
    {
      var propertyName = e.PropertyName;
      bool mayHaveChanged = string.IsNullOrEmpty( propertyName );

      if( mayHaveChanged || ( propertyName == ColumnCollection.MainColumnPropertyName ) )
      {
        this.SetMainColumn();
      }
    }

    private void OnDetailConfigurationPropertyChanged( PropertyChangedEventArgs e )
    {
      var propertyName = e.PropertyName;
      bool mayHaveChanged = string.IsNullOrEmpty( propertyName );

      if( mayHaveChanged || ( propertyName == DetailConfiguration.DataGridControlPropertyName ) )
      {
        this.DataGridControl = m_detailConfiguration.DataGridControl;
      }
    }

    private void OnItemPropertyMapMappingChanged( EventArgs e )
    {
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
      Debug.Assert( sender != null );

      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        if( ( m_masterColumnManager != null ) && ( sender == m_masterColumnManager.Columns ) )
        {
          this.OnMasterColumnsCollectionChanged( ( NotifyCollectionChangedEventArgs )e );
        }
        else if( ( m_detailColumnManager != null ) && ( sender == m_detailColumnManager.Columns ) )
        {
          this.OnDetailColumnsCollectionChanged( ( NotifyCollectionChangedEventArgs )e );
        }
      }
      else if( managerType == typeof( PropertyChangedEventManager ) )
      {
        if( ( m_masterColumnManager != null ) && ( sender == m_masterColumnManager.Columns ) )
        {
          this.OnMasterColumnsPropertyChanged( ( PropertyChangedEventArgs )e );
        }
        else if( sender == m_detailConfiguration )
        {
          this.OnDetailConfigurationPropertyChanged( ( PropertyChangedEventArgs )e );
        }
      }
      else if( managerType == typeof( ColumnsLayoutChangedEventManager ) )
      {
        if( sender == m_masterColumnManager )
        {
          this.OnMasterVisibleColumnsUpdated( e );
        }
        else if( sender == m_detailColumnManager )
        {
          this.OnDetailVisibleColumnsUpdated( e );
        }
      }
      else if( managerType == typeof( MappingChangedEventManager ) )
      {
        if( sender == m_itemPropertyMap )
        {
          this.OnItemPropertyMapMappingChanged( e );
        }
      }
      else if( managerType == typeof( ViewChangedEventManager ) )
      {
        if( sender == m_dataGridControl )
        {
          this.OnViewChanged();
        }
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    private readonly DetailConfiguration m_detailConfiguration;
    private readonly DataGridItemPropertyMap m_itemPropertyMap;
    private readonly Dictionary<SynchronizationKey, SynchronizationEntry> m_pairedColumns = new Dictionary<SynchronizationKey, SynchronizationEntry>( 0 );

    private bool m_isUpdatingColumns;

    #region DeferState Private Class

    private sealed class DeferState : DeferredDisposableState
    {
      internal DeferState( ColumnSynchronizationManager target )
      {
        Debug.Assert( target != null );
        m_target = target;
      }

      protected override bool IsDeferred
      {
        get
        {
          return m_target.IsSynchronizationDeferred;
        }
      }

      protected override void Increment()
      {
        m_target.m_deferSynchronizationCount++;
      }

      protected override void Decrement()
      {
        m_target.m_deferSynchronizationCount--;
      }

      protected override void OnDeferEnded( bool disposing )
      {
        m_target.UpdateColumns();
      }

      private readonly ColumnSynchronizationManager m_target;
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
      protected SynchronizationEntry( ColumnSynchronizationManager owner, SynchronizationKey key )
      {
        if( owner == null )
          throw new ArgumentNullException( "owner" );

        if( key == null )
          throw new ArgumentNullException( "key" );

        m_owner = owner;
        m_key = key;
      }

      #region Owner Protected Property

      protected ColumnSynchronizationManager Owner
      {
        get
        {
          return m_owner;
        }
      }

      private readonly ColumnSynchronizationManager m_owner;

      #endregion

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
      internal BoundColumn( ColumnSynchronizationManager owner, SynchronizationKey key )
        : base( owner, key )
      {
        if( key.MasterColumn == null )
          throw new ArgumentException( "The master column is not set.", "key" );
      }

      public override void Synchronize()
      {
        var masterColumn = this.MasterColumn;
        var detailColumn = this.DetailColumn;

        if( m_localValues == null )
        {
          m_localValues = new Dictionary<DependencyProperty, object>( 8 );

          masterColumn.PropertyChanged += new PropertyChangedEventHandler( this.OnMasterColumnPropertyChanged );
          masterColumn.FittedWidthRequested += new FittedWidthRequestedEventHandler( this.OnMasterColumnFittedWidthRequested );

          BoundColumn.StoreLocalValue( m_localValues, detailColumn, ColumnBase.WidthProperty );
          BoundColumn.StoreLocalValue( m_localValues, detailColumn, ColumnBase.MinWidthProperty );
          BoundColumn.StoreLocalValue( m_localValues, detailColumn, ColumnBase.MaxWidthProperty );
          BoundColumn.StoreLocalValue( m_localValues, detailColumn, ColumnBase.DesiredWidthProperty );
          BoundColumn.StoreLocalValue( m_localValues, detailColumn, ColumnBase.VisibleProperty );
          BoundColumn.StoreLocalValue( m_localValues, detailColumn, ColumnReorderingDragSourceManager.AnimatedColumnReorderingTranslationProperty );
          BoundColumn.StoreLocalValue( m_localValues, detailColumn, TableflowView.IsBeingDraggedAnimatedProperty );
          BoundColumn.StoreLocalValue( m_localValues, detailColumn, TableflowView.ColumnReorderingDragSourceManagerProperty );
        }

        BoundColumn.SetValue( masterColumn, detailColumn, ColumnBase.WidthProperty );
        BoundColumn.SetValue( masterColumn, detailColumn, ColumnBase.MinWidthProperty );
        BoundColumn.SetValue( masterColumn, detailColumn, ColumnBase.MaxWidthProperty );
        BoundColumn.SetValue( masterColumn, detailColumn, ColumnBase.DesiredWidthProperty );
        BoundColumn.SetValue( masterColumn, detailColumn, ColumnBase.VisibleProperty );
        BoundColumn.SetValue( masterColumn, detailColumn, ColumnReorderingDragSourceManager.AnimatedColumnReorderingTranslationProperty );
        BoundColumn.SetValue( masterColumn, detailColumn, TableflowView.IsBeingDraggedAnimatedProperty );
        BoundColumn.SetValue( masterColumn, detailColumn, TableflowView.ColumnReorderingDragSourceManagerProperty );

        this.UpdateTargetPosition();
      }

      public override void Desynchronize()
      {
        if( m_localValues == null )
          return;

        var localValues = m_localValues;
        m_localValues = null;

        var masterColumn = this.MasterColumn;
        masterColumn.PropertyChanged -= new PropertyChangedEventHandler( this.OnMasterColumnPropertyChanged );
        masterColumn.FittedWidthRequested -= new FittedWidthRequestedEventHandler( this.OnMasterColumnFittedWidthRequested );

        var detailColumn = this.DetailColumn;
        BoundColumn.RestoreLocalValue( localValues, detailColumn, ColumnBase.WidthProperty );
        BoundColumn.RestoreLocalValue( localValues, detailColumn, ColumnBase.MinWidthProperty );
        BoundColumn.RestoreLocalValue( localValues, detailColumn, ColumnBase.MaxWidthProperty );
        BoundColumn.RestoreLocalValue( localValues, detailColumn, ColumnBase.DesiredWidthProperty );
        BoundColumn.RestoreLocalValue( localValues, detailColumn, ColumnBase.VisibleProperty );
        BoundColumn.RestoreLocalValue( localValues, detailColumn, ColumnReorderingDragSourceManager.AnimatedColumnReorderingTranslationProperty );
        BoundColumn.RestoreLocalValue( localValues, detailColumn, TableflowView.IsBeingDraggedAnimatedProperty );
        BoundColumn.RestoreLocalValue( localValues, detailColumn, TableflowView.ColumnReorderingDragSourceManagerProperty );

        detailColumn.ClearValue( ColumnBase.VisiblePositionProperty );
      }

      private static void StoreLocalValue( Dictionary<DependencyProperty, object> store, ColumnBase column, DependencyProperty property )
      {
        Debug.Assert( store != null );
        Debug.Assert( column != null );
        Debug.Assert( property != null );

        var binding = BindingOperations.GetBindingBase( column, property );
        if( binding != null )
        {
          store[ property ] = binding;
        }
        else
        {
          var value = column.ReadLocalValue( property );
          if( value != DependencyProperty.UnsetValue )
          {
            store[ property ] = value;
          }
          else
          {
            store.Remove( property );
          }
        }
      }

      private static void RestoreLocalValue( Dictionary<DependencyProperty, object> store, ColumnBase column, DependencyProperty property )
      {
        Debug.Assert( store != null );
        Debug.Assert( column != null );
        Debug.Assert( property != null );

        object value;
        if( !store.TryGetValue( property, out value ) || ( value == DependencyProperty.UnsetValue ) )
        {
          column.ClearValue( property );
        }
        else if( value is BindingBase )
        {
          BindingOperations.SetBinding( column, property, ( BindingBase )value );
        }
        else
        {
          column.SetValue( property, value );
        }
      }

      private static void SetValue( ColumnBase source, ColumnBase destination, DependencyProperty property, string propertyName )
      {
        Debug.Assert( property != null );

        if( string.IsNullOrEmpty( propertyName ) || ( propertyName == property.Name ) )
        {
          BoundColumn.SetValue( source, destination, property );
        }
      }

      private static void SetValue( ColumnBase source, ColumnBase destination, DependencyProperty property )
      {
        Debug.Assert( source != null );
        Debug.Assert( destination != null );
        Debug.Assert( property != null );

        destination.SetValue( property, source.GetValue( property ) );
      }

      private void UpdateTargetPosition()
      {
        var detailConfig = this.DetailConfiguration;
        var dataGridControl = detailConfig.DataGridControl;

        if( dataGridControl == null )
          return;

        var masterColumn = this.MasterColumn;
        var masterColumnManager = DataGridControl.GetDataGridContext( dataGridControl ).ColumnManager;
        var masterColumnLocation = masterColumnManager.GetColumnLocationFor( masterColumn );

        if( masterColumnLocation == null )
          return;

        var detailColumn = this.DetailColumn;
        var detailColumnManager = detailConfig.ColumnManager;
        var detailColumnLocation = detailColumnManager.GetColumnLocationFor( detailColumn );

        if( detailColumnLocation == null )
          return;

        var map = detailConfig.ItemPropertyMap;
        var previousMasterLocation = masterColumnLocation.GetPreviousSiblingOrCousin();
        Debug.Assert( previousMasterLocation != null );

        switch( previousMasterLocation.Type )
        {
          case LocationType.Start:
          case LocationType.Splitter:
            {
              for( var previousDetailLocation = detailColumnLocation.GetPreviousSiblingOrCousin(); previousDetailLocation != null; previousDetailLocation = previousDetailLocation.GetPreviousSiblingOrCousin() )
              {
                // The detail column is at the appropriate location.
                if( previousDetailLocation.Type == previousMasterLocation.Type )
                  return;

                if( previousDetailLocation.Type != LocationType.Column )
                  break;

                ColumnBase unused;
                if( DataGridItemPropertyMapHelper.TryGetMasterColumn( map, masterColumnManager.Columns, ( ( ColumnHierarchyManager.IColumnLocation )previousDetailLocation ).Column, out unused ) )
                  break;
              }
            }
            break;

          case LocationType.Column:
            {
              var previousMasterColumn = ( ( ColumnHierarchyManager.IColumnLocation )previousMasterLocation ).Column;

              for( var previousDetailLocation = detailColumnLocation.GetPreviousSiblingOrCousin(); previousDetailLocation != null; previousDetailLocation = previousDetailLocation.GetPreviousSiblingOrCousin() )
              {
                if( previousDetailLocation.Type != LocationType.Column )
                  break;

                ColumnBase targetMasterColumn;
                if( DataGridItemPropertyMapHelper.TryGetMasterColumn( map, masterColumnManager.Columns, ( ( ColumnHierarchyManager.IColumnLocation )previousDetailLocation ).Column, out targetMasterColumn ) )
                {
                  // The detail column is at the appropriate location.
                  if( previousMasterColumn == targetMasterColumn )
                    return;
                }
              }
            }
            break;

          default:
            // Unexpected location.
            throw new NotSupportedException();
        }

        var nextMasterLocation = masterColumnLocation.GetNextSiblingOrCousin();
        Debug.Assert( nextMasterLocation != null );

        switch( nextMasterLocation.Type )
        {
          case LocationType.Splitter:
          case LocationType.Orphan:
            {
              for( var nextDetailLocation = detailColumnLocation.GetNextSiblingOrCousin(); nextDetailLocation != null; nextDetailLocation = nextDetailLocation.GetNextSiblingOrCousin() )
              {
                // The detail column is at the appropriate location.
                if( nextDetailLocation.Type == nextMasterLocation.Type )
                  return;

                if( nextDetailLocation.Type != LocationType.Column )
                  break;

                ColumnBase unused;
                if( DataGridItemPropertyMapHelper.TryGetMasterColumn( map, masterColumnManager.Columns, ( ( ColumnHierarchyManager.IColumnLocation )nextDetailLocation ).Column, out unused ) )
                  break;
              }
            }
            break;

          case LocationType.Column:
            {
              var nextMasterColumn = ( ( ColumnHierarchyManager.IColumnLocation )nextMasterLocation ).Column;

              for( var nextDetailLocation = detailColumnLocation.GetNextSiblingOrCousin(); nextDetailLocation != null; nextDetailLocation = nextDetailLocation.GetNextSiblingOrCousin() )
              {
                if( nextDetailLocation.Type != LocationType.Column )
                  break;

                ColumnBase targetMasterColumn;
                if( DataGridItemPropertyMapHelper.TryGetMasterColumn( map, masterColumnManager.Columns, ( ( ColumnHierarchyManager.IColumnLocation )nextDetailLocation ).Column, out targetMasterColumn ) )
                {
                  // The detail column is at the appropriate location.
                  if( nextMasterColumn == targetMasterColumn )
                    return;
                }
              }
            }
            break;

          default:
            // Unexpected location.
            throw new NotSupportedException();
        }

        // If we get here, it means that the column is really not at the appropriate location.
        for( var pivotMasterLocation = previousMasterLocation; pivotMasterLocation != null; pivotMasterLocation = pivotMasterLocation.GetPreviousSiblingOrCousin() )
        {
          if( pivotMasterLocation.Type == LocationType.Column )
          {
            ColumnBase pivotDetailColumn;
            if( !DataGridItemPropertyMapHelper.TryGetDetailColumn( map, detailColumnManager.Columns, ( ( ColumnHierarchyManager.IColumnLocation )pivotMasterLocation ).Column, out pivotDetailColumn ) )
              continue;

            var pivotDetailLocation = ( pivotDetailColumn != null ) ? detailColumnManager.GetColumnLocationFor( pivotDetailColumn ) : null;
            if( pivotDetailLocation == null )
              continue;

            Debug.Assert( detailColumnLocation.CanMoveAfter( pivotDetailLocation ) );
            detailColumnLocation.MoveAfter( pivotDetailLocation );
          }
          else
          {
            switch( pivotMasterLocation.Type )
            {
              case LocationType.Start:
                {
                  var pivotDetailLocation = detailColumnManager.GetLevelMarkersFor( detailColumnManager.Columns ).Start;

                  if( !detailColumnLocation.CanMoveAfter( pivotDetailLocation ) )
                    throw new NotSupportedException();

                  detailColumnLocation.MoveAfter( pivotDetailLocation );
                }
                break;

              case LocationType.Splitter:
                {
                  var pivotDetailLocation = detailColumnManager.GetLevelMarkersFor( detailColumnManager.Columns ).Splitter;

                  if( !detailColumnLocation.CanMoveAfter( pivotDetailLocation ) )
                    throw new NotSupportedException();

                  detailColumnLocation.MoveAfter( pivotDetailLocation );
                }
                break;

              default:
                // Unexpected location.
                throw new NotSupportedException();
            }
          }

          // The detail column is now at the appropriate location.
          return;
        }
      }

      private void OnMasterColumnPropertyChanged( object sender, PropertyChangedEventArgs e )
      {
        var masterColumn = sender as ColumnBase;
        if( masterColumn == null )
          return;

        Debug.Assert( masterColumn == this.MasterColumn );

        var masterColumnManager = this.Owner.MasterColumnManager;
        if( ( masterColumnManager == null ) || masterColumnManager.IsUpdateDeferred )
          return;

        var detailColumn = this.DetailColumn;
        var propertyName = e.PropertyName;

        BoundColumn.SetValue( masterColumn, detailColumn, ColumnBase.WidthProperty, propertyName );
        BoundColumn.SetValue( masterColumn, detailColumn, ColumnBase.MinWidthProperty, propertyName );
        BoundColumn.SetValue( masterColumn, detailColumn, ColumnBase.MaxWidthProperty, propertyName );
        BoundColumn.SetValue( masterColumn, detailColumn, ColumnBase.DesiredWidthProperty, propertyName );
        BoundColumn.SetValue( masterColumn, detailColumn, ColumnBase.VisibleProperty, propertyName );
        BoundColumn.SetValue( masterColumn, detailColumn, ColumnReorderingDragSourceManager.AnimatedColumnReorderingTranslationProperty, propertyName );
        BoundColumn.SetValue( masterColumn, detailColumn, TableflowView.IsBeingDraggedAnimatedProperty, propertyName );
        BoundColumn.SetValue( masterColumn, detailColumn, TableflowView.ColumnReorderingDragSourceManagerProperty, propertyName );
      }

      private void OnMasterColumnFittedWidthRequested( object sender, FittedWidthRequestedEventArgs e )
      {
        Debug.Assert( ( ColumnBase )sender == this.MasterColumn );

        var fittedWidth = this.DetailColumn.GetFittedWidth();
        if( fittedWidth < 0d )
          return;

        e.SetValue( fittedWidth );
      }

      private Dictionary<DependencyProperty, object> m_localValues;
    }

    #endregion

    #region UnboundColumn Private Class

    private sealed class UnboundColumn : SynchronizationEntry
    {
      internal UnboundColumn( ColumnSynchronizationManager owner, SynchronizationKey key )
        : base( owner, key )
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

      private object m_localValue = DependencyProperty.UnsetValue;
    }

    #endregion
  }
}
