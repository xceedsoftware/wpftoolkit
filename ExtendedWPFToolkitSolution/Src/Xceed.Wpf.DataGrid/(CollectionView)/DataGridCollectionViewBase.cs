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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Xceed.Utils.Collections;
using Xceed.Wpf.DataGrid.Utils;

namespace Xceed.Wpf.DataGrid
{
  public abstract partial class DataGridCollectionViewBase : CollectionView, IEditableCollectionView, IWeakEventListener, IItemProperties
  {
    #region Static Fields

    internal static readonly string GroupsPropertyName = PropertyHelper.GetPropertyName( ( DataGridCollectionViewBase c ) => c.Groups );
    internal static readonly string RootGroupPropertyName = PropertyHelper.GetPropertyName( ( DataGridCollectionViewBase c ) => c.RootGroup );

    #endregion

    #region Static Members

    internal static string GetPropertyNameFromGroupDescription( GroupDescription groupDescription )
    {
      var propertyGroupDescription = groupDescription as PropertyGroupDescription;
      if( propertyGroupDescription != null )
        return propertyGroupDescription.PropertyName;

      var dataGridGroupDescription = groupDescription as DataGridGroupDescription;
      if( dataGridGroupDescription != null )
        return dataGridGroupDescription.PropertyName;

      return null;
    }

    #endregion

//    static DataGridCollectionViewBase()
//    {
//      Licenser.VerifyLicenseSilently();
//    }

    private DataGridCollectionViewBase()
      : base( new object[ 0 ] )
    {

      this.PreConstruct();

      m_distinctValues = new DistinctValuesDictionary( this );

      m_filteredItemProperties = new List<DataGridItemPropertyBase>();
      m_readOnlyFilteredItemProperties = new ReadOnlyCollection<DataGridItemPropertyBase>( m_filteredItemProperties );

      // The constructor used for detail creation will rectify the m_rootDataGridCollectionViewBase later on.
      m_rootDataGridCollectionViewBase = this;

      m_deferredOperationManager = new DeferredOperationManager( this, this.Dispatcher, true );
    }

    internal DataGridCollectionViewBase(
      object modelSource,
      IEnumerable source,
      Type itemType,
      bool autoCreateItemProperties,
      bool autoCreateDetailDescriptions,
      bool autoCreateForeignKeyDescriptions )
      : this()
    {
      m_desiredItemType = itemType;

      m_itemProperties = new DataGridItemPropertyCollection();
      m_detailDescriptions = new DataGridDetailDescriptionCollection();
      m_defaultPropertyDescriptions = new PropertyDescriptionRouteDictionary();

      this.AutoCreateItemProperties = autoCreateItemProperties;
      this.AutoCreateDetailDescriptions = autoCreateDetailDescriptions;
      this.AutoCreateForeignKeyDescriptions = autoCreateForeignKeyDescriptions;

      if( itemType == null )
      {
        if( source != null )
        {
          itemType = ItemsSourceHelper.GetItemTypeFromEnumeration( source );
        }
        else if( modelSource != null )
        {
          var queryable = modelSource as IQueryable;
          if( queryable != null )
          {
            itemType = queryable.ElementType;
          }
        }
      }
      else
      {
        var listItemType = ItemsSourceHelper.GetItemTypeFromEnumeration( source ) ?? typeof( object );
        if( !listItemType.IsAssignableFrom( itemType ) && !itemType.IsAssignableFrom( listItemType ) )
          throw new InvalidOperationException( "The itemType is not assignable to the type of the list." );
      }

      m_modelSource = modelSource;
      m_itemType = itemType;
      m_enumeration = source;

      this.CreateDefaultCollections( null );
      this.RegisterChangedEvents();
      this.SetupCurrent( true );
      this.SetupDefaultPropertyDescriptions();
      this.SetupDefaultDetailDescriptions();
      this.PrepareItemProperties( m_itemProperties );

      if( this.AutoCreateItemProperties )
      {
        ItemsSourceHelper.CreateAndAddItemPropertiesForPropertyDescriptions( m_itemProperties, m_defaultPropertyDescriptions.Values );
        ItemsSourceHelper.AutoDetectSynonyms( this );
      }
    }

    internal DataGridCollectionViewBase( IEnumerable collection, DataGridDetailDescription parentDetailDescription, DataGridCollectionViewBase rootDataGridCollectionViewBase )
      : this()
    {
      if( parentDetailDescription == null )
        throw new ArgumentNullException( "parentDetailDescription" );

      m_rootDataGridCollectionViewBase = rootDataGridCollectionViewBase;
      m_parentDetailDescription = parentDetailDescription;
      m_itemProperties = parentDetailDescription.ItemProperties;
      m_detailDescriptions = parentDetailDescription.DetailDescriptions;
      m_defaultPropertyDescriptions = parentDetailDescription.DefaultPropertyDescriptions;

      m_distinctValuesConstraint = m_parentDetailDescription.DistinctValuesConstraint;

      this.AutoCreateDetailDescriptions = parentDetailDescription.AutoCreateDetailDescriptions;
      this.AutoCreateItemProperties = parentDetailDescription.AutoCreateItemProperties;
      this.AutoCreateForeignKeyDescriptions = parentDetailDescription.AutoCreateForeignKeyDescriptions;

      if( parentDetailDescription.ItemType == null )
      {
        parentDetailDescription.ItemType = ItemsSourceHelper.GetItemTypeFromEnumeration( collection ) ?? typeof( object );
      }

      m_modelSource = default( object );
      m_itemType = parentDetailDescription.ItemType;
      m_enumeration = collection;

      this.CreateDefaultCollections( m_parentDetailDescription );
      this.RegisterChangedEvents();
      this.SetupCurrent( false );
      this.SetupDefaultPropertyDescriptions();
      this.SetupDefaultDetailDescriptions();

      // Creates the item properties if auto-creation is ON and no one already created them.
      if( this.AutoCreateItemProperties && !m_parentDetailDescription.AutoCreateItemPropertiesCompleted )
      {
        ItemsSourceHelper.CreateAndAddItemPropertiesForPropertyDescriptions( m_itemProperties, m_defaultPropertyDescriptions.Values );
        ItemsSourceHelper.AutoDetectSynonyms( this );

        m_parentDetailDescription.AutoCreateItemPropertiesCompleted = true;
      }

      m_filteredItemProperties.Clear();

      this.PrepareItemProperties( m_itemProperties );

      // This is required in the Master/Detail scheme of things!
      this.ForceRefresh( false, true, false );
    }

    internal virtual void PreConstruct()
    {
    }

    #region ParentCollectionViewSource Internal Property

    internal DataGridCollectionViewSourceBase ParentCollectionViewSourceBase
    {
      get
      {
        return m_parentCollectionViewSourceBase;
      }
      set
      {
        if( ( m_parentCollectionViewSourceBase != null ) && ( value != null ) && ( m_parentCollectionViewSourceBase != value ) )
          throw new InvalidOperationException( "An attempt was made to use a DataGridCollectionView that is already being used by a DataGridCollectionViewSource." );

        m_parentCollectionViewSourceBase = value;
      }
    }

    private DataGridCollectionViewSourceBase m_parentCollectionViewSourceBase;

    #endregion

    #region NeedsRefresh Public Property

    public override bool NeedsRefresh
    {
      get
      {
        return m_deferredOperationManager.RefreshPending;
      }
    }

    #endregion

    #region Culture Public Property

    public override CultureInfo Culture
    {
      get
      {
        return base.Culture;
      }
      set
      {
        base.Culture = value;

        // This is the only way we found to update the extra properties defined on our CollectionViewSource.
        // In the CollectionViewSource.ApplyPropertiesToView method, the first thing that is done 
        // is setting the Culture; therefore, we have no choice but to use this as the entry point and then
        // apply our properties to the DataGridCollectionView.
        // 
        // See CollectionViewSource.ApplyPropertiesToView in reflector for a better understanding.
        if( ( this.InDeferRefresh ) && ( m_parentCollectionViewSourceBase != null ) )
        {
          m_parentCollectionViewSourceBase.ApplyExtraPropertiesToView( this );
        }
      }
    }

    #endregion

    #region AutoCreateItemProperties Public Property

    public bool AutoCreateItemProperties
    {
      get
      {
        return m_flags[ ( int )DataGridCollectionViewBaseFlags.AutoCreateItemProperties ];
      }
      private set
      {
        m_flags[ ( int )DataGridCollectionViewBaseFlags.AutoCreateItemProperties ] = value;
      }
    }

    #endregion

    #region AutoCreateDetailDescriptions Public Property

    internal bool AutoCreateDetailDescriptions
    {
      get
      {
        return m_flags[ ( int )DataGridCollectionViewBaseFlags.AutoCreateDetailDescriptions ];
      }
      private set
      {
        m_flags[ ( int )DataGridCollectionViewBaseFlags.AutoCreateDetailDescriptions ] = value;
      }
    }

    #endregion

    #region ParentDetailDescription Public Property

    internal DataGridDetailDescription ParentDetailDescription
    {
      get
      {
        return m_parentDetailDescription;
      }
    }


    #endregion

    #region AutoCreateForeignKeyDescriptions Public Property

    public bool AutoCreateForeignKeyDescriptions
    {
      get
      {
        return m_flags[ ( int )DataGridCollectionViewBaseFlags.AutoCreateForeignKeyDescriptions ];
      }
      private set
      {
        m_flags[ ( int )DataGridCollectionViewBaseFlags.AutoCreateForeignKeyDescriptions ] = value;
      }
    }

    #endregion

    #region SyncRoot Internal Property

    internal virtual object SyncRoot
    {
      get
      {
        if( m_syncRoot == null )
        {
          Interlocked.CompareExchange( ref m_syncRoot, new object(), null );
        }

        return m_syncRoot;
      }
    }

    private object m_syncRoot; //null

    #endregion

    #region DeferredOperationManager Internal Property

    internal DeferredOperationManager DeferredOperationManager
    {
      get
      {
        return m_deferredOperationManager;
      }
    }

    #endregion

    #region InDeferRefresh Internal Property

    internal bool InDeferRefresh
    {
      get
      {
        return ( m_deferRefreshCount > 0 );
      }
    }

    #endregion

    #region Loaded Internal Property

