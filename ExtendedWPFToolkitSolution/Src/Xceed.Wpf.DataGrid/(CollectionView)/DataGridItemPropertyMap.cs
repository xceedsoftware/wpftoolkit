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
using Xceed.Utils.Wpf;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DataGridItemPropertyMap : IWeakEventListener
  {
    #region MasterItemProperties Property

    internal DataGridItemPropertyCollection MasterItemProperties
    {
      get
      {
        return m_masterItemProperties;
      }
      set
      {
        if( value == m_masterItemProperties )
          return;

        using( this.DeferMappingChanged() )
        {
          this.UnregisterItemProperties( m_masterItemProperties );
          this.UnmapItemProperties();

          m_masterItemProperties = value;

          this.MapItemProperties();
          this.RegisterItemProperties( m_masterItemProperties );
        }
      }
    }

    private DataGridItemPropertyCollection m_masterItemProperties;

    #endregion

    #region DetailItemProperties Property

    internal DataGridItemPropertyCollection DetailItemProperties
    {
      get
      {
        return m_detailItemProperties;
      }
      set
      {
        if( value == m_detailItemProperties )
          return;

        using( this.DeferMappingChanged() )
        {
          this.UnregisterItemProperties( m_detailItemProperties );
          this.UnmapItemProperties();

          m_detailItemProperties = value;

          this.MapItemProperties();
          this.RegisterItemProperties( m_detailItemProperties );
        }
      }
    }

    private DataGridItemPropertyCollection m_detailItemProperties;

    #endregion

    #region IsMapping Property

    internal bool IsMapping
    {
      get
      {
        return ( m_masterItemProperties != null )
            && ( m_detailItemProperties != null );
      }
    }

    #endregion

    #region MappingChanged Event

    internal event EventHandler MappingChanged;

    private void OnMappingChanged()
    {
      if( m_deferRaiseMappingChangedCount != 0 )
      {
        m_raiseMappingChanged = true;
      }
      else
      {
        m_raiseMappingChanged = false;

        var handler = this.MappingChanged;
        if( handler == null )
          return;

        handler.Invoke( this, EventArgs.Empty );
      }
    }

    #endregion

    internal IDisposable DeferMappingChanged()
    {
      return new DeferredDisposable( new DeferMappingChangedEvent( this ) );
    }

    internal bool TryGetMasterItemProperty( DataGridItemPropertyBase detailItemProperty, out DataGridItemPropertyBase masterItemProperty )
    {
      return DataGridItemPropertyMap.TryGetTargetItemProperty( m_detailToMaster, detailItemProperty, out masterItemProperty );
    }

    internal bool TryGetDetailItemProperty( DataGridItemPropertyBase masterItemProperty, out DataGridItemPropertyBase detailItemProperty )
    {
      return DataGridItemPropertyMap.TryGetTargetItemProperty( m_masterToDetail, masterItemProperty, out detailItemProperty );
    }

    private static bool TryGetTargetItemProperty( Dictionary<DataGridItemPropertyBase, DataGridItemPropertyBase> collection, DataGridItemPropertyBase sourceItemProperty, out DataGridItemPropertyBase targetItemProperty )
    {
      if( ( collection != null ) && ( sourceItemProperty != null ) && collection.TryGetValue( sourceItemProperty, out targetItemProperty ) )
        return true;

      targetItemProperty = default( DataGridItemPropertyBase );

      return false;
    }

    private void RegisterItemProperties( DataGridItemPropertyCollection itemProperties )
    {
      if( itemProperties == null )
        return;

      CollectionChangedEventManager.AddListener( itemProperties, this );

      foreach( var itemProperty in itemProperties )
      {
        this.RegisterItemProperty( itemProperty );
      }
    }

    private void UnregisterItemProperties( DataGridItemPropertyCollection itemProperties )
    {
      if( itemProperties == null )
        return;

      foreach( var itemProperty in itemProperties )
      {
        this.UnregisterItemProperty( itemProperty );
      }

      CollectionChangedEventManager.RemoveListener( itemProperties, this );
    }

    private void RegisterItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return;

      if( !m_listeningToPropertyChanged.Add( itemProperty ) )
        return;

      PropertyChangedEventManager.AddListener( itemProperty, this, string.Empty );
      this.RegisterItemProperties( itemProperty.ItemPropertiesInternal );
    }

    private void UnregisterItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return;

      if( !m_listeningToPropertyChanged.Remove( itemProperty ) )
        return;

      this.UnregisterItemProperties( itemProperty.ItemPropertiesInternal );
      PropertyChangedEventManager.RemoveListener( itemProperty, this, string.Empty );
    }

    private void MapItemProperties()
    {
      this.MapItemProperties( m_masterItemProperties, m_detailItemProperties );
    }

    private void MapItemProperties( DataGridItemPropertyCollection masterItemProperties, DataGridItemPropertyCollection detailItemProperties )
    {
      if( ( masterItemProperties == null ) || ( masterItemProperties.Count <= 0 ) )
        return;

      foreach( var masterItemProperty in masterItemProperties )
      {
        this.MapMasterItemProperty( masterItemProperty );
      }
    }

    private void MapItemProperties( DataGridItemPropertyBase masterItemProperty, DataGridItemPropertyBase detailItemProperty )
    {
      if( ( masterItemProperty == null ) || ( detailItemProperty == null ) )
        return;

      DataGridItemPropertyBase mappedItemProperty;
      if( m_masterToDetail.TryGetValue( masterItemProperty, out mappedItemProperty ) )
      {
        if( mappedItemProperty == detailItemProperty )
          return;

        this.UnmapMasterItemProperty( masterItemProperty );
      }

      this.UnmapDetailItemProperty( detailItemProperty );

      m_masterToDetail[ masterItemProperty ] = detailItemProperty;
      m_detailToMaster[ detailItemProperty ] = masterItemProperty;

      this.OnMappingChanged();
      this.MapItemProperties( masterItemProperty.ItemPropertiesInternal, detailItemProperty.ItemPropertiesInternal );
    }

    private void MapMasterItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return;

      var collection = itemProperty.ContainingCollection;
      if( collection == null )
        return;

      DataGridItemPropertyCollection mappedItemProperties;
      DataGridItemPropertyBase mappedItemProperty;

      var owner = collection.Owner;
      if( owner == null )
      {
        Debug.Assert( collection == m_masterItemProperties );
        if( collection != m_masterItemProperties )
          return;

        mappedItemProperties = m_detailItemProperties;
      }
      else
      {
        if( !m_masterToDetail.TryGetValue( owner, out mappedItemProperty ) )
          return;

        mappedItemProperties = mappedItemProperty.ItemPropertiesInternal;
      }

      if( mappedItemProperties == null )
        return;

      mappedItemProperty = mappedItemProperties.GetForSynonym( itemProperty.Name );

      if( mappedItemProperty != null )
      {
        this.MapItemProperties( itemProperty, mappedItemProperty );
      }
      else
      {
        this.UnmapMasterItemProperty( itemProperty );
      }
    }

    private void MapDetailItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return;

      var collection = itemProperty.ContainingCollection;
      if( collection == null )
        return;

      DataGridItemPropertyCollection mappedItemProperties;
      DataGridItemPropertyBase mappedItemProperty;

      var owner = collection.Owner;
      if( owner == null )
      {
        Debug.Assert( collection == m_detailItemProperties );
        if( collection != m_detailItemProperties )
          return;

        mappedItemProperties = m_masterItemProperties;
      }
      else
      {
        if( !m_detailToMaster.TryGetValue( owner, out mappedItemProperty ) )
          return;

        mappedItemProperties = mappedItemProperty.ItemPropertiesInternal;
      }

      if( mappedItemProperties == null )
        return;

      mappedItemProperty = ( !string.IsNullOrEmpty( itemProperty.Synonym ) ) ? mappedItemProperties[ itemProperty.Synonym ] : null;

      if( mappedItemProperty != null )
      {
        this.MapItemProperties( mappedItemProperty, itemProperty );
      }
      else
      {
        this.UnmapDetailItemProperty( itemProperty );
      }
    }

    private void UnmapItemProperties()
    {
      if( ( m_masterItemProperties == null ) || ( m_detailItemProperties == null ) )
        return;

      while( m_masterToDetail.Count > 0 )
      {
        var entry = m_masterToDetail.First();

        this.UnmapItemProperties( entry.Key, entry.Value, false );
      }

      Debug.Assert( m_masterToDetail.Count == 0 );
      Debug.Assert( m_detailToMaster.Count == 0 );

      while( m_detailToMaster.Count > 0 )
      {
        var entry = m_detailToMaster.First();

        this.UnmapItemProperties( entry.Value, entry.Key, false );
      }

      m_masterToDetail.Clear();
      m_detailToMaster.Clear();
    }

    private void UnmapItemProperties( DataGridItemPropertyBase masterItemProperty, DataGridItemPropertyBase detailItemProperty, bool recursive )
    {
      if( ( masterItemProperty == null ) || ( detailItemProperty == null ) )
        return;

      DataGridItemPropertyBase mappedItemProperty;
      if( !m_masterToDetail.TryGetValue( masterItemProperty, out mappedItemProperty ) || ( mappedItemProperty != detailItemProperty ) )
        throw new InvalidOperationException();

      if( !m_detailToMaster.TryGetValue( detailItemProperty, out mappedItemProperty ) || ( mappedItemProperty != masterItemProperty ) )
        throw new InvalidOperationException();

      m_masterToDetail.Remove( masterItemProperty );
      m_detailToMaster.Remove( detailItemProperty );

      this.OnMappingChanged();

      if( recursive )
      {
        this.UnmapMasterItemProperties( masterItemProperty.ItemPropertiesInternal );
        this.UnmapDetailItemProperties( detailItemProperty.ItemPropertiesInternal );
      }
    }

    private void UnmapMasterItemProperties( DataGridItemPropertyCollection itemProperties )
    {
      if( itemProperties == null )
        return;

      foreach( var itemProperty in itemProperties )
      {
        this.UnmapMasterItemProperty( itemProperty );
      }
    }

    private void UnmapDetailItemProperties( DataGridItemPropertyCollection itemProperties )
    {
      if( itemProperties == null )
        return;

      foreach( var itemProperty in itemProperties )
      {
        this.UnmapDetailItemProperty( itemProperty );
      }
    }

    private void UnmapMasterItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return;

      DataGridItemPropertyBase mappedItemProperty;
      if( !m_masterToDetail.TryGetValue( itemProperty, out mappedItemProperty ) )
        return;

      Debug.Assert( mappedItemProperty != null );

      this.UnmapItemProperties( itemProperty, mappedItemProperty, true );
    }

    private void UnmapDetailItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return;

      DataGridItemPropertyBase mappedItemProperty;
      if( !m_detailToMaster.TryGetValue( itemProperty, out mappedItemProperty ) )
        return;

      Debug.Assert( mappedItemProperty != null );

      this.UnmapItemProperties( mappedItemProperty, itemProperty, true );
    }

    private void OnItemPropertyCollectionChanged( DataGridItemPropertyCollection collection, NotifyCollectionChangedEventArgs e )
    {
      var rootCollection = ItemsSourceHelper.GetRootCollection( collection );
      if( rootCollection == null )
        return;

      if( rootCollection == m_masterItemProperties )
      {
        if( e.Action == NotifyCollectionChangedAction.Reset )
          throw new NotSupportedException();

        if( e.Action == NotifyCollectionChangedAction.Move )
          return;

        using( this.DeferMappingChanged() )
        {
          if( e.OldItems != null )
          {
            foreach( DataGridItemPropertyBase itemProperty in e.OldItems )
            {
              this.UnregisterItemProperty( itemProperty );
              this.UnmapMasterItemProperty( itemProperty );
            }
          }

          if( e.NewItems != null )
          {
            foreach( DataGridItemPropertyBase itemProperty in e.NewItems )
            {
              this.RegisterItemProperty( itemProperty );
              this.MapMasterItemProperty( itemProperty );
            }
          }
        }
      }
      else if( rootCollection == m_detailItemProperties )
      {
        if( e.Action == NotifyCollectionChangedAction.Reset )
          throw new NotSupportedException();

        if( e.Action == NotifyCollectionChangedAction.Move )
          return;

        using( this.DeferMappingChanged() )
        {
          if( e.OldItems != null )
          {
            foreach( DataGridItemPropertyBase itemProperty in e.OldItems )
            {
              this.UnregisterItemProperty( itemProperty );
              this.UnmapDetailItemProperty( itemProperty );
            }
          }

          if( e.NewItems != null )
          {
            foreach( DataGridItemPropertyBase itemProperty in e.NewItems )
            {
              this.RegisterItemProperty( itemProperty );
              this.MapDetailItemProperty( itemProperty );
            }
          }
        }
      }
      else
      {
        Debug.Fail( "The collection should have been either for the master or the detail item properties." );
        CollectionChangedEventManager.RemoveListener( collection, this );
      }
    }

    private void OnItemPropertyPropertyChanged( DataGridItemPropertyBase itemProperty, PropertyChangedEventArgs e )
    {
      var rootCollection = ItemsSourceHelper.GetRootCollection( itemProperty );
      if( rootCollection == null )
        return;

      using( this.DeferMappingChanged() )
      {
        if( string.IsNullOrEmpty( e.PropertyName ) || ( e.PropertyName == DataGridItemPropertyBase.SynonymPropertyName ) )
        {
          if( rootCollection == m_detailItemProperties )
          {
            this.MapDetailItemProperty( itemProperty );
          }
        }

        if( string.IsNullOrEmpty( e.PropertyName ) || ( e.PropertyName == DataGridItemPropertyBase.ItemPropertiesInternalPropertyName ) )
        {
          var itemProperties = itemProperty.ItemPropertiesInternal;
          if( itemProperties != null )
          {
            this.UnregisterItemProperties( itemProperties );
            this.RegisterItemProperties( itemProperties );

            if( rootCollection == m_masterItemProperties )
            {
              foreach( var childItemProperty in itemProperties )
              {
                this.MapMasterItemProperty( childItemProperty );
              }
            }
            else if( rootCollection == m_detailItemProperties )
            {
              foreach( var childItemProperty in itemProperties )
              {
                this.MapDetailItemProperty( childItemProperty );
              }
            }
          }
        }
      }
    }

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        this.OnItemPropertyCollectionChanged( ( DataGridItemPropertyCollection )sender, ( NotifyCollectionChangedEventArgs )e );
      }
      else if( managerType == typeof( PropertyChangedEventManager ) )
      {
        this.OnItemPropertyPropertyChanged( ( DataGridItemPropertyBase )sender, ( PropertyChangedEventArgs )e );
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    private readonly Dictionary<DataGridItemPropertyBase, DataGridItemPropertyBase> m_masterToDetail = new Dictionary<DataGridItemPropertyBase, DataGridItemPropertyBase>();
    private readonly Dictionary<DataGridItemPropertyBase, DataGridItemPropertyBase> m_detailToMaster = new Dictionary<DataGridItemPropertyBase, DataGridItemPropertyBase>();
    private readonly HashSet<DataGridItemPropertyBase> m_listeningToPropertyChanged = new HashSet<DataGridItemPropertyBase>();
    private int m_deferRaiseMappingChangedCount;
    private bool m_raiseMappingChanged; //false

    #region DeferMappingChangedEvent Private Class

    private sealed class DeferMappingChangedEvent : DeferredDisposableState
    {
      internal DeferMappingChangedEvent( DataGridItemPropertyMap target )
      {
        if( target == null )
          throw new ArgumentNullException( "target" );

        m_target = target;
      }

      protected override object SyncRoot
      {
        get
        {
          return m_target.m_listeningToPropertyChanged;
        }
      }

      protected override bool IsDeferred
      {
        get
        {
          return ( m_target.m_deferRaiseMappingChangedCount != 0 );
        }
      }

      protected override void Increment()
      {
        m_target.m_deferRaiseMappingChangedCount++;
      }

      protected override void Decrement()
      {
        m_target.m_deferRaiseMappingChangedCount--;
      }

      protected override void OnDeferEnded( bool disposing )
      {
        if( !disposing )
          return;

        if( m_target.m_raiseMappingChanged )
        {
          m_target.OnMappingChanged();
        }
      }

      private readonly DataGridItemPropertyMap m_target;
    }

    #endregion
  }
}
