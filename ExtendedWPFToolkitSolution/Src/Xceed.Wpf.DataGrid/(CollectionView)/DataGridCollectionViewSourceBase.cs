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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  public abstract class DataGridCollectionViewSourceBase : CollectionViewSource
  {
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

    #region ItemProperties Property

    public ObservableCollection<DataGridItemPropertyBase> ItemProperties
    {
      get
      {
        return m_itemProperties;
      }
    }

    private ObservableCollection<DataGridItemPropertyBase> m_itemProperties;

    #endregion

    #region DetailDescriptions Property

    internal ObservableCollection<DataGridDetailDescription> DetailDescriptions
    {
      get
      {
        return m_dataGridDetailDescriptions;
      }
    }

    private ObservableCollection<DataGridDetailDescription> m_dataGridDetailDescriptions;

    #endregion

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

    #endregion

    #region CollectionViewType Property

    protected override void OnCollectionViewTypeChanged(
      Type oldCollectionViewType,
      Type newCollectionViewType )
    {
      if( !typeof( DataGridCollectionViewBase ).IsAssignableFrom( newCollectionViewType ) )
        throw new InvalidOperationException( "An attempt was made to use a view other than DataGridCollectionViewBase." );

      base.OnCollectionViewTypeChanged( oldCollectionViewType, newCollectionViewType );
    }

    #endregion

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

    #endregion

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

    #endregion

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

    #endregion

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

    #endregion

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

    #endregion

    #region OriginalSource Property

    internal object OriginalSource
    {
      get
      {
        return m_originalSource;
      }
    }

    #endregion

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

    #region DataSourceProvider Internal Property

    internal DataGridCollectionViewBaseDataProvider DataSourceProvider
    {
      get
      {
        return m_dataSourceProvider;
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
      {
        this.EditCanceled( this, e );
      }
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

    #endregion

    internal virtual void ApplyExtraPropertiesToView( DataGridCollectionViewBase currentView )
    {
      var currentViewItemProperties = currentView.ItemProperties;

      foreach( var itemProperty in m_itemProperties )
      {
        currentViewItemProperties[ itemProperty.Name ] = itemProperty;
      }

      var defaultCalculateDistinctValues = this.DefaultCalculateDistinctValues;

      foreach( var itemProperty in currentViewItemProperties )
      {
        // Set default value for CalculateDistinctValues if not explicitly set
        if( !itemProperty.IsCalculateDistinctValuesInitialized )
        {
          itemProperty.CalculateDistinctValues = defaultCalculateDistinctValues;
        }
      }

      var autoCreateForeignKeyDescriptions = this.AutoCreateForeignKeyDescriptions;

      for( int i = 0; i < m_dataGridDetailDescriptions.Count; i++ )
      {
        DataGridDetailDescription detailDescription = m_dataGridDetailDescriptions[ i ];

        // We assume we want to auto-create ForeignKeyDescriptions for DetailDescriptions
        // if this.AutoCreateForeignKeyDescriptions is true and it was auto-created
        if( detailDescription.IsAutoCreated )
        {
          detailDescription.AutoCreateForeignKeyDescriptions = autoCreateForeignKeyDescriptions;
        }
      }
    }

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

    #region Private Fields

    private DataGridCollectionViewBaseDataProvider m_dataSourceProvider;
    private object m_originalSource;

    #endregion
  }
}
