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
using System.Linq;
using System.Printing;
using System.Security;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Utils.Wpf;
using Xceed.Utils.Wpf.DragDrop;
using Xceed.Wpf.DataGrid.Export;
using Xceed.Wpf.DataGrid.Utils;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  [TemplatePart( Name = "PART_ScrollViewer", Type = typeof( ScrollViewer ) )]
  [StyleTypedProperty( Property = "ItemContainerStyle", StyleTargetType = typeof( DataRow ) )] // Blend has to be able to create an instance of this for the style edition so it can't be typeof( Row )
  [StyleTypedProperty( Property = "CellErrorStyle", StyleTargetType = typeof( Cell ) )]
  public partial class DataGridControl : ItemsControl, INotifyPropertyChanged, IDocumentPaginatorSource, IWeakEventListener
  {
    #region Static Fields

    internal static readonly string SelectionModeToUsePropertyName = PropertyHelper.GetPropertyName( ( DataGridControl d ) => d.SelectionModeToUse );

    #endregion

    static DataGridControl()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( DataGridControl ), new FrameworkPropertyMetadata( TableflowView.GetDefaultStyleKey( typeof( TableflowView ), typeof( DataGridControl ) ) ) );

      ItemsControl.ItemsSourceProperty.OverrideMetadata( typeof( DataGridControl ), new FrameworkPropertyMetadata( null, new CoerceValueCallback( DataGridControl.ItemsSourceCoerceCallback ) ) );
      ItemsControl.GroupStyleSelectorProperty.OverrideMetadata( typeof( DataGridControl ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnGroupStyleSelectorChanged ) ) );

      KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata( typeof( DataGridControl ), new FrameworkPropertyMetadata( KeyboardNavigationMode.Contained ) );
      KeyboardNavigation.TabNavigationProperty.OverrideMetadata( typeof( DataGridControl ), new FrameworkPropertyMetadata( KeyboardNavigationMode.None ) );

      FrameworkElement.FlowDirectionProperty.OverrideMetadata( typeof( DataGridControl ), new FrameworkPropertyMetadata( new PropertyChangedCallback( DataGridControl.OnFlowDirectionPropertyChanged ) ) );

      DataGridControl.ColumnsProperty = DataGridControl.ColumnsPropertyKey.DependencyProperty;
      DataGridControl.VisibleColumnsProperty = DataGridControl.VisibleColumnsPropertyKey.DependencyProperty;
      DataGridControl.SelectedItemsProperty = DataGridControl.SelectedItemsPropertyKey.DependencyProperty;
      DataGridControl.SelectedItemRangesProperty = DataGridControl.SelectedItemRangesPropertyKey.DependencyProperty;
      DataGridControl.SelectedCellRangesProperty = DataGridControl.SelectedCellRangesPropertyKey.DependencyProperty;
      DataGridControl.ParentDataGridControlProperty = DataGridControl.ParentDataGridControlPropertyKey.DependencyProperty;
      DataGridControl.DataGridContextProperty = DataGridControl.DataGridContextPropertyKey.DependencyProperty;
      DataGridControl.StatContextProperty = DataGridControl.StatContextPropertyKey.DependencyProperty;
      DataGridControl.GroupLevelDescriptionsProperty = DataGridControl.GroupLevelDescriptionsPropertyKey.DependencyProperty;
      DataGridControl.HasExpandedDetailsProperty = DataGridControl.HasExpandedDetailsPropertyKey.DependencyProperty;
    }

    public DataGridControl()
    {
      m_selectionChangerManager = new SelectionManager( this );
      m_columnManagerRowConfiguration = new ColumnManagerRowConfiguration();

      this.SetValue( DataGridControl.ParentDataGridControlPropertyKey, this );

      //set the FixedItem for the gridControl to NotSet, this is to prevent problems with nested DataGridControls
      DataGridControl.SetFixedItem( this, DataGridControl.NotSet );

      this.CommandBindings.Add( new CommandBinding( DataGridCommands.ExpandGroup, this.OnExpandGroupExecuted, this.OnExpandGroupCanExecute ) );
      this.CommandBindings.Add( new CommandBinding( DataGridCommands.CollapseGroup, this.OnCollapseGroupExecuted, this.OnCollapseGroupCanExecute ) );
      this.CommandBindings.Add( new CommandBinding( DataGridCommands.ToggleGroupExpansion, this.OnToggleGroupExecuted, this.OnToggleGroupCanExecute ) );
      this.CommandBindings.Add( new CommandBinding( ApplicationCommands.SelectAll, this.OnSelectAllExecuted, this.OnSelectAllCanExecute ) );

      // We keep a references to be able to remove the CommandBindings when they are not required (feature disabled)
      m_refreshCommandBinding = new CommandBinding( DataGridCommands.Refresh, this.OnRefreshExecuted, this.OnRefreshCanExecute );
      this.CommandBindings.Add( m_refreshCommandBinding );

      // We keep a references to be able to remove the CommandBindings when they are not required (feature disabled)
      m_copyCommandBinding = new CommandBinding( ApplicationCommands.Copy, this.OnCopyExecuted, this.OnCopyCanExecute );
      this.CommandBindings.Add( m_copyCommandBinding );

      // The delete command is not enabled by default, so don't add it to CommandBindings
      m_deleteCommandBinding = new CommandBinding( ApplicationCommands.Delete, this.OnDeleteExecuted, this.OnDeleteCanExecute );

      DataGridContext dataGridContext = new DataGridContext( null, this, null, this.Items, null );
      m_customItemContainerGenerator = CustomItemContainerGenerator.CreateGenerator( this, this.Items, dataGridContext );
      m_customItemContainerGenerator.DetailsChanged += OnDetailsChanged;

      DataGridControl.SetDataGridContext( this, dataGridContext );
      m_localDataGridContext = dataGridContext;

      //so that at least one DataGridContext is always current
      dataGridContext.SetIsCurrent( true );
      this.SetCurrentDataGridContextHelper( dataGridContext );

      this.SetValue( DataGridControl.ColumnsPropertyKey, dataGridContext.Columns );
      this.SetValue( DataGridControl.VisibleColumnsPropertyKey, dataGridContext.VisibleColumns );
      this.SetValue( DataGridControl.GroupLevelDescriptionsPropertyKey, dataGridContext.GroupLevelDescriptions );
      this.SetValue( DataGridControl.SelectedItemsPropertyKey, dataGridContext.SelectedItems );
      this.SetValue( DataGridControl.SelectedItemRangesPropertyKey, new SelectionItemRangeCollection( dataGridContext.SelectedItemsStore ) );
      this.SetValue( DataGridControl.SelectedCellRangesPropertyKey, new SelectionCellRangeCollection( dataGridContext.SelectedCellsStore ) );

      // Apparently, we don't need to unsubscribe from these event handlers. These event 
      // subscriptions do not "root" the grid and we observed no leak cause be these.
      // We did not investigate why that is and we should keep an eye on it.
      this.GroupStyle.CollectionChanged += new NotifyCollectionChangedEventHandler( GroupStyle_CollectionChanged );


      if( DesignerProperties.GetIsInDesignMode( this ) )
      {
        // Workaround for VS2008's know issue (the DataGrid's Template is not active).
        this.ClipToBounds = true;
      }

      this.Loaded += new RoutedEventHandler( DataGridControl_Loaded );
    }

    #region INITIALIZATION

    public override void BeginInit()
    {
      m_initCount++;
      if( m_initCount != 1 )
        return;

      Debug.Assert( m_deferColumnsUpdate == null );
      m_deferColumnsUpdate = this.DataGridContext.DeferColumnsUpdate();
      Debug.Assert( m_deferColumnsUpdate != null );

      m_deferColumnsNotifications = this.Columns.DeferNotifications();

      var dataGridCollectionViewBase = this.DataGridContext.Items as DataGridCollectionViewBase;
      if( dataGridCollectionViewBase != null )
      {
        m_deferItemPropertiesUpdate = dataGridCollectionViewBase.ItemProperties.DeferCollectionChanged();
      }

      base.BeginInit();
    }

    public override void EndInit()
    {
      m_initCount--;
      if( m_initCount != 0 )
        return;

      // Set a view if none is set at this point.  This will also make sure that eventual implicit style declarations for the default View in user applications will work.
      if( this.View == null )
      {
        this.View = this.GetDefaultView() as UIViewBase;
      }

      if( this.ItemsSourceChangedDelayed )
      {
        this.ItemsSourceChangedDelayed = false;
        this.ProcessDelayedItemsSourceChanged();
      }

      var dataGridContext = this.DataGridContext;
      var columnManager = dataGridContext.ColumnManager;

      columnManager.Initialize( this );
      columnManager.SetFixedColumnCount( TableView.GetFixedColumnCount( dataGridContext ) );

      //see if any of the GroupLevelDescriptions are matched to a column but aren't bound ( case 102437 ) and update then (bind them) if necessary
      dataGridContext.SetupTitleBindingForGroupLevelDescriptions();

      if( m_deferItemPropertiesUpdate != null )
      {
        var collectionViewDisposable = m_deferItemPropertiesUpdate;
        m_deferItemPropertiesUpdate = null;
        collectionViewDisposable.Dispose();
      }

      var columnsNotificationsDisposable = m_deferColumnsNotifications;
      m_deferColumnsNotifications = null;
      columnsNotificationsDisposable.Dispose();

      Debug.Assert( m_deferColumnsUpdate != null );
      var columnsUpdateDisposable = m_deferColumnsUpdate;
      m_deferColumnsUpdate = null;
      columnsUpdateDisposable.Dispose();

      m_customItemContainerGenerator.ForceReset = true;

      base.EndInit();
    }

    private void DataGridControl_Loaded( object sender, RoutedEventArgs e )
    {
      this.InvalidateViewStyle();

      // Always set this as ParentDataGridControl to stop inheritance
      // when a grid is inside a another grid
      this.SetValue( ParentDataGridControlPropertyKey, this );
      this.HookToUnloaded();

      // The layout pass was inhibited while the DataGridControl was unloaded
      // to make the parent control load faster.  A new layout pass
      // must be forced to render the datagrid content correctly.  The layout
      // pass isn't inhibited when the grid is use for printing purpose.
      if( ( this.DeferInitialLayoutPass ) && ( m_isFirstTimeLoaded )  )
      {
        m_isFirstTimeLoaded = false;

        this.Dispatcher.BeginInvoke( new Action( this.InvalidateMeasure ) );
      }
    }

    private void HookToUnloaded()
    {
      this.UnhookToUnloaded();

      m_parentWindow = null;
      FrameworkElement parentWindow;
      DependencyObject parent = this;

      do
      {
        parent = TreeHelper.GetParent( parent );
        parentWindow = parent as Window;

        if( parentWindow == null )
        {
          parentWindow = parent as Page;
        }
      } while( ( parent != null ) && ( parentWindow == null ) );

      if( parentWindow == null )
      {
        parentWindow = this;
      }
      else
      {
        m_parentWindow = new WeakReference( parentWindow );
      }

      FrameworkElementUnloadedEventManager.AddListener( parentWindow, this );
    }

    private void UnhookToUnloaded()
    {
      var parentWindow = ( m_parentWindow != null ) ? m_parentWindow.Target as FrameworkElement : this;
      if( parentWindow == null )
        return;

      FrameworkElementUnloadedEventManager.RemoveListener( parentWindow, this );
    }

    private int m_initCount; //0
    private IDisposable m_deferColumnsUpdate; //null
    private IDisposable m_deferColumnsNotifications; //null
    private IDisposable m_deferItemPropertiesUpdate; //null

    #endregion

    #region ParentDataGridControl Attached Property

    internal static readonly DependencyPropertyKey ParentDataGridControlPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "ParentDataGridControl",
      typeof( DataGridControl ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static readonly DependencyProperty ParentDataGridControlProperty;

    [Obsolete( "GetParentDataGridControl is obsolete. Use DataGridContext.DataGridControl instead" )]
    public static DataGridControl GetParentDataGridControl( DependencyObject obj )
    {
      return ( DataGridControl )obj.GetValue( DataGridControl.ParentDataGridControlProperty );
    }

    #endregion

    #region DataGridContext Attached Property

    internal static readonly DependencyPropertyKey DataGridContextPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "DataGridContext",
      typeof( DataGridContext ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static readonly DependencyProperty DataGridContextProperty;

    public static DataGridContext GetDataGridContext( DependencyObject obj )
    {
      return ( DataGridContext )obj.GetValue( DataGridControl.DataGridContextProperty );
    }

    internal static void SetDataGridContext( DependencyObject obj, DataGridContext value )
    {
      obj.SetValue( DataGridControl.DataGridContextPropertyKey, value );
    }

    private readonly DataGridContext m_localDataGridContext; // = null

    #endregion

    #region Container Attached Property

    private static readonly DependencyPropertyKey ContainerPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "Container",
      typeof( FrameworkElement ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static readonly DependencyProperty ContainerProperty = DataGridControl.ContainerPropertyKey.DependencyProperty;

    public static FrameworkElement GetContainer( DependencyObject obj )
    {
      return ( FrameworkElement )obj.GetValue( DataGridControl.ContainerProperty );
    }

    internal static void SetContainer( DependencyObject obj, object value )
    {
      obj.SetValue( DataGridControl.ContainerPropertyKey, value );
    }

    internal static void ClearContainer( DependencyObject obj )
    {
      obj.ClearValue( DataGridControl.ContainerPropertyKey );
    }

    #endregion

    #region IsContainerPrepared Attached Property

    private static readonly DependencyPropertyKey IsContainerPreparedPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "IsContainerPrepared",
      typeof( bool ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( false ) );

    private static readonly DependencyProperty IsContainerPreparedProperty = DataGridControl.IsContainerPreparedPropertyKey.DependencyProperty;

    internal static bool GetIsContainerPrepared( DependencyObject obj )
    {
      return ( bool )obj.GetValue( DataGridControl.IsContainerPreparedProperty );
    }

    private static void SetIsContainerPrepared( DependencyObject obj, bool value )
    {
      obj.SetValue( DataGridControl.IsContainerPreparedPropertyKey, value );
    }

    private static void ClearIsContainerPrepared( DependencyObject obj )
    {
      obj.ClearValue( DataGridControl.IsContainerPreparedPropertyKey );
    }

    #endregion

    #region StatContext Attached Property

    internal static readonly DependencyPropertyKey StatContextPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "StatContext",
      typeof( object ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static readonly DependencyProperty StatContextProperty;

    public static object GetStatContext( DependencyObject obj )
    {
      return obj.GetValue( DataGridControl.StatContextProperty );
    }

    internal static void SetStatContext( DependencyObject obj, object statContext )
    {
      obj.SetValue( DataGridControl.StatContextPropertyKey, statContext );
    }

    #endregion

    #region HasExpandedDetails Attached Property

    internal static readonly DependencyPropertyKey HasExpandedDetailsPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "HasExpandedDetails",
      typeof( bool ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.Inherits ) );

    public static readonly DependencyProperty HasExpandedDetailsProperty;

    public static bool GetHasExpandedDetails( DependencyObject obj )
    {
      return ( bool )obj.GetValue( DataGridControl.HasExpandedDetailsProperty );
    }

    internal static void SetHasExpandedDetails( DependencyObject obj, bool value )
    {
      obj.SetValue( DataGridControl.HasExpandedDetailsPropertyKey, value );
    }

    #endregion

    #region Columns Read-Only Property

    private static readonly DependencyPropertyKey ColumnsPropertyKey = DependencyProperty.RegisterReadOnly(
      "Columns",
      typeof( ColumnCollection ),
      typeof( DataGridControl ),
      new PropertyMetadata( null ) );

    public static readonly DependencyProperty ColumnsProperty;

    public ColumnCollection Columns
    {
      get
      {
        return ( ColumnCollection )this.GetValue( DataGridControl.ColumnsProperty );
      }
    }

    #endregion

    #region VisibleColumns Read-Only Property

    private static readonly DependencyPropertyKey VisibleColumnsPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "VisibleColumns",
      typeof( ReadOnlyObservableCollection<ColumnBase> ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static readonly DependencyProperty VisibleColumnsProperty;

    // VisibleColumns is a ReadOnlyObservableCollection because it would be too confusing to 
    // have a ColumnCollection. For instance, if somebody would have set the VisiblePosition
    // of some columns and then used VisibleColumns.Insert to add them in the collection
    // at the right place, what should hold the real visible index value? The VisiblePosition
    // property or the index in the VisibleColumns collection?
    public ReadOnlyObservableCollection<ColumnBase> VisibleColumns
    {
      get
      {
        return ( ReadOnlyObservableCollection<ColumnBase> )this.GetValue( DataGridControl.VisibleColumnsProperty );
      }
    }

    #endregion

    #region ColumnsByVisiblePosition Read-Only Property

    internal HashedLinkedList<ColumnBase> ColumnsByVisiblePosition
    {
      get
      {
        return this.DataGridContext.ColumnsByVisiblePosition;
      }
    }

    #endregion

    #region CellEditorDisplayConditions Property

    public static readonly DependencyProperty CellEditorDisplayConditionsProperty = DependencyProperty.RegisterAttached(
      "CellEditorDisplayConditions",
      typeof( CellEditorDisplayConditions ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( CellEditorDisplayConditions.None, FrameworkPropertyMetadataOptions.Inherits ) );

    public CellEditorDisplayConditions CellEditorDisplayConditions
    {
      get
      {
        return ( CellEditorDisplayConditions )this.GetValue( DataGridControl.CellEditorDisplayConditionsProperty );
      }
      set
      {
        this.SetValue( DataGridControl.CellEditorDisplayConditionsProperty, value );
      }
    }

    #endregion

    #region DefaultCellEditors Property

    public Dictionary<Type, CellEditor> DefaultCellEditors
    {
      get
      {
        return m_defaultCellEditors;
      }
    }

    private readonly Dictionary<Type, CellEditor> m_defaultCellEditors = new Dictionary<Type, CellEditor>();

    #endregion

    #region CellErrorStyle Property

    public static readonly DependencyProperty CellErrorStyleProperty = DependencyProperty.RegisterAttached(
      "CellErrorStyle",
      typeof( Style ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    public Style CellErrorStyle
    {
      get
      {
        return ( Style )this.GetValue( DataGridControl.CellErrorStyleProperty );
      }

      set
      {
        this.SetValue( DataGridControl.CellErrorStyleProperty, value );
      }
    }

    #endregion

    #region HasValidationError Property

    private static readonly DependencyPropertyKey HasValidationErrorPropertyKey = DependencyProperty.RegisterReadOnly(
      "HasValidationError",
      typeof( bool ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( false ) );

    public static readonly DependencyProperty HasValidationErrorProperty = DataGridControl.HasValidationErrorPropertyKey.DependencyProperty;

    public bool HasValidationError
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.HasValidationErrorProperty );
      }
    }

    internal void SetHasValidationError( bool value )
    {
      if( value != this.HasValidationError )
      {
        if( value )
        {
          this.SetValue( DataGridControl.HasValidationErrorPropertyKey, value );
        }
        else
        {
          this.SetValue( DataGridControl.HasValidationErrorPropertyKey, DependencyProperty.UnsetValue );
        }
      }
    }

    #endregion

    #region ValidationMode Property

    [Obsolete( "The ValidationMode is obsolete. Refer to the Editing and Validating Data topic in the documentation.", true )]
    public static readonly DependencyProperty ValidationModeProperty = DependencyProperty.Register(
      "ValidationMode",
      typeof( ValidationMode ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( ValidationMode.RowEndingEdit ) );

    [Obsolete( "The ValidationMode is obsolete. Refer to the Editing and Validating Data topic in the documentation.", true )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public ValidationMode ValidationMode
    {
      get
      {
        return ValidationMode.RowEndingEdit;
      }
      set
      {
      }
    }

    #endregion

    #region SelectedItems Read-Only Property

    private static readonly DependencyPropertyKey SelectedItemsPropertyKey = DependencyProperty.RegisterReadOnly(
      "SelectedItems",
      typeof( IList ),
      typeof( DataGridControl ),
      new PropertyMetadata( null ) );

    public static readonly DependencyProperty SelectedItemsProperty;

    public IList SelectedItems
    {
      get
      {
        return ( IList )this.GetValue( DataGridControl.SelectedItemsProperty );
      }
    }

    #endregion

    #region SelectedRanges Read-Only Property

    private static readonly DependencyPropertyKey SelectedItemRangesPropertyKey = DependencyProperty.RegisterReadOnly(
      "SelectedItemRanges",
      typeof( IList<SelectionRange> ),
      typeof( DataGridControl ),
      new PropertyMetadata( null ) );

    public static readonly DependencyProperty SelectedItemRangesProperty;

    public IList<SelectionRange> SelectedItemRanges
    {
      get
      {
        return ( IList<SelectionRange> )this.GetValue( DataGridControl.SelectedItemRangesProperty );
      }
    }

    #endregion

    #region SelectedCellRanges Read-Only Property

    private static readonly DependencyPropertyKey SelectedCellRangesPropertyKey = DependencyProperty.RegisterReadOnly(
      "SelectedCellRanges",
      typeof( IList<SelectionCellRange> ),
      typeof( DataGridControl ),
      new PropertyMetadata( null ) );

    public static readonly DependencyProperty SelectedCellRangesProperty;

    public IList<SelectionCellRange> SelectedCellRanges
    {
      get
      {
        return ( IList<SelectionCellRange> )this.GetValue( DataGridControl.SelectedCellRangesProperty );
      }
    }

    #endregion

    #region SelectedItem Property

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
      "SelectedItem",
      typeof( object ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback( DataGridControl.OnSelectedItemChanged ), new CoerceValueCallback( DataGridControl.OnCoerceSelectedItem ) ) );

    public object SelectedItem
    {
      get
      {
        return this.GetValue( DataGridControl.SelectedItemProperty );
      }
      set
      {
        this.SetValue( DataGridControl.SelectedItemProperty, value );
      }
    }

    internal void SetSkipCoerceSelectedItemCheck( bool value )
    {
      m_skipCoerceSelectedItemCheck = value;
    }

    private static object OnCoerceSelectedItem( DependencyObject sender, object value )
    {
      var self = sender as DataGridControl;
      if( self == null )
        return value;

      if( ( value != null ) && ( !self.m_skipCoerceSelectedItemCheck ) )
      {
        if( !self.Items.Contains( value ) )
        {
          self.SelectedItemPropertyNeedCoerce = true;
          return DependencyProperty.UnsetValue;
        }
      }

      self.SelectedItemPropertyNeedCoerce = false;
      return value;
    }

    private static void OnSelectedItemChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( ( self == null ) || self.m_selectionChangerManager.IsActive )
        return;

      self.m_selectionChangerManager.Begin();

      try
      {
        if( e.NewValue == null )
        {
          self.m_selectionChangerManager.UnselectAll();
        }
        else
        {
          self.m_selectionChangerManager.SelectJustThisItem( self.DataGridContext, self.Items.IndexOf( e.NewValue ), e.NewValue );
        }
      }
      finally
      {
        self.m_selectionChangerManager.End( false, true );
      }
    }

    private bool m_skipCoerceSelectedItemCheck;

    #endregion

    #region SelectedIndex Property

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
      "SelectedIndex",
      typeof( int ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( -1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback( DataGridControl.OnSelectedIndexChanged ), new CoerceValueCallback( DataGridControl.OnCoerceSelectedIndex ) ) );

    public int SelectedIndex
    {
      get
      {
        return ( int )this.GetValue( DataGridControl.SelectedIndexProperty );
      }
      set
      {
        this.SetValue( DataGridControl.SelectedIndexProperty, value );
      }
    }

    private static object OnCoerceSelectedIndex( DependencyObject sender, object value )
    {
      var self = sender as DataGridControl;
      if( self == null )
        return value;

      var newValue = ( int )value;

      if( ( newValue < -1 ) || ( newValue >= self.Items.Count ) )
      {
        self.SelectedIndexPropertyNeedCoerce = true;
        return DependencyProperty.UnsetValue;
      }

      self.SelectedIndexPropertyNeedCoerce = false;
      return value;
    }

    private static void OnSelectedIndexChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( ( self == null ) || self.m_selectionChangerManager.IsActive )
        return;

      var newValue = ( int )e.NewValue;

      self.m_selectionChangerManager.Begin();

      try
      {
        if( ( newValue >= 0 ) && ( newValue < self.Items.Count ) )
        {
          //affect the item as the new selection
          self.m_selectionChangerManager.SelectJustThisItem( self.DataGridContext, newValue, self.Items[ newValue ] );
        }
        else
        {
          //clear the selection.
          self.m_selectionChangerManager.UnselectAll();
        }
      }
      finally
      {
        self.m_selectionChangerManager.End( false, true );
      }
    }

    #endregion

    #region GlobalSelectedItems Read-Only Property

    public IEnumerable GlobalSelectedItems
    {
      get
      {
        foreach( var context in this.SelectedContexts )
        {
          foreach( var selectedItem in context.SelectedItems )
          {
            yield return selectedItem;
          }
        }
      }
    }

    internal void NotifyGlobalSelectedItemsChanged()
    {
      this.OnPropertyChanged( "GlobalSelectedItems" );
    }

    #endregion

    #region SelectedContexts Read-Only Property

    public ReadOnlyCollection<DataGridContext> SelectedContexts
    {
      get
      {
        if( m_readOnlySelectedContexts == null )
        {
          m_readOnlySelectedContexts = new ReadOnlyCollection<DataGridContext>( m_selectedContexts );
        }

        return m_readOnlySelectedContexts;
      }
    }

    private readonly List<DataGridContext> m_selectedContexts = new List<DataGridContext>();
    private ReadOnlyCollection<DataGridContext> m_readOnlySelectedContexts;

    #endregion

    #region SelectionMode Property

    public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(
      "SelectionMode",
      typeof( SelectionMode ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( SelectionMode.Extended, new PropertyChangedCallback( DataGridControl.OnSelectionModeChanged ) ) );

    public SelectionMode SelectionMode
    {
      get
      {
        return ( SelectionMode )this.GetValue( DataGridControl.SelectionModeProperty );
      }
      set
      {
        this.SetValue( DataGridControl.SelectionModeProperty, value );
      }
    }

    internal SelectionMode SelectionModeToUse
    {
      get
      {
        return this.SelectionMode;
      }
    }

    private static void OnSelectionModeChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( self == null )
        return;
    }

    #endregion

    #region SelectionUnit Property

    public static readonly DependencyProperty SelectionUnitProperty = DependencyProperty.Register(
      "SelectionUnit",
      typeof( SelectionUnit ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( SelectionUnit.Row, new PropertyChangedCallback( DataGridControl.OnSelectionUnitChanged ) ) );

    public SelectionUnit SelectionUnit
    {
      get
      {
        return ( SelectionUnit )this.GetValue( DataGridControl.SelectionUnitProperty );
      }
      set
      {
        this.SetValue( DataGridControl.SelectionUnitProperty, value );
      }
    }

    private static void OnSelectionUnitChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( self == null )
        return;

      var selectionUnit = ( SelectionUnit )e.NewValue;

      self.m_selectionChangerManager.Begin();

      try
      {
        switch( selectionUnit )
        {
          case SelectionUnit.Row:
            {
              self.m_selectionChangerManager.UnselectAllCells();
              break;
            }
          case SelectionUnit.Cell:
            {
              self.m_selectionChangerManager.UnselectAllItems();
              break;
            }
        }
      }
      finally
      {
        self.m_selectionChangerManager.End( false, true );
      }
    }

    #endregion

    #region CurrentItem Property

    public object CurrentItem
    {
      get
      {
        return this.DataGridContext.CurrentItem;
      }
      set
      {
        if( value == this.DataGridContext.CurrentItem )
          return;

        this.DataGridContext.SetCurrent( value, null, null, this.DataGridContext.CurrentColumn, false, true, this.SynchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers.CurrentItemChanged );
      }
    }

    internal bool ShouldSynchronizeCurrentItem
    {
      get
      {
        return ( this.SynchronizeCurrent );
      }
    }

    #endregion

    #region CurrentColumn Property

    public ColumnBase CurrentColumn
    {
      get
      {
        return this.DataGridContext.CurrentColumn;
      }
      set
      {
        if( value == this.DataGridContext.CurrentColumn )
          return;

        this.DataGridContext.SetCurrent( this.DataGridContext.InternalCurrentItem, null, null, value, false, true, this.SynchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers.CurrentColumnChanged );
      }
    }

    #endregion

    #region CurrentContext Property

    public DataGridContext CurrentContext
    {
      get
      {
        Debug.Assert( m_currentDataGridContext != null );

        return m_currentDataGridContext;
      }
    }

    internal void SetCurrentDataGridContextHelper( DataGridContext value )
    {
      m_currentDataGridContext = value;

      this.OnPropertyChanged( "CurrentContext" );
      this.OnPropertyChanged( "CurrentColumn" );
      this.OnPropertyChanged( "CurrentItem" );
      this.OnPropertyChanged( "GlobalCurrentItem" );
      this.OnPropertyChanged( "GlobalCurrentColumn" );
    }

    private DataGridContext m_currentDataGridContext; // = null

    #endregion

    #region GlobalCurrentItem Read-Only Property

    public object GlobalCurrentItem
    {
      get
      {
        return this.CurrentContext.CurrentItem;
      }
    }

    #endregion

    #region GlobalCurrentColumn Read-Only Property

    public ColumnBase GlobalCurrentColumn
    {
      get
      {
        return this.CurrentContext.CurrentColumn;
      }
    }

    #endregion

    #region DragDropAdornerDecorator Property

    internal AdornerDecorator DragDropAdornerDecorator
    {
      get
      {
        return m_dragDropAdornerDecorator;
      }
    }

    private AdornerDecorator m_dragDropAdornerDecorator;

    #endregion

    #region AutoCreateColumns Property

    public static readonly DependencyProperty AutoCreateColumnsProperty = DependencyProperty.Register(
      "AutoCreateColumns",
      typeof( bool ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( true ) );

    public bool AutoCreateColumns
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.AutoCreateColumnsProperty );
      }
      set
      {
        this.SetValue( DataGridControl.AutoCreateColumnsProperty, value );
      }
    }

    #endregion

    #region AutoCreateForeignKeyConfigurations Property

    public static readonly DependencyProperty AutoCreateForeignKeyConfigurationsProperty = DependencyProperty.Register(
      "AutoCreateForeignKeyConfigurations",
      typeof( bool ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata(
        ( bool )false,
        new PropertyChangedCallback( DataGridControl.OnAutoCreateForeignKeyConfigurationsChanged ) ) );

    public bool AutoCreateForeignKeyConfigurations
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.AutoCreateForeignKeyConfigurationsProperty );
      }
      set
      {
        this.SetValue( DataGridControl.AutoCreateForeignKeyConfigurationsProperty, value );
      }
    }

    private static void OnAutoCreateForeignKeyConfigurationsChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( ( self == null ) || !self.AutoCreateForeignKeyConfigurations )
        return;

      self.SynchronizeForeignKeyConfigurations();
    }

    #endregion

    #region HideSelection Property

    public static readonly DependencyProperty HideSelectionProperty = DependencyProperty.Register(
      "HideSelection",
      typeof( bool ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( false ) );

    public bool HideSelection
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.HideSelectionProperty );
      }
      set
      {
        this.SetValue( DataGridControl.HideSelectionProperty, value );
      }
    }

    #endregion

    #region ReadOnly Property

    public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.RegisterAttached(
      "ReadOnly",
      typeof( bool ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.Inherits ) );

    public bool ReadOnly
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.ReadOnlyProperty );
      }
      set
      {
        this.SetValue( DataGridControl.ReadOnlyProperty, value );
      }
    }

    #endregion

    #region AutoScrollCurrentItem Property

    public static readonly DependencyProperty AutoScrollCurrentItemProperty = DependencyProperty.Register(
      "AutoScrollCurrentItem",
      typeof( AutoScrollCurrentItemTriggers ),
      typeof( DataGridControl ),
      new PropertyMetadata( AutoScrollCurrentItemTriggers.All ) );

    public AutoScrollCurrentItemTriggers AutoScrollCurrentItem
    {
      get
      {
        return ( AutoScrollCurrentItemTriggers )this.GetValue( DataGridControl.AutoScrollCurrentItemProperty );
      }
      set
      {
        this.SetValue( DataGridControl.AutoScrollCurrentItemProperty, value );
      }
    }

    #endregion

    #region EditTriggers Property

    public static readonly DependencyProperty EditTriggersProperty = DependencyProperty.RegisterAttached(
      "EditTriggers",
      typeof( EditTriggers ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( EditTriggers.BeginEditCommand | EditTriggers.ClickOnCurrentCell | EditTriggers.ActivationGesture, FrameworkPropertyMetadataOptions.Inherits ) );

    public EditTriggers EditTriggers
    {
      get
      {
        return ( EditTriggers )this.GetValue( DataGridControl.EditTriggersProperty );
      }
      set
      {
        this.SetValue( DataGridControl.EditTriggersProperty, value );
      }
    }

    #endregion

    #region ScrollViewer Property

    internal ScrollViewer ScrollViewer
    {
      get
      {
        return m_scrollViewer;
      }
    }

    private ScrollViewer m_scrollViewer;

    #endregion

    #region ItemsHost Property

    internal FrameworkElement ItemsHost
    {
      get
      {
        if( m_itemsHost == null )
        {
          var scrollViewer = this.ScrollViewer;
          if( scrollViewer == null )
            return null;

          //this can be either a ItemsPresenter (which implements IScrollInfo and reforwards it to the ItemsPanel instantiated from the
          //ItemsPanelTemplate or the DataGridItemsHost used for the Container Recycling.
          var scrollViewerContent = default( FrameworkElement );

          var itemsPresenter = scrollViewer.Content as ItemsPresenter;
          if( itemsPresenter != null )
          {
            if( VisualTreeHelper.GetChildrenCount( itemsPresenter ) > 0 )
            {
              scrollViewerContent = VisualTreeHelper.GetChild( itemsPresenter, 0 ) as FrameworkElement;
            }
          }
          else
          {
            scrollViewerContent = scrollViewer.Content as DataGridItemsHost;

            if( scrollViewerContent == null )
            {
              scrollViewerContent = scrollViewer.Content as Panel;
            }
          }

          if( scrollViewerContent == null )
            throw new InvalidOperationException( "An attempt was made to use a ScrollViewer that does not have an ItemsPresenter, Panel, or DataGridItemsHost as its content." );

          m_itemsHost = scrollViewerContent;
        }

        return m_itemsHost;
      }
    }

    #endregion

    #region ItemsPanel override

    [Obsolete( "The ItemsPanel property is obsolete and has been replaced by the DataGridItemsHost (and derived) classes.", false )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public new ItemsPanelTemplate ItemsPanel
    {
      get
      {
        return base.ItemsPanel;
      }
      set
      {
        base.ItemsPanel = value;
      }
    }

    #endregion

    #region FixedHeadersHostPanel Property

    internal Panel FixedHeadersHostPanel
    {
      get
      {
        return m_fixedHeadersHostPanel;
      }
    }

    private void SetFixedHeadersHostPanel( Panel panel )
    {
      if( m_fixedHeadersHostPanel == panel )
        return;

      m_fixedHeadersHostPanel = panel;

      if( m_fixedHeadersHostPanel != null )
      {
        DataGridControl.RefreshFixedHeaderFooter( this, m_fixedHeadersHostPanel, this.GetView().FixedHeaders );
      }
    }

    private Panel m_fixedHeadersHostPanel;

    #endregion

    #region FixedFootersHostPanel Property

    internal Panel FixedFootersHostPanel
    {
      get
      {
        return m_fixedFootersHostPanel;
      }
    }

    private void SetFixedFootersHostPanel( Panel panel )
    {
      if( m_fixedFootersHostPanel == panel )
        return;

      m_fixedFootersHostPanel = panel;

      if( m_fixedFootersHostPanel != null )
      {
        DataGridControl.RefreshFixedHeaderFooter( this, m_fixedFootersHostPanel, this.GetView().FixedFooters );
      }
    }

    private Panel m_fixedFootersHostPanel;

    #endregion

    #region IsFixedHeadersHost Attached Property

    public static readonly DependencyProperty IsFixedHeadersHostProperty = DependencyProperty.RegisterAttached(
      "IsFixedHeadersHost",
      typeof( bool ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( false, new PropertyChangedCallback( DataGridControl.OnIsFixedHeadersHost_Changed ) ) );

    public static bool GetIsFixedHeadersHost( DependencyObject obj )
    {
      return ( bool )obj.GetValue( DataGridControl.IsFixedHeadersHostProperty );
    }

    public static void SetIsFixedHeadersHost( DependencyObject obj, bool value )
    {
      obj.SetValue( DataGridControl.IsFixedHeadersHostProperty, value );
    }

    private static void OnIsFixedHeadersHost_Changed( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      if( !( bool )e.NewValue )
        return;

      var panel = sender as Panel;
      if( panel == null )
        return;

      var dataGridContext = DataGridControl.GetDataGridContext( sender );
      var dataGridControl = ( dataGridContext != null ) ? dataGridContext.DataGridControl : null;

      if( dataGridControl != null )
      {
        dataGridControl.SetFixedHeadersHostPanel( panel );
      }
    }

    #endregion

    #region IsFixedFootersHost Attached Property

    public static readonly DependencyProperty IsFixedFootersHostProperty = DependencyProperty.RegisterAttached(
      "IsFixedFootersHost",
      typeof( bool ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( false, new PropertyChangedCallback( DataGridControl.OnIsFixedFootersHost_Changed ) ) );

    public static bool GetIsFixedFootersHost( DependencyObject obj )
    {
      return ( bool )obj.GetValue( DataGridControl.IsFixedFootersHostProperty );
    }

    public static void SetIsFixedFootersHost( DependencyObject obj, bool value )
    {
      obj.SetValue( DataGridControl.IsFixedFootersHostProperty, value );
    }

    private static void OnIsFixedFootersHost_Changed( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      if( !( bool )e.NewValue )
        return;

      var panel = sender as Panel;
      if( panel == null )
        return;

      var dataGridContext = DataGridControl.GetDataGridContext( sender );
      var dataGridControl = ( dataGridContext != null ) ? dataGridContext.DataGridControl : null;

      if( dataGridControl != null )
      {
        dataGridControl.SetFixedFootersHostPanel( panel );
      }
    }

    #endregion

    #region ItemsPrimaryAxis Property

    public static readonly DependencyProperty ItemsPrimaryAxisProperty = DependencyProperty.Register(
      "ItemsPrimaryAxis",
      typeof( PrimaryAxis ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( PrimaryAxis.Vertical ) );

    public PrimaryAxis ItemsPrimaryAxis
    {
      get
      {
        return ( PrimaryAxis )this.GetValue( DataGridControl.ItemsPrimaryAxisProperty );
      }
      set
      {
        this.SetValue( DataGridControl.ItemsPrimaryAxisProperty, value );
      }
    }

    #endregion

    #region View Property

    public static readonly DependencyProperty ViewProperty = DependencyProperty.Register(
      "View",
      typeof( UIViewBase ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( null, new PropertyChangedCallback( DataGridControl.OnViewChanged ), new CoerceValueCallback( DataGridControl.ViewCoerceValueCallback ) ) );

    [TypeConverter( typeof( Markup.ViewConverter ) )]
    public UIViewBase View
    {
      get
      {
        return ( UIViewBase )this.GetValue( DataGridControl.ViewProperty );
      }
      set
      {
        this.SetValue( DataGridControl.ViewProperty, value );
      }
    }

    internal event EventHandler ViewChanged;
    internal event EventHandler ThemeChanged;

    private static object ViewCoerceValueCallback( DependencyObject sender, object value )
    {
      var view = value as UIViewBase;
      var dataGridControl = sender as DataGridControl;

      if( ( view == null ) && ( dataGridControl != null ) )
        return dataGridControl.GetDefaultView();

      if( view != null )
      {
        if( ( view.Parent != null ) && ( view.Parent != sender ) )
          throw new InvalidOperationException( "An attempt was made to associate a view with more than one grid." );
      }

      return value;
    }

    private static void OnViewChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( self == null )
        return;

      // Cannot stay in editing mode when the view is changed
      self.CancelEdit();

      var oldView = e.OldValue as Views.ViewBase;
      var newView = e.NewValue as Views.ViewBase;

      self.OnViewChanged( oldView, newView );
    }

    private void OnViewChanged( Views.ViewBase oldView, Views.ViewBase newView )
    {
      if( oldView != null )
      {
        oldView.ThemeChanged -= new DependencyPropertyChangedEventHandler( this.View_ThemeChanged );
        oldView.FixedHeaders.CollectionChanged -= new NotifyCollectionChangedEventHandler( this.View_FixedHeadersCollectionChanged );
        oldView.FixedFooters.CollectionChanged -= new NotifyCollectionChangedEventHandler( this.View_FixedFootersCollectionChanged );
      }

      object newDefaultStyleKey = null;

      if( newView != null )
      {
        newView.ThemeChanged += new DependencyPropertyChangedEventHandler( this.View_ThemeChanged );
        newView.FixedHeaders.CollectionChanged += new NotifyCollectionChangedEventHandler( this.View_FixedHeadersCollectionChanged );
        newView.FixedFooters.CollectionChanged += new NotifyCollectionChangedEventHandler( this.View_FixedFootersCollectionChanged );

        newDefaultStyleKey = newView.GetDefaultStyleKey( typeof( DataGridControl ) );
      }

      // Cache if the view requires to preserve container size
      this.ViewPreservesContainerSize = ( newView != null ) ? newView.PreserveContainerSize : true;

      if( !object.Equals( newDefaultStyleKey, this.DefaultStyleKey ) )
      {
        this.ClearValue( FrameworkElement.DefaultStyleKeyProperty );

        if( !object.Equals( newDefaultStyleKey, this.DefaultStyleKey ) )
        {
          this.DefaultStyleKey = newDefaultStyleKey;
        }
      }

      this.InvalidateViewStyle();

      // We cannot be sure that the grid elements default style key are different with this new View/Theme (for instance, if the new View/Theme are of the same type 
      // as the old ones). So, we have to force the new templates. This is mainly to setup the new ViewBindings.
      this.ReapplyTemplate();

      if( this.ViewChanged != null )
      {
        this.ViewChanged( this, EventArgs.Empty );
      }

      // Reset the size states since we do not want to apply old size states to a new view/new container style.
      this.DataGridContext.ClearSizeStates();

      // Reset the flag
      this.ForceGeneratorReset = false;

      // Make sure the current item is into view.
      this.DelayBringIntoViewAndFocusCurrent( DispatcherPriority.Render, AutoScrollCurrentItemSourceTriggers.ViewChanged );
    }

    private UIViewBase m_defaultView;

    #endregion

    #region GroupLevelDescriptions Read-Only Property

    private static readonly DependencyPropertyKey GroupLevelDescriptionsPropertyKey = DependencyProperty.RegisterReadOnly(
      "GroupLevelDescriptions",
      typeof( GroupLevelDescriptionCollection ),
      typeof( DataGridControl ),
      new PropertyMetadata( null ) );

    public static readonly DependencyProperty GroupLevelDescriptionsProperty;

    public GroupLevelDescriptionCollection GroupLevelDescriptions
    {
      get
      {
        return ( GroupLevelDescriptionCollection )this.GetValue( DataGridControl.GroupLevelDescriptionsProperty );
      }
    }

    #endregion

    #region NavigationBehavior Property

    public static readonly DependencyProperty NavigationBehaviorProperty = DependencyProperty.RegisterAttached(
      "NavigationBehavior",
      typeof( NavigationBehavior ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( NavigationBehavior.CellOnly, FrameworkPropertyMetadataOptions.Inherits ) );

    public NavigationBehavior NavigationBehavior
    {
      get
      {
        return ( NavigationBehavior )this.GetValue( DataGridControl.NavigationBehaviorProperty );
      }
      set
      {
        this.SetValue( DataGridControl.NavigationBehaviorProperty, value );
      }
    }

    #endregion

    #region PagingBehavior Property

    public static readonly DependencyProperty PagingBehaviorProperty = DependencyProperty.Register(
      "PagingBehavior",
      typeof( PagingBehavior ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( PagingBehavior.TopToBottom ) );

    public PagingBehavior PagingBehavior
    {
      get
      {
        return ( PagingBehavior )this.GetValue( DataGridControl.PagingBehaviorProperty );
      }
      set
      {
        this.SetValue( DataGridControl.PagingBehaviorProperty, value );
      }
    }

    #endregion

    #region ItemScrollingBehavior Property

    public static readonly DependencyProperty ItemScrollingBehaviorProperty = DependencyProperty.Register(
      "ItemScrollingBehavior",
      typeof( ItemScrollingBehavior ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( ItemScrollingBehavior.Deferred ) );

    public ItemScrollingBehavior ItemScrollingBehavior
    {
      get
      {
        return ( ItemScrollingBehavior )this.GetValue( DataGridControl.ItemScrollingBehaviorProperty );
      }
      set
      {
        this.SetValue( DataGridControl.ItemScrollingBehaviorProperty, value );
      }
    }

    #endregion

    #region CurrentRowInEditionState Read-Only Property

    internal RowState CurrentRowInEditionState
    {
      get
      {
        return m_currentRowInEditionState;
      }
    }

    #endregion

    #region CurrentItemInEdition Read-Only Property

    internal object CurrentItemInEdition
    {
      get
      {
        return m_currentItemInEdition;
      }
    }

    #endregion

    #region FixedItem Attached Property

    private static readonly object NotSet = new object();

    // The ownerType is set to FrameworkElement to make the inheritance works for all grid elements.
    private static readonly DependencyProperty FixedItemProperty = DependencyProperty.RegisterAttached(
      "FixedItem",
      typeof( object ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( DataGridControl.NotSet, FrameworkPropertyMetadataOptions.Inherits ) );

    private static object GetFixedItem( DependencyObject obj )
    {
      return obj.GetValue( DataGridControl.FixedItemProperty );
    }

    private static void SetFixedItem( DependencyObject obj, object value )
    {
      obj.SetValue( DataGridControl.FixedItemProperty, value );
    }

    #endregion

    #region Hidden Properties

    // We want to hide these inherited properties because they have no impact on the DataGridControl behavior.

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public new DataTemplate ItemTemplate
    {
      get
      {
        return base.ItemTemplate;
      }
      set
      {
        base.ItemTemplate = value;
      }
    }

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public new DataTemplateSelector ItemTemplateSelector
    {
      get
      {
        return base.ItemTemplateSelector;
      }
      set
      {
        base.ItemTemplateSelector = value;
      }
    }

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public new ObservableCollection<GroupStyle> GroupStyle
    {
      get
      {
        return base.GroupStyle;
      }
    }

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public new GroupStyleSelector GroupStyleSelector
    {
      get
      {
        return base.GroupStyleSelector;
      }

      set
      {
        base.GroupStyleSelector = value;
      }
    }

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public new bool IsGrouping
    {
      get
      {
        return base.IsGrouping;
      }
    }

    #endregion

    #region ItemContainerGenerator Property

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The ItemContainerGenerator is obsolete and has been replaced by the GetContainerFromItem and GetItemFromContainer methods.", true )]
    public new ItemContainerGenerator ItemContainerGenerator
    {
      get
      {
        return base.ItemContainerGenerator;
      }
    }

    #endregion

    #region CustomItemContainerGenerator Property

    internal CustomItemContainerGenerator CustomItemContainerGenerator
    {
      get
      {
        return m_customItemContainerGenerator;
      }
    }

    #endregion

    #region IsBeingEdited Property

    private static readonly DependencyPropertyKey IsBeingEditedPropertyKey = DependencyProperty.RegisterReadOnly(
      "IsBeingEdited",
      typeof( bool ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( false ) );

    public static readonly DependencyProperty IsBeingEditedProperty = IsBeingEditedPropertyKey.DependencyProperty;

    public bool IsBeingEdited
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.IsBeingEditedProperty );
      }
    }

    private void UpdateIsBeingEdited()
    {
      var isBeingEdited = ( m_currentItemInEdition != null );
      if( isBeingEdited == this.IsBeingEdited )
        return;

      if( isBeingEdited )
      {
        this.SetValue( DataGridControl.IsBeingEditedPropertyKey, isBeingEdited );
      }
      else
      {
        this.ClearValue( DataGridControl.IsBeingEditedPropertyKey );
      }
    }

    #endregion

    #region DataGridContext Read-Only Property

    internal DataGridContext DataGridContext
    {
      get
      {
        return m_localDataGridContext;
      }
    }

    #endregion

    #region GroupConfigurationSelector Property

    public static readonly DependencyProperty GroupConfigurationSelectorProperty = DependencyProperty.Register(
      "GroupConfigurationSelector",
      typeof( GroupConfigurationSelector ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnGroupConfigurationSelectorChanged ) ) );

    public GroupConfigurationSelector GroupConfigurationSelector
    {
      get
      {
        return ( GroupConfigurationSelector )this.GetValue( DataGridControl.GroupConfigurationSelectorProperty );
      }
      set
      {
        this.SetValue( DataGridControl.GroupConfigurationSelectorProperty, value );
      }
    }

    internal event EventHandler GroupConfigurationSelectorChanged;

    private static void OnGroupConfigurationSelectorChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( self == null )
        return;

      var handler = self.GroupConfigurationSelectorChanged;
      if( handler == null )
        return;

      handler.Invoke( self, EventArgs.Empty );
    }

    #endregion

    #region AllowDetailToggle Property

    public static readonly DependencyProperty AllowDetailToggleProperty = DependencyProperty.Register(
      "AllowDetailToggle",
      typeof( bool ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( true, new PropertyChangedCallback( DataGridControl.OnAllowDetailToggleChanged ) ) );

    public bool AllowDetailToggle
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.AllowDetailToggleProperty );
      }
      set
      {
        this.SetValue( DataGridControl.AllowDetailToggleProperty, value );
      }
    }

    internal event EventHandler AllowDetailToggleChanged;

    private static void OnAllowDetailToggleChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( self == null )
        return;

      var handler = self.AllowDetailToggleChanged;
      if( handler == null )
        return;

      handler.Invoke( self, EventArgs.Empty );
    }

    #endregion

    #region DefaultDetailConfiguration Property

    public static readonly DependencyProperty DefaultDetailConfigurationProperty = DependencyProperty.Register(
      "DefaultDetailConfiguration",
      typeof( DefaultDetailConfiguration ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( null ) );

    public DefaultDetailConfiguration DefaultDetailConfiguration
    {
      get
      {
        return ( DefaultDetailConfiguration )this.GetValue( DataGridControl.DefaultDetailConfigurationProperty );
      }
      set
      {
        this.SetValue( DataGridControl.DefaultDetailConfigurationProperty, value );
      }
    }

    #endregion

    #region DefaultGroupConfiguration Property

    public static readonly DependencyProperty DefaultGroupConfigurationProperty = DependencyProperty.Register(
      "DefaultGroupConfiguration",
      typeof( GroupConfiguration ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( null ) );

    public GroupConfiguration DefaultGroupConfiguration
    {
      get
      {
        return ( GroupConfiguration )this.GetValue( DataGridControl.DefaultGroupConfigurationProperty );
      }
      set
      {
        this.SetValue( DataGridControl.DefaultGroupConfigurationProperty, value );
      }
    }

    #endregion

    #region ContainerGroupConfiguration Attached Property

    internal static readonly DependencyProperty ContainerGroupConfigurationProperty = DependencyProperty.RegisterAttached(
      "ContainerGroupConfiguration",
      typeof( GroupConfiguration ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( null ) );

    internal static GroupConfiguration GetContainerGroupConfiguration( DependencyObject obj )
    {
      return ( GroupConfiguration )obj.GetValue( DataGridControl.ContainerGroupConfigurationProperty );
    }

    internal static void SetContainerGroupConfiguration( DependencyObject obj, GroupConfiguration value )
    {
      obj.SetValue( DataGridControl.ContainerGroupConfigurationProperty, value );
    }

    #endregion

    #region ItemsSourceName Property

    public object ItemsSourceName
    {
      get
      {
        if( m_itemsSourceName == null )
          return m_detectedName;

        return m_itemsSourceName;
      }
      set
      {
        if( value == m_itemsSourceName )
          return;

        m_itemsSourceName = value;

        this.OnPropertyChanged( "ItemsSourceName" );
      }
    }

    private object m_itemsSourceName;
    private object m_detectedName;

    #endregion

    #region ItemsSourceNameTemplate Property

    public static readonly DependencyProperty ItemsSourceNameTemplateProperty = DependencyProperty.Register(
      "ItemsSourceNameTemplate",
      typeof( DataTemplate ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( null ) );

    public DataTemplate ItemsSourceNameTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( DataGridControl.ItemsSourceNameTemplateProperty );
      }
      set
      {
        this.SetValue( DataGridControl.ItemsSourceNameTemplateProperty, value );
      }
    }

    #endregion

    #region UpdateSourceTrigger Property

    public static readonly DependencyProperty UpdateSourceTriggerProperty = DependencyProperty.Register(
      "UpdateSourceTrigger",
      typeof( DataGridUpdateSourceTrigger ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( DataGridUpdateSourceTrigger.RowEndingEdit ) );

    public DataGridUpdateSourceTrigger UpdateSourceTrigger
    {
      get
      {
        return ( DataGridUpdateSourceTrigger )this.GetValue( DataGridControl.UpdateSourceTriggerProperty );
      }
      set
      {
        this.SetValue( DataGridControl.UpdateSourceTriggerProperty, value );
      }
    }

    #endregion

    #region ForceGeneratorReset Property

    // This property is used to avoid a reset when Column Virtualization is on and all Row instances are using FixedCellPanel as PART_CellsHost
    internal bool ForceGeneratorReset
    {
      get
      {
        return m_forceGeneratorReset;
      }
      set
      {
        m_forceGeneratorReset = value;
      }
    }

    private bool m_forceGeneratorReset; // = false;

    #endregion

    #region ClipboardExporters Property

    public Dictionary<string, ClipboardExporterBase> ClipboardExporters
    {
      get
      {
        if( m_clipboardExporters == null )
        {
          m_clipboardExporters = new Dictionary<string, ClipboardExporterBase>();

          if( !DesignerProperties.GetIsInDesignMode( this ) )
          {
            // Configure CSV ClipboardExporter
            CsvClipboardExporter csvClipboardExporter = new CsvClipboardExporter();
            csvClipboardExporter.FormatSettings.Separator = ',';
            csvClipboardExporter.FormatSettings.TextQualifier = '"';
            m_clipboardExporters.Add( DataFormats.CommaSeparatedValue, csvClipboardExporter );

            // Configure tab separated value ClipboardExporter
            csvClipboardExporter = new CsvClipboardExporter();
            csvClipboardExporter.FormatSettings.Separator = '\t';
            csvClipboardExporter.FormatSettings.TextQualifier = '"';
            m_clipboardExporters.Add( DataFormats.Text, csvClipboardExporter );

            // Configure HTML exporter
            m_clipboardExporters.Add( DataFormats.Html, new HtmlClipboardExporter() );

            // Configure Unicode ClipboardExporter 
            m_clipboardExporters.Add( DataFormats.UnicodeText, new UnicodeCsvClipboardExporter() );
          }
        }

        return m_clipboardExporters;
      }
    }

    private Dictionary<string, ClipboardExporterBase> m_clipboardExporters; // = null;

    #endregion

    #region IsDeleteCommandEnabled Property

    public static readonly DependencyProperty IsDeleteCommandEnabledProperty = DependencyProperty.Register(
      "IsDeleteCommandEnabled",
      typeof( bool ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( false, new PropertyChangedCallback( DataGridControl.OnIsDeleteCommandEnabledChanged ) ) );

    public bool IsDeleteCommandEnabled
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.IsDeleteCommandEnabledProperty );
      }
      set
      {
        this.SetValue( DataGridControl.IsDeleteCommandEnabledProperty, value );
      }
    }

    private static void OnIsDeleteCommandEnabledChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( self == null )
        return;

      // On keep the command binding active if it is required
      if( ( bool )e.NewValue )
      {
        if( !self.CommandBindings.Contains( self.m_deleteCommandBinding ) )
        {
          self.CommandBindings.Add( self.m_deleteCommandBinding );
        }
      }
      else
      {
        self.CommandBindings.Remove( self.m_deleteCommandBinding );
      }

      CommandManager.InvalidateRequerySuggested();
    }

    #endregion

    #region IsCopyCommandEnabled Property

    public static readonly DependencyProperty IsCopyCommandEnabledProperty = DependencyProperty.Register(
      "IsCopyCommandEnabled",
      typeof( bool ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( true, new PropertyChangedCallback( OnIsCopyCommandEnabledChanged ) ) );

    public bool IsCopyCommandEnabled
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.IsCopyCommandEnabledProperty );
      }
      set
      {
        this.SetValue( DataGridControl.IsCopyCommandEnabledProperty, value );
      }
    }

    private static void OnIsCopyCommandEnabledChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( self == null )
        return;

      // On keep the command binding active if it is required
      if( ( bool )e.NewValue )
      {
        if( !self.CommandBindings.Contains( self.m_copyCommandBinding ) )
        {
          self.CommandBindings.Add( self.m_copyCommandBinding );
        }
      }
      else
      {
        self.CommandBindings.Remove( self.m_copyCommandBinding );
      }

      CommandManager.InvalidateRequerySuggested();
    }

    #endregion

    #region IsRefreshCommandEnabled Property

    public static readonly DependencyProperty IsRefreshCommandEnabledProperty = DependencyProperty.Register(
      "IsRefreshCommandEnabled",
      typeof( bool ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( true, new PropertyChangedCallback( DataGridControl.OnIsRefreshCommandEnabledChanged ) ) );

    public bool IsRefreshCommandEnabled
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.IsRefreshCommandEnabledProperty );
      }
      set
      {
        this.SetValue( DataGridControl.IsRefreshCommandEnabledProperty, value );
      }
    }

    private static void OnIsRefreshCommandEnabledChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( self == null )
        return;

      // On keep the command binding active if it is required
      if( ( bool )e.NewValue )
      {
        if( !self.CommandBindings.Contains( self.m_refreshCommandBinding ) )
        {
          self.CommandBindings.Add( self.m_refreshCommandBinding );
        }
      }
      else
      {
        self.CommandBindings.Remove( self.m_refreshCommandBinding );
      }

      CommandManager.InvalidateRequerySuggested();
    }

    #endregion

    #region MaxSortLevels Property

    public static readonly DependencyProperty MaxSortLevelsProperty = DependencyProperty.Register(
      "MaxSortLevels",
      typeof( int ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( -1, new PropertyChangedCallback( DataGridControl.OnMaxSortLevelsChanged ) ) );

    public int MaxSortLevels
    {
      get
      {
        return ( int )this.GetValue( DataGridControl.MaxSortLevelsProperty );
      }
      set
      {
        this.SetValue( DataGridControl.MaxSortLevelsProperty, value );
      }
    }

    internal event EventHandler MaxSortLevelsChanged;

    private static void OnMaxSortLevelsChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( self == null )
        return;

      var handler = self.MaxSortLevelsChanged;
      if( handler == null )
        return;

      handler.Invoke( self, EventArgs.Empty );
    }

    #endregion

    #region MaxGroupLevels Property

    public static readonly DependencyProperty MaxGroupLevelsProperty = DependencyProperty.Register(
      "MaxGroupLevels",
      typeof( int ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( -1, new PropertyChangedCallback( DataGridControl.OnMaxGroupLevelsChanged ) ) );

    public int MaxGroupLevels
    {
      get
      {
        return ( int )this.GetValue( DataGridControl.MaxGroupLevelsProperty );
      }
      set
      {
        this.SetValue( DataGridControl.MaxGroupLevelsProperty, value );
      }
    }

    internal event EventHandler MaxGroupLevelsChanged;

    private static void OnMaxGroupLevelsChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( self == null )
        return;

      var handler = self.MaxGroupLevelsChanged;
      if( handler == null )
        return;

      handler.Invoke( self, EventArgs.Empty );
    }

    #endregion

    #region SynchronizeSelectionWithCurrent Property

    public static readonly DependencyProperty SynchronizeSelectionWithCurrentProperty = DependencyProperty.Register(
      "SynchronizeSelectionWithCurrent",
      typeof( bool ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( false ) );

    public bool SynchronizeSelectionWithCurrent
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.SynchronizeSelectionWithCurrentProperty );
      }
      set
      {
        this.SetValue( DataGridControl.SynchronizeSelectionWithCurrentProperty, value );
      }
    }

    #endregion

    #region SynchronizeCurrent Property

    public static readonly DependencyProperty SynchronizeCurrentProperty = DependencyProperty.Register(
      "SynchronizeCurrent",
      typeof( bool ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( true ) );

    public bool SynchronizeCurrent
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.SynchronizeCurrentProperty );
      }
      set
      {
        this.SetValue( DataGridControl.SynchronizeCurrentProperty, value );
      }
    }

    #endregion

    #region PreserveExtendedSelection Property

    [Obsolete( "The PreserveExtendedSelection dependency property is obsolete.", false )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static readonly DependencyProperty PreserveExtendedSelectionProperty = DependencyProperty.Register(
      "PreserveExtendedSelection",
      typeof( bool ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( false ) );

    [Obsolete( "The PreserveExtendedSelection property is obsolete.", false )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public bool PreserveExtendedSelection
    {
      get
      {
#pragma warning disable 618
        return ( bool )this.GetValue( DataGridControl.PreserveExtendedSelectionProperty );
#pragma warning restore 618
      }
      set
      {
#pragma warning disable 618
        this.SetValue( DataGridControl.PreserveExtendedSelectionProperty, value );
#pragma warning restore 618
      }
    }

    #endregion

    #region PreserveSelectionWhenEnteringEdit Property

    [Obsolete( "The PreserveSelectionWhenEnteringEdit dependency property is obsolete.", false )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static readonly DependencyProperty PreserveSelectionWhenEnteringEditProperty = DependencyProperty.Register(
      "PreserveSelectionWhenEnteringEdit",
      typeof( bool ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( false ) );

    [Obsolete( "The PreserveSelectionWhenEnteringEdit property is obsolete.", false )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public bool PreserveSelectionWhenEnteringEdit
    {
      get
      {
#pragma warning disable 618
        return ( bool )this.GetValue( DataGridControl.PreserveSelectionWhenEnteringEditProperty );
#pragma warning restore 618
      }
      set
      {
#pragma warning disable 618
        this.SetValue( DataGridControl.PreserveSelectionWhenEnteringEditProperty, value );
#pragma warning restore 618
      }
    }

    #endregion

    #region ItemsSourceChangedDelayed Private Property

    private bool ItemsSourceChangedDelayed
    {
      get
      {
        return m_flags[ ( int )DataGridControlFlags.ItemsSourceChangedDelayed ];
      }
      set
      {
        m_flags[ ( int )DataGridControlFlags.ItemsSourceChangedDelayed ] = value;
      }
    }

    #endregion

    #region SelectedIndexPropertyNeedCoerce Internal Property

    internal bool SelectedIndexPropertyNeedCoerce
    {
      get
      {
        return m_flags[ ( int )DataGridControlFlags.SelectedIndexPropertyNeedCoerce ];
      }
      set
      {
        m_flags[ ( int )DataGridControlFlags.SelectedIndexPropertyNeedCoerce ] = value;
      }
    }

    #endregion

    #region SelectedItemPropertyNeedCoerce Internal Property

    internal bool SelectedItemPropertyNeedCoerce
    {
      get
      {
        return m_flags[ ( int )DataGridControlFlags.SelectedItemPropertyNeedCoerce ];
      }
      set
      {
        m_flags[ ( int )DataGridControlFlags.SelectedItemPropertyNeedCoerce ] = value;
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

    #region IsBoundToDataGridVirtualizingCollectionViewBase Private Property

    private bool IsBoundToDataGridVirtualizingCollectionViewBase
    {
      get
      {
        return m_flags[ ( int )DataGridControlFlags.IsBoundToDataGridVirtualizingCollectionViewBase ];
      }
      set
      {
        m_flags[ ( int )DataGridControlFlags.IsBoundToDataGridVirtualizingCollectionViewBase ] = value;
      }
    }

    #endregion

    #region ViewPreservesContainerSize Private Property

    private bool ViewPreservesContainerSize
    {
      get
      {
        return m_flags[ ( int )DataGridControlFlags.ViewPreservesContainerSize ];
      }
      set
      {
        m_flags[ ( int )DataGridControlFlags.ViewPreservesContainerSize ] = value;
      }
    }

    #endregion

    #region DebugSaveRestore Private Property

    private bool DebugSaveRestore
    {
      get
      {
        return m_flags[ ( int )DataGridControlFlags.DebugSaveRestore ];
      }
      set
      {
        m_flags[ ( int )DataGridControlFlags.DebugSaveRestore ] = value;
      }
    }

    #endregion

    #region SelectionChangerManager Property

    internal SelectionManager SelectionChangerManager
    {
      get
      {
        return m_selectionChangerManager;
      }
    }

    #endregion

    #region MouseDownSelectionRangePoint Property

    internal SelectionRangePoint MouseDownSelectionRangePoint
    {
      get
      {
        return m_mouseDownSelectionRangePoint;
      }
    }

    #endregion

    #region HandlesScrolling Property

    protected override bool HandlesScrolling
    {
      get
      {
        return true;
      }
    }

    #endregion

    #region ConnectionState ReadOnly Dependency Property

    private static readonly DependencyPropertyKey ConnectionStatePropertyKey = DependencyProperty.RegisterReadOnly(
      "ConnectionState",
      typeof( DataGridConnectionState ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( DataGridConnectionState.Idle ) );

    public static readonly DependencyProperty ConnectionStateProperty = DataGridControl.ConnectionStatePropertyKey.DependencyProperty;

    public DataGridConnectionState ConnectionState
    {
      get
      {
        return ( DataGridConnectionState )this.GetValue( DataGridControl.ConnectionStateProperty );
      }
    }

    private void SetConnectionState( DataGridConnectionState value )
    {
      if( value == DataGridConnectionState.Idle )
      {
        this.ClearValue( DataGridControl.ConnectionStatePropertyKey );
      }
      else
      {
        this.SetValue( DataGridControl.ConnectionStatePropertyKey, value );
      }
    }

    #endregion

    #region ConnectionError ReadOnly Dependency Property

    private static readonly DependencyPropertyKey ConnectionErrorPropertyKey = DependencyProperty.RegisterReadOnly(
      "ConnectionError",
      typeof( object ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( null ) );

    public static readonly DependencyProperty ConnectionErrorProperty = DataGridControl.ConnectionErrorPropertyKey.DependencyProperty;

    public object ConnectionError
    {
      get
      {
        return this.GetValue( DataGridControl.ConnectionErrorProperty );
      }
    }

    private void SetConnectionError( object value )
    {
      if( value == null )
      {
        this.ClearValue( DataGridControl.ConnectionErrorPropertyKey );
      }
      else
      {
        this.SetValue( DataGridControl.ConnectionErrorPropertyKey, value );
      }
    }

    #endregion

    #region DeferInitialLayoutPass Property

    public static readonly DependencyProperty DeferInitialLayoutPassProperty = DependencyProperty.Register(
      "DeferInitialLayoutPass",
      typeof( bool ),
      typeof( DataGridControl ),
      new FrameworkPropertyMetadata( ( bool )false ) );

    public bool DeferInitialLayoutPass
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.DeferInitialLayoutPassProperty );
      }
      set
      {
        this.SetValue( DataGridControl.DeferInitialLayoutPassProperty, value );
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

    #region AreDetailsFlatten Internal Property

    internal bool AreDetailsFlatten
    {
      get
      {
        var view = this.GetView();
        if( view == null )
          return false;

        var viewType = view.GetType();
        if( viewType != m_areDetailsFlattenCache.Key )
        {
          var attributes = viewType.GetCustomAttributes( typeof( MasterDetailLayoutAttribute ), true );
          var flatten = ( attributes != null )
                     && ( attributes.Length > 0 )
                     && ( ( ( MasterDetailLayoutAttribute )attributes[ 0 ] ).MasterDetailLayoutMode == MasterDetailLayoutMode.Flatten );

          m_areDetailsFlattenCache = new KeyValuePair<Type, bool>( viewType, flatten );
        }

        return m_areDetailsFlattenCache.Value;
      }
    }

    private KeyValuePair<Type, bool> m_areDetailsFlattenCache;

    #endregion

    #region GridUniqueName Property

    internal string GridUniqueName
    {
      get;
      set;
    }

    #endregion

    #region TemplateApplied Event

    internal event EventHandler TemplateApplied;

    #endregion

    #region DetailsChanged Event

    internal event EventHandler DetailsChanged;

    private void OnDetailsChanged( object sender, EventArgs e )
    {
      var handler = this.DetailsChanged;
      if( handler == null )
        return;

      handler.Invoke( sender, e );
    }

    #endregion

    #region DeletingSelectedItems Event

    public static readonly RoutedEvent DeletingSelectedItemsEvent = EventManager.RegisterRoutedEvent(
      "DeletingSelectedItems",
      RoutingStrategy.Bubble,
      typeof( CancelRoutedEventHandler ),
      typeof( DataGridControl ) );

    public event CancelRoutedEventHandler DeletingSelectedItems
    {
      add
      {
        base.AddHandler( DataGridControl.DeletingSelectedItemsEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridControl.DeletingSelectedItemsEvent, value );
      }
    }

    protected virtual void OnDeletingSelectedItems( CancelRoutedEventArgs e )
    {
      this.RaiseEvent( e );
    }

    #endregion

    #region SelectedItemsDeleted Event

    public static readonly RoutedEvent SelectedItemsDeletedEvent = EventManager.RegisterRoutedEvent(
      "SelectedItemsDeleted",
      RoutingStrategy.Bubble,
      typeof( RoutedEventHandler ),
      typeof( DataGridControl ) );

    public event RoutedEventHandler SelectedItemsDeleted
    {
      add
      {
        base.AddHandler( DataGridControl.SelectedItemsDeletedEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridControl.SelectedItemsDeletedEvent, value );
      }
    }

    protected virtual void OnSelectedItemsDeleted()
    {
      this.RaiseEvent( new RoutedEventArgs( DataGridControl.SelectedItemsDeletedEvent, this ) );
    }

    #endregion

    #region SelectionChanged Event

    public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
      "SelectionChanged",
      RoutingStrategy.Bubble,
      typeof( DataGridSelectionChangedEventHandler ),
      typeof( DataGridControl ) );

    public event DataGridSelectionChangedEventHandler SelectionChanged
    {
      add
      {
        base.AddHandler( DataGridControl.SelectionChangedEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridControl.SelectionChangedEvent, value );
      }
    }

    protected virtual void OnSelectionChanged( DataGridSelectionChangedEventArgs e )
    {
      this.RaiseEvent( e );
    }

    internal void RaiseSelectionChanged( DataGridSelectionChangedEventArgs e )
    {
      this.OnSelectionChanged( e );
    }

    #endregion

    #region SelectionChanging Event

    public static readonly RoutedEvent SelectionChangingEvent = EventManager.RegisterRoutedEvent(
      "SelectionChanging",
      RoutingStrategy.Bubble,
      typeof( DataGridSelectionChangingEventHandler ),
      typeof( DataGridControl ) );

    public event DataGridSelectionChangingEventHandler SelectionChanging
    {
      add
      {
        base.AddHandler( DataGridControl.SelectionChangingEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridControl.SelectionChangingEvent, value );
      }
    }

    protected virtual void OnSelectionChanging( DataGridSelectionChangingEventArgs e )
    {
      this.RaiseEvent( e );
    }

    internal void RaiseSelectionChanging( DataGridSelectionChangingEventArgs e )
    {
      using( m_inhibitPreviewGotKeyboardFocus.Set() )
      {
        this.OnSelectionChanging( e );
      }
    }

    #endregion

    #region DeletingSelectedItemError Event

    public static readonly RoutedEvent DeletingSelectedItemErrorEvent = EventManager.RegisterRoutedEvent(
      "DeletingSelectedItemError",
      RoutingStrategy.Bubble,
      typeof( DeletingSelectedItemErrorRoutedEventHandler ),
      typeof( DataGridControl ) );

    public event DeletingSelectedItemErrorRoutedEventHandler DeletingSelectedItemError
    {
      add
      {
        base.AddHandler( DataGridControl.DeletingSelectedItemErrorEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridControl.DeletingSelectedItemErrorEvent, value );
      }
    }

    protected virtual void OnDeletingSelectedItemError( DeletingSelectedItemErrorRoutedEventArgs e )
    {
      this.RaiseEvent( e );
    }

    #endregion

    #region CurrentChanging Event

    public static readonly RoutedEvent CurrentChangingEvent = EventManager.RegisterRoutedEvent(
      "CurrentChanging",
      RoutingStrategy.Bubble,
      typeof( DataGridCurrentChangingEventHandler ),
      typeof( DataGridControl ) );

    public event DataGridCurrentChangingEventHandler CurrentChanging
    {
      add
      {
        base.AddHandler( DataGridControl.CurrentChangingEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridControl.CurrentChangingEvent, value );
      }
    }

    protected virtual void OnCurrentChanging( DataGridCurrentChangingEventArgs e )
    {
      this.RaiseEvent( e );
    }

    internal void RaiseCurrentChanging( DataGridCurrentChangingEventArgs e )
    {
      this.OnCurrentChanging( e );
    }

    #endregion

    #region CurrentChanged Event

    public static readonly RoutedEvent CurrentChangedEvent = EventManager.RegisterRoutedEvent(
      "CurrentChanged",
      RoutingStrategy.Bubble,
      typeof( DataGridCurrentChangedEventHandler ),
      typeof( DataGridControl ) );

    public event DataGridCurrentChangedEventHandler CurrentChanged
    {
      add
      {
        base.AddHandler( DataGridControl.CurrentChangedEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridControl.CurrentChangedEvent, value );
      }
    }

    protected virtual void OnCurrentChanged( DataGridCurrentChangedEventArgs e )
    {
      this.RaiseEvent( e );
    }

    internal void RaiseCurrentChanged( DataGridCurrentChangedEventArgs e )
    {
      this.OnCurrentChanged( e );
    }

    #endregion

    #region SortDirectionChanging Event

    public static readonly RoutedEvent SortDirectionChangingEvent = EventManager.RegisterRoutedEvent(
      "SortDirectionChanging",
      RoutingStrategy.Bubble,
      typeof( ColumnSortDirectionChangingEventHandler ),
      typeof( DataGridControl ) );

    public event ColumnSortDirectionChangingEventHandler SortDirectionChanging
    {
      add
      {
        base.AddHandler( DataGridControl.SortDirectionChangingEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridControl.SortDirectionChangingEvent, value );
      }
    }

    protected virtual void OnSortDirectionChanging( ColumnSortDirectionChangingEventArgs e )
    {
      this.RaiseEvent( e );
    }

    internal void RaiseSortDirectionChanging( ColumnSortDirectionChangingEventArgs e )
    {
      this.OnSortDirectionChanging( e );
    }

    #endregion

    #region GroupCollapsing Event

    public static readonly RoutedEvent GroupCollapsingEvent = EventManager.RegisterRoutedEvent(
      "GroupCollapsing",
      RoutingStrategy.Bubble,
      typeof( GroupExpansionChangingEventHandler ),
      typeof( DataGridControl ) );

    public event GroupExpansionChangingEventHandler GroupCollapsing
    {
      add
      {
        base.AddHandler( DataGridControl.GroupCollapsingEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridControl.GroupCollapsingEvent, value );
      }
    }

    protected virtual void OnGroupCollapsing( GroupExpansionChangingEventArgs e )
    {
      this.RaiseEvent( e );
    }

    internal void RaiseGroupCollapsing( GroupExpansionChangingEventArgs e )
    {
      this.OnGroupCollapsing( e );
    }

    #endregion

    #region GroupCollapsed Event

    public static readonly RoutedEvent GroupCollapsedEvent = EventManager.RegisterRoutedEvent(
      "GroupCollapsed",
      RoutingStrategy.Bubble,
      typeof( GroupExpansionChangedEventHandler ),
      typeof( DataGridControl ) );

    public event GroupExpansionChangedEventHandler GroupCollapsed
    {
      add
      {
        base.AddHandler( DataGridControl.GroupCollapsedEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridControl.GroupCollapsedEvent, value );
      }
    }

    protected virtual void OnGroupCollapsed( GroupExpansionChangedEventArgs e )
    {
      this.RaiseEvent( e );
    }

    internal void RaiseGroupCollapsed( GroupExpansionChangedEventArgs e )
    {
      this.OnGroupCollapsed( e );
    }

    #endregion

    #region GroupExpanding Event

    public static readonly RoutedEvent GroupExpandingEvent = EventManager.RegisterRoutedEvent(
      "GroupExpanding",
      RoutingStrategy.Bubble,
      typeof( GroupExpansionChangingEventHandler ),
      typeof( DataGridControl ) );

    public event GroupExpansionChangingEventHandler GroupExpanding
    {
      add
      {
        base.AddHandler( DataGridControl.GroupExpandingEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridControl.GroupExpandingEvent, value );
      }
    }

    protected virtual void OnGroupExpanding( GroupExpansionChangingEventArgs e )
    {
      this.RaiseEvent( e );
    }

    internal void RaiseGroupExpanding( GroupExpansionChangingEventArgs e )
    {
      this.OnGroupExpanding( e );
    }

    #endregion

    #region GroupExpanded Event

    public static readonly RoutedEvent GroupExpandedEvent = EventManager.RegisterRoutedEvent(
      "GroupExpanded",
      RoutingStrategy.Bubble,
      typeof( GroupExpansionChangedEventHandler ),
      typeof( DataGridControl ) );

    public event GroupExpansionChangedEventHandler GroupExpanded
    {
      add
      {
        base.AddHandler( DataGridControl.GroupExpandedEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridControl.GroupExpandedEvent, value );
      }
    }

    protected virtual void OnGroupExpanded( GroupExpansionChangedEventArgs e )
    {
      this.RaiseEvent( e );
    }

    internal void RaiseGroupExpanded( GroupExpansionChangedEventArgs e )
    {
      this.OnGroupExpanded( e );
    }

    #endregion

    #region Event Handlers

    private static void OnFlowDirectionPropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( self == null )
        return;

      self.CustomItemContainerGenerator.ResetGeneratorContent();
    }

    private void OnDataGridCollectionView_PropertyChanged( DataGridCollectionViewBase sender, PropertyChangedEventArgs e )
    {
      Debug.Assert( sender != null );
      Debug.Assert( e != null );

      var propertyName = e.PropertyName;

      if( string.IsNullOrEmpty( propertyName ) || ( propertyName == DataGridCollectionViewBase.RootGroupPropertyName ) )
      {
        this.SetValue( DataGridControl.StatContextPropertyKey, sender.RootGroup );
      }
    }

    private void OnDataGridDetailDescriptionsChanged( DataGridDetailDescriptionCollection sender, NotifyCollectionChangedEventArgs e )
    {
      if( sender == null )
        return;

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Reset:
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Remove:
        case NotifyCollectionChangedAction.Add:
         break;

        case NotifyCollectionChangedAction.Move:
          //Don't care about the move. 
          break;

        default:
          break;
      }
    }

    private static void OnGroupStyleSelectorChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      throw new NotSupportedException( "GroupStyles are not supported by the DataGridControl." );
    }

    private static void GroupStyle_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( e.Action == NotifyCollectionChangedAction.Add )
      {
        throw new NotSupportedException( "GroupStyles are not supported by the DataGridControl." );
      }
    }

    #endregion

    #region Drag Management

    public static readonly DependencyProperty AllowDragProperty = DependencyProperty.Register(
      "AllowDrag",
      typeof( bool ),
      typeof( DataGridControl ),
      new UIPropertyMetadata( false, new PropertyChangedCallback( DataGridControl.OnAllowDragPropertyChanged ) ) );

    public bool AllowDrag
    {
      get
      {
        return ( bool )GetValue( DataGridControl.AllowDragProperty );
      }
      set
      {
        this.SetValue( DataGridControl.AllowDragProperty, value );
      }
    }

    private static void OnAllowDragPropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as DataGridControl;
      if( ( self == null ) || ( ( bool )e.NewValue ) )
        return;
    }

    internal bool UseDragBehavior
    {
      get
      {
        return false;
      }
    }

    internal void InitializeDragPostion( MouseEventArgs e )
    {
      if( !this.UseDragBehavior )
        return;

      m_initialDragPosition = e.GetPosition( this );
    }

    internal void DoDrag( MouseEventArgs e )
    {
      return;
    }

    internal void ResetDragDataObject()
    {
      m_dragDataObject = null;
      m_initialDragPosition = null;
    }

    internal IDisposable InhibitDrag()
    {
      return m_inhibitDrag.Set();
    }

    private void PrepareDragDataObject( MouseEventArgs e )
    {
      if( !this.UseDragBehavior )
        return;

      try
      {
        new UIPermission( UIPermissionClipboard.AllClipboard ).Demand();

        m_dragDataObject = ClipboardExporterBase.CreateDataObject( this );
      }
      catch( SecurityException )
      {
        throw;
      }
    }

    private IDataObject m_dragDataObject; // = null;
    private Nullable<Point> m_initialDragPosition; // = null;
    private readonly AutoResetFlag m_inhibitDrag = AutoResetFlagFactory.Create( false );

    #endregion

    #region VIEWBASE METHODS

    private void View_FixedHeadersCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      ObservableCollection<DataTemplate> collection = ( ObservableCollection<DataTemplate> )sender;
      Panel panel = this.FixedHeadersHostPanel;

      if( panel != null )
      {
        DataGridControl.RefreshFixedHeaderFooter( this, panel, collection );
      }
    }
    private void View_FixedFootersCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      ObservableCollection<DataTemplate> collection = ( ObservableCollection<DataTemplate> )sender;
      Panel panel = this.FixedFootersHostPanel;

      if( panel != null )
      {
        DataGridControl.RefreshFixedHeaderFooter( this, panel, collection );
      }
    }

    private void View_ThemeChanged( object sender, DependencyPropertyChangedEventArgs e )
    {

      // We need to reevaluate the CurrentContext when it is a detail context since the details are cleared and recreated when the theme changes and no update is
      // made to the current item itself. We dispatch it with a high priority in order to be sure it is executed before a potential BringIntoView of the current item.
      if( this.CurrentContext != this.DataGridContext )
      {
        this.Dispatcher.BeginInvoke( new Action( delegate ()
        {
          this.UpdateCurrentContextOnThemeChanged( this.CurrentContext );
        } ), DispatcherPriority.Normal );
      }

      var view = this.GetView();

      this.SaveDataGridContextState( this.DataGridContext, true, int.MaxValue );

      this.ClearFixedHostPanels();

      if( this.ThemeChanged != null )
      {
        this.ThemeChanged( this, EventArgs.Empty );
      }

      // Reset the size states since we do not want to apply old size states to a new view/new container style.
      this.DataGridContext.ClearSizeStates();

      // Reset the flag
      this.ForceGeneratorReset = false;

      var oldTemplate = this.Template;

      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( DataGridControl ) );

      var newTemplate = this.Template;

      this.ClearValue( DataGridControl.ParentDataGridControlPropertyKey );
      this.SetValue( DataGridControl.ParentDataGridControlPropertyKey, this );

      // When a null Theme is set or the new Theme is the already the applied System Theme, we detect if the
      // template changed after setting the DefaultStyleKey to refresh the FixedHeaders and FixedFooters
      if( oldTemplate == newTemplate )
      {
        this.ResetFixedRegions();
      }

      // Make sure the current item is into view.
      this.DelayBringIntoViewAndFocusCurrent( DispatcherPriority.Render, AutoScrollCurrentItemSourceTriggers.ThemeChanged );
    }

    /// <summary>
    /// Calling this method will enable the grid's current view to be styled.
    /// If any implicit Style exits, it will be immediately applied.
    /// </summary>
    private void InvalidateViewStyle()
    {
      this.AddLogicalChild( this.View );
    }

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The ViewBindingContext property is obsolete and has been replaced by the DataGridControl.DataGridContext attached property.", false )]
    public object ViewBindingContext
    {
      get
      {
        return this.GetView();
      }
    }

    internal Views.ViewBase GetView()
    {
      return this.View;
    }

    private Views.ViewBase GetDefaultView()
    {
      if( m_defaultView == null )
      {
        m_defaultView = new TableflowView();
      }

      return m_defaultView;
    }

    #endregion

    #region FOCUS

    internal bool IsSetFocusInhibited
    {
      get
      {
        return m_inhibitSetFocus.IsSet;
      }
    }

    internal bool SettingFocusOnCell
    {
      get
      {
        return m_flags[ ( int )DataGridControlFlags.SettingFocusOnCell ];
      }
      private set
      {
        m_flags[ ( int )DataGridControlFlags.SettingFocusOnCell ] = value;
      }
    }

    internal static bool SetFocusUIElementHelper( UIElement toFocus )
    {
      if( toFocus == null )
        return false;

      try
      {
        if( toFocus.Focus() )
          return true;
      }
      catch
      {
      }

      if( toFocus.IsKeyboardFocusWithin )
        return true;

      UIElement childToFocus = DataGridControl.FindFirstFocusableChild( toFocus );

      if( childToFocus == null )
        return false;

      try
      {
        return childToFocus.Focus();
      }
      catch
      {
        return false;
      }
    }

    internal static UIElement FindFirstFocusableChild( UIElement reference )
    {
      UIElement retval = null;

      int childCount = VisualTreeHelper.GetChildrenCount( reference );

      for( int i = 0; i < childCount; i++ )
      {
        UIElement element = VisualTreeHelper.GetChild( reference, i ) as UIElement;

        if( ( element != null ) && ( element.Visibility == Visibility.Visible ) )
        {
          if( ( element.Focusable == true ) && ( KeyboardNavigation.GetIsTabStop( element ) == true ) )
          {
            retval = element;
          }

          if( retval == null )
          {
            retval = FindFirstFocusableChild( element );
          }
        }

        if( retval != null )
        {
          break;
        }
      }

      return retval;
    }

    internal void QueueSetFocusHelper( bool forceFocus )
    {
      if( m_queueSetFocus != null )
        return;

      m_queueSetFocus = this.Dispatcher.BeginInvoke( DispatcherPriority.Loaded,
        new DelayedSetFocusHelperHandler( this.DelayedSetFocusHelper ), forceFocus, new object[] { true } );
    }

    internal bool SetFocusHelper( UIElement itemContainer, ColumnBase column, bool forceFocus, bool preserveEditorFocus )
    {
      return this.SetFocusHelper( itemContainer, column, forceFocus, preserveEditorFocus, false );
    }

    internal bool SetFocusHelper( UIElement itemContainer, ColumnBase column, bool forceFocus, bool preserveEditorFocus, bool preventMakeVisibleIfCellFocused )
    {
      if( ( itemContainer == null ) || m_inhibitSetFocus.IsSet )
        return false;

      //There is an identified weakness with the IsKeyboardFocusWithin property where it cannot tell if the focus is within a Popup which is within the element
      //This has been identified, and only the places where it caused problems were fixed... This comment is only here to remind developpers of the flaw
      if( ( !this.IsKeyboardFocusWithin ) && ( !this.IsKeyboardFocused ) && ( !forceFocus ) )
        return false;

#if DEBUG
      DataGridContext itemContainerDataGridContext = DataGridControl.GetDataGridContext( itemContainer );
      Debug.Assert( itemContainerDataGridContext != null );
      Debug.Assert( ( column == null ) || ( itemContainerDataGridContext.Columns.Contains( column ) ) );
#endif

      var toFocus = default( UIElement );

      var row = Row.FromContainer( itemContainer );
      var preventMakeVisibleDisposable = default( IDisposable );

      try
      {
        if( row != null )
        {
          //ColumnManagerRow and derived classes should not be focused
          if( row.IsUnfocusable )
            return false;

          var cell = default( Cell );
          if( column != null )
          {
            cell = row.Cells[ column ];

            // The cell must be in VisualTree to be able to get the focus, else, .Focus() will always return false
            if( cell != null )
            {
              cell.EnsureInVisualTree();

              if( preventMakeVisibleIfCellFocused )
              {
                preventMakeVisibleDisposable = cell.InhibitMakeVisible();
              }
            }
          }

          if( ( row.IsBeingEdited ) && ( cell != null ) )
          {
            if( ( cell.IsBeingEdited ) && ( preserveEditorFocus ) )
            {
              //Obtain the CellEditor's Focus Scope
              toFocus = Cell.GetCellFocusScope( cell );

              //If there was none defined
              if( toFocus == null )
              {
                //obtain the first focusable child into the template
                toFocus = DataGridControl.FindFirstFocusableChild( cell );
              }
            }

            if( toFocus == null )
            {
              toFocus = cell;
            }

            var currentFocusIsInsideCell = ( cell != Keyboard.FocusedElement ) && ( TreeHelper.IsDescendantOf( Keyboard.FocusedElement as DependencyObject, cell ) );

            // If the focus is already within the Cell to focus, there is noting to do.  The item to focus should be the Keyboard focused element - hence, already focused.
            // Verify cell.IsBeingEdited to prevent the focus to stay on the editor when in fact the editing process is being canceled (pressing ESC).
            if( ( currentFocusIsInsideCell ) && ( preserveEditorFocus ) && ( cell.IsBeingEdited ) )
              return true;
          }
          else if( ( row.NavigationBehavior == NavigationBehavior.RowOnly ) || ( cell == null ) )
          {
            toFocus = row.RowFocusRoot;

            if( toFocus == null )
            {
              toFocus = row;
            }

            if( ( row.NavigationBehavior == NavigationBehavior.RowOrCell ) && ( row.IsSelected ) )
            {
              toFocus.Focusable = true;
            }
          }
          else
          {
            toFocus = cell;

            var currentFocusIsInsideCell = ( cell != Keyboard.FocusedElement ) && ( TreeHelper.IsDescendantOf( Keyboard.FocusedElement as DependencyObject, cell ) );

            //If the focus is already within the Cell to focus, then don't touch a thing.  It means the item to focus should be the Keyboard focused element - Already focused
            if( ( currentFocusIsInsideCell ) && ( preserveEditorFocus ) )
              return true;
          }
        }
        else
        {
          toFocus = itemContainer;

          if( TreeHelper.IsDescendantOf( Keyboard.FocusedElement as DependencyObject, toFocus ) )
            return true;
        }

        // If setting the focus on a row fails, we must try to focus the item container in case the target row is not the topmost container,
        // i.e. a StatRow that is contained in a a GroupHeaderControl.
        if( ( toFocus != itemContainer ) && DataGridControl.SetFocusUIElementHelper( toFocus ) )
          return true;

        return DataGridControl.SetFocusUIElementHelper( itemContainer );
      }
      finally
      {
        if( preventMakeVisibleDisposable != null )
        {
          preventMakeVisibleDisposable.Dispose();
          preventMakeVisibleDisposable = null;
        }
      }
    }

    private void DelayedSetFocusHelper( bool forceFocus, bool preserveEditorFocus )
    {
      DataGridContext currentContext = this.CurrentContext;

      if( currentContext == null )
        return;

      this.SettingFocusOnCell = ( currentContext.CurrentCell != null );

      try
      {
        UIElement element = currentContext.GetContainerFromItem( currentContext.InternalCurrentItem ) as UIElement;

        using( m_selectionChangerManager.PushUpdateSelectionSource( SelectionManager.UpdateSelectionSource.None ) )
        {
          this.SetFocusHelper( element, currentContext.CurrentColumn, forceFocus, preserveEditorFocus );
        }
      }
      finally
      {
        this.SettingFocusOnCell = false;
        m_queueSetFocus = null;
      }
    }

    #endregion

    #region BRING INTO VIEW

    internal bool IsBringIntoViewDelayed
    {
      get
      {
        var itemsHost = this.ItemsHost as DataGridItemsHost;

        return ( itemsHost != null )
            && ( itemsHost.DelayBringIntoView );
      }
    }

    internal IDisposable SetQueueBringIntoViewRestrictions( AutoScrollCurrentItemSourceTriggers trigger )
    {
      return m_inhibitAutoScroll.SetRestriction( trigger );
    }

    internal IDisposable InhibitQueueBringIntoView()
    {
      return m_inhibitBringIntoView.Set();
    }

    internal IDisposable InhibitSetFocus()
    {
      return m_inhibitSetFocus.Set();
    }

    internal void DelayBringIntoViewAndFocusCurrent( AutoScrollCurrentItemSourceTriggers trigger )
    {
      this.DelayBringIntoViewAndFocusCurrent( DispatcherPriority.DataBind, trigger );
    }

    private void DelayBringIntoViewAndFocusCurrent( DispatcherPriority priority, AutoScrollCurrentItemSourceTriggers trigger )
    {
      if( ( m_queueBringIntoView != null ) || m_inhibitBringIntoView.IsSet || m_inhibitSetFocus.IsSet || !this.IsBringIntoViewAllowed( trigger ) )
        return;

      m_queueBringIntoView = this.QueueBringIntoView( priority );

      //if the operation is not inhibited
      if( m_queueBringIntoView != null )
      {
        //register to the DispatcherOperation completed event
        m_queueBringIntoView.Completed += new EventHandler( BringIntoView_Completed );
      }
    }

    private DispatcherOperation QueueBringIntoView( DispatcherPriority priority )
    {
      Debug.Assert( !m_inhibitBringIntoView.IsSet );

      return this.Dispatcher.BeginInvoke( priority, new GenericHandler( this.QueueBringIntoViewHelper ) );
    }

    private bool QueueBringIntoViewHelper()
    {
      var dataGridContext = this.CurrentContext;
      if( dataGridContext == null )
        return false;

      var currentItem = dataGridContext.InternalCurrentItem;
      if( currentItem == null )
        return false;

      return this.BringItemIntoViewHelper( dataGridContext, currentItem );
    }

    public bool BringItemIntoView( object item )
    {
      return this.BringItemIntoViewHelper( this.DataGridContext, item );
    }

    internal bool BringItemIntoViewHelper( DataGridContext dataGridContext, object item )
    {
      //this is a protection in case the Template is incomplete or not realized yet.
      var itemsHost = this.ItemsHost;
      if( itemsHost == null )
        return false;

      // It is possible that a BringIntoView was queued before an operation that will
      // detach the ItemsHost of the DataGridControl and we want to avoid to call 
      // ICustomVirtualizingPanel.BringIntoView in this situation.
      var itemsHostDataGridContext = DataGridControl.GetDataGridContext( itemsHost );
      if( itemsHostDataGridContext == null )
        return false;

      // We don't want to call BringIntoView directly on the container if a layout pass
      // is scheduled.
      if( !this.IsBringIntoViewDelayed )
      {
        // If a container exists, call bring into view on it!
        var container = dataGridContext.GetContainerFromItem( item ) as FrameworkElement;
        if( container != null )
        {
          container.BringIntoView();

          //flag the function as being successful
          return true;
        }
      }

      // The container does not exist, it is not yet realized or the action is delayed.

      // If we are not virtualizing any items, return.
      // The call to SetFocusHelper when the BringIntoView completes will call a FrameworkElement BringIntoView
      // which will cause the ScrollViewer to bring the item into view approprietly.
      if( ( this.ScrollViewer != null ) && ( !this.ScrollViewer.CanContentScroll ) )
        return false;

      var customPanel = itemsHost as ICustomVirtualizingPanel;
      if( customPanel == null )
        return false;


      int index = this.GetGlobalGeneratorIndexFromItem( null, item );

      if( index != -1 )
      {
        //call the special function to bring an index into view.
        customPanel.BringIntoView( index );
        return true;
      }

      return false;
    }

    internal bool IsBringIntoViewAllowed( AutoScrollCurrentItemSourceTriggers trigger )
    {
      if( trigger == AutoScrollCurrentItemSourceTriggers.None )
        return false;

      var source = ( ~m_inhibitAutoScroll.Restrictions & trigger );
      if( source == AutoScrollCurrentItemSourceTriggers.None )
        return false;

      var options = this.AutoScrollCurrentItem;

      switch( source )
      {
        case AutoScrollCurrentItemSourceTriggers.Editing:
        case AutoScrollCurrentItemSourceTriggers.Navigation:
          return true;

        case AutoScrollCurrentItemSourceTriggers.CurrentItemChanged:
        case AutoScrollCurrentItemSourceTriggers.CurrentColumnChanged:
        case AutoScrollCurrentItemSourceTriggers.CollectionViewCurrentItemChanged:
        case AutoScrollCurrentItemSourceTriggers.SelectionChanged:
        case AutoScrollCurrentItemSourceTriggers.ColumnsCollectionChanged:
          return DataGridControl.IsSet( options, AutoScrollCurrentItemTriggers.CurrentChanged );

        case AutoScrollCurrentItemSourceTriggers.SortChanged:
          return DataGridControl.IsSet( options, AutoScrollCurrentItemTriggers.SortChanged );

        case AutoScrollCurrentItemSourceTriggers.GroupChanged:
          return DataGridControl.IsSet( options, AutoScrollCurrentItemTriggers.GroupChanged );

        case AutoScrollCurrentItemSourceTriggers.FocusChanged:
          return DataGridControl.IsSet( options, AutoScrollCurrentItemTriggers.FocusChanged );

        case AutoScrollCurrentItemSourceTriggers.ItemsSourceChanged:
          return DataGridControl.IsSet( options, AutoScrollCurrentItemTriggers.ItemsSourceChanged );

        case AutoScrollCurrentItemSourceTriggers.ViewChanged:
          return DataGridControl.IsSet( options, AutoScrollCurrentItemTriggers.ViewChanged );

        case AutoScrollCurrentItemSourceTriggers.ThemeChanged:
          return DataGridControl.IsSet( options, AutoScrollCurrentItemTriggers.ThemeChanged );
      }

      return false;
    }

    private void BringIntoView_Completed( object sender, EventArgs e )
    {
      m_queueBringIntoView.Completed -= new EventHandler( BringIntoView_Completed );
      bool successful = ( bool )m_queueBringIntoView.Result;
      m_queueBringIntoView = null;

      if( successful )
      {
        this.QueueSetFocusHelper( false );
      }
      else if( this.IsKeyboardFocusWithin )
      {
        //The restore current and the bringintoview failed, give the focus back to the DataGridControl (safe handling of focus).
        this.ForceFocus( this.ScrollViewer );
      }
    }

    private static bool IsSet( AutoScrollCurrentItemTriggers source, AutoScrollCurrentItemTriggers comparand )
    {
      return ( ( source & comparand ) == comparand );
    }

    #endregion

    #region COMMANDS

    private void OnExpandGroupCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = false;

      var group = e.Parameter as CollectionViewGroup;
      if( group == null )
        return;

      if( this.DataGridContext.IsGroupExpanded( group, false ) == false )
      {
        e.CanExecute = true;
      }
    }

    protected virtual void OnExpandGroupExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      var group = e.Parameter as CollectionViewGroup;
      if( group == null )
        return;

      this.DataGridContext.ExpandGroup( group );
    }

    private void OnCollapseGroupCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = false;

      var group = e.Parameter as CollectionViewGroup;
      if( group == null )
        return;

      if( this.DataGridContext.IsGroupExpanded( group, false ) == true )
      {
        e.CanExecute = true;
      }
    }

    protected virtual void OnCollapseGroupExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      var group = e.Parameter as CollectionViewGroup;
      if( group == null )
        return;

      this.DataGridContext.CollapseGroup( group );
    }

    private void OnToggleGroupCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = false;

      var group = e.Parameter as CollectionViewGroup;
      if( group == null )
        return;

      e.CanExecute = this.DataGridContext.IsGroupExpanded( group, false ).HasValue;
    }

    protected virtual void OnToggleGroupExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      var group = e.Parameter as CollectionViewGroup;
      if( group == null )
        return;

      this.DataGridContext.ToggleGroupExpansion( group );
    }

    private void OnSelectAllCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = ( this.SelectionModeToUse != SelectionMode.Single );
    }

    private void OnSelectAllExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      if( this.SelectionModeToUse == SelectionMode.Single )
        return;

      this.UpdateLayout();

      SelectAllVisitor selectAllVisitor = new SelectAllVisitor();
      DataGridContext originContext = e.Parameter as DataGridContext;

      if( originContext == null )
      {
        originContext = this.DataGridContext;
      }

      this.SelectionChangerManager.Begin();

      try
      {
        bool visitWasStopped;

        ( ( IDataGridContextVisitable )originContext ).AcceptVisitor(
          0, int.MaxValue, selectAllVisitor, DataGridContextVisitorType.DataGridContext, out visitWasStopped );
      }
      finally
      {
        this.SelectionChangerManager.End( true, false );
      }
    }

    private bool CopyCanExecute()
    {
      try
      {
        new UIPermission( UIPermissionClipboard.AllClipboard ).Demand();
        return ( this.ClipboardExporters.Count > 0 ) && ( this.SelectedContexts.Count > 0 );
      }
      catch( SecurityException )
      {
        return false;
      }
    }

    private void OnCopyCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = this.CopyCanExecute();
    }

    protected virtual void OnCopyExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      if( !this.CopyCanExecute() )
        return;

      try
      {
        IDataObject copyDataObject = ClipboardExporterBase.CreateDataObject( this );

        if( copyDataObject != null )
          Clipboard.SetDataObject( copyDataObject, true );
      }
      catch( SecurityException )
      {
      }
    }

    private void OnDeleteCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      foreach( DataGridContext dataGridContext in this.SelectedContexts )
      {
        if( ( !dataGridContext.IsDeleteCommandEnabled ) || ( dataGridContext.SelectedItems.Count == 0 ) )
          continue;

        DataGridCollectionViewBase dataGridCollectionViewBase = dataGridContext.ItemsSourceCollection as DataGridCollectionViewBase;

        // If we already have a DataGridCollectionView, no need to look for an IList source.
        IList list = ( dataGridCollectionViewBase == null ) ? ItemsSourceHelper.TryGetIList( dataGridContext.ItemsSourceCollection ) : null;

        if( ( dataGridCollectionViewBase != null ) || ( ( list != null ) && DataGridControl.IsRemoveAllowedForDeleteCommand( list ) ) )
        {
          // Ensure that at least on item can be deleted.
          foreach( object item in dataGridContext.SelectedItems )
          {
            if( dataGridCollectionViewBase != null )
            {
              if( dataGridCollectionViewBase.Contains( item ) )
              {
                e.CanExecute = true;
                return;
              }
            }
            else if( list != null )
            {
              if( list.Contains( item ) )
              {
                e.CanExecute = true;
                return;
              }
            }
          }
        }
      }

      e.CanExecute = false;
    }

    protected virtual void OnDeleteExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      CancelRoutedEventArgs args = new CancelRoutedEventArgs( DataGridControl.DeletingSelectedItemsEvent, this );
      this.OnDeletingSelectedItems( args );

      if( args.Cancel )
        return;

      // Copy the selected item in a temp hashtable.
      List<KeyValuePair<DataGridContext, object[]>> selectedItemList = new List<KeyValuePair<DataGridContext, object[]>>();

      for( int i = m_selectedContexts.Count - 1; i >= 0; i-- )
      {
        DataGridContext selectedContext = m_selectedContexts[ i ];
        IList<object> sourceSelectedItems = selectedContext.SelectedItems;
        object[] clonedSelectedItems = new object[ sourceSelectedItems.Count ];
        sourceSelectedItems.CopyTo( clonedSelectedItems, 0 );
        selectedItemList.Add( new KeyValuePair<DataGridContext, object[]>( selectedContext, clonedSelectedItems ) );
      }

      bool raiseItemDeleted = false;

      // Delete the selected item
      foreach( KeyValuePair<DataGridContext, object[]> keyValuePair in selectedItemList )
      {
        DataGridContext dataGridContext = keyValuePair.Key;
        object[] selectedItems = keyValuePair.Value;

        if( ( !dataGridContext.IsDeleteCommandEnabled ) || ( selectedItems.Length == 0 ) )
          continue;

        DataGridCollectionViewBase dataGridCollectionViewBase = dataGridContext.ItemsSourceCollection as DataGridCollectionViewBase;

        // If we already have a DataGridCollectionView, no need to look for an IList source.
        IList list = ( dataGridCollectionViewBase == null ) ? ItemsSourceHelper.TryGetIList( dataGridContext.ItemsSourceCollection ) : null;

        if( ( dataGridCollectionViewBase != null ) || ( ( list != null ) && DataGridControl.IsRemoveAllowedForDeleteCommand( list ) ) )
        {
          foreach( object item in selectedItems )
          {
            bool retry = false;

            do
            {
              try
              {
                int itemIndex;

                if( dataGridCollectionViewBase != null )
                {
                  itemIndex = dataGridCollectionViewBase.IndexOf( item );

                  if( itemIndex != -1 )
                  {
                    dataGridCollectionViewBase.RemoveAt( itemIndex );
                  }
                }
                else if( list != null )
                {
                  itemIndex = list.IndexOf( item );

                  if( itemIndex != -1 )
                  {
                    list.RemoveAt( itemIndex );
                  }
                }

                retry = false;
                raiseItemDeleted = true;
              }
              catch( Exception ex )
              {
                DeletingSelectedItemErrorRoutedEventArgs deletingItemErrorArgs =
                    new DeletingSelectedItemErrorRoutedEventArgs( item, ex, DataGridControl.DeletingSelectedItemErrorEvent, this );

                this.OnDeletingSelectedItemError( deletingItemErrorArgs );

                switch( deletingItemErrorArgs.Action )
                {
                  case DeletingSelectedItemErrorAction.Abort:
                    return;

                  default:
                    retry = ( deletingItemErrorArgs.Action == DeletingSelectedItemErrorAction.Retry );
                    break;
                }
              }
            }
            while( retry );
          }
        }
      }

      if( raiseItemDeleted )
      {
        this.OnSelectedItemsDeleted();
      }
    }

    private void OnRefreshCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = ( this.IsRefreshCommandEnabled ) && ( this.Items != null ) && ( this.Items.SourceCollection is ICollectionView );
    }

    protected virtual void OnRefreshExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      Debug.Assert( this.IsRefreshCommandEnabled );

      if( this.Items != null )
      {
        ICollectionView collectionView = this.Items.SourceCollection as ICollectionView;

        if( collectionView != null )
        {
          collectionView.Refresh();
        }
      }
    }

    #endregion

    #region SOURCE HANDLING

    private void ProcessDelayedItemsSourceChanged()
    {
      var dataGridContext = this.DataGridContext;

      if( this.ShouldSynchronizeCurrentItem )
      {
        var currentItem = this.Items.CurrentItem;
        var currentItemIndex = this.Items.CurrentPosition;

        if( ( this.Items.IsCurrentAfterLast ) || ( this.Items.IsCurrentBeforeFirst ) )
        {
          currentItemIndex = -1;
        }

        dataGridContext.SetCurrent( currentItem, null, currentItemIndex, null, false, false, this.SynchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers.ItemsSourceChanged );

        if( !this.SynchronizeSelectionWithCurrent )
        {
          m_selectionChangerManager.Begin();

          try
          {
            if( ( this.SelectionUnit == SelectionUnit.Row ) && ( !this.Items.IsCurrentAfterLast ) && ( !this.Items.IsCurrentBeforeFirst ) && ( currentItemIndex != -1 ) )
            {
              m_selectionChangerManager.SelectJustThisItem( dataGridContext, currentItemIndex, currentItem );
            }
            else
            {
              m_selectionChangerManager.UnselectAll();
            }
          }
          finally
          {
            m_selectionChangerManager.End( false, false );
          }
        }
      }

      var columnManager = dataGridContext.ColumnManager;

      using( columnManager.DeferUpdate( new ColumnHierarchyManager.UpdateOptions( TableView.GetFixedColumnCount( dataGridContext ), true ) ) )
      {
        ItemsSourceHelper.ResetPropertyDescriptions( m_itemsSourcePropertyDescriptions, m_itemPropertyMap, this, this.ItemsSource );
        ItemsSourceHelper.CleanUpColumns( this.Columns, false );

        if( this.AutoCreateColumns )
        {
          ItemsSourceHelper.CreateColumnsFromPropertyDescriptions( this.DataGridContext.ColumnManager,
                                                                   this.DefaultCellEditors,
                                                                   m_itemsSourcePropertyDescriptions,
                                                                   this.AutoCreateForeignKeyConfigurations );
        }

        var dataGridCollectionView = this.ItemsSource as DataGridCollectionView;
        if( dataGridCollectionView != null )
        {
          PropertyChangedEventManager.AddListener( dataGridCollectionView, this, string.Empty );
          this.SetValue( DataGridControl.StatContextPropertyKey, dataGridCollectionView.RootGroup );

          ProxyCollectionRefreshEventManager.AddListener( dataGridCollectionView, this );
          ProxyGroupDescriptionsChangedEventManager.AddListener( dataGridCollectionView, this );
          ProxySortDescriptionsChangedEventManager.AddListener( dataGridCollectionView, this );
        }
        else
        {
          this.ClearValue( DataGridControl.StatContextPropertyKey );
        }

        var virtualizingCollectionView = this.ItemsSource as DataGridVirtualizingCollectionViewBase;

        // Keep if the source is bound to DataGridVirtualizingCollectionView to avoid preserving ContainerSizeState to avoid memory leaks
        this.IsBoundToDataGridVirtualizingCollectionViewBase = ( virtualizingCollectionView != null );

        if( this.IsBoundToDataGridVirtualizingCollectionViewBase )
        {
          ConnectionStateChangedEventManager.AddListener( virtualizingCollectionView, this );
          ConnectionErrorChangedEventManager.AddListener( virtualizingCollectionView, this );
        }

        columnManager.Initialize( this );
      }

      dataGridContext.NotifyItemsSourceChanged();
      this.OnItemsSourceChangeCompleted();
    }

    [Obsolete( "The AddingNewDataItem event is obsolete and has been replaced by the DataGridCollectionView.CreatingNewItem and InitializingNewItem events.", false )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public event EventHandler<AddingNewDataItemEventArgs> AddingNewDataItem;

    [Obsolete( "The AddingNewDataItem event is obsolete and has been replaced by the DataGridCollectionView.CreatingNewItem and InitializingNewItem events.", false )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    protected internal virtual void OnAddingNewDataItem( AddingNewDataItemEventArgs e )
    {
      if( this.AddingNewDataItem != null )
      {
        this.AddingNewDataItem( this, e );
      }
    }

    public event EventHandler ItemsSourceChangeCompleted;

    private void OnItemsSourceChangeCompleted()
    {
      var handler = this.ItemsSourceChangeCompleted;
      if( handler == null )
        return;

      handler.Invoke( this, EventArgs.Empty );
    }

    protected override void OnItemsSourceChanged( IEnumerable oldValue, IEnumerable newValue )
    {
      base.OnItemsSourceChanged( oldValue, newValue );

      if( m_inhibitItemsCollectionChanged != null )
      {
        m_inhibitItemsCollectionChanged.Dispose();
        m_inhibitItemsCollectionChanged = null;
      }

      ScrollViewer scrollViewer = this.ScrollViewer;
      if( scrollViewer != null )
      {
        ScrollViewerHelper.ResetScrollPositions( scrollViewer );
      }

      //This section is to detect automatically the ItemsSourceName
      DataView dataView = newValue as DataView;
      if( dataView == null )
      {
        CollectionView colView = newValue as CollectionView;
        if( colView != null )
        {
          dataView = colView.SourceCollection as DataView;
        }
      }

      //Refresh the detectedName
      //If the DataSource is directly or indirectly a DataView, then use the TableName.
      object oldItemsSourceName = this.ItemsSourceName;
      m_detectedName = ( dataView != null ) ? dataView.Table.TableName : null;

      if( !object.Equals( this.ItemsSourceName, oldItemsSourceName ) )
      {
        this.OnPropertyChanged( "ItemsSourceName" );
      }

      DataGridContext localContext = this.DataGridContext;
      this.UpdateCurrentRowInEditionCellStates( null, null );
      localContext.ClearSizeStates();

      if( m_initCount != 0 )
      {
        this.ItemsSourceChangedDelayed = true;
      }
      else
      {
        this.ProcessDelayedItemsSourceChanged();
      }

      DataGridCollectionView dataGridCollectionView = oldValue as DataGridCollectionView;

      if( dataGridCollectionView != null )
      {
        PropertyChangedEventManager.RemoveListener( dataGridCollectionView, this, string.Empty );

        ProxyCollectionRefreshEventManager.RemoveListener( dataGridCollectionView, this );
        ProxyGroupDescriptionsChangedEventManager.RemoveListener( dataGridCollectionView, this );
        ProxySortDescriptionsChangedEventManager.RemoveListener( dataGridCollectionView, this );

        m_dataGridContextsStateDictionary = null;
      }

      DataGridVirtualizingCollectionViewBase virtualizingCollectionView = oldValue as DataGridVirtualizingCollectionViewBase;

      if( virtualizingCollectionView != null )
      {
        ConnectionStateChangedEventManager.RemoveListener( virtualizingCollectionView, this );
        ConnectionErrorChangedEventManager.RemoveListener( virtualizingCollectionView, this );
      }

      // We must refresh the FixedRegions to call Clear/Prepare Container for IDataGridItemContainer elements
      this.ResetFixedRegions();

      // Reset the flag
      this.ForceGeneratorReset = false;
    }

    private object OnCoerceItemsSource( object value )
    {
      // Ensure to EndEdit the currently edited container
      var currentContext = this.CurrentContext;
      var currentItem = currentContext.InternalCurrentItem;
      if( currentItem != null )
      {
        // Use Row.FromContainer to ensure Rows in Header/Footers are returned correctly
        var row = Row.FromContainer( currentContext.GetContainerFromItem( currentItem ) );
        if( ( row != null ) && ( row.IsBeingEdited ) )
        {
          row.CancelEdit();
        }
      }

      if( this.ItemsSource != value )
      {
        if( m_inhibitItemsCollectionChanged == null )
        {
          m_inhibitItemsCollectionChanged = this.Items.DeferRefresh();
        }
      }
      else if( m_inhibitItemsCollectionChanged != null )
      {
        m_inhibitItemsCollectionChanged.Dispose();
        m_inhibitItemsCollectionChanged = null;
      }

      return value;
    }

    private static object ItemsSourceCoerceCallback( DependencyObject sender, object value )
    {
      return ( ( DataGridControl )sender ).OnCoerceItemsSource( value );
    }

    #endregion

    #region SAVE/RESTORE STATE

    internal void SaveDataGridContextState( DataGridContext dataGridContext, bool handleExpandedItems, int maxGroupLevel )
    {
      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      Debug.Assert( !dataGridContext.IsRestoringState );

      if( dataGridContext.IsSavingState )
        return;

      dataGridContext.IsSavingState = true;

      try
      {
        WeakDataGridContextKey weakDataGridContextKey = new WeakDataGridContextKey( dataGridContext );

        if( m_dataGridContextsStateDictionary == null )
        {
          m_dataGridContextsStateDictionary = new Dictionary<WeakDataGridContextKey, SaveRestoreDataGridContextStateVisitor>();
        }
        else if( m_dataGridContextsStateDictionary.ContainsKey( weakDataGridContextKey ) )
        {
          // Already a state saved for this context.  Don't overwrite it.
          Debug.WriteLineIf( this.DebugSaveRestore, "Already a state saved for WeakDataGridContextKey: " + weakDataGridContextKey.GetHashCode().ToString() );
          return;
        }

        SaveRestoreDataGridContextStateVisitor saveRestoreDataGridContextStateVisitor = new SaveRestoreDataGridContextStateVisitor( handleExpandedItems, maxGroupLevel, false );

        try
        {
          saveRestoreDataGridContextStateVisitor.SaveState( ( IDataGridContextVisitable )dataGridContext );

          m_dataGridContextsStateDictionary.Add( weakDataGridContextKey, saveRestoreDataGridContextStateVisitor );

          Debug.WriteLineIf( this.DebugSaveRestore, "SAVING DataGridContext state for WeakDataGridContextKey: " + weakDataGridContextKey.GetHashCode().ToString() + " SAVED!" );
        }
        catch
        {
          Debug.WriteLineIf( this.DebugSaveRestore, "SAVING DataGridContext state for WeakDataGridContextKey: " + weakDataGridContextKey.GetHashCode().ToString() + " FAILED!" );
        }
      }
      finally
      {
        dataGridContext.IsSavingState = false;
      }
    }

    internal void RestoreDataGridContextState( DataGridContext dataGridContext )
    {
      if( ( m_dataGridContextsStateDictionary == null ) || ( m_dataGridContextsStateDictionary.Count == 0 ) )
        return;

      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      // A call to RestoreDataGridContextState can be made while processing a save or a restore
      // of a DataGridContext because the Generator forces the call in EnsureNodeTreeCreated
      // when public methods are accessed. If we are saving, already restoring or pending restoring
      // we ignore this call
      if( dataGridContext.IsSavingState
          || dataGridContext.IsDeferRestoringState
          || dataGridContext.IsRestoringState )
        return;

      WeakDataGridContextKey weakDataGridContextKey = new WeakDataGridContextKey( dataGridContext );
      SaveRestoreDataGridContextStateVisitor saveRestoreDataGridContextStateVisitor;

      if( m_dataGridContextsStateDictionary.TryGetValue( weakDataGridContextKey, out saveRestoreDataGridContextStateVisitor ) )
      {
        try
        {
          dataGridContext.IsRestoringState = true;

          // Restoring this dataGridContext state's will expand sub items that should be expanded.
          // Their expansion will make the customItemGenerator generate sub detailNodes, which in turn will try 
          // to be restored to their previous state.
          saveRestoreDataGridContextStateVisitor.RestoreState( ( IDataGridContextVisitable )dataGridContext );
          Debug.WriteLineIf( this.DebugSaveRestore, "RESTORING DataGridContext state for WeakDataGridContextKey: " + weakDataGridContextKey.GetHashCode().ToString() + " RESTORED!" );
        }
        catch
        {
          Debug.WriteLineIf( this.DebugSaveRestore, "RESTORING DataGridContext state for WeakDataGridContextKey: " + weakDataGridContextKey.GetHashCode().ToString() + " FAILED!" );
        }
        finally
        {
          m_dataGridContextsStateDictionary.Remove( weakDataGridContextKey );
          dataGridContext.IsRestoringState = false;
        }
      }
      else
      {
        Debug.WriteLineIf( this.DebugSaveRestore, "Cannot Restore. No state saved for WeakDataGridContextKey: " + weakDataGridContextKey.GetHashCode().ToString() );
      }
    }

    private void OnDataGridCollectionView_ProxyCollectionRefresh( DataGridCollectionView sender, EventArgs e )
    {
      if( ( sender == null ) || ( sender.GroupDescriptions.Count <= 0 ) )
        return;

      var targetDataGridContext = DataGridContext.SafeGetDataGridContextForDataGridCollectionView( this.DataGridContext, sender );
      if( targetDataGridContext != null )
      {
        this.SaveDataGridContextState( targetDataGridContext, false, int.MaxValue );
      }
    }

    private void OnDataGridCollectionView_ProxyGroupDescriptionsChanged( DataGridCollectionView sender, NotifyCollectionChangedEventArgs e )
    {
      if( sender == null )
        return;

      var maxGroupLevelToSave = -1;

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Add:
          {
            // Only save/restore group states when there was already at least one GroupDescription.
            maxGroupLevelToSave = e.NewStartingIndex - 1;
            break;
          }

        case NotifyCollectionChangedAction.Move:
          {
            // Only save/restore the state if there's other groups located before the moved group old/new index.
            // Else, do not save, since we already know that we won't be able to find any match when restoring.
            maxGroupLevelToSave = Math.Min( e.OldStartingIndex, e.NewStartingIndex ) - 1;
            break;
          }

        case NotifyCollectionChangedAction.Remove:
          {
            // No need to save/restore groups states if there won't be any group left.
            maxGroupLevelToSave = e.OldStartingIndex - 1;
            break;
          }

        case NotifyCollectionChangedAction.Replace:
          {
            maxGroupLevelToSave = e.NewStartingIndex - 1;
            break;
          }

        case NotifyCollectionChangedAction.Reset:
          {
            // Don't save.
            break;
          }
      }

      if( maxGroupLevelToSave > -1 )
      {
        var targetDataGridContext = DataGridContext.SafeGetDataGridContextForDataGridCollectionView( this.DataGridContext, sender );
        if( targetDataGridContext != null )
        {
          this.SaveDataGridContextState( targetDataGridContext, false, maxGroupLevelToSave );
        }
      }
    }

    private void OnDataGridCollectionView_ProxySortDescriptionsChanged( DataGridCollectionView sender, NotifyCollectionChangedEventArgs e )
    {
      if( ( sender == null ) || ( sender.GroupDescriptions.Count <= 0 ) )
        return;

      var targetDataGridContext = DataGridContext.SafeGetDataGridContextForDataGridCollectionView( this.DataGridContext, sender );
      if( targetDataGridContext != null )
      {
        this.SaveDataGridContextState( targetDataGridContext, false, int.MaxValue );
      }
    }

    private void OnDataGridCollectionView_ProxyAutoFilterValuesChanged( DataGridCollectionView sender, NotifyCollectionChangedEventArgs e )
    {
      if( ( sender == null ) || ( sender.GroupDescriptions.Count <= 0 ) )
        return;

      var targetDataGridContext = DataGridContext.SafeGetDataGridContextForDataGridCollectionView( this.DataGridContext, sender );
      if( targetDataGridContext != null )
      {
        this.SaveDataGridContextState( targetDataGridContext, false, int.MaxValue );
      }
    }

    private void OnDataGridCollectionView_ProxyApplyingFilterCriterias( DataGridCollectionView sender, EventArgs e )
    {
      if( ( sender == null ) || ( sender.GroupDescriptions.Count <= 0 ) )
        return;

      var targetDataGridContext = DataGridContext.SafeGetDataGridContextForDataGridCollectionView( this.DataGridContext, sender );
      if( targetDataGridContext != null )
      {
        this.SaveDataGridContextState( targetDataGridContext, false, int.MaxValue );
      }
    }

    #endregion

    #region VIRTUALIZING COLLECTION VIEW SUPPORT

    private void OnDataGridVirtualizingCollectionViewBase_ConnectionStateChanged( DataGridVirtualizingCollectionViewBase sender, EventArgs e )
    {
      if( sender == null )
        return;

      this.SetConnectionState( sender.ConnectionState );
    }

    private void OnDataGridVirtualizingCollectionViewBase_ConnectionErrorChanged( DataGridVirtualizingCollectionViewBase sender, EventArgs e )
    {
      if( sender == null )
        return;

      this.SetConnectionError( sender.ConnectionError );
    }

    #endregion

    #region EDITING

    public void BeginEdit()
    {
      this.BeginEdit( this.CurrentContext.InternalCurrentItem );
    }

    public void BeginEdit( object item )
    {
      DataGridControl.BeginEditHelper( this.DataGridContext, item );
    }

    internal static void BeginEditHelper( DataGridContext dataGridContext, object item )
    {
      if( item == null )
        return;

      var dataGridControl = dataGridContext.DataGridControl;
      if( dataGridControl.IsBeingEdited )
        return;

      if( !dataGridContext.IsContainingItem( item ) )
        throw new InvalidOperationException( "An attempt was made to call the BeginEdit method of an item that is not part of the specified context." );

      var container = dataGridContext.GetContainerFromItem( item );

      //if the item is realized, then I could call the BeginEdit() directly on the container.
      if( container != null )
      {
        var row = Row.FromContainer( container );
        if( row != null )
        {
          row.BeginEdit();
        }

        //not a row, then not editable.
      }
      //if the container is not realized, then I need to set things up so that when the container is realized, it's gonna resume edition.
      else
      {
        dataGridContext.DataGridControl.UpdateCurrentRowInEditionCellStates( null, item );
        dataGridContext.DataGridControl.BringItemIntoView( item );
      }
    }

    public void EndEdit()
    {
      DataGridControl.EndEditHelper( this.DataGridContext );
    }

    internal static void EndEditHelper( DataGridContext dataGridContext )
    {
      var dataGridControl = dataGridContext.DataGridControl;
      if( !dataGridControl.IsBeingEdited )
        return;

      if( !dataGridContext.IsContainingItem( dataGridContext.DataGridControl.CurrentItemInEdition ) )
        throw new InvalidOperationException( "An attempt was made to call the EndEdit method of an item that is not part of the specified context." );

      var container = dataGridContext.GetContainerFromItem( dataGridControl.m_currentItemInEdition );

      //if the item is realized, then I could call the EndEdit() directly on the container.
      if( container != null )
      {
        var row = Row.FromContainer( container );
        if( row != null )
        {
          row.EndEdit();
        }

        //not a row, then not editable.
      }
      //if the container is not realized, then set things up so that when the container is realized, editing will resume.
      else
      {
        DataGridControl.ProcessUnrealizedEndEdit( dataGridContext );
      }
    }

    public void CancelEdit()
    {
      DataGridControl.CancelEditHelper( this.DataGridContext );
    }

    internal static void CancelEditHelper( DataGridContext dataGridContext )
    {
      var dataGridControl = dataGridContext.DataGridControl;
      if( !dataGridControl.IsBeingEdited )
        return;

      if( !dataGridContext.IsCurrent )
        throw new InvalidOperationException( "An attempt was made to call the CancelEdit method on a DataGridContext that is not current." );

      var container = dataGridContext.GetContainerFromItem( dataGridControl.m_currentItemInEdition );

      //if the item is realized, then I could call the CancelEdit() directly on the container.
      if( container != null )
      {
        var row = Row.FromContainer( container );
        if( row != null )
        {
          row.CancelEdit();
        }

        //not a row, then not editable.
      }
      //if the container is not realized, then I need to clear the parameters that indicate that the row is being edited.
      else
      {
        dataGridControl.UpdateCurrentRowInEditionCellStates( null, null );
      }
    }

    private static void ProcessUnrealizedEndEdit( DataGridContext dataGridContext )
    {
      Row row;

      object currentItemInEdition = dataGridContext.DataGridControl.m_currentItemInEdition;

      //first, determine if the item currently in edition is an Item or a [Group]Header/Footer
      if( dataGridContext.Items.Contains( currentItemInEdition ) )
      {
        //the item currently in edition is a data item.
        DependencyObject container = dataGridContext.DataGridControl.CreateContainerForItem();
        Debug.Assert( container != null, "CreateContainerForItem() returned null" );

        row = container as Row;
      }
      else
      {
        DataTemplate template = DataGridControl.GetTemplateForItem( currentItemInEdition );
        Debug.Assert( template != null, "DataTemplate for [Group]Header/Footer is null" );

        HeaderFooterItem headerFooterItem = new HeaderFooterItem();
        headerFooterItem.ContentTemplate = template;
        headerFooterItem.Measure( new Size( double.PositiveInfinity, double.PositiveInfinity ) );

        row = headerFooterItem.AsVisual() as Row;
      }

      if( row != null )
      {
        // Ensure to set the DataGridContext on the Container for the 
        // PrepareItemContainer to correctly fetch the DataGridContext
        DataGridControl.SetDataGridContext( row, dataGridContext );
        dataGridContext.DataGridControl.PrepareItemContainer( row, currentItemInEdition );

        try
        {
          row.EndEdit();
        }
        finally
        {
          dataGridContext.DataGridControl.ClearItemContainer( row, currentItemInEdition );
        }
      }
    }

    // Used to initialize/clear Column.CurrentRowInEditionCellState for each cell in the row passed as parameter
    internal void UpdateCurrentRowInEditionCellStates( Row newCurrentItemContainer, object newCurrentItemInEdition )
    {
      if( newCurrentItemInEdition != m_currentItemInEdition )
      {
        var currentRowInEdition = default( Row );

        if( m_currentItemInEdition != null )
        {
          // Get the container for m_currentItemInEdition
          currentRowInEdition = Row.FromContainer( this.CurrentContext.GetContainerFromItem( m_currentItemInEdition ) );

          if( newCurrentItemInEdition != null )
          {
            if( ( currentRowInEdition != null ) && ( currentRowInEdition.IsBeingEdited ) )
              throw new InvalidOperationException( "An attempt was made to place a row in edit mode while another row is being edited." );
          }
        }

        // The newCurrentItemContainer is null
        if( newCurrentItemContainer == null )
        {
          if( currentRowInEdition != null )
          {
            // We must clear the edition state of the old Row in edition
            foreach( Cell cell in currentRowInEdition.CreatedCells )
            {
              var parentColumn = cell.ParentColumn;
              if( parentColumn == null )
                continue;

              parentColumn.CurrentRowInEditionCellState = null;
            }
          }
        }

        m_currentItemInEdition = newCurrentItemInEdition;
        m_currentRowInEditionState = new RowState();

        this.UpdateIsBeingEdited();
      }

      // It may occur that the newCurrentItemInEdition was set for a
      // Container that is currently out of view, so the newCurrentItemContainer
      // was null at this time. We must then ensure the CellStates are 
      // create for the newCurrentItemContainer when not null even if 
      // newCurrentItemInEdition == m_currentItemInEdition
      if( newCurrentItemContainer != null )
      {
        foreach( Cell cell in newCurrentItemContainer.CreatedCells )
        {
          var parentColumn = cell.ParentColumn;
          if( parentColumn == null )
            continue;

          var cellState = new CellState();
          cellState.SetContentBeforeRowEdition( cell.Content );

          parentColumn.CurrentRowInEditionCellState = cellState;
        }
      }
    }

    #endregion

    #region SELECTION/CURRENT

    internal bool UseDragToSelectBehavior
    {
      get
      {
        return false;
      }
    }

    internal bool IsSetCurrentInProgress
    {
      get
      {
        return m_flags[ ( int )DataGridControlFlags.IsSetCurrentInProgress ];
      }
      set
      {
        m_flags[ ( int )DataGridControlFlags.IsSetCurrentInProgress ] = value;
      }
    }

    internal void QueueClearCurrentColumn( object currentItem )
    {
      if( m_queueClearCurrentColumn != null )
      {
        m_queueClearCurrentColumn.Abort();
        m_queueClearCurrentColumn = null;
      }

      m_queueClearCurrentColumn = this.Dispatcher.BeginInvoke( System.Windows.Threading.DispatcherPriority.DataBind,
                                                               new ParametrizedGenericHandler( ClearCurrentColumnHandler ), currentItem );
    }

    internal void AddToSelectedContexts( DataGridContext context )
    {
      if( !m_selectedContexts.Contains( context ) )
        m_selectedContexts.Add( context );
    }

    internal void RemoveFromSelectedContexts( DataGridContext context )
    {
      m_selectedContexts.Remove( context );
    }

    private void ClearCurrentColumnHandler( object oldCurrentItem )
    {
      try
      {
        DataGridContext currentDataGridContext = this.CurrentContext;

        //the old data item was nescessarily the current item since only the current item can be in edit mode.
        if( currentDataGridContext.InternalCurrentItem == oldCurrentItem )
        {
          try
          {
            currentDataGridContext.SetCurrent( oldCurrentItem, null, null, null, false, false,
                                               this.SynchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers.Editing );
          }
          catch( DataGridException )
          {
            // We swallow the exception if it occurs because of a validation error or Cell was read-only or any other GridException.
          }
        }
      }
      finally
      {
        m_queueClearCurrentColumn = null;
      }
    }

    private void UpdateCurrentContextOnThemeChanged( DataGridContext currentContext )
    {
      CustomItemContainerGenerator generator = this.CustomItemContainerGenerator;

      // the CurrentContext is the master DataGridContext, and this one will never change
      if( currentContext == this.DataGridContext )
        return;

      object newCurrentItem = currentContext.CurrentItem;

      // Try to get the new DataGridContext mathing the old one (containing the old CurrentItem).
      DataGridContext newCurrentContext = this.UpdateCurrentContextRecursive( this.DataGridContext, currentContext, out newCurrentItem );

      // If it none was found, affect the master DataGridContext
      if( newCurrentContext == null )
      {
        Debug.WriteLine( "Unable to find a matching DataGridContext" );
        this.SetCurrentDataGridContextHelper( this.DataGridContext );
      }
      else
      {
        // Affect the newly found DataGridContext
        this.SetCurrentDataGridContextHelper( newCurrentContext );

        // Set it as current using the old context CurrentItem, CurrentColumn
        newCurrentContext.SetCurrent( newCurrentItem, null, null, currentContext.CurrentColumn, false, false,
                                      this.SynchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers.ThemeChanged );
      }
    }

    private DataGridContext UpdateCurrentContextRecursive( DataGridContext parentDataGridContext, DataGridContext oldCurrentDataGridContext, out object newCurrentItem )
    {
      if( parentDataGridContext == null )
      {
        newCurrentItem = null;
        return null;
      }

      foreach( DataGridContext childContext in parentDataGridContext.GetChildContexts() )
      {
        if( childContext.SourceDetailConfiguration == oldCurrentDataGridContext.SourceDetailConfiguration )
        {
          object oldCurrentItem = oldCurrentDataGridContext.CurrentItem;

          System.Data.DataView dataView = ItemsSourceHelper.TryGetDataViewFromDataGridContext( childContext );

          System.Data.DataRowView oldCurrentDataRowView = oldCurrentItem as System.Data.DataRowView;

          if( ( dataView != null ) && ( oldCurrentDataRowView != null ) )
          {
            System.Data.DataRow oldDataRow = oldCurrentDataRowView.Row;

            foreach( System.Data.DataRowView dataRowView in dataView )
            {
              if( dataRowView.Row == oldDataRow )
              {
                newCurrentItem = dataRowView;
                return childContext;
              }
            }
          }
          else
          {
            if( childContext.Items.Contains( oldCurrentItem ) )
            {
              newCurrentItem = oldCurrentItem;
              return childContext;
            }
          }
        }

        DataGridContext foundContext = this.UpdateCurrentContextRecursive( childContext, oldCurrentDataGridContext, out newCurrentItem );

        if( foundContext != null )
          return foundContext;
      }

      newCurrentItem = null;
      return null;
    }

    #endregion

    #region DRAG TO SELECT

    private AutoScrollManager AutoScrollManager
    {
      get
      {
        return m_autoScrollManager;
      }
      set
      {
        if( m_autoScrollManager != null )
        {
          m_autoScrollManager.AutoScrolled -= this.OnAutoScrollManagerAutoScrolled;
        }

        m_autoScrollManager = value;

        if( m_autoScrollManager != null )
        {
          m_autoScrollManager.AutoScrolled += this.OnAutoScrollManagerAutoScrolled;
        }
      }
    }

    private void OnPreviewMouseDown_DragToSelect( MouseButtonEventArgs e )
    {
      if( !this.UseDragToSelectBehavior )
        return;

      if( e.LeftButton != MouseButtonState.Pressed )
        return;

      var source = e.OriginalSource as DependencyObject;
      if( source == null )
        return;

      var container = DataGridControl.GetContainer( source );
      var itemContainer = container as IDataGridItemContainer;
      if( itemContainer == null )
        return;

      var dataGridContext = DataGridControl.GetDataGridContext( container );
      if( ( dataGridContext == null ) || ( dataGridContext.DataGridControl != this ) )
        return;

      m_startDragSelection = false;

      if( container is Row )
      {
        m_startDragSelection = ( DataGridVirtualizingPanel.GetItemIndex( container ) >= 0 );
      }
    }

    private void OnMouseLeftButtonUp_DragToSelect( MouseButtonEventArgs e )
    {
      this.AbortDragSelection();
    }

    private void OnLostMouseCapture_DragToSelect( MouseEventArgs e )
    {
      this.AbortDragSelection();
    }

    private void OnMouseMove_DragToSelect( MouseEventArgs e )
    {
      // Verify if the "Excel like drag selection" should be handled.
      if( !this.UseDragToSelectBehavior )
        return;

      if( e.LeftButton != MouseButtonState.Pressed )
        return;

      if( !this.IsKeyboardFocusWithin )
        return;

      var autoScrollManager = this.AutoScrollManager;
      if( autoScrollManager != null )
      {
        m_startDragSelection = false;

        // This should happen only on the second mouse move after the mouse down.
        // The mouse is capture so "MouseMove" and "ButtonUp" events can be handled outside the DataGridControl.
        if( !this.IsMouseCaptured )
        {
          this.CaptureMouse();
        }

        autoScrollManager.HandleMouseMove( e );
        e.Handled = this.SelectMouseOverElement();
      }
      // This should happen only on the first mouse move after the mouse down, if it was detected that DragSelection should be processed.
      else if( m_startDragSelection )
      {
        m_startDragSelection = false;

        // Before starting the AutoScrollManager, make sure all the conditions that apply in OnPreviewMouseDown_DragToSelect are still valid here.
        var source = e.OriginalSource as DependencyObject;
        if( source == null )
          return;

        var container = DataGridControl.GetContainer( source );
        var itemContainer = container as IDataGridItemContainer;
        if( itemContainer == null )
          return;

        var dataGridContext = DataGridControl.GetDataGridContext( container );
        if( ( dataGridContext == null ) || ( dataGridContext.DataGridControl != this ) )
          return;

        autoScrollManager = new AutoScrollManager( this.ScrollViewer );
        autoScrollManager.Start();
        this.AutoScrollManager = autoScrollManager;
      }
    }

    private void OnAutoScrollManagerAutoScrolled( object sender, EventArgs e )
    {
      Debug.Assert( this.UseDragToSelectBehavior );
      this.SelectMouseOverElement();
    }

    private void AbortDragSelection()
    {
      var autoScrollManager = this.AutoScrollManager;
      if( autoScrollManager != null )
      {
        autoScrollManager.Stop();
        this.AutoScrollManager = null;
        this.ReleaseMouseCapture();
      }
    }

    private bool SelectMouseOverElement()
    {
      var itemsHost = this.ItemsHost;
      var mousePosition = Mouse.GetPosition( itemsHost );
      var hitPosition = new Point();

      var width = itemsHost.ActualWidth;
      var height = itemsHost.ActualHeight;

      //Here, Since we are auto scrolling, the mouse position may be 
      // out of the bounds of the datagrid. Get the mouse's nearest row or cell.
      hitPosition.Y = Math.Min( Math.Max( 0d, mousePosition.Y ), height - 1 );
      hitPosition.X = Math.Min( Math.Max( 0d, mousePosition.X ), width - 1 );

      var targetItem = itemsHost.InputHitTest( hitPosition ) as DependencyObject;
      if( targetItem == null )
        return false;

      var container = DataGridControl.GetContainer( targetItem );
      if( container == null )
        return false;

      var dataGridContext = DataGridControl.GetDataGridContext( container );
      if( ( dataGridContext == null ) || ( DataGridContext.DataGridControl != this ) )
        return false;

      var newPosition = default( SelectionRangePoint );

      var row = container as Row;
      if( row != null )
      {
        var sourceDataItemIndex = DataGridVirtualizingPanel.GetItemIndex( row );
        if( sourceDataItemIndex < 0 )
          return false;

        var columnIndex = -1;

        // Dont border finding the cell if we are not in cell selection mode.
        if( this.SelectionUnit == DataGrid.SelectionUnit.Cell )
        {
          var cell = targetItem as Cell;
          if( ( cell == null ) || ( cell.ParentColumn.DataGridControl != this ) )
          {
            cell = Cell.FindFromChild( this, targetItem );
          }

          if( cell == null )
          {
            // If the cell is still null, It may be because the X mouse position was out of bounds
            // an thus, we should take the first or last visible cell in screen
            if( mousePosition.X >= 0d && mousePosition.X < itemsHost.Width )
              return false;

            var columnManager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManagerBase;
            if( columnManager == null )
              return false;

            var columns = columnManager.GetVisibleFieldNames();
            if( columns.Count <= 0 )
              return false;

            var index = ( mousePosition.X < 0 ) ? 0 : columns.Count - 1;

            columnIndex = dataGridContext.Columns[ columns[ index ] ].VisiblePosition;
          }
          else
          {
            columnIndex = cell.ParentColumn.VisiblePosition;
          }
        }

        var item = dataGridContext.GetItemFromContainer( row );

        newPosition = SelectionRangePoint.TryCreateRangePoint( dataGridContext, item, sourceDataItemIndex, columnIndex );
      }
      else
      {
        return false;
      }

      if( newPosition == null )
        return false;

      m_selectionChangerManager.UpdateSelection( null, newPosition, true, false, SelectionManager.UpdateSelectionSource.MouseMove );
      return true;
    }

    #endregion

    public override void OnApplyTemplate()
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      DataGridControl parentGrid = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      if( parentGrid == null )
      {
        this.SetValue( ParentDataGridControlPropertyKey, this );
      }

      base.OnApplyTemplate();

      m_scrollViewer = this.Template.FindName( "PART_ScrollViewer", this ) as ScrollViewer;
      m_dragDropAdornerDecorator = this.Template.FindName( "PART_DragDropAdornerDecorator", this ) as AdornerDecorator;

      this.CustomItemContainerGenerator.InvalidateIsInUse();

      m_itemsHost = null;

      Views.ViewBase view = this.GetView();

      if( view.UseDefaultHeadersFooters )
      {
        view.InvokeAddDefaultHeadersFooters();
      }

      // Cache if the view requires to preserve container size
      this.ViewPreservesContainerSize = view.PreserveContainerSize;

      // Notify the template was reapplied
      if( this.TemplateApplied != null )
      {
        this.TemplateApplied( this, EventArgs.Empty );
      }
    }

    public bool ExpandGroup( CollectionViewGroup group )
    {
      if( group == null )
        throw new ArgumentNullException( "group" );

      return this.DataGridContext.ExpandGroup( group );
    }

    public bool CollapseGroup( CollectionViewGroup group )
    {
      if( group == null )
        throw new ArgumentNullException( "group" );

      return this.DataGridContext.CollapseGroup( group );
    }

    public bool ToggleGroupExpansion( CollectionViewGroup group )
    {
      if( group == null )
        throw new ArgumentNullException( "group" );

      return this.DataGridContext.ToggleGroupExpansion( group );
    }

    public bool IsGroupExpanded( CollectionViewGroup group )
    {
      if( group == null )
        throw new ArgumentNullException( "group" );

      return this.DataGridContext.IsGroupExpanded( group );
    }

    public bool AreDetailsExpanded( object dataItem )
    {
      //This function will work only for master items.
      //In the cases where a Detail item is passed to the function, the function will return false.
      return this.DataGridContext.AreDetailsExpanded( dataItem );
    }

    public DataGridContext GetChildContext( object parentItem, string relationName )
    {
      return this.DataGridContext.GetChildContext( parentItem, relationName );
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
    public IEnumerable<DataGridContext> GetChildContexts()
    {
      return this.DataGridContext.GetChildContexts();
    }

    public DependencyObject GetContainerFromItem( object item )
    {
      return this.DataGridContext.GetContainerFromItem( item );
    }

    public Group GetGroupFromCollectionViewGroup( CollectionViewGroup collectionViewGroup )
    {
      return this.DataGridContext.GetGroupFromCollectionViewGroup( collectionViewGroup );
    }

    [EditorBrowsable( EditorBrowsableState.Never )]
    public DependencyObject GetContainerFromIndex( int index )
    {
      return this.DataGridContext.GetContainerFromIndex( index );
    }

    [EditorBrowsable( EditorBrowsableState.Never )]
    public int GetIndexFromContainer( DependencyObject container )
    {
      return this.DataGridContext.GetIndexFromContainer( container );
    }

    public object GetItemFromContainer( DependencyObject container )
    {
      return this.DataGridContext.GetItemFromContainer( container );
    }

    public CollectionViewGroup GetParentGroupFromItem( object item )
    {
      return this.DataGridContext.GetParentGroupFromItem( item );
    }

    public IDisposable DeferColumnsUpdate()
    {
      return this.DataGridContext.DeferColumnsUpdate();
    }

    public bool MoveColumnBefore( ColumnBase current, ColumnBase next )
    {
      return this.DataGridContext.MoveColumnBefore( current, next );
    }

    public bool MoveColumnAfter( ColumnBase current, ColumnBase previous )
    {
      return this.DataGridContext.MoveColumnAfter( current, previous );
    }

    public bool MoveColumnUnder( ColumnBase current, ColumnBase parent )
    {
      return this.DataGridContext.MoveColumnUnder( current, parent );
    }

    [Obsolete( "MoveMergedColumn is obsolete. Use MoveColumnUnder instead." )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public void MoveMergedColumn( ColumnBase columnToMove, ColumnBase parentMergedColumn )
    {
      if( !this.MoveColumnUnder( columnToMove, parentMergedColumn ) )
        throw new InvalidOperationException( "Failed to move the column." );
    }

    [Obsolete( "MoveMergedColumn is obsolete. Use MoveColumnUnder instead." )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public void MoveMergedColumn( string columnToMove, string parentMergedColumn )
    {
      var level = -1;
      var parentColumn = default( ColumnBase );

      if( level == -1 )
        throw new ArgumentException( "An attempt was made to use a column that is not part of a MergedHeader.", "parentMergedColumn" );

      ColumnBase childColumn = null;
      childColumn = this.Columns[ columnToMove ];
      if( childColumn == null )
        throw new ArgumentException( "An attempt was made to move an invalid child column to a parent merged column.", "childColumn" );

      if( !this.MoveColumnUnder( childColumn, parentColumn ) )
        throw new InvalidOperationException( "Failed to move the column." );
    }

    protected override Size MeasureOverride( Size constraint )
    {
      //By default the measure pass must not be delayed, or if the grid is already loaded or if we are printing.
      if( ( !this.DeferInitialLayoutPass ) || ( this.IsLoaded )|| ( !m_isFirstTimeLoaded ) )
        return base.MeasureOverride( constraint );

      return this.GetStartUpSize( constraint );
    }

    protected override Size ArrangeOverride( Size arrangeBounds )
    {
      //By default the measure pass must not be delayed, or if the grid is already loaded or if we are printing.
      if( ( !this.DeferInitialLayoutPass ) || ( this.IsLoaded ) || ( !m_isFirstTimeLoaded ) )
        return base.ArrangeOverride( arrangeBounds );

      return arrangeBounds;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
      return new DataRow();
    }

    protected override bool IsItemItsOwnContainerOverride( object item )
    {
      return ( item is DataRow );
    }

    protected override void ClearContainerForItemOverride( DependencyObject element, object item )
    {
      base.ClearContainerForItemOverride( element, item );
      FrameworkElement feContainer = element as FrameworkElement;

      if( ( ( feContainer.IsKeyboardFocused ) || ( feContainer.IsKeyboardFocusWithin ) )
        && ( !this.IsSetFocusInhibited ) )
      {
        // If the cleared element contain focus, move the focus on the scrollViewer        
        this.ForceFocus( this.ScrollViewer );
      }

      var dataGridItemContainer = element as IDataGridItemContainer;
      var row = element as Row;

      // Only preserve ContainerSizeState if necessary
      if( !this.IsBoundToDataGridVirtualizingCollectionViewBase
          && this.ViewPreservesContainerSize )
      {
        var dataGridContext = DataGridControl.GetDataGridContext( element );

        //Save the size state of the container (this will do nothing if no size has been set on the item ).
        dataGridContext.SaveContainerSizeState( item, feContainer );
      }

      // Try to get the element as a HeaderFooterItem to avoid
      // calling ClearContainer on a Row inside a HeaderFooterItem.
      // HeaderFooterItem implements IDataGridItemContainer and we call
      // ClearContainer on it if not null.
      var hfi = element as HeaderFooterItem;

      // If element is a HeaderFooterItem, then it cannot be a Row
      if( hfi != null )
      {
        row = Row.FromContainer( hfi );
      }

      // Special handling case for the Row class when it is not in a HeaderFooterItem
      if( row != null )
      {
        //cancel edit if applicable and preserve state of the row.
        if( row.IsBeingEdited )
        {
          var oldRowEditionState = m_currentRowInEditionState.Clone();
          var oldColumnsStates = new Dictionary<ColumnBase, CellState>();

          //Debug.Assert( item == m_currentItemInEdition, "The current item being edited on the DataGridControl differs from the one of the currently edited container." );

          // We can use row because there is only one row in edition in the DataGridControl
          foreach( Cell cell in row.CreatedCells )
          {
            var parentColumn = cell.ParentColumn;
            if( parentColumn == null )
              continue;

            oldColumnsStates.Add( parentColumn, parentColumn.CurrentRowInEditionCellState.Clone() );
          }

          try
          {
            row.CancelEdit();
          }
          catch( InvalidOperationException )
          {
            // CancelEdit can throw if we call it when we are in the state of ending the edit process.
          }

          // We do not clear any thing when the item is its own container.
          if( !this.IsItemItsOwnContainerOverride( item ) )
          {
            // Call the ClearContainer on the correct HeaderFooterItem if not null
            // to correctly unregister the HeaderFooterItem from the Loaded event 
            // else directly on the Row
            if( hfi != null )
            {
              ( ( IDataGridItemContainer )hfi ).ClearContainer();
            }
            else
            {
              ( ( IDataGridItemContainer )row ).ClearContainer();
            }
          }

          m_currentRowInEditionState = oldRowEditionState;

          foreach( ColumnBase column in oldColumnsStates.Keys )
          {
            column.CurrentRowInEditionCellState = oldColumnsStates[ column ];
          }

          // Though the row state is preserved, the DataGridControl is not in editing anymore.
          m_currentItemInEdition = null;
          this.UpdateIsBeingEdited();
        }
        else
        {
          // We do not clear any thing when the item is its own container.
          if( !this.IsItemItsOwnContainerOverride( item ) )
          {
            // Call the ClearContainer on the correct HeaderFooterItem if not null
            // to correctly unregister the HeaderFooterItem from the Loaded event 
            // else directly on the Row
            if( hfi != null )
            {
              ( ( IDataGridItemContainer )hfi ).ClearContainer();
            }
            else
            {
              ( ( IDataGridItemContainer )row ).ClearContainer();
            }
          }
        }
      }
      else if( dataGridItemContainer != null )
      {
        //If the Container is not directly a Row (data item) or if the first visual child of HFI is not a Row ( row header or footer).
        //then call the ClearContainer on the interface (will catch cases such as GroupHeaderControl ).
        dataGridItemContainer.ClearContainer();
      }

      var uiElement = element as UIElement;
      if( ( uiElement != null ) && ( ( uiElement.IsKeyboardFocusWithin ) || ( uiElement.IsKeyboardFocused ) ) )
      {
        // If the element we are cleaning up contains the focus, we ensure to refocus the ScrollViewer to have
        // the focus on a valid element of the grid.
        this.DelayBringIntoViewAndFocusCurrent( AutoScrollCurrentItemSourceTriggers.FocusChanged );
      }
    }

    protected override void PrepareContainerForItemOverride( DependencyObject element, object item )
    {
      base.PrepareContainerForItemOverride( element, item );

      //this is specific to the DataRows from the Original items list... do not want to do that in the Row.PrepareContainer()

      IDataGridItemContainer dataGridItemContainer = element as IDataGridItemContainer;
      Row row = element as Row;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( element );
      Debug.Assert( dataGridContext != null );

      if( dataGridItemContainer != null )
      {
        dataGridItemContainer.PrepareContainer( dataGridContext, item );
      }

      //Note: the ItemContainerStyle and ItemContainerStyleSelector will only be applied if the container is a Row or Row-derived object.
      // Headers, Footers, Group Headers and Group Footers, even if they contains row types will not be affected since its only the "root" container
      // that matters ( headers and the such are enclosed within a HeaderFooterItem container ).

      if( row != null )
      {
        //First, Check if the GroupConfiguration for the container has an ItemContainerStyle defined.
        GroupConfiguration groupConfig = DataGridControl.GetContainerGroupConfiguration( row );
        Style itemContainerStyle = null;

        //If there is a GroupConfiguration present, try to use its ItemContainerStyle
        if( groupConfig != null )
        {
          //If an ItemContainerStyle is applied on the GroupConfiguration, I want to force it onto the Container.
          if( groupConfig.ItemContainerStyle != null )
          {
            itemContainerStyle = groupConfig.ItemContainerStyle;
          }
          //if no Style but a Selector is present, the same is done.
          else if( groupConfig.ItemContainerStyleSelector != null )
          {
            itemContainerStyle = groupConfig.ItemContainerStyleSelector.SelectStyle( item, row );
          }
        }

        //If there is no GroupConfiguration or if there is no ItemContainerStyle defined on it, go look in the DataGridContext
        if( itemContainerStyle == null )
        {
          //If not, Use the DataGridContext to determine the appropriate ItemContainerStyle to be applied to the container
          DataGridContext containerContext = DataGridControl.GetDataGridContext( row );
          if( containerContext == null )
          {
            //container did not pass by the CustomItemContainerGenerator
            containerContext = this.DataGridContext;
          }

          if( containerContext != null )
          {
            //If an ItemContainerStyle is applied on the DataGridContext, I want to force it onto the Container.
            if( containerContext.ItemContainerStyle != null )
            {
              itemContainerStyle = containerContext.ItemContainerStyle;
            }
            //if no Style but a Selector is present, the same is done.
            else if( containerContext.ItemContainerStyleSelector != null )
            {
              itemContainerStyle = containerContext.ItemContainerStyleSelector.SelectStyle( item, row );
            }
          }
        }

        //If an ItemContainerStyle was found anywhere (GroupConfig or Context), assign it to the container.
        if( itemContainerStyle != null )
        {
          row.Style = itemContainerStyle;
        }
        else
        {
          //this condition is for Rows that are recycled from an environment where they had and item container style to one that doesn't...
          if( row.ReadLocalValue( Row.StyleProperty ) != DependencyProperty.UnsetValue )
          {
            row.ClearValue( Row.StyleProperty );
          }
        }

      } // end if row != null ( data items only)

      //The goal of the next section is to ensure that focus is moved to the appropriate element of the visual tree when the container is being prepared.
      //This is to ensure that focus will not be left on a control that has been recycled ( and which is no longer the one we want ).

      //This next section I want to perform independently of wether the container is for a [Fixed]Header/Footer, DataItem, Group Header/Footer
      //as such, I will re-determine if the item is a Row ( this time digging beyond HeaderFooterItem

      // Only restore ContainerSizeState if necessary
      if( !this.IsBoundToDataGridVirtualizingCollectionViewBase && this.ViewPreservesContainerSize )
      {
        //Restore the Size of the item (if there was some special size assigned to the item container previously
        dataGridContext.RestoreContainerSizeState( item, element as FrameworkElement );
      }

      // We restore the edit state during the OnApplyTemplate of the Row since the cells are not created before that.
    }

    protected override void OnKeyDown( KeyEventArgs e )
    {
      base.OnKeyDown( e );

      if( e.Handled )
        return;

      //Only perform the BringItemIntoView if the item focused is within the host panel! 
      if( ScrollViewerHelper.IsFocusInElement( this.ItemsHost as FrameworkElement ) )
      {
        var currentDataGridContext = this.CurrentContext;

        //ensure that the key pressed is a Keyboard Navigation key
        switch( e.Key )
        {
          //if any of the directional key
          case Key.Up:
          case Key.Down:
          case Key.Left:
          case Key.Right:
          case Key.Tab:
            // Those keys will be handled by Children of the DataGridControl 
            // if required
            break;


          case Key.Space:
            {
              var originalSource = e.OriginalSource as DependencyObject;
              if( originalSource == null )
                break;

              var container = DataGridControl.GetContainer( originalSource );
              if( container == null )
                break;

              var dataGridContext = DataGridControl.GetDataGridContext( container );
              if( ( dataGridContext == null ) || ( dataGridContext.DataGridControl != this ) )
                break;

              var row = container as Row;
              var newPosition = default( SelectionRangePoint );

              if( row != null )
              {
                if( row.IsBeingEdited )
                  break;

                var cell = Cell.FindFromChildOrSelf( this, originalSource );
                var item = dataGridContext.GetItemFromContainer( row );
                var itemIndex = DataGridVirtualizingPanel.GetItemIndex( row );
                var column = ( cell != null ) ? cell.ParentColumn : null;
                var columnIndex = ( column != null ) ? column.VisiblePosition : -1;

                newPosition = SelectionRangePoint.TryCreateRangePoint( dataGridContext, item, itemIndex, columnIndex );
              }
              else
              {
                break;
              }

              if( newPosition != null )
              {
                var oldPosition = SelectionRangePoint.TryCreateFromCurrent( DataGridControl.GetDataGridContext( container ) );

                m_selectionChangerManager.UpdateSelection( oldPosition, newPosition, true, false, SelectionManager.UpdateSelectionSource.SpaceDown );

                e.Handled = true;
              }
            }

            break;

          case Key.LWin:
          case Key.RWin:
          case Key.MediaNextTrack:
          case Key.MediaPlayPause:
          case Key.MediaPreviousTrack:
          case Key.MediaStop:
          case Key.System:
          case Key.F1:
          case Key.F2:  // Should be processed as DataGridCommands.BeginEdit
          case Key.F3:
          case Key.F4:
          case Key.F5:
          case Key.F6:
          case Key.F7:
          case Key.F8:
          case Key.F9:
          case Key.F10:
          case Key.F11:
          case Key.F12:
          case Key.F13:
          case Key.F14:
          case Key.F15:
          case Key.F16:
          case Key.F17:
          case Key.F18:
          case Key.F19:
          case Key.F20:
          case Key.F21:
          case Key.F22:
          case Key.F23:
          case Key.F24:
          case Key.BrowserBack:
          case Key.BrowserFavorites:
          case Key.BrowserForward:
          case Key.BrowserHome:
          case Key.BrowserRefresh:
          case Key.BrowserSearch:
          case Key.BrowserStop:
          case Key.CapsLock:
          case Key.NumLock:
          case Key.Scroll:
          case Key.SelectMedia:
          case Key.PrintScreen:
          case Key.Pause:
          case Key.Play:
          case Key.Apps:
          case Key.Sleep:
          case Key.VolumeDown:
          case Key.VolumeMute:
          case Key.VolumeUp:
          case Key.Zoom:
          case Key.LaunchApplication1:
          case Key.LaunchApplication2:
          case Key.LaunchMail:
          case Key.LeftAlt:
          case Key.RightAlt:
          case Key.RightCtrl:
          case Key.LeftCtrl:
          case Key.RightShift:
          case Key.LeftShift:
          case Key.Delete: // Should be processed by the ApplicationCommands.Delete
            //do not bring into view in those conditions
            break;

          //by default, if not a special key, then bring into view the item.
          default:
            DataGridItemsHost.BringIntoViewKeyboardFocusedElement();
            break;
        }
      }
    }

    protected override void OnPreviewMouseDown( MouseButtonEventArgs e )
    {
      if( m_mouseDownUpdateSelectionSource == null )
      {
        m_mouseDownUpdateSelectionSource = this.SelectionChangerManager.PushUpdateSelectionSource( SelectionManager.UpdateSelectionSource.MouseDown );
      }

      var currentColumn = this.CurrentColumn;

      m_mouseDownSelectionRangePoint = SelectionRangePoint.TryCreateFromCurrent( this.CurrentContext );

      base.OnPreviewMouseDown( e );

      if( e.Handled )
        return;

      var oldDataGridContext = this.CurrentContext;
      var oldRow = ( oldDataGridContext == null ) ? null : oldDataGridContext.CurrentRow;

      var source = e.OriginalSource as DependencyObject;
      if( source == null )
        return;

      var container = DataGridControl.GetContainer( source );
      if( container == null )
        return;

      var newDataGridContext = DataGridControl.GetDataGridContext( container );
      if( ( newDataGridContext == null ) || ( newDataGridContext.DataGridControl != this ) )
        return;

      var newRow = Row.FromContainer( container );

      this.OnPreviewMouseDown_DragToSelect( e );

      if( ( oldRow == null ) || ( !oldRow.IsBeingEdited ) || ( newRow == oldRow ) )
        return;

      var oldCurrentColumn = oldDataGridContext.CurrentColumn;

      bool mouseDownHandled;
      Row newFocusedRow;
      ColumnBase newFocusedColumn;

      try
      {
        // If the source is reset during the EndEdit, we move the focus temporarely on the ScrollViewer to prevent some issue
        // when the EndEdit cause a Reset (The ScrollViewer was trying to bring into view the old editing Cell).
        var focused = this.ForceFocus( this.ScrollViewer );
        if( !focused )
          return;

        using( this.InhibitSetFocus() )
        {
          oldRow.EndEdit();

          // If the source was reset during the EndEdit, Ensure to relayout right away.
          // So the mouse down will continue on the new layouted element.
          this.UpdateLayout();

          IInputElement visualHit = this.InputHitTest( e.GetPosition( this ) );
          mouseDownHandled = ( visualHit == null ) || ( !object.ReferenceEquals( visualHit, e.OriginalSource ) );
          newFocusedRow = newRow;
          newFocusedColumn = currentColumn;
        }
      }
      catch( DataGridException )
      {
        // We swallow exception if it occurs because of a validation error or Cell was read-only or any other GridException.
        mouseDownHandled = true;
        newFocusedRow = oldRow;
        newFocusedColumn = oldCurrentColumn;
      }

      if( mouseDownHandled )
      {
        if( m_mouseDownUpdateSelectionSource != null )
        {
          m_mouseDownUpdateSelectionSource.Dispose();
          m_mouseDownUpdateSelectionSource = null;
        }

        this.SetFocusHelper( newFocusedRow, newFocusedColumn, false, true );
        e.Handled = true;
      }
    }

    protected override void OnPreviewMouseUp( MouseButtonEventArgs e )
    {
      if( m_mouseDownUpdateSelectionSource != null )
      {
        m_mouseDownUpdateSelectionSource.Dispose();
        m_mouseDownUpdateSelectionSource = null;
      }

      base.OnPreviewMouseUp( e );

      if( e.ChangedButton != MouseButton.Left )
        return;

      var source = e.OriginalSource as DependencyObject;
      if( source == null )
        return;

      var container = DataGridControl.GetContainer( source );
      if( ( container == null ) || ( !container.IsKeyboardFocused && !container.IsKeyboardFocusWithin ) )
        return;

      var dataGridContext = DataGridControl.GetDataGridContext( container );
      if( ( dataGridContext == null ) || ( dataGridContext.DataGridControl != this ) )
        return;

      var row = container as Row;
      var newPosition = default( SelectionRangePoint );
      var rowIsBeingEdited = false;

      if( row != null )
      {
        var cell = Cell.FindFromChildOrSelf( this, source );
        if( ( cell != null ) && ( this.SelectionUnit == SelectionUnit.Cell ) && ( !cell.IsKeyboardFocused ) && ( !cell.IsKeyboardFocusWithin ) )
          return;

        var item = dataGridContext.GetItemFromContainer( row );
        var itemIndex = DataGridVirtualizingPanel.GetItemIndex( row );
        var column = ( cell != null ) ? cell.ParentColumn : null;
        var columnIndex = ( column != null ) ? column.VisiblePosition : -1;

        // If we have an error during the EndEdit that prevent the PreviewMouseDown from occuring  (since we put e.Handled = true if an exception occurs), 
        // the PreviewMouseUp will not occur, so it is safe to call UpdateSelection().
        newPosition = SelectionRangePoint.TryCreateRangePoint( dataGridContext, item, itemIndex, columnIndex );
        rowIsBeingEdited = row.IsBeingEdited;
      }
      else
      {
        return;
      }

      if( newPosition != null )
      {
        m_selectionChangerManager.UpdateSelection( m_mouseDownSelectionRangePoint, newPosition, true, rowIsBeingEdited, SelectionManager.UpdateSelectionSource.MouseUp );
      }
    }

    protected override void OnMouseLeave( MouseEventArgs e )
    {
      if( m_mouseDownUpdateSelectionSource != null )
      {
        m_mouseDownUpdateSelectionSource.Dispose();
        m_mouseDownUpdateSelectionSource = null;
      }

      base.OnMouseLeave( e );
    }

    protected override void OnIsKeyboardFocusWithinChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnIsKeyboardFocusWithinChanged( e );

      KeyboardNavigation.SetIsTabStop( this, !( bool )e.NewValue );
    }

    protected override void OnPreviewGotKeyboardFocus( KeyboardFocusChangedEventArgs e )
    {
      base.OnPreviewGotKeyboardFocus( e );

      if( e.Handled || m_inhibitPreviewGotKeyboardFocus.IsSet )
        return;

      var newFocus = e.NewFocus as DependencyObject;

      if( this.NavigationBehavior == NavigationBehavior.None )
      {
        if( newFocus != this.ScrollViewer )
        {
          e.Handled = true;
          this.ForceFocus( this.ScrollViewer );
        }
        return;
      }

      if( ( newFocus == this ) || ( newFocus == this.ScrollViewer ) )
      {
        var currentDataGridContext = this.CurrentContext;
        var internalCurrentItem = currentDataGridContext.InternalCurrentItem;
        var itemUIElement = default( UIElement );

        if( ( internalCurrentItem == null ) && ( currentDataGridContext.Items.Count > 0 ) )
        {
          //if InternalCurrentItem is null, then soft-select the first item.
          internalCurrentItem = currentDataGridContext.Items.GetItemAt( 0 );

          if( internalCurrentItem != null )
          {
            using( this.InhibitSetFocus() )
            {
              currentDataGridContext.SetCurrent( internalCurrentItem, null, 0, currentDataGridContext.CurrentColumn, false, false, this.SynchronizeSelectionWithCurrent,
                                                 AutoScrollCurrentItemSourceTriggers.FocusChanged );
            }
          }
        }

        if( internalCurrentItem != null )
        {
          //try to fetch the Container for the Internal current item
          itemUIElement = currentDataGridContext.GetContainerFromItem( internalCurrentItem ) as UIElement;

          //  Special case when the previous element with the focus is now collapsed, the focus must be moved to the ScrollViewer, so the grid is kept focusable. 
          var focusScrollViewer = ( itemUIElement == null );
          if( !focusScrollViewer )
          {
            var headerFooterItem = itemUIElement as HeaderFooterItem;
            if( headerFooterItem != null )
            {
              focusScrollViewer = headerFooterItem.Visibility == Visibility.Collapsed;
              if( !focusScrollViewer )
              {
                var container = headerFooterItem.Container as UIElement;
                if( container != null )
                {
                  focusScrollViewer = container.Visibility == Visibility.Collapsed;
                }
              }
            }
          }

          //if container exists, then it's visible (or close to it), try to focus it 
          if( focusScrollViewer )
          {
            currentDataGridContext.DelayBringIntoViewAndFocusCurrent( AutoScrollCurrentItemSourceTriggers.FocusChanged );

            if( newFocus != this.ScrollViewer )
            {
              e.Handled = true;
              this.ForceFocus( this.ScrollViewer );
            }
          }
          else
          {
            e.Handled = true;
            this.SetFocusHelper( itemUIElement, currentDataGridContext.CurrentColumn, true, true );
          }
        }
        else if( newFocus != this.ScrollViewer )
        {
          e.Handled = true;
          this.ForceFocus( this.ScrollViewer );
        }

        return;
      }

      // If the focus was in a popup menu, we don't want to change the selection, so we consider the focus to be outside the grid in that situation.      
      var oldFocusWasInTheGrid = ( e.OldFocus != null ) && ( TreeHelper.IsDescendantOf( e.OldFocus as DependencyObject, this, false ) );
      var oldCurrentDataGridContext = this.CurrentContext;
      var oldRow = default( Row );
      var oldPosition = default( SelectionRangePoint );

      if( oldCurrentDataGridContext != null )
      {
        oldRow = oldCurrentDataGridContext.CurrentRow;
        oldPosition = SelectionRangePoint.TryCreateFromCurrent( oldCurrentDataGridContext );
        m_oldColumn = oldCurrentDataGridContext.CurrentCell != null ? new WeakReference( oldCurrentDataGridContext.CurrentCell.ParentColumn ) : m_oldColumn;
      }

      var newContainer = this.GetLocalContainer( newFocus );
      var newDataGridContext = ( newContainer == null ) ? null : DataGridControl.GetDataGridContext( newContainer );
      var newRow = Row.FromContainer( newContainer );
      var newCell = newFocus as Cell;

      if( ( newCell == null ) || ( newCell.ParentColumn.DataGridControl != this ) )
      {
        newCell = Cell.FindFromChild( this, newFocus );
      }

      var newColumn = ( newCell == null ) ? null : newCell.ParentColumn;
      var item = ( newContainer == null ) ? null : newDataGridContext.GetItemFromContainer( newContainer );
      var containerChangedDuringEndEdit = false;

      // Prevent the focus to be moved during the EndEdit since we are changing focus at the moment
      using( this.InhibitSetFocus() )
      {
        if( ( oldRow != null ) && ( oldRow.IsBeingEdited ) )
        {
          var oldCell = ( oldCurrentDataGridContext == null ) ? null : oldCurrentDataGridContext.CurrentCell;

          if( oldRow != newRow )
          {
            try
            {
              var previousNewContainer = newContainer;
              oldRow.EndEdit();

              // We must refetch the container for the current item in case EndEdit triggers a reset on the CustomItemContainerGenerator
              // and remap this item to a container other than the one previously fetched
              this.UpdateLayout();

              newContainer = ( item == null ) ? null : newDataGridContext.GetContainerFromItem( item ) as FrameworkElement;
              newRow = Row.FromContainer( newContainer );
              newCell = ( newRow == null ) ? null : newRow.Cells[ newColumn ];

              if( previousNewContainer != newContainer )
              {
                e.Handled = true;
                containerChangedDuringEndEdit = true;
              }
              else
              {
                // If the newFocus is not in the grid anymore, stop the focus.
                if( DataGridControl.GetDataGridContext( newFocus ) == null )
                {
                  e.Handled = true;
                  newFocus = null;
                }
              }
            }
            catch( DataGridException )
            {
              e.Handled = true;
              return;
            }
          }
          else if( ( oldCell != null ) && ( oldCell != newCell ) )
          {
            try
            {
              oldCell.EndEdit();
            }
            catch( DataGridException )
            {
              e.Handled = true;
              return;
            }
          }
        }
      }

      if( item == null )
        return;

      if( newCell != null && newRow != null )
      {
        IDisposable inhibitSetFocus = null;

        if( !containerChangedDuringEndEdit )
        {
          // Prevent the focus to be moved during the EndEdit since we are changing focus at the moment
          inhibitSetFocus = this.InhibitSetFocus();
        }

        try
        {
          var oldFocusedCell = e.OldFocus as Cell;
          var newFocusedElement = e.NewFocus as UIElement;

          // here we check if the oldFocus is the parent of the new focused element because the OnPreviewGotKeyboardFocus
          // is called twice if it's the case which give twice the focus and then endsup unselecting the element during the mouseUp 
          // because during the second updateSelection, the element is already contained then m_updateSelectionOnNextMouseUp set to true
          var isNewFocusParentTheOldFocusedCell = ( ( oldFocusedCell != null ) && ( newFocusedElement != null ) ) ?
                                                    ( oldFocusedCell != Cell.FindFromChild( this, newFocusedElement ) ) : true;
          var sourceDataItemIndex = DataGridVirtualizingPanel.GetItemIndex( newRow );

          // newCell != newFocus is in fact a way to test if newFocus is a child of the cell.
          if( ( newFocus != null ) && ( newCell != newFocus ) && ( !newCell.IsBeingEdited ) && ( newCell.IsCellEditorDisplayed ) )
          {
            newCell.BeginEdit();
          }
          else if( !newCell.IsCurrent )
          {
            //set the current row/column as this cell's
            newDataGridContext.SetCurrent( item, newRow, sourceDataItemIndex, newColumn, false, true, false, AutoScrollCurrentItemSourceTriggers.FocusChanged );
          }

          if( isNewFocusParentTheOldFocusedCell )
          {
            var rowIsBeingEditedAndCurrentRowNotChanged = this.IsRowBeingEditedAndCurrentRowNotChanged( newRow, oldRow );

            var newPosition = SelectionRangePoint.TryCreateRangePoint( newDataGridContext, item, sourceDataItemIndex, newCell.ParentColumn.VisiblePosition );
            m_selectionChangerManager.UpdateSelection( oldPosition, newPosition, oldFocusWasInTheGrid, rowIsBeingEditedAndCurrentRowNotChanged, null );
          }
        }
        catch( DataGridException )
        {
          e.Handled = true;
        }
        finally
        {
          if( inhibitSetFocus != null )
          {
            inhibitSetFocus.Dispose();
          }
        }

        return;
      }

      if( newRow != null )
      {
        if( newRow.IsCurrent )
          return;

        try
        {
          var currentColumn = default( ColumnBase );

          //  The current column may need to be propagated, unless the focus is being moved to a GroupHeaderControl
          // Special case for a row that is always set to NavigationBehavior.CellOnly (like FilterRow), as it needs a current column for the navigation to work propertly.
          if( ( ( this.NavigationBehavior == NavigationBehavior.CellOnly ) || ( newRow.NavigationBehavior == NavigationBehavior.CellOnly ) )
              && !( item is GroupHeaderFooterItem ) )
          {
            if( ( newDataGridContext != oldCurrentDataGridContext ) && this.AreDetailsFlatten )
            {
              currentColumn = newDataGridContext.GetMatchingColumn( oldCurrentDataGridContext, m_oldColumn.Target as ColumnBase );
            }

            if( currentColumn == null )
            {
              currentColumn = newDataGridContext.CurrentColumn;
            }

            //if there is no current column,
            if( currentColumn == null )
            {
              //Check what was the last currentColumn before navigating in Row mode, in the case it's navigating again towards a cell (as for the FilterRow for instance).
              var oldColumn = m_oldColumn.Target as ColumnBase;
              if( ( oldColumn != null ) && ( newDataGridContext == oldCurrentDataGridContext ) )
              {
                var fixedCellPanel = newRow.CellsHostPanel as FixedCellPanel;
                if( fixedCellPanel != null )
                {
                  var cell = default( Cell );
                  var virtualizingCellCollection = fixedCellPanel.ParentRowCells;
                  if( virtualizingCellCollection.TryGetBindedCell( oldColumn, out cell ) )
                  {
                    currentColumn = oldColumn;
                  }
                }
              }
            }

            //If there is no last currentColumn, or it is not available for navigation.
            if( currentColumn == null )
            {
              //Then use the first visible column
              var columns = newDataGridContext.VisibleColumns;
              if( columns.Count > 0 )
              {
                //First try to find the first navigable column that is visible in the viewport.
                var firstFocusableColumn = NavigationHelper.GetFirstVisibleFocusableInViewportColumnIndex( newDataGridContext, newRow );
                if( firstFocusableColumn == -1 )
                {
                  //If none is found, find the first navigable one.  This will make the grid scroll if one is found, since it's currently out-of-view.
                  firstFocusableColumn = NavigationHelper.GetFirstVisibleFocusableColumnIndex( newDataGridContext, newRow );
                }
                if( firstFocusableColumn < 0 )
                  throw new DataGridException( "Trying to edit while no cell is focusable. ", this );

                currentColumn = columns[ firstFocusableColumn ];
              }
            }
          }

          int sourceDataItemIndex = DataGridVirtualizingPanel.GetItemIndex( newRow );

          if( currentColumn == null )
          {
            IDisposable inhibitSetFocus = null;

            if( !containerChangedDuringEndEdit )
            {
              inhibitSetFocus = this.InhibitSetFocus();
            }

            try
            {
              newDataGridContext.SetCurrent( item, newRow, sourceDataItemIndex, null, true, true, false, AutoScrollCurrentItemSourceTriggers.FocusChanged );
            }
            finally
            {
              if( inhibitSetFocus != null )
              {
                inhibitSetFocus.Dispose();
              }
            }
          }
          else
          {
            e.Handled = true; // We prevent the focus to go on the row since we will move it on a cell
            newDataGridContext.SetCurrent( item, newRow, sourceDataItemIndex, currentColumn, true, true, false, AutoScrollCurrentItemSourceTriggers.FocusChanged );
          }

          var columnIndex = ( currentColumn == null ) ? -1 : currentColumn.VisiblePosition;
          var rowIsBeingEditedAndCurrentRowNotChanged = this.IsRowBeingEditedAndCurrentRowNotChanged( newRow, oldRow );

          var newPosition = SelectionRangePoint.TryCreateRangePoint( newDataGridContext, item, sourceDataItemIndex, columnIndex );
          m_selectionChangerManager.UpdateSelection( oldPosition, newPosition, oldFocusWasInTheGrid, rowIsBeingEditedAndCurrentRowNotChanged, null );
        }
        catch( DataGridException )
        {
          e.Handled = true;
        }

        return;
      }

      if( newContainer is HeaderFooterItem )
      {
        var headerFooterItem = ( HeaderFooterItem )newContainer;

        var inhibitSetFocus = default( IDisposable );

        if( !containerChangedDuringEndEdit )
        {
          inhibitSetFocus = this.InhibitSetFocus();
        }

        try
        {
          SelectionRangePoint newPosition;

          var currentColumn = newDataGridContext.CurrentColumn;
          newDataGridContext.SetCurrent( item, null, -1, currentColumn, true, true, false, AutoScrollCurrentItemSourceTriggers.FocusChanged );

          var columnIndex = ( currentColumn == null ) ? -1 : currentColumn.VisiblePosition;

          newPosition = SelectionRangePoint.TryCreateRangePoint( newDataGridContext, item, -1, columnIndex );

          m_selectionChangerManager.UpdateSelection( oldPosition, newPosition, oldFocusWasInTheGrid, false, null );
        }
        catch( DataGridException )
        {
          e.Handled = true;
          return;
        }
        finally
        {
          if( inhibitSetFocus != null )
          {
            inhibitSetFocus.Dispose();
          }
        }
      }
    }

    protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      base.OnMouseLeftButtonDown( e );

      if( !e.Handled )
      {
        e.Handled = true;
        this.Focus();
      }
    }

    protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
    {
      base.OnMouseLeftButtonUp( e );

      if( e.Handled )
        return;

      this.OnMouseLeftButtonUp_DragToSelect( e );
    }

    protected override void OnLostMouseCapture( MouseEventArgs e )
    {
      base.OnLostMouseCapture( e );

      this.OnLostMouseCapture_DragToSelect( e );
    }

    protected override void OnMouseMove( MouseEventArgs e )
    {
      base.OnMouseMove( e );

      if( e.Handled )
        return;

      this.OnMouseMove_DragToSelect( e );
    }


    protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnPropertyChanged( e );
      this.OnPropertyChanged( e.Property.Name );
    }

    internal static object GetFixedItemFromContainer( DependencyObject container )
    {
      object fixedItem = DataGridControl.GetFixedItem( container );
      if( fixedItem != DataGridControl.NotSet )
      {
        return fixedItem;
      }
      return null;
    }

    internal bool ForceFocus( UIElement element )
    {
      if( element == null )
        return false;

      using( m_inhibitPreviewGotKeyboardFocus.Set() )
      {
        return element.Focus();
      }
    }

    internal DependencyObject GetContainerForFixedItem( object item )
    {
      Panel fixedHeaderPanel = this.FixedHeadersHostPanel;
      if( fixedHeaderPanel != null )
      {
        foreach( DependencyObject headerContainer in fixedHeaderPanel.Children )
        {
          object containerItem = DataGridControl.GetFixedItem( headerContainer );
          if( containerItem == item )
          {
            return headerContainer;
          }
        }
      }

      Panel fixedFooterPanel = this.FixedFootersHostPanel;
      if( fixedFooterPanel != null )
      {
        foreach( DependencyObject footerContainer in fixedFooterPanel.Children )
        {
          object containerItem = DataGridControl.GetFixedItem( footerContainer );
          if( containerItem == item )
          {
            return footerContainer;
          }
        }
      }

      return null;
    }

    internal void PrepareItemContainer( DependencyObject container, object item )
    {
      //this function is called by the CustomItemContainerGenerator to prepare both the ActualDataItems and the custom items (e.g., Headers and Footers)
      DataGridControl.SetContainer( container, container );
      DataGridControl.SetIsContainerPrepared( container, true );
      this.PrepareContainerForItemOverride( container, item );
    }

    internal void ClearItemContainer( DependencyObject container, object item )
    {
      // For performance reason, it is useless to clear the ContainerProperty. The value is set in DataGridControl.PrepareItemContainer and it contains the 
      // container itself.  Clearing it here to set it back later is pointless.  Furthermore, setting and clearing the property cost more than usual since the
      // DependencyProperty is inherited.  To determine if the container is in use, the DataGridControl.IsContainerPrepared attached property should be use instead.
      //DataGridControl.ClearContainer( container );
      DataGridControl.ClearIsContainerPrepared( container );
      this.ClearContainerForItemOverride( container, item );
    }

    internal DependencyObject CreateContainerForItem()
    {
      DependencyObject container = this.GetContainerForItemOverride();

      if( container != null )
      {

        // Set a local value for containers to ensure the DefaultStyleKey is correctly applied on this container
        container.SetValue( DataGridControl.ParentDataGridControlPropertyKey, this );
      }

      return container;
    }

    internal bool IsItemItsOwnContainer( object dataItem )
    {
      return this.IsItemItsOwnContainerOverride( dataItem );
    }

    internal void ClearFixedHostPanels()
    {
      Panel fixedHeaderHostPanel = this.FixedHeadersHostPanel;
      if( fixedHeaderHostPanel != null )
      {
        DataGridControl.ClearFixedHostPanelHelper( this, fixedHeaderHostPanel.Children );
      }

      Panel fixedFooterHostPanel = this.FixedFootersHostPanel;
      if( fixedFooterHostPanel != null )
      {
        DataGridControl.ClearFixedHostPanelHelper( this, fixedFooterHostPanel.Children );
      }
    }

    internal void ResetFixedRegions()
    {
      if( m_fixedHeadersHostPanel != null )
      {
        DataGridControl.RefreshFixedHeaderFooter( this, m_fixedHeadersHostPanel, this.GetView().FixedHeaders );
      }

      if( m_fixedFootersHostPanel != null )
      {
        DataGridControl.RefreshFixedHeaderFooter( this, m_fixedFootersHostPanel, this.GetView().FixedFooters );
      }
    }

    internal int GetGlobalGeneratorIndexFromItem( DataGridContext dataGridContext, object item )
    {
      return this.CustomItemContainerGenerator.FindIndexForItem( item, dataGridContext );
    }

    internal void ShowWaitCursor()
    {
      if( m_waitCursorCount == 0 )
      {
        Cursor oldCursor = this.ReadLocalValue( FrameworkElement.CursorProperty ) as Cursor;
        object oldForceCursor = this.ReadLocalValue( FrameworkElement.ForceCursorProperty );

        if( oldCursor != DependencyProperty.UnsetValue )
          m_oldCursor = oldCursor;

        if( oldForceCursor != DependencyProperty.UnsetValue )
          m_oldForceCursor = ( bool )oldForceCursor;
      }

      UIViewBase uiViewBase = this.GetView() as UIViewBase;

      this.ForceCursor = true;

      this.Cursor = ( uiViewBase != null )
        ? uiViewBase.BusyCursor
        : Cursors.Wait;

      m_waitCursorCount++;
    }

    internal void HideWaitCursor()
    {
      m_waitCursorCount--;
      Debug.Assert( m_waitCursorCount >= 0 );

      if( m_waitCursorCount == 0 )
      {
        if( m_oldCursor != null )
        {
          this.Cursor = m_oldCursor;
          m_oldCursor = null;
        }
        else
        {
          this.ClearValue( FrameworkElement.CursorProperty );
        }

        if( m_oldForceCursor != null )
        {
          this.ForceCursor = m_oldForceCursor.Value;
          m_oldForceCursor = null;
        }
        else
        {
          this.ClearValue( FrameworkElement.ForceCursorProperty );
        }
      }
    }

    internal void CleanUpAfterUnload()
    {
      m_customItemContainerGenerator.CleanupGenerator();
      this.SetValue( ParentDataGridControlPropertyKey, null );
    }

    internal void SynchronizeForeignKeyConfigurations()
    {
      ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurations( this.Columns,
                                                                     this.ItemsSource,
                                                                     m_itemsSourcePropertyDescriptions,
                                                                     this.AutoCreateForeignKeyConfigurations );
    }


    private static bool IsRemoveAllowedForDeleteCommand( IList list )
    {
      // If the list has a fixed sized or if it's read only, we won't allow remove.
      bool allowRemove = ( !list.IsFixedSize && !list.IsReadOnly );

      IBindingList bindingList = list as IBindingList;

      // Even if the list does not have a fixed size and is not read only,
      // we don't want to allow remove if the list implements IBindingList
      // and does not allow remove.
      if( ( allowRemove )
        && ( bindingList != null ) )
      {
        allowRemove = bindingList.AllowRemove;
      }

      // Finally, we won't support remove if the list does support change notifications
      // because we need the grid to be notified of the changes to reflect and adjust
      // the displayed items.
      if( allowRemove )
      {
        allowRemove = ItemsSourceHelper.IsSourceSupportingChangeNotification( list );
      }

      return allowRemove;
    }

    private static DataTemplate GetTemplateForItem( object item )
    {
      DataTemplate retval = item as DataTemplate;
      if( retval == null )
      {
        if( item.GetType() == typeof( GroupHeaderFooterItem ) )
        {
          GroupHeaderFooterItem groupHeaderFooterItem = ( GroupHeaderFooterItem )item;

          retval = groupHeaderFooterItem.Template as DataTemplate;
          if( retval == null )
          {
            GroupHeaderFooterItemTemplate headerFooterItemTemplate = groupHeaderFooterItem.Template as GroupHeaderFooterItemTemplate;
            if( headerFooterItemTemplate != null )
            {
              headerFooterItemTemplate.Seal();
              retval = headerFooterItemTemplate.Template;
            }
          }
        }
      }
      return retval;
    }

    private static void ClearFixedHostPanelHelper( DataGridControl dataGrid, UIElementCollection collection )
    {
      TableViewScrollViewer tableViewScrollViewer = dataGrid.ScrollViewer as TableViewScrollViewer;
      RowSelectorPane rowSelectorPane = ( tableViewScrollViewer != null ) ? tableViewScrollViewer.RowSelectorPane : null;

      foreach( DependencyObject headerFooterItem in collection )
      {
        if( rowSelectorPane != null )
        {
          rowSelectorPane.FreeRowSelector( headerFooterItem );
        }

        dataGrid.ClearItemContainer( headerFooterItem, GetFixedItem( headerFooterItem ) );
        DataGridControl.SetDataGridContext( headerFooterItem, null );
      }

      collection.Clear();

    }

    private static void RefreshFixedHeaderFooter( DataGridControl dataGrid, Panel targetPanel, ObservableCollection<DataTemplate> collection )
    {
      TableViewScrollViewer tableViewScrollViewer = dataGrid.ScrollViewer as TableViewScrollViewer;
      RowSelectorPane rowSelectorPane = ( tableViewScrollViewer != null ) ? tableViewScrollViewer.RowSelectorPane : null;

      foreach( HeaderFooterItem element in targetPanel.Children )
      {
        if( rowSelectorPane != null )
        {
          rowSelectorPane.FreeRowSelector( element );
        }

        dataGrid.ClearItemContainer( element, DataGridControl.GetFixedItem( element ) );
        DataGridControl.SetDataGridContext( element, null );
      }

      targetPanel.Children.Clear();

      var view = dataGrid.GetView();
      DataGridContext dataGridContext = dataGrid.DataGridContext;

      foreach( DataTemplate template in collection )
      {
        HeaderFooterItem control = new HeaderFooterItem();

        control.Content = dataGrid.DataContext;
        control.ContentTemplate = template;

        DataGridControl.SetDataGridContext( control, dataGridContext );
        dataGrid.PrepareItemContainer( control, template );

        DataGridControl.SetFixedItem( control, template );
        GroupLevelIndicatorPane.SetGroupLevel( control, -1 );

        targetPanel.Children.Add( control );
      }

    }

    private void ReapplyTemplate()
    {
      ControlTemplate template = this.Template;

      if( template != null )
      {
        this.ClearFixedHostPanels();

        bool localTemplate = ( this.ReadLocalValue( TemplateProperty ) != DependencyProperty.UnsetValue );
        this.Template = new ControlTemplate();

        if( localTemplate )
        {
          this.Template = template;
        }
        else
        {
          this.ClearValue( TemplateProperty );
        }

        m_scrollViewer = null;
      }
    }

    private bool IsRowBeingEditedAndCurrentRowNotChanged( Row newRow, Row oldRow )
    {
      bool rowIsBeingEditedAndCurrentRowNotChanged = newRow.IsBeingEdited;

      if( rowIsBeingEditedAndCurrentRowNotChanged )
      {
        rowIsBeingEditedAndCurrentRowNotChanged &= ( newRow == oldRow );
      }

      return rowIsBeingEditedAndCurrentRowNotChanged;
    }

    private FrameworkElement GetLocalContainer( DependencyObject container )
    {
      if( container == null )
        return null;

      var candidate = DataGridControl.GetContainer( container );
      while( candidate != null )
      {
        if( ( candidate is IDataGridItemContainer ) && ( candidate is FrameworkElement ) )
        {
          var dataGridContext = DataGridControl.GetDataGridContext( candidate );
          if( dataGridContext == null )
            break;

          var dataGridControl = dataGridContext.DataGridControl;
          if( dataGridControl == this )
            return ( FrameworkElement )candidate;

          candidate = dataGridControl;
        }

        var parent = TreeHelper.GetParent( candidate );
        if( parent == null )
          break;

        candidate = DataGridControl.GetContainer( parent );
      }

      return null;
    }

    private Size GetStartUpSize( Size constraint )
    {
      double width = this.Width;
      double minWidth = this.MinWidth;
      double maxWidth = this.MaxWidth;

      double height = this.Height;
      double minHeight = this.MinHeight;
      double maxHeight = this.MaxHeight;

      if( double.IsNaN( width ) )
      {
        width = constraint.Width;
      }

      if( double.IsNaN( height ) )
      {
        height = constraint.Height;
      }

      if( !double.IsPositiveInfinity( maxWidth ) )
      {
        width = Math.Min( maxWidth, width );
      }

      if( !double.IsPositiveInfinity( maxHeight ) )
      {
        height = Math.Min( maxHeight, height );
      }

      if( ( width < minWidth ) || ( double.IsPositiveInfinity( width ) ) )
      {
        width = minWidth;
      }

      if( ( height < minHeight ) || ( double.IsPositiveInfinity( height ) ) )
      {
        height = minHeight;
      }

      return new Size( width, height );
    }

    private delegate void ParametrizedGenericHandler( object param );
    private delegate void DelayedSetFocusHelperHandler( bool forceFocus, bool preserveEditorFocus );
    private delegate bool GenericHandler();

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnNotifyPropertyChanged( PropertyChangedEventArgs e )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    private void OnPropertyChanged( string propertyName )
    {
      this.OnNotifyPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
    }

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( PropertyChangedEventManager ) )
      {
        var dataGridCollectionView = this.ItemsSource as DataGridCollectionViewBase;
        var eventArgs = e as PropertyChangedEventArgs;

        if( sender == dataGridCollectionView )
        {
          this.OnDataGridCollectionView_PropertyChanged( dataGridCollectionView, eventArgs );
        }
      }
      else if( managerType == typeof( CollectionChangedEventManager ) )
      {
        this.OnDataGridDetailDescriptionsChanged( sender as DataGridDetailDescriptionCollection, ( NotifyCollectionChangedEventArgs )e );
      }
      else if( managerType == typeof( ProxyCollectionRefreshEventManager ) )
      {
        this.OnDataGridCollectionView_ProxyCollectionRefresh( sender as DataGridCollectionView, e );
      }
      else if( managerType == typeof( ProxyGroupDescriptionsChangedEventManager ) )
      {
        this.OnDataGridCollectionView_ProxyGroupDescriptionsChanged( sender as DataGridCollectionView, ( NotifyCollectionChangedEventArgs )e );
      }
      else if( managerType == typeof( ProxySortDescriptionsChangedEventManager ) )
      {
        this.OnDataGridCollectionView_ProxySortDescriptionsChanged( sender as DataGridCollectionView, ( NotifyCollectionChangedEventArgs )e );
      }
      else if( managerType == typeof( ConnectionStateChangedEventManager ) )
      {
        this.OnDataGridVirtualizingCollectionViewBase_ConnectionStateChanged( sender as DataGridVirtualizingCollectionViewBase, e );
      }
      else if( managerType == typeof( ConnectionErrorChangedEventManager ) )
      {
        this.OnDataGridVirtualizingCollectionViewBase_ConnectionErrorChanged( sender as DataGridVirtualizingCollectionViewBase, e );
      }
      else if( managerType == typeof( FrameworkElementUnloadedEventManager ) )
      {
        this.UnhookToUnloaded();
        this.SaveDataGridContextState( this.DataGridContext, true, int.MaxValue );
        this.CleanUpAfterUnload();
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    #region IDocumentPaginatorSource Members

    DocumentPaginator IDocumentPaginatorSource.DocumentPaginator
    {
      get
      {
        return null;
      }
    }

    #endregion

    private IDisposable m_inhibitItemsCollectionChanged; // = null

    private Cursor m_oldCursor; // = null;
    private bool? m_oldForceCursor; // = null;
    private int m_waitCursorCount; // = 0;

    private FrameworkElement m_itemsHost; // = null
    private readonly CustomItemContainerGenerator m_customItemContainerGenerator; // = null

    private AutoScrollManager m_autoScrollManager;
    private bool m_startDragSelection;

    private object m_currentItemInEdition; // = null
    private RowState m_currentRowInEditionState; // = null

    private readonly AutoResetFlag m_inhibitBringIntoView = AutoResetFlagFactory.Create( false );
    private readonly AutoResetFlag m_inhibitSetFocus = AutoResetFlagFactory.Create( false );
    private readonly AutoResetFlag m_inhibitPreviewGotKeyboardFocus = AutoResetFlagFactory.Create( false );
    private readonly AutoScrollCurrentItemSourceTriggersRestrictions m_inhibitAutoScroll = new AutoScrollCurrentItemSourceTriggersRestrictions();
    private DispatcherOperation m_queueBringIntoView; // = null
    private DispatcherOperation m_queueSetFocus; // = null
    private DispatcherOperation m_queueClearCurrentColumn; // = null

    private Dictionary<WeakDataGridContextKey, SaveRestoreDataGridContextStateVisitor> m_dataGridContextsStateDictionary;
    private SelectionManager m_selectionChangerManager;

    private CommandBinding m_copyCommandBinding; // = null;
    private CommandBinding m_deleteCommandBinding; // = null;
    private CommandBinding m_refreshCommandBinding; // = null;
    private IDisposable m_mouseDownUpdateSelectionSource;

    private SelectionRangePoint m_mouseDownSelectionRangePoint;

    private bool m_isFirstTimeLoaded = true;

    private WeakReference m_parentWindow;
    private WeakReference m_oldColumn = new WeakReference( null );

    private BitVector32 m_flags = new BitVector32();

    [Flags]
    private enum DataGridControlFlags
    {
      SettingFocusOnCell = 1,
      IsSetCurrentInProgress = 2,
      IsBoundToDataGridVirtualizingCollectionViewBase = 8,
      ViewPreservesContainerSize = 16,
      DebugSaveRestore = 32,
      ItemsSourceChangedDelayed = 64,
      SelectedIndexPropertyNeedCoerce = 128,
      SelectedItemPropertyNeedCoerce = 256
    }
  }
}
