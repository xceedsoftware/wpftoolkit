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
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xceed.Wpf.DataGrid.Stats;
using System.Data;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using Xceed.Utils.Collections;
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class DataGridDetailDescription : DependencyObject, IWeakEventListener
  {
    #region Constructor

    protected DataGridDetailDescription()
    {
      m_detailDescriptions = new DataGridDetailDescriptionCollection();
      m_detailDescriptions.CollectionChanged += this.OnDetailDescriptionsCollectionChanged;

      m_itemProperties = new DataGridItemPropertyCollection();
      m_itemProperties.CollectionChanged += this.OnItemPropertiesCollectionChanged;

      m_groupDescriptions = new GroupDescriptionCollection();
      m_sortDescriptions = new DataGridSortDescriptionCollection();
      m_statFunctions = new StatFunctionCollection();
      m_autoFilterValues = new ReadOnlyDictionary<string, IList>();
      m_autoFilteredItems = new ObservableCollection<DataGridItemPropertyBase>();
      m_registeredFieldNamesToAutoFilterValues = new Dictionary<string, INotifyCollectionChanged>();
      m_registeredAutoFilterValuesToFieldNames = new Dictionary<INotifyCollectionChanged, string>();

      this.AutoCreateDetailDescriptions = true;
      this.AutoCreateItemProperties = true;
      this.DefaultCalculateDistinctValues = true;
    }

    #endregion

    #region RelationName Public Property

    public string RelationName
    {
      get
      {
        return m_relationName;
      }
      set
      {
        if( this.InternalIsSealed == true )
          throw new InvalidOperationException( "An attempt was made to change the RelationName property after the DataGridDetailDescription has been sealed." );

        m_relationName = value;
      }
    }

    #endregion

    #region Title Public Property

    public object Title
    {
      get
      {
        return m_title;
      }
      set
      {
        m_title = value;
      }
    }

    #endregion

    #region TitleTemplate Public Property

    public DataTemplate TitleTemplate
    {
      get
      {
        return m_titleTemplate;
      }
      set
      {
        m_titleTemplate = value;
      }
    }

    #endregion

    #region AutoCreateItemProperty Public Property

    public bool AutoCreateItemProperties
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateItemProperties ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateItemProperties ] = value;
      }
    }

    #endregion

    #region AutoCreateDetailDescriptions Public Property

    public bool AutoCreateDetailDescriptions
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateDetailDescriptions ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateDetailDescriptions ] = value;
      }
    }

    #endregion

    #region AutoCreateForeignKeyDescriptions Public Property

    public bool AutoCreateForeignKeyDescriptions
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateForeignKeyDescriptions ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateForeignKeyDescriptions ] = value;
      }
    }

    #endregion

    #region AutoFilterValues Public Property

    internal IDictionary<string, IList> AutoFilterValues
    {
      get
      {
        return m_autoFilterValues;
      }
    }

    #endregion

    #region AutoFilterMode Public Property

    internal AutoFilterMode AutoFilterMode
    {
      get
      {
        return m_autoFilterMode;
      }
      set
      {
        m_autoFilterMode = value;
      }
    }

    #endregion

    #region DistinctValuesConstraint Public Property

    public DistinctValuesConstraint DistinctValuesConstraint
    {
      get
      {
        return m_distinctValuesConstraint;
      }
      set
      {
        m_distinctValuesConstraint = value;
      }
    }

    #endregion

    #region FilterCriteriaMode Public Property

    public FilterCriteriaMode FilterCriteriaMode
    {
      get
      {
        return m_filterCriteriaMode;
      }
      set
      {
        m_filterCriteriaMode = value;
      }
    }

    #endregion

    #region ItemProperty Public Property

    public DataGridItemPropertyCollection ItemProperties
    {
      get
      {
        return m_itemProperties;
      }
    }

    #endregion

    #region DefaultCalculateDistinctValues Public Property

    public bool DefaultCalculateDistinctValues
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.DefaultCalculateDistinctValues ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.DefaultCalculateDistinctValues ] = value;
      }
    }

    #endregion

    #region DetailDesctiptions Public Property

    public DataGridDetailDescriptionCollection DetailDescriptions
    {
      get
      {
        return m_detailDescriptions;
      }
    }

    #endregion

    #region StatFunctions Public Property

    public StatFunctionCollection StatFunctions
    {
      get
      {
        return m_statFunctions;
      }
    }

    #endregion

    #region GroupDescriptions Public Property

    public ObservableCollection<GroupDescription> GroupDescriptions
    {
      get
      {
        return m_groupDescriptions;
      }
    }

    #endregion

    #region SortDescriptions Public Propertiy

    public SortDescriptionCollection SortDescriptions
    {
      get
      {
        return m_sortDescriptions;
      }
    }

    #endregion

    #region AutoCreateItemPropertiesCompleted Internal Property

    internal bool AutoCreateItemPropertiesCompleted
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateItemPropertiesCompleted ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateItemPropertiesCompleted ] = value;
      }
    }

    #endregion

    #region AutoCreateDetailDescriptionsCompleted Internal Property

    internal bool AutoCreateDetailDescriptionsCompleted
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateDetailDescriptionsCompleted ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateDetailDescriptionsCompleted ] = value;
      }
    }

    #endregion

    #region AutoFilteredItems Internal Property

    internal ObservableCollection<DataGridItemPropertyBase> AutoFilteredItems
    {
      get
      {
        return m_autoFilteredItems;
      }
    }

    #endregion

    #region IsAutoCreated Internal Property

    internal bool IsAutoCreated
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.IsAutoCreated ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.IsAutoCreated ] = value;
      }
    }

    #endregion

    #region InternalIsSealed Internal Property

    internal bool InternalIsSealed
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.IsSealed ];
      }
      private set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.IsSealed ] = value;
      }
    }

    #endregion

    #region IsInitialized Internal Property

    internal bool IsInitialized
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.IsInitialized ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.IsInitialized ] = value;
      }
    }

    #endregion

    #region DataGridSortDescriptions Internal Property

    internal DataGridSortDescriptionCollection DataGridSortDescriptions
    {
      get
      {
        return m_sortDescriptions;
      }
    }

    #endregion

    #region AutoFilterValuesChanged Internal Property

    internal Action<AutoFilterValuesChangedEventArgs> DetailDescriptionAutoFilterValuesChanged;

    #endregion

    #region Protected Methods

    protected internal virtual void Initialize( DataGridCollectionViewBase parentCollectionView )
    {
    }

    protected internal abstract IEnumerable GetDetailsForParentItem( DataGridCollectionViewBase parentCollectionView, object parentItem );

    #endregion

    #region Internal Methods

    internal void Seal()
    {
      this.InternalIsSealed = true;
    }

    internal void InternalInitialize( DataGridCollectionViewBase parentCollectionView )
    {
      if( string.IsNullOrEmpty( this.RelationName ) )
        throw new InvalidOperationException( "An attempt was made to initialize a detail description that does not have a relation name." );

      this.Initialize( parentCollectionView );
    }

    internal virtual void OnDetailDescriptionsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
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

        description.DetailDescriptionAutoFilterValuesChanged = this.RaiseDetailDescriptionAutoFilterValuesChangedEvent;
      }
    }

    internal virtual void OnItemPropertiesCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Replace:
          Debug.Assert( false, "The replace of an ItemProperty is not supported." );
          break;
        case NotifyCollectionChangedAction.Remove:

          int oldItemsCount = ( e.OldItems != null )
            ? e.OldItems.Count
            : 0;

          for( int i = 0; i < oldItemsCount; i++ )
          {
            DataGridItemPropertyBase dataGridItemProperty = e.OldItems[ i ] as DataGridItemPropertyBase;
            Debug.Assert( dataGridItemProperty != null );

            this.UnregisterAutoFilterValuesChangedEvent( dataGridItemProperty.Name );
          }
          break;
        case NotifyCollectionChangedAction.Reset:

          this.UnregisterAllAutoFilterValuesChangedEvent();
          break;
      }
    }

    internal void RegisterAutoFilterValuesChangedEvent( string fieldName, INotifyCollectionChanged autoFilterValues )
    {
      if( this.DetailDescriptionAutoFilterValuesChanged == null )
        return;

      if( m_registeredFieldNamesToAutoFilterValues.ContainsKey( fieldName ) )
        return;

      m_registeredFieldNamesToAutoFilterValues.Add( fieldName, autoFilterValues );
      m_registeredAutoFilterValuesToFieldNames.Add( autoFilterValues, fieldName );

      CollectionChangedEventManager.AddListener( autoFilterValues, this );
    }

    internal void UnregisterAllAutoFilterValuesChangedEvent()
    {
      foreach( INotifyCollectionChanged autoFilterValues in m_registeredAutoFilterValuesToFieldNames.Keys )
        CollectionChangedEventManager.RemoveListener( autoFilterValues, this );

      m_registeredAutoFilterValuesToFieldNames.Clear();
      m_registeredFieldNamesToAutoFilterValues.Clear();
      this.DetailDescriptionAutoFilterValuesChanged = null;
    }


    #endregion

    #region Private Methods

    private void UnregisterAutoFilterValuesChangedEvent( string fieldName )
    {
      if( string.IsNullOrEmpty( fieldName ) )
        return;

      INotifyCollectionChanged collectionChanged = null;

      if( !m_registeredFieldNamesToAutoFilterValues.TryGetValue( fieldName, out collectionChanged ) )
        return;

      CollectionChangedEventManager.RemoveListener( collectionChanged, this );

      m_registeredFieldNamesToAutoFilterValues.Remove( fieldName );
      m_registeredAutoFilterValuesToFieldNames.Remove( collectionChanged );
    }

    private void RaiseDetailDescriptionAutoFilterValuesChangedEvent( AutoFilterValuesChangedEventArgs e )
    {
      // Forward this change notification to the root CollectionView
      // or CollectionViewSource
      if( this.DetailDescriptionAutoFilterValuesChanged != null )
        this.DetailDescriptionAutoFilterValuesChanged( e );
    }

    private void OnAutoFilterValuesCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( this.AutoFilterMode == AutoFilterMode.None )
        return;

      ObservableHashList hashList = sender as ObservableHashList;

      if( hashList == null )
        return;

      string fieldName = m_registeredAutoFilterValuesToFieldNames[ hashList ];

      if( string.IsNullOrEmpty( fieldName ) )
        return;

      DataGridItemPropertyBase itemProperty = this.ItemProperties[ fieldName ];

      if( itemProperty == null )
        return;

      this.DetailDescriptionAutoFilterValuesChanged( new AutoFilterValuesChangedEventArgs( this, itemProperty, hashList, e ) );
    }

    #endregion

    #region Private Fields

    private string m_relationName;
    private object m_title;
    private DataTemplate m_titleTemplate;
    private DataGridItemPropertyCollection m_itemProperties;
    private DataGridDetailDescriptionCollection m_detailDescriptions;
    private DistinctValuesConstraint m_distinctValuesConstraint = DistinctValuesConstraint.All;
    private StatFunctionCollection m_statFunctions;
    private GroupDescriptionCollection m_groupDescriptions;
    private DataGridSortDescriptionCollection m_sortDescriptions;
    private ReadOnlyDictionary<string, IList> m_autoFilterValues;
    private ObservableCollection<DataGridItemPropertyBase> m_autoFilteredItems;
    private AutoFilterMode m_autoFilterMode = AutoFilterMode.None;
    private FilterCriteriaMode m_filterCriteriaMode = FilterCriteriaMode.And;
    private BitVector32 m_flags = new BitVector32();
    private Dictionary<string, INotifyCollectionChanged> m_registeredFieldNamesToAutoFilterValues;
    private Dictionary<INotifyCollectionChanged, string> m_registeredAutoFilterValuesToFieldNames;

    #endregion

    #region DataGridDetailDescriptionFlags Private Classes

    [Flags]
    private enum DataGridDetailDescriptionFlags
    {
      IsSealed = 1,
      DefaultCalculateDistinctValues = 2,
      IsInitialized = 4,
      AutoCreateItemProperties = 8,
      AutoCreateDetailDescriptions = 16,
      AutoCreateForeignKeyDescriptions = 32,
      IsAutoCreated = 64,
      AutoCreateItemPropertiesCompleted = 128,
      AutoCreateDetailDescriptionsCompleted = 256,
    }

    #endregion

    #region IWeakEventListener Members

    public bool ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( ( managerType == null )
          || ( sender == null )
          || ( e == null ) )
        return false;

      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs =
          e as NotifyCollectionChangedEventArgs;

        this.OnAutoFilterValuesCollectionChanged( sender, notifyCollectionChangedEventArgs );

        return true;
      }

      return false;
    }

    #endregion
  }
}
