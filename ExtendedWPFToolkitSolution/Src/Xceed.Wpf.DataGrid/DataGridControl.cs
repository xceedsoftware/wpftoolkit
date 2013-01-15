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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Wpf.DataGrid.Views;
using Xceed.Wpf.DataGrid.Print;
using Xceed.Utils.Wpf;
using Xceed.Utils.Collections;
using System.Security;
using System.Security.Permissions;
using System.Windows.Automation.Peers;
using Xceed.Wpf.DataGrid.Automation;
using Xceed.Wpf.DataGrid.Settings;
using Xceed.Wpf.DataGrid.Export;
using Xceed.Utils.Wpf.DragDrop;
using System.Windows.Xps.Packaging;
using System.IO;
using System.IO.Packaging;
using System.Windows.Xps;
using System.Windows.Markup;
using System.Printing;
using System.Windows.Media.Imaging;
using Xceed.Wpf.DataGrid.Utils;

namespace Xceed.Wpf.DataGrid
{
  [TemplatePart( Name = "PART_ScrollViewer", Type = typeof( ScrollViewer ) )]
  [StyleTypedProperty( Property = "ItemContainerStyle", StyleTargetType = typeof( DataRow ) )] // Blend has to be able to create an instance of this for the style edition so it can't be typeof( Row )
  [StyleTypedProperty( Property = "CellErrorStyle", StyleTargetType = typeof( Cell ) )]
  public partial class DataGridControl : ItemsControl, INotifyPropertyChanged, IDocumentPaginatorSource, IWeakEventListener
  {
    static DataGridControl()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( DataGridControl ), new FrameworkPropertyMetadata( TableflowView.GetDefaultStyleKey( typeof( TableflowView ), typeof( DataGridControl ) ) ) );

