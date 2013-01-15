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
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using Xceed.Wpf.DataGrid.Views;
using System.ComponentModel;
using Xceed.Wpf.DataGrid.Markup;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DetailConfiguration : DependencyObject, IWeakEventListener, ISupportInitialize, INotifyPropertyChanged
  {
    #region Constructors

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

    internal DetailConfiguration( bool isSynchronizedWithParent )
    {
      if( !isSynchronizedWithParent )
      {
        this.SetColumns( new ColumnCollection( this.DataGridControl, this ) );
        this.Columns.CollectionChanged += new NotifyCollectionChangedEventHandler( this.OnColumnsCollectionChanged );
        VisibilityChangedEventManager.AddListener( this.Columns, this );

        this.SetVisibleColumns( new ReadOnlyColumnCollection() );

        m_columnsByVisiblePosition = new HashedLinkedList<ColumnBase>();

        this.SetGroupLevelDescriptions( new GroupLevelDescriptionCollection() );
      }

      this.SetHeaders( new ObservableCollection<DataTemplate>() );
      this.SetFooters( new ObservableCollection<DataTemplate>() );

      this.SetDetailConfigurations( new DetailConfigurationCollection( this.DataGridControl, this ) );

      CollectionChangedEventManager.AddListener( this.DetailConfigurations, this );
    }

    internal DetailConfiguration( DataGridContext synchronizedParent )
      : this( true )
    {
      m_shouldCreateColumns = false;

      this.SetColumns( synchronizedParent.Columns );
      this.SetVisibleColumns( synchronizedParent.VisibleColumns );
      m_columnsByVisiblePosition = synchronizedParent.ColumnsByVisiblePosition;

      m_sortDescriptions = synchronizedParent.Items.SortDescriptions;
      m_groupDescriptions = synchronizedParent.Items.GroupDescriptions;

      this.SetGroupLevelDescriptions( synchronizedParent.GroupLevelDescriptions );
    }

    public DetailConfiguration()
      : this( false )
    {
    }


    #endregion

    #region Static Fields

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

    #endregion

    #region Static Methods

    internal static void AddDefaultHeadersFooters( ObservableCollection<DataTemplate> headersCollection, ObservableCollection<DataTemplate> footersCollection )
    {
      headersCollection.Insert( 0, DetailConfiguration.DefaultColumnManagerRowTemplate );
      headersCollection.Insert( 0, DetailConfiguration.DefaultHeaderSpacerTemplate );
    }

    internal static void SynchronizeDetailConfigurations(
      DataGridDetailDescriptionCollection dataGridDetailDescriptionCollection,
      DetailConfigurationCollection detailConfigurationCollection,
      bool autoCreateDetailConfigurations,
      bool autoCreateForeignKeyConfigurations,
      bool autoRemoveUnassociated )
    {
      HashSet<DetailConfiguration> detailConfigWithAssociation = new HashSet<DetailConfiguration>();
      DetailConfiguration detailConfig;

      foreach( DataGridDetailDescription detailDescription in dataGridDetailDescriptionCollection )
      {
        detailConfig = detailConfigurationCollection[ detailDescription.RelationName ];

        if( detailConfig == null )
        {
          if( autoCreateDetailConfigurations )
          {
            detailConfig = new DetailConfiguration();

            detailConfig.SetIsAutoCreated( true );
            detailConfig.RelationName = detailDescription.RelationName;
            detailConfig.AutoCreateForeignKeyConfigurations = autoCreateForeignKeyConfigurations;
            detailConfig.AutoRemoveColumnsAndDetailConfigurations = autoRemoveUnassociated;
            detailConfigurationCollection.Add( detailConfig );
          }
        }

        if( detailConfig != null )
        {
          detailConfigWithAssociation.Add( detailConfig );
          detailConfig.SynchronizeWithDetailDescription( detailDescription );
        }
      }

      for( int i = detailConfigurationCollection.Count - 1; i >= 0; i-- )
      {
        detailConfig = detailConfigurationCollection[ i ];

        if( !detailConfigWithAssociation.Contains( detailConfig ) )
        {
          detailConfig.DetachFromDetailDescription();

          if( ( autoRemoveUnassociated ) && ( detailConfig.IsAutoCreated ) )
          {
            DetailConfiguration.CleanupDetailConfigurations( detailConfig.DetailConfigurations, autoRemoveUnassociated );
            detailConfigurationCollection.RemoveAt( i );
          }
        }
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

    #endregion

    // PUBLIC PROPERTIES

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

    #region AutoCreateDetailConfigurations Property

    public static readonly DependencyProperty AutoCreateDetailConfigurationsProperty =
      DataGridControl.AutoCreateDetailConfigurationsProperty.AddOwner( typeof( DetailConfiguration ), new FrameworkPropertyMetadata( true ) );

    public bool AutoCreateDetailConfigurations
    {
      get
      {
        return ( bool )this.GetValue( DetailConfiguration.AutoCreateDetailConfigurationsProperty );
      }
      set
      {
        this.SetValue( DetailConfiguration.AutoCreateDetailConfigurationsProperty, value );
      }
    }

    #endregion AutoCreateColumns Property

    #region AutoRemoveColumnsAndDetailConfigurations Property

    public static readonly DependencyProperty AutoRemoveColumnsAndDetailConfigurationsProperty = 
      DataGridControl.AutoRemoveColumnsAndDetailConfigurationsProperty.AddOwner( typeof( DetailConfiguration ) );

    public bool AutoRemoveColumnsAndDetailConfigurations
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.AutoRemoveColumnsAndDetailConfigurationsProperty );
      }
      set
      {
        this.SetValue( DataGridControl.AutoRemoveColumnsAndDetailConfigurationsProperty, value );
      }
    }

    #endregion AutoRemoveColumnsAndDetailConfigurations Property

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

    #endregion Columns Read-Only Property

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


    #region Events

    internal event EventHandler<EventArgs> RequestingDelayBringIntoViewAndFocusCurrent;

    #endregion


    // INTERNAL PROPERTIES

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
        return m_columnsByVisiblePosition;
      }
    }

    private HashedLinkedList<ColumnBase> m_columnsByVisiblePosition;

    #endregion

    #region ItemsSourceFieldDescriptors Property

    internal Dictionary<string, ItemsSourceHelper.FieldDescriptor> ItemsSourceFieldDescriptors
    {
      get
      {
        return m_itemsSourceFieldDescriptors;
      }
      set
      {
        m_itemsSourceFieldDescriptors = value;
      }
    }

    private Dictionary<string, ItemsSourceHelper.FieldDescriptor> m_itemsSourceFieldDescriptors; // = null

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

    #endregion

    #region GroupDescriptions Read-Only Property

    internal ObservableCollection<GroupDescription> GroupDescriptions
    {
      get
      {
        return m_groupDescriptions;
      }
    }

    #endregion

    #region DataGridControl Property

    internal DataGridControl DataGridControl
    {
      get
      {
        if( m_containingCollection == null )
          return null;

        return m_containingCollection.DataGridControl;
      }
    }

    #endregion DataGridControl Property

    #region ParentDetailConfiguration Property

    internal DetailConfiguration ParentDetailConfiguration
    {
      get
      {
        if( m_containingCollection == null )
          return null;

        return m_containingCollection.ParentDetailConfiguration;
      }
    }

    #endregion ParentDetailConfiguration Property

    #region IsAutoCreated Property

    internal bool IsAutoCreated
    {
      get
      {
        return m_isAutoCreated;
      }
    }

    internal void SetIsAutoCreated( bool newValue )
    {
      m_isAutoCreated = newValue;
    }

    private bool m_isAutoCreated = false;

    #endregion

    #region RecyclingManager Read-Only property

    internal RecyclingManager RecyclingManager
    {
      get
      {
        return m_recyclingManager;
      }
    }

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

    #region Protected Methods

    protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnPropertyChanged( e );

      if( e.Property == DetailConfiguration.GroupConfigurationSelectorProperty )
      {
        if( this.GroupConfigurationSelectorChanged != null )
        {
          this.GroupConfigurationSelectorChanged( this, EventArgs.Empty );
        }
      }
    }

    #endregion

    #region Internal Methods

    internal void AddDefaultHeadersFooters()
    {
      if( m_defaultHeadersFootersAdded )
        return;

      m_defaultHeadersFootersAdded = true;

      DetailConfiguration.AddDefaultHeadersFooters( this.Headers, this.Footers );
    }

    internal void SynchronizeForeignKeyConfigurations()
    {
      if( m_detailDescription == null )
        return;

      ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView(
        this.Columns.FieldNameToColumnDictionary,
        m_detailDescription.ItemProperties,
        this.AutoCreateForeignKeyConfigurations );
    }

    #endregion

    #region Private Methods

    private void SynchronizeWithDetailDescription( DataGridDetailDescription detailDescription )
    {
      if( detailDescription != m_detailDescription )
      {
        this.DetachFromDetailDescription();

        m_shouldCreateColumns = true;
        m_detailDescription = detailDescription;

        DataGridSortDescriptionCollection dataGridSortDescriptionCollection = null;

        if( detailDescription != null )
        {
          //register to the collectionchanged of the DataGridDetailDescription collection of the detailDescription matching this one.
          dataGridSortDescriptionCollection = detailDescription.DataGridSortDescriptions;
          m_groupDescriptions = detailDescription.GroupDescriptions;

          CollectionChangedEventManager.AddListener( detailDescription.ItemProperties, this );
          CollectionChangedEventManager.AddListener( detailDescription.DetailDescriptions, this );
        }
        else
        {
          dataGridSortDescriptionCollection = new DataGridSortDescriptionCollection();
          m_groupDescriptions = new GroupDescriptionCollection();
        }

        m_sortDescriptions = dataGridSortDescriptionCollection;
        m_sortDescriptionsSyncContext = null; //clear it, if it was ever set!


        //This update is required since there might be some columns in the Columns collection after the XAML parsing of the DetailConfiguration
        DataGridContext.SynchronizeSortProperties( dataGridSortDescriptionCollection.SyncContext, m_sortDescriptions, this.Columns );

        //This update is required since we want the GroupLevelDescriptions to be created if DetailConfiguration in XAML
        //contains GroupDescriptions
        DataGridContext.UpdateGroupLevelDescriptions( this.GroupLevelDescriptions, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ), this.GroupDescriptions, this.Columns );

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

          DetailConfiguration.SynchronizeDetailConfigurations(
            detailDescription.DetailDescriptions,
            this.DetailConfigurations,
            this.AutoCreateDetailConfigurations,
            this.AutoCreateForeignKeyConfigurations,
            this.AutoRemoveColumnsAndDetailConfigurations );
        }
      }

      if( detailDescription != null )
      {
        ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView(
          this.Columns.FieldNameToColumnDictionary,
          detailDescription.ItemProperties,
          this.AutoCreateForeignKeyConfigurations );
      }
    }

    private void DetachFromDetailDescription()
    {
      if( m_detailDescription != null )
      {
        CollectionChangedEventManager.RemoveListener( m_detailDescription.ItemProperties, this );
        CollectionChangedEventManager.RemoveListener( m_detailDescription.DetailDescriptions, this );
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
      m_detailDescription = null;

      m_recyclingManager.Clear();
    }

    private void SortDescriptions_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      bool runSynchronization = true;

      this.RequestDelayBringIntoViewAndFocusCurrent();

      //If the resort is defered, it's because another section of code is already taking care of the update... Do not process the change notification
      DataGridSortDescriptionCollection dataGridSortDescriptions = m_sortDescriptions as DataGridSortDescriptionCollection;
      if( dataGridSortDescriptions != null )
        runSynchronization = !dataGridSortDescriptions.IsResortDefered;

      if( runSynchronization == true )
      {
        //No need to protect against SortDescriptions being null, since this reacts to the sort descriptions changes
        DataGridContext.SynchronizeSortProperties( this.SortDescriptionsSyncContext, m_sortDescriptions, this.Columns );
      }
    }

    private void GroupDescriptions_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      this.RequestDelayBringIntoViewAndFocusCurrent();
      DataGridContext.UpdateGroupLevelDescriptions( this.GroupLevelDescriptions, e, m_groupDescriptions, this.Columns );
    }

    private void OnColumnsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      //call the method that will detect if the CurrentColumn should be removed based on the changes in the Columns collection.
      this.HandleCurrentColumnRemove( e );

      this.RequestDelayBringIntoViewAndFocusCurrent();

      //Then call the method that will update the VisibleColumns
      DataGridContext.HandleColumnsCollectionChanged( e, this.Columns,
        this.VisibleColumns as ReadOnlyColumnCollection,
        this.ColumnsByVisiblePosition,
        this.SortDescriptionsSyncContext,
        m_sortDescriptions );

      if( m_detailDescription != null )
      {
        switch( e.Action )
        {
          case NotifyCollectionChangedAction.Reset:
            {
              ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView(
                this.Columns.FieldNameToColumnDictionary,
                m_detailDescription.ItemProperties,
                this.AutoCreateForeignKeyConfigurations );

              break;
            }

          case NotifyCollectionChangedAction.Add:
          case NotifyCollectionChangedAction.Replace:
            {
              IList newItems = e.NewItems;
              int count = newItems.Count;
              Dictionary<string, ColumnBase> newColumns = new Dictionary<string, ColumnBase>( count );

              for( int i = 0; i < count; i++ )
              {
                ColumnBase column = newItems[ i ] as ColumnBase;
                newColumns.Add( column.FieldName, column );
              }

              ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView(
                newColumns,
                m_detailDescription.ItemProperties,
                this.AutoCreateForeignKeyConfigurations );

              break;
            }
        }
      }
    }

    private void HandleCurrentColumnRemove( NotifyCollectionChangedEventArgs e )
    {
      bool removeCurrentColumn = false;
      ColumnBase currentColumn = this.CurrentColumn;

      if( e.Action == NotifyCollectionChangedAction.Reset )
      {
        //Reset means that collection was cleared.
        removeCurrentColumn = true;
      }
      else if( ( e.Action == NotifyCollectionChangedAction.Remove ) && ( e.OldItems.Contains( currentColumn ) == true ) )
      {
        //Remove of at least the current column
        removeCurrentColumn = true;
      }
      else if( ( e.Action == NotifyCollectionChangedAction.Replace ) && ( e.OldItems.Contains( currentColumn ) == true )
        && ( e.NewItems.Contains( currentColumn ) == false ) )
      {
        //Replace in which at least the current column was "replaced" by another (current column not present in new items )
        removeCurrentColumn = true;
      }

      //If we computed that current columns should be cleared
      if( removeCurrentColumn == true )
      {
        //reset current column.
        this.CurrentColumn = DataGridContext.GetClosestColumn( currentColumn, this.ColumnsByVisiblePosition );
      }

    }

    private void OnDetailConfigurationsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      DataGridDetailDescriptionCollection detailDescriptions = null;
      if( m_detailDescription != null )
      {
        detailDescriptions = m_detailDescription.DetailDescriptions;
      }

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

    private void RequestDelayBringIntoViewAndFocusCurrent()
    {
      if( this.RequestingDelayBringIntoViewAndFocusCurrent != null )
      {
        this.RequestingDelayBringIntoViewAndFocusCurrent( this, EventArgs.Empty );
      }
    }

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      //check if the event comes from a INotifyCollectionChanged collection
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        NotifyCollectionChangedEventArgs ncce = ( NotifyCollectionChangedEventArgs )e;

        if( sender is DetailConfigurationCollection )
        {
          this.OnDetailConfigurationsChanged( sender, ncce );
          return true;
        }
        else if( sender is SortDescriptionCollection )
        {
          this.SortDescriptions_CollectionChanged( sender, ncce );
          return true;
        }
        else if( sender is ObservableCollection<GroupDescription> )
        {
          this.GroupDescriptions_CollectionChanged( sender, ncce );
          return true;
        }
        else if( sender is DataGridDetailDescriptionCollection )
        {
          if( m_detailDescription != null )
          {
            DetailConfiguration.SynchronizeDetailConfigurations(
              m_detailDescription.DetailDescriptions,
              this.DetailConfigurations,
              this.AutoCreateDetailConfigurations,
              this.AutoCreateForeignKeyConfigurations,
              this.AutoRemoveColumnsAndDetailConfigurations );
          }

          return true;
        }
        else if( sender is DataGridItemPropertyCollection )
        {
          // Ensure to update ForeignKeyDescriptions and ForeignKeyConfigurations
          // when ItemProperties change on DetailDescription
          if( m_detailDescription != null )
          {
            ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView(
              this.Columns.FieldNameToColumnDictionary,
              m_detailDescription.ItemProperties,
              this.AutoCreateForeignKeyConfigurations );
          }
          return true;
        }
      }

      if( managerType == typeof( VisibilityChangedEventManager ) )
      {
        var wrappedEventArgs = ( ColumnCollection.WrappedEventEventArgs )e;

        DataGridContext.UpdateVisibleColumns(
          this.Columns, this.VisibleColumns as ReadOnlyColumnCollection, this.ColumnsByVisiblePosition,
          new object[] { wrappedEventArgs.WrappedSender }, wrappedEventArgs.WrappedEventArgs as ColumnVisiblePositionChangedEventArgs );

        return true;
      }

      return false;
    }

    #endregion

    #region ISupportInitialize Members

    public void BeginInit()
    {
      m_beginInitCount++;

      //It is determined at construction of the DetailConfiguration if it is synchronized with its parent, therefore, I can assume it is set appropriatly here
      if( m_columnsInitDisposable == null )
      {
        m_columnsInitDisposable = this.Columns.DeferColumnAdditionMessages();
      }
    }

    public void EndInit()
    {
      if( m_beginInitCount > 0 )
        m_beginInitCount--;

      if( m_beginInitCount == 0 )
      {
        if( m_columnsInitDisposable != null )
        {
          m_columnsInitDisposable.Dispose();
          m_columnsInitDisposable = null;
        }
      }
    }

    private int m_beginInitCount = 0;

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged( string propertyName )
    {
      if( this.PropertyChanged != null )
      {
        this.PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
      }
    }

    #endregion

    #region ContainingCollection Methods

    internal void AttachToContainingCollection( DetailConfigurationCollection detailConfigurationCollection )
    {
      if( m_containingCollection == detailConfigurationCollection )
        return;

      Debug.Assert( detailConfigurationCollection != null, "For clarity purposes, you should use Detach in this context... or remove this assert." );

      if( detailConfigurationCollection == null )
      {
        this.DetachFromContainingCollection();
      }
      else
      {
        m_containingCollection = detailConfigurationCollection;

        if( this.Columns != null )
          this.Columns.DataGridControl = this.DataGridControl;

        if( this.DetailConfigurations != null )
          this.DetailConfigurations.DataGridControl = this.DataGridControl;
      }
    }

    internal void DetachFromContainingCollection()
    {
      m_containingCollection = null;

      if( this.Columns != null )
        this.Columns.DataGridControl = this.DataGridControl;

      if( this.DetailConfigurations != null )
        this.DetailConfigurations.DataGridControl = this.DataGridControl;
    }

    #endregion ContainingCollection Methods

    #region Private Fields

    private DetailConfigurationCollection m_containingCollection; // = null

    private SortDescriptionCollection m_sortDescriptions; // = null
    private ObservableCollection<GroupDescription> m_groupDescriptions; // = null

    private bool m_shouldCreateColumns = true;
    private IDisposable m_columnsInitDisposable; // = null
    private bool m_defaultHeadersFootersAdded; // = false

    private readonly RecyclingManager m_recyclingManager = new RecyclingManager();

    private DataGridDetailDescription m_detailDescription; // = null 

    #endregion
  }
}