    internal bool Loaded
    {
      get
      {
        return m_flags[ ( int )DataGridCollectionViewBaseFlags.Loaded ];
      }
      set
      {
        m_flags[ ( int )DataGridCollectionViewBaseFlags.Loaded ] = value;
      }
    }

    #endregion

    #region IsRefreshingDestinctValues Internal Property

    internal bool IsRefreshingDistinctValues
    {
      get
      {
        return m_flags[ ( int )DataGridCollectionViewBaseFlags.IsRefreshingDistinctValues ];
      }
      set
      {
        m_flags[ ( int )DataGridCollectionViewBaseFlags.IsRefreshingDistinctValues ] = value;
      }
    }

    #endregion

    #region Refreshing Internal Property

    internal bool Refreshing
    {
      get
      {
        return m_flags[ ( int )DataGridCollectionViewBaseFlags.Refreshing ];
      }
      set
      {
        m_flags[ ( int )DataGridCollectionViewBaseFlags.Refreshing ] = value;
      }
    }

    #endregion

    #region ModelSource Internal Property

    internal object ModelSource
    {
      get
      {
        return m_modelSource;
      }
    }

    #endregion

    #region ItemEditionIsManuallyHandled Private Property

    private bool ItemEditionIsManuallyHandled
    {
      get
      {
        return m_flags[ ( int )DataGridCollectionViewBaseFlags.ItemEditionIsManuallyHandled ];
      }
      set
      {
        m_flags[ ( int )DataGridCollectionViewBaseFlags.ItemEditionIsManuallyHandled ] = value;
      }
    }

    #endregion

    #region CreatingNewItemIsManuallyHandled Private Property

    private bool CreatingNewItemIsManuallyHandled
    {
      get
      {
        return m_flags[ ( int )DataGridCollectionViewBaseFlags.CreatingNewItemIsManuallyHandled ];
      }
      set
      {
        m_flags[ ( int )DataGridCollectionViewBaseFlags.CreatingNewItemIsManuallyHandled ] = value;
      }
    }

    #endregion

    #region ItemProperties Public Property

    public DataGridItemPropertyCollection ItemProperties
    {
      get
      {
        return m_itemProperties;
      }
    }

    #endregion

    #region DefaultItemPropertiesInitialized Private Property

    private bool DefaultItemPropertiesInitialized
    {
      get
      {
        if( m_parentDetailDescription != null )
          return m_parentDetailDescription.DefaultItemPropertiesInitialized;

        return m_flags[ ( int )DataGridCollectionViewBaseFlags.DefaultItemPropertiesInitialized ];
      }
      set
      {
        if( m_parentDetailDescription != null )
        {
          m_parentDetailDescription.DefaultItemPropertiesInitialized = value;
        }
        else
        {
          m_flags[ ( int )DataGridCollectionViewBaseFlags.DefaultItemPropertiesInitialized ] = value;
        }
      }
    }

    #endregion

    #region DefaultPropertyDescriptionsCreated Private Property

    private bool DefaultPropertyDescriptionsCreated
    {
      get
      {
        if( m_parentDetailDescription != null )
          return m_parentDetailDescription.DefaultPropertyDescriptionsCreated;

        return m_flags[ ( int )DataGridCollectionViewBaseFlags.DefaultPropertyDescriptionsCreated ];
      }
      set
      {
        if( m_parentDetailDescription != null )
        {
          m_parentDetailDescription.DefaultPropertyDescriptionsCreated = value;
        }
        else
        {
          m_flags[ ( int )DataGridCollectionViewBaseFlags.DefaultPropertyDescriptionsCreated ] = value;
        }
      }
    }

    #endregion

    #region CanCreateItemPropertiesFormModelSource Private Property

    private bool CanCreateItemPropertiesFromModelSource
    {
      get
      {
        return ( ( m_modelSource != null ) && ( m_modelSource is DataTable || m_modelSource is IQueryable ) );
      }
    }

    #endregion

    #region DetailDeferRefreshes Internal Property

    internal List<List<IDisposable>> DetailDeferRefreshes
    {
      get
      {
        return m_detailDeferRefreshes;
      }
    }

    private List<List<IDisposable>> m_detailDeferRefreshes;

    #endregion

    #region DataGridContext Internal Property

    internal DataGridContext DataGridContext
    {
      get
      {
        if( m_dataGridContext == null )
          return null;

        return m_dataGridContext.Target as DataGridContext;
      }
      set
      {
        if( value == null )
        {
          m_dataGridContext = null;
        }
        else
        {
          m_dataGridContext = new WeakReference( value );
        }
      }
    }

    private WeakReference m_dataGridContext;

    #endregion

    #region DataGridCollectionViewBase ABSTRACT MEMBERS

    internal virtual int IndexOfSourceItem( object item )
    {
      return this.IndexOf( item );
    }

    internal abstract DataGridCollectionViewBase CreateDetailDataGridCollectionViewBase(
      IEnumerable detailDataSource,
      DataGridDetailDescription parentDetailDescription,
      DataGridCollectionViewBase parentDataGridCollectionViewBase );

    internal abstract void EnsurePosition( int globalSortedIndex );

    internal abstract int SourceItemCount
    {
      get;
    }

    internal abstract void ForceRefresh( bool sendResetNotification, bool initialLoad, bool setCurrentToFirstOnInitialLoad );

    #endregion

    #region SOURCE

    internal IEnumerable Enumeration
    {
      get
      {
        return m_enumeration;
      }
    }

    public Type ItemType
    {
      get
      {
        return m_itemType;
      }
    }

    internal Type DesiredItemType
    {
      get
      {
        return m_desiredItemType;
      }
    }

    internal bool HasSource
    {
      get
      {
        return ( m_enumeration != null );
      }
    }

    #endregion

    #region SOURCE COLLECTION CHANGED HANDLERS

    private void OnItemsSourceCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      Debug.Assert( !( this is DataGridVirtualizingCollectionViewBase ),
        "The DataGridVirtualizingCollectionView is unbound and therefore should never receive a notification from a source." );

      lock( m_deferredOperationManager )
      {
        if( m_deferredOperationManager.RefreshPending )
          return;

        DeferredOperation deferredOperation = null;

        switch( e.Action )
        {
          case NotifyCollectionChangedAction.Add:
            {
              CollectionView collectionView = m_enumeration as CollectionView;
              int count = ( collectionView == null ) ? -1 : collectionView.Count;

              deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.Add, count, e.NewStartingIndex, e.NewItems );
              break;
            }

          case NotifyCollectionChangedAction.Move:
            {
              deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.Move, -1, e.NewStartingIndex, e.NewItems, e.OldStartingIndex, e.OldItems );
              break;
            }

          case NotifyCollectionChangedAction.Remove:
            {
              deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.Remove, e.OldStartingIndex, e.OldItems );
              break;
            }