      ItemsControl.ItemsSourceProperty.OverrideMetadata( typeof( DataGridControl ), new FrameworkPropertyMetadata( null,
        new CoerceValueCallback( DataGridControl.ItemsSourceCoerceCallback ) ) );

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
      DataGridControl.DetailConfigurationsProperty = DataGridControl.DetailConfigurationsPropertyKey.DependencyProperty;
      DataGridControl.HasExpandedDetailsProperty = DataGridControl.HasExpandedDetailsPropertyKey.DependencyProperty;
    }

    private static object ItemsSourceCoerceCallback( DependencyObject sender, object value )
    {
      DataGridControl dataGridControl = sender as DataGridControl;

      // Ensure to EndEdit the currently edited container
      object currentItem = dataGridControl.CurrentContext.InternalCurrentItem;

      if( currentItem != null )
      {
        // Use Row.FromContainer to ensure Rows in Header/Footers are returned correctly
        Row row = Row.FromContainer( dataGridControl.CurrentContext.GetContainerFromItem( currentItem ) );

        if( ( row != null ) && ( row.IsBeingEdited ) )
          row.CancelEdit();
      }

      return value;
    }

    public DataGridControl()
    {
      m_selectionChangerManager = new SelectionManager( this );

      this.SetValue( DataGridControl.ParentDataGridControlPropertyKey, this );

      //set the FixedItem for the gridControl to NotSet, this is to prevent problems with nested DataGridControls
      DataGridControl.SetFixedItem( this, DataGridControl.NotSet );

      this.SetDetailConfigurations( new DetailConfigurationCollection( this, null ) );

      this.DetailConfigurations.CollectionChanged += this.OnDetailConfigurationsChanged;

      this.CommandBindings.Add( new CommandBinding( DataGridCommands.ExpandGroup, this.OnExpandGroupExecuted, this.OnExpandGroupCanExecute ) );

      this.CommandBindings.Add( new CommandBinding( DataGridCommands.CollapseGroup, this.OnCollapseGroupExecuted, this.OnCollapseGroupCanExecute ) );

      this.CommandBindings.Add( new CommandBinding( DataGridCommands.ToggleGroupExpansion, this.OnToggleGroupExecuted, this.OnToggleGroupCanExecute ) );

      this.CommandBindings.Add( new CommandBinding( ApplicationCommands.SelectAll, this.OnSelectAllExecuted, this.OnSelectAllCanExecute ) );

      this.CommandBindings.Add( new CommandBinding( DataGridCommands.ClearFilter, this.OnClearFilterExecuted ) );

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
      dataGridContext.SetIsCurrentHelper( true );
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
        // TODO (case 117288): Remove when the issue will be corrected.
        this.ClipToBounds = true;
      }

      this.Loaded += new RoutedEventHandler( DataGridControl_Loaded );
      this.LayoutUpdated += new EventHandler( DataGridControl_LayoutUpdated );
    }

    #region INITIALIZATION

    public override void BeginInit()
    {
      m_beginInitCount++;

      if( m_columnsInitDisposable == null )
      {
        m_columnsInitDisposable = this.Columns.DeferColumnAdditionMessages();
      }

      // The FrameworkElement implementation does not support nested BeginInit/EndInit
      if( m_beginInitCount == 1 )
        base.BeginInit();
    }

    public override void EndInit()
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

        // Reassociate the default view to the grid at the last moment so that 
        // eventual implicit style declarations for the default View in user applications 
        // will work.
        if( this.View == null )
        {
          this.OnViewChanged( null, null );
        }


        if( this.ItemsSourceChangedDelayed )
        {
          this.ItemsSourceChangedDelayed = false;
          this.ProcessDelayedItemsSourceChanged();
        }

        //see if any of the GroupLevelDescriptions are matched to a column but aren't bound ( case 102437 ) and
        //update then (bind them) if necessary
        this.DataGridContext.SetupTitleBindingForGroupLevelDescriptions();

        // The FrameworkElement implementation does not support nested BeginInit/EndInit
        base.EndInit();
      }
    }

    private void DataGridControl_LayoutUpdated( object sender, EventArgs e )
    {
      if( m_dataGridContextToRefreshPeerChlidren == null )
        return;

      foreach( DataGridContext dataGridContext in m_dataGridContextToRefreshPeerChlidren )
      {
        DataGridContextAutomationPeer dataGridContextPeer = dataGridContext.Peer;

        if( dataGridContextPeer == null )
          continue;

        dataGridContextPeer.ResetChildrenCache();
      }

      m_dataGridContextToRefreshPeerChlidren = null;
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
      if( ( this.DeferInitialLayoutPass ) && ( m_isFirstTimeLoaded ) && ( !this.IsPrinting ) )
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
      FrameworkElement parentWindow = ( m_parentWindow == null )
        ? this : m_parentWindow.Target as FrameworkElement;

      FrameworkElementUnloadedEventManager.RemoveListener( parentWindow, this );
    }

    #endregion INITIALIZATION

    #region ParentDataGridControl Attached Property

    internal static readonly DependencyPropertyKey ParentDataGridControlPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "ParentDataGridControl", typeof( DataGridControl ), typeof( DataGridControl ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static readonly DependencyProperty ParentDataGridControlProperty;

    [Obsolete( "GetParentDataGridControl is obsolete. Use DataGridContext.DataGridControl instead" )]
    public static DataGridControl GetParentDataGridControl( DependencyObject obj )
    {
      return ( DataGridControl )obj.GetValue( DataGridControl.ParentDataGridControlProperty );
    }

    #endregion ParentDataGridControl Attached Property

    #region DataGridContext Attached Property

    internal static readonly DependencyPropertyKey DataGridContextPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "DataGridContext", typeof( DataGridContext ), typeof( DataGridControl ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static readonly DependencyProperty DataGridContextProperty;

    public static DataGridContext GetDataGridContext( DependencyObject obj )
    {
      if( obj == null )
        throw new ArgumentNullException( "obj" );

      return ( DataGridContext )obj.GetValue( DataGridControl.DataGridContextProperty );
    }

    internal static void SetDataGridContext( DependencyObject obj, DataGridContext value )
    {
      obj.SetValue( DataGridControl.DataGridContextPropertyKey, value );
    }

    private DataGridContext m_localDataGridContext; // = null

    #endregion DataGridContext Attached Property

    #region Container Attached Property

    private static readonly DependencyPropertyKey ContainerPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "Container", typeof( FrameworkElement ), typeof( DataGridControl ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static readonly DependencyProperty ContainerProperty = DataGridControl.ContainerPropertyKey.DependencyProperty;

    public static FrameworkElement GetContainer( DependencyObject obj )
    {
      return ( FrameworkElement )obj.GetValue( DataGridControl.ContainerProperty );
    }

    internal static void SetContainer( DependencyObject obj, object value )
    {
      Debug.Assert( ( value == null ) || ( value is IDataGridItemContainer ) );
      obj.SetValue( DataGridControl.ContainerPropertyKey, value );
    }

    internal static void ClearContainer( DependencyObject obj )
    {
      obj.ClearValue( DataGridControl.ContainerPropertyKey );
    }

    #endregion Container Attached Property

    #region StatContext Attached Property

    internal static readonly DependencyPropertyKey StatContextPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "StatContext", typeof( object ), typeof( DataGridControl ),
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

    #endregion StatContext Attached Property

    #region HasDetails Read-Only Property

    public bool HasDetails
    {
      get
      {
        return ( this.DetailConfigurations.Count > 0 );
      }
    }

    #endregion

    #region HasExpandedDetails Attached Property

    internal static readonly DependencyPropertyKey HasExpandedDetailsPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "HasExpandedDetails", typeof( bool ), typeof( DataGridControl ),
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

    #endregion HasDetails Attached Property

    #region Columns Read-Only Property

    private static readonly DependencyPropertyKey ColumnsPropertyKey =
        DependencyProperty.RegisterReadOnly( "Columns", typeof( ColumnCollection ), typeof( DataGridControl ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty ColumnsProperty;

    public ColumnCollection Columns
    {
      get
      {
        return ( ColumnCollection )this.GetValue( DataGridControl.ColumnsProperty );
      }
    }

    #endregion Columns Read-Only Property

    #region VisibleColumns Read-Only Property

    private static readonly DependencyPropertyKey VisibleColumnsPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "VisibleColumns", typeof( ReadOnlyObservableCollection<ColumnBase> ), typeof( DataGridControl ),
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

    #endregion VisibleColumns Read-Only Property

    #region ColumnsByVisiblePosition Read-Only Property

    internal HashedLinkedList<ColumnBase> ColumnsByVisiblePosition
    {
      get
      {
        return this.DataGridContext.ColumnsByVisiblePosition;
      }
    }

    #endregion ColumnsByVisiblePosition Read-Only Property

    #region CellEditorDisplayConditions Property

    public static readonly DependencyProperty CellEditorDisplayConditionsProperty = DependencyProperty.RegisterAttached(
      "CellEditorDisplayConditions", typeof( CellEditorDisplayConditions ), typeof( DataGridControl ),
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

    #endregion CellEditorDisplayConditions Property

    #region DefaultCellEditors Property

    public Dictionary<Type, CellEditor> DefaultCellEditors
    {
      get
      {
        return m_defaultCellEditors;
      }
    }

    private Dictionary<Type, CellEditor> m_defaultCellEditors = new Dictionary<Type, CellEditor>();

    #endregion DefaultCellEditors Property

    #region CellErrorStyle Property

    public static readonly DependencyProperty CellErrorStyleProperty = DependencyProperty.RegisterAttached(
      "CellErrorStyle", typeof( Style ), typeof( DataGridControl ),
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

    #endregion CellErrorStyle Property

    #region HasValidationError Property

    private static readonly DependencyPropertyKey HasValidationErrorPropertyKey =
        DependencyProperty.RegisterReadOnly( "HasValidationError", typeof( bool ), typeof( DataGridControl ), new UIPropertyMetadata( false ) );

    public static readonly DependencyProperty HasValidationErrorProperty =
      HasValidationErrorPropertyKey.DependencyProperty;

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

    #endregion HasValidationError Property

    #region ValidationMode Property

    [Obsolete( "The ValidationMode is obsolete. Refer to the Editing and Validating Data topic in the documentation.", true )]
    public static readonly DependencyProperty ValidationModeProperty =
        DependencyProperty.Register( "ValidationMode", typeof( ValidationMode ), typeof( DataGridControl ), new UIPropertyMetadata( ValidationMode.RowEndingEdit ) );

    [Obsolete( "The ValidationMode is obsolete. Refer to the Editing and Validating Data topic in the documentation.", true )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public ValidationMode ValidationMode
    {
      get
      {
        return ( ValidationMode )this.GetValue( DataGridControl.ValidationModeProperty );
      }

      set
      {
        this.SetValue( DataGridControl.ValidationModeProperty, value );
      }
    }

    #endregion ValidationMode Property

    #region SelectedItems Read-Only Property

    private static readonly DependencyPropertyKey SelectedItemsPropertyKey =
        DependencyProperty.RegisterReadOnly( "SelectedItems", typeof( IList ), typeof( DataGridControl ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty SelectedItemsProperty;

    public IList SelectedItems
    {
      get
      {
        return ( IList )this.GetValue( DataGridControl.SelectedItemsProperty );
      }
    }

    #endregion SelectedItems Read-Only Property

    #region SelectedRanges Read-Only Property

    private static readonly DependencyPropertyKey SelectedItemRangesPropertyKey =
        DependencyProperty.RegisterReadOnly( "SelectedItemRanges", typeof( IList<SelectionRange> ), typeof( DataGridControl ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty SelectedItemRangesProperty;

    public IList<SelectionRange> SelectedItemRanges
    {
      get
      {
        return ( IList<SelectionRange> )this.GetValue( DataGridControl.SelectedItemRangesProperty );
      }
    }

    #endregion SelectedItemRanges Read-Only Property

    #region SelectedCellRanges Read-Only Property

    private static readonly DependencyPropertyKey SelectedCellRangesPropertyKey =
        DependencyProperty.RegisterReadOnly( "SelectedCellRanges", typeof( IList<SelectionCellRange> ), typeof( DataGridControl ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty SelectedCellRangesProperty;

    public IList<SelectionCellRange> SelectedCellRanges
    {
      get
      {
        return ( IList<SelectionCellRange> )this.GetValue( DataGridControl.SelectedCellRangesProperty );
      }
    }

    #endregion SelectedCellRanges Read-Only Property

    #region SelectedItem Property

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register( "SelectedItem", typeof( object ), typeof( DataGridControl ), new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback( OnSelectedItemChanged ), new CoerceValueCallback( OnCoerceSelectedItem ) ) );

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
      DataGridControl dataGridControl = ( DataGridControl )sender;

      if( ( value != null ) && ( !dataGridControl.m_skipCoerceSelectedItemCheck ) )
      {
        if( !dataGridControl.Items.Contains( value ) )
        {
          dataGridControl.SelectedItemPropertyNeedCoerce = true;
          return DependencyProperty.UnsetValue;
        }
      }

      dataGridControl.SelectedItemPropertyNeedCoerce = false;
      return value;
    }

    private static void OnSelectedItemChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl dataGrid = ( DataGridControl )sender;

      if( dataGrid != null )
      {
        if( !dataGrid.m_selectionChangerManager.IsActive )
        {
          dataGrid.m_selectionChangerManager.Begin();

          try
          {
            if( e.NewValue == null )
            {
              dataGrid.m_selectionChangerManager.UnselectAll();
            }
            else
            {
              dataGrid.m_selectionChangerManager.SelectJustThisItem(
                dataGrid.DataGridContext, dataGrid.Items.IndexOf( e.NewValue ), e.NewValue );
            }
          }
          finally
          {
            dataGrid.m_selectionChangerManager.End( false, false, true );
          }
        }
      }
    }

    private bool m_skipCoerceSelectedItemCheck;

    #endregion SelectedItem Property

    #region SelectedIndex Property

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register( "SelectedIndex", typeof( int ), typeof( DataGridControl ), new FrameworkPropertyMetadata( -1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback( OnSelectedIndexChanged ), new CoerceValueCallback( OnCoerceSelectedIndex ) ) );

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
      DataGridControl dataGrid = sender as DataGridControl;

      int newValue = ( int )value;

      if( ( newValue < -1 ) || ( newValue >= dataGrid.Items.Count ) )
      {
        dataGrid.SelectedIndexPropertyNeedCoerce = true;
        return DependencyProperty.UnsetValue;
      }

      dataGrid.SelectedIndexPropertyNeedCoerce = false;
      return value;
    }

    private static void OnSelectedIndexChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl dataGrid = sender as DataGridControl;

      if( dataGrid != null )
      {
        int newValue = ( int )e.NewValue;

        if( !dataGrid.m_selectionChangerManager.IsActive )
        {
          dataGrid.m_selectionChangerManager.Begin();

          try
          {
            if( ( newValue >= 0 ) && ( newValue < dataGrid.Items.Count ) )
            {
              //affect the item as the new selection
              dataGrid.m_selectionChangerManager.SelectJustThisItem(
                dataGrid.DataGridContext, newValue, dataGrid.Items[ newValue ] );
            }
            else
            {
              //clear the selection.
              dataGrid.m_selectionChangerManager.UnselectAll();
            }
          }
          finally
          {
            dataGrid.m_selectionChangerManager.End( false, false, true );
          }
        }
      }
    }

    #endregion SelectedIndex Property

    #region GlobalSelectedItems Read-Only Property

    public IEnumerable GlobalSelectedItems
    {
      get
      {
        foreach( DataGridContext context in this.SelectedContexts )
        {
          foreach( object selectedItem in context.SelectedItems )
          {
            yield return selectedItem;
          }
        }
      }
    }

    internal void NotifyGlobalSelectedItemsChanged()
    {
      this.OnNotifyPropertyChanged( new PropertyChangedEventArgs( "GlobalSelectedItems" ) );
    }

    #endregion

    #region SelectedContexts Read-Only Property

    public ReadOnlyCollection<DataGridContext> SelectedContexts
    {
      get
      {
        if( m_readOnlySelectedContexts == null )
          m_readOnlySelectedContexts = new ReadOnlyCollection<DataGridContext>( m_selectedContexts );

        return m_readOnlySelectedContexts;
      }
    }

    private List<DataGridContext> m_selectedContexts = new List<DataGridContext>();
    private ReadOnlyCollection<DataGridContext> m_readOnlySelectedContexts;

    #endregion SelectedContexts Read-Only Property

    #region SelectionMode Property

    public static readonly DependencyProperty SelectionModeProperty =
        DependencyProperty.Register( "SelectionMode", typeof( SelectionMode ), typeof( DataGridControl ), new UIPropertyMetadata( SelectionMode.Extended ) );

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

    #endregion SelectionMode Property

    #region SelectionUnit Property

    public static readonly DependencyProperty SelectionUnitProperty =
        DependencyProperty.Register( "SelectionUnit", typeof( SelectionUnit ), typeof( DataGridControl ),
        new UIPropertyMetadata( SelectionUnit.Row,
          new PropertyChangedCallback( DataGridControl.OnSelectionUnitChanged ),
          new CoerceValueCallback( DataGridControl.SelectionUnitCoerceValueCallback ) ) );

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
      DataGridControl dataGrid = ( DataGridControl )sender;

      if( dataGrid != null )
      {
        SelectionUnit selectionUnit = ( SelectionUnit )e.NewValue;

        dataGrid.m_selectionChangerManager.Begin();

        try
        {
          switch( selectionUnit )
          {
            case SelectionUnit.Row:
              {
                dataGrid.m_selectionChangerManager.UnselectAllCells();
                break;
              }
            case SelectionUnit.Cell:
              {
                dataGrid.m_selectionChangerManager.UnselectAllItems();
                break;
              }
          }
        }
        finally
        {
          dataGrid.m_selectionChangerManager.End( false, false, true );
        }
      }
    }

    private static object SelectionUnitCoerceValueCallback( DependencyObject sender, object value )
    {
      return value;
    }

    #endregion SelectionUnit Property

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

        this.DataGridContext.SetCurrent( value, null, null, this.DataGridContext.CurrentColumn, false, true, this.SynchronizeSelectionWithCurrent );
      }
    }

    internal bool ShouldSynchronizeCurrentItem
    {
      get
      {
        return ( !this.IsPrinting ) && ( this.SynchronizeCurrent );
      }
    }

    #endregion CurrentItem Property

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

        this.DataGridContext.SetCurrent( this.DataGridContext.InternalCurrentItem, null, null, value, false, true, this.SynchronizeSelectionWithCurrent );
      }
    }

    #endregion CurrentColumn Property

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

      this.OnNotifyPropertyChanged( new PropertyChangedEventArgs( "CurrentContext" ) );
      this.OnNotifyPropertyChanged( new PropertyChangedEventArgs( "CurrentColumn" ) );
      this.OnNotifyPropertyChanged( new PropertyChangedEventArgs( "CurrentItem" ) );
      this.OnNotifyPropertyChanged( new PropertyChangedEventArgs( "GlobalCurrentItem" ) );
      this.OnNotifyPropertyChanged( new PropertyChangedEventArgs( "GlobalCurrentColumn" ) );
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

    private AdornerDecorator m_dragDropAdornerDecorator;

    internal AdornerDecorator DragDropAdornerDecorator
    {
      get
      {
        return m_dragDropAdornerDecorator;
      }
    }

    #endregion

    #region AutoCreateColumns Property

    public static readonly DependencyProperty AutoCreateColumnsProperty =
        DependencyProperty.Register( "AutoCreateColumns", typeof( bool ), typeof( DataGridControl ), new UIPropertyMetadata( true ) );

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

    #endregion AutoCreateColumns Property

    #region AutoCreateDetailConfigurations Property

    internal static readonly DependencyProperty AutoCreateDetailConfigurationsProperty =
        DependencyProperty.Register( "AutoCreateDetailConfigurations", typeof( bool ), typeof( DataGridControl ), new UIPropertyMetadata( false, null, new CoerceValueCallback( DataGridControl.AutoCreateDetailConfigurationsCoerceValueCallback ) ) );

    internal bool AutoCreateDetailConfigurations
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.AutoCreateDetailConfigurationsProperty );
      }
      set
      {
        this.SetValue( DataGridControl.AutoCreateDetailConfigurationsProperty, value );
      }
    }

    private static object AutoCreateDetailConfigurationsCoerceValueCallback( DependencyObject sender, object value )
    {
      return value;
    }

    #endregion AutoCreateColumns Property

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
      DataGridControl dataGridControl = sender as DataGridControl;

      if( ( dataGridControl != null ) && ( dataGridControl.AutoCreateForeignKeyConfigurations ) )
      {
        dataGridControl.SynchronizeForeignKeyConfigurations();
      }
    }

    #endregion

    #region AutoRemoveColumnsAndDetailConfigurations Property

    public static readonly DependencyProperty AutoRemoveColumnsAndDetailConfigurationsProperty =
        DependencyProperty.Register( "AutoRemoveColumnsAndDetailConfigurations", typeof( bool ), typeof( DataGridControl ), new UIPropertyMetadata( true ) );

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

    #region HideSelection Property

    public static readonly DependencyProperty HideSelectionProperty =
        DependencyProperty.Register( "HideSelection", typeof( bool ), typeof( DataGridControl ), new UIPropertyMetadata( false ) );

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

    #endregion HideSelection Property

    #region ReadOnly Property

    public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.RegisterAttached(
      "ReadOnly", typeof( bool ), typeof( DataGridControl ),
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

    #endregion ReadOnly Property

    #region EditTriggers Property

    public static readonly DependencyProperty EditTriggersProperty = DependencyProperty.RegisterAttached(
      "EditTriggers", typeof( EditTriggers ), typeof( DataGridControl ),
      new FrameworkPropertyMetadata( EditTriggers.BeginEditCommand | EditTriggers.ClickOnCurrentCell | EditTriggers.ActivationGesture,
        FrameworkPropertyMetadataOptions.Inherits ) );

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

    #endregion EditTriggers Property

    #region ScrollViewer Property

    private ScrollViewer m_scrollViewer;

    internal ScrollViewer ScrollViewer
    {
      get
      {
        return m_scrollViewer;
      }
    }

    #endregion

    #region ItemsHost Property

    internal FrameworkElement ItemsHost
    {
      get
      {
        if( m_itemsHost == null )
        {
          ScrollViewer scrollViewer = this.ScrollViewer;
          if( scrollViewer == null )
            return null;

          //this can be either a ItemsPresenter (which implements IScrollInfo and reforwards it to the ItemsPanel instantiated from the
          //ItemsPanelTemplate or the DataGridItemsHost used for the Container Recycling.
          FrameworkElement scrollViewerContent = null;

          ItemsPresenter itemsPresenter = scrollViewer.Content as ItemsPresenter;
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

    [Obsolete( "The ItemsPanel property  is obsolete and has been replaced by the DataGridItemsHost (and derived) classes.", false )]
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

    #endregion ItemsPanel override

    #region FixedHeadersHostPanel Property

    private Panel m_fixedHeadersHostPanel;

    internal Panel FixedHeadersHostPanel
    {
      get
      {
        return m_fixedHeadersHostPanel;
      }
    }

    private void SetFixedHeadersHostPanel( Panel fixedHeadersHostPanel )
    {
      if( m_fixedHeadersHostPanel == fixedHeadersHostPanel )
        return;

      m_fixedHeadersHostPanel = fixedHeadersHostPanel;

      if( m_fixedHeadersHostPanel != null )
        DataGridControl.RefreshFixedHeaderFooter( this, m_fixedHeadersHostPanel, this.GetView().FixedHeaders );
    }

    #endregion FixedHeadersHostPanel Property

    #region FixedFootersHostPanel Property

    private Panel m_fixedFootersHostPanel;

    internal Panel FixedFootersHostPanel
    {
      get
      {
        return m_fixedFootersHostPanel;
      }
    }

    private void SetFixedFootersHostPanel( Panel fixedFootersHostPanel )
    {
      if( m_fixedFootersHostPanel == fixedFootersHostPanel )
        return;

      m_fixedFootersHostPanel = fixedFootersHostPanel;

      if( m_fixedFootersHostPanel != null )
        DataGridControl.RefreshFixedHeaderFooter( this, m_fixedFootersHostPanel, this.GetView().FixedFooters );
    }

    #endregion FixedFootersHostPanel Property

    #region IsFixedHeadersHost Attached Property

    public static readonly DependencyProperty IsFixedHeadersHostProperty =
        DependencyProperty.RegisterAttached( "IsFixedHeadersHost", typeof( bool ), typeof( DataGridControl ), new UIPropertyMetadata( false, new PropertyChangedCallback( OnIsFixedHeadersHost_Changed ) ) );

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

      Panel panel = sender as Panel;

      if( panel == null )
        return;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( sender );

      DataGridControl grid = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      if( grid != null )
      {
        grid.SetFixedHeadersHostPanel( panel );
      }
    }

    #endregion IsFixedHeadersHost Attached Property

    #region IsFixedFootersHost Attached Property

    public static readonly DependencyProperty IsFixedFootersHostProperty =
        DependencyProperty.RegisterAttached( "IsFixedFootersHost", typeof( bool ), typeof( DataGridControl ), new UIPropertyMetadata( false, new PropertyChangedCallback( OnIsFixedFootersHost_Changed ) ) );

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

      Panel panel = sender as Panel;

      if( panel == null )
        return;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( sender );

      DataGridControl grid = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      if( grid != null )
      {
        grid.SetFixedFootersHostPanel( panel );
      }
    }

    #endregion IsFixedFootersHost Attached Property

    #region ItemsPrimaryAxis Property

    public static readonly DependencyProperty ItemsPrimaryAxisProperty =
        DependencyProperty.Register( "ItemsPrimaryAxis", typeof( PrimaryAxis ), typeof( DataGridControl ), new UIPropertyMetadata( PrimaryAxis.Vertical ) );

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

    #endregion ItemsPrimaryAxis Property

    #region View Property

    public static readonly DependencyProperty ViewProperty =
            DependencyProperty.Register(
            "View",
            typeof( Xceed.Wpf.DataGrid.Views.UIViewBase ),
            typeof( DataGridControl ),
            new UIPropertyMetadata( null, new PropertyChangedCallback( DataGridControl.OnViewChanged ), new CoerceValueCallback( ViewCoerceValueCallback ) ) );

    [TypeConverter( typeof( Markup.ViewConverter ) )]
    public Xceed.Wpf.DataGrid.Views.UIViewBase View
    {
      get
      {
        return m_cachedView; 
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
      Xceed.Wpf.DataGrid.Views.UIViewBase view = value as Xceed.Wpf.DataGrid.Views.UIViewBase;

      if( view != null )
      {
        if( ( view.Parent != null ) && ( view.Parent != sender ) )
          throw new InvalidOperationException( "An attempt was made to associate a view with more than one grid." );
      }

      return value;
    }

    private static void OnViewChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl grid = ( DataGridControl )sender;

      //for case 1044618: leave edition mode (cancel) when view changes
      grid.CancelEdit();

      Xceed.Wpf.DataGrid.Views.ViewBase oldView = ( Xceed.Wpf.DataGrid.Views.ViewBase )e.OldValue;
      Xceed.Wpf.DataGrid.Views.ViewBase newView = ( Xceed.Wpf.DataGrid.Views.ViewBase )e.NewValue;

      grid.OnViewChanged( oldView, newView );
    }

    private void OnViewChanged( Xceed.Wpf.DataGrid.Views.ViewBase oldView, Xceed.Wpf.DataGrid.Views.ViewBase newView )
    {
      if( oldView != null )
      {
        oldView.ThemeChanged -= new DependencyPropertyChangedEventHandler( this.View_ThemeChanged );
        oldView.FixedHeaders.CollectionChanged -= new NotifyCollectionChangedEventHandler( this.View_FixedHeadersCollectionChanged );
        oldView.FixedFooters.CollectionChanged -= new NotifyCollectionChangedEventHandler( this.View_FixedFootersCollectionChanged );
      }

      if( newView == null )
      {
        newView = this.GetDefaultView();
      }
      else
      {
        newView.ThemeChanged += new DependencyPropertyChangedEventHandler( this.View_ThemeChanged );
        newView.FixedHeaders.CollectionChanged += new NotifyCollectionChangedEventHandler( this.View_FixedHeadersCollectionChanged );
        newView.FixedFooters.CollectionChanged += new NotifyCollectionChangedEventHandler( this.View_FixedFootersCollectionChanged );
      }

      // Cache if the view requires to preserve container size
      this.ViewPreservesContainerSize = ( newView != null )
        ? newView.PreserveContainerSize
        : true;

      if( ( newView == null ) || ( newView is UIViewBase ) )
      {
        this.m_cachedView = ( UIViewBase )newView;
      }

      object newDefaultStyleKey = newView.GetDefaultStyleKey( typeof( DataGridControl ) );
      if( !object.Equals( newDefaultStyleKey, this.DefaultStyleKey ) )
      {
        this.ClearValue( FrameworkElement.DefaultStyleKeyProperty );

        if( !object.Equals( newDefaultStyleKey, this.DefaultStyleKey ) )
        {
          this.DefaultStyleKey = newDefaultStyleKey;
        }
      }

      this.InvalidateViewStyle();

      // We cannot be sure that the grid elements default style key are different with
      // this new View/Theme (for instance, if the new View/Theme are of the same type 
      // as the old ones). So, we have to force the new templates. This is mainly to 
      // setup the new ViewBindings.
      this.ReapplyTemplate();


      if( this.ViewChanged != null )
        this.ViewChanged( this, EventArgs.Empty );

      // Reset the size states since we do not want to apply old size states to a new view/new container style.
      this.DataGridContext.ClearSizeStates();

      // Reset the flag
      this.ForceGeneratorReset = false;

      // Make sure the current item is into view.
      this.DelayBringIntoViewAndFocusCurrent( DispatcherPriority.Render );
    }

    private Xceed.Wpf.DataGrid.Views.UIViewBase m_defaultView;
    private Xceed.Wpf.DataGrid.Views.UIViewBase m_cachedView; // = null

    #endregion View Property

    #region GroupLevelDescriptions Read-Only Property

    private static readonly DependencyPropertyKey GroupLevelDescriptionsPropertyKey =
        DependencyProperty.RegisterReadOnly( "GroupLevelDescriptions", typeof( GroupLevelDescriptionCollection ), typeof( DataGridControl ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty GroupLevelDescriptionsProperty;

    public GroupLevelDescriptionCollection GroupLevelDescriptions
    {
      get
      {
        return ( GroupLevelDescriptionCollection )this.GetValue( DataGridControl.GroupLevelDescriptionsProperty );
      }
    }

    #endregion GroupLevelDescriptions Read-Only Property

    #region DetailConfigurations Read-Only Property

    private static readonly DependencyPropertyKey DetailConfigurationsPropertyKey =
        DependencyProperty.RegisterReadOnly( "DetailConfigurations", typeof( DetailConfigurationCollection ), typeof( DataGridControl ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty DetailConfigurationsProperty;

    internal DetailConfigurationCollection DetailConfigurations
    {
      get
      {
        return ( DetailConfigurationCollection )this.GetValue( DataGridControl.DetailConfigurationsProperty );
      }
    }

    internal void SetDetailConfigurations( DetailConfigurationCollection value )
    {
      this.SetValue( DataGridControl.DetailConfigurationsPropertyKey, value );
    }

    #endregion DetailConfigurations Read-Only Property

    #region NavigationBehavior Property

    public static readonly DependencyProperty NavigationBehaviorProperty = DependencyProperty.RegisterAttached(
      "NavigationBehavior", typeof( NavigationBehavior ), typeof( DataGridControl ),
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

    #endregion NavigationBehavior Property

    #region PagingBehavior Property

    public static readonly DependencyProperty PagingBehaviorProperty =
      DependencyProperty.Register(
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

    #endregion PagingBehavior Property

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

    #endregion ItemScrollingBehavior Property

    #region CurrentRowInEditionState Read-Only Property

    internal RowState CurrentRowInEditionState
    {
      get
      {
        return m_currentRowInEditionState;
      }
    }

    #endregion CurrentRowInEditionState Read-Only Property

    #region CurrentItemInEdition Read-Only Property

    internal object CurrentItemInEdition
    {
      get
      {
        return m_currentItemInEdition;
      }
    }

    #endregion CurrentItemInEdition Read-Only Property

    #region FixedItem Attached Property

    private static readonly object NotSet = new Object();

    // The ownerType is set to FrameworkElement to make the inheritance works for all grid elements.
    private static readonly DependencyProperty FixedItemProperty = DependencyProperty.RegisterAttached(
      "FixedItem", typeof( object ), typeof( DataGridControl ),
      new FrameworkPropertyMetadata( DataGridControl.NotSet, FrameworkPropertyMetadataOptions.Inherits ) );

    private static object GetFixedItem( DependencyObject obj )
    {
      return obj.GetValue( DataGridControl.FixedItemProperty );
    }

    private static void SetFixedItem( DependencyObject obj, object value )
    {
      obj.SetValue( DataGridControl.FixedItemProperty, value );
    }

    #endregion FixedItem Attached Property

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

    #endregion Hidden Properties

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

    #endregion ItemContainerGenerator Property

    #region CustomItemContainerGenerator Property

    internal CustomItemContainerGenerator CustomItemContainerGenerator
    {
      get
      {
        return m_customItemContainerGenerator;
      }
    }

    #endregion CustomItemContainerGenerator Property

    #region IsBeingEdited Property

    private static readonly DependencyPropertyKey IsBeingEditedPropertyKey =
        DependencyProperty.RegisterReadOnly( "IsBeingEdited", typeof( bool ), typeof( DataGridControl ), new UIPropertyMetadata( false ) );

    public static readonly DependencyProperty IsBeingEditedProperty =
      IsBeingEditedPropertyKey.DependencyProperty;

    public bool IsBeingEdited
    {
      get
      {
        return ( bool )this.GetValue( DataGridControl.IsBeingEditedProperty );
      }
    }

    private void UpdateIsBeingEdited()
    {
      bool isBeingEdited = ( m_currentItemInEdition != null );

      if( isBeingEdited != this.IsBeingEdited )
      {
        if( isBeingEdited )
        {
          this.SetValue( DataGridControl.IsBeingEditedPropertyKey, isBeingEdited );
        }
        else
        {
          this.ClearValue( DataGridControl.IsBeingEditedPropertyKey );
        }
      }
    }

    #endregion IsBeingEdited Property

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

    public static readonly DependencyProperty GroupConfigurationSelectorProperty =
        DependencyProperty.Register( "GroupConfigurationSelector", typeof( GroupConfigurationSelector ), typeof( DataGridControl ), new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnGroupConfigurationSelectorChanged ) ) );

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
      DataGridControl dataGridControl = sender as DataGridControl;
      if( dataGridControl != null )
      {
        if( dataGridControl.GroupConfigurationSelectorChanged != null )
        {
          dataGridControl.GroupConfigurationSelectorChanged( dataGridControl, EventArgs.Empty );
        }
      }
    }

    #endregion GroupConfigurationSelector Property

    #region AllowDetailToggle Property

    public static readonly DependencyProperty AllowDetailToggleProperty =
        DependencyProperty.Register( "AllowDetailToggle", typeof( bool ), typeof( DataGridControl ), new FrameworkPropertyMetadata( true, new PropertyChangedCallback( OnAllowDetailToggleChanged ) ) );

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
      DataGridControl dataGridControl = sender as DataGridControl;
      if( dataGridControl != null )
      {
        if( dataGridControl.AllowDetailToggleChanged != null )
        {
          dataGridControl.AllowDetailToggleChanged( dataGridControl, EventArgs.Empty );
        }
      }
    }

    #endregion AllowDetailToggle Property

    #region DefaultDetailConfiguration Property

    public static readonly DependencyProperty DefaultDetailConfigurationProperty =
        DependencyProperty.Register( "DefaultDetailConfiguration", typeof( DefaultDetailConfiguration ), typeof( DataGridControl ), new FrameworkPropertyMetadata( null ) );

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

    #endregion DefaultDetailConfiguration Property

    #region DefaultGroupConfiguration Property

    public static readonly DependencyProperty DefaultGroupConfigurationProperty =
        DependencyProperty.Register( "DefaultGroupConfiguration", typeof( GroupConfiguration ), typeof( DataGridControl ), new FrameworkPropertyMetadata( null ) );

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

    #endregion DefaultGroupConfiguration Property

    #region ContainerGroupConfiguration Attached Property

    internal static readonly DependencyProperty ContainerGroupConfigurationProperty =
        DependencyProperty.RegisterAttached( "ContainerGroupConfiguration", typeof( GroupConfiguration ), typeof( DataGridControl ), new UIPropertyMetadata( null ) );

    internal static GroupConfiguration GetContainerGroupConfiguration( DependencyObject obj )
    {
      return ( GroupConfiguration )obj.GetValue( DataGridControl.ContainerGroupConfigurationProperty );
    }

    internal static void SetContainerGroupConfiguration( DependencyObject obj, GroupConfiguration value )
    {
      obj.SetValue( DataGridControl.ContainerGroupConfigurationProperty, value );
    }

    #endregion ContainerGroupConfiguration Attached Property

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
        m_itemsSourceName = value;
        this.OnNotifyPropertyChanged( new PropertyChangedEventArgs( "ItemsSourceName" ) );
      }
    }

    private object m_itemsSourceName;
    private object m_detectedName;

    #endregion

    #region ItemsSourceNameTemplate Property

    public static readonly DependencyProperty ItemsSourceNameTemplateProperty =
        DependencyProperty.Register( "ItemsSourceNameTemplate", typeof( DataTemplate ), typeof( DataGridControl ), new UIPropertyMetadata( null ) );

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

    #endregion ItemsSourceNameTemplate Property

    #region UpdateSourceTrigger Property

    public static readonly DependencyProperty UpdateSourceTriggerProperty =
        DependencyProperty.Register( "UpdateSourceTrigger", typeof( DataGridUpdateSourceTrigger ), typeof( DataGridControl ), new UIPropertyMetadata( DataGridUpdateSourceTrigger.RowEndingEdit ) );

    public DataGridUpdateSourceTrigger UpdateSourceTrigger
    {
      get
      {
        return ( DataGridUpdateSourceTrigger )GetValue( UpdateSourceTriggerProperty );
      }
      set
      {
        SetValue( UpdateSourceTriggerProperty, value );
      }
    }

    #endregion UpdateSourceTrigger Property

    #region ForceGeneratorReset Property

    // This property is used to avoid a reset when Column Virtualization is
    // on and all Row instances are using FixedCellPanel as PART_CellsHost
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
        }

        return m_clipboardExporters;
      }
    }

    private Dictionary<string, ClipboardExporterBase> m_clipboardExporters; // = null;

    #endregion

    #region IsDeleteCommandEnabled Property

    public static readonly DependencyProperty IsDeleteCommandEnabledProperty =
      DependencyProperty.Register( "IsDeleteCommandEnabled", typeof( bool ), typeof( DataGridControl ), new FrameworkPropertyMetadata( false, new PropertyChangedCallback( DataGridControl.OnIsDeleteCommandEnabledChanged ) ) );

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

    private static void OnIsDeleteCommandEnabledChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl dataGridControl = obj as DataGridControl;

      if( dataGridControl == null )
        return;

      // On keep the command binding active if it is required
      if( ( bool )e.NewValue )
      {
        if( !dataGridControl.CommandBindings.Contains( dataGridControl.m_deleteCommandBinding ) )
          dataGridControl.CommandBindings.Add( dataGridControl.m_deleteCommandBinding );
      }
      else
      {
        dataGridControl.CommandBindings.Remove( dataGridControl.m_deleteCommandBinding );
      }

      CommandManager.InvalidateRequerySuggested();
    }

    #endregion IsDeleteCommandEnabled Property

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

    private static void OnIsCopyCommandEnabledChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl dataGridControl = o as DataGridControl;

      if( dataGridControl == null )
        return;

      // On keep the command binding active if it is required
      if( ( bool )e.NewValue )
      {
        if( !dataGridControl.CommandBindings.Contains( dataGridControl.m_copyCommandBinding ) )
          dataGridControl.CommandBindings.Add( dataGridControl.m_copyCommandBinding );
      }
      else
      {
        dataGridControl.CommandBindings.Remove( dataGridControl.m_copyCommandBinding );
      }

      CommandManager.InvalidateRequerySuggested();
    }

    #endregion

    #region IsRefreshCommandEnabled Property

    public static readonly DependencyProperty IsRefreshCommandEnabledProperty =
      DependencyProperty.Register( "IsRefreshCommandEnabled", typeof( bool ), typeof( DataGridControl ),
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

    private static void OnIsRefreshCommandEnabledChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl dataGridControl = obj as DataGridControl;

      if( dataGridControl == null )
        return;

      // On keep the command binding active if it is required
      if( ( bool )e.NewValue )
      {
        if( !dataGridControl.CommandBindings.Contains( dataGridControl.m_refreshCommandBinding ) )
          dataGridControl.CommandBindings.Add( dataGridControl.m_refreshCommandBinding );
      }
      else
      {
        dataGridControl.CommandBindings.Remove( dataGridControl.m_refreshCommandBinding );
      }

      CommandManager.InvalidateRequerySuggested();
    }

    #endregion IsRefreshCommandEnabled Property

    #region MaxSortLevels Property

    public static readonly DependencyProperty MaxSortLevelsProperty =
        DependencyProperty.Register( "MaxSortLevels", typeof( int ), typeof( DataGridControl ),
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
      DataGridControl dataGridControl = sender as DataGridControl;
      if( dataGridControl != null )
      {
        if( dataGridControl.MaxSortLevelsChanged != null )
        {
          dataGridControl.MaxSortLevelsChanged( dataGridControl, EventArgs.Empty );
        }
      }
    }

    #endregion MaxSortLevels Property

    #region MaxGroupLevels Property

    public static readonly DependencyProperty MaxGroupLevelsProperty =
        DependencyProperty.Register( "MaxGroupLevels", typeof( int ), typeof( DataGridControl ),
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
      DataGridControl dataGridControl = sender as DataGridControl;
      if( dataGridControl != null )
      {
        if( dataGridControl.MaxGroupLevelsChanged != null )
        {
          dataGridControl.MaxGroupLevelsChanged( dataGridControl, EventArgs.Empty );
        }
      }
    }

    #endregion MaxGroupLevels Property

    #region SynchronizeSelectionWithCurrent Property

    public static readonly DependencyProperty SynchronizeSelectionWithCurrentProperty =
        DependencyProperty.Register( "SynchronizeSelectionWithCurrent", typeof( bool ), typeof( DataGridControl ), new UIPropertyMetadata( false ) );

    public bool SynchronizeSelectionWithCurrent
    {
      get
      {
        return ( bool )GetValue( DataGridControl.SynchronizeSelectionWithCurrentProperty );
      }
      set
      {
        SetValue( DataGridControl.SynchronizeSelectionWithCurrentProperty, value );
      }
    }

    #endregion SynchronizeSelectionWithCurrent Property

    #region SynchronizeCurrent Property

    public static readonly DependencyProperty SynchronizeCurrentProperty =
        DependencyProperty.Register( "SynchronizeCurrent", typeof( bool ), typeof( DataGridControl ), new UIPropertyMetadata( true ) );

    public bool SynchronizeCurrent
    {
      get
      {
        return ( bool )GetValue( DataGridControl.SynchronizeCurrentProperty );
      }
      set
      {
        SetValue( DataGridControl.SynchronizeCurrentProperty, value );
      }
    }

    #endregion SynchronizeCurrent Property

    #region PreserveExtendedSelection Property

    [Obsolete( "The PreserveExtendedSelection dependency property is obsolete.", false )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static readonly DependencyProperty PreserveExtendedSelectionProperty =
        DependencyProperty.Register( "PreserveExtendedSelection", typeof( bool ), typeof( DataGridControl ), new UIPropertyMetadata( false ) );

    [Obsolete( "The PreserveExtendedSelection property is obsolete.", false )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public bool PreserveExtendedSelection
    {
      get
      {
        return ( bool )GetValue( DataGridControl.PreserveExtendedSelectionProperty );
      }
      set
      {
        SetValue( DataGridControl.PreserveExtendedSelectionProperty, value );
      }
    }

    #endregion PreserveExtendedSelection

    #region PreserveSelectionWhenEnteringEdit Property

    [Obsolete( "The PreserveSelectionWhenEnteringEdit dependency property is obsolete.", false )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static readonly DependencyProperty PreserveSelectionWhenEnteringEditProperty =
        DependencyProperty.Register( "PreserveSelectionWhenEnteringEdit", typeof( bool ), typeof( DataGridControl ), new UIPropertyMetadata( false ) );

    [Obsolete( "The PreserveSelectionWhenEnteringEdit property is obsolete.", false )]
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public bool PreserveSelectionWhenEnteringEdit
    {
      get
      {
        return ( bool )GetValue( DataGridControl.PreserveSelectionWhenEnteringEditProperty );
      }
      set
      {
        SetValue( DataGridControl.PreserveSelectionWhenEnteringEditProperty, value );
      }
    }

    #endregion PreserveSelectionWhenEnteringEdit

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

    #region InhibitPreviewGotKeyboardFocus Private Property

    private bool InhibitPreviewGotKeyboardFocus
    {
      get
      {
        return m_flags[ ( int )DataGridControlFlags.InhibitPreviewGotKeyboardFocus ];
      }
      set
      {
        m_flags[ ( int )DataGridControlFlags.InhibitPreviewGotKeyboardFocus ] = value;
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

    private static readonly DependencyPropertyKey ConnectionStatePropertyKey =
        DependencyProperty.RegisterReadOnly( "ConnectionState", typeof( DataGridConnectionState ), typeof( DataGridControl ), new UIPropertyMetadata( DataGridConnectionState.Idle ) );

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

    #endregion ConnectionState ReadOnly Dependency Property

    #region ConnectionError ReadOnly Dependency Property

    private static readonly DependencyPropertyKey ConnectionErrorPropertyKey =
        DependencyProperty.RegisterReadOnly( "ConnectionError", typeof( object ), typeof( DataGridControl ), new UIPropertyMetadata( null ) );

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

    #endregion ConnectionError ReadOnly Dependency Property

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


    #region TemplateApplied Event

    internal event EventHandler TemplateApplied;

    #endregion

    #region DetailsChanged event

    internal event EventHandler DetailsChanged;

    private void OnDetailsChanged( object sender, EventArgs e )
    {
      if( this.DetailsChanged != null )
      {
        this.DetailsChanged( sender, e );
      }
    }

    #endregion

    #region DeletingSelectedItems Event

    public static readonly RoutedEvent DeletingSelectedItemsEvent =
      EventManager.RegisterRoutedEvent( "DeletingSelectedItems", RoutingStrategy.Bubble, typeof( CancelRoutedEventHandler ), typeof( DataGridControl ) );

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

    #endregion DeletingSelectedItems Event

    #region SelectedItemsDeleted Event

    public static readonly RoutedEvent SelectedItemsDeletedEvent =
      EventManager.RegisterRoutedEvent( "SelectedItemsDeleted", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( DataGridControl ) );

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
      RoutedEventArgs e = new RoutedEventArgs( DataGridControl.SelectedItemsDeletedEvent, this );
      this.RaiseEvent( e );
    }

    #endregion SelectedItemsDeleted Event

    #region SelectionChanged Event

    public static readonly RoutedEvent SelectionChangedEvent =
      EventManager.RegisterRoutedEvent( "SelectionChanged", RoutingStrategy.Bubble, typeof( DataGridSelectionChangedEventHandler ), typeof( DataGridControl ) );

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

    #endregion SelectionChanged Event

    #region SelectionChanging Event

    public static readonly RoutedEvent SelectionChangingEvent =
      EventManager.RegisterRoutedEvent( "SelectionChanging", RoutingStrategy.Bubble, typeof( DataGridSelectionChangingEventHandler ), typeof( DataGridControl ) );

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
      this.OnSelectionChanging( e );
    }

    #endregion SelectionChanging Event

    #region DeletingSelectedItemError Event

    public static readonly RoutedEvent DeletingSelectedItemErrorEvent =
      EventManager.RegisterRoutedEvent( "DeletingSelectedItemError", RoutingStrategy.Bubble, typeof( DeletingSelectedItemErrorRoutedEventHandler ), typeof( DataGridControl ) );

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

    #endregion DeletingSelectedItemError Event

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

    #region Event handlers

    private static void OnFlowDirectionPropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl dataGrid = sender as DataGridControl;
      if( dataGrid != null )
      {
        dataGrid.CustomItemContainerGenerator.ResetGeneratorContent();
      }
    }

    public void OnDataGridCollectionView_RootGroupChanged( object sender, EventArgs e )
    {
      this.SetValue( DataGridControl.StatContextPropertyKey, ( ( DataGridCollectionView )sender ).RootGroup );
    }

    private void OnDetailConfigurationsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      this.OnNotifyPropertyChanged( new PropertyChangedEventArgs( "HasDetails" ) );

      DataGridDetailDescriptionCollection detailDescriptions = null;
      DataGridCollectionViewBase dataGridCollectionViewBase = this.ItemsSource as DataGridCollectionViewBase;

      if( dataGridCollectionViewBase != null )
        detailDescriptions = dataGridCollectionViewBase.DetailDescriptions;

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

    public void OnDataGridDetailDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      DataGridDetailDescriptionCollection detailDescriptions = sender as DataGridDetailDescriptionCollection;
      Debug.Assert( detailDescriptions != null );

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Reset:
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Remove:
        case NotifyCollectionChangedAction.Add:
          DetailConfiguration.SynchronizeDetailConfigurations(
            detailDescriptions,
            this.DetailConfigurations,
            this.AutoCreateDetailConfigurations,
            this.AutoCreateForeignKeyConfigurations,
            this.AutoRemoveColumnsAndDetailConfigurations );
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

    public bool AllowDrag
    {
      get
      {
        return ( bool )GetValue( DataGridControl.AllowDragProperty );
      }
      set
      {
        SetValue( DataGridControl.AllowDragProperty, value );
      }
    }

    public static readonly DependencyProperty AllowDragProperty =
        DependencyProperty.Register( "AllowDrag", typeof( bool ), typeof( DataGridControl ), new UIPropertyMetadata( false, new PropertyChangedCallback( DataGridControl.OnAllowDragPropertyChanged ) ) );

    private static void OnAllowDragPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl dataGrid = obj as DataGridControl;

      if( ( dataGrid == null ) || ( ( bool )e.NewValue ) )
      {
        return;
      }

      dataGrid.ResetDragDataObject();
    }

    internal IDataObject DragDataObject
    {
      get
      {
        return m_dragDataObject;
      }
    }

    internal void InitializeDragPostion( MouseEventArgs e )
    {
      if( !this.AllowDrag )
        return;

      m_initialDragPosition = e.GetPosition( this );
    }

    internal void DoDrag( MouseEventArgs e )
    {
      if( !this.AllowDrag )
        return;

      //If we have effect, it means the drag is already processing, no need to do anything further.
      if( m_dragDropEffects != null )
        return;

      // Compare the present mouse position to the inital drag position, as to start the drag only if there is a reasonable mouse movement
      if( ( m_initialDragPosition != null ) && ( m_initialDragPosition.HasValue ) )
      {
        Point currentMousePosition = e.GetPosition( this );

        if( DragDropHelper.IsMouseMoveDrag( m_initialDragPosition.Value, currentMousePosition ) )
        {
          //The first time the drag operation starts, we need to initialize the data that will be dragged.
          if( m_dragDataObject == null )
          {
            this.PrepareDragDataObject( e );
          }

          m_dragDropEffects = DragDrop.DoDragDrop( this, m_dragDataObject, DragDropEffects.Copy );
        }
      }
    }

    internal void ResetDragDataObject()
    {
      m_dragDataObject = null;
      m_initialDragPosition = null;
      m_dragDropEffects = null;
    }

    private void PrepareDragDataObject( MouseEventArgs e )
    {
      if( !this.AllowDrag )
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
    private Nullable<DragDropEffects> m_dragDropEffects; // = null;

    #endregion Drag Management

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
      this.QueueDataGridContextPeerChlidrenRefresh( this.DataGridContext );

      // We need to reevaluate the CurrentContext when it is a detail context
      // since the details are cleared and recreated when the theme changes
      // and no update is made to the current item itself. We dispatch it with
      // a high priority in order to be sure it is executed before a potential
      // BringIntoView of the current item.
      if( this.CurrentContext != this.DataGridContext )
      {
        this.Dispatcher.BeginInvoke( new Action( delegate()
        {
          this.UpdateCurrentContextOnThemeChanged( this.CurrentContext );
        } ), DispatcherPriority.Normal );
      }

      Xceed.Wpf.DataGrid.Views.ViewBase view = this.GetView();

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

      ControlTemplate oldTemplate = this.Template;

      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( DataGridControl ) );

      ControlTemplate newTemplate = this.Template;

      // When a null Theme is set or the new Theme is the already the applied 
      // System Theme, we detect if the template changed after setting the 
      // DefaultStyleKey to refresh the FixedHeaders and FixedFooters
      if( oldTemplate == newTemplate )
        this.ResetFixedRegions();

      // Make sure the current item is into view.
      this.DelayBringIntoViewAndFocusCurrent( DispatcherPriority.Render );
    }

    /// <summary>
    /// Calling this method will enable the grid's current view to be styled.
    /// If any implicit Style exits, it will be immediately applied.
    /// </summary>
    private void InvalidateViewStyle()
    {
      if( this.IsPrinting )
      {
        Xceed.Wpf.DataGrid.Views.ViewBase viewBase = this.GetView();

        // If more than one DataGridPaginator is paginating, the ViewStyle is already invalidated and
        // the PrintView is already a logical child.
        if( LogicalTreeHelper.GetParent( viewBase ) == null )
        {
          this.AddLogicalChild( viewBase );
        }
      }
      else
      {
        this.AddLogicalChild( this.GetView() );
      }
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

    internal Xceed.Wpf.DataGrid.Views.ViewBase GetView( bool getViewOrDefault = true )
    {
      Xceed.Wpf.DataGrid.Views.ViewBase view = this.View;
      if( view != null )
        return view;

      if( ( m_defaultView != null ) || ( getViewOrDefault ) )
        return this.GetDefaultView();

      return null;
    }

    private Xceed.Wpf.DataGrid.Views.ViewBase GetDefaultView()
    {
      if( m_defaultView == null )
      {
        m_defaultView = new Xceed.Wpf.DataGrid.Views.TableflowView();
      }

      return m_defaultView;
    }

    #endregion VIEWBASE METHODS

    #region FOCUS

    internal bool IsSetFocusInhibited
    {
      get
      {
        return m_inhibitSetFocus != 0;
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

      toFocus = DataGridControl.FindFirstFocusableChild( toFocus );

      if( toFocus == null )
        return false;

      try
      {
        return toFocus.Focus();
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

    internal bool SetFocusHelper(
      UIElement itemContainer,
      ColumnBase column,
      bool forceFocus,
      bool preserveEditorFocus )
    {
      return this.SetFocusHelper( itemContainer,
        column,
        forceFocus,
        preserveEditorFocus,
        false );
    }

    internal bool SetFocusHelper(
      UIElement itemContainer,
      ColumnBase column,
      bool forceFocus,
      bool preserveEditorFocus,
      bool preventMakeVisibleIfCellFocused )
    {
      if( m_inhibitSetFocus != 0 )
        return false;

      if( itemContainer == null )
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

      UIElement toFocus = null;

      Row row = Row.FromContainer( itemContainer );

      IDisposable preventMakeVisibleDisposable = null;

      try
      {
        if( row != null )
        {
          Cell cell = row.Cells[ column ];

          // The Cell must be in VisualTree to be able to get
          // the focus, else, .Focus() will always return false
          if( cell != null )
          {
            cell.EnsureInVisualTree();

            if( preventMakeVisibleIfCellFocused )
              preventMakeVisibleDisposable = cell.InhibitMakeVisible();
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

            bool currentFocusIsInsideCell =
              ( cell != Keyboard.FocusedElement )
              && ( TreeHelper.IsDescendantOf( Keyboard.FocusedElement as DependencyObject, cell ) );

            //if the focus is already within the Cell to focus, then don't touch a thing
            if( ( currentFocusIsInsideCell ) && ( preserveEditorFocus ) && ( cell.IsBeingEdited ) )
            {
              //that means that the item to focus should be the Keyboard focused element

              // Already focused
              return true;
            }
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

            bool currentFocusIsInsideCell =
              ( cell != Keyboard.FocusedElement )
              && ( TreeHelper.IsDescendantOf( Keyboard.FocusedElement as DependencyObject, cell ) );

            //if the focus is already within the Cell to focus, then don't touch a thing
            if( ( currentFocusIsInsideCell ) && ( preserveEditorFocus ) )
            {
              // That means that the item to focus should be the Keyboard focused element
              // Already focused
              return true;
            }
          }
        }
        else
        {
          toFocus = itemContainer;

          //if the container for the item is not a Row, and the focus is already within that item
          //then don't touch the focus.
          bool partOfContainer = TreeHelper.IsDescendantOf( Keyboard.FocusedElement as DependencyObject, toFocus );

          if( partOfContainer )
          {
            // Already focused
            return true;
          }
        }

        return DataGridControl.SetFocusUIElementHelper( toFocus );
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

    #endregion FOCUS

    #region BRING INTO VIEW

    internal IDisposable InhibitQueueBringIntoView()
    {
      return new GridControlQueueInhibiter( this );
    }

    internal IDisposable InhibitSetFocus()
    {
      return new DataGridControlSetFocusInhibiter( this );
    }

    internal void DelayBringIntoViewAndFocusCurrent()
    {
      this.DelayBringIntoViewAndFocusCurrent( DispatcherPriority.DataBind );
    }

    private void DelayBringIntoViewAndFocusCurrent( DispatcherPriority priority )
    {
      if( ( m_inhibitBringIntoView != 0 ) || ( m_inhibitSetFocus != 0 ) || ( m_queueBringIntoView != null ) )
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
      if( m_inhibitBringIntoView != 0 )
        return null;

      return this.Dispatcher.BeginInvoke( priority, new GenericHandler( this.QueueBringIntoViewHelper ) );
    }

    private bool QueueBringIntoViewHelper()
    {
      DataGridContext currentContext = this.CurrentContext;
      object internalCurrentItem = ( currentContext != null ) ? currentContext.InternalCurrentItem : null;

      if( internalCurrentItem == null )
        return false;

      return this.BringItemIntoViewHelper( currentContext, internalCurrentItem );
    }

    public bool BringItemIntoView( object item )
    {
      return this.BringItemIntoViewHelper( this.DataGridContext, item );
    }

    internal bool BringItemIntoViewHelper( DataGridContext dataGridContext, object item )
    {
      FrameworkElement itemsHost = this.ItemsHost;

      //this is a protection in case the Template is incomplete or not realized yet.
      if( itemsHost == null )
        return false;

      // It is possible that a BringIntoView was queued before an operation that will
      // detach the ItemsHost of the DataGridControl and we want to avoid to call 
      // ICustomVirtualizingPanel.BringIntoView in this situation.
      DataGridContext itemsHostDataGridContext = DataGridControl.GetDataGridContext( itemsHost );

      if( itemsHostDataGridContext == null )
      {
        return false;
      }

      FrameworkElement container = dataGridContext.GetContainerFromItem( item ) as FrameworkElement;

      //if a container exists, call bring into view on it!
      if( container != null )
      {
        container.BringIntoView();

        //flag the function as being successful
        return true;
      }

      // The container does not exist, it is not yet realized.

      // If we are not virtualizing any items, return.
      // The call to SetFocusHelper when the BringIntoView completes will call a FrameworkElement BringIntoView
      // which will cause the ScrollViewer to bring the item into view approprietly.
      if( ( this.ScrollViewer != null ) && ( !this.ScrollViewer.CanContentScroll ) )
        return false;

      ICustomVirtualizingPanel customPanel = itemsHost as ICustomVirtualizingPanel;

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

    #endregion BRING INTO VIEW

    #region COMMANDS

    private void OnExpandGroupCanExecute( Object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = false;

      CollectionViewGroup group = e.Parameter as CollectionViewGroup;
      if( ( e.Parameter != null ) && ( group != null ) )
      {
        try
        {
          if( this.DataGridContext.IsGroupExpandedCore( group, true ) == false )
          {
            e.CanExecute = true;
          }
        }
        catch( InvalidOperationException )
        { //suppress
        }
      }
    }

    protected virtual void OnExpandGroupExecuted( Object sender, ExecutedRoutedEventArgs e )
    {
      CollectionViewGroup cvg = e.Parameter as CollectionViewGroup;
      if( cvg != null )
      {
        try
        {
          this.DataGridContext.ExpandGroup( cvg );
        }
        catch( InvalidOperationException )
        {
          //suppress
        }
      }
    }

    private void OnCollapseGroupCanExecute( Object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = false;

      CollectionViewGroup group = e.Parameter as CollectionViewGroup;
      if( ( e.Parameter != null ) && ( group != null ) )
      {
        try
        {
          if( this.DataGridContext.IsGroupExpandedCore( group, true ) == true )
          {
            e.CanExecute = true;
          }
        }
        catch( InvalidOperationException )
        { //suppress
        }
      }
    }

    protected virtual void OnCollapseGroupExecuted( Object sender, ExecutedRoutedEventArgs e )
    {
      CollectionViewGroup cvg = e.Parameter as CollectionViewGroup;
      if( cvg != null )
      {
        try
        {
          this.DataGridContext.CollapseGroup( cvg );
        }
        catch( InvalidOperationException )
        {
          //suppress
        }
      }
    }

    private void OnToggleGroupCanExecute( Object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = false;

      if( ( e.Parameter != null ) && ( e.Parameter is CollectionViewGroup ) )
      {
        e.CanExecute = true;
      }
    }

    protected virtual void OnToggleGroupExecuted( Object sender, ExecutedRoutedEventArgs e )
    {
      CollectionViewGroup cvg = e.Parameter as CollectionViewGroup;
      if( cvg != null )
      {
        try
        {
          this.DataGridContext.ToggleGroupExpansion( cvg );
        }
        catch( InvalidOperationException )
        {
          //suppress
        }
      }
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
        try
        {
          this.SelectionChangerManager.End( false, true, false );
        }
        catch( DataGridException )
        {
          // This is to swallow when selection is aborted
        }
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
        if( ( !dataGridContext.IsDeleteCommandEnabled ) ||
          ( dataGridContext.SelectedItems.Count == 0 ) )
        {
          continue;
        }

        DataGridCollectionViewBase dataGridCollectionViewBase =
          dataGridContext.ItemsSourceCollection as DataGridCollectionViewBase;

        IList list = ( dataGridCollectionViewBase == null )
          ? ItemsSourceHelper.TryGetIList( dataGridContext.ItemsSourceCollection )
          : null; // If we already have a DataGridCollectionView, no need to look for an IList source.

        if( ( dataGridCollectionViewBase != null )
          || ( ( list != null ) && DataGridControl.IsRemoveAllowedForDeleteCommand( list ) ) )
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

        DataGridCollectionViewBase dataGridCollectionViewBase =
          dataGridContext.ItemsSourceCollection as DataGridCollectionViewBase;

        IList list = ( dataGridCollectionViewBase == null )
          ? ItemsSourceHelper.TryGetIList( dataGridContext.ItemsSourceCollection )
          : null; // If we already have a DataGridCollectionView, no need to look for an IList source.

        if( ( dataGridCollectionViewBase != null )
          || ( ( list != null ) && DataGridControl.IsRemoveAllowedForDeleteCommand( list ) ) )
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
                    dataGridCollectionViewBase.RemoveAt( itemIndex );
                }
                else if( list != null )
                {
                  itemIndex = list.IndexOf( item );

                  if( itemIndex != -1 )
                    list.RemoveAt( itemIndex );
                }

                retry = false;
                raiseItemDeleted = true;
              }
              catch( Exception ex )
              {
                DeletingSelectedItemErrorRoutedEventArgs deletingItemErrorArgs = new DeletingSelectedItemErrorRoutedEventArgs( item, ex, DataGridControl.DeletingSelectedItemErrorEvent, this );
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
        this.OnSelectedItemsDeleted();
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
          collectionView.Refresh();
      }
    }

    protected virtual void OnClearFilterExecuted( Object sender, ExecutedRoutedEventArgs e )
    {
      DataGridCollectionViewBase dgcv = this.ItemsSource as DataGridCollectionViewBase;

      if( dgcv != null )
      {
        using( dgcv.DeferRefresh() )
        {
          foreach( DataGridItemPropertyBase itemProperty in dgcv.ItemProperties )
          {
            if( itemProperty.FilterCriterion != null )
            {
              itemProperty.FilterCriterion = null;
            }
          }

          foreach( DataGridDetailDescription detailDescription in dgcv.DetailDescriptions )
          {
            foreach( DataGridItemPropertyBase itemProperty in detailDescription.ItemProperties )
            {
              if( itemProperty.FilterCriterion != null )
              {
                itemProperty.FilterCriterion = null;
              }
            }
          }
        }
      }
    }

    #endregion

    #region SOURCE HANDLING

    private void ProcessDelayedItemsSourceChanged()
    {
      if( this.ShouldSynchronizeCurrentItem )
      {
        DataGridContext localContext = this.DataGridContext;
        object currentItem = this.Items.CurrentItem;
        int currentItemIndex = this.Items.CurrentPosition;

        if( ( this.Items.IsCurrentAfterLast ) || ( this.Items.IsCurrentBeforeFirst ) )
        {
          currentItemIndex = -1;
        }

        localContext.SetCurrent( currentItem, null, currentItemIndex, null, false, false, this.SynchronizeSelectionWithCurrent );

        if( !this.SynchronizeSelectionWithCurrent )
        {
          m_selectionChangerManager.Begin();

          try
          {
            if( ( this.SelectionUnit == SelectionUnit.Row )
              && ( !this.Items.IsCurrentAfterLast )
              && ( !this.Items.IsCurrentBeforeFirst )
              && ( currentItemIndex != -1 ) )
            {
              m_selectionChangerManager.SelectJustThisItem( localContext, currentItemIndex, currentItem );
            }
            else
            {
              m_selectionChangerManager.UnselectAll();
            }
          }
          finally
          {
            m_selectionChangerManager.End( true, false, false );
          }
        }
      }

      this.DataGridContext.ItemsSourceFieldDescriptors = null;
      ItemsSourceHelper.CleanUpColumns( this.Columns, this.AutoRemoveColumnsAndDetailConfigurations );
      this.GenerateColumnsFromItemsSourceFields();

      DataGridCollectionView dataGridCollectionView = this.ItemsSource as DataGridCollectionView;

      if( dataGridCollectionView != null )
      {
        // We must keep a reference to the handler so that we can remove the same instance later on when unregistering from it.
        m_rootGroupChangedHandler = new WeakEventHandler<EventArgs>( this.OnDataGridCollectionView_RootGroupChanged ).Handler;
        dataGridCollectionView.RootGroupChanged += m_rootGroupChangedHandler;

        this.SetValue( DataGridControl.StatContextPropertyKey, dataGridCollectionView.RootGroup );

        // We must keep a reference to the handler so that we can remove the same instance later on when unregistering from it.
        m_dataGridDetailDescriptionsChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>( this.OnDataGridDetailDescriptionsChanged ).Handler;
        dataGridCollectionView.DetailDescriptions.CollectionChanged += m_dataGridDetailDescriptionsChangedHandler;

        DetailConfiguration.SynchronizeDetailConfigurations(
          dataGridCollectionView.DetailDescriptions,
          this.DetailConfigurations,
          this.AutoCreateDetailConfigurations,
          this.AutoCreateForeignKeyConfigurations,
          this.AutoRemoveColumnsAndDetailConfigurations );

        // We must keep a reference to the handler so that we can remove the same instance later on when unregistering from it.
        m_proxyCollectionRefreshHandler = new WeakEventHandler<EventArgs>( this.OnDataGridCollectionView_ProxyCollectionRefresh ).Handler;
        dataGridCollectionView.ProxyCollectionRefresh += m_proxyCollectionRefreshHandler;

        // We must keep a reference to the handler so that we can remove the same instance later on when unregistering from it.
        m_proxyGroupDescriptionsChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>( this.OnDataGridCollectionView_ProxyGroupDescriptionsChanged ).Handler;
        dataGridCollectionView.ProxyGroupDescriptionsChanged += m_proxyGroupDescriptionsChangedHandler;

        // We must keep a reference to the handler so that we can remove the same instance later on when unregistering from it.
        m_proxySortDescriptionsChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>( this.OnDataGridCollectionView_ProxySortDescriptionsChanged ).Handler;
        dataGridCollectionView.ProxySortDescriptionsChanged += m_proxySortDescriptionsChangedHandler;

        // We must keep a reference to the handler so that we can remove the same instance later on when unregistering from it.
        m_proxyAutoFilterValuesChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>( this.OnDataGridCollectionView_ProxyAutoFilterValuesChanged ).Handler;
        dataGridCollectionView.ProxyAutoFilterValuesChanged += m_proxyAutoFilterValuesChangedHandler;

        // We must keep a reference to the handler so that we can remove the same instance later on when unregistering from it.
        m_proxyApplyingFilterCriteriasHandler = new WeakEventHandler<EventArgs>( this.OnDataGridCollectionView_ProxyApplyingFilterCriterias ).Handler;
        dataGridCollectionView.ProxyApplyingFilterCriterias += m_proxyApplyingFilterCriteriasHandler;
      }
      else
      {
        DetailConfiguration.CleanupDetailConfigurations( this.DetailConfigurations, this.AutoRemoveColumnsAndDetailConfigurations );
        this.ClearValue( DataGridControl.StatContextPropertyKey );
      }

      DataGridVirtualizingCollectionViewBase virtualizingCollectionView = this.ItemsSource as DataGridVirtualizingCollectionViewBase;

      // Keep if the source is bound to DataGridVirtualizingCollectionView
      // to avoid preserving ContainerSizeState to avoid memory leaks
      this.IsBoundToDataGridVirtualizingCollectionViewBase = ( virtualizingCollectionView != null );

      if( this.IsBoundToDataGridVirtualizingCollectionViewBase )
      {
        // We must keep a reference to the handler so that we can remove the same instance later on when unregistering from it.
        m_connectionStateChangedHandler = new WeakEventHandler<EventArgs>( this.OnDataGridVirtualizingCollectionViewBase_ConnectionStateChanged ).Handler;
        virtualizingCollectionView.ConnectionStateChanged += m_connectionStateChangedHandler;

        // We must keep a reference to the handler so that we can remove the same instance later on when unregistering from it.
        m_connectionStateErrorHandler = new WeakEventHandler<EventArgs>( this.OnDataGridVirtualizingCollectionViewBase_ConnectionErrorChanged ).Handler;
        virtualizingCollectionView.ConnectionErrorChanged += m_connectionStateErrorHandler;
      }


      this.DataGridContext.NotifyItemsSourceChanged();

      if( this.ItemsSourceChangeCompleted != null )
        this.ItemsSourceChangeCompleted( this, EventArgs.Empty );
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
        this.AddingNewDataItem( this, e );
    }

    public event EventHandler ItemsSourceChangeCompleted;

    protected override void OnItemsSourceChanged( IEnumerable oldValue, IEnumerable newValue )
    {
      base.OnItemsSourceChanged( oldValue, newValue );

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
        this.OnNotifyPropertyChanged( new PropertyChangedEventArgs( "ItemsSourceName" ) );

      DataGridContext localContext = this.DataGridContext;
      this.UpdateCurrentRowInEditionCellStates( null, null );
      localContext.ClearSizeStates();

      if( m_beginInitCount > 0 )
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
        dataGridCollectionView.RootGroupChanged -= m_rootGroupChangedHandler;
        dataGridCollectionView.DetailDescriptions.CollectionChanged -= m_dataGridDetailDescriptionsChangedHandler;

        dataGridCollectionView.ProxyCollectionRefresh -= m_proxyCollectionRefreshHandler;
        dataGridCollectionView.ProxyGroupDescriptionsChanged -= m_proxyGroupDescriptionsChangedHandler;
        dataGridCollectionView.ProxySortDescriptionsChanged -= m_proxySortDescriptionsChangedHandler;
        dataGridCollectionView.ProxyAutoFilterValuesChanged -= m_proxyAutoFilterValuesChangedHandler;
        dataGridCollectionView.ProxyApplyingFilterCriterias -= m_proxyApplyingFilterCriteriasHandler;

        m_dataGridContextsStateDictionary = null;
      }

      DataGridVirtualizingCollectionViewBase virtualizingCollectionView = oldValue as DataGridVirtualizingCollectionViewBase;

      if( virtualizingCollectionView != null )
      {
        virtualizingCollectionView.ConnectionStateChanged -= m_connectionStateChangedHandler;
        virtualizingCollectionView.ConnectionErrorChanged -= m_connectionStateErrorHandler;
      }

      // We must refresh the FixedRegions to call Clear/Prepare Container for IDataGridItemContainer elements
      this.ResetFixedRegions();

      // Reset the flag
      this.ForceGeneratorReset = false;
    }

    private void GenerateColumnsFromItemsSourceFields()
    {
      if( this.AutoCreateColumns )
      {
        ItemsSourceHelper.GenerateColumnsFromItemsSourceFields(
          this.DataGridContext.Columns,
          this.DefaultCellEditors,
          this.DataGridContext.ItemsSourceFieldDescriptors,
          this.AutoCreateForeignKeyConfigurations );
      }
    }

    #endregion SOURCE HANDLING

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

    public void OnDataGridCollectionView_ProxyCollectionRefresh( object sender, EventArgs e )
    {
      DataGridCollectionView dataGridCollectionView = ( DataGridCollectionView )sender;

      ObservableCollection<GroupDescription> groupDescriptions = dataGridCollectionView.GroupDescriptions;

      if( dataGridCollectionView.GroupDescriptions.Count > 0 )
      {
        DataGridContext targetDataGridContext = DataGridContext.SafeGetDataGridContextForDataGridCollectionView( this.DataGridContext, dataGridCollectionView );

        if( targetDataGridContext != null )
        {
          this.SaveDataGridContextState( targetDataGridContext, false, int.MaxValue );
        }
      }
    }

    public void OnDataGridCollectionView_ProxyGroupDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      DataGridCollectionView dataGridCollectionView = ( DataGridCollectionView )sender;

      ObservableCollection<GroupDescription> groupDescriptions = dataGridCollectionView.GroupDescriptions;

      int maxGroupLevelToSave = -1;

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
        DataGridContext targetDataGridContext = DataGridContext.SafeGetDataGridContextForDataGridCollectionView(
 this.DataGridContext, dataGridCollectionView );

        if( targetDataGridContext != null )
          this.SaveDataGridContextState( targetDataGridContext, false, maxGroupLevelToSave );
      }
    }

    public void OnDataGridCollectionView_ProxySortDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      DataGridCollectionView dataGridCollectionView = ( DataGridCollectionView )sender;

      ObservableCollection<GroupDescription> groupDescriptions = dataGridCollectionView.GroupDescriptions;

      if( dataGridCollectionView.GroupDescriptions.Count > 0 )
      {
        DataGridContext targetDataGridContext = DataGridContext.SafeGetDataGridContextForDataGridCollectionView(
          this.DataGridContext, dataGridCollectionView );

        if( targetDataGridContext != null )
          this.SaveDataGridContextState( targetDataGridContext, false, int.MaxValue );
      }
    }

    public void OnDataGridCollectionView_ProxyAutoFilterValuesChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      DataGridCollectionView dataGridCollectionView = ( DataGridCollectionView )sender;

      ObservableCollection<GroupDescription> groupDescriptions = dataGridCollectionView.GroupDescriptions;

      if( dataGridCollectionView.GroupDescriptions.Count > 0 )
      {
        DataGridContext targetDataGridContext = DataGridContext.SafeGetDataGridContextForDataGridCollectionView(
          this.DataGridContext, dataGridCollectionView );

        if( targetDataGridContext != null )
          this.SaveDataGridContextState( targetDataGridContext, false, int.MaxValue );
      }
    }

    public void OnDataGridCollectionView_ProxyApplyingFilterCriterias( object sender, EventArgs e )
    {
      DataGridCollectionView dataGridCollectionView = ( DataGridCollectionView )sender;

      ObservableCollection<GroupDescription> groupDescriptions = dataGridCollectionView.GroupDescriptions;

      if( dataGridCollectionView.GroupDescriptions.Count > 0 )
      {
        DataGridContext targetDataGridContext = DataGridContext.SafeGetDataGridContextForDataGridCollectionView(
          this.DataGridContext, dataGridCollectionView );

        if( targetDataGridContext != null )
          this.SaveDataGridContextState( targetDataGridContext, false, int.MaxValue );
      }
    }

    #endregion SAVE/RESTORE STATE

    #region VIRTUALIZING COLLECTION VIEW SUPPORT

    public void OnDataGridVirtualizingCollectionViewBase_ConnectionStateChanged( object sender, EventArgs e )
    {
      DataGridVirtualizingCollectionViewBase dataGridVirtualizingCollectionViewBase = sender as DataGridVirtualizingCollectionViewBase;

      this.SetConnectionState( dataGridVirtualizingCollectionViewBase.ConnectionState );
    }

    public void OnDataGridVirtualizingCollectionViewBase_ConnectionErrorChanged( object sender, EventArgs e )
    {
      DataGridVirtualizingCollectionViewBase dataGridVirtualizingCollectionViewBase = sender as DataGridVirtualizingCollectionViewBase;

      this.SetConnectionError( dataGridVirtualizingCollectionViewBase.ConnectionError );
    }

    #endregion VIRTUALIZING COLLECTION VIEW SUPPORT

    #region PRINTING/EXPORTING

    internal bool IsPrinting
    {
      get
      {
        return m_flags[ ( int )DataGridControlFlags.UsePrintView ];
      }
      private set
      {
        m_flags[ ( int )DataGridControlFlags.UsePrintView ] = value;
      }
    }

    #endregion PRINTING/EXPORTING

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
      DataGridControl dataGridControl = dataGridContext.DataGridControl;

      if( dataGridControl.IsBeingEdited )
        return;

      if( item == null )
        return;

      if( !dataGridContext.IsContainingItem( item ) )
        throw new InvalidOperationException( "An attempt was made to call the BeginEdit method of an item that is not part of the specified context." );

      DependencyObject container = dataGridContext.GetContainerFromItem( item );

      //if the item is realized, then I could call the BeginEdit() directly on the container.
      if( container != null )
      {
        Row row = Row.FromContainer( container );

        if( row != null )
          row.BeginEdit();

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
      DataGridControl dataGridControl = dataGridContext.DataGridControl;

      if( !dataGridControl.IsBeingEdited )
        return;

      if( !dataGridContext.IsContainingItem( dataGridContext.DataGridControl.CurrentItemInEdition ) )
        throw new InvalidOperationException( "An attempt was made to call the EndEdit method of an item that is not part of the specified context." );

      DependencyObject container = dataGridContext.GetContainerFromItem( dataGridControl.m_currentItemInEdition );

      //if the item is realized, then I could call the EndEdit() directly on the container.
      if( container != null )
      {
        Row row = Row.FromContainer( container );

        if( row != null )
          row.EndEdit();

        //not a row, then not editable.
      }
      //if the container is not realized, then I need to set things up so that when the container is realized, it's gonna resume edition.
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
      DataGridControl dataGridControl = dataGridContext.DataGridControl;

      if( !dataGridControl.IsBeingEdited )
        return;

      if( !dataGridContext.IsCurrent )
        throw new InvalidOperationException( "An attempt was made to call the CancelEdit method on a DataGridContext that is not current." );

      DependencyObject container = dataGridContext.GetContainerFromItem( dataGridControl.m_currentItemInEdition );

      //if the item is realized, then I could call the CancelEdit() directly on the container.
      if( container != null )
      {
        Row row = Row.FromContainer( container );

        if( row != null )
          row.CancelEdit();

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
        Row currentRowInEdition = null;

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
              ColumnBase parentColumn = cell.ParentColumn;

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
          ColumnBase parentColumn = cell.ParentColumn;

          if( parentColumn == null )
            continue;

          CellState cellState = new CellState();
          cellState.SetContentBeforeRowEdition( cell.Content );

          parentColumn.CurrentRowInEditionCellState = cellState;
        }
      }
    }

    #endregion EDITING

    #region SELECTION/CURRENT

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

      m_queueClearCurrentColumn = this.Dispatcher.BeginInvoke( System.Windows.Threading.DispatcherPriority.DataBind, new ParametrizedGenericHandler( ClearCurrentColumnHandler ), currentItem );
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
            currentDataGridContext.SetCurrent( oldCurrentItem, null, null, null, false, false, this.SynchronizeSelectionWithCurrent );
          }
          catch( DataGridException )
          {            // We swallow the exception if it occurs because of a validation error or Cell was read-only or
            // any other GridException.
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

      // the CurrentContext is the master DataGridContext, and
      // this one will never change
      if( currentContext == this.DataGridContext )
      {
        return;
      }

      object newCurrentItem = currentContext.CurrentItem;

      // Try to get the new DataGridContext mathing the old one (containing 
      // the old CurrentItem).
      DataGridContext newCurrentContext = this.UpdateCurrentContextRecursive( this.DataGridContext,
                                            currentContext,
                                            out newCurrentItem );

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
        newCurrentContext.SetCurrent( newCurrentItem, null, null, currentContext.CurrentColumn, false, false, this.SynchronizeSelectionWithCurrent );
      }
    }

    private DataGridContext UpdateCurrentContextRecursive(
      DataGridContext parentDataGridContext,
      DataGridContext oldCurrentDataGridContext,
      out object newCurrentItem )
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

          System.Data.DataView dataView =
            ItemsSourceHelper.TryGetDataViewFromDataGridContext( childContext );

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

    #endregion SELECTION/CURRENT

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

      Xceed.Wpf.DataGrid.Views.ViewBase view = this.GetView();

      if( view.UseDefaultHeadersFooters )
        view.InvokeAddDefaultHeadersFooters();

      // Cache if the view requires to preserve container size
      this.ViewPreservesContainerSize = view.PreserveContainerSize;

      // Notify the template was reapplied
      if( this.TemplateApplied != null )
        this.TemplateApplied( this, EventArgs.Empty );

      this.UpdateDataGridAdorner();
    }

    private void UpdateDataGridAdorner()
    {
      Xceed.Wpf.DataGrid.Views.ViewBase view = this.GetView();
      bool needAdorner = !view.UseDefaultHeadersFooters;
      if( needAdorner && (m_mainAdorner == null) )
      {
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer( this );
        m_mainAdorner = new DataGridMainAdorner( this );
        adornerLayer.Add( m_mainAdorner );
      }
      else if( !needAdorner && ( m_mainAdorner != null ) )
      {
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer( this );
        adornerLayer.Remove( m_mainAdorner );
        m_mainAdorner = null;
      }
    }



    public void ExpandGroup( CollectionViewGroup group )
    {
      if( group == null )
        throw new ArgumentNullException( "group" );

      this.DataGridContext.ExpandGroup( group );
    }

    public void CollapseGroup( CollectionViewGroup group )
    {
      if( group == null )
        throw new ArgumentNullException( "group" );

      this.DataGridContext.CollapseGroup( group );
    }

    public void ToggleGroupExpansion( CollectionViewGroup group )
    {
      if( group == null )
        throw new ArgumentNullException( "group" );

      this.DataGridContext.ToggleGroupExpansion( group );
    }

    public bool IsGroupExpanded( CollectionViewGroup group )
    {
      if( group == null )
        throw new ArgumentNullException( "group" );

      return this.DataGridContext.IsGroupExpanded( group );
    }

    public void CollapseDetails( object dataItem )
    {
      //This function will work only for master items.
      //the DataGridContext and its generator will determine that the item is not present in the immediate structure
      //and throw if the item passed is not a master item.
      this.DataGridContext.CollapseDetails( dataItem );
    }

    public void ExpandDetails( object dataItem )
    {
      //This function will work only for master items.
      //the DataGridContext and its generator will determine that the item is not present in the immediate structure
      //and throw if the item passed is not a master item.
      this.DataGridContext.ExpandDetails( dataItem );
    }

    public void ToggleDetailExpansion( object dataItem )
    {
      //This function will work only for master items.
      //the DataGridContext and its generator will determine that the item is not present in the immediate structure
      //and throw if the item passed is not a master item.
      this.DataGridContext.ToggleDetailExpansion( dataItem );
    }

    public bool AreDetailsExpanded( object dataItem )
    {
      //This function will work only for master items.
      //In the cases where a Detail item is passed to the function, the function will return false.
      return this.DataGridContext.AreDetailsExpanded( dataItem );
    }

    internal DataGridContext GetChildContext( object parentItem, DetailConfiguration detailConfiguration )
    {
      return this.DataGridContext.GetChildContext( parentItem, detailConfiguration );
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

    protected override Size MeasureOverride( Size constraint )
    {
      //By default the measure pass must not be delayed, or if the grid is already loaded or if we are printing.
      if( ( !this.DeferInitialLayoutPass ) || ( this.IsLoaded ) || ( this.IsPrinting ) || ( !m_isFirstTimeLoaded ) )
        return base.MeasureOverride( constraint );

      return this.GetStartUpSize( constraint );
    }

    protected override Size ArrangeOverride( Size arrangeBounds )
    {
      //By default the measure pass must not be delayed, or if the grid is already loaded or if we are printing.
      if( ( !this.DeferInitialLayoutPass ) || ( this.IsLoaded ) || ( this.IsPrinting ) || ( !m_isFirstTimeLoaded ) )
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

      IDataGridItemContainer dataGridItemContainer = element as IDataGridItemContainer;
      Row row = element as Row;

      // Only preserve ContainerSizeState if necessary
      if( !this.IsBoundToDataGridVirtualizingCollectionViewBase
          && this.ViewPreservesContainerSize )
      {
        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( element );

        //Save the size state of the container (this will do nothing if no size has been set on the item ).
        dataGridContext.SaveContainerSizeState( item, feContainer );
      }

      // Try to get the element as a HeaderFooterItem to avoid
      // calling ClearContainer on a Row inside a HeaderFooterItem.
      // HeaderFooterItem implements IDataGridItemContainer and we call
      // ClearContainer on it if not null.
      HeaderFooterItem hfi = element as HeaderFooterItem;

      // If element is a HeaderFooterItem, then it cannot be a Row
      if( hfi != null )
      {
        row = HeaderFooterItem.FindIDataGridItemContainerInChildren( hfi, hfi.AsVisual() ) as Row;
      }

      // Special handling case for the Row class when it is not in a HeaderFooterItem
      if( row != null )
      {
        //cancel edit if applicable and preserve state of the row.
        if( row.IsBeingEdited )
        {
          RowState oldRowEditionState = m_currentRowInEditionState.Clone();

          Dictionary<ColumnBase, CellState> oldColumnsStates = new Dictionary<ColumnBase, CellState>();

          Debug.Assert( item == m_currentItemInEdition, "The current item being edited on the DataGridControl differs from the one of the currently edited container." );

          // We can use row because there is only one row in edition in the DataGridControl
          foreach( Cell cell in row.CreatedCells )
          {
            ColumnBase parentColumn = cell.ParentColumn;

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
              hfi.ClearContainer();
            }
            else
            {
              row.ClearContainer();
            }
          }

          m_currentRowInEditionState = oldRowEditionState;

          foreach( ColumnBase column in oldColumnsStates.Keys )
          {
            CellState oldCellState = oldColumnsStates[ column ];
            column.CurrentRowInEditionCellState = oldCellState;
          }

          m_currentItemInEdition = item;

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
              hfi.ClearContainer();
            }
            else
            {
              row.ClearContainer();
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

      UIElement uiElement = element as UIElement;

      if( ( uiElement != null ) && ( ( uiElement.IsKeyboardFocusWithin ) || ( uiElement.IsKeyboardFocused ) ) )
      {
        // If the element we are cleaning up contains the focus, we ensure to refocus the ScrollViewer to have
        // the focus on a valid element of the grid.
        this.DelayBringIntoViewAndFocusCurrent();
      }
    }

    protected override void PrepareContainerForItemOverride( DependencyObject element, object item )
    {
      base.PrepareContainerForItemOverride( element, item );

      //this is specific to the DataRows from the Original items list... do not want to do that in the
      //Row.PrepareContainer()

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
          //this condition is for Rows that are recycled from an environment where they had
          //and item container style to one that doesn't...
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
      if( !this.IsBoundToDataGridVirtualizingCollectionViewBase
          && this.ViewPreservesContainerSize )
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
        DataGridContext currentDataGridContext = this.CurrentContext;

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
              DependencyObject originalSource = e.OriginalSource as DependencyObject;

              Row row = CustomItemContainerGenerator.FindContainerFromChildOrRowSelectorOrSelf(
                this, originalSource ) as Row;

              if( ( row != null ) && ( !row.IsBeingEdited ) )
              {
                DataGridContext newDataGridContext = DataGridControl.GetDataGridContext( row );

                if( newDataGridContext != null )
                {
                  int sourceDataItemIndex = DataGridVirtualizingPanel.GetItemIndex( row );
                  object item = newDataGridContext.GetItemFromContainer( row );
                  Cell cell = originalSource as Cell;

                  if( ( cell == null ) || ( cell.ParentColumn.DataGridControl != this ) )
                  {
                    cell = Cell.FindFromChild( this, originalSource );
                  }

                  int columnIndex = ( cell == null ) ? -1 : cell.ParentColumn.VisiblePosition;

                  m_selectionChangerManager.UpdateSelection(
                    this.CurrentContext, this.CurrentItem, this.CurrentColumn,
                    newDataGridContext, true, row.IsBeingEdited, sourceDataItemIndex, item, columnIndex,
                    SelectionManager.UpdateSelectionSource.SpaceDown );

                  e.Handled = true;
                }
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
        m_mouseDownUpdateSelectionSource = this.SelectionChangerManager.PushUpdateSelectionSource(
          SelectionManager.UpdateSelectionSource.MouseDown );
      }

      m_mouseDownCurrentDataGridContext = this.CurrentContext;
      m_mouseDownCurrentItem = this.CurrentItem;
      m_mouseDownCurrentColumn = this.CurrentColumn;

      base.OnPreviewMouseDown( e );

      if( e.Handled )
        return;

      DataGridContext oldDataGridContext = this.CurrentContext;
      Row oldRow = ( oldDataGridContext == null ) ? null : oldDataGridContext.CurrentRow;

      UIElement newContainer = CustomItemContainerGenerator.FindContainerFromChildOrRowSelectorOrSelf(
        this, e.OriginalSource as DependencyObject );

      Row newRow = Row.FromContainer( newContainer );

      // testing newContainer == null is a way to not end edition when moving
      // the scroll bar around.
      if( ( oldRow == null ) || ( newContainer == null ) || ( !oldRow.IsBeingEdited ) || ( newRow == oldRow ) )
        return;

      ColumnBase oldCurrentColumn = oldDataGridContext.CurrentColumn;

      try
      {
        // If the source is reset during the EndEdit, we move the focus temporarely on the ScrollViewer to prevent some issue
        // when the EndEdit cause a Reset (The ScrollViewer was trying to bring into view the old editing Cell).
        bool focused = this.ForceFocus( this.ScrollViewer );

        if( !focused )
          return;

        using( this.InhibitSetFocus() )
        {
          oldRow.EndEdit();

          // If the source was reset during the EndEdit, Ensure to relayout right away.
          // So the mouse down will continue on the new layouted element.
          this.UpdateLayout();
        }
      }
      catch( DataGridException )
      {
        // We swallow exception if it occurs because of a validation error or Cell was read-only or
        // any other GridException.
        if( m_mouseDownUpdateSelectionSource != null )
        {
          m_mouseDownUpdateSelectionSource.Dispose();
          m_mouseDownUpdateSelectionSource = null;
        }

        this.SetFocusHelper( oldRow, oldCurrentColumn, false, true );
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

      if( e.ChangedButton == MouseButton.Left )
      {
        DependencyObject originalSource = e.OriginalSource as DependencyObject;

        Row row = CustomItemContainerGenerator.FindContainerFromChildOrRowSelectorOrSelf(
          this, originalSource ) as Row;

        if( row == null || ( !row.IsKeyboardFocused && !row.IsKeyboardFocusWithin ) )
          return;

        DataGridContext newDataGridContext = DataGridControl.GetDataGridContext( row );

        if( newDataGridContext == null )
          return;

        int sourceDataItemIndex = DataGridVirtualizingPanel.GetItemIndex( row );
        object item = newDataGridContext.GetItemFromContainer( row );
        Cell cell = originalSource as Cell;

        if( ( cell == null ) || ( cell.ParentColumn.DataGridControl != this ) )
        {
          cell = Cell.FindFromChild( this, originalSource );

          if( ( cell != null ) && ( this.SelectionUnit == SelectionUnit.Cell ) && ( !cell.IsKeyboardFocused ) && ( !cell.IsKeyboardFocusWithin ) )
            return;
        }

        int columnIndex = ( cell == null ) ? -1 : cell.ParentColumn.VisiblePosition;

        // If we have an error during the EndEdit that prevent the PreviewMouseDown from occuring 
        // (since we put e.Handled = true if an exception occurs), 
        // the PreviewMouseUp will not occur, so it is safe to call UpdateSelection().
        m_selectionChangerManager.UpdateSelection(
          m_mouseDownCurrentDataGridContext, m_mouseDownCurrentItem, m_mouseDownCurrentColumn,
          newDataGridContext, true, row.IsBeingEdited, sourceDataItemIndex, item, columnIndex,
          SelectionManager.UpdateSelectionSource.MouseUp );
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

      if( e.Handled )
        return;

      if( this.InhibitPreviewGotKeyboardFocus )
        return;

      DependencyObject newFocus = e.NewFocus as DependencyObject;

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
        DataGridContext currentDataGridContext = this.CurrentContext;
        object internalCurrentItem = currentDataGridContext.InternalCurrentItem;
        UIElement itemUIElement = null;

        if( ( internalCurrentItem == null ) && ( currentDataGridContext.Items.Count > 0 ) )
        {
          //if InternalCurrentItem is null, then soft-select the first item.
          internalCurrentItem = currentDataGridContext.Items.GetItemAt( 0 );

          if( internalCurrentItem != null )
          {
            using( this.InhibitSetFocus() )
            {
              currentDataGridContext.SetCurrent( internalCurrentItem, null, 0, currentDataGridContext.CurrentColumn, false, false, this.SynchronizeSelectionWithCurrent );
            }
          }
        }

        if( internalCurrentItem != null )
        {
          //try to fetch the Container for the Internal current item
          itemUIElement = currentDataGridContext.GetContainerFromItem( internalCurrentItem ) as UIElement;

          //if container exists, then it's visible (or close to it), try to focus it 
          if( itemUIElement == null )
          {
            currentDataGridContext.DelayBringIntoViewAndFocusCurrent();

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
      bool oldFocusWasInTheGrid = ( e.OldFocus != null ) && ( TreeHelper.IsDescendantOf( e.OldFocus as DependencyObject, this, false ) );
      DataGridContext oldCurrentDataGridContext = this.CurrentContext;

      object oldCurrentItem;
      ColumnBase oldCurrentColumn;
      Row oldRow;

      if( oldCurrentDataGridContext == null )
      {
        oldCurrentItem = null;
        oldCurrentColumn = null;
        oldRow = null;
      }
      else
      {
        oldCurrentItem = ( oldCurrentDataGridContext == null ) ? null : oldCurrentDataGridContext.CurrentItem;
        oldCurrentColumn = ( oldCurrentDataGridContext == null ) ? null : oldCurrentDataGridContext.CurrentColumn;
        oldRow = ( oldCurrentDataGridContext == null ) ? null : oldCurrentDataGridContext.CurrentRow;
      }

      FrameworkElement newContainer = CustomItemContainerGenerator.FindContainerFromChildOrRowSelectorOrSelf( this, newFocus );

      DataGridContext newDataGridContext = ( newContainer == null )
        ? null : DataGridControl.GetDataGridContext( newContainer );

      Row newRow = Row.FromContainer( newContainer );
      Cell newCell = newFocus as Cell;

      if( ( newCell == null ) || ( newCell.ParentColumn.DataGridControl != this ) )
      {
        newCell = Cell.FindFromChild( this, newFocus );
      }

      ColumnBase newColumn = ( newCell == null ) ? null : newCell.ParentColumn;
      object item = ( newContainer == null ) ? null : newDataGridContext.GetItemFromContainer( newContainer );
      bool containerChangedDuringEndEdit = false;

      // Prevent the focus to be moved during the EndEdit since we are changing focus at the moment
      using( this.InhibitSetFocus() )
      {
        if( ( oldRow != null ) && ( oldRow.IsBeingEdited ) )
        {
          Cell oldCell = ( oldCurrentDataGridContext == null ) ? null : oldCurrentDataGridContext.CurrentCell;

          if( oldRow != newRow )
          {
            try
            {
              FrameworkElement previousNewContainer = newContainer;
              oldRow.EndEdit();

              // We must refetch the container for the current item in case EndEdit triggers a reset on the 
              // CustomItemContainerGenerator and remap this item to a container other than the one previously
              // fetched
              this.UpdateLayout();

              newContainer = ( item == null ) ?
                null : newDataGridContext.GetContainerFromItem( item ) as FrameworkElement;

              newRow = Row.FromContainer( newContainer );
              newCell = ( newRow == null ) ? null : newRow.Cells[ newColumn ];

              if( previousNewContainer != newContainer )
              {
                e.Handled = true;
                containerChangedDuringEndEdit = true;
              }
              else
              {
                // If the newFocus is not in the the grid anymore, stop the focus.
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

      if( newCell != null )
      {
        IDisposable inhibitSetFocus = null;

        if( !containerChangedDuringEndEdit )
        {
          // Prevent the focus to be moved during the EndEdit since we are changing focus at the moment
          inhibitSetFocus = this.InhibitSetFocus();
        }

        try
        {
          Cell oldFocusedCell = e.OldFocus as Cell;
          UIElement newFocusedElement = e.NewFocus as UIElement;

          // here we check if the oldFocus is the parent of the new focused element because the OnPreviewGotKeyboardFocus
          // is called twice if it's the case which give twice the focus and then endsup unselecting the element during the mouseUp 
          // because during the second updateSelection, the element is already contained then m_updateSelectionOnNextMouseUp set to true
          bool isNewFocusParentTheOldFocusedCell = ( ( oldFocusedCell != null ) && ( newFocusedElement != null ) ) ?
                                ( oldFocusedCell != Cell.FindFromChild( this, newFocusedElement ) ) : true;

          int sourceDataItemIndex = DataGridVirtualizingPanel.GetItemIndex( newRow );

          // newCell != newFocus is in fact a way to test if newFocus is a child of the cell.
          if( ( newFocus != null ) && ( newCell != newFocus ) && ( !newCell.IsBeingEdited ) && ( newCell.IsCellEditorDisplayed ) )
          {
            newCell.BeginEdit();
          }
          else if( !newCell.IsCurrent )
          {
            //set the current row/column as this cell's
            newDataGridContext.SetCurrent(
              item, newRow,
              sourceDataItemIndex,
              newColumn, false, true, false );
          }

          if( isNewFocusParentTheOldFocusedCell )
          {
            bool rowIsBeingEditedAndCurrentRowNotChanged = this.IsRowBeingEditedAndCurrentRowNotChanged( newRow, oldRow );

            m_selectionChangerManager.UpdateSelection(
              oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumn,
              newDataGridContext, oldFocusWasInTheGrid, rowIsBeingEditedAndCurrentRowNotChanged,
              sourceDataItemIndex, item, newCell.ParentColumn.VisiblePosition, null );
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
          ColumnBase currentColumn = null;

          //if the current column need to be propagated
          if( this.NavigationBehavior == NavigationBehavior.CellOnly )
          {
            currentColumn = newDataGridContext.CurrentColumn;

            //if there is no current column,
            if( currentColumn == null )
            {
              //then use the first visible column
              ReadOnlyObservableCollection<ColumnBase> columns = newDataGridContext.VisibleColumns;

              if( columns.Count > 0 )
              {

                int firstFocusableColumn = DataGridScrollViewer.GetFirstVisibleFocusableColumnIndex( newDataGridContext, newRow );

                if( firstFocusableColumn < 0 )
                  throw new DataGridException( "Trying to edit while no cell is focusable. " );

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
              newDataGridContext.SetCurrent( item, newRow, sourceDataItemIndex, null, true, true, false );
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
            newDataGridContext.SetCurrent( item, newRow, sourceDataItemIndex, currentColumn, true, true, false );
          }

          int columnIndex = ( currentColumn == null ) ? -1 : currentColumn.VisiblePosition;

          bool rowIsBeingEditedAndCurrentRowNotChanged = this.IsRowBeingEditedAndCurrentRowNotChanged( newRow, oldRow );

          m_selectionChangerManager.UpdateSelection(
            oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumn,
            newDataGridContext, oldFocusWasInTheGrid, rowIsBeingEditedAndCurrentRowNotChanged,
            sourceDataItemIndex, item, columnIndex, null );
        }
        catch( DataGridException )
        {
          e.Handled = true;
        }

        return;
      }

      if( newContainer != null )
      {
        HeaderFooterItem headerFooterItem = newContainer as HeaderFooterItem;

        if( headerFooterItem == null )
          return;

        IDisposable inhibitSetFocus = null;

        if( !containerChangedDuringEndEdit )
        {
          inhibitSetFocus = this.InhibitSetFocus();
        }

        try
        {
          ColumnBase currentColumn = newDataGridContext.CurrentColumn;
          newDataGridContext.SetCurrent( item, null, -1, currentColumn, true, true, false );

          int columnIndex = ( currentColumn == null ) ? -1 : currentColumn.VisiblePosition;

          m_selectionChangerManager.UpdateSelection(
            oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumn,
            newDataGridContext, oldFocusWasInTheGrid, false, -1, item, columnIndex, null );
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

    protected override AutomationPeer OnCreateAutomationPeer()
    {
      return new DataGridControlAutomationPeer( this );
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

      Debug.Assert( !this.InhibitPreviewGotKeyboardFocus );
      this.InhibitPreviewGotKeyboardFocus = true;

      try
      {
        return element.Focus();
      }
      finally
      {
        this.InhibitPreviewGotKeyboardFocus = false;
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
      this.PrepareContainerForItemOverride( container, item );
    }

    internal void ClearItemContainer( DependencyObject container, object item )
    {
      DataGridControl.ClearContainer( container );
      this.ClearContainerForItemOverride( container, item );
    }

    internal DependencyObject CreateContainerForItem()
    {
      DependencyObject container = this.GetContainerForItemOverride();

      if( container != null )
      {

        // Set a local value for containers to ensure the 
        // DefaultStyleKey is correctly applied on this container
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
      m_customItemContainerGenerator.CleanupGenerator( false );
      this.SetValue( ParentDataGridControlPropertyKey, null );
    }

    internal void QueueDataGridContextPeerChlidrenRefresh( DataGridContext dataGridContext )
    {
      if( dataGridContext.Peer == null )
        return;

      if( m_dataGridContextToRefreshPeerChlidren == null )
      {
        m_dataGridContextToRefreshPeerChlidren = new HashSet<DataGridContext>();
        m_dataGridContextToRefreshPeerChlidren.Add( dataGridContext );
      }
      else
      {
        if( !m_dataGridContextToRefreshPeerChlidren.Contains( dataGridContext ) )
        {
          m_dataGridContextToRefreshPeerChlidren.Add( dataGridContext );
        }
      }
    }

    internal void SynchronizeForeignKeyConfigurations()
    {
      ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurations(
        this.Columns.FieldNameToColumnDictionary,
        this.ItemsSource,
        this.DataGridContext.ItemsSourceFieldDescriptors,
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

        dataGrid.ClearItemContainer( element, GetFixedItem( element ) );
        DataGridControl.SetDataGridContext( element, null );
      }

      targetPanel.Children.Clear();

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
      if( this.PropertyChanged != null )
        this.PropertyChanged( this, e );
    }

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( FrameworkElementUnloadedEventManager ) )
      {
        this.UnhookToUnloaded();
        this.SaveDataGridContextState( this.DataGridContext, true, int.MaxValue );
        this.CleanUpAfterUnload();
        return true;
      }

      return false;
    }

    #endregion

    #region IDocumentPaginatorSource Members

    DocumentPaginator IDocumentPaginatorSource.DocumentPaginator
    {
      get
      {
        return this.CreateDocumentPaginator();
      }
    }

    protected virtual DocumentPaginator CreateDocumentPaginator()
    {
      return null;
    }

    #endregion

    private IDisposable m_columnsInitDisposable; // = null

    private int m_beginInitCount; // = 0

    private Cursor m_oldCursor; // = null;
    private bool? m_oldForceCursor; // = null;
    private int m_waitCursorCount; // = 0;

    private FrameworkElement m_itemsHost; // = null
    private CustomItemContainerGenerator m_customItemContainerGenerator; // = null

    private object m_currentItemInEdition; // = null
    private RowState m_currentRowInEditionState; // = null

    private int m_inhibitBringIntoView; // = 0
    private int m_inhibitSetFocus; // = 0
    private DispatcherOperation m_queueBringIntoView; // = null
    private DispatcherOperation m_queueSetFocus; // = null
    private DispatcherOperation m_queueClearCurrentColumn; // = null

    private Dictionary<WeakDataGridContextKey, SaveRestoreDataGridContextStateVisitor> m_dataGridContextsStateDictionary;
    private SelectionManager m_selectionChangerManager;
    private HashSet<DataGridContext> m_dataGridContextToRefreshPeerChlidren;

    private CommandBinding m_copyCommandBinding; // = null;
    private CommandBinding m_deleteCommandBinding; // = null;
    private CommandBinding m_refreshCommandBinding; // = null;
    private IDisposable m_mouseDownUpdateSelectionSource;

    private DataGridContext m_mouseDownCurrentDataGridContext;
    private object m_mouseDownCurrentItem;
    private ColumnBase m_mouseDownCurrentColumn;

    private bool m_isFirstTimeLoaded = true;

    private WeakReference m_parentWindow;

    private BitVector32 m_flags = new BitVector32();

    private EventHandler m_proxyCollectionRefreshHandler;
    private NotifyCollectionChangedEventHandler m_proxyGroupDescriptionsChangedHandler;
    private NotifyCollectionChangedEventHandler m_proxySortDescriptionsChangedHandler;
    private NotifyCollectionChangedEventHandler m_proxyAutoFilterValuesChangedHandler;
    private EventHandler m_proxyApplyingFilterCriteriasHandler;
    private EventHandler m_connectionStateChangedHandler;
    private EventHandler m_connectionStateErrorHandler;
    private NotifyCollectionChangedEventHandler m_dataGridDetailDescriptionsChangedHandler;
    private EventHandler m_rootGroupChangedHandler;

    [Flags]
    private enum DataGridControlFlags
    {
      SettingFocusOnCell = 1,
      IsSetCurrentInProgress = 2,
      UsePrintView = 4,
      IsBoundToDataGridVirtualizingCollectionViewBase = 8,
      ViewPreservesContainerSize = 16,
      InhibitPreviewGotKeyboardFocus = 32,
      DebugSaveRestore = 64,
      ItemsSourceChangedDelayed = 128,
      SelectedIndexPropertyNeedCoerce = 256,
      SelectedItemPropertyNeedCoerce = 512
    }

    #region GridControlQueueInhibiter Private Class

    private sealed class GridControlQueueInhibiter : IDisposable
    {
      public GridControlQueueInhibiter( DataGridControl dataGrid )
      {
        if( dataGrid == null )
        {
          throw new ArgumentNullException( "dataGrid" );
        }

        m_gridControl = dataGrid;
        m_gridControl.m_inhibitBringIntoView++;
      }

      #region IDisposable Members

      public void Dispose()
      {
        m_gridControl.m_inhibitBringIntoView--;
      }

      #endregion

      private DataGridControl m_gridControl; // = null
    }

    #endregion

    #region DataGridMainAdorner Private Class

    private DataGridMainAdorner m_mainAdorner;
    private class DataGridMainAdorner : Adorner
    {
      static DataGridMainAdorner()
      {
        string c = Char.ConvertFromUtf32( 169 );
        m_sDisplayText = new FormattedText( c + " \u0058\u0063\u0065\u0065\u0064",
          System.Globalization.CultureInfo.InvariantCulture,
          FlowDirection.LeftToRight,
          new Typeface( "Verdana" ),
          12, new SolidColorBrush( Color.FromArgb( 150, 250, 100, 0 ) ) );
      }

      private static FormattedText m_sDisplayText;

      public DataGridMainAdorner( UIElement adornedElement )
        : base( adornedElement )
      {
        this.IsHitTestVisible = false;
      }

      protected override void OnRender( System.Windows.Media.DrawingContext drawingContext )
      {
        base.OnRender( drawingContext );

        drawingContext.PushTransform( new RotateTransform( -90 ) );
        drawingContext.PushTransform( new TranslateTransform( -this.ActualHeight, 0d ) );
        drawingContext.DrawText( m_sDisplayText, new System.Windows.Point( 32, 2 ) );
        drawingContext.Pop();
        drawingContext.Pop();
      }
    }

    #endregion

    #region DataGridControlSetFocusInhibiter Private Class

    private sealed class DataGridControlSetFocusInhibiter : IDisposable
    {
      public DataGridControlSetFocusInhibiter( DataGridControl dataGrid )
      {
        if( dataGrid == null )
          throw new ArgumentNullException( "dataGrid" );

        m_gridControl = dataGrid;
        m_gridControl.m_inhibitSetFocus++;
      }

      #region IDisposable Members

      public void Dispose()
      {
        m_gridControl.m_inhibitSetFocus--;
      }

      #endregion

      private DataGridControl m_gridControl; // = null
    }

    #endregion
  }
}
