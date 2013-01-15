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
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections;
using Xceed.Utils.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using System.Data;
using System.Windows;
using System.Xml;
using System.Reflection;
using System.Data.Objects.DataClasses;
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid
{
  public abstract partial class DataGridCollectionViewBase : CollectionView, IEditableCollectionView, IWeakEventListener, IItemProperties
  {
    #region STATIC MEMBERS

    internal static string GetPropertyNameFromGroupDescription( GroupDescription groupDescription )
    {
      PropertyGroupDescription propertyGroupDescription =
        groupDescription as PropertyGroupDescription;

      if( propertyGroupDescription != null )
        return propertyGroupDescription.PropertyName;

      DataGridGroupDescription dataGridGroupDescription =
        groupDescription as DataGridGroupDescription;

      if( dataGridGroupDescription != null )
        return dataGridGroupDescription.PropertyName;

      return null;
    }

    private static List<DataGridItemPropertyBase> GetDataGridItemProperties( Dictionary<string, ItemsSourceHelper.FieldDescriptor> fieldDescriptors )
    {
      List<DataGridItemPropertyBase> itemProperties = new List<DataGridItemPropertyBase>( fieldDescriptors.Count );

      foreach( KeyValuePair<string, ItemsSourceHelper.FieldDescriptor> fieldDescriptorKeyValuePair in fieldDescriptors )
      {
        ItemsSourceHelper.FieldDescriptor fieldDescriptor = fieldDescriptorKeyValuePair.Value;

        itemProperties.Add( new DataGridItemProperty(
                              fieldDescriptor.Name,
                              fieldDescriptor.PropertyDescriptor,
                              fieldDescriptor.DisplayName,
                              fieldDescriptor.BindingXPath,
                              fieldDescriptor.BindingPath,
                              fieldDescriptor.DataType,
                              true,
                              fieldDescriptor.ReadOnly,
                              fieldDescriptor.OverrideReadOnlyForInsertion,
                              fieldDescriptor.IsASubRelationship,
                              fieldDescriptor.ForeignKeyDescription ) );
      }

      return itemProperties;
    }

    private static List<DataGridItemPropertyBase> EmptyDataGridItemPropertyList = new List<DataGridItemPropertyBase>();

    #endregion STATIC MEMBERS

    private DataGridCollectionViewBase()
      : base( new object[ 0 ] )
    {

      this.PreConstruct();

      m_distinctValues = new DistinctValuesDictionary( this );
      m_autoFilterValues = new ReadOnlyDictionary<string, IList>();
      m_autoFilteredDataGridItemProperties = new ObservableCollection<DataGridItemPropertyBase>();

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
      this.AutoCreateItemProperties = autoCreateItemProperties;
      this.AutoCreateDetailDescriptions = autoCreateDetailDescriptions;
      this.AutoCreateForeignKeyDescriptions = autoCreateForeignKeyDescriptions;

      m_desiredItemType = itemType;

      if( itemType == null )
      {
        if( source != null )
        {
          itemType = ItemsSourceHelper.GetItemTypeFromEnumeration( source );
        }
        else if( modelSource != null )
        {
          IQueryable queryable = modelSource as IQueryable;

          if( queryable != null )
            itemType = queryable.ElementType;
        }
      }
      else
      {
        Type listItemType = ItemsSourceHelper.GetItemTypeFromEnumeration( source );

        if( listItemType == null )
          listItemType = typeof( object );

        if( !listItemType.IsAssignableFrom( itemType ) && !itemType.IsAssignableFrom( listItemType ) )
          throw new InvalidOperationException( "The itemType is not assignable to the type of the list." );
      }

      this.SetSource( modelSource, source, itemType, true );

      // This will create a new ItemProperties/DatatGridDetailDescriptions collection and pre-feed them with the 
      // ItemProperties/DetailDescriptions detected on the source collection.
      // For DataGridCollectionViews not supporting master/detail, the collection will be created but nothing will be inserted.
      this.SetupDefaultDetailDescriptions();

      this.SetupDefaultItemProperties();
      this.CreateDefaultCollections( null );
      this.RegisterCollectionChanged();
    }

    internal DataGridCollectionViewBase( IEnumerable collection, DataGridDetailDescription parentDetailDescription, DataGridCollectionViewBase rootDataGridCollectionViewBase )
      : this()
    {
      if( !this.CanHaveDetails )
        throw new InvalidOperationException( "An attempt was made to provide a detail description for a source that cannot have details." );

      if( parentDetailDescription == null )
        throw new ArgumentNullException( "parentDetailDescription" );

      this.AutoCreateDetailDescriptions = parentDetailDescription.AutoCreateDetailDescriptions;
      this.AutoCreateItemProperties = parentDetailDescription.AutoCreateItemProperties;
      this.AutoCreateForeignKeyDescriptions = parentDetailDescription.AutoCreateForeignKeyDescriptions;

      m_parentDetailDescription = parentDetailDescription;
      m_rootDataGridCollectionViewBase = rootDataGridCollectionViewBase;

      Type itemType;
      itemType = ItemsSourceHelper.GetItemTypeFromEnumeration( collection );

      if( itemType == null )
        itemType = typeof( object );

      this.InitializeFromExternalDeclaration( collection, itemType );

      // This is required in the Master/Detail scheme of things!
      this.ForceRefresh( false, true, false );
    }

    internal virtual void PreConstruct()
    {
    }

    #region ParentCollectionViewSource Property

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

    #endregion ParentCollectionViewSource Property

    #region NeedsRefresh Property

    public override bool NeedsRefresh
    {
      get
      {
        return m_deferredOperationManager.RefreshPending;
      }
    }

    #endregion NeedsRefresh Property

    #region Culture Property

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
        if( ( this.InDeferRefresh )
          && ( m_parentCollectionViewSourceBase != null ) )
        {
          m_parentCollectionViewSourceBase.ApplyExtraPropertiesToView( this );
        }
      }
    }

    #endregion Culture Property

    #region AutoCreateItemProperties Property

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

    #endregion AutoCreateItemProperties Property

    #region AutoCreateDetailDescriptions Property

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

    #endregion AutoCreateDetailDescriptions Property

    #region ParentDetailDescription Property

    internal DataGridDetailDescription ParentDetailDescription
    {
      get
      {
        return m_parentDetailDescription;
      }
    }


    #endregion ParentDetailDescription Property

    #region AutoCreateForeignKeyDescriptions Property

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

    #region SyncRoot Property

    internal virtual object SyncRoot
    {
      get
      {
        return this;
      }
    }

    #endregion

    #region DeferredOperationManager Property

    internal DeferredOperationManager DeferredOperationManager
    {
      get
      {
        return m_deferredOperationManager;
      }
    }

    #endregion

    #region InDeferRefresh Property

    internal bool InDeferRefresh
    {
      get
      {
        return m_deferRefreshCount > 0;
      }
    }

    #endregion

    #region Loaded Property

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

    #region IsRefreshingDestinctValues Property

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

    #region Refreshing Property

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

    #region ModelSource Property

    internal object ModelSource
    {
      get
      {
        return m_modelSource;
      }
    }

    #endregion

    #region ItemEditionIsManuallyHandled Property

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

    #region CreatingNewItemIsManuallyHandled Property

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

    #region ItemPropertiesCreated Property

    private bool ItemPropertiesCreated
    {
      get
      {
        return m_flags[ ( int )DataGridCollectionViewBaseFlags.ItemPropertiesCreated ];
      }
      set
      {
        m_flags[ ( int )DataGridCollectionViewBaseFlags.ItemPropertiesCreated ] = value;
      }
    }

    #endregion

    #region ItemProperties Property

    public DataGridItemPropertyCollection ItemProperties
    {
      get
      {
        Debug.Assert( m_itemProperties != null );
        return m_itemProperties;
      }
    }

    #endregion

    #region  CanCreateItemPropertiesFormModelSource Property

    private bool CanCreateItemPropertiesFromModelSource
    {
      get
      {
        return ( ( m_modelSource != null ) && ( m_modelSource is DataTable || m_modelSource is IQueryable ) );
      }
    }

    #endregion


    #region DataGridCollectionViewBase ABSTRACT MEMBERS

    internal virtual int IndexOfSourceItem( object item )
    {
      return this.IndexOf( item );
    }

    internal abstract DataGridCollectionViewBase CreateDetailDataGridCollectionViewBase(
      IEnumerable detailDataSource,
      DataGridDetailDescription parentDetailDescription,
      DataGridCollectionViewBase rootDataGridCollectionViewBase );

    internal abstract void EnsurePosition( int globalSortedIndex );

    internal abstract int SourceItemCount
    {
      get;
    }

    internal abstract void ForceRefresh( bool sendResetNotification, bool initialLoad, bool setCurrentToFirstOnInitialLoad );

    #endregion DataGridCollectionViewBase ABSTRACT MEMBERS

    #region SOURCE

    private void SetSource( object modelSource, IEnumerable source, Type itemType, bool setCurrentToFirst )
    {
      if( this.Loaded )
        throw new InvalidOperationException( "An attempt was made to change the source when the collection view has already been loaded." );

      m_modelSource = modelSource;
      m_itemType = itemType;

      this.SetSourceCore( source, setCurrentToFirst );
    }

    private void SetSourceCore( IEnumerable source, bool setCurrentToFirst )
    {
      Debug.Assert( !this.Loaded );

      if( source == null )
      {
        m_enumeration = null;
        m_notifyCollectionChanged = null;
        m_dataTable = null;

        this.SetCurrentItemAndPositionCore( null, -1, true, true );
        return;
      }

      // Unsubscribe from old list event
      if( m_notifyCollectionChanged != null )
        CollectionChangedEventManager.RemoveListener( m_notifyCollectionChanged, this );

      IBindingList oldBindingList = m_enumeration as IBindingList;

      if( ( oldBindingList != null ) && ( oldBindingList.SupportsChangeNotification ) )
        ListChangedEventManager.RemoveListener( oldBindingList, this );

      m_enumeration = source;
      m_notifyCollectionChanged = m_enumeration as INotifyCollectionChanged;

      if( m_notifyCollectionChanged != null )
        CollectionChangedEventManager.AddListener( m_notifyCollectionChanged, this );

      if( m_notifyCollectionChanged == null )
      {
        IBindingList newBindingList = m_enumeration as IBindingList;

        if( ( newBindingList != null ) && ( newBindingList.SupportsChangeNotification ) )
          ListChangedEventManager.AddListener( newBindingList, this );
      }

      m_dataTable = m_enumeration as DataTable;

      if( m_enumeration != null )
      {
        // Position to the first item
        IEnumerator enumerator = m_enumeration.GetEnumerator();

        if( enumerator.MoveNext() )
        {
          if( setCurrentToFirst )
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
      else
      {
        this.SetCurrentItemAndPositionCore( null, -1, true, true );
      }
    }

    internal IEnumerable Enumeration
    {
      get
      {
        return m_enumeration;
      }
    }

    internal INotifyCollectionChanged NotifyCollectionChanged
    {
      get
      {
        return m_notifyCollectionChanged;
      }
    }

    public Type ItemType
    {
      get
      {
        return m_itemType;
      }
      private set
      {
        if( m_itemType == value )
          return;

        if( this.ItemPropertiesCreated )
          throw new InvalidOperationException( "An attempt was made to modify the ItemType property after the DataGridItemProperty has been created." );

        m_itemType = value;
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
        return m_enumeration != null;
      }
    }

    #endregion SOURCE

    #region SOURCE COLLECTION CHANGED HANDLERS

    private void NotifyCollectionChanged_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
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
          this.ExecuteOrQueueSourceItemOperation( deferredOperation );
      }
    }

    private void BindingList_ListChanged( object sender, ListChangedEventArgs e )
    {
      Debug.Assert( !( this is DataGridVirtualizingCollectionViewBase ),
        "The DataGridVirtualizingCollectionView is unbound and therefore should never receive a notification from a source." );

      IBindingList bindingList = ( IBindingList )m_enumeration;
      int index = e.NewIndex;
      IList items = null;
      ListChangedType listChangedType = e.ListChangedType;

      //If we're within bounds
      if( ( index > -1 ) && ( index < bindingList.Count ) )
      {
        //Let's get the item
        items = new object[] { bindingList[ index ] };
      }
      else
      {
        //Let's reset the list because a change to the list has happenned (e.g. a delete) before we had a chance to process the current change.
        listChangedType = ListChangedType.Reset;
      }

      lock( m_deferredOperationManager )
      {
        if( m_deferredOperationManager.RefreshPending )
          return;

        DeferredOperation deferredOperation = null;

        switch( listChangedType )
        {
          case ListChangedType.ItemAdded:
            {
              deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.Add, bindingList.Count, index, items );
              break;
            }

          case ListChangedType.ItemChanged:
            {
              DataGridCollectionView view = this as DataGridCollectionView;
              if( view != null && view.UpdateChangedPropertyStatsOnly && e.PropertyDescriptor != null )
              {
                view.CalculateChangedPropertyStatsOnly = true;
                foreach( Stats.StatFunction statFunc in view.StatFunctions )
                {
                  if( statFunc.SourcePropertyName.Contains( e.PropertyDescriptor.Name ) && !view.InvalidatedStatFunctions.Contains( statFunc ) )
                  {
                    view.InvalidatedStatFunctions.Add( statFunc );
                  }
                }
              }

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

          case ListChangedType.PropertyDescriptorAdded:
            {
              if( this.CheckAccess() )
              {
                this.HandlePropertyDescriptorAdded( e.PropertyDescriptor );
              }
              else
              {
                this.Dispatcher.Invoke( new Action<PropertyDescriptor>( this.HandlePropertyDescriptorAdded ), e.PropertyDescriptor );
              }
              break;
            }

          case ListChangedType.PropertyDescriptorChanged:
            {
              if( this.CheckAccess() )
              {
                this.HandlePropertyDescriptorChanged( e.PropertyDescriptor );
              }
              else
              {
                this.Dispatcher.Invoke( new Action<PropertyDescriptor>( this.HandlePropertyDescriptorChanged ), e.PropertyDescriptor );
              }
              break;
            }

          case ListChangedType.PropertyDescriptorDeleted:
            {
              if( this.CheckAccess() )
              {
                this.HandlePropertyDescriptorDeleted( e.PropertyDescriptor );
              }
              else
              {
                this.Dispatcher.Invoke( new Action<PropertyDescriptor>( this.HandlePropertyDescriptorDeleted ), e.PropertyDescriptor );
              }
              break;
            }

          default:
            throw new NotSupportedException( "This ListChangedType (" + listChangedType.ToString() + ") is not supported." );
        }

        if( deferredOperation != null )
        {
          this.ExecuteOrQueueSourceItemOperation( deferredOperation );
        }
      }
    }

    private void HandlePropertyDescriptorAdded( PropertyDescriptor property )
    {
      // setting m_defaultItemProperties, will force the collection view to reanalyse the source to get a new list of default ItemProperties.
      m_defaultItemProperties = null;

      if( this.AutoCreateItemProperties )
      {
        string name = property.Name;

        if( m_itemProperties[ name ] == null )
        {
          foreach( DataGridItemPropertyBase itemProperty in this.GetDefaultItemProperties() )
          {
            if( itemProperty.Name == name )
            {
              this.AddValidItemProperty( itemProperty );
              break;
            }
          }
        }
      }
    }

    private void HandlePropertyDescriptorChanged( PropertyDescriptor property )
    {
      // setting m_defaultItemProperties, will force the collection view to reanalyse the source to get a new list of default ItemProperties.
      m_defaultItemProperties = null;

      if( this.AutoCreateItemProperties )
      {
        if( property == null )
        {
          for( int i = m_itemProperties.Count - 1; i >= 0; i-- )
          {
            DataGridItemProperty oldItemProperty = m_itemProperties[ i ] as DataGridItemProperty;

            if( ( oldItemProperty != null ) && ( oldItemProperty.IsAutoCreated ) )
            {
              m_itemProperties.RemoveAt( i );
            }
          }

          foreach( DataGridItemPropertyBase itemProperty in this.GetDefaultItemProperties() )
          {
            this.AddValidItemProperty( itemProperty );
          }
        }
        else
        {
          string name = property.Name;
          DataGridItemProperty oldItemProperty = m_itemProperties[ name ] as DataGridItemProperty;

          if( ( oldItemProperty != null ) && ( oldItemProperty.IsAutoCreated ) )
          {
            // It is better to completely remove the item property and add a new one instead of changing is content.
            m_itemProperties.Remove( oldItemProperty );
            List<DataGridItemPropertyBase> defaultItemProperties = this.GetDefaultItemProperties();

            foreach( DataGridItemPropertyBase itemProperty in defaultItemProperties )
            {
              if( itemProperty.Name == name )
              {
                this.AddValidItemProperty( itemProperty );
                break;
              }
            }
          }
        }
      }
    }

    private void HandlePropertyDescriptorDeleted( PropertyDescriptor property )
    {
      // setting m_defaultItemProperties, will force the collection view to reanalyse the source to get a new list of default ItemProperties.
      m_defaultItemProperties = null;

      if( this.AutoCreateItemProperties )
      {
        string name = property.Name;
        DataGridItemProperty oldItemProperty = m_itemProperties[ name ] as DataGridItemProperty;

        if( ( oldItemProperty != null ) && ( oldItemProperty.IsAutoCreated ) )
        {
          m_itemProperties.Remove( oldItemProperty );
        }
      }
    }

    #endregion SOURCE COLLECTION CHANGED HANDLERS

    #region CHILD COLLECTION CHANGED HANDLERS

    private void ItemProperties_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      // For detail DataGridCollectionView, this is usually called the first time the DetailDescription is used to generate a DataGridCollectionView.

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Add:

          int newItemsCount = e.NewItems.Count;

          for( int i = 0; i < newItemsCount; i++ )
          {
            DataGridItemPropertyBase dataGridItemProperty = e.NewItems[ i ] as DataGridItemPropertyBase;
            Debug.Assert( dataGridItemProperty != null );

            INotifyCollectionChanged collectionChanged = this.GetAutoFilterValues( dataGridItemProperty.Name ) as INotifyCollectionChanged;

            if( collectionChanged != null )
            {
              collectionChanged.CollectionChanged += this.OnAutoFilterValuesChanged;
            }

            if( dataGridItemProperty.FilterCriterion != null )
            {
              this.AddFilteredItemProperty( dataGridItemProperty );
            }
          }
          break;

        case NotifyCollectionChangedAction.Remove:

          int oldItemsCount = e.OldItems.Count;

          using( this.DeferRefresh() )
          {
            for( int i = 0; i < oldItemsCount; i++ )
            {
              DataGridItemPropertyBase dataGridItemProperty = e.OldItems[ i ] as DataGridItemPropertyBase;
              Debug.Assert( dataGridItemProperty != null );

              string fieldName = dataGridItemProperty.Name;

              INotifyCollectionChanged collectionChanged = this.GetAutoFilterValues( fieldName ) as INotifyCollectionChanged;

              if( collectionChanged != null )
              {
                collectionChanged.CollectionChanged -= this.OnAutoFilterValuesChanged;
              }

              IList autoFilterValues = collectionChanged as IList;

              if( autoFilterValues != null )
              {
                autoFilterValues.Clear();
              }

              this.AutoFilteredDataGridItemProperties.Remove( dataGridItemProperty );

              // Clear DistinctValues for this item
              // NOTE: we use InternalTryGetValue instead of [ ] or TryGetValue to avoid creating the item inside the DistinctValuesDictionary
              ReadOnlyObservableHashList readOnlyObservableHashList = null;

              if( ( ( DistinctValuesDictionary )this.DistinctValues ).InternalTryGetValue( fieldName, out readOnlyObservableHashList ) )
              {
                IList distinctValues = readOnlyObservableHashList.InnerObservableHashList;

                if( distinctValues != null )
                {
                  distinctValues.Clear();
                }
              }

              if( dataGridItemProperty.FilterCriterion != null )
              {
                m_filteredItemProperties.Remove( dataGridItemProperty );
              }
            }
          }

          break;

        case NotifyCollectionChangedAction.Replace:
          int replaceItemsCount = e.NewItems.Count;

          for( int i = 0; i < replaceItemsCount; i++ )
          {
            // If necessary, remove the old ItemProperty and add the new one in the list
            // of ItemProperties having their FilterCriterion set.
            DataGridItemPropertyBase dataGridItemProperty = e.OldItems[ i ] as DataGridItemPropertyBase;
            Debug.Assert( dataGridItemProperty != null );

            if( dataGridItemProperty.FilterCriterion != null )
            {
              m_filteredItemProperties.Remove( dataGridItemProperty );
            }

            dataGridItemProperty = e.NewItems[ i ] as DataGridItemPropertyBase;
            Debug.Assert( dataGridItemProperty != null );

            if( dataGridItemProperty.FilterCriterion != null )
            {
              this.AddFilteredItemProperty( dataGridItemProperty );
            }
          }
          break;

        case NotifyCollectionChangedAction.Reset:

          this.UnregisterAutoFilterCollectionChanged();

          using( this.DeferRefresh() )
          {
            this.ResetDistinctValues();
            this.ResetAutoFilterValues();
            this.AutoFilteredDataGridItemProperties.Clear();
            m_filteredItemProperties.Clear();
          }

          break;
      }
    }

    private void OnAutoFilterValuesChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      this.OnProxyAutoFilterValuesChanged( this, e );

      IList autoFilterValues = sender as IList;

      Debug.Assert( autoFilterValues != null );
      string fieldName = this.GetFieldNameFromAutoFitlerValues( autoFilterValues );
      Debug.Assert( fieldName != null );
      Debug.Assert( m_autoFilterValues != null );
      Debug.Assert( m_autoFilterValues[ fieldName ] == autoFilterValues );

      DataGridItemPropertyBase dataGridItemProperty = this.ItemProperties[ fieldName ];
      Debug.Assert( dataGridItemProperty != null );

      ObservableCollection<DataGridItemPropertyBase> autoFilteredItemProperties = this.AutoFilteredDataGridItemProperties;
      Debug.Assert( autoFilteredItemProperties != null );

      // If we are filtered
      if( autoFilterValues.Count > 0 )
      {
        // If this is a newly filtered DataGridItemProperty
        if( !autoFilteredItemProperties.Contains( dataGridItemProperty ) )
        {
          autoFilteredItemProperties.Add( dataGridItemProperty );
        }
      }
      else
      {
        // There is no more filter values for this DataGridItemProperty, 
        // remove it from m_autoFilteredItems. 
        if( dataGridItemProperty != null )
        {
          autoFilteredItemProperties.Remove( dataGridItemProperty );
        }
      }

      if( this.AutoFilterMode != AutoFilterMode.None )
      {
        AutoFilterValuesChangedEventArgs autoFilterValuesChangedEventArgs = new AutoFilterValuesChangedEventArgs( m_parentDetailDescription, dataGridItemProperty, autoFilterValues, e );

        this.RaiseAutoFilterValuesChangedEvent( autoFilterValuesChangedEventArgs );
        m_excludedItemPropertyFromDistinctValueCalculation = dataGridItemProperty;

        try
        {
          // The refresh may be defered, if it's the case, m_excludedItemPropertyFromDistinctValueCalculation will 
          // not be considered.  This behavior is what we want since a defered refresh can mean aother operation
          // altered the items of the source so we want to rebuild the distinct value.
          this.Refresh();
        }
        finally
        {
          m_excludedItemPropertyFromDistinctValueCalculation = null;
        }
      }
    }

    private void OnSortDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      lock( this.SyncRoot )
      {
        lock( this.DeferredOperationManager )
        {
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
          this.OnProxyGroupDescriptionsChanged( this, e );

          this.ExecuteOrQueueSourceItemOperation( new DeferredOperation( DeferredOperation.DeferredOperationAction.Regroup, -1, null ) );
        }
      }
    }

    #endregion CHILD COLLECTION CHANGED HANDLERS

    #region PROXY EVENTS

    internal DataGridCollectionViewBase RootDataGridCollectionViewBase
    {
      get
      {
        return m_rootDataGridCollectionViewBase;
      }
    }

    internal event EventHandler ProxyApplyingFilterCriterias;

    private void OnProxyApplyingFilterCriterias( object sender, EventArgs e )
    {
      if( m_rootDataGridCollectionViewBase.ProxyApplyingFilterCriterias != null )
        m_rootDataGridCollectionViewBase.ProxyApplyingFilterCriterias( sender, e );
    }

    internal event NotifyCollectionChangedEventHandler ProxyAutoFilterValuesChanged;

    private void OnProxyAutoFilterValuesChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( m_rootDataGridCollectionViewBase.ProxyAutoFilterValuesChanged != null )
        m_rootDataGridCollectionViewBase.ProxyAutoFilterValuesChanged( sender, e );
    }

    internal event EventHandler ProxyCollectionRefresh;

    internal void OnProxyCollectionRefresh()
    {
      if( m_rootDataGridCollectionViewBase.ProxyCollectionRefresh != null )
        m_rootDataGridCollectionViewBase.ProxyCollectionRefresh( this, EventArgs.Empty );
    }

    internal event NotifyCollectionChangedEventHandler ProxySortDescriptionsChanged;

    internal void OnProxySortDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( m_rootDataGridCollectionViewBase.ProxySortDescriptionsChanged != null )
        m_rootDataGridCollectionViewBase.ProxySortDescriptionsChanged( sender, e );
    }

    internal event NotifyCollectionChangedEventHandler ProxyGroupDescriptionsChanged;

    internal void OnProxyGroupDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( m_rootDataGridCollectionViewBase.ProxyGroupDescriptionsChanged != null )
        m_rootDataGridCollectionViewBase.ProxyGroupDescriptionsChanged( sender, e );
    }

    #endregion PROXY EVENTS

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
      IEditableObject editableObject = ItemsSourceHelper.GetEditableObject( item );

      // editableObject can be an Xceed datarow when directly inserted as Items in the DataGridControl.
      if( ( editableObject != null ) && ( !( editableObject is Xceed.Wpf.DataGrid.DataRow ) ) )
      {
        // Keep a copy of the values, because if EndEdit throw ( for a DataView ), we loose the old values.
        DataGridItemPropertyCollection itemProperties = this.ItemProperties;
        int count = itemProperties.Count;
        object[] oldValueBeforeEndEdit = new object[ count ];

        for( int i = 0; i < count; i++ )
        {
          oldValueBeforeEndEdit[ i ] = itemProperties[ i ].GetValue( item );
        }

        try
        {
          editableObject.EndEdit();
          endEditCalled = true;
        }
        catch
        {
          // Since the DataView of MS cancel the edition when EndEdit is throwing, we ensure to restart the edition mode.
          editableObject.BeginEdit();

          itemProperties.SuspendUnboundItemPropertyChanged( item );

          try
          {
            // restore the oldvalues
            for( int i = 0; i < count; i++ )
            {
              DataGridItemPropertyBase itemProperty = itemProperties[ i ];

              bool readOnly = ( ( item == m_currentAddItem )
                              && itemProperty.OverrideReadOnlyForInsertion.HasValue
                              && itemProperty.OverrideReadOnlyForInsertion.Value )
                ? false
                : itemProperty.IsReadOnly;

              if( !readOnly )
                itemProperties[ i ].SetValue( item, oldValueBeforeEndEdit[ i ] );
            }
          }
          finally
          {
            itemProperties.ResumeUnboundItemPropertyChanged( item );
          }

          throw;
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
        this.EditCanceled( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnEditCanceled( e );
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

    #endregion EDIT PROCESS

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

    #endregion INSERTION PROCESS

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

    #endregion DELETION PROCESS

    #region MASTER DETAIL

    private static List<DataGridDetailDescription> EmptyDataGridDetailDescriptionList = new List<DataGridDetailDescription>();

    internal virtual bool CanHaveDetails
    {
      get
      {
        return false;
      }
    }

    private void DataGridDetailDescriptions_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( !this.CanHaveDetails )
        throw new InvalidOperationException( "An attempt was made to modify the DetailDescriptions of a " + this.GetType().Name + ", which does not support master detail." );

      // Set the AutoFilterValuesChanged event for each
      // newly added DataGridDetailDescription added to 
      // collection
      int newItemsCount = ( e.NewItems != null )
       ? e.NewItems.Count
       : 0;

      int oldItemsCount = ( e.OldItems != null )
      ? e.OldItems.Count
      : 0;

      for( int i = 0; i < oldItemsCount; i++ )
      {
        DataGridDetailDescription description = e.OldItems[ i ] as DataGridDetailDescription;

        if( description == null )
          continue;

        description.UnregisterAllAutoFilterValuesChangedEvent();
      }

      for( int i = 0; i < newItemsCount; i++ )
      {
        DataGridDetailDescription description = e.NewItems[ i ] as DataGridDetailDescription;

        if( description == null )
          continue;

        description.DetailDescriptionAutoFilterValuesChanged = this.RaiseAutoFilterValuesChangedEvent;
      }
    }

    internal DataGridDetailDescriptionCollection DetailDescriptions
    {
      get
      {
        Debug.Assert( m_dataGridDetailDescriptions != null );
        return m_dataGridDetailDescriptions;
      }
    }

    internal virtual List<DataGridDetailDescription> GetDefaultDetailDescriptions()
    {
      if( m_defaultDetailDescriptions == null )
      {
        if( this.CanCreateDetailDescriptions() )
        {
          m_defaultDetailDescriptions = this.CreateDetailDescriptions();

          // Ensure to Affect the AutoCreateForeignKeyDescription for every 
          // DataGridDetailDescription created
          foreach( DataGridDetailDescription description in m_defaultDetailDescriptions )
          {
            if( description.IsAutoCreated )
            {
              description.AutoCreateForeignKeyDescriptions = this.AutoCreateForeignKeyDescriptions;
            }
          }
        }
        else
        {
          return DataGridCollectionViewBase.EmptyDataGridDetailDescriptionList;
        }
      }

      return m_defaultDetailDescriptions;
    }

    internal virtual bool CanCreateDetailDescriptions()
    {
      return ( ( m_enumeration != null ) || ( m_itemType != typeof( object ) ) );
    }

    private List<DataGridDetailDescription> CreateDetailDescriptions()
    {
      //Source collection is a DataTable or a DataTable was detected by looking at the first item in the collection.
      if( m_dataTable != null )
        return this.CreateDetailDescriptionsForTable();

      if( typeof( XmlNode ).IsAssignableFrom( m_itemType ) )
        return new List<DataGridDetailDescription>();

      //Unbound mode, we do not support Master/Detail in this scenario.
      if( typeof( DataRow ).IsAssignableFrom( m_itemType ) )
        return new List<DataGridDetailDescription>();

      //we do not support Master/Details when Item is a Value type...
      if( ItemsSourceHelper.IsValueType( m_itemType ) )
        return new List<DataGridDetailDescription>();

      //Check if the object is a Entity Framework Entity, before checking for IEnumerable (since Entity Framework does have IEnumerable
      //properties, but require special handling )...
      if( ItemsSourceHelper.IsEntityFramework( m_itemType ) )
        return DataGridCollectionViewBase.CreateDetailDescriptionsForEntityFramework( m_itemType );

      //If the first item maps to an object that implements IEnumerable, expand that as a Relation ( and only that )...
      if( typeof( IEnumerable ).IsAssignableFrom( m_itemType ) )
        return DataGridCollectionViewBase.CreateDetailDescriptionsForEnumerable();

      if( typeof( IListSource ).IsAssignableFrom( m_itemType ) )
        return DataGridCollectionViewBase.CreateDetailDescriptionsForListSource();

      //If the Source collection implements ITypedList
      ITypedList typedList = m_enumeration as ITypedList;

      if( typedList != null )
        return DataGridCollectionViewBase.GetDataGridDetailDescriptions( typedList.GetItemProperties( null ) );

      TypeDescriptionProvider typeDescriptionProvider = TypeDescriptor.GetProvider( m_itemType );
      object firstItem = ItemsSourceHelper.GetFirstItemByEnumerable( m_enumeration );
      ICustomTypeDescriptor customTypeDescriptor = firstItem as ICustomTypeDescriptor;

      if( customTypeDescriptor == null )
        customTypeDescriptor = typeDescriptionProvider.GetTypeDescriptor( m_itemType, firstItem );

      if( customTypeDescriptor != null )
        return DataGridCollectionViewBase.GetDataGridDetailDescriptions( customTypeDescriptor.GetProperties() );

      return DataGridCollectionViewBase.GetDataGridDetailDescriptions( TypeDescriptor.GetProperties( m_itemType ) );
    }

    private static List<DataGridDetailDescription> CreateDetailDescriptionsForEnumerable()
    {
      List<DataGridDetailDescription> newList = new List<DataGridDetailDescription>( 1 );
      DataGridDetailDescription detailDesc = new EnumerableDetailDescription();
      detailDesc.IsAutoCreated = true;

      newList.Add( detailDesc );

      return newList;
    }

    private static List<DataGridDetailDescription> CreateDetailDescriptionsForListSource()
    {
      List<DataGridDetailDescription> newList = new List<DataGridDetailDescription>( 1 );
      ListSourceDetailDescription detailDesc = new ListSourceDetailDescription();
      detailDesc.IsAutoCreated = true;

      newList.Add( detailDesc );

      return newList;
    }

    private List<DataGridDetailDescription> CreateDetailDescriptionsForTable()
    {
      List<DataGridDetailDescription> relations = new List<DataGridDetailDescription>( m_dataTable.ChildRelations.Count );
      foreach( DataRelation relation in m_dataTable.ChildRelations )
      {
        DataRelationDetailDescription description = new DataRelationDetailDescription( relation );
        description.IsAutoCreated = true;
        relations.Add( description );
      }

      return relations;
    }

    private static List<DataGridDetailDescription> CreateDetailDescriptionsForEntityFramework( Type type )
    {
      List<DataGridDetailDescription> relations = new List<DataGridDetailDescription>();

      // Gets all the public properties of the type.
      PropertyInfo[] propertyInfos = type.GetProperties( BindingFlags.Instance | BindingFlags.Public );

      // Loop throught the properties to build up the detail descriptions.
      foreach( PropertyInfo propertyInfo in propertyInfos )
      {
        Type propertyType = propertyInfo.PropertyType;

        // The property must be of type RelatedEnd and IEnumerable to continue.

        if( ( propertyType.BaseType != null )
          && ( propertyType.BaseType.FullName == "System.Data.Objects.DataClasses.RelatedEnd" )
          && ( typeof( IEnumerable ).IsAssignableFrom( propertyInfo.PropertyType ) ) )
        {
          relations.Add( new EntityDetailDescription( propertyInfo.Name ) );
        }
      }

      return relations;
    }

    private static List<DataGridDetailDescription> GetDataGridDetailDescriptions( PropertyDescriptorCollection properties )
    {
      Type enumrableType = typeof( IEnumerable );
      Type listSourceType = typeof( IListSource );

      List<DataGridDetailDescription> detailDescriptions = new List<DataGridDetailDescription>( properties.Count );

      foreach( PropertyDescriptor property in properties )
      {
        // We only create details for properties that are browsable.
        if( !property.IsBrowsable )
          continue;

        if( ItemsSourceHelper.IsASubRelationship( property.PropertyType ) )
        {
          PropertyDetailDescription description = new PropertyDetailDescription( property );
          description.IsAutoCreated = true;
          detailDescriptions.Add( description );
        }
      }

      return detailDescriptions;
    }

    #endregion MASTER DETAIL

    #region AUTO FILTERING

    internal event EventHandler<AutoFilterValuesChangedEventArgs> AutoFilterValuesChanged;

    public event EventHandler DistinctValuesRefreshNeeded;

    internal AutoFilterMode AutoFilterMode
    {
      get
      {
        return m_autoFilterMode;
      }
      set
      {
        if( ( !this.CanAutoFilter ) && ( value != AutoFilterMode.None ) )
          throw new NotSupportedException( "This collection view does not support automatic filtering." );

        if( m_autoFilterMode != value )
        {
          m_autoFilterMode = value;

          if( value == AutoFilterMode.None )
            this.ResetDistinctValues();

          if( this.Loaded )
            this.Refresh();

          this.OnPropertyChanged( new PropertyChangedEventArgs( "AutoFilterMode" ) );
        }
      }
    }

    public virtual DistinctValuesConstraint DistinctValuesConstraint
    {
      get
      {
        return m_distinctValuesConstraint;
      }
      set
      {
        if( value != m_distinctValuesConstraint )
        {
          using( this.DeferRefresh() )
          {
            m_distinctValuesConstraint = value;

            this.RefreshDistinctValues( true );
          }
        }
      }
    }

    public DistinctValuesUpdateMode DistinctValuesUpdateMode
    {
      get
      {
        return m_distinctValueUpdateMode;
      }
      set
      {
        m_distinctValueUpdateMode = value;
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods" )]
    public IDictionary<string, ReadOnlyObservableHashList> DistinctValues
    {
      get
      {
        return m_distinctValues;
      }
    }

    internal IDictionary<string, IList> AutoFilterValues
    {
      get
      {
        return m_autoFilterValues;
      }
    }

    public ICollection GetDistinctValues( string fieldName )
    {
      ReadOnlyObservableHashList distinctValues = null;

      this.DistinctValues.TryGetValue( fieldName, out distinctValues );

      return distinctValues;
    }

    internal ObservableCollection<DataGridItemPropertyBase> AutoFilteredDataGridItemProperties
    {
      get
      {
        return m_autoFilteredDataGridItemProperties;
      }
    }

    internal static void RemoveUnusedDistinctValues( IComparer sortComparer, HashSet<object> currentValues, ObservableHashList cachedValues, object convertNullTo )
    {
      bool needsResort = false;
      List<int> itemsToDelete = new List<int>();
      int cachedValuesCount = cachedValues.Count;

      // Find no more existing values
      for( int i = 0; i < cachedValuesCount; i++ )
      {
        object cachedValue = cachedValues[ i ];

        if( cachedValue == convertNullTo )
        {
          cachedValue = null;
        }

        if( currentValues.Remove( cachedValue ) )
        {
          // remaining items will only be newly added ones
        }
        else
        {
          // Item is not currently in column, will be deleted
          itemsToDelete.Add( i );
        }
      }

      // Remove unused items
      int count = itemsToDelete.Count;

      for( int i = count - 1; i >= 0; i-- )
      {
        cachedValues.RemoveAt( itemsToDelete[ i ] );
      }

      // Every item not removed from the current values are new values, we add them to columnDistinctValues
      needsResort = ( currentValues.Count > 0 );

      foreach( object key in currentValues )
      {
        if( key == null )
        {
          cachedValues.Add( convertNullTo );
        }
        else
        {
          cachedValues.Add( key );
        }
      }

      if( needsResort )
        cachedValues.Sort( sortComparer );
    }

    internal void RaiseAutoFilterValuesChangedEvent( AutoFilterValuesChangedEventArgs e )
    {
      // This will not be raised for Details since the ParentCollectionViewSourceBase
      // is always null for a detail. The event will be raised by the 
      // DataGridDetailDescription.AutoFilterValuesChanged internal Action
      // to assert only 1 event is sent to the user event if there are more than
      // 1 DataGridContext expanded.
      if( this.AutoFilterValuesChanged != null )
        this.AutoFilterValuesChanged( this, e );

      if( m_parentCollectionViewSourceBase != null )
        m_parentCollectionViewSourceBase.OnAutoFilterValuesChanged( e );
    }

    internal void RaiseDistinctValuesRefreshNeeded()
    {
      //The AutoFilterControl is the primary "user" of this events, it will use it to know when to update the DistinctValues when its DropDown gets open.
      //However, users can subscribe to it, so they can update the DistinctValues when they use the AutoFilterControl outside the grid.
      if( this.DistinctValuesRefreshNeeded != null )
      {
        this.DistinctValuesRefreshNeeded( this, EventArgs.Empty );
      }

      if( m_parentCollectionViewSourceBase != null )
      {
        m_parentCollectionViewSourceBase.RaiseDistinctValuesRefreshNeeded();
      }
    }

    internal IList GetAutoFilterValues( string fieldName )
    {
      Debug.Assert( !string.IsNullOrEmpty( fieldName ), "fieldName can't be null" );

      if( fieldName == null )
        throw new ArgumentNullException( "dataGridItemProperty" );

      IList autoFilterValues;

      if( !m_autoFilterValues.TryGetValue( fieldName, out autoFilterValues ) )
      {
        ObservableHashList hashList = new ObservableHashList();
        autoFilterValues = hashList;
        m_autoFilterValues.InternalAdd( fieldName, hashList );

        // Notify the DetailDescription that it must listen to 
        // AutoFilterValues CollectionChanged for a specific
        // field name and autoFilterValues in order to raise
        // the AutoFilterValuesChanged properly
        if( m_parentDetailDescription != null )
          m_parentDetailDescription.RegisterAutoFilterValuesChangedEvent( fieldName, hashList );
      }

      return autoFilterValues;
    }

    internal void ForceRefreshDistinctValuesForFieldName( string fieldName, ObservableHashList columnDistinctValues )
    {
      if( this.AutoFilterMode == AutoFilterMode.None )
        return;

      // Avoid multiple refreshes caused by NotifyCollectionChanged events when adding or removing
      // values from distinct values lists
      if( this.IsRefreshingDistinctValues )
        throw new InvalidOperationException( "An attempt was made to refresh the DataGridCollectionView while it is already in the process of refreshing." );

      this.IsRefreshingDistinctValues = true;

      DataGridItemPropertyBase dataGridItemProperty = this.ItemProperties[ fieldName ];

      if( ( dataGridItemProperty == null ) || ( !dataGridItemProperty.CalculateDistinctValues ) )
      {
        this.IsRefreshingDistinctValues = false;
        return;
      }

      this.RefreshDistinctValuesForField( dataGridItemProperty );

      // Reset flags
      this.IsRefreshingDistinctValues = false;
    }

    internal abstract void RefreshDistinctValuesForField( DataGridItemPropertyBase dataGridItemProperty );

    internal void ForceRefreshDistinctValues( bool filteredItemsChanged )
    {
      if( this.AutoFilterMode == AutoFilterMode.None )
        return;

      // Avoid multiple refreshes caused by NotifyCollectionChanged events when adding or removing
      // values from distinct values lists
      if( this.IsRefreshingDistinctValues )
        throw new InvalidOperationException( "An attempt was made to refresh the DataGridCollectionView while it is already in the process of refreshing." );

      this.IsRefreshingDistinctValues = true;

      int dataGridItemPropertiesCount = m_itemProperties.Count;

      for( int propertyIndex = 0; propertyIndex < dataGridItemPropertiesCount; propertyIndex++ )
      {
        DataGridItemPropertyBase dataGridItemProperty = m_itemProperties[ propertyIndex ];

        if( !dataGridItemProperty.CalculateDistinctValues )
          continue;

        if( dataGridItemProperty == m_excludedItemPropertyFromDistinctValueCalculation )
          continue;

        // No modifications to filtered items, do not calculated Distinct Values for this DataGridItemProperty
        if( ( this.DistinctValuesConstraint == DistinctValuesConstraint.Filtered ) && ( !filteredItemsChanged ) )
          continue;

        this.RefreshDistinctValuesForField( dataGridItemProperty );
      }

      // Reset flags
      this.IsRefreshingDistinctValues = false;
    }

    internal void RefreshDistinctValues( bool filteredItemsChanged )
    {
      Debug.Assert( !( this is DataGridVirtualizingCollectionViewBase ) );

      this.EnsureThread();

      lock( this.SyncRoot )
      {
        lock( m_deferredOperationManager )
        {
          this.ExecuteOrQueueSourceItemOperation(
            new DeferredOperation(
              DeferredOperation.DeferredOperationAction.RefreshDistincValues,
              filteredItemsChanged ) );
        }
      }
    }

    private void ResetDistinctValues()
    {
      if( m_distinctValues == null )
        return;

      foreach( DataGridItemPropertyBase dataGridItemProperty in this.ItemProperties )
      {
        ReadOnlyObservableHashList readOnlyList = null;

        // Not all field will be initialized
        if( ( ( DistinctValuesDictionary )this.DistinctValues ).InternalTryGetValue( dataGridItemProperty.Name, out readOnlyList ) )
        {
          if( readOnlyList != null )
          {
            IList values = readOnlyList.InnerObservableHashList;

            if( values != null )
              values.Clear();
          }
        }
      }
    }

    private void ResetAutoFilterValues()
    {
      if( m_autoFilterValues == null )
        return;

      foreach( IList values in m_autoFilterValues.Values )
      {
        if( values != null )
          values.Clear();
      }
    }

    private string GetFieldNameFromAutoFitlerValues( IList targetAutoFilterValues )
    {
      IDictionary<string, IList> autoFilterValues = this.AutoFilterValues;

      foreach( string fieldName in autoFilterValues.Keys )
      {
        if( autoFilterValues[ fieldName ] == targetAutoFilterValues )
          return fieldName;
      }

      return null;
    }

    #endregion AUTO FILTERING

    #region FILTER CRITERION

    public FilterCriteriaMode FilterCriteriaMode
    {
      get
      {
        return m_filterCriteriaMode;
      }

      set
      {
        if( value != m_filterCriteriaMode )
        {
          m_filterCriteriaMode = value;
          this.ReapplyFilterCriterion();
          this.OnPropertyChanged( new PropertyChangedEventArgs( "FilterCriteriaMode" ) );
        }
      }
    }

    internal ReadOnlyCollection<DataGridItemPropertyBase> FilteredCriterionItemProperties
    {
      get
      {
        return m_readOnlyFilteredItemProperties;
      }
    }

    // We use the FilterCriterionChanged event instead of the generic PropertyChanged 
    // event to be more efficient and be advised if any of the FilterCriterion hierarchy
    // sub-property changes.
    private void ItemProperties_FilterCriterionChanged( object sender, EventArgs e )
    {
      this.ReapplyFilterCriterion();
    }

    private void ReapplyFilterCriterion()
    {
      this.OnProxyApplyingFilterCriterias( this, EventArgs.Empty );

      m_filteredItemProperties.Clear();

      foreach( DataGridItemPropertyBase itemProperty in this.ItemProperties )
      {
        if( itemProperty.FilterCriterion != null )
        {
          this.AddFilteredItemProperty( itemProperty );
        }
      }

      this.Refresh();
    }

    private void AddFilteredItemProperty( DataGridItemPropertyBase itemProperty )
    {
      m_filteredItemProperties.Add( itemProperty );
    }

    #endregion FILTER CRITERION

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

    #endregion CURRENCY MANAGEMENT

    #region DEFERRED OPERATIONS HANDLING

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
        lock( m_deferredOperationManager )
        {
          return
            ( !this.Loaded )
            || ( m_deferRefreshCount != 0 )
            || ( m_deferredOperationManager.HasPendingOperations )
            || ( !this.CheckAccess() );
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

      if( ( this.ShouldDeferOperation ) || ( queueOperationForPendingAddNew ) )
      {
        if( queueOperationForPendingAddNew )
        {
          this.AddDeferredOperationForAddNew( deferredOperation );
        }
        else
        {
          this.AddDeferredOperation( deferredOperation );
        }
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

    #endregion DEFERRED OPERATIONS HANDLING

    internal virtual void ProcessInvalidatedGroupStats( List<DataGridCollectionViewGroup> invalidatedGroups, bool ensurePosition )
    {
    }

    internal virtual bool CanCreateItemProperties()
    {
      return ( ( m_enumeration != null ) || ( m_itemType != typeof( object ) ) || this.CanCreateItemPropertiesFromModelSource );
    }

    internal virtual void CreateDefaultCollections( DataGridDetailDescription parentDetailDescription )
    {
      Debug.Assert( parentDetailDescription == null || this.CanHaveDetails );

      m_sortDescriptions = ( parentDetailDescription == null ) ? new DataGridSortDescriptionCollection() :
        parentDetailDescription.SortDescriptions as DataGridSortDescriptionCollection;

      m_groupDescriptions = ( parentDetailDescription == null ) ? new GroupDescriptionCollection() :
        parentDetailDescription.GroupDescriptions;
    }

    internal virtual void RegisterCollectionChanged()
    {
      ( ( INotifyCollectionChanged )m_sortDescriptions ).CollectionChanged +=
        new NotifyCollectionChangedEventHandler( this.OnSortDescriptionsChanged );

      ( ( INotifyCollectionChanged )m_groupDescriptions ).CollectionChanged +=
        new NotifyCollectionChangedEventHandler( this.OnGroupDescriptionsChanged );
    }

    internal virtual void UnregisterChangedEvent()
    {
      ( ( INotifyCollectionChanged )m_sortDescriptions ).CollectionChanged -= this.OnSortDescriptionsChanged;
      ( ( INotifyCollectionChanged )m_groupDescriptions ).CollectionChanged -= this.OnGroupDescriptionsChanged;

      m_itemProperties.CollectionChanged -= this.ItemProperties_CollectionChanged;
      m_itemProperties.FilterCriterionChanged -= this.ItemProperties_FilterCriterionChanged;

      this.UnregisterAutoFilterCollectionChanged();
    }

    internal bool IsSourceSupportingChangeNotification
    {
      get
      {
        return ItemsSourceHelper.IsSourceSupportingChangeNotification( m_enumeration );
      }
    }

    private void SetupDefaultDetailDescriptions()
    {
      if( this.CanHaveDetails )
      {
        List<DataGridDetailDescription> defaultDetailDescriptions = this.GetDefaultDetailDescriptions();

        if( this.AutoCreateDetailDescriptions )
        {
          m_dataGridDetailDescriptions = new DataGridDetailDescriptionCollection( defaultDetailDescriptions );
        }
        else
        {
          m_dataGridDetailDescriptions = new DataGridDetailDescriptionCollection( new List<DataGridDetailDescription>() );
        }

        m_dataGridDetailDescriptions.DefaultDetailDescriptions = defaultDetailDescriptions;
      }
      else
      {
        // We still create the DetailDescriptions collection but will validate on any modification that the collection view does
        // in fact support master/detail.
        m_dataGridDetailDescriptions = new DataGridDetailDescriptionCollection( new List<DataGridDetailDescription>() );
      }

      CollectionChangedEventManager.AddListener( m_dataGridDetailDescriptions, this );
    }

    private void UnregisterAutoFilterCollectionChanged()
    {
      if( m_autoFilterValues != null )
      {
        foreach( INotifyCollectionChanged notifyCollectionChanged in m_autoFilterValues.Values )
        {
          notifyCollectionChanged.CollectionChanged -= this.OnAutoFilterValuesChanged;
        }
      }
    }

    private void InitializeFromExternalDeclaration( IEnumerable collection, Type itemType )
    {
      Debug.Assert( m_parentDetailDescription.ItemProperties != null );
      Debug.Assert( m_parentDetailDescription.DetailDescriptions != null );
      Debug.Assert( m_parentDetailDescription.StatFunctions != null );
      Debug.Assert( m_parentDetailDescription.SortDescriptions != null );
      Debug.Assert( m_parentDetailDescription.GroupDescriptions != null );
      Debug.Assert( itemType != null );
      Debug.Assert( m_parentDetailDescription.AutoFilterValues != null );
      Debug.Assert( m_parentDetailDescription.AutoFilteredItems != null );

      m_itemProperties = m_parentDetailDescription.ItemProperties;

      //Ensure that the default item properties and the item type is set on the ItemProperties collection
      if( m_itemProperties.ItemType == null )
        m_itemProperties.ItemType = itemType;

      m_dataGridDetailDescriptions = m_parentDetailDescription.DetailDescriptions;

      this.CreateDefaultCollections( m_parentDetailDescription );

      m_autoFilterMode = m_parentDetailDescription.AutoFilterMode;
      m_autoFilterValues = m_parentDetailDescription.AutoFilterValues as ReadOnlyDictionary<string, IList>;
      m_autoFilteredDataGridItemProperties = m_parentDetailDescription.AutoFilteredItems;
      m_distinctValuesConstraint = m_parentDetailDescription.DistinctValuesConstraint;
      m_filterCriteriaMode = m_parentDetailDescription.FilterCriteriaMode;

      //Register to the change notifications for the Group/Sort descriptions collections
      this.RegisterCollectionChanged();

      // - then call the SetSource, with the source and the itemType...
      this.SetSource( null, collection, itemType, false );


      //cross-initialize the DefaultDetailDescriptions of the DataGridCollectionView and/or DataGridDetailDescriptionCollection
      if( m_dataGridDetailDescriptions.DefaultDetailDescriptions != null )
      {
        m_defaultDetailDescriptions = new List<DataGridDetailDescription>( m_dataGridDetailDescriptions.DefaultDetailDescriptions );
      }
      else
      {
        m_dataGridDetailDescriptions.DefaultDetailDescriptions = new List<DataGridDetailDescription>( this.GetDefaultDetailDescriptions() );
      }

      //cross-initialize the DefaultItemProperties of the DataGridCollectionView and/or DataGridItemPropertyCollection
      if( m_itemProperties.DefaultItemProperties != null )
      {
        m_defaultItemProperties = new List<DataGridItemPropertyBase>( m_itemProperties.DefaultItemProperties );
      }
      else
      {
        m_itemProperties.DefaultItemProperties = new List<DataGridItemPropertyBase>( this.GetDefaultItemProperties() );
      }

      // Creates the item properties if auto-creation is ON no one already created them.
      if( ( this.AutoCreateItemProperties )
        && ( !m_parentDetailDescription.AutoCreateItemPropertiesCompleted ) )
      {
        foreach( DataGridItemPropertyBase itemProperty in m_defaultItemProperties )
        {
          //  
          if( !itemProperty.Browsable )
            continue;

          if( itemProperty.IsASubRelationship )
            continue;

          if( m_itemProperties[ itemProperty.Name ] == null )
          {
            m_itemProperties.Add( itemProperty );
          }
        }

        m_parentDetailDescription.AutoCreateItemPropertiesCompleted = true;
      }

      // List for ItemProperties changes to be able to refresh when AutoFilterValues changes
      m_itemProperties.CollectionChanged += this.ItemProperties_CollectionChanged;
      m_itemProperties.FilterCriterionChanged += this.ItemProperties_FilterCriterionChanged;

      // Register CollectionChanged for AutoFiltering on every DataGridItemProperties 
      // already in ItemProperties since they are already defined
      foreach( DataGridItemPropertyBase dataGridItemProperty in m_itemProperties )
      {
        INotifyCollectionChanged collectionChanged = this.GetAutoFilterValues( dataGridItemProperty.Name ) as INotifyCollectionChanged;

        if( collectionChanged != null )
          collectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler( this.OnAutoFilterValuesChanged );
      }

      // Creates the detail descriptions if auto-creation is ON no one already created them.
      if( ( this.AutoCreateDetailDescriptions )
        && ( !m_parentDetailDescription.AutoCreateDetailDescriptionsCompleted ) )
      {
        List<DataGridDetailDescription> defaultDetailDescriptions = this.GetDefaultDetailDescriptions();

        foreach( DataGridDetailDescription detailDescription in defaultDetailDescriptions )
        {
          if( m_dataGridDetailDescriptions[ detailDescription.RelationName ] == null )
            m_dataGridDetailDescriptions.Add( detailDescription );
        }

        m_parentDetailDescription.AutoCreateDetailDescriptionsCompleted = true;
      }

      m_filteredItemProperties.Clear();

      foreach( DataGridItemPropertyBase itemProperty in m_itemProperties )
      {
        // Set default value for CalculateDistinctValues if not explicitly set
        if( !itemProperty.IsCalculateDistinctValuesInitialized )
          itemProperty.CalculateDistinctValues = m_parentDetailDescription.DefaultCalculateDistinctValues;

        if( itemProperty.FilterCriterion != null )
        {
          this.AddFilteredItemProperty( itemProperty );
        }
      }

      // We must manually add AutoFilterDataGridItemProperties if some AutoFilterValues were specified a the DataGridDetailDescription
      // since those are used to know in which order filter were applied. Normally, those DataGridItemProperties are
      // added in OnAutoFilterValuesChanged because the
      if( m_autoFilterValues != null )
      {
        foreach( string key in m_autoFilterValues.Keys )
        {
          IList autoFilterValues = m_autoFilterValues[ key ];

          if( ( autoFilterValues != null ) && ( autoFilterValues.Count > 0 ) )
          {
            DataGridItemPropertyBase dataGridItemProperty = m_itemProperties[ key ];

            if( dataGridItemProperty != null )
            {
              if( m_autoFilteredDataGridItemProperties.Contains( dataGridItemProperty ) == false )
                m_autoFilteredDataGridItemProperties.Add( dataGridItemProperty );
            }
          }
        }
      }
    }

    private void SetupDefaultItemProperties()
    {
      m_itemProperties = new DataGridItemPropertyCollection();
      m_itemProperties.ItemType = m_itemType;

      // Listen for ItemProperties changes to be able to refresh when AutoFilterValues changes
      m_itemProperties.CollectionChanged += this.ItemProperties_CollectionChanged;
      m_itemProperties.FilterCriterionChanged += this.ItemProperties_FilterCriterionChanged;

      // Calling GetDefaultItemProperties will also cache the result in m_defaultItemProperties
      // if able to detect properties
      List<DataGridItemPropertyBase> defaultItemProperties = this.GetDefaultItemProperties();

      if( this.AutoCreateItemProperties )
      {
        // Add new auto created item properties
        foreach( DataGridItemPropertyBase defaultProperty in defaultItemProperties )
        {
          this.AddValidItemProperty( defaultProperty );
        }
      }

      if( ( m_itemProperties.DefaultItemProperties == null ) && ( m_defaultItemProperties != null ) )
        m_itemProperties.DefaultItemProperties = new List<DataGridItemPropertyBase>( m_defaultItemProperties );

      //no need to cycle through the content of the ItemProperties collection to update them since
      //they originate from the default anyway.
    }

    private void AddValidItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( !itemProperty.Browsable )
        return;

      if( itemProperty.IsASubRelationship )
        return;

      if( m_itemProperties[ itemProperty.Name ] != null )
        return;

      m_itemProperties.Add( itemProperty );
    }

    private List<DataGridItemPropertyBase> GetDefaultItemProperties()
    {
      if( m_defaultItemProperties == null )
      {
        if( this.CanCreateItemProperties() )
        {
          m_defaultItemProperties = this.CreateItemProperties();
        }
        else
        {
          return DataGridCollectionViewBase.EmptyDataGridItemPropertyList;
        }
      }

      return m_defaultItemProperties;
    }

    private List<DataGridItemPropertyBase> CreateItemProperties()
    {
      List<DataGridItemPropertyBase> itemProperties = null;

      // That call will not always return existing property
      // PropertyDescriptor, with an equivalent of a PropertyPath in is name, are also created for each sub index of a JaggedArray for example
      this.ItemPropertiesCreated = true;

      DataTable modelDataTable = ( m_dataTable != null ) ? m_dataTable : m_modelSource as DataTable;

      if( modelDataTable != null )
      {
        itemProperties = this.CreateItemPropertiesForTable();
      }
      else if( typeof( DataRow ).IsAssignableFrom( m_itemType ) )
      {
        itemProperties = new List<DataGridItemPropertyBase>();
      }

      if( itemProperties == null )
      {
        Dictionary<string, ItemsSourceHelper.FieldDescriptor> fieldDescriptors = ItemsSourceHelper.GetFields( m_enumeration, m_itemType );
        itemProperties = DataGridCollectionViewBase.GetDataGridItemProperties( fieldDescriptors );
      }

      return itemProperties;
    }

    private List<DataGridItemPropertyBase> CreateItemPropertiesForTable()
    {
      DataTable modelDataTable = ( m_dataTable != null ) ? m_dataTable : m_modelSource as DataTable;

      Debug.Assert( modelDataTable != null,
        "This method should not have been called in the first place if both m_dataTable and m_sourceSchema are null." );


      DataColumnCollection columns = modelDataTable.Columns;
      int columnCount = columns.Count;
      List<DataGridItemPropertyBase> properties = new List<DataGridItemPropertyBase>( columnCount );

      for( int i = 0; i < columnCount; i++ )
      {
        DataColumn column = columns[ i ];

        DataGridItemProperty property = new DataGridItemProperty(
          new DataRowColumnPropertyDescriptor( column ), true );

        property.Title = column.Caption;
        properties.Add( property );
      }

      return properties;
    }

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        if( sender is DataGridDetailDescriptionCollection )
        {
          this.DataGridDetailDescriptions_CollectionChanged( sender, ( NotifyCollectionChangedEventArgs )e );
        }
        else
        {
          this.NotifyCollectionChanged_CollectionChanged( sender, ( NotifyCollectionChangedEventArgs )e );
        }

        return true;
      }

      if( managerType == typeof( ListChangedEventManager ) )
      {
        this.BindingList_ListChanged( sender, ( ListChangedEventArgs )e );
        return true;
      }

      return false;
    }

    #endregion IWeakEventListener Members

    #region ICollectionView MEMBERS

    public override IDisposable DeferRefresh()
    {
      return new DataGridCollectionView.DeferRefreshHelper( this );
    }

    public override void Refresh()
    {
      this.EnsureThread();

      // We are not calling ShouldDeferOperation, we only want to defer operation if we are
      // in defer refresh.
      if( m_deferRefreshCount != 0 )
      {
        this.AddDeferredOperation( new DeferredOperation(
          DeferredOperation.DeferredOperationAction.Refresh, -1, null ) );
      }
      else
      {
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

    internal virtual bool CanAutoFilter
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

    #endregion ICollectionView MEMBERS

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

          this.SetCurrentAddNew(
            newItem,
            newItemIndex );
        }

        DataGridItemEventArgs initializingNewItemEventArgs = new DataGridItemEventArgs( this, m_currentAddItem );
        this.RootDataGridCollectionViewBase.OnInitializingNewItem( initializingNewItemEventArgs );
      }
      catch
      {
        // QueueOperationForAddNew must be off when calling CancelNew
        this.QueueOperationForAddNew = false;

        if( m_currentAddItem != null )
          this.CancelNew();

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
        DataGridCommittingNewItemEventArgs committingNewItemEventArgs =
          new DataGridCommittingNewItemEventArgs( this, m_currentAddItem, false );

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

          addOperationToNotifyIfNotAlreadyQueued = new DeferredOperation(
            DeferredOperation.DeferredOperationAction.Add,
            addItemPosition + 1, addItemPosition,
            new object[] { m_currentAddItem } );
        }
        else
        {
          if( committingNewItemEventArgs.Index == -1 )
            throw new InvalidOperationException( "An attempt was made to handle the CommittingNewItem event without providing the index at which the new item was inserted." );

          if( committingNewItemEventArgs.NewCount == -1 )
            throw new InvalidOperationException( "An attempt was made to handle the CommittingNewItem event without providing the new item count." );

          if( committingNewItemEventArgs.Index >= committingNewItemEventArgs.NewCount )
            throw new InvalidOperationException( "The index at which the new item was inserted was greater than the new item count." );

          addOperationToNotifyIfNotAlreadyQueued = new DeferredOperation(
            DeferredOperation.DeferredOperationAction.Add,
            committingNewItemEventArgs.NewCount, committingNewItemEventArgs.Index,
            new object[] { m_currentAddItem } );
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

        // No pending operation was in m_deferredAddNewOperationManager,
        // so the list have not trigger notification for the new item added.
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

          ItemsSourceHelper.CancelNewDataItem(
            this.Enumeration,
            null,
            m_currentAddItem,
            m_currentAddItemPosition );
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

      DataGridItemCancelEventArgs itemCancelEventArgs = new DataGridItemCancelEventArgs( this, item, false );
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
          DataGridItemPropertyCollection itemProperties = this.ItemProperties;
          int count = itemProperties.Count;
          m_oldValuesBeforeEdition = new object[ count ];

          for( int i = 0; i < count; i++ )
          {
            m_oldValuesBeforeEdition[ i ] = itemProperties[ i ].GetValue( item );
          }
        }
      }

      m_currentEditItem = item;
      DataGridItemEventArgs itemEventArgs = new DataGridItemEventArgs( this, item );
      m_rootDataGridCollectionViewBase.OnEditBegun( itemEventArgs );
    }

    public virtual void CommitEdit()
    {
      if( m_currentEditItem == null )
        return;

      DataGridItemCancelEventArgs itemCancelEventArgs = new DataGridItemCancelEventArgs( this, m_currentEditItem, false );
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

      DataGridItemEventArgs itemEventArgs = new DataGridItemEventArgs( this, m_currentEditItem );
      int index = this.IndexOfSourceItem( m_currentEditItem );
      Debug.Assert( index != -1 );

      object[] items = new object[] { m_currentEditItem };
      m_currentEditItem = null;

      // Only execute or queue a replace operation if the item is not already queued for a remove operation.
      if( !this.DeferredOperationManager.ContainsItemForRemoveOperation( items[ 0 ] ) )
      {
        this.ExecuteOrQueueSourceItemOperation( new DeferredOperation(
          DeferredOperation.DeferredOperationAction.Replace,
          -1, index, items, index, items ) );
      }

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

      DataGridItemHandledEventArgs itemHandleEventArgs = new DataGridItemHandledEventArgs( this, m_currentEditItem );
      m_rootDataGridCollectionViewBase.OnCancelingEdit( itemHandleEventArgs );

      if( itemHandleEventArgs.Handled != this.ItemEditionIsManuallyHandled )
        throw new InvalidOperationException( "An attempt was made to manually handle the edit prcess without handling the BeginningEdit, CommittingEdit, and CancelingEdit events." );

      if( !itemHandleEventArgs.Handled )
      {
        bool cancelEditCalled;
        this.CancelEditInternal( m_currentEditItem, out cancelEditCalled );

        DataGridItemPropertyCollection itemProperties = this.ItemProperties;
        int count = itemProperties.Count;

        if( ( !cancelEditCalled ) && ( count > 0 ) )
        {
          itemProperties.SuspendUnboundItemPropertyChanged( m_currentEditItem );

          try
          {
            for( int i = 0; i < count; i++ )
            {
              DataGridItemPropertyBase itemProperty = itemProperties[ i ];

              bool readOnly = ( ( m_currentEditItem == m_currentAddItem )
                              && itemProperty.OverrideReadOnlyForInsertion.HasValue
                              && itemProperty.OverrideReadOnlyForInsertion.Value )
                ? false
                : itemProperty.IsReadOnly;

              if( !readOnly )
              {
                try
                {
                  itemProperty.SetValue( m_currentEditItem, m_oldValuesBeforeEdition[ i ] );
                }
                catch
                {
                  // Swallow any Exception, setting a property to is old value should not throw.
                }
              }
            }
          }
          finally
          {
            itemProperties.ResumeUnboundItemPropertyChanged( m_currentEditItem );
          }
        }
      }

      DataGridItemEventArgs itemEventArgs = new DataGridItemEventArgs( this, m_currentEditItem );
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

    #endregion IEditableCollectionView

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
      List<ItemPropertyInfo> itemPropertyInfos = new List<ItemPropertyInfo>();

      int itemPropertyCount = m_itemProperties.Count;

      for( int i = 0; i < itemPropertyCount; i++ )
      {
        DataGridItemPropertyBase dataGridItemPropertyBase = m_itemProperties[ i ];

        itemPropertyInfos.Add(
          new ItemPropertyInfo(
            dataGridItemPropertyBase.Name,
            dataGridItemPropertyBase.DataType,
            dataGridItemPropertyBase.GetPropertyDescriptorForBinding() ) );
      }

      return itemPropertyInfos;
    }

    #endregion

    private BitVector32 m_flags = new BitVector32();

    private DeferredOperationManager m_deferredOperationManager;
    private DeferredOperationManager m_deferredAddNewOperationManager;

    private int m_deferRefreshCount;

    private DataGridCollectionViewBase m_rootDataGridCollectionViewBase;

    private Type m_itemType;
    private Type m_desiredItemType;
    private object m_modelSource;
    private IEnumerable m_enumeration;
    private INotifyCollectionChanged m_notifyCollectionChanged;
    private DataTable m_dataTable;

    private int m_currentPosition;
    private object m_currentItem;
    private int m_deferCurrencyEventCount;

    private object m_currentEditItem;
    private object[] m_oldValuesBeforeEdition;

    private object m_currentAddItem;
    private int m_currentAddItemPosition = -1;

    private AutoFilterMode m_autoFilterMode = AutoFilterMode.None;
    private DistinctValuesConstraint m_distinctValuesConstraint = DistinctValuesConstraint.All;
    private DistinctValuesUpdateMode m_distinctValueUpdateMode = DistinctValuesUpdateMode.Manual;

    private FilterCriteriaMode m_filterCriteriaMode = FilterCriteriaMode.And;
    private List<DataGridItemPropertyBase> m_filteredItemProperties;
    private ReadOnlyCollection<DataGridItemPropertyBase> m_readOnlyFilteredItemProperties;

    private DataGridDetailDescription m_parentDetailDescription;

    private DataGridItemPropertyCollection m_itemProperties;
    private List<DataGridItemPropertyBase> m_defaultItemProperties;

    private DataGridDetailDescriptionCollection m_dataGridDetailDescriptions;
    private List<DataGridDetailDescription> m_defaultDetailDescriptions;

    private DataGridSortDescriptionCollection m_sortDescriptions;
    private ObservableCollection<GroupDescription> m_groupDescriptions;

    private CollectionViewGroup m_rootGroup;

    private Predicate<object> m_filter;

    // Containing a DataGridItemProperty key, ObservableCollection<object> values map
    private DistinctValuesDictionary m_distinctValues;
    private ReadOnlyDictionary<string, IList> m_autoFilterValues;
    private ObservableCollection<DataGridItemPropertyBase> m_autoFilteredDataGridItemProperties;
    private DataGridItemPropertyBase m_excludedItemPropertyFromDistinctValueCalculation;

    [Flags]
    private enum DataGridCollectionViewBaseFlags
    {
      Loaded = 1,
      ItemPropertiesCreated = 2,
      Refreshing = 4,
      QueueOperationForAddNew = 16,
      IsRefreshingDistinctValues = 32,
      IsCurrentAfterLast = 64,
      IsCurrentBeforeFirst = 128,
      CreatingNewItemIsManuallyHandled = 256,
      ItemEditionIsManuallyHandled = 512,
      AutoCreateItemProperties = 1024,
      AutoCreateDetailDescriptions = 2048,
      AutoCreateForeignKeyDescriptions = 4096,
    }

    #region Private Class DeferRefreshHelper

    private sealed class DeferRefreshHelper : IDisposable
    {
      public DeferRefreshHelper( DataGridCollectionViewBase collectionView )
      {
        m_collectionView = collectionView;
        collectionView.m_deferRefreshCount++;
      }

      public void Dispose()
      {
        if( m_collectionView == null )
          return;

        m_collectionView.m_deferRefreshCount--;

        if( m_collectionView.CheckAccess() )
        {
          this.ProcessDispose();
        }
        else
        {
          //In case Dispose is called from a different thread, make sure the refresh is not lost!
          m_collectionView.Dispatcher.BeginInvoke( DispatcherPriority.Send, new Action( this.ProcessDispose ) );
        }
      }

      private void ProcessDispose()
      {
        if( m_collectionView.Loaded )
        {
          m_collectionView.m_deferredOperationManager.Process();
        }
        else
        {
          // We call ForceRefresh when not yet "Loaded" because we want the Dispose here to
          // triger the CollectionChanged( NotifyCollectionChangedAction.Reset )
          // That is needed because of the way the ItemsControl, other than our grid, work.
          m_collectionView.ForceRefresh( true, true, true );
        }

        m_collectionView = null;
      }

      private DataGridCollectionViewBase m_collectionView;
    }

    #endregion Private Class DeferRefreshHelper

    #region Private Class DeferCurrencyEventHelper

    private sealed class DeferCurrencyEventHelper : IDisposable
    {
      public DeferCurrencyEventHelper( DataGridCollectionViewBase collectionView )
      {
        m_collectionView = collectionView;
        m_oldCurrentItem = m_collectionView.CurrentItem;
        m_oldCurrentPosition = m_collectionView.CurrentPosition;
        m_oldIsCurrentAfterLast = m_collectionView.IsCurrentAfterLast;
        m_oldIsCurrentBeforeFirst = m_collectionView.IsCurrentBeforeFirst;
        m_collectionView.m_deferCurrencyEventCount++;
      }

      public void Dispose()
      {
        if( m_collectionView == null )
          return;

        m_collectionView.m_deferCurrencyEventCount--;

        if( m_collectionView.m_deferCurrencyEventCount == 0 )
        {
          bool itemChanged = false;

          if( !object.Equals( m_oldCurrentItem, m_collectionView.CurrentItem ) )
          {
            itemChanged = true;
            m_collectionView.OnPropertyChanged( new PropertyChangedEventArgs( "CurrentItem" ) );
          }

          if( m_oldCurrentPosition != m_collectionView.CurrentPosition )
          {
            itemChanged = true;
            m_collectionView.OnPropertyChanged( new PropertyChangedEventArgs( "CurrentPosition" ) );
          }

          if( m_oldIsCurrentBeforeFirst != m_collectionView.IsCurrentBeforeFirst )
          {
            itemChanged = true;
            m_collectionView.OnPropertyChanged( new PropertyChangedEventArgs( "IsCurrentBeforeFirst" ) );
          }

          if( m_oldIsCurrentAfterLast != m_collectionView.IsCurrentAfterLast )
          {
            itemChanged = true;
            m_collectionView.OnPropertyChanged( new PropertyChangedEventArgs( "IsCurrentAfterLast" ) );
          }

          if( itemChanged )
            m_collectionView.OnCurrentChanged();
        }

        m_collectionView = null;
      }

      private int m_oldCurrentPosition;
      private object m_oldCurrentItem;
      private bool m_oldIsCurrentAfterLast;
      private bool m_oldIsCurrentBeforeFirst;
      private DataGridCollectionViewBase m_collectionView;
    }

    #endregion Private Class DeferCurrencyEventHelper
  }
}