          case NotifyCollectionChangedAction.Replace:
            {
              deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.Replace, -1, e.NewStartingIndex, e.NewItems, e.OldStartingIndex, e.OldItems );
              break;
            }

          case NotifyCollectionChangedAction.Reset:
            {
              deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.Refresh, -1, null );
              break;
            }

          default:
            throw new NotSupportedException( e.Action.ToString() + " is not a supported action." );
        }

        if( deferredOperation != null )
        {
          this.ExecuteOrQueueSourceItemOperation( deferredOperation );
        }
      }
    }

    private void OnItemsSourceListChanged( object sender, ListChangedEventArgs e )
    {
      Debug.Assert( !( this is DataGridVirtualizingCollectionViewBase ),
        "The DataGridVirtualizingCollectionView is unbound and therefore should never receive a notification from a source." );

      Action<PropertyDescriptor> propertyDescriptorHandler;
      ListChangedType listChangedType = e.ListChangedType;

      switch( listChangedType )
      {
        case ListChangedType.PropertyDescriptorAdded:
          propertyDescriptorHandler = this.HandlePropertyDescriptorAdded;
          break;

        case ListChangedType.PropertyDescriptorChanged:
          propertyDescriptorHandler = this.HandlePropertyDescriptorChanged;
          break;

        case ListChangedType.PropertyDescriptorDeleted:
          propertyDescriptorHandler = this.HandlePropertyDescriptorDeleted;
          break;

        default:
          propertyDescriptorHandler = null;
          break;
      }

      if( propertyDescriptorHandler != null )
      {
        if( this.CheckAccess() )
        {
          propertyDescriptorHandler.Invoke( e.PropertyDescriptor );
        }
        else
        {
          this.Dispatcher.Invoke( new Action<PropertyDescriptor>( propertyDescriptorHandler ), e.PropertyDescriptor );
        }
      }
      else
      {
        IBindingList bindingList = ( IBindingList )m_enumeration;
        int index = e.NewIndex;
        IList items = null;

        if( ( listChangedType != ListChangedType.Reset ) && ( listChangedType != ListChangedType.ItemDeleted ) )
        {
          var indexInBound = ( index >= 0 ) && ( index < bindingList.Count );

          //If we're within bounds
          if( indexInBound )
          {
            //Let's get the item
            items = new object[] { bindingList[ index ] };
          }
          else
          {
            //Let's reset the list because a change to the list has happenned (e.g. a delete) before we had a chance to process the current change.
            listChangedType = ListChangedType.Reset;
          }
        }

        lock( m_deferredOperationManager )
        {
          if( m_deferredOperationManager.RefreshPending )
            return;

          DeferredOperation deferredOperation;

          switch( listChangedType )
          {
            case ListChangedType.ItemAdded:
              {
                deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.Add, bindingList.Count, index, items );
                break;
              }

            case ListChangedType.ItemChanged:
              {
                var collectionView = this as DataGridCollectionView;
                deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.Replace, -1, index, items, index, items );
                break;
              }

            case ListChangedType.ItemDeleted:
              {
                deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.Remove, index, new object[ 1 ] );
                break;
              }

            case ListChangedType.ItemMoved:
              {
                deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.Move, -1, index, items, e.OldIndex, items );
                break;
              }

            case ListChangedType.Reset:
              {
                deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.Refresh, -1, null );
                break;
              }

            default:
              throw new NotSupportedException( "This ListChangedType (" + listChangedType.ToString() + ") is not supported." );
          }

          Debug.Assert( deferredOperation != null );

          this.ExecuteOrQueueSourceItemOperation( deferredOperation );
        }
      }
    }

    private void HandlePropertyDescriptorAdded( PropertyDescriptor property )
    {
      this.ClearDefaultPropertyDescriptions();
      this.SetupDefaultPropertyDescriptions();

      if( !this.AutoCreateItemProperties )
        return;

      if( m_itemProperties[ property.Name ] != null )
        return;

      var propertyRoute = PropertyRouteParser.Parse( property.Name );
      if( propertyRoute == null )
        return;

      ItemsSourceHelper.CreateAndAddItemPropertyForPropertyDescription( m_itemProperties, m_defaultPropertyDescriptions[ propertyRoute ] );
      ItemsSourceHelper.AutoDetectSynonyms( this, DataGridItemPropertyRoute.Create( ItemsSourceHelper.GetItemPropertyFromProperty( m_itemProperties, propertyRoute ) ) );
    }

    private void HandlePropertyDescriptorChanged( PropertyDescriptor property )
    {
      this.ClearDefaultPropertyDescriptions();
      this.SetupDefaultPropertyDescriptions();

      if( !this.AutoCreateItemProperties )
        return;

      if( property == null )
      {
        using( m_itemProperties.DeferCollectionChanged() )
        {
          for( int i = m_itemProperties.Count - 1; i >= 0; i-- )
          {
            var oldItemProperty = m_itemProperties[ i ] as DataGridItemProperty;
            if( ( oldItemProperty != null ) && ( oldItemProperty.IsAutoCreated ) )
            {
              m_itemProperties.RemoveAt( i );
            }
          }
        }

        ItemsSourceHelper.CreateAndAddItemPropertiesForPropertyDescriptions( m_itemProperties, m_defaultPropertyDescriptions.Values );
        ItemsSourceHelper.AutoDetectSynonyms( this );
      }
      else
      {
        var oldItemProperty = m_itemProperties[ property.Name ] as DataGridItemProperty;

        if( ( oldItemProperty != null ) && ( oldItemProperty.IsAutoCreated ) )
        {
          // It is better to completely remove the item property and add a new one instead of changing is content.
          m_itemProperties.Remove( oldItemProperty );

          if( m_itemProperties[ property.Name ] != null )
            return;

          var propertyRoute = PropertyRouteParser.Parse( property.Name );
          if( propertyRoute == null )
            return;

          ItemsSourceHelper.CreateAndAddItemPropertyForPropertyDescription( m_itemProperties, m_defaultPropertyDescriptions[ propertyRoute ] );
          ItemsSourceHelper.AutoDetectSynonyms( this, DataGridItemPropertyRoute.Create( ItemsSourceHelper.GetItemPropertyFromProperty( m_itemProperties, propertyRoute ) ) );
        }
      }
    }

    private void HandlePropertyDescriptorDeleted( PropertyDescriptor property )
    {
      this.ClearDefaultPropertyDescriptions();
      this.SetupDefaultPropertyDescriptions();

      if( this.AutoCreateItemProperties )
      {
        var oldItemProperty = m_itemProperties[ property.Name ] as DataGridItemProperty;

        if( ( oldItemProperty != null ) && ( oldItemProperty.IsAutoCreated ) )
        {
          m_itemProperties.Remove( oldItemProperty );
        }
      }
    }

    #endregion

    #region CHILD COLLECTION CHANGED HANDLERS

    // This method is also called for sub properties.
    private void OnItemPropertiesCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      var addedItems = default( IEnumerable<DataGridItemPropertyBase> );
      var removedItems = default( IEnumerable<DataGridItemPropertyBase> );

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Add:
          addedItems = e.NewItems.Cast<DataGridItemPropertyBase>();
          break;

        case NotifyCollectionChangedAction.Remove:
          removedItems = e.OldItems.Cast<DataGridItemPropertyBase>();
          break;

        case NotifyCollectionChangedAction.Replace:
          addedItems = e.NewItems.Cast<DataGridItemPropertyBase>();
          removedItems = e.OldItems.Cast<DataGridItemPropertyBase>();
          break;

        case NotifyCollectionChangedAction.Reset:
          throw new NotSupportedException();
      }

      if( ( addedItems == null ) && ( removedItems == null ) )
        return;

      var deferRefresh = ( this.Loaded ) ? this.DeferRefresh() : null;

      try
      {
        if( removedItems != null )
        {
          foreach( var itemProperty in removedItems )
          {
            this.ClearItemProperty( itemProperty );
          }
        }

        if( addedItems != null )
        {
          foreach( var itemProperty in addedItems )
          {
            this.PrepareItemProperty( itemProperty );
          }
        }
      }
      finally
      {
        lock( this.SyncRoot )
        {
          lock( this.DeferredOperationManager )
          {
            //This is required as the GroupSortStatResultPropertyName property affects this collection, and thus must be updated when there is a change to the ItemsProperties.
            this.ClearGroupSortComparers();
          }
        }

        if( deferRefresh != null )
        {
          deferRefresh.Dispose();
        }
      }
    }

    private void OnItemPropertyPropertyChanged( DataGridItemPropertyBase itemProperty, PropertyChangedEventArgs e )
    {
      if( itemProperty == null )
        return;

      var propertyName = e.PropertyName;

      if( string.IsNullOrEmpty( propertyName ) || ( propertyName == DataGridItemPropertyBase.ItemPropertiesInternalPropertyName ) )
      {
        var itemProperties = itemProperty.ItemPropertiesInternal;
        if( itemProperties != null )
        {
          this.ClearItemProperties( itemProperties );
          this.PrepareItemProperties( itemProperties );
        }
      }
    }

    private void OnSortDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      lock( this.SyncRoot )
      {
        lock( this.DeferredOperationManager )
        {
          this.ClearGroupSortComparers();

          this.OnProxySortDescriptionsChanged( this, e );

          if( this.DataGridSortDescriptions.IsResortDefered )
          {
            this.DataGridSortDescriptions.AddResortNotification( this.DeferRefresh() );
          }

          this.ExecuteOrQueueSourceItemOperation( new DeferredOperation( DeferredOperation.DeferredOperationAction.Resort, -1, null ) );
        }
      }
    }

    private void OnGroupDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      lock( this.SyncRoot )
      {
        lock( this.DeferredOperationManager )
        {
          this.ClearGroupSortComparers();

          this.OnProxyGroupDescriptionsChanged( this, e );

          this.ExecuteOrQueueSourceItemOperation( new DeferredOperation( DeferredOperation.DeferredOperationAction.Regroup, -1, null ) );
        }
      }
    }

    #endregion

    #region PROXY EVENTS

    internal DataGridCollectionViewBase RootDataGridCollectionViewBase
    {
      get
      {
        return m_rootDataGridCollectionViewBase;
      }
    }

    internal event EventHandler ProxyCollectionRefresh;

    internal void OnProxyCollectionRefresh()
    {
      if( m_rootDataGridCollectionViewBase.ProxyCollectionRefresh != null )
      {
        m_rootDataGridCollectionViewBase.ProxyCollectionRefresh( this, EventArgs.Empty );
      }
    }

    internal event NotifyCollectionChangedEventHandler ProxySortDescriptionsChanged;

    internal void OnProxySortDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( m_rootDataGridCollectionViewBase.ProxySortDescriptionsChanged != null )
      {
        m_rootDataGridCollectionViewBase.ProxySortDescriptionsChanged( sender, e );
      }
    }

    internal event NotifyCollectionChangedEventHandler ProxyGroupDescriptionsChanged;

    internal void OnProxyGroupDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( m_rootDataGridCollectionViewBase.ProxyGroupDescriptionsChanged != null )
      {
        m_rootDataGridCollectionViewBase.ProxyGroupDescriptionsChanged( sender, e );
      }
    }

    #endregion

    #region EDIT PROCESS

    internal void SetCurrentEditItem( object item )
    {
      m_currentEditItem = item;
    }

    private void EditItemInternal( object item, out bool beginEditCalled )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      beginEditCalled = false;
      IEditableObject editableObject = ItemsSourceHelper.GetEditableObject( item );

      if( editableObject != null )
      {
        // editableObject can be an Xceed DataRow when the datarow is directly inserted in us.
        // In that case we do not call BeginEdit since we already being called from it.
        if( !( editableObject is DataRow ) )
        {
          editableObject.BeginEdit();
          beginEditCalled = true;
        }
      }
    }

    private void EndEditInternal( object item, out bool endEditCalled )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      endEditCalled = false;
      var editableObject = ItemsSourceHelper.GetEditableObject( item );

      // editableObject can be an Xceed datarow when directly inserted as Items in the DataGridControl.
      if( ( editableObject != null ) && ( !( editableObject is Xceed.Wpf.DataGrid.DataRow ) ) )
      {
        // Keep a copy of the values, because if EndEdit throw ( for a DataView ), we loose the old values.
        var itemProperties = this.ItemProperties;
        var oldValueBeforeEndEdit = new Dictionary<DataGridItemPropertyBase, object>( itemProperties.Count );

        foreach( var itemProperty in itemProperties )
        {
          oldValueBeforeEndEdit[ itemProperty ] = ItemsSourceHelper.GetValueFromItemProperty( itemProperty, item );
        }

        try
        {
          editableObject.EndEdit();
          endEditCalled = true;
        }
        finally
        {
          if( !endEditCalled )
          {
            // Since the DataView of MS cancel the edition when EndEdit is throwing, we ensure to restart the editing mode.
            editableObject.BeginEdit();

            itemProperties.SuspendUnboundItemPropertyChanged( item );

            try
            {
              // restore the oldvalues
              foreach( var itemProperty in itemProperties )
              {
                var readOnly = ( ( item == m_currentAddItem ) && itemProperty.OverrideReadOnlyForInsertion.HasValue && itemProperty.OverrideReadOnlyForInsertion.Value )
                                 ? false
                                 : itemProperty.IsReadOnly;

                if( !readOnly )
                {
                  ItemsSourceHelper.SetValueForItemProperty( itemProperty, item, oldValueBeforeEndEdit[ itemProperty ] );
                }
              }
            }
            finally
            {
              itemProperties.ResumeUnboundItemPropertyChanged( item );
            }
          }
        }
      }
    }

    private void CancelEditInternal( object item, out bool cancelEditCalled )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      cancelEditCalled = false;
      IEditableObject editableObject = ItemsSourceHelper.GetEditableObject( item );

      // editableObject can be an Xceed datarow when directly inserted as Items in the DataGridControl.
      if( ( editableObject != null ) && ( !( editableObject is Xceed.Wpf.DataGrid.DataRow ) ) )
      {
        editableObject.CancelEdit();
        cancelEditCalled = true;
      }
    }


    public event EventHandler<DataGridItemCancelEventArgs> BeginningEdit;

    internal virtual void OnBeginningEdit( DataGridItemCancelEventArgs e )
    {
      // We throw instead of setting e.Cancel to True because we do not want to give the developer the chance to set it back to False.
      if( e.Item is EmptyDataItem )
        throw new DataGridException( "Cannot begin edit on an empty data item." );

      if( this.BeginningEdit != null )
        this.BeginningEdit( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnBeginningEdit( e );
    }

    public event EventHandler<DataGridItemEventArgs> EditBegun;

    internal virtual void OnEditBegun( DataGridItemEventArgs e )
    {
      if( this.EditBegun != null )
        this.EditBegun( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnEditBegun( e );
    }

    public event EventHandler<DataGridItemHandledEventArgs> CancelingEdit;

    internal void OnCancelingEdit( DataGridItemHandledEventArgs e )
    {
      if( this.CancelingEdit != null )
        this.CancelingEdit( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnCancelingEdit( e );
    }

    public event EventHandler<DataGridItemEventArgs> EditCanceled;

    internal virtual void OnEditCanceled( DataGridItemEventArgs e )
    {
      if( this.EditCanceled != null )
      {
        this.EditCanceled( this, e );
      }

      if( m_parentCollectionViewSourceBase != null )
      {
        m_parentCollectionViewSourceBase.OnEditCanceled( e );
      }
    }

    public event EventHandler<DataGridItemCancelEventArgs> CommittingEdit;

    internal void OnCommittingEdit( DataGridItemCancelEventArgs e )
    {
      if( this.CommittingEdit != null )
        this.CommittingEdit( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnCommittingEdit( e );
    }

    public event EventHandler<DataGridItemEventArgs> EditCommitted;

    internal virtual void OnEditCommitted( DataGridItemEventArgs e )
    {
      if( this.EditCommitted != null )
        this.EditCommitted( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnEditCommitted( e );
    }

    #endregion

    #region INSERTION PROCESS

    internal void SetCurrentAddNew( object newItem, int position )
    {
      m_currentAddItem = newItem;
      m_currentAddItemPosition = position;
    }

    public event EventHandler<DataGridItemEventArgs> InitializingNewItem;

    internal void OnInitializingNewItem( DataGridItemEventArgs e )
    {
      if( this.InitializingNewItem != null )
        this.InitializingNewItem( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnInitializingNewItem( e );
    }

    public event EventHandler<DataGridCreatingNewItemEventArgs> CreatingNewItem;

    internal void OnCreatingNewItem( DataGridCreatingNewItemEventArgs e )
    {
      if( this.CreatingNewItem != null )
        this.CreatingNewItem( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnCreatingNewItem( e );
    }

    public event EventHandler<DataGridCommittingNewItemEventArgs> CommittingNewItem;

    internal void OnCommittingNewItem( DataGridCommittingNewItemEventArgs e )
    {
      if( this.CommittingNewItem != null )
        this.CommittingNewItem( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnCommittingNewItem( e );
    }

    public event EventHandler<DataGridItemHandledEventArgs> CancelingNewItem;

    internal void OnCancelingNewItem( DataGridItemHandledEventArgs e )
    {
      if( this.CancelingNewItem != null )
        this.CancelingNewItem( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnCancelingNewItem( e );
    }

    public event EventHandler<DataGridItemEventArgs> NewItemCreated;

    internal void OnNewItemCreated( DataGridItemEventArgs e )
    {
      if( this.NewItemCreated != null )
        this.NewItemCreated( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnNewItemCreated( e );
    }

    public event EventHandler<DataGridItemEventArgs> NewItemCommitted;

    internal void OnNewItemCommitted( DataGridItemEventArgs e )
    {
      if( this.NewItemCommitted != null )
        this.NewItemCommitted( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnNewItemCommitted( e );
    }

    public event EventHandler<DataGridItemEventArgs> NewItemCanceled;

    internal void OnNewItemCanceled( DataGridItemEventArgs e )
    {
      if( this.NewItemCanceled != null )
        this.NewItemCanceled( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnNewItemCanceled( e );
    }

    #endregion

    #region DELETION PROCESS

    internal void InternalRemoveAt( int sourceIndex )
    {
      IList list = m_enumeration as IList;

      if( list != null )
      {
        list.RemoveAt( sourceIndex );
        return;
      }

      throw new InvalidOperationException( "An attempt was made to remove an item from the source collection." );
    }

    public event EventHandler<DataGridRemovingItemEventArgs> RemovingItem;

    internal void OnRemovingItem( DataGridRemovingItemEventArgs e )
    {
      if( this.RemovingItem != null )
        this.RemovingItem( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnRemovingItem( e );
    }

    public event EventHandler<DataGridItemRemovedEventArgs> ItemRemoved;

    internal void OnItemRemoved( DataGridItemRemovedEventArgs e )
    {
      if( this.ItemRemoved != null )
        this.ItemRemoved( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnItemRemoved( e );
    }

    #endregion

    #region CURRENCY MANAGEMENT

    internal IDisposable DeferCurrencyEvent()
    {
      return new DataGridCollectionViewBase.DeferCurrencyEventHelper( this );
    }

    internal bool IsCurrencyDeferred
    {
      get
      {
        return m_deferCurrencyEventCount > 0;
      }
    }

    public override object CurrentItem
    {
      get
      {
        return m_currentItem;
      }
    }

    public override int CurrentPosition
    {
      get
      {
        return m_currentPosition;
      }
    }

    public override bool IsCurrentAfterLast
    {
      get
      {
        return m_flags[ ( int )DataGridCollectionViewBaseFlags.IsCurrentAfterLast ];
      }
    }

    public override bool IsCurrentBeforeFirst
    {
      get
      {
        return m_flags[ ( int )DataGridCollectionViewBaseFlags.IsCurrentBeforeFirst ];
      }
    }

    public override bool MoveCurrentTo( object item )
    {
      // This is done in DataGridCollectionView's IndexOf and MoveCurrentToPosition :
      // this.EnsureThreadAndCollectionLoaded();

      int index = this.IndexOf( item );
      return this.MoveCurrentToPosition( index );
    }

    public override bool MoveCurrentToFirst()
    {
      // This is done in DataGridCollectionView's MoveCurrentToPosition :
      // this.EnsureThreadAndCollectionLoaded();

      return this.MoveCurrentToPosition( 0 );
    }

    public override bool MoveCurrentToLast()
    {
      // This is done in DataGridCollectionView's MoveCurrentToPosition :
      // this.EnsureThreadAndCollectionLoaded();

      return this.MoveCurrentToPosition( this.Count - 1 );
    }

    public override bool MoveCurrentToNext()
    {
      // This is done in DataGridCollectionView's MoveCurrentToPosition :
      // this.EnsureThreadAndCollectionLoaded();

      if( this.CurrentPosition < this.Count )
        return this.MoveCurrentToPosition( this.CurrentPosition + 1 );

      return false;
    }

    public override bool MoveCurrentToPrevious()
    {
      // This is done in DataGridCollectionView's MoveCurrentToPosition :
      // this.EnsureThreadAndCollectionLoaded();

      if( this.CurrentPosition > -1 )
        return this.MoveCurrentToPosition( this.CurrentPosition - 1 );

      return false;
    }

    internal virtual void SetCurrentItemAndPositionCore(
      object currentItem,
      int currentPosition,
      bool isCurrentBeforeFirst,
      bool isCurrentAfterLast )
    {
      m_currentItem = currentItem;
      m_currentPosition = currentPosition;

      m_flags[ ( int )DataGridCollectionViewBaseFlags.IsCurrentBeforeFirst ] = isCurrentBeforeFirst;
      m_flags[ ( int )DataGridCollectionViewBaseFlags.IsCurrentAfterLast ] = isCurrentAfterLast;
    }

    #endregion

    #region DEFERRED OPERATIONS HANDLING

    internal event EventHandler PreBatchCollectionChanged;
    internal event EventHandler PostBatchCollectionChanged;

    internal void RaisePreBatchCollectionChanged()
    {
      var handler = this.PreBatchCollectionChanged;
      if( handler == null )
        return;

      handler.Invoke( this, EventArgs.Empty );
    }

    internal void RaisePostBatchCollectionChanged()
    {
      var handler = this.PostBatchCollectionChanged;
      if( handler == null )
        return;

      handler.Invoke( this, EventArgs.Empty );
    }

    internal void EnsureThread()
    {
      if( !this.CheckAccess() )
        throw new InvalidOperationException( "An attempt was made to execute an operation on a thread other than the dispatcher thread." );
    }

    internal void EnsureThreadAndCollectionLoaded()
    {
      this.EnsureThread();

      if( !this.Loaded )
      {
        this.ForceRefresh( false, true, true );
        Debug.Assert( this.Loaded );
      }
    }

    private bool QueueOperationForAddNew
    {
      get
      {
        return m_flags[ ( int )DataGridCollectionViewBaseFlags.QueueOperationForAddNew ];
      }
      set
      {
        m_flags[ ( int )DataGridCollectionViewBaseFlags.QueueOperationForAddNew ] = value;
      }
    }

    internal bool ShouldDeferOperation
    {
      get
      {
        if( this.InDeferRefresh || !this.CheckAccess() )
          return true;

        lock( m_deferredOperationManager )
        {
          return ( !this.Loaded ) || ( m_deferredOperationManager.HasPendingOperations );
        }
      }
    }

    private bool MustQueueOperationForPendingAddNew( int index, IList oldItems, IList newItems )
    {
      if( this.QueueOperationForAddNew )
        return true;

      if( m_currentAddItem == null )
        return false;

      if( oldItems != null )
      {
        if( ( oldItems.Count == 1 ) && ( oldItems[ 0 ] == m_currentAddItem ) )
          return true;
      }

      if( newItems != null )
      {
        if( ( newItems.Count == 1 ) && ( newItems[ 0 ] == m_currentAddItem ) )
          return true;
      }

      if( ( oldItems == null ) && ( newItems == null ) )
      {
        if( index == -1 )
          return false;

        if( m_currentAddItemPosition == index )
          return true;
      }

      return false;
    }

    internal void ExecuteOrQueueSourceItemOperation( DeferredOperation deferredOperation )
    {
      bool queueOperationForPendingAddNew = this.MustQueueOperationForPendingAddNew( deferredOperation.OldStartingIndex, deferredOperation.OldItems, deferredOperation.NewItems );

      if( queueOperationForPendingAddNew )
      {
        this.AddDeferredOperationForAddNew( deferredOperation );
      }
      else if( this.ShouldDeferOperation )
      {
        this.AddDeferredOperation( deferredOperation );
      }
      else
      {
        bool refreshForced;
        this.ExecuteSourceItemOperation( deferredOperation, out refreshForced );
      }
    }

    internal virtual void ExecuteSourceItemOperation( DeferredOperation deferredOperation, out bool refreshForced )
    {
      throw new NotSupportedException( deferredOperation.Action.ToString() + " is not a supported action." );
    }

    internal void AddDeferredOperation( DeferredOperation deferredOperation )
    {
      m_deferredOperationManager.Add( deferredOperation );
    }

    private void AddDeferredOperationForAddNew( DeferredOperation deferredOperation )
    {
      if( m_deferredAddNewOperationManager == null )
        m_deferredAddNewOperationManager = new DeferredOperationManager( this, null, false );

      m_deferredAddNewOperationManager.Add( deferredOperation );
    }

    #endregion

    protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs args )
    {

      if( ( args != null )
        && ( ( ( args.NewItems != null ) && ( args.NewItems.Count > 1 ) )
          || ( ( args.OldItems != null ) && ( args.OldItems.Count > 1 ) ) ) )
      {
        args = new NotifyRangeCollectionChangedEventArgs( args );
      }

      base.OnCollectionChanged( args );
    }

    internal virtual void ProcessInvalidatedGroupStats( HashSet<DataGridCollectionViewGroup> invalidatedGroups, bool resortGroups )
    {
    }

    internal virtual void ClearGroupSortComparers()
    {
    }

    internal virtual bool CanCreateItemProperties()
    {
      return ( ( m_enumeration != null ) || ( m_itemType != typeof( object ) ) || this.CanCreateItemPropertiesFromModelSource );
    }

    internal virtual void CreateDefaultCollections( DataGridDetailDescription parentDetailDescription )
    {
      Debug.Assert( parentDetailDescription == null );

      m_sortDescriptions = ( parentDetailDescription == null ) ? new DataGridSortDescriptionCollection() : parentDetailDescription.SortDescriptions as DataGridSortDescriptionCollection;
      m_groupDescriptions = ( parentDetailDescription == null ) ? new GroupDescriptionCollection() : parentDetailDescription.GroupDescriptions;
    }

    internal bool IsSourceSupportingChangeNotification
    {
      get
      {
        return ItemsSourceHelper.IsSourceSupportingChangeNotification( m_enumeration );
      }
    }

    internal void PrepareRootContextForDeferRefresh( DataGridContext dataGridContext )
    {
      this.DataGridContext = dataGridContext;
      m_detailDeferRefreshes = new List<List<IDisposable>>();
    }

    internal void Dispose()
    {
      if( this.ParentCollectionViewSourceBase != null )
      {
        this.ParentCollectionViewSourceBase = null;

        this.ClearDefaultPropertyDescriptions();
        this.ClearItemProperties();
      }

      this.ClearItemProperties( m_itemProperties );
      this.UnregisterChangedEvents();
    }

    private void SetupDefaultDetailDescriptions()
    {
      if( ( m_parentDetailDescription != null ) && m_parentDetailDescription.AutoCreateDetailDescriptionsCompleted )
        return;

      if( this.AutoCreateDetailDescriptions)
      {
        var defaultDetailDescriptions = ItemsSourceHelper.CreateDetailDescriptions( m_itemType, m_enumeration );
        if( defaultDetailDescriptions != null )
        {
          foreach( var detailDescription in defaultDetailDescriptions )
          {
            if( m_detailDescriptions[ detailDescription.RelationName ] != null )
              continue;

            if( detailDescription.IsAutoCreated )
            {
              // Propagate the AutoCreateForeignKeyDescriptions values to the auto created DataGridDetailDescription.
              detailDescription.AutoCreateForeignKeyDescriptions = this.AutoCreateForeignKeyDescriptions;
            }

            m_detailDescriptions.Add( detailDescription );
          }
        }
      }

      if( m_parentDetailDescription != null )
      {
        m_parentDetailDescription.AutoCreateDetailDescriptionsCompleted = true;
      }
    }

    private void SetupCurrent( bool setToFirstItem )
    {
      Debug.Assert( !this.Loaded );

      var enumerator = ( m_enumeration != null ) ? m_enumeration.GetEnumerator() : null;

      try
      {
        if( ( enumerator != null ) && enumerator.MoveNext() )
        {
          if( setToFirstItem )
          {
            this.SetCurrentItemAndPositionCore( enumerator.Current, 0, false, false );
          }
          else
          {
            this.SetCurrentItemAndPositionCore( null, -1, true, false );
          }
        }
        else
        {
          this.SetCurrentItemAndPositionCore( null, -1, true, true );
        }
      }
      finally
      {
        var disposable = enumerator as IDisposable;
        if( disposable != null )
        {
          disposable.Dispose();
        }
      }
    }

    private void RegisterChangedEvents()
    {
      this.RegisterSourceChanged( m_enumeration );

      CollectionChangedEventManager.AddListener( m_sortDescriptions, this );
      CollectionChangedEventManager.AddListener( m_groupDescriptions, this );

      ItemPropertyGroupSortStatNameChangedEventManager.AddListener( m_itemProperties, this );

      if( m_parentDetailDescription == null )
      {
        InitializeItemPropertyEventManager.AddListener( m_itemProperties, this );
        CollectionChangedEventManager.AddListener( m_detailDescriptions, this );
      }
    }

    private void UnregisterChangedEvents()
    {
      this.UnregisterSourceChanged( m_enumeration );

      CollectionChangedEventManager.RemoveListener( m_sortDescriptions, this );
      CollectionChangedEventManager.RemoveListener( m_groupDescriptions, this );

      ItemPropertyGroupSortStatNameChangedEventManager.RemoveListener( m_itemProperties, this );

      if( m_parentDetailDescription == null )
      {
        InitializeItemPropertyEventManager.RemoveListener( m_itemProperties, this );
        CollectionChangedEventManager.RemoveListener( m_detailDescriptions, this );
      }
    }

    private void RegisterSourceChanged( IEnumerable source )
    {
      if( source == null )
        return;

      var collectionChanged = source as INotifyCollectionChanged;
      var bindingList = source as IBindingList;

      if( collectionChanged != null )
      {
        CollectionChangedEventManager.AddListener( collectionChanged, this );
      }
      else if( ( bindingList != null ) && bindingList.SupportsChangeNotification )
      {
        ListChangedEventManager.AddListener( bindingList, this );
      }
    }

    private void UnregisterSourceChanged( IEnumerable source )
    {
      if( source == null )
        return;

      var collectionChanged = source as INotifyCollectionChanged;
      if( collectionChanged != null )
      {
        CollectionChangedEventManager.RemoveListener( collectionChanged, this );
      }
      var bindingList = source as IBindingList;
      if( ( bindingList != null ) && bindingList.SupportsChangeNotification )
      {
        ListChangedEventManager.RemoveListener( bindingList, this );
      }
    }

    private void RegisterItemProperties( DataGridItemPropertyCollection itemProperties )
    {
      if( itemProperties == null )
        return;

      CollectionChangedEventManager.AddListener( itemProperties, this );
    }

    private void UnregisterItemProperties( DataGridItemPropertyCollection itemProperties )
    {
      if( itemProperties == null )
        return;

      CollectionChangedEventManager.RemoveListener( itemProperties, this );
    }

    private void RegisterItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return;

      PropertyChangedEventManager.AddListener( itemProperty, this, DataGridItemPropertyBase.ItemPropertiesInternalPropertyName );
    }

    private void UnregisterItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return;

      PropertyChangedEventManager.RemoveListener( itemProperty, this, DataGridItemPropertyBase.ItemPropertiesInternalPropertyName );
    }

    private void PrepareItemProperties( DataGridItemPropertyCollection itemProperties )
    {
      if( itemProperties == null )
        return;

      foreach( var itemProperty in itemProperties )
      {
        this.PrepareItemProperty( itemProperty );
      }

      this.RegisterItemProperties( itemProperties );
    }

    private void ClearItemProperties( DataGridItemPropertyCollection itemProperties )
    {
      if( itemProperties == null )
        return;

      foreach( var itemProperty in itemProperties )
      {
        this.ClearItemProperty( itemProperty );
      }

      this.UnregisterItemProperties( itemProperties );
    }

    private void PrepareItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return;

      // Set default value for CalculateDistinctValues if not explicitly set.
      if( !itemProperty.IsCalculateDistinctValuesInitialized && ( m_parentDetailDescription != null ) )
      {
        itemProperty.CalculateDistinctValues = m_parentDetailDescription.DefaultCalculateDistinctValues;
      }

      var propertyPath = PropertyRouteParser.Parse( itemProperty );

      this.RegisterItemProperty( itemProperty );
      this.PrepareItemProperties( itemProperty.ItemPropertiesInternal );
    }

    private void ClearItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return;

      this.ClearItemProperties( itemProperty.ItemPropertiesInternal );
      this.UnregisterItemProperty( itemProperty );

      var propertyPath = PropertyRouteParser.Parse( itemProperty );
    }

    private void SetupDefaultPropertyDescriptions()
    {
      if( this.DefaultPropertyDescriptionsCreated )
        return;

      var modelDataTable = ( m_enumeration as DataTable ) ?? ( m_modelSource as DataTable );

      if( this.CanCreateItemProperties() )
      {
        this.DefaultItemPropertiesInitialized = false;
        this.DefaultPropertyDescriptionsCreated = true;

        ItemsSourceHelper.SetPropertyDescriptions( m_defaultPropertyDescriptions, modelDataTable, m_enumeration, m_itemType, true );
      }

      if( !this.DefaultItemPropertiesInitialized )
      {
        this.DefaultItemPropertiesInitialized = true;

        // Initialize the item properties according to the default property descriptions.
        foreach( var itemProperty in m_itemProperties )
        {
          var itemPropertyRoute = DataGridItemPropertyRoute.Create( itemProperty );

          ItemsSourceHelper.SetPropertyDescriptionsFromItemProperty( m_defaultPropertyDescriptions, modelDataTable, m_enumeration, m_itemType, itemPropertyRoute );
          ItemsSourceHelper.InitializePropertyDescriptions( m_defaultPropertyDescriptions, itemPropertyRoute, m_itemType, this.DefaultPropertyDescriptionsCreated );
        }
      }
    }

    private void ClearDefaultPropertyDescriptions()
    {
      this.DefaultItemPropertiesInitialized = false;
      this.DefaultPropertyDescriptionsCreated = false;

      m_defaultPropertyDescriptions.Clear();
    }

    private void ClearItemProperties()
    {
      m_itemProperties.Clear();
    }

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        var eventArgs = ( NotifyCollectionChangedEventArgs )e;

        if( sender == this.SortDescriptions )
        {
          this.OnSortDescriptionsChanged( sender, eventArgs );
        }
        else if( sender == this.GroupDescriptions )
        {
          this.OnGroupDescriptionsChanged( sender, eventArgs );
        }
        else if( sender == this.Enumeration )
        {
          this.OnItemsSourceCollectionChanged( sender, eventArgs );
        }
        else if( sender is DataGridItemPropertyCollection )
        {
          this.OnItemPropertiesCollectionChanged( sender, eventArgs );
        }
      }
      else if( managerType == typeof( PropertyChangedEventManager ) )
      {
        var eventArgs = ( PropertyChangedEventArgs )e;
        var itemProperty = sender as DataGridItemPropertyBase;

        if( itemProperty != null )
        {
          this.OnItemPropertyPropertyChanged( itemProperty, eventArgs );
        }
      }
      else if( managerType == typeof( ListChangedEventManager ) )
      {
        if( sender == this.Enumeration )
        {
          this.OnItemsSourceListChanged( sender, ( ListChangedEventArgs )e );
        }
      }
      else if( managerType == typeof( ItemPropertyGroupSortStatNameChangedEventManager ) )
      {
        if( sender == this.ItemProperties )
        {
          this.ClearGroupSortComparers();
        }
      }
      else if( managerType == typeof( InitializeItemPropertyEventManager ) )
      {
        var eventArgs = ( InitializeItemPropertyEventArgs )e;

        if( sender == this.ItemProperties )
        {
          var itemProperty = eventArgs.ItemProperty;
          var itemPropertyRoute = DataGridItemPropertyRoute.Create( itemProperty );
          var modelDataTable = ( m_enumeration as DataTable ) ?? ( m_modelSource as DataTable );

          ItemsSourceHelper.SetPropertyDescriptionsFromItemProperty( m_defaultPropertyDescriptions, modelDataTable, m_enumeration, m_itemType, itemPropertyRoute );
          ItemsSourceHelper.InitializePropertyDescriptions( m_defaultPropertyDescriptions, itemPropertyRoute, m_itemType, this.DefaultPropertyDescriptionsCreated );
          ItemsSourceHelper.AutoDetectSynonyms( this, itemPropertyRoute );
        }
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    #region ICollectionView Members

    public override IDisposable DeferRefresh()
    {
      this.EnsureThread();

      var dataGridContext = this.DataGridContext;

      if( dataGridContext != null )
      {
        //Make sure detail DataGridCollectionView's are also deferred.
        List<IDisposable> detailDisposables = new List<IDisposable>();
        foreach( DataGridContext detailContext in dataGridContext.GetChildContextsCore() )
        {
          detailDisposables.Add( detailContext.Items.DeferRefresh() );
        }

        m_detailDeferRefreshes.Add( detailDisposables );
      }

      return new DataGridCollectionView.DeferRefreshHelper( this );
    }

    public override void Refresh()
    {
      this.EnsureThread();

      var dataGridContext = this.DataGridContext;

      if( this.InDeferRefresh )
      {
        if( dataGridContext != null )
        {
          //Make sure to queue a Refresh on all detail DataGridCollectionView's.
          foreach( DataGridContext detailContext in dataGridContext.GetChildContextsCore() )
          {
            detailContext.Items.Refresh();
          }
        }

        this.AddDeferredOperation( new DeferredOperation( DeferredOperation.DeferredOperationAction.Refresh, -1, null ) );
      }
      else
      {
        if( dataGridContext != null )
        {
          //Make sure all detail DataGridCollectionView's are refreshed.
          foreach( DataGridContext detailContext in dataGridContext.GetChildContextsCore() )
          {
            detailContext.Items.Refresh();
          }
        }

        this.OnProxyCollectionRefresh();
        this.ForceRefresh( true, !this.Loaded, true );
      }
    }

    public override SortDescriptionCollection SortDescriptions
    {
      get
      {
        return m_sortDescriptions;
      }
    }

    internal DataGridSortDescriptionCollection DataGridSortDescriptions
    {
      get
      {
        return m_sortDescriptions;
      }
    }

    public override ObservableCollection<GroupDescription> GroupDescriptions
    {
      get
      {
        return m_groupDescriptions;
      }
    }

    public override ReadOnlyObservableCollection<object> Groups
    {
      get
      {
        if( m_groupDescriptions.Count > 0 )
        {
          this.EnsureThreadAndCollectionLoaded();

          IList<object> rootGroups = m_rootGroup.GetItems();

          Debug.Assert( rootGroups is ReadOnlyObservableCollection<object>,
            "The CollectionViewGroup GetItems extensibility method should always return a ReadOnlyObservableCollection<object> when " +
            "dealing with a DataGridVirtualizingCollectionViewGroupRoot which has subgroups." );

          return ( ReadOnlyObservableCollection<object> )rootGroups;
        }

        return null;
      }
    }

    internal CollectionViewGroup RootGroup
    {
      get
      {
        return m_rootGroup;
      }
      set
      {
        m_rootGroup = value;
      }
    }

    public override bool CanSort
    {
      get
      {
        return true;
      }
    }

    public override bool CanGroup
    {
      get
      {
        return true;
      }
    }

    public override bool CanFilter
    {
      get
      {
        return true;
      }
    }

    public override Predicate<object> Filter
    {
      get
      {
        return m_filter;
      }
      set
      {
        if( !this.CanFilter )
          throw new NotSupportedException();

        if( ( value == null ) && ( value == m_filter ) )
          return;

        m_filter = value;
        this.Refresh();
      }
    }

    public override bool PassesFilter( object item )
    {
      if( ( this.CanFilter ) && ( m_filter != null ) )
        return m_filter( item );

      return true;
    }

    #endregion

    #region IEditableCollectionView

    public virtual bool CanAddNew
    {
      get
      {
        return true;
      }
    }

    public virtual object AddNew()
    {
      if( !this.CanAddNew )
        throw new InvalidOperationException( "An attempt was made to add an item to a source that does not support addition." );

      // If a current AddNew has not been committed, commit it.
      this.CommitNew();
      this.CommitEdit();

      this.QueueOperationForAddNew = true;

      try
      {
        DataGridCreatingNewItemEventArgs creatingNewItemEventArgs = new DataGridCreatingNewItemEventArgs( this, null, false );

        this.RootDataGridCollectionViewBase.OnCreatingNewItem( creatingNewItemEventArgs );

        this.SetCurrentAddNew( creatingNewItemEventArgs.NewItem, -1 );

        if( creatingNewItemEventArgs.Cancel )
          throw new DataGridException( "AddNew was canceled." );

        this.CreatingNewItemIsManuallyHandled = creatingNewItemEventArgs.Handled;

        if( this.CreatingNewItemIsManuallyHandled )
        {
          if( m_currentAddItem == null )
            throw new InvalidOperationException( "An attempt was made to handle the CreatingNewItem event without providing a new item." );
        }
        else
        {
          if( !this.HasSource )
            throw new InvalidOperationException( "The CreatingNewItem event must be handled when not using an underlying data source." );

          int newItemIndex;
          object newItem = ItemsSourceHelper.AddNewDataItem( this.Enumeration, null, out newItemIndex );

          this.SetCurrentAddNew( newItem, newItemIndex );
        }

        DataGridItemEventArgs initializingNewItemEventArgs = new DataGridItemEventArgs( this, m_currentAddItem );
        this.RootDataGridCollectionViewBase.OnInitializingNewItem( initializingNewItemEventArgs );
      }
      catch
      {
        // QueueOperationForAddNew must be off when calling CancelNew
        this.QueueOperationForAddNew = false;

        if( m_currentAddItem != null )
        {
          this.CancelNew();
        }

        throw;
      }
      finally
      {
        this.QueueOperationForAddNew = false;
      }

      DataGridItemEventArgs itemCreatedEventArgs = new DataGridItemEventArgs( this, m_currentAddItem );
      m_rootDataGridCollectionViewBase.OnNewItemCreated( itemCreatedEventArgs );

      return m_currentAddItem;
    }

    public virtual void CommitNew()
    {
      if( m_currentAddItem == null )
        return;

      object item = m_currentAddItem;
      DeferredOperation addOperationToNotifyIfNotAlreadyQueued = null;
      this.QueueOperationForAddNew = true;

      try
      {
        DataGridCommittingNewItemEventArgs committingNewItemEventArgs = new DataGridCommittingNewItemEventArgs( this, m_currentAddItem, false );

        this.RootDataGridCollectionViewBase.OnCommittingNewItem( committingNewItemEventArgs );

        if( committingNewItemEventArgs.Cancel )
          throw new DataGridException( "CommitNew was canceled." );

        if( this.CreatingNewItemIsManuallyHandled != committingNewItemEventArgs.Handled )
          throw new InvalidOperationException( "When manually handling the item-insertion process the CreatingNewItem, CommittingNewItem, and CancelingNewItem events must all be handled." );

        if( !committingNewItemEventArgs.Handled )
        {
          if( !this.HasSource )
            throw new InvalidOperationException( "The CommittingNewItem event must be handled when not using an underlying data source." );

          int addItemPosition = m_currentAddItemPosition;
          ItemsSourceHelper.EndNewDataItem( this.Enumeration, null, m_currentAddItem, ref addItemPosition );

          addOperationToNotifyIfNotAlreadyQueued = new DeferredOperation( DeferredOperation.DeferredOperationAction.Add, addItemPosition + 1,
                                                                          addItemPosition, new object[] { m_currentAddItem } );
        }
        else
        {
          if( committingNewItemEventArgs.Index == -1 )
            throw new InvalidOperationException( "An attempt was made to handle the CommittingNewItem event without providing the index at which the new item was inserted." );

          if( committingNewItemEventArgs.NewCount == -1 )
            throw new InvalidOperationException( "An attempt was made to handle the CommittingNewItem event without providing the new item count." );

          if( committingNewItemEventArgs.Index >= committingNewItemEventArgs.NewCount )
            throw new InvalidOperationException( "The index at which the new item was inserted was greater than the new item count." );

          addOperationToNotifyIfNotAlreadyQueued = new DeferredOperation( DeferredOperation.DeferredOperationAction.Add, committingNewItemEventArgs.NewCount,
                                                                          committingNewItemEventArgs.Index, new object[] { m_currentAddItem } );
        }

        this.SetCurrentAddNew( null, -1 );
      }
      finally
      {
        this.QueueOperationForAddNew = false;
      }

      if( m_deferredAddNewOperationManager != null )
      {
        m_deferredAddNewOperationManager.PurgeAddWithRemoveOrReplace();

        DeferredOperationManager deferredOperationManager = this.DeferredOperationManager;

        lock( deferredOperationManager )
        {
          if( this.ShouldDeferOperation )
          {
            deferredOperationManager.Combine( m_deferredAddNewOperationManager );
          }
          else
          {
            m_deferredAddNewOperationManager.Process();
          }
        }

        m_deferredAddNewOperationManager = null;
      }
      else
      {
        Debug.Assert( !this.IsSourceSupportingChangeNotification );

        // No pending operation was in m_deferredAddNewOperationManager, so the list has not trigger notification for the new item added.
        // We will, in that case, raise the notification ourself.
        this.ExecuteOrQueueSourceItemOperation( addOperationToNotifyIfNotAlreadyQueued );
      }

      DataGridItemEventArgs itemEventArgs = new DataGridItemEventArgs( this, item );
      m_rootDataGridCollectionViewBase.OnNewItemCommitted( itemEventArgs );
    }

    public virtual void CancelNew()
    {
      if( m_currentAddItem == null )
        return;

      object currentAddItem = m_currentAddItem;
      this.QueueOperationForAddNew = true;

      try
      {
        DataGridItemHandledEventArgs cancelingNewItemEventArgs = new DataGridItemHandledEventArgs( this, currentAddItem );
        this.RootDataGridCollectionViewBase.OnCancelingNewItem( cancelingNewItemEventArgs );

        if( this.CreatingNewItemIsManuallyHandled != cancelingNewItemEventArgs.Handled )
          throw new InvalidOperationException( "When manually handling the item-insertion process the CreatingNewItem, CommittingNewItem, and CancelingNewItem events must all be handled." );

        if( !cancelingNewItemEventArgs.Handled )
        {
          if( !this.HasSource )
            throw new InvalidOperationException( "The CancelingNewItem event must be handled when not using an underlying data source." );

          ItemsSourceHelper.CancelNewDataItem( this.Enumeration, null, m_currentAddItem, m_currentAddItemPosition );
        }

        this.SetCurrentAddNew( null, -1 );
      }
      finally
      {
        this.QueueOperationForAddNew = false;
      }

      if( m_deferredAddNewOperationManager != null )
      {
        m_deferredAddNewOperationManager.PurgeAddWithRemoveOrReplace();

        DeferredOperationManager deferredOperationManager = this.DeferredOperationManager;

        lock( deferredOperationManager )
        {
          if( this.ShouldDeferOperation )
          {
            deferredOperationManager.Combine( m_deferredAddNewOperationManager );
          }
          else
          {
            m_deferredAddNewOperationManager.Process();
          }
        }

        m_deferredAddNewOperationManager = null;
      }

      DataGridItemEventArgs itemEventArgs = new DataGridItemEventArgs( this, currentAddItem );
      m_rootDataGridCollectionViewBase.OnNewItemCanceled( itemEventArgs );
    }

    public virtual void EditItem( object item )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      this.CommitNew();

      if( m_currentEditItem == item )
        return;

      this.CommitEdit();

      var itemCancelEventArgs = new DataGridItemCancelEventArgs( this, item, false );
      m_rootDataGridCollectionViewBase.OnBeginningEdit( itemCancelEventArgs );

      if( itemCancelEventArgs.Cancel )
        throw new DataGridException( "EditItem was canceled." );

      this.ItemEditionIsManuallyHandled = itemCancelEventArgs.Handled;

      if( !this.ItemEditionIsManuallyHandled )
      {
        bool beginEditCalled;
        this.EditItemInternal( item, out beginEditCalled );

        if( !beginEditCalled )
        {
          var itemProperties = this.ItemProperties;
          m_oldValuesBeforeEdition = new Dictionary<DataGridItemPropertyBase, object>( itemProperties.Count );

          foreach( var itemProperty in itemProperties )
          {
            m_oldValuesBeforeEdition[ itemProperty ] = ItemsSourceHelper.GetValueFromItemProperty( itemProperty, item );
          }
        }
      }

      m_currentEditItem = item;
      m_rootDataGridCollectionViewBase.OnEditBegun( new DataGridItemEventArgs( this, item ) );
    }

    public virtual void CommitEdit()
    {
      if( m_currentEditItem == null )
        return;

      var itemCancelEventArgs = new DataGridItemCancelEventArgs( this, m_currentEditItem, false );
      m_rootDataGridCollectionViewBase.OnCommittingEdit( itemCancelEventArgs );

      if( itemCancelEventArgs.Cancel )
        throw new DataGridException( "CommitEdit was canceled." );

      if( itemCancelEventArgs.Handled != this.ItemEditionIsManuallyHandled )
        throw new InvalidOperationException( "When manually handling the item-edition process the BeginningEdit, CommittingEdit, and CancelingEdit events must all be handled." );

      if( !itemCancelEventArgs.Handled )
      {
        bool endEditCalled;
        this.EndEditInternal( m_currentEditItem, out endEditCalled );
        m_oldValuesBeforeEdition = null;
      }

      var itemEventArgs = new DataGridItemEventArgs( this, m_currentEditItem );
      var index = this.IndexOfSourceItem( m_currentEditItem );

      var items = new object[] { m_currentEditItem };
      m_currentEditItem = null;

      this.ExecuteOrQueueSourceItemOperation( new DeferredOperation( DeferredOperation.DeferredOperationAction.Replace, -1, index, items, index, items ) );

      m_rootDataGridCollectionViewBase.OnEditCommitted( itemEventArgs );
    }

    public virtual bool CanCancelEdit
    {
      get
      {
        return true;
      }
    }

    public virtual void CancelEdit()
    {
      if( !this.CanCancelEdit )
        throw new InvalidOperationException( "An attempt was made to cancel the edit process on a source that does not support cancellation." );

      if( m_currentEditItem == null )
        return;

      var itemHandleEventArgs = new DataGridItemHandledEventArgs( this, m_currentEditItem );
      m_rootDataGridCollectionViewBase.OnCancelingEdit( itemHandleEventArgs );

      if( itemHandleEventArgs.Handled != this.ItemEditionIsManuallyHandled )
        throw new InvalidOperationException( "An attempt was made to manually handle the edit prcess without handling the BeginningEdit, CommittingEdit, and CancelingEdit events." );

      if( !itemHandleEventArgs.Handled )
      {
        bool cancelEditCalled;
        this.CancelEditInternal( m_currentEditItem, out cancelEditCalled );

        var itemProperties = this.ItemProperties;

        if( ( !cancelEditCalled ) && ( itemProperties.Count > 0 ) )
        {
          itemProperties.SuspendUnboundItemPropertyChanged( m_currentEditItem );

          try
          {
            foreach( var itemProperty in itemProperties )
            {
              var readOnly = ( ( m_currentEditItem == m_currentAddItem ) && itemProperty.OverrideReadOnlyForInsertion.HasValue && itemProperty.OverrideReadOnlyForInsertion.Value )
                               ? false
                               : itemProperty.IsReadOnly;

              if( readOnly )
                continue;

              try
              {
                ItemsSourceHelper.SetValueForItemProperty( itemProperty, m_currentEditItem, m_oldValuesBeforeEdition[ itemProperty ] );
              }
              catch
              {
                // Swallow any Exception, setting a property to is old value should not throw.
              }
            }
          }
          finally
          {
            itemProperties.ResumeUnboundItemPropertyChanged( m_currentEditItem );
          }
        }
      }

      var itemEventArgs = new DataGridItemEventArgs( this, m_currentEditItem );
      m_currentEditItem = null;
      m_rootDataGridCollectionViewBase.OnEditCanceled( itemEventArgs );
    }

    public virtual bool CanRemove
    {
      get
      {
        return true;
      }
    }

    public virtual void Remove( object item )
    {
      if( !this.CanRemove )
        throw new InvalidOperationException( "An attempt was made to remove an item from a source that does not support removal." );

      if( this.IsEditingItem || this.IsAddingNew )
        throw new InvalidOperationException( "An attempt was made to remove an item while an item is being edited or added." );

      int sourceIndex = -2;

      if( ( !this.HasSource ) || ( !this.IsSourceSupportingChangeNotification ) )
        sourceIndex = this.IndexOfSourceItem( item );

      DataGridRemovingItemEventArgs removingItemEventArgs = new DataGridRemovingItemEventArgs( this, item, -1, false );
      this.RootDataGridCollectionViewBase.OnRemovingItem( removingItemEventArgs );

      if( removingItemEventArgs.Cancel )
        throw new DataGridException( "Remove was canceled." );

      if( !removingItemEventArgs.Handled )
      {
        if( !this.HasSource )
          throw new InvalidOperationException( "The RemovingItem event must be handled when not using an underlying data source." );

        if( sourceIndex == -2 )
          sourceIndex = this.IndexOfSourceItem( item );

        if( sourceIndex != -1 )
          this.InternalRemoveAt( sourceIndex );
      }

      if( ( !this.HasSource ) || ( !this.IsSourceSupportingChangeNotification ) )
      {
        DeferredOperation deferredOperation = new DeferredOperation(
          DeferredOperation.DeferredOperationAction.Remove,
          sourceIndex,
          new object[] { item } );

        this.ExecuteOrQueueSourceItemOperation( deferredOperation );
      }

      DataGridItemRemovedEventArgs itemRemovedEventArgs = new DataGridItemRemovedEventArgs( this, item, -1 );
      this.RootDataGridCollectionViewBase.OnItemRemoved( itemRemovedEventArgs );
    }

    public virtual void RemoveAt( int index )
    {
      object item = this.GetItemAt( index );
      this.Remove( item );
    }

    public virtual object CurrentAddItem
    {
      get
      {
        return m_currentAddItem;
      }
    }

    public virtual object CurrentEditItem
    {
      get
      {
        return m_currentEditItem;
      }
    }

    public virtual bool IsAddingNew
    {
      get
      {
        return m_currentAddItem != null;
      }
    }

    public virtual bool IsEditingItem
    {
      get
      {
        return m_currentEditItem != null;
      }
    }

    NewItemPlaceholderPosition IEditableCollectionView.NewItemPlaceholderPosition
    {
      get
      {
        return m_newItemPlaceholderPosition;
      }
      set
      {
        m_newItemPlaceholderPosition = value;
      }
    }

    private NewItemPlaceholderPosition m_newItemPlaceholderPosition = NewItemPlaceholderPosition.None;

    #endregion

    #region IItemProperties Members

    ReadOnlyCollection<ItemPropertyInfo> IItemProperties.ItemProperties
    {
      get
      {
        List<ItemPropertyInfo> itemPropertyInfos = this.GetItemPropertyInfos();

        if( itemPropertyInfos == null )
          return null;

        return new ReadOnlyCollection<ItemPropertyInfo>( itemPropertyInfos );
      }
    }

    internal virtual List<ItemPropertyInfo> GetItemPropertyInfos()
    {
      return ( from item in m_itemProperties
               select new ItemPropertyInfo( item.Name, item.DataType, item.GetPropertyDescriptorForBinding() ) ).ToList();
    }

    #endregion

    private BitVector32 m_flags = new BitVector32();

    private DeferredOperationManager m_deferredOperationManager;
    private DeferredOperationManager m_deferredAddNewOperationManager;

    private int m_deferRefreshCount;

    private readonly DataGridCollectionViewBase m_rootDataGridCollectionViewBase;

    private Type m_itemType;
    private Type m_desiredItemType;
    private object m_modelSource;
    private IEnumerable m_enumeration;

    private int m_currentPosition;
    private object m_currentItem;
    private int m_deferCurrencyEventCount;

    private object m_currentEditItem;
    private Dictionary<DataGridItemPropertyBase, object> m_oldValuesBeforeEdition;

    private object m_currentAddItem;
    private int m_currentAddItemPosition = -1;

    private DistinctValuesConstraint m_distinctValuesConstraint = DistinctValuesConstraint.All;
    private DistinctValuesUpdateMode m_distinctValueUpdateMode = DistinctValuesUpdateMode.Manual;

    private List<DataGridItemPropertyBase> m_filteredItemProperties;
    private ReadOnlyCollection<DataGridItemPropertyBase> m_readOnlyFilteredItemProperties;

    private DataGridDetailDescription m_parentDetailDescription;

    private readonly DataGridItemPropertyCollection m_itemProperties;

    private DataGridSortDescriptionCollection m_sortDescriptions;
    private ObservableCollection<GroupDescription> m_groupDescriptions;
    private readonly DataGridDetailDescriptionCollection m_detailDescriptions;
    private readonly PropertyDescriptionRouteDictionary m_defaultPropertyDescriptions;

    private CollectionViewGroup m_rootGroup;

    private Predicate<object> m_filter;

    // Containing a DataGridItemProperty key, ObservableCollection<object> values map
    private DistinctValuesDictionary m_distinctValues;
    private DataGridItemPropertyBase m_excludedItemPropertyFromDistinctValueCalculation;

    [Flags]
    private enum DataGridCollectionViewBaseFlags
    {
      Loaded = 1 << 0,
      Refreshing = 1 << 1,
      QueueOperationForAddNew = 1 << 2,
      IsRefreshingDistinctValues = 1 << 3,
      IsCurrentAfterLast = 1 << 4,
      IsCurrentBeforeFirst = 1 << 5,
      CreatingNewItemIsManuallyHandled = 1 << 6,
      ItemEditionIsManuallyHandled = 1 << 7,
      AutoCreateItemProperties = 1 << 8,
      AutoCreateDetailDescriptions = 1 << 9,
      AutoCreateForeignKeyDescriptions = 1 << 10,
      DefaultItemPropertiesInitialized = 1 << 11,
      DefaultPropertyDescriptionsCreated = 1 << 12,
    }

    private sealed class DeferRefreshHelper : IDisposable
    {
      public DeferRefreshHelper( DataGridCollectionViewBase collectionView )
      {
        m_collectionView = collectionView;

        Interlocked.Increment( ref collectionView.m_deferRefreshCount );
      }

      void IDisposable.Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        // Prevent this method from being invoked more than once by the same IDisposable.
        var collectionView = Interlocked.Exchange( ref m_collectionView, null );
        if( collectionView == null )
          return;

        if( collectionView.CheckAccess() )
        {
          DeferRefreshHelper.ProcessDispose( collectionView );
        }
        else
        {
          //In case Dispose is called from a different thread, make sure the refresh is done on the UI thread.
          collectionView.Dispatcher.BeginInvoke( new Action<DataGridCollectionViewBase>( DeferRefreshHelper.ProcessDispose ), DispatcherPriority.Send, collectionView );
        }
      }

      private static void ProcessDispose( DataGridCollectionViewBase collectionView )
      {
        //If there are detail DataGridCollectionView's being defered.
        if( collectionView.m_detailDeferRefreshes != null && collectionView.m_detailDeferRefreshes.Count > 0 )
        {
          //Dispose them.
          foreach( IDisposable detailDisposable in collectionView.m_detailDeferRefreshes[ 0 ] )
          {
            if( detailDisposable != null )
            {
              detailDisposable.Dispose();
            }
          }

          //Clear and remove the list that was just disposed of.
          collectionView.m_detailDeferRefreshes[ 0 ].Clear();
          collectionView.m_detailDeferRefreshes.RemoveAt( 0 );
        }

        //Only process the DeferRefresh when count is back to 0.
        if( Interlocked.Decrement( ref collectionView.m_deferRefreshCount ) > 0 )
          return;

        if( collectionView.Loaded )
        {
          collectionView.m_deferredOperationManager.Process();
        }
        else
        {
          // We call ForceRefresh when not yet "Loaded" because we want the Dispose here to triger the CollectionChanged( NotifyCollectionChangedAction.Reset )
          // This is needed because of the way the ItemsControl works (other than our grid).
          collectionView.ForceRefresh( true, true, true );
        }
      }

      ~DeferRefreshHelper()
      {
        //Make sure the defering process for the CollectionView stays in a valid state, by disposing of details and properly setting the count.
        this.Dispose( false );
      }

      private DataGridCollectionViewBase m_collectionView;
    }

    private sealed class DeferCurrencyEventHelper : IDisposable
    {
      public DeferCurrencyEventHelper( DataGridCollectionViewBase collectionView )
      {
        m_collectionView = collectionView;
        m_oldCurrentItem = m_collectionView.CurrentItem;
        m_oldCurrentPosition = m_collectionView.CurrentPosition;
        m_oldIsCurrentBeforeFirst = m_collectionView.IsCurrentBeforeFirst;
        m_oldIsCurrentAfterLast = m_collectionView.IsCurrentAfterLast;

        Interlocked.Increment( ref m_collectionView.m_deferCurrencyEventCount );
      }

      void IDisposable.Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        // Prevent this method from being invoked more than once by the same IDisposable.
        var collectionView = Interlocked.Exchange( ref m_collectionView, null );
        if( collectionView == null )
          return;

        if( collectionView.CheckAccess() )
        {
          DeferCurrencyEventHelper.ProcessDispose( collectionView, m_oldCurrentItem, m_oldCurrentPosition, m_oldIsCurrentBeforeFirst, m_oldIsCurrentAfterLast );
        }
        else
        {
          // Make sure the calls to the collection view will be done on the collection view's thread.
          collectionView.Dispatcher.BeginInvoke(
            new Action<DataGridCollectionViewBase, object, int, bool, bool>( DeferCurrencyEventHelper.ProcessDispose ),
            DispatcherPriority.Send,
            collectionView,
            m_oldCurrentItem,
            m_oldCurrentPosition,
            m_oldIsCurrentBeforeFirst,
            m_oldIsCurrentAfterLast );
        }
      }

      private static void ProcessDispose(
        DataGridCollectionViewBase collectionView,
        object oldCurrentItem,
        int oldCurrentPosition,
        bool oldIsCurrentBeforeFirst,
        bool oldIsCurrentAfterLast )
      {
        if( Interlocked.Decrement( ref collectionView.m_deferCurrencyEventCount ) > 0 )
          return;

        bool itemChanged = false;

        if( !object.Equals( oldCurrentItem, collectionView.CurrentItem ) )
        {
          itemChanged = true;
          collectionView.OnPropertyChanged( new PropertyChangedEventArgs( "CurrentItem" ) );
        }

        if( oldCurrentPosition != collectionView.CurrentPosition )
        {
          itemChanged = true;
          collectionView.OnPropertyChanged( new PropertyChangedEventArgs( "CurrentPosition" ) );
        }

        if( oldIsCurrentBeforeFirst != collectionView.IsCurrentBeforeFirst )
        {
          itemChanged = true;
          collectionView.OnPropertyChanged( new PropertyChangedEventArgs( "IsCurrentBeforeFirst" ) );
        }

        if( oldIsCurrentAfterLast != collectionView.IsCurrentAfterLast )
        {
          itemChanged = true;
          collectionView.OnPropertyChanged( new PropertyChangedEventArgs( "IsCurrentAfterLast" ) );
        }

        if( itemChanged )
        {
          collectionView.OnCurrentChanged();
        }
      }

      ~DeferCurrencyEventHelper()
      {
        this.Dispose( false );
      }

      private readonly int m_oldCurrentPosition;
      private readonly object m_oldCurrentItem;
      private readonly bool m_oldIsCurrentAfterLast;
      private readonly bool m_oldIsCurrentBeforeFirst;

      private DataGridCollectionViewBase m_collectionView;
    }
  }
}
