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
using System.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid
{
  public abstract class DataGridCollectionViewSourceBase : CollectionViewSource
  {
    #region CONSTRUCTORS

    static DataGridCollectionViewSourceBase()
    {
      CollectionViewSource.SourceProperty.OverrideMetadata(
        typeof( DataGridCollectionViewSourceBase ),
        new FrameworkPropertyMetadata(
          null,
          null,
          new CoerceValueCallback( DataGridCollectionViewSourceBase.OnSourceCoerceValue ) ) );
    }

    public DataGridCollectionViewSourceBase()
      : base()
    {
      m_itemProperties = new ObservableCollection<DataGridItemPropertyBase>();
      m_itemProperties.CollectionChanged += new NotifyCollectionChangedEventHandler( this.ForwardedCollection_CollectionChanged );
      m_dataGridDetailDescriptions = new DataGridDetailDescriptionCollection();
      m_dataGridDetailDescriptions.CollectionChanged += new NotifyCollectionChangedEventHandler( this.ForwardedCollection_CollectionChanged );

      // We force a culture because it is the only way to be
      // notified when the Culture changes in order to launch our own
      // ApplyExtraPropertiesToView (because the implentation of ApplyPropertiesToView is not virtual!)
      // 
      // See CollectionViewSource.ApplyPropertiesToView in reflector for a better understanding.
      this.Culture = CultureInfo.InvariantCulture;
    }

    #endregion CONSTRUCTORS

    #region ItemProperties Property

    public ObservableCollection<DataGridItemPropertyBase> ItemProperties
    {
      get
      {
        return m_itemProperties;
      }
    }

    private ObservableCollection<DataGridItemPropertyBase> m_itemProperties;

    #endregion ItemProperties Property

    #region DetailDescriptions Property

    internal ObservableCollection<DataGridDetailDescription> DetailDescriptions
    {
      get
      {
        return m_dataGridDetailDescriptions;
      }
    }

    private ObservableCollection<DataGridDetailDescription> m_dataGridDetailDescriptions;

    #endregion DetailDescriptions Property

    #region ItemType Property

    public static readonly DependencyProperty ItemTypeProperty = DependencyProperty.Register(
      "ItemType", typeof( Type ), typeof( DataGridCollectionViewSourceBase ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.NotDataBindable ) );

    public Type ItemType
    {
      get
      {
        return this.GetValue( ItemTypeProperty ) as Type;
      }
      set
      {
        this.SetValue( ItemTypeProperty, value );

        if( m_dataSourceProvider != null )
          m_dataSourceProvider.DelayRefresh( this.Dispatcher, System.Windows.Threading.DispatcherPriority.DataBind );
      }
    }

    #endregion ItemType Property

    #region CollectionViewType Property

    protected override void OnCollectionViewTypeChanged(
      Type oldCollectionViewType,
      Type newCollectionViewType )
    {
      if( !typeof( DataGridCollectionViewBase ).IsAssignableFrom( newCollectionViewType ) )
        throw new InvalidOperationException( "An attempt was made to use a view other than DataGridCollectionViewBase." );

      base.OnCollectionViewTypeChanged( oldCollectionViewType, newCollectionViewType );
    }

    #endregion CollectionViewType Property

    #region AutoCreateItemProperties Property

    public static readonly DependencyProperty AutoCreateItemPropertiesProperty = DependencyProperty.Register(
      "AutoCreateItemProperties", typeof( bool ), typeof( DataGridCollectionViewSourceBase ),
      new FrameworkPropertyMetadata( true,
        new PropertyChangedCallback( DataGridCollectionViewSourceBase.OnDataGridCollectionViewSourceChanged ) ) );

    public bool AutoCreateItemProperties
    {
      get
      {
        return ( bool )this.GetValue( AutoCreateItemPropertiesProperty );
      }
      set
      {
        this.SetValue( AutoCreateItemPropertiesProperty, value );
      }
    }

    #endregion AutoCreateItemProperties Property

    #region AutoCreateDetailDescriptions Property

    internal static readonly DependencyProperty AutoCreateDetailDescriptionsProperty = DependencyProperty.Register(
      "AutoCreateDetailDescriptions", typeof( bool ), typeof( DataGridCollectionViewSourceBase ),
      new FrameworkPropertyMetadata( false,
        new PropertyChangedCallback( DataGridCollectionViewSourceBase.OnDataGridCollectionViewSourceChanged ) ) );

    internal bool AutoCreateDetailDescriptions
    {
      get
      {
        return ( bool )this.GetValue( AutoCreateDetailDescriptionsProperty );
      }
      set
      {
        this.SetValue( AutoCreateDetailDescriptionsProperty, value );
      }
    }

    #endregion AutoCreateDetailDescriptions Property

    #region AutoFilterMode Property

    internal static readonly DependencyProperty AutoFilterModeProperty = DependencyProperty.Register(
      "AutoFilterMode", typeof( AutoFilterMode ), typeof( DataGridCollectionViewSourceBase ),
      new UIPropertyMetadata( AutoFilterMode.None,
        new PropertyChangedCallback( DataGridCollectionViewSourceBase.OnDataGridCollectionViewSourceBaseDependencyPropertyChanged ) ) );

    internal AutoFilterMode AutoFilterMode
    {
      get
      {
        return ( AutoFilterMode )this.GetValue( AutoFilterModeProperty );
      }
      set
      {
        this.SetValue( AutoFilterModeProperty, value );
      }
    }

    #endregion AutoFilterMode Property

    #region DistinctValuesConstraints Property

    public static readonly DependencyProperty DistinctValuesConstraintProperty = DependencyProperty.Register(
      "DistinctValuesConstraint", typeof( DistinctValuesConstraint ), typeof( DataGridCollectionViewSourceBase ),
      new UIPropertyMetadata( DistinctValuesConstraint.All,
        new PropertyChangedCallback( DataGridCollectionViewSourceBase.OnDataGridCollectionViewSourceBaseDependencyPropertyChanged ) ) );

    public DistinctValuesConstraint DistinctValuesConstraint
    {
      get
      {
        return ( DistinctValuesConstraint )this.GetValue( DistinctValuesConstraintProperty );
      }
      set
      {
        this.SetValue( DistinctValuesConstraintProperty, value );
      }
    }

    #endregion DistinctValuesConstraints Property

    #region DistinctValuesUpdateMode Property

    public static readonly DependencyProperty DistinctValuesUpdateModeProperty = DependencyProperty.Register(
      "DistinctValuesUpdateMode", typeof( DistinctValuesUpdateMode ), typeof( DataGridCollectionViewSourceBase ),
      new UIPropertyMetadata( DistinctValuesUpdateMode.Manual,
        new PropertyChangedCallback( DataGridCollectionViewSourceBase.OnDataGridCollectionViewSourceBaseDependencyPropertyChanged ) ) );

    public DistinctValuesUpdateMode DistinctValuesUpdateMode
    {
      get
      {
        return ( DistinctValuesUpdateMode )this.GetValue( DistinctValuesUpdateModeProperty );
      }
      set
      {
        this.SetValue( DistinctValuesUpdateModeProperty, value );
      }
    }

    #endregion

    #region DefaultCalculateDistinctValues Property

    public static readonly DependencyProperty DefaultCalculateDistinctValuesProperty = DependencyProperty.Register(
      "DefaultCalculateDistinctValues", typeof( bool ), typeof( DataGridCollectionViewSourceBase ),
      new UIPropertyMetadata( true,
        new PropertyChangedCallback( DataGridCollectionViewSourceBase.OnDataGridCollectionViewSourceBaseDependencyPropertyChanged ) ) );

    public bool DefaultCalculateDistinctValues
    {
      get
      {
        return ( bool )this.GetValue( DefaultCalculateDistinctValuesProperty );
      }
      set
      {
        this.SetValue( DefaultCalculateDistinctValuesProperty, value );
      }
    }

    #endregion DefaultCalculateDistinctValues Property

    #region Culture Property

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    public new CultureInfo Culture
    {
      get
      {
        return base.Culture;
      }
      set
      {
        // We force a culture because it is the only way to be
        // notified when the Culture changes in order to launch our own
        // ApplyExtraPropertiesToView (because the implentation of ApplyPropertiesToView is not virtual!)
        // 
        // See CollectionViewSource.ApplyPropertiesToView in reflector for a better understanding.
        if( value == null )
          throw new ArgumentNullException( "Culture" );

        base.Culture = value;
      }
    }

    #endregion Culture Property

    #region FilterCriteriaMode Property

    public static readonly DependencyProperty FilterCriteriaModeProperty = DependencyProperty.Register(
      "FilterCriteriaMode", typeof( FilterCriteriaMode ), typeof( DataGridCollectionViewSourceBase ),
      new UIPropertyMetadata( FilterCriteriaMode.And,
        new PropertyChangedCallback( DataGridCollectionViewSourceBase.OnDataGridCollectionViewSourceBaseDependencyPropertyChanged ) ) );

    public FilterCriteriaMode FilterCriteriaMode
    {
      get
      {
        return ( FilterCriteriaMode )this.GetValue( DataGridCollectionViewSourceBase.FilterCriteriaModeProperty );
      }
      set
      {
        this.SetValue( DataGridCollectionViewSourceBase.FilterCriteriaModeProperty, value );
      }
    }

    #endregion FilterCriteriaMode Property

    #region AutoFilterValuesChanged Event

    internal event EventHandler<AutoFilterValuesChangedEventArgs> AutoFilterValuesChanged;

    internal void OnAutoFilterValuesChanged( AutoFilterValuesChangedEventArgs e )
    {
      if( this.AutoFilterValuesChanged != null )
        this.AutoFilterValuesChanged( this, e );
    }

    #endregion

    #region DistinctValuesRefreshNeeded Event

    public event EventHandler DistinctValuesRefreshNeeded;

    internal void RaiseDistinctValuesRefreshNeeded()
    {
      //Inform the user DistinctValues need to be refreshed.
      if( this.DistinctValuesRefreshNeeded != null )
      {
        this.DistinctValuesRefreshNeeded( this, EventArgs.Empty );
      }
    }

    #endregion

    #region Source property

    public new object Source
    {
      get
      {
        return m_originalSource;
      }
      set
      {
        base.Source = value;
      }
    }

    private static object OnSourceCoerceValue( DependencyObject d, object newValue )
    {
      if( newValue is DataGridCollectionViewBaseDataProvider )
        return newValue;

      DataGridCollectionViewSourceBase collectionViewSourceBase = ( DataGridCollectionViewSourceBase )d;
      collectionViewSourceBase.m_originalSource = newValue;

      if( collectionViewSourceBase.m_dataSourceProvider == null )
      {
        collectionViewSourceBase.m_dataSourceProvider = collectionViewSourceBase.CreateDataProvider();
      }
      else
      {
        collectionViewSourceBase.m_dataSourceProvider.DelayRefresh( collectionViewSourceBase.Dispatcher, System.Windows.Threading.DispatcherPriority.DataBind );
      }

      return collectionViewSourceBase.m_dataSourceProvider;
    }

    internal abstract DataGridCollectionViewBaseDataProvider CreateDataProvider();

    #endregion Source property

    #region OriginalSource Property

    internal object OriginalSource
    {
      get
      {
        return m_originalSource;
      }
    }

    #endregion OriginalSource Property

    #region AutoCreateForeignKeyDescriptions Property

    public static readonly DependencyProperty AutoCreateForeignKeyDescriptionsProperty = DependencyProperty.Register(
      "AutoCreateForeignKeyDescriptions",
      typeof( bool ),
      typeof( DataGridCollectionViewSourceBase ),
      new FrameworkPropertyMetadata( false,
        new PropertyChangedCallback( DataGridCollectionViewSourceBase.OnDataGridCollectionViewSourceChanged ) ) );

    public bool AutoCreateForeignKeyDescriptions
    {
      get
      {
        return ( bool )this.GetValue( DataGridCollectionViewSourceBase.AutoCreateForeignKeyDescriptionsProperty );
      }
      set
      {
        this.SetValue( DataGridCollectionViewSourceBase.AutoCreateForeignKeyDescriptionsProperty, value );
      }
    }

    #endregion

    #region EVENTS

    public event EventHandler<DataGridItemEventArgs> InitializingNewItem;

    internal void OnInitializingNewItem( DataGridItemEventArgs e )
    {
      if( this.InitializingNewItem != null )
        this.InitializingNewItem( this, e );
    }

    public event EventHandler<DataGridCreatingNewItemEventArgs> CreatingNewItem;

    internal void OnCreatingNewItem( DataGridCreatingNewItemEventArgs e )
    {
      if( this.CreatingNewItem != null )
        this.CreatingNewItem( this, e );
    }

    public event EventHandler<DataGridCommittingNewItemEventArgs> CommittingNewItem;

    internal void OnCommittingNewItem( DataGridCommittingNewItemEventArgs e )
    {
      if( this.CommittingNewItem != null )
        this.CommittingNewItem( this, e );
    }

    public event EventHandler<DataGridItemHandledEventArgs> CancelingNewItem;

    internal void OnCancelingNewItem( DataGridItemHandledEventArgs e )
    {
      if( this.CancelingNewItem != null )
        this.CancelingNewItem( this, e );
    }

    public event EventHandler<DataGridItemEventArgs> NewItemCreated;

    internal void OnNewItemCreated( DataGridItemEventArgs e )
    {
      if( this.NewItemCreated != null )
        this.NewItemCreated( this, e );
    }

    public event EventHandler<DataGridItemEventArgs> NewItemCommitted;

    internal void OnNewItemCommitted( DataGridItemEventArgs e )
    {
      if( this.NewItemCommitted != null )
        this.NewItemCommitted( this, e );
    }

    public event EventHandler<DataGridItemEventArgs> NewItemCanceled;

    internal void OnNewItemCanceled( DataGridItemEventArgs e )
    {
      if( this.NewItemCanceled != null )
        this.NewItemCanceled( this, e );
    }

    public event EventHandler<DataGridItemCancelEventArgs> BeginningEdit;

    internal void OnBeginningEdit( DataGridItemCancelEventArgs e )
    {
      if( this.BeginningEdit != null )
        this.BeginningEdit( this, e );
    }

    public event EventHandler<DataGridItemEventArgs> EditBegun;

    internal void OnEditBegun( DataGridItemEventArgs e )
    {
      if( this.EditBegun != null )
        this.EditBegun( this, e );
    }

    public event EventHandler<DataGridItemHandledEventArgs> CancelingEdit;

    internal void OnCancelingEdit( DataGridItemHandledEventArgs e )
    {
      if( this.CancelingEdit != null )
        this.CancelingEdit( this, e );
    }

    public event EventHandler<DataGridItemEventArgs> EditCanceled;

    internal void OnEditCanceled( DataGridItemEventArgs e )
    {
      if( this.EditCanceled != null )
        this.EditCanceled( this, e );
    }

    public event EventHandler<DataGridItemCancelEventArgs> CommittingEdit;

    internal void OnCommittingEdit( DataGridItemCancelEventArgs e )
    {
      if( this.CommittingEdit != null )
        this.CommittingEdit( this, e );
    }

    public event EventHandler<DataGridItemEventArgs> EditCommitted;

    internal void OnEditCommitted( DataGridItemEventArgs e )
    {
      if( this.EditCommitted != null )
        this.EditCommitted( this, e );
    }

    public event EventHandler<DataGridRemovingItemEventArgs> RemovingItem;

    internal void OnRemovingItem( DataGridRemovingItemEventArgs e )
    {
      if( this.RemovingItem != null )
        this.RemovingItem( this, e );
    }

    public event EventHandler<DataGridItemRemovedEventArgs> ItemRemoved;

    internal void OnItemRemoved( DataGridItemRemovedEventArgs e )
    {
      if( this.ItemRemoved != null )
        this.ItemRemoved( this, e );
    }

    #endregion EVENTS

    #region INTERNAL PROPERTIES

    internal DataGridCollectionViewBaseDataProvider DataSourceProvider
    {
      get
      {
        return m_dataSourceProvider;
      }
    }

    #endregion INTERNAL PROPERTIES

    #region INTERNAL METHODS

    internal virtual void ApplyExtraPropertiesToView( DataGridCollectionViewBase currentView )
    {
      DataGridItemPropertyCollection currentViewItemProperties = currentView.ItemProperties;
      int count = m_itemProperties.Count;

      for( int i = 0; i < count; i++ )
      {
        DataGridItemPropertyBase itemProperty = m_itemProperties[ i ];
        int index = currentViewItemProperties.IndexOf( itemProperty.Name );

        if( index == -1 )
        {
          currentViewItemProperties.Add( itemProperty );
        }
        else
        {
          currentViewItemProperties[ index ] = itemProperty;
        }
      }

      count = currentView.ItemProperties.Count;

      bool defaultCalculateDistinctValues = this.DefaultCalculateDistinctValues;

      for( int i = 0; i < count; i++ )
      {
        DataGridItemPropertyBase dataGridItemProperty = currentView.ItemProperties[ i ];

        // Set default value for CalculateDistinctValues if not explicitly set
        if( !dataGridItemProperty.IsCalculateDistinctValuesInitialized )
          dataGridItemProperty.CalculateDistinctValues = defaultCalculateDistinctValues;
      }

      count = m_dataGridDetailDescriptions.Count;

      bool autoCreateForeignKeyDescriptions = this.AutoCreateForeignKeyDescriptions;
      DataGridDetailDescriptionCollection currentViewDetailDescriptions =
        currentView.DetailDescriptions;

      for( int i = 0; i < count; i++ )
      {
        DataGridDetailDescription detailDescription = m_dataGridDetailDescriptions[ i ];
        int index = currentViewDetailDescriptions.IndexOf( detailDescription.RelationName );

        if( index == -1 )
        {
          currentViewDetailDescriptions.Add( detailDescription );
        }
        else
        {
          currentViewDetailDescriptions[ index ] = detailDescription;
        }

        // We assume we want to auto-create ForeignKeyDescriptions for DetailDescriptions
        // if this.AutoCreateForeignKeyDescriptions is true and it was auto-created
        if( detailDescription.IsAutoCreated )
        {
          detailDescription.AutoCreateForeignKeyDescriptions = autoCreateForeignKeyDescriptions;
        }
      }

      currentView.AutoFilterMode = this.AutoFilterMode;
      currentView.DistinctValuesConstraint = this.DistinctValuesConstraint;
      currentView.DistinctValuesUpdateMode = this.DistinctValuesUpdateMode;
      currentView.FilterCriteriaMode = this.FilterCriteriaMode;
    }

    #endregion INTERNAL METHODS

    #region PRIVATE METHODS

    internal static void OnDataGridCollectionViewSourceBaseDependencyPropertyChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      DataGridCollectionViewSourceBase source = o as DataGridCollectionViewSourceBase;

      if( source == null )
        return;

      source.AdviseForwardedPropertyChanged();
    }

    internal static void OnDataGridCollectionViewSourceChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      DataGridCollectionViewSourceBase source = o as DataGridCollectionViewSourceBase;

      if( source == null )
        return;

      if( source.m_dataSourceProvider != null )
        source.m_dataSourceProvider.DelayRefresh( source.Dispatcher, System.Windows.Threading.DispatcherPriority.DataBind );
    }

    internal void ForwardedCollection_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      this.AdviseForwardedPropertyChanged();
    }

    internal void AdviseForwardedPropertyChanged()
    {
      // This is the only way to react like we are increasing the version number
      // and ending with calling the base.ApplyPropertiesToView 
      base.Culture = base.Culture;
    }

    #endregion PRIVATE METHODS

    #region PRIVATE FIELDS

    private DataGridCollectionViewBaseDataProvider m_dataSourceProvider;
    private object m_originalSource;

    #endregion PRIVATE FIELDS
  }
}
