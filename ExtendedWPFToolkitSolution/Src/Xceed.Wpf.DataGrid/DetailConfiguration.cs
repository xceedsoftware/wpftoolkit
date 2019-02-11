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
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Xceed.Wpf.DataGrid.Markup;
using Xceed.Wpf.DataGrid.Utils;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DetailConfiguration : DependencyObject, IWeakEventListener, ISupportInitialize, INotifyPropertyChanged
  {
    #region Static Fields

    internal static readonly string DataGridControlPropertyName = PropertyHelper.GetPropertyName( ( DetailConfiguration d ) => d.DataGridControl );
    internal static readonly string DetailDescriptionPropertyName = PropertyHelper.GetPropertyName( ( DetailConfiguration d ) => d.DetailDescription );

    #endregion

    static DetailConfiguration()
    {
      DetailConfiguration.DetailConfigurationsProperty = DetailConfiguration.DetailConfigurationsPropertyKey.DependencyProperty;
      DetailConfiguration.ColumnsProperty = DetailConfiguration.ColumnsPropertyKey.DependencyProperty;
      DetailConfiguration.GroupLevelDescriptionsProperty = DetailConfiguration.GroupLevelDescriptionsPropertyKey.DependencyProperty;
      DetailConfiguration.VisibleColumnsProperty = DetailConfiguration.VisibleColumnsPropertyKey.DependencyProperty;
      DetailConfiguration.HeadersProperty = DetailConfiguration.HeadersPropertyKey.DependencyProperty;
      DetailConfiguration.FootersProperty = DetailConfiguration.FootersPropertyKey.DependencyProperty;

      DetailConfiguration.BuildDefaultTemplates();
    }

    public DetailConfiguration()
    {
      var columns = new ColumnCollection( null, this );

      m_columnManager = new ColumnHierarchyManager( columns );

      this.SetColumns( columns );
      this.SetVisibleColumns( m_columnManager.VisibleColumns );

      m_columnSynchronizationManager = new ColumnSynchronizationManager( this );
      m_columnManagerRowConfiguration = new ColumnManagerRowConfiguration();

      this.SetHeaders( new ObservableCollection<DataTemplate>() );
      this.SetFooters( new ObservableCollection<DataTemplate>() );

      this.SetGroupLevelDescriptions( new GroupLevelDescriptionCollection() );
      this.SetDetailConfigurations( new DetailConfigurationCollection( null, this ) );

      columns.CollectionChanged += new NotifyCollectionChangedEventHandler( this.OnColumnsCollectionChanged );
      CollectionChangedEventManager.AddListener( this.DetailConfigurations, this );

      this.BeginInit();
    }

    private static void BuildDefaultTemplates()
    {
      //Defining the FrameworkElementFactory variables I will use below.
      FrameworkElementFactory hglip;
      FrameworkElementFactory glip;
      FrameworkElementFactory border;
      FrameworkElementFactory contentPresenter;

      //Defining the Converters, Bindings and ViewBindings I will use to build the templates below.
      Converters.ThicknessConverter thicknessConverter = new Xceed.Wpf.DataGrid.Converters.ThicknessConverter();

      ViewBindingExtension borderThicknessBinding = new ViewBindingExtension( "HorizontalGridLineThickness" );
      borderThicknessBinding.Converter = thicknessConverter;
      borderThicknessBinding.ConverterParameter = Converters.ThicknessConverter.ThicknessSides.Top;

      //ViewBindingExtension borderBrushBinding = new ViewBindingExtension( "HorizontalGridLineBrush" );

      Binding detailTitleBinding = new Binding();
      detailTitleBinding.Path = new PropertyPath( "(0).SourceDetailConfiguration.Title", DataGridControl.DataGridContextProperty );
      detailTitleBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

      Binding detailTitleTemplateBinding = new Binding();
      detailTitleTemplateBinding.Path = new PropertyPath( "(0).SourceDetailConfiguration.TitleTemplate", DataGridControl.DataGridContextProperty );
      detailTitleTemplateBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

      //Defining the Header Spacer template.
      DefaultHeaderSpacerTemplate = new DataTemplate();
      DefaultHeaderSpacerTemplate.VisualTree = new FrameworkElementFactory( typeof( DockPanel ) );
      DefaultHeaderSpacerTemplate.VisualTree.SetValue( RowSelector.VisibleProperty, false );

      hglip = new FrameworkElementFactory( typeof( HierarchicalGroupLevelIndicatorPane ) );
      hglip.SetValue( DockPanel.DockProperty, Dock.Left );
      hglip.SetValue( TableView.CanScrollHorizontallyProperty, false );
      DefaultHeaderSpacerTemplate.VisualTree.AppendChild( hglip );

      glip = new FrameworkElementFactory( typeof( GroupLevelIndicatorPane ) );
      glip.SetValue( GroupLevelIndicatorPane.IndentedProperty, false );
      glip.SetValue( DockPanel.DockProperty, Dock.Left );
      glip.SetValue( GroupLevelIndicatorPane.ShowIndicatorsProperty, false );
      glip.SetValue( GroupLevelIndicatorPane.ShowVerticalBorderProperty, false );
      glip.SetValue( GroupLevelIndicatorPane.GroupLevelProperty, 0 );
      glip.SetValue( TableView.CanScrollHorizontallyProperty, false );
      DefaultHeaderSpacerTemplate.VisualTree.AppendChild( glip );

      border = new FrameworkElementFactory( typeof( Border ) );
      border.SetBinding( Border.BorderThicknessProperty, ( BindingBase )borderThicknessBinding.ProvideValue( null ) );
      //border.SetBinding( Border.BorderBrushProperty, ( BindingBase )borderBrushBinding.ProvideValue( null ) );
      border.SetValue( Border.MinHeightProperty, 5d );
      border.SetValue( TableView.CanScrollHorizontallyProperty, false );

      contentPresenter = new FrameworkElementFactory( typeof( ContentPresenter ) );
      contentPresenter.SetBinding( ContentPresenter.ContentProperty, detailTitleBinding );
      contentPresenter.SetBinding( ContentPresenter.ContentTemplateProperty, detailTitleTemplateBinding );
      contentPresenter.SetValue( Control.FontSizeProperty, 14d );

      border.AppendChild( contentPresenter );

      DefaultHeaderSpacerTemplate.VisualTree.AppendChild( border );
      DefaultHeaderSpacerTemplate.Seal();

      //Defining the ColumnManagerRow template
      DefaultColumnManagerRowTemplate = new DataTemplate();
      DefaultColumnManagerRowTemplate.VisualTree = new FrameworkElementFactory( typeof( ColumnManagerRow ) );
      //DefaultColumnManagerRowTemplate.VisualTree.SetValue( ColumnManagerRow.BackgroundProperty, null );
      DefaultColumnManagerRowTemplate.Seal();

      DefaultGroupByControlTemplate = new DataTemplate();
      DefaultGroupByControlTemplate.VisualTree = new FrameworkElementFactory( typeof( HierarchicalGroupByControl ) );
      DefaultGroupByControlTemplate.Seal();
    }

    private static DataTemplate DefaultHeaderSpacerTemplate;
    private static DataTemplate DefaultColumnManagerRowTemplate;
    private static DataTemplate DefaultGroupByControlTemplate;

    internal static void AddDefaultHeadersFooters( ObservableCollection<DataTemplate> headersCollection, int mergedHeadersCount, bool areDetailsFlatten )
    {
      if( !areDetailsFlatten )
      {
        headersCollection.Insert( 0, DetailConfiguration.DefaultColumnManagerRowTemplate );
        headersCollection.Insert( 0, DetailConfiguration.DefaultHeaderSpacerTemplate );
      }
    }

    internal static void SynchronizeAddedConfigurations( IList addedDetailConfigs, DataGridDetailDescriptionCollection detailDescriptions )
    {
      foreach( DetailConfiguration detailConfig in addedDetailConfigs )
      {
        DataGridDetailDescription detailDesc = null;

        if( detailDescriptions != null )
        {
          detailDesc = detailDescriptions[ detailConfig.RelationName ];
        }

        detailConfig.SynchronizeWithDetailDescription( detailDesc );
      }
    }

    internal static void CleanupDetailConfigurations( DetailConfigurationCollection detailConfigurationCollection, bool autoRemoveUnassociated )
    {
      for( int i = detailConfigurationCollection.Count - 1; i >= 0; i-- )
      {
        DetailConfiguration detailConfiguration = detailConfigurationCollection[ i ];
        detailConfiguration.DetachFromDetailDescription();
        DetailConfiguration.CleanupDetailConfigurations( detailConfiguration.DetailConfigurations, autoRemoveUnassociated );

        if( ( detailConfiguration.IsAutoCreated ) && ( autoRemoveUnassociated ) )
        {
          detailConfigurationCollection.RemoveAt( i );
        }
      }
    }

    #region RelationName Property

    public static readonly DependencyProperty RelationNameProperty =
        DependencyProperty.Register( "RelationName", typeof( string ), typeof( DetailConfiguration ), new FrameworkPropertyMetadata( string.Empty ) );

    public string RelationName
    {
      get
      {
        return ( string )this.GetValue( DetailConfiguration.RelationNameProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.RelationNameProperty, value );
      }
    }

    #endregion RelationName Property

    #region AllowDetailToggle Property

    public static readonly DependencyProperty AllowDetailToggleProperty = DataGridControl.AllowDetailToggleProperty.AddOwner( typeof( DetailConfiguration ), new FrameworkPropertyMetadata( true, new PropertyChangedCallback( OnAllowDetailToggleChanged ) ) );

    public bool AllowDetailToggle
    {
      get
      {
        return ( bool )this.GetValue( DetailConfiguration.AllowDetailToggleProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.AllowDetailToggleProperty, value );
      }
    }

    internal event EventHandler AllowDetailToggleChanged;

    private static void OnAllowDetailToggleChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DetailConfiguration detailConfig = sender as DetailConfiguration;
      if( detailConfig != null )
      {
        if( detailConfig.AllowDetailToggleChanged != null )
        {
          detailConfig.AllowDetailToggleChanged( detailConfig, EventArgs.Empty );
        }
      }
    }

    #endregion AllowDetailToggle Property

    #region AutoCreateColumns Property

    public static readonly DependencyProperty AutoCreateColumnsProperty = DataGridControl.AutoCreateColumnsProperty.AddOwner( typeof( DetailConfiguration ) );

    public bool AutoCreateColumns
    {
      get
      {
        return ( bool )this.GetValue( DetailConfiguration.AutoCreateColumnsProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.AutoCreateColumnsProperty, value );
      }
    }

    #endregion AutoCreateColumns Property

    #region Columns Read-Only Property

    internal static readonly DependencyPropertyKey ColumnsPropertyKey =
        DependencyProperty.RegisterReadOnly( "Columns", typeof( ColumnCollection ), typeof( DetailConfiguration ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty ColumnsProperty;

    public ColumnCollection Columns
    {
      get
      {
        return ( ColumnCollection )this.GetValue( DetailConfiguration.ColumnsProperty );
      }
    }

    internal void SetColumns( ColumnCollection value )
    {
      this.SetValue( DetailConfiguration.ColumnsPropertyKey, value );
    }

    #endregion

    #region DefaultDetailConfiguration Property

    public static readonly DependencyProperty DefaultDetailConfigurationProperty = DataGridControl.DefaultDetailConfigurationProperty.AddOwner( typeof( DetailConfiguration ) );

    public DefaultDetailConfiguration DefaultDetailConfiguration
    {
      get
      {
        return ( DefaultDetailConfiguration )this.GetValue( DetailConfiguration.DefaultDetailConfigurationProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.DefaultDetailConfigurationProperty, value );
      }
    }

    #endregion DefaultDetailConfiguration Property

    #region DefaultGroupConfiguration Property

    public static readonly DependencyProperty DefaultGroupConfigurationProperty = DataGridControl.DefaultGroupConfigurationProperty.AddOwner( typeof( DetailConfiguration ) );

    public GroupConfiguration DefaultGroupConfiguration
    {
      get
      {
        return ( GroupConfiguration )this.GetValue( DetailConfiguration.DefaultGroupConfigurationProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.DefaultGroupConfigurationProperty, value );
      }
    }

    #endregion DefaultGroupConfiguration Property

    #region VisibleColumns Read-Only Property

    internal static readonly DependencyPropertyKey VisibleColumnsPropertyKey =
        DependencyProperty.RegisterReadOnly( "VisibleColumns", typeof( ReadOnlyObservableCollection<ColumnBase> ), typeof( DetailConfiguration ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty VisibleColumnsProperty;

    public ReadOnlyObservableCollection<ColumnBase> VisibleColumns
    {
      get
      {
        return ( ReadOnlyObservableCollection<ColumnBase> )this.GetValue( DetailConfiguration.VisibleColumnsProperty );
      }
    }

    private void SetVisibleColumns( ReadOnlyObservableCollection<ColumnBase> value )
    {
      this.SetValue( DetailConfiguration.VisibleColumnsPropertyKey, value );
    }

    #endregion VisibleColumns Read-Only Property

    #region GroupLevelDescriptions Read-Only Property

    internal static readonly DependencyPropertyKey GroupLevelDescriptionsPropertyKey =
        DependencyProperty.RegisterReadOnly( "GroupLevelDescriptions", typeof( GroupLevelDescriptionCollection ), typeof( DetailConfiguration ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty GroupLevelDescriptionsProperty;

    public GroupLevelDescriptionCollection GroupLevelDescriptions
    {
      get
      {
        return ( GroupLevelDescriptionCollection )this.GetValue( DetailConfiguration.GroupLevelDescriptionsProperty );
      }
    }

    internal void SetGroupLevelDescriptions( GroupLevelDescriptionCollection value )
    {
      this.SetValue( DetailConfiguration.GroupLevelDescriptionsPropertyKey, value );
    }

    #endregion GroupLevelDescriptions Read-Only Property

    #region GroupConfigurationSelector Property

    public static readonly DependencyProperty GroupConfigurationSelectorProperty = DataGridControl.GroupConfigurationSelectorProperty.AddOwner( typeof( DetailConfiguration ) );

    public GroupConfigurationSelector GroupConfigurationSelector
    {
      get
      {
        return ( GroupConfigurationSelector )this.GetValue( GroupConfigurationSelectorProperty );
      }
      set
      {
        this.SetValue( GroupConfigurationSelectorProperty, value );
      }
    }



    internal event EventHandler GroupConfigurationSelectorChanged;

    #endregion

    #region ItemContainerStyle Property

    public static readonly DependencyProperty ItemContainerStyleProperty =
        DependencyProperty.Register( "ItemContainerStyle", typeof( Style ), typeof( DetailConfiguration ), new UIPropertyMetadata( null ) );

    public Style ItemContainerStyle
    {
      get
      {
        return ( Style )this.GetValue( DetailConfiguration.ItemContainerStyleProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.ItemContainerStyleProperty, value );
      }
    }

    #endregion ItemContainerStyle Property

    #region ItemContainerStyleSelector Property

    public static readonly DependencyProperty ItemContainerStyleSelectorProperty =
        DependencyProperty.Register( "ItemContainerStyleSelector", typeof( StyleSelector ), typeof( DetailConfiguration ), new UIPropertyMetadata( null ) );

    public StyleSelector ItemContainerStyleSelector
    {
      get
      {
        return ( StyleSelector )this.GetValue( DetailConfiguration.ItemContainerStyleSelectorProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.ItemContainerStyleSelectorProperty, value );
      }
    }

    #endregion ItemContainerStyleSelector Property

    #region UseDefaultHeadersFooters Property

    public static readonly DependencyProperty UseDefaultHeadersFootersProperty =
        DependencyProperty.Register( "UseDefaultHeadersFooters", typeof( bool ), typeof( DetailConfiguration ), new PropertyMetadata( true ) );

    public bool UseDefaultHeadersFooters
    {
      get
      {
        return ( bool )this.GetValue( DetailConfiguration.UseDefaultHeadersFootersProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.UseDefaultHeadersFootersProperty, value );
      }
    }

    #endregion UseDefaultHeadersFooters Property

    #region DetailConfigurations Read-Only Property

    private static readonly DependencyPropertyKey DetailConfigurationsPropertyKey =
        DependencyProperty.RegisterReadOnly( "DetailConfigurations", typeof( DetailConfigurationCollection ), typeof( DetailConfiguration ), new FrameworkPropertyMetadata( null ) );

    public static readonly DependencyProperty DetailConfigurationsProperty;

    public DetailConfigurationCollection DetailConfigurations
    {
      get
      {
        return ( DetailConfigurationCollection )this.GetValue( DetailConfiguration.DetailConfigurationsProperty );
      }
    }

    private void SetDetailConfigurations( DetailConfigurationCollection value )
    {
      this.SetValue( DetailConfiguration.DetailConfigurationsPropertyKey, value );
    }

    #endregion DetailConfigurations Read-Only Property

    #region Headers Read-Only Property

    private static readonly DependencyPropertyKey HeadersPropertyKey =
        DependencyProperty.RegisterReadOnly( "Headers", typeof( ObservableCollection<DataTemplate> ), typeof( DetailConfiguration ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty HeadersProperty;

    public ObservableCollection<DataTemplate> Headers
    {
      get
      {
        return ( ObservableCollection<DataTemplate> )this.GetValue( DetailConfiguration.HeadersProperty );
      }
    }

    private void SetHeaders( ObservableCollection<DataTemplate> value )
    {
      this.SetValue( DetailConfiguration.HeadersPropertyKey, value );
    }

    #endregion Headers Read-Only Property

    #region Footers Read-Only Property

    private static readonly DependencyPropertyKey FootersPropertyKey =
        DependencyProperty.RegisterReadOnly( "Footers", typeof( ObservableCollection<DataTemplate> ), typeof( DetailConfiguration ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty FootersProperty;

    public ObservableCollection<DataTemplate> Footers
    {
      get
      {
        return ( ObservableCollection<DataTemplate> )this.GetValue( DetailConfiguration.FootersProperty );
      }
    }

    private void SetFooters( ObservableCollection<DataTemplate> value )
    {
      this.SetValue( DetailConfiguration.FootersPropertyKey, value );
    }

    #endregion Footers Read-Only Property

    #region DetailIndicatorStyle Property

    public static readonly DependencyProperty DetailIndicatorStyleProperty =
        DependencyProperty.Register( "DetailIndicatorStyle", typeof( Style ), typeof( DetailConfiguration ), new FrameworkPropertyMetadata( null ) );

    public Style DetailIndicatorStyle
    {
      get
      {
        return ( Style )this.GetValue( DetailConfiguration.DetailIndicatorStyleProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.DetailIndicatorStyleProperty, value );
      }
    }

    #endregion DetailIndicatorStyle Property

    #region MaxSortLevels Property

    public static readonly DependencyProperty MaxSortLevelsProperty = DataGridControl.MaxSortLevelsProperty.AddOwner( typeof( DetailConfiguration ),
      new FrameworkPropertyMetadata( -1, new PropertyChangedCallback( DetailConfiguration.OnMaxSortLevelsChanged ) ) );

    public int MaxSortLevels
    {
      get
      {
        return ( int )this.GetValue( DetailConfiguration.MaxSortLevelsProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.MaxSortLevelsProperty, value );
      }
    }

    internal event EventHandler MaxSortLevelsChanged;

    private static void OnMaxSortLevelsChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DetailConfiguration detailConfig = sender as DetailConfiguration;
      if( detailConfig != null )
      {
        if( detailConfig.MaxSortLevelsChanged != null )
        {
          detailConfig.MaxSortLevelsChanged( detailConfig, EventArgs.Empty );
        }
      }
    }

    #endregion MaxSortLevels Property

    #region MaxGroupLevels Property

    public static readonly DependencyProperty MaxGroupLevelsProperty = DataGridControl.MaxGroupLevelsProperty.AddOwner( typeof( DetailConfiguration ),
      new FrameworkPropertyMetadata( -1, new PropertyChangedCallback( DetailConfiguration.OnMaxGroupLevelsChanged ) ) );

    public int MaxGroupLevels
    {
      get
      {
        return ( int )this.GetValue( DetailConfiguration.MaxGroupLevelsProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.MaxGroupLevelsProperty, value );
      }
    }

    internal event EventHandler MaxGroupLevelsChanged;

    private static void OnMaxGroupLevelsChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DetailConfiguration detailConfig = sender as DetailConfiguration;
      if( detailConfig != null )
      {
        if( detailConfig.MaxGroupLevelsChanged != null )
        {
          detailConfig.MaxGroupLevelsChanged( detailConfig, EventArgs.Empty );
        }
      }
    }

    #endregion MaxGroupLevels Property

    #region Title Property

    public static readonly DependencyProperty TitleProperty =
       DependencyProperty.Register( "Title", typeof( object ), typeof( DetailConfiguration ), new UIPropertyMetadata( null ) );

    public object Title
    {
      get
      {
        return this.GetValue( DetailConfiguration.TitleProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.TitleProperty, value );
      }
    }

    #endregion

    #region TitleTemplate Property

    public static readonly DependencyProperty TitleTemplateProperty =
        DependencyProperty.Register( "TitleTemplate", typeof( DataTemplate ), typeof( DetailConfiguration ), new UIPropertyMetadata( null ) );

    public DataTemplate TitleTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( DetailConfiguration.TitleTemplateProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.TitleTemplateProperty, value );
      }
    }

    #endregion TitleTemplate Property

    #region Visible Property

    public static readonly DependencyProperty VisibleProperty =
        DependencyProperty.Register( "Visible", typeof( bool ), typeof( DetailConfiguration ),
            new FrameworkPropertyMetadata( ( bool )true, new PropertyChangedCallback( OnVisibilityChanged ) ) );

    public bool Visible
    {
      get
      {
        return ( bool )this.GetValue( VisibleProperty );
      }
      set
      {
        this.SetValue( VisibleProperty, value );
      }
    }

    private static void OnVisibilityChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DetailConfiguration detailConfig = sender as DetailConfiguration;
      if( detailConfig != null )
      {
        if( detailConfig.VisibilityChanged != null )
        {
          detailConfig.VisibilityChanged( detailConfig, EventArgs.Empty );
        }
      }
    }

    internal event EventHandler VisibilityChanged;

    #endregion

    #region AutoCreateForeignKeyConfigurations Property

    public static readonly DependencyProperty AutoCreateForeignKeyConfigurationsProperty = DependencyProperty.Register(
      "AutoCreateForeignKeyConfigurations",
      typeof( bool ),
      typeof( DetailConfiguration ),
      new FrameworkPropertyMetadata(
        ( bool )false,
        new PropertyChangedCallback( DetailConfiguration.OnAutoCreateForeignKeyConfigurationsChanged ) ) );

    public bool AutoCreateForeignKeyConfigurations
    {
      get
      {
        return ( bool )this.GetValue( DetailConfiguration.AutoCreateForeignKeyConfigurationsProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.AutoCreateForeignKeyConfigurationsProperty, value );
      }
    }

    private static void OnAutoCreateForeignKeyConfigurationsChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DetailConfiguration configuration = sender as DetailConfiguration;

      if( ( configuration == null ) || ( !configuration.AutoCreateForeignKeyConfigurations ) )
        return;

      configuration.SynchronizeForeignKeyConfigurations();
    }

    #endregion

    #region ForeignKeysUpdatedOnAutoCreate Property

    internal bool ForeignKeysUpdatedOnAutoCreate
    {
      get;
      set;
    }

    #endregion

    #region CurrentColumn Property

    internal ColumnBase CurrentColumn
    {
      get
      {
        return m_currentColumn;
      }
      set
      {
        if( m_currentColumn != value )
        {
          m_currentColumn = value;

          if( this.CurrentColumnChanged != null )
            this.CurrentColumnChanged( this, EventArgs.Empty );
        }
      }
    }

    internal event EventHandler CurrentColumnChanged;

    private ColumnBase m_currentColumn; // = null

    #endregion

    #region ColumnsByVisiblePosition Read-Only Property

    internal HashedLinkedList<ColumnBase> ColumnsByVisiblePosition
    {
      get
      {
        return m_columnManager.ColumnsByVisiblePosition;
      }
    }

    #endregion

    #region ItemsSourcePropertyDescriptions Internal Property

    internal PropertyDescriptionRouteDictionary ItemsSourcePropertyDescriptions
    {
      get
      {
        return m_itemsSourcePropertyDescriptions;
      }
    }

    private readonly PropertyDescriptionRouteDictionary m_itemsSourcePropertyDescriptions = new PropertyDescriptionRouteDictionary();

    #endregion

    #region ShouldCreateColumns Property

    internal bool ShouldCreateColumns
    {
      get
      {
        return m_shouldCreateColumns;
      }
      set
      {
        m_shouldCreateColumns = value;
      }
    }

    #endregion

    #region SortDescriptionsSyncContext Read-Only Property

    internal SortDescriptionsSyncContext SortDescriptionsSyncContext
    {
      get
      {
        DataGridSortDescriptionCollection dataGridSortDescriptions = m_sortDescriptions as DataGridSortDescriptionCollection;
        if( dataGridSortDescriptions != null )
          return dataGridSortDescriptions.SyncContext;

        if( m_sortDescriptionsSyncContext == null )
        {
          m_sortDescriptionsSyncContext = new SortDescriptionsSyncContext();
        }

        return m_sortDescriptionsSyncContext;
      }
    }

    private SortDescriptionsSyncContext m_sortDescriptionsSyncContext; // = null

    #endregion

    #region SortDescriptions Read-Only Property

    internal SortDescriptionCollection SortDescriptions
    {
      get
      {
        return m_sortDescriptions;
      }
    }

    private SortDescriptionCollection m_sortDescriptions; // = null

    #endregion

    #region GroupDescriptions Read-Only Property

    internal ObservableCollection<GroupDescription> GroupDescriptions
    {
      get
      {
        return m_groupDescriptions;
      }
    }

    private ObservableCollection<GroupDescription> m_groupDescriptions; // = null

    #endregion

    #region DataGridControl Internal Property

    internal DataGridControl DataGridControl
    {
      get
      {
        return m_dataGridControl;
      }
      private set
      {
        if( value == m_dataGridControl )
          return;

        var oldDataGridControl = m_dataGridControl;

        if( m_dataGridControl != null )
        {
          ViewChangedEventManager.RemoveListener( m_dataGridControl, this );
        }

        m_dataGridControl = value;

        if( m_dataGridControl != null )
        {
          ViewChangedEventManager.AddListener( m_dataGridControl, this );
        }

        if( value != null )
        {
          m_columnManager.Initialize( value );
          m_columnManager.SetFixedColumnCount( TableView.GetFixedColumnCount( this ) );
        }
        else
        {
          m_columnManager.Clear();
        }

        var detailConfigurations = this.DetailConfigurations;
        if( detailConfigurations != null )
        {
          detailConfigurations.DataGridControl = value;
        }

        this.InitializeItemPropertyMap( this.DataGridControl, this.DetailDescription );
        this.OnPropertyChanged( DetailConfiguration.DataGridControlPropertyName );

        if( m_dataGridControl == null )
        {
          this.BeginInit();
        }
        else if( oldDataGridControl == null )
        {
          this.EndInit();
        }
      }
    }

    private DataGridControl m_dataGridControl;

    #endregion

    #region DataGridContext Property

    internal DataGridContext DataGridContext
    {
      get
      {
        return m_dataGridContexts.FirstOrDefault();
      }
    }

    internal void AddDataGridContext( DataGridContext dataGridContext )
    {
      Debug.Assert( dataGridContext != null );

      m_dataGridContexts.Add( dataGridContext );
    }

    internal void RemoveDataGridContext( DataGridContext dataGridContext )
    {
      var oldCount = m_dataGridContexts.Count;
      m_dataGridContexts.Remove( dataGridContext );
      var newCount = m_dataGridContexts.Count;
    }

    private readonly ICollection<DataGridContext> m_dataGridContexts = new HashSet<DataGridContext>();

    #endregion

    #region ParentDetailConfiguration Internal Property

    internal DetailConfiguration ParentDetailConfiguration
    {
      get
      {
        if( m_containingCollection == null )
          return null;

        return m_containingCollection.ParentDetailConfiguration;
      }
    }

    #endregion

    #region ColumnManagerRowConfiguration Internal Property

    internal ColumnManagerRowConfiguration ColumnManagerRowConfiguration
    {
      get
      {
        return m_columnManagerRowConfiguration;
      }
    }

    private readonly ColumnManagerRowConfiguration m_columnManagerRowConfiguration;

    #endregion

    #region IsAutoCreated Internal Property

    internal bool IsAutoCreated
    {
      get
      {
        return m_isAutoCreated;
      }
      private set
      {
        m_isAutoCreated = value;
      }
    }

    private bool m_isAutoCreated = false;

    #endregion

    #region IsCreatedFromSelector Property

    private bool IsCreatedFromSelector
    {
      get;
      set;
    }

    #endregion

    #region ItemPropertyMap Internal Property

    internal DataGridItemPropertyMap ItemPropertyMap
    {
      get
      {
        return m_itemPropertyMap;
      }
    }

    private readonly DataGridItemPropertyMap m_itemPropertyMap = new DataGridItemPropertyMap();

    #endregion

    #region UpdateColumnSortCommand Internal Property

    internal UpdateColumnSortCommand UpdateColumnSortCommand
    {
      get
      {
        if( m_updateColumnSortCommand == null )
        {
          m_updateColumnSortCommand = new DetailConfigurationUpdateColumnSortCommand( this );
        }

        Debug.Assert( m_updateColumnSortCommand != null );

        return m_updateColumnSortCommand;
      }
    }

    private UpdateColumnSortCommand m_updateColumnSortCommand;

    #endregion

    #region AddGroupCommand Internal Property

    internal ColumnAddGroupCommand AddGroupCommand
    {
      get
      {
        if( m_addGroupCommand == null )
        {
          m_addGroupCommand = new DetailConfigurationAddGroupCommand( this );
        }

        Debug.Assert( m_addGroupCommand != null );

        return m_addGroupCommand;
      }
    }

    private ColumnAddGroupCommand m_addGroupCommand;

    #endregion

    #region IsDeleteCommandEnabled Property

    public static readonly DependencyProperty IsDeleteCommandEnabledProperty = DependencyProperty.Register(
      "IsDeleteCommandEnabled",
      typeof( bool ),
      typeof( DetailConfiguration ),
      new FrameworkPropertyMetadata( false, new PropertyChangedCallback( DetailConfiguration.OnIsDeleteCommandEnabledChanged ) ) );

    public bool IsDeleteCommandEnabled
    {
      get
      {
        return ( bool )this.GetValue( DetailConfiguration.IsDeleteCommandEnabledProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.IsDeleteCommandEnabledProperty, value );
      }
    }

    private static void OnIsDeleteCommandEnabledChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
    {
      CommandManager.InvalidateRequerySuggested();
    }

    #endregion IsDeleteCommandEnabled Property

    #region DetailDescription Internal Property

    internal DataGridDetailDescription DetailDescription
    {
      get
      {
        return m_detailDescription;
      }
      private set
      {
        if( value == m_detailDescription )
          return;

        m_detailDescription = value;
        this.OnPropertyChanged( DetailConfiguration.DetailDescriptionPropertyName );
      }
    }

    private DataGridDetailDescription m_detailDescription; // = null 

    #endregion

    #region ContainingCollection Private Property

    private DetailConfigurationCollection ContainingCollection
    {
      get
      {
        return m_containingCollection;
      }
      set
      {
        if( value == m_containingCollection )
          return;

        if( ( m_containingCollection != null ) && ( value != null ) )
          throw new InvalidOperationException( "The DetailConfiguration is already contained in another DetailConfigurationCollection." );

        if( m_containingCollection != null )
        {
          ( ( INotifyPropertyChanged )m_containingCollection ).PropertyChanged -= new PropertyChangedEventHandler( this.OnContainingCollectionPropertyChanged );
        }

        m_containingCollection = value;

        if( m_containingCollection != null )
        {
          ( ( INotifyPropertyChanged )m_containingCollection ).PropertyChanged += new PropertyChangedEventHandler( this.OnContainingCollectionPropertyChanged );

          this.DataGridControl = m_containingCollection.DataGridControl;
        }
        else
        {
          this.DataGridControl = null;
        }
      }
    }

    private void OnContainingCollectionPropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      var containingCollection = ( DetailConfigurationCollection )sender;
      Debug.Assert( containingCollection == this.ContainingCollection );

      var propertyName = e.PropertyName;
      bool mayHaveChanged = string.IsNullOrEmpty( propertyName );

      if( mayHaveChanged || ( propertyName == "DataGridControl" ) )
      {
        this.DataGridControl = containingCollection.DataGridControl;
      }
    }

    private DetailConfigurationCollection m_containingCollection; // = null

    #endregion

    #region ColumnSynchronizationManager Private Read-Only Property

    private ColumnSynchronizationManager ColumnSynchronizationManager
    {
      get
      {
        return m_columnSynchronizationManager;
      }
    }

    private readonly ColumnSynchronizationManager m_columnSynchronizationManager;

    #endregion

    #region ColumnManager Internal Property

    internal ColumnHierarchyManager ColumnManager
    {
      get
      {
        return m_columnManager;
      }
    }

    private readonly ColumnHierarchyManager m_columnManager;

    #endregion

    public IDisposable DeferColumnsUpdate()
    {
      return ColumnHierarchyManagerHelper.DeferColumnsUpdate( this );
    }

    public bool MoveColumnBefore( ColumnBase current, ColumnBase next )
    {
      return ColumnHierarchyManagerHelper.MoveColumnBefore( this, current, next );
    }

    public bool MoveColumnAfter( ColumnBase current, ColumnBase previous )
    {
      return ColumnHierarchyManagerHelper.MoveColumnAfter( this, current, previous );
    }

    public bool MoveColumnUnder( ColumnBase current, ColumnBase parent )
    {
      return ColumnHierarchyManagerHelper.MoveColumnUnder( this, current, parent );
    }

    protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnPropertyChanged( e );

      if( e.Property == DetailConfiguration.GroupConfigurationSelectorProperty )
      {
        var handler = this.GroupConfigurationSelectorChanged;
        if( handler != null )
        {
          handler.Invoke( this, EventArgs.Empty );
        }
      }

      this.OnPropertyChanged( e.Property.Name );
    }

    internal void AddDefaultHeadersFooters()
    {
      if( m_defaultHeadersFootersAdded )
        return;

      m_defaultHeadersFootersAdded = true;

      DetailConfiguration.AddDefaultHeadersFooters( this.Headers, 0, this.DataGridControl.AreDetailsFlatten );
    }

    internal void SynchronizeForeignKeyConfigurations()
    {
      var detailDescription = this.DetailDescription;
      if( detailDescription == null )
        return;

      ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView(
        this.Columns,
        detailDescription.ItemProperties,
        this.AutoCreateForeignKeyConfigurations );
    }

    internal void AttachToContainingCollection( DetailConfigurationCollection detailConfigurationCollection )
    {
      if( detailConfigurationCollection == null )
        throw new ArgumentNullException( "detailConfigurationCollection" );

      this.ContainingCollection = detailConfigurationCollection;
    }

    internal void DetachFromContainingCollection()
    {
      this.ContainingCollection = null;
    }

    private void SynchronizeWithDetailDescription( DataGridDetailDescription detailDescription )
    {
      if( detailDescription == this.DetailDescription )
        return;

      this.DetachFromDetailDescription( false );

      m_shouldCreateColumns = true;
      this.DetailDescription = detailDescription;

      DataGridSortDescriptionCollection dataGridSortDescriptionCollection;
      ObservableCollection<GroupDescription> groupDescriptionCollection;

      if( detailDescription != null )
      {
        groupDescriptionCollection = detailDescription.GroupDescriptions;

        //register to the collectionchanged of the DataGridDetailDescription collection of the detailDescription matching this one.
        dataGridSortDescriptionCollection = detailDescription.DataGridSortDescriptions;

        this.RegisterItemProperties( detailDescription.ItemProperties );

        CollectionChangedEventManager.AddListener( detailDescription.DetailDescriptions, this );
      }
      else
      {
        groupDescriptionCollection = new GroupDescriptionCollection();
        dataGridSortDescriptionCollection = new DataGridSortDescriptionCollection();
      }

      m_groupDescriptions = groupDescriptionCollection;
      m_sortDescriptions = dataGridSortDescriptionCollection;
      m_sortDescriptionsSyncContext = null; //clear it, if it was ever set!

      this.InitializeItemPropertyMap( this.DataGridControl, detailDescription );

      //This update is required since there might be some columns in the columns collection after the XAML parsing of the DetailConfiguration
      this.UpdateColumnSortOrder();

      //This update is required since we want the GroupLevelDescriptions to be created if DetailConfiguration in XAML
      //contains GroupDescriptions
      DataGridContext.UpdateGroupLevelDescriptions( this.GroupLevelDescriptions,
                                                    new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ),
                                                    this.GroupDescriptions,
                                                    this.Columns );

      CollectionChangedEventManager.AddListener( m_sortDescriptions, this );
      CollectionChangedEventManager.AddListener( m_groupDescriptions, this );

      if( detailDescription != null )
      {
        if( this.ReadLocalValue( DetailConfiguration.TitleProperty ) == DependencyProperty.UnsetValue )
        {
          if( detailDescription.Title == null )
          {
            this.Title = detailDescription.RelationName;
          }
          else
          {
            this.Title = detailDescription.Title;
          }
        }

        if( ( this.ReadLocalValue( DetailConfiguration.TitleTemplateProperty ) == DependencyProperty.UnsetValue )
          && ( detailDescription.TitleTemplate != null ) )
        {
          this.TitleTemplate = detailDescription.TitleTemplate;
        }
      }

      this.UpdateColumns();
    }

    private void DetachFromDetailDescription()
    {
      this.DetachFromDetailDescription( true );
    }

    private void DetachFromDetailDescription( bool synchronizeColumns )
    {
      var detailDescription = this.DetailDescription;
      if( detailDescription != null )
      {
        this.UnregisterItemProperties( detailDescription.ItemProperties );

        CollectionChangedEventManager.RemoveListener( detailDescription.DetailDescriptions, this );
      }

      if( m_sortDescriptions != null )
      {
        CollectionChangedEventManager.RemoveListener( m_sortDescriptions, this );
      }

      if( m_groupDescriptions != null )
      {
        CollectionChangedEventManager.RemoveListener( m_groupDescriptions, this );
      }

      m_sortDescriptions = null;
      m_groupDescriptions = null;
      m_itemsSourcePropertyDescriptions.Clear();

      this.InitializeItemPropertyMap( this.DataGridControl, null );
      this.DetailDescription = null;

      if( synchronizeColumns )
      {
        m_columnSynchronizationManager.Refresh();
      }
    }

    private void InitializeItemPropertyMap( DataGridControl dataGridControl, DataGridDetailDescription detailDescription )
    {
      using( m_itemPropertyMap.DeferMappingChanged() )
      {
        if( ( dataGridControl == null ) || ( detailDescription == null ) || !dataGridControl.AreDetailsFlatten )
        {
          m_itemPropertyMap.MasterItemProperties = null;
          m_itemPropertyMap.DetailItemProperties = null;
        }
        else
        {
          var collectionView = dataGridControl.Items.SourceCollection as DataGridCollectionViewBase;
          if( collectionView != null )
          {
            m_itemPropertyMap.MasterItemProperties = collectionView.ItemProperties;
            m_itemPropertyMap.DetailItemProperties = detailDescription.ItemProperties;
          }
          else
          {
            m_itemPropertyMap.MasterItemProperties = null;
            m_itemPropertyMap.DetailItemProperties = null;
          }
        }
      }
    }

    private void UpdateColumns()
    {
      var detailDescription = this.DetailDescription;
      var dataGridControl = this.DataGridControl;
      var dataGridContext = this.DataGridContext;

      if( ( detailDescription != null ) && ( dataGridControl != null ) && ( dataGridContext != null ) )
      {
        ItemsSourceHelper.ResetPropertyDescriptions( m_itemsSourcePropertyDescriptions, m_itemPropertyMap, dataGridControl, dataGridContext.Items );

        if( this.AutoCreateColumns )
        {
          ItemsSourceHelper.UpdateColumnsFromPropertyDescriptions( m_columnManager, dataGridControl.DefaultCellEditors, m_itemsSourcePropertyDescriptions, this.AutoCreateForeignKeyConfigurations );
        }
      }
      else
      {
        m_itemsSourcePropertyDescriptions.Clear();
      }

      if( detailDescription != null )
      {
        // Ensure to update ForeignKeyDescriptions and ForeignKeyConfigurations when ItemProperties change on DetailDescription.
        ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView(
          this.Columns,
          detailDescription.ItemProperties,
          this.AutoCreateForeignKeyConfigurations );
      }
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

      PropertyChangedEventManager.AddListener( itemProperty, this, DataGridItemPropertyBase.ItemPropertiesInternalPropertyName );
      this.RegisterItemProperties( itemProperty.ItemPropertiesInternal );
    }

    private void UnregisterItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return;

      this.UnregisterItemProperties( itemProperty.ItemPropertiesInternal );
      PropertyChangedEventManager.RemoveListener( itemProperty, this, DataGridItemPropertyBase.ItemPropertiesInternalPropertyName );
    }

    private void UpdateColumnSortOrder()
    {
      var updateColumnSortCommand = this.UpdateColumnSortCommand;
      if( updateColumnSortCommand.CanExecute() )
      {
        this.UpdateColumnSortCommand.Execute();
      }
    }

    private void SortDescriptions_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      this.RequestDelayBringIntoViewAndFocusCurrent( AutoScrollCurrentItemSourceTriggers.SortChanged );
      this.UpdateColumnSortOrder();
    }

    private void GroupDescriptions_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      this.RequestDelayBringIntoViewAndFocusCurrent( AutoScrollCurrentItemSourceTriggers.GroupChanged );
      DataGridContext.UpdateGroupLevelDescriptions( this.GroupLevelDescriptions, e, m_groupDescriptions, this.Columns );
    }

    private void OnColumnsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      //call the method that will detect if the CurrentColumn should be removed based on the changes in the Columns collection.
      this.HandleCurrentColumnRemove( e );

      this.RequestDelayBringIntoViewAndFocusCurrent( AutoScrollCurrentItemSourceTriggers.ColumnsCollectionChanged );
      this.UpdateColumnSortOrder();

      var detailDescription = this.DetailDescription;
      if( detailDescription != null )
      {
        switch( e.Action )
        {
          case NotifyCollectionChangedAction.Add:
          case NotifyCollectionChangedAction.Replace:
          case NotifyCollectionChangedAction.Reset:
            {
              ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView(
                this.Columns,
                detailDescription.ItemProperties,
                this.AutoCreateForeignKeyConfigurations );
            }
            break;

          default:
            break;
        }
      }
    }

    private void OnItemPropertiesCollectionChanged( DataGridItemPropertyCollection collection, NotifyCollectionChangedEventArgs e )
    {
      var detailDescription = this.DetailDescription;
      if( detailDescription == null )
        return;

      var rootCollection = ItemsSourceHelper.GetRootCollection( collection );
      if( rootCollection == null )
        return;

      if( rootCollection == detailDescription.ItemProperties )
      {
        if( e.Action == NotifyCollectionChangedAction.Reset )
          throw new NotSupportedException();

        if( e.Action == NotifyCollectionChangedAction.Move )
          return;

        if( e.OldItems != null )
        {
          foreach( DataGridItemPropertyBase itemProperty in e.OldItems )
          {
            this.UnregisterItemProperty( itemProperty );
          }
        }

        if( e.NewItems != null )
        {
          foreach( DataGridItemPropertyBase itemProperty in e.NewItems )
          {
            this.RegisterItemProperty( itemProperty );
          }
        }

        this.UpdateColumns();
      }
      else
      {
        Debug.Fail( "The collection is not linked to the detail configuration's item properties." );
        this.UnregisterItemProperties( collection );
      }
    }

    private void OnItemPropertyPropertyChanged( DataGridItemPropertyBase itemProperty, PropertyChangedEventArgs e )
    {
      var detailDescription = this.DetailDescription;
      if( detailDescription == null )
        return;

      var rootCollection = ItemsSourceHelper.GetRootCollection( itemProperty );
      if( rootCollection == null )
        return;

      if( rootCollection != detailDescription.ItemProperties )
      {
        Debug.Fail( "The collection is not linked to the detail configuration's item properties." );
        this.UnregisterItemProperty( itemProperty );
        return;
      }

      if( string.IsNullOrEmpty( e.PropertyName ) || ( e.PropertyName == DataGridItemPropertyBase.ItemPropertiesInternalPropertyName ) )
      {
        var itemProperties = itemProperty.ItemPropertiesInternal;
        if( itemProperties != null )
        {
          this.UnregisterItemProperties( itemProperties );
          this.RegisterItemProperties( itemProperties );
          this.UpdateColumns();
        }
      }
    }

    private void HandleCurrentColumnRemove( NotifyCollectionChangedEventArgs e )
    {
      var currentColumn = this.CurrentColumn;
      if( currentColumn == null )
        return;

      var removeCurrentColumn = false;

      if( e.Action == NotifyCollectionChangedAction.Reset )
      {
        removeCurrentColumn = !this.Columns.Contains( currentColumn );
      }
      else if( ( e.Action == NotifyCollectionChangedAction.Remove ) && ( e.OldItems.Contains( currentColumn ) ) )
      {
        //Remove of at least the current column
        removeCurrentColumn = true;
      }
      else if( ( e.Action == NotifyCollectionChangedAction.Replace ) && ( e.OldItems.Contains( currentColumn ) ) && ( !e.NewItems.Contains( currentColumn ) ) )
      {
        //Replace in which at least the current column was "replaced" by another (current column not present in new items )
        removeCurrentColumn = true;
      }

      //If we computed that current columns should be cleared
      if( removeCurrentColumn )
      {
        //reset current column.
        this.CurrentColumn = DataGridContext.GetClosestColumn( currentColumn, this.ColumnsByVisiblePosition );
      }
    }

    private void OnDetailConfigurationsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      var detailDescription = this.DetailDescription;
      var detailDescriptions = ( detailDescription != null ) ? detailDescription.DetailDescriptions : null;

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Add:
          DetailConfiguration.SynchronizeAddedConfigurations( e.NewItems, detailDescriptions );
          break;
        case NotifyCollectionChangedAction.Reset:
        case NotifyCollectionChangedAction.Remove:
        case NotifyCollectionChangedAction.Move:
        default:
          break;
      }
    }

    private void RequestDelayBringIntoViewAndFocusCurrent( AutoScrollCurrentItemSourceTriggers trigger )
    {
    }

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    private bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      //check if the event comes from a INotifyCollectionChanged collection
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        var eventArgs = ( NotifyCollectionChangedEventArgs )e;

        if( sender is DetailConfigurationCollection )
        {
          this.OnDetailConfigurationsChanged( sender, eventArgs );
        }
        else if( sender is SortDescriptionCollection )
        {
          this.SortDescriptions_CollectionChanged( sender, eventArgs );
        }
        else if( sender is ObservableCollection<GroupDescription> )
        {
          this.GroupDescriptions_CollectionChanged( sender, eventArgs );
        }
        else if( sender is DataGridDetailDescriptionCollection )
        {
        }
        else if( sender is DataGridItemPropertyCollection )
        {
          this.OnItemPropertiesCollectionChanged( ( DataGridItemPropertyCollection )sender, eventArgs );
        }
      }
      else if( managerType == typeof( PropertyChangedEventManager ) )
      {
        var eventArgs = ( PropertyChangedEventArgs )e;

        if( sender is DataGridItemPropertyBase )
        {
          this.OnItemPropertyPropertyChanged( ( DataGridItemPropertyBase )sender, eventArgs );
        }
      }
      else if( managerType == typeof( ViewChangedEventManager ) )
      {
        this.InitializeItemPropertyMap( this.DataGridControl, this.DetailDescription );
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    #region ISupportInitialize Members

    public void BeginInit()
    {
      m_initCount++;
      if( m_initCount != 1 )
        return;

      Debug.Assert( m_deferColumnsLayoutUpdate == null );
      m_deferColumnsLayoutUpdate = m_columnManager.DeferUpdate();

      m_deferColumnsNotifications = this.Columns.DeferNotifications();
    }

    public void EndInit()
    {
      m_initCount--;
      if( m_initCount != 0 )
        return;

      var columnsNotificationsDisposable = m_deferColumnsNotifications;
      m_deferColumnsNotifications = null;
      columnsNotificationsDisposable.Dispose();

      Debug.Assert( m_deferColumnsLayoutUpdate != null );
      var columnsUpdateDisposable = m_deferColumnsLayoutUpdate;
      m_deferColumnsLayoutUpdate = null;
      columnsUpdateDisposable.Dispose();
    }

    private int m_initCount = 0;
    private IDisposable m_deferColumnsLayoutUpdate; //null
    private IDisposable m_deferColumnsNotifications; //null

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged( string propertyName )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    #endregion

    private bool m_shouldCreateColumns = true;
    private bool m_defaultHeadersFootersAdded; // = false

    private sealed class DetailConfigurationUpdateColumnSortCommand : UpdateColumnSortCommand
    {
      private static void ThrowIfDetailsNotFlatten( DetailConfiguration detailConfiguration )
      {
        Debug.Assert( detailConfiguration != null );

        var dataGridControl = detailConfiguration.DataGridControl;
        if( ( dataGridControl != null ) && ( !dataGridControl.AreDetailsFlatten ) )
          throw new InvalidOperationException( "This method cannot be invoked when details are not flattened." );
      }

      internal DetailConfigurationUpdateColumnSortCommand( DetailConfiguration detailConfiguration )
      {
        DetailConfigurationUpdateColumnSortCommand.ThrowIfNull( detailConfiguration, "detailConfiguration" );

        m_detailConfiguration = new WeakReference( detailConfiguration );
      }

      #region SortDescriptionsSyncContext Protected Property

      protected override SortDescriptionsSyncContext SortDescriptionsSyncContext
      {
        get
        {
          var detailConfiguration = this.DetailConfiguration;
          if( detailConfiguration == null )
            return null;

          return detailConfiguration.SortDescriptionsSyncContext;
        }
      }

      #endregion

      #region DetailConfiguration Private Property

      private DetailConfiguration DetailConfiguration
      {
        get
        {
          return m_detailConfiguration.Target as DetailConfiguration;
        }
      }

      private readonly WeakReference m_detailConfiguration;

      #endregion

      protected override bool CanExecuteCore()
      {
        var detailConfiguration = this.DetailConfiguration;
        if( detailConfiguration == null )
          return false;

        var dataGridControl = detailConfiguration.DataGridControl;
        if( dataGridControl == null )
          return false;

        var detailDescription = detailConfiguration.DetailDescription;
        if( detailDescription == null )
          return false;

        return true;
      }

      protected override void ExecuteCore()
      {
        DetailConfiguration detailConfiguration = this.DetailConfiguration;
        if( detailConfiguration == null )
          return;

        var dataGridControl = detailConfiguration.DataGridControl;
        if( dataGridControl == null )
          return;

        using( var synchronizationContext = this.StartSynchronizing( detailConfiguration.SortDescriptionsSyncContext ) )
        {
          // The SortDescription collection is already being updated.
          if( !synchronizationContext.Own )
            return;

          // The sort order of the flatten details is driven by the master.  Flatten details are not
          // allowed to modify the sort order at their level.
          if( dataGridControl.AreDetailsFlatten )
          {
            this.SynchronizeDetailSortDescriptions( synchronizationContext, detailConfiguration );
          }

          this.SynchronizeColumnSort(
            synchronizationContext,
            detailConfiguration.SortDescriptions,
            detailConfiguration.Columns );
        }
      }

      private static IEnumerable<DataGridContext> GetDataGridContexts( DetailConfiguration detailConfiguration )
      {
        Debug.Assert( detailConfiguration != null );

        var dataGridControl = detailConfiguration.DataGridControl;
        Debug.Assert( dataGridControl != null );

        var queue = new Queue<DataGridContext>();
        queue.Enqueue( dataGridControl.DataGridContext );

        while( queue.Count > 0 )
        {
          DataGridContext dataGridContext = queue.Dequeue();

          if( dataGridContext.SourceDetailConfiguration == detailConfiguration )
          {
            yield return dataGridContext;
          }
          else
          {
            foreach( var childContext in dataGridContext.GetChildContexts() )
            {
              queue.Enqueue( childContext );
            }
          }
        }
      }

      private IDisposable DeferResortHelper( DetailConfiguration detailConfiguration )
      {
        ColumnSortCommand.ThrowIfNull( detailConfiguration, "detailConfiguration" );

        IDisposable defer;
        if( this.TryDeferResort( detailConfiguration, out defer ) )
          return defer;

        var disposer = new Disposer();

        foreach( var dataGridContext in DetailConfigurationUpdateColumnSortCommand.GetDataGridContexts( detailConfiguration ) )
        {
          disposer.Add( this.DeferResortHelper( dataGridContext.ItemsSourceCollection, dataGridContext.Items ), DisposableType.DeferResort );
        }

        return disposer;
      }

      private void SynchronizeDetailSortDescriptions( SynchronizationContext synchronizationContext, DetailConfiguration detailConfiguration )
      {
        ColumnSortCommand.ThrowIfNull( synchronizationContext, "synchronizationContext" );
        ColumnSortCommand.ThrowIfNull( detailConfiguration, "detailConfiguration" );
        DetailConfigurationUpdateColumnSortCommand.ThrowIfDetailsNotFlatten( detailConfiguration );

        if( !synchronizationContext.Own )
          return;

        if( detailConfiguration.DetailDescription == null )
          return;

        var dataGridControl = detailConfiguration.DataGridControl;
        if( dataGridControl == null )
          return;

        var rootDataGridContext = dataGridControl.DataGridContext;
        var masterSortDescriptions = rootDataGridContext.Items.SortDescriptions;
        var detailSortDescriptions = detailConfiguration.SortDescriptions;

        // There is nothing to synchronize.
        if( ( masterSortDescriptions.Count == 0 ) && ( detailSortDescriptions.Count == 0 ) )
          return;

        using( this.DeferResortHelper( detailConfiguration ) )
        {
          this.SynchronizeDetailSortDescriptions( masterSortDescriptions, detailSortDescriptions, detailConfiguration );
        }
      }

      private void SynchronizeDetailSortDescriptions( SortDescriptionCollection masterSortDescriptions, SortDescriptionCollection detailSortDescriptions, DetailConfiguration detailConfiguration )
      {
        ColumnSortCommand.ThrowIfNull( masterSortDescriptions, "masterSortDescriptions" );
        ColumnSortCommand.ThrowIfNull( detailSortDescriptions, "detailSortDescriptions" );

        var itemPropertyMap = detailConfiguration.ItemPropertyMap;
        ColumnSortCommand.ThrowIfNull( itemPropertyMap, "itemPropertyMap" );

        var masterSortDescriptionsCount = masterSortDescriptions.Count;
        if( masterSortDescriptionsCount > 0 )
        {
          var insertionIndex = 0;

          for( int i = 0; i < masterSortDescriptionsCount; i++ )
          {
            var sortDescription = masterSortDescriptions[ i ];
            var detailPropertyName = default( string );

            if( !DataGridItemPropertyMapHelper.TryGetDetailColumnName( itemPropertyMap, sortDescription.PropertyName, out detailPropertyName ) )
              continue;

            var detailDirection = sortDescription.Direction;

            if( insertionIndex < detailSortDescriptions.Count )
            {
              var detailSortDescription = detailSortDescriptions[ insertionIndex ];

              if( ( detailSortDescription.PropertyName != detailPropertyName ) || ( detailSortDescription.Direction != detailDirection ) )
              {
                detailSortDescriptions[ insertionIndex ] = new SortDescription( detailPropertyName, detailDirection );
              }
            }
            else
            {
              detailSortDescriptions.Add( new SortDescription( detailPropertyName, detailDirection ) );
            }

            insertionIndex++;
          }

          while( insertionIndex < detailSortDescriptions.Count )
          {
            detailSortDescriptions.RemoveAt( insertionIndex );
          }
        }
        else if( detailSortDescriptions.Count > 0 )
        {
          detailSortDescriptions.Clear();
        }
      }
    }

    private sealed class DetailConfigurationAddGroupCommand : ColumnAddGroupCommand
    {
      internal DetailConfigurationAddGroupCommand( DetailConfiguration detailConfiguration )
      {
        DetailConfigurationAddGroupCommand.ThrowIfNull( detailConfiguration, "detailConfiguration" );

        m_detailConfiguration = new WeakReference( detailConfiguration );
      }

      #region GroupDescriptions Protected Property

      protected override ObservableCollection<GroupDescription> GroupDescriptions
      {
        get
        {
          var detailConfiguration = this.DetailConfiguration;
          if( detailConfiguration == null )
            return null;

          return detailConfiguration.GroupDescriptions;
        }
      }

      #endregion

      #region DetailConfiguration Private Property

      private DetailConfiguration DetailConfiguration
      {
        get
        {
          return m_detailConfiguration.Target as DetailConfiguration;
        }
      }

      private readonly WeakReference m_detailConfiguration;

      #endregion

      protected override string GetColumnName( ColumnBase column )
      {
        var detailConfiguration = this.DetailConfiguration;
        if( ( detailConfiguration == null ) || ( column == null ) )
          return null;

        var itemPropertyMap = detailConfiguration.ItemPropertyMap;
        if( ( itemPropertyMap != null ) && itemPropertyMap.IsMapping )
        {
          string fieldName;
          if( DataGridItemPropertyMapHelper.TryGetDetailColumnName( itemPropertyMap, column.FieldName, out fieldName ) )
            return fieldName;
        }
        else
        {
          return column.FieldName;
        }

        return null;
      }

      protected override GroupDescription GetGroupDescription( ColumnBase column )
      {
        var detailConfiguration = this.DetailConfiguration;
        if( detailConfiguration == null )
          return null;

        return base.GetGroupDescription( column );
      }

      protected override GroupConfiguration GetGroupConfiguration( ColumnBase column )
      {
        var detailConfiguration = this.DetailConfiguration;
        if( detailConfiguration == null )
          return null;

        return base.GetGroupConfiguration( column );
      }

      protected override bool CanExecuteCore( ColumnBase column, int index )
      {
        if( this.DetailConfiguration == null )
          return false;

        return base.CanExecuteCore( column, index );
      }

      protected override void ExecuteCore( ColumnBase column, int index )
      {
        var detailConfig = this.DetailConfiguration;
        if( detailConfig == null )
          return;

        var dataGridControl = detailConfig.DataGridControl;
        IDisposable disposable = null;

        try
        {
          if( dataGridControl != null )
          {
            disposable = dataGridControl.SetQueueBringIntoViewRestrictions( AutoScrollCurrentItemSourceTriggers.CollectionViewCurrentItemChanged );
          }

          base.ExecuteCore( column, index );
        }
        finally
        {
          if( disposable != null )
          {
            disposable.Dispose();
          }
        }
      }
    }
  }
}
