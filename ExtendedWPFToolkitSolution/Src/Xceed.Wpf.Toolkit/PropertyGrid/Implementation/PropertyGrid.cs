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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.PropertyGrid.Commands;
using System.Collections.Specialized;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Collections.ObjectModel;
using System.Collections;
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Reflection;
using System.Linq.Expressions;
using System.Windows.Markup;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  [TemplatePart( Name = PART_DragThumb, Type = typeof( Thumb ) )]
  [TemplatePart( Name = PART_PropertyItemsControl, Type = typeof( PropertyItemsControl ) )]
  [StyleTypedProperty( Property = "PropertyContainerStyle", StyleTargetType = typeof( PropertyItemBase ) )]
  public class PropertyGrid : Control, ISupportInitialize, IPropertyContainer, INotifyPropertyChanged
  {
    private const string PART_DragThumb = "PART_DragThumb";
    internal const string PART_PropertyItemsControl = "PART_PropertyItemsControl";
    private static readonly ComponentResourceKey SelectedObjectAdvancedOptionsMenuKey = new ComponentResourceKey( typeof( PropertyGrid ), "SelectedObjectAdvancedOptionsMenu" );

    #region Members

    private Thumb _dragThumb;
    private bool _hasPendingSelectedObjectChanged;
    private int _initializationCount;
    private ContainerHelperBase _containerHelper;
    private PropertyDefinitionCollection _propertyDefinitions;
    private EditorDefinitionCollection _editorDefinitions;
    private WeakEventListener<NotifyCollectionChangedEventArgs> _propertyDefinitionsListener;
    private WeakEventListener<NotifyCollectionChangedEventArgs> _editorDefinitionsListener;

    #endregion //Members

    #region Properties

    #region AdvancedOptionsMenu

    public static readonly DependencyProperty AdvancedOptionsMenuProperty = DependencyProperty.Register( "AdvancedOptionsMenu", typeof( ContextMenu ), typeof( PropertyGrid ), new UIPropertyMetadata( null ) );
    public ContextMenu AdvancedOptionsMenu
    {
      get
      {
        return ( ContextMenu )GetValue( AdvancedOptionsMenuProperty );
      }
      set
      {
        SetValue( AdvancedOptionsMenuProperty, value );
      }
    }

    #endregion //AdvancedOptionsMenu

    #region AutoGenerateProperties

    public static readonly DependencyProperty AutoGeneratePropertiesProperty = DependencyProperty.Register( "AutoGenerateProperties", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( true ) );
    public bool AutoGenerateProperties
    {
      get
      {
        return ( bool )GetValue( AutoGeneratePropertiesProperty );
      }
      set
      {
        SetValue( AutoGeneratePropertiesProperty, value );
      }
    }

    #endregion //AutoGenerateProperties

    #region ShowSummary

    public static readonly DependencyProperty ShowSummaryProperty = DependencyProperty.Register( "ShowSummary", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( true ) );
    public bool ShowSummary
    {
      get
      {
        return ( bool )GetValue( ShowSummaryProperty );
      }
      set
      {
        SetValue( ShowSummaryProperty, value );
      }
    }

    #endregion //ShowSummary

    #region EditorDefinitions

    public EditorDefinitionCollection EditorDefinitions
    {
      get
      {
        return _editorDefinitions;
      }
      set
      {
        if( _editorDefinitions != value )
        {
          EditorDefinitionCollection oldValue = _editorDefinitions;
          _editorDefinitions = value;
          this.OnEditorDefinitionsChanged( oldValue, value );
        }
      }
    }

    protected virtual void OnEditorDefinitionsChanged( EditorDefinitionCollection oldValue, EditorDefinitionCollection newValue )
    {
      if( oldValue != null )
        CollectionChangedEventManager.RemoveListener( oldValue, _editorDefinitionsListener );

      if( newValue != null )
        CollectionChangedEventManager.AddListener( newValue, _editorDefinitionsListener );

      this.Notify( this.PropertyChanged, () => this.EditorDefinitions );
    }

    private void OnEditorDefinitionsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      _containerHelper.NotifyEditorDefinitionsCollectionChanged();
    }

    #endregion //EditorDefinitions

    #region Filter

    public static readonly DependencyProperty FilterProperty = DependencyProperty.Register( "Filter", typeof( string ), typeof( PropertyGrid ), new UIPropertyMetadata( null, OnFilterChanged ) );
    public string Filter
    {
      get
      {
        return ( string )GetValue( FilterProperty );
      }
      set
      {
        SetValue( FilterProperty, value );
      }
    }

    private static void OnFilterChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PropertyGrid propertyGrid = o as PropertyGrid;
      if( propertyGrid != null )
        propertyGrid.OnFilterChanged( ( string )e.OldValue, ( string )e.NewValue );
    }

    protected virtual void OnFilterChanged( string oldValue, string newValue )
    {
      // The Filter property affects the resulting FilterInfo of IPropertyContainer. Raise an event corresponding
      // to this property.
      this.Notify( this.PropertyChanged, () => ( ( IPropertyContainer )this ).FilterInfo );
    }

    #endregion //Filter

    #region FilterWatermark

    public static readonly DependencyProperty FilterWatermarkProperty = DependencyProperty.Register( "FilterWatermark", typeof( string ), typeof( PropertyGrid ), new UIPropertyMetadata( "Search" ) );
    public string FilterWatermark
    {
      get
      {
        return ( string )GetValue( FilterWatermarkProperty );
      }
      set
      {
        SetValue( FilterWatermarkProperty, value );
      }
    }

    #endregion //FilterWatermark

    #region IsCategorized

    public static readonly DependencyProperty IsCategorizedProperty = DependencyProperty.Register( "IsCategorized", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( true, OnIsCategorizedChanged ) );
    public bool IsCategorized
    {
      get
      {
        return ( bool )GetValue( IsCategorizedProperty );
      }
      set
      {
        SetValue( IsCategorizedProperty, value );
      }
    }

    private static void OnIsCategorizedChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PropertyGrid propertyGrid = o as PropertyGrid;
      if( propertyGrid != null )
        propertyGrid.OnIsCategorizedChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnIsCategorizedChanged( bool oldValue, bool newValue )
    {
      this.UpdateThumb();
    }

    #endregion //IsCategorized




















    #region NameColumnWidth

    public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register( "NameColumnWidth", typeof( double ), typeof( PropertyGrid ), new UIPropertyMetadata( 150.0, OnNameColumnWidthChanged ) );
    public double NameColumnWidth
    {
      get
      {
        return ( double )GetValue( NameColumnWidthProperty );
      }
      set
      {
        SetValue( NameColumnWidthProperty, value );
      }
    }

    private static void OnNameColumnWidthChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PropertyGrid propertyGrid = o as PropertyGrid;
      if( propertyGrid != null )
        propertyGrid.OnNameColumnWidthChanged( ( double )e.OldValue, ( double )e.NewValue );
    }

    protected virtual void OnNameColumnWidthChanged( double oldValue, double newValue )
    {
      if( _dragThumb != null )
        ( ( TranslateTransform )_dragThumb.RenderTransform ).X = newValue;
    }

    #endregion //NameColumnWidth

    #region Properties

    public IList Properties
    {
      get
      {
        return _containerHelper.Properties;
      }
    }

    #endregion //Properties








    #region PropertyContainerStyle

    /// <summary>
    /// Identifies the PropertyContainerStyle dependency property
    /// </summary>
    public static readonly DependencyProperty PropertyContainerStyleProperty =
        DependencyProperty.Register( "PropertyContainerStyle", typeof( Style ), typeof( PropertyGrid ), new UIPropertyMetadata( null, OnPropertyContainerStyleChanged ) );

    /// <summary>
    /// Gets or sets the style that will be applied to all PropertyItemBase instances displayed in the property grid.
    /// </summary>
    public Style PropertyContainerStyle
    {
      get { return ( Style )GetValue( PropertyContainerStyleProperty ); }
      set { SetValue( PropertyContainerStyleProperty, value ); }
    }

    private static void OnPropertyContainerStyleChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      var owner = o as PropertyGrid;
      if( owner != null )
        owner.OnPropertyContainerStyleChanged( ( Style )e.OldValue, ( Style )e.NewValue );
    }

    protected virtual void OnPropertyContainerStyleChanged( Style oldValue, Style newValue )
    {
    }

    #endregion //PropertyContainerStyle

    #region PropertyDefinitions

    public PropertyDefinitionCollection PropertyDefinitions
    {
      get
      {
        return _propertyDefinitions;
      }
      set
      {
        if( _propertyDefinitions != value )
        {
          PropertyDefinitionCollection oldValue = _propertyDefinitions;
          _propertyDefinitions = value;
          this.OnPropertyDefinitionsChanged( oldValue, value );
        }
      }
    }

    protected virtual void OnPropertyDefinitionsChanged( PropertyDefinitionCollection oldValue, PropertyDefinitionCollection newValue )
    {
      if( oldValue != null )
        CollectionChangedEventManager.RemoveListener( oldValue, _propertyDefinitionsListener );

      if( newValue != null )
        CollectionChangedEventManager.AddListener( newValue, _propertyDefinitionsListener );

      this.Notify( this.PropertyChanged, () => this.PropertyDefinitions );
    }

    private void OnPropertyDefinitionsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      _containerHelper.NotifyPropertyDefinitionsCollectionChanged();
    }

    #endregion //PropertyDefinitions

    #region IsReadOnly

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register( "IsReadOnly", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( false ) );
    public bool IsReadOnly
    {
      get
      {
        return ( bool )GetValue( IsReadOnlyProperty );
      }
      set
      {
        SetValue( IsReadOnlyProperty, value );
      }
    }

    #endregion //ReadOnly

    #region SelectedObject

    public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register( "SelectedObject", typeof( object ), typeof( PropertyGrid ), new UIPropertyMetadata( null, OnSelectedObjectChanged ) );
    public object SelectedObject
    {
      get
      {
        return ( object )GetValue( SelectedObjectProperty );
      }
      set
      {
        SetValue( SelectedObjectProperty, value );
      }
    }

    private static void OnSelectedObjectChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PropertyGrid propertyInspector = o as PropertyGrid;
      if( propertyInspector != null )
        propertyInspector.OnSelectedObjectChanged( ( object )e.OldValue, ( object )e.NewValue );
    }

    protected virtual void OnSelectedObjectChanged( object oldValue, object newValue )
    {
      // We do not want to process the change now if the grid is initializing (ie. BeginInit/EndInit).
      if( _initializationCount != 0 )
      {
        _hasPendingSelectedObjectChanged = true;
        return;
      }

      this.UpdateContainerHelper();

      RaiseEvent( new RoutedPropertyChangedEventArgs<object>( oldValue, newValue, PropertyGrid.SelectedObjectChangedEvent ) );
    }

    #endregion //SelectedObject

    #region SelectedObjectType

    public static readonly DependencyProperty SelectedObjectTypeProperty = DependencyProperty.Register( "SelectedObjectType", typeof( Type ), typeof( PropertyGrid ), new UIPropertyMetadata( null, OnSelectedObjectTypeChanged ) );
    public Type SelectedObjectType
    {
      get
      {
        return ( Type )GetValue( SelectedObjectTypeProperty );
      }
      set
      {
        SetValue( SelectedObjectTypeProperty, value );
      }
    }

    private static void OnSelectedObjectTypeChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PropertyGrid propertyGrid = o as PropertyGrid;
      if( propertyGrid != null )
        propertyGrid.OnSelectedObjectTypeChanged( ( Type )e.OldValue, ( Type )e.NewValue );
    }

    protected virtual void OnSelectedObjectTypeChanged( Type oldValue, Type newValue )
    {
    }

    #endregion //SelectedObjectType

    #region SelectedObjectTypeName

    public static readonly DependencyProperty SelectedObjectTypeNameProperty = DependencyProperty.Register( "SelectedObjectTypeName", typeof( string ), typeof( PropertyGrid ), new UIPropertyMetadata( string.Empty ) );
    public string SelectedObjectTypeName
    {
      get
      {
        return ( string )GetValue( SelectedObjectTypeNameProperty );
      }
      set
      {
        SetValue( SelectedObjectTypeNameProperty, value );
      }
    }

    #endregion //SelectedObjectTypeName

    #region SelectedObjectName

    public static readonly DependencyProperty SelectedObjectNameProperty = DependencyProperty.Register( "SelectedObjectName", typeof( string ), typeof( PropertyGrid ), new UIPropertyMetadata( string.Empty, OnSelectedObjectNameChanged, OnCoerceSelectedObjectName ) );
    public string SelectedObjectName
    {
      get
      {
        return ( string )GetValue( SelectedObjectNameProperty );
      }
      set
      {
        SetValue( SelectedObjectNameProperty, value );
      }
    }

    private static object OnCoerceSelectedObjectName( DependencyObject o, object baseValue )
    {
      PropertyGrid propertyGrid = o as PropertyGrid;
      if( propertyGrid != null )
      {
        if( (propertyGrid.SelectedObject is FrameworkElement) && ( String.IsNullOrEmpty( ( String )baseValue ) ))
          return "<no name>";
      }

      return baseValue;
    }

    private static void OnSelectedObjectNameChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PropertyGrid propertyGrid = o as PropertyGrid;
      if( propertyGrid != null )
        propertyGrid.SelectedObjectNameChanged( ( string )e.OldValue, ( string )e.NewValue );
    }

    protected virtual void SelectedObjectNameChanged( string oldValue, string newValue )
    {
    }

    #endregion //SelectedObjectName











    #region SelectedPropertyItem

    private static readonly DependencyPropertyKey SelectedPropertyItemPropertyKey = DependencyProperty.RegisterReadOnly( "SelectedPropertyItem", typeof( PropertyItemBase ), typeof( PropertyGrid ), new UIPropertyMetadata( null, OnSelectedPropertyItemChanged ) );
    public static readonly DependencyProperty SelectedPropertyItemProperty = SelectedPropertyItemPropertyKey.DependencyProperty;
    public PropertyItemBase SelectedPropertyItem
    {
      get
      {
        return ( PropertyItemBase )GetValue( SelectedPropertyItemProperty );
      }
      internal set
      {
        SetValue( SelectedPropertyItemPropertyKey, value );
      }
    }

    private static void OnSelectedPropertyItemChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PropertyGrid propertyGrid = o as PropertyGrid;
      if( propertyGrid != null )
        propertyGrid.OnSelectedPropertyItemChanged( ( PropertyItemBase )e.OldValue, ( PropertyItemBase )e.NewValue );
    }

    protected virtual void OnSelectedPropertyItemChanged( PropertyItemBase oldValue, PropertyItemBase newValue )
    {
      if( oldValue != null )
        oldValue.IsSelected = false;

      if( newValue != null )
        newValue.IsSelected = true;

      this.SelectedProperty = ( newValue != null ) ? _containerHelper.ItemFromContainer( newValue ) : null;

      RaiseEvent( new RoutedPropertyChangedEventArgs<PropertyItemBase>( oldValue, newValue, PropertyGrid.SelectedPropertyItemChangedEvent ) );
    }

    #endregion //SelectedPropertyItem

    #region SelectedProperty

    /// <summary>
    /// Identifies the SelectedProperty dependency property
    /// </summary>
    public static readonly DependencyProperty SelectedPropertyProperty =
        DependencyProperty.Register( "SelectedProperty", typeof( object ), typeof( PropertyGrid ), new UIPropertyMetadata( null, OnSelectedPropertyChanged ) );

    /// <summary>
    /// Gets or sets the selected property or returns null if the selection is empty.
    /// </summary>
    public object SelectedProperty
    {
      get { return ( object )GetValue( SelectedPropertyProperty ); }
      set { SetValue( SelectedPropertyProperty, value ); }
    }

    private static void OnSelectedPropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args )
    {
      PropertyGrid propertyGrid = sender as PropertyGrid;
      if( propertyGrid != null )
      {
        propertyGrid.OnSelectedPropertyChanged( ( object )args.OldValue, ( object )args.NewValue );
      }
    }

    private void OnSelectedPropertyChanged( object oldValue, object newValue )
    {
      // Do not update the SelectedPropertyItem if the Current SelectedPropertyItem
      // item is the same as the new SelectedProperty. There may be 
      // duplicate items and the result could be to change the selection to the wrong item.
      object currentSelectedProperty = _containerHelper.ItemFromContainer( this.SelectedPropertyItem );
      if( !object.Equals( currentSelectedProperty, newValue ) )
      {
        this.SelectedPropertyItem = _containerHelper.ContainerFromItem( newValue );
      }
    }

    #endregion //SelectedProperty

    #region ShowAdvancedOptions

    public static readonly DependencyProperty ShowAdvancedOptionsProperty = DependencyProperty.Register( "ShowAdvancedOptions", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( false ) );
    public bool ShowAdvancedOptions
    {
      get
      {
        return ( bool )GetValue( ShowAdvancedOptionsProperty );
      }
      set
      {
        SetValue( ShowAdvancedOptionsProperty, value );
      }
    }

    #endregion //ShowAdvancedOptions

    #region ShowSearchBox

    public static readonly DependencyProperty ShowSearchBoxProperty = DependencyProperty.Register( "ShowSearchBox", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( true ) );
    public bool ShowSearchBox
    {
      get
      {
        return ( bool )GetValue( ShowSearchBoxProperty );
      }
      set
      {
        SetValue( ShowSearchBoxProperty, value );
      }
    }

    #endregion //ShowSearchBox

    #region ShowSortOptions

    public static readonly DependencyProperty ShowSortOptionsProperty = DependencyProperty.Register( "ShowSortOptions", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( true ) );
    public bool ShowSortOptions
    {
      get
      {
        return ( bool )GetValue( ShowSortOptionsProperty );
      }
      set
      {
        SetValue( ShowSortOptionsProperty, value );
      }
    }

    #endregion //ShowSortOptions

    #region ShowTitle

    public static readonly DependencyProperty ShowTitleProperty = DependencyProperty.Register( "ShowTitle", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( true ) );
    public bool ShowTitle
    {
      get
      {
        return ( bool )GetValue( ShowTitleProperty );
      }
      set
      {
        SetValue( ShowTitleProperty, value );
      }
    }

    #endregion //ShowTitle

    #endregion //Properties

    #region Constructors

    static PropertyGrid()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGrid ), new FrameworkPropertyMetadata( typeof( PropertyGrid ) ) );
    }

    public PropertyGrid()
    {
      _propertyDefinitionsListener = new WeakEventListener<NotifyCollectionChangedEventArgs>( this.OnPropertyDefinitionsCollectionChanged );
      _editorDefinitionsListener = new WeakEventListener<NotifyCollectionChangedEventArgs>( this.OnEditorDefinitionsCollectionChanged);
      UpdateContainerHelper();
      EditorDefinitions = new EditorDefinitionCollection();
      PropertyDefinitions = new PropertyDefinitionCollection();

      AddHandler( PropertyItemBase.ItemSelectionChangedEvent, new RoutedEventHandler( OnItemSelectionChanged ) );
      AddHandler( PropertyItemsControl.PreparePropertyItemEvent, new PropertyItemEventHandler( OnPreparePropertyItemInternal ) );
      AddHandler( PropertyItemsControl.ClearPropertyItemEvent, new PropertyItemEventHandler( OnClearPropertyItemInternal ) );
      CommandBindings.Add( new CommandBinding( PropertyGridCommands.ClearFilter, ClearFilter, CanClearFilter ) );
    }


    #endregion //Constructors

    #region Base Class Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( _dragThumb != null )
        _dragThumb.DragDelta -= DragThumb_DragDelta;
      _dragThumb = GetTemplateChild( PART_DragThumb ) as Thumb;
      if( _dragThumb != null )
        _dragThumb.DragDelta += DragThumb_DragDelta;

      _containerHelper.ChildrenItemsControl = GetTemplateChild( PART_PropertyItemsControl ) as PropertyItemsControl;

      //Update TranslateTransform in code-behind instead of XAML to remove the
      //output window error.
      //When we use FindAncesstor in custom control template for binding internal elements property 
      //into its ancestor element, Visual Studio displays data warning messages in output window when 
      //binding engine meets unmatched target type during visual tree traversal though it does the proper 
      //binding when it receives expected target type during visual tree traversal
      //ref : http://www.codeproject.com/Tips/124556/How-to-suppress-the-System-Windows-Data-Error-warn
      TranslateTransform _moveTransform = new TranslateTransform();
      _moveTransform.X = NameColumnWidth;
      _dragThumb.RenderTransform = _moveTransform;

      this.UpdateThumb();
    }

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      //hitting enter on textbox will update value of underlying source
      if( this.SelectedPropertyItem != null && e.Key == Key.Enter && e.OriginalSource is TextBox )
      {
        if( !( e.OriginalSource as TextBox ).AcceptsReturn )
        {
          BindingExpression be = ( ( TextBox )e.OriginalSource ).GetBindingExpression( TextBox.TextProperty );
          if( be != null )
            be.UpdateSource();
        }
      }
    }

    protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnPropertyChanged( e );

      // First check that the raised property is actually a real CLR property.
      // This could be something else like a Attached DP.
      if( ReflectionHelper.IsPublicInstanceProperty( GetType(), e.Property.Name ) )
      {
        this.Notify( this.PropertyChanged, e.Property.Name );
      }
    }

    #endregion //Base Class Overrides

    #region Event Handlers

    private void OnItemSelectionChanged( object sender, RoutedEventArgs args )
    {
      PropertyItemBase item = ( PropertyItemBase )args.OriginalSource;
      if( item.IsSelected )
      {
        SelectedPropertyItem = item;
      }
      else
      {
        if( object.ReferenceEquals( item, SelectedPropertyItem ) )
        {
          SelectedPropertyItem = null;
        }
      }
    }

    private void OnPreparePropertyItemInternal( object sender, PropertyItemEventArgs args )
    {
      _containerHelper.PrepareChildrenPropertyItem( args.PropertyItem, args.Item );
      args.Handled = true;
    }

    private void OnClearPropertyItemInternal( object sender, PropertyItemEventArgs args )
    {
      _containerHelper.ClearChildrenPropertyItem( args.PropertyItem, args.Item );
      args.Handled = true;
    }

    private void DragThumb_DragDelta( object sender, DragDeltaEventArgs e )
    {
      NameColumnWidth = Math.Max( 0, NameColumnWidth + e.HorizontalChange );
    }

    #endregion //Event Handlers

    #region Commands

    private void ClearFilter( object sender, ExecutedRoutedEventArgs e )
    {
      Filter = String.Empty;
    }

    private void CanClearFilter( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = !String.IsNullOrEmpty( Filter );
    }

    #endregion //Commands

    #region Methods

    private void UpdateContainerHelper()
    {
      // Keep a backup of the template element and initialize the
      // new helper with it.
      ItemsControl childrenItemsControl = null;
      if( _containerHelper != null )
      {
        childrenItemsControl = _containerHelper.ChildrenItemsControl;
        _containerHelper.ClearHelper();
        if( _containerHelper is ObjectContainerHelperBase )
        {
          // If the actual AdvancedOptionMenu is the default menu for selected object, 
          // remove it. Otherwise, it is a custom menu provided by the user.
          // This "default" menu is only valid for the SelectedObject[s] case. Otherwise, 
          // it is useless and we must remove it.
          var defaultAdvancedMenu = ( ContextMenu )this.FindResource( PropertyGrid.SelectedObjectAdvancedOptionsMenuKey );
          if( this.AdvancedOptionsMenu == defaultAdvancedMenu )
          {
            this.AdvancedOptionsMenu = null;
          }
        }
      }




      _containerHelper = new ObjectContainerHelper( this, SelectedObject );
      ( ( ObjectContainerHelper )_containerHelper ).GenerateProperties();


      _containerHelper.ChildrenItemsControl = childrenItemsControl;
      // Since the template will bind on this property and this property
      // will be different when the property parent is updated.
      this.Notify( this.PropertyChanged, () => this.Properties );
    }












    private void UpdateThumb()
    {
      if( _dragThumb != null )
      {
      if( IsCategorized )
        _dragThumb.Margin = new Thickness( 6, 0, 0, 0 );
      else
        _dragThumb.Margin = new Thickness( -1, 0, 0, 0 );
      }
    }

    /// <summary>
    /// Override this call to control the filter applied based on the
    /// text input.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    protected virtual Predicate<object> CreateFilter( string filter )
    {
      return null;
    }

    /// <summary>
    /// Updates all property values in the PropertyGrid with the data from the SelectedObject
    /// </summary>
    public void Update()
    {
      _containerHelper.UpdateValuesFromSource();
    }



    #endregion //Methods

    #region Events

    #region PropertyChanged Event

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region PropertyValueChangedEvent Routed Event
    public static readonly RoutedEvent PropertyValueChangedEvent = EventManager.RegisterRoutedEvent( "PropertyValueChanged", RoutingStrategy.Bubble, typeof( PropertyValueChangedEventHandler ), typeof( PropertyGrid ) );
    public event PropertyValueChangedEventHandler PropertyValueChanged
    {
      add
      {
        AddHandler( PropertyValueChangedEvent, value );
      }
      remove
      {
        RemoveHandler( PropertyValueChangedEvent, value );
      }
    }
    #endregion

    #region SelectedPropertyItemChangedEvent Routed Event

    public static readonly RoutedEvent SelectedPropertyItemChangedEvent = EventManager.RegisterRoutedEvent( "SelectedPropertyItemChanged", RoutingStrategy.Bubble, typeof( RoutedPropertyChangedEventHandler<PropertyItemBase> ), typeof( PropertyGrid ) );
    public event RoutedPropertyChangedEventHandler<PropertyItemBase> SelectedPropertyItemChanged
    {
      add
      {
        AddHandler( SelectedPropertyItemChangedEvent, value );
      }
      remove
      {
        RemoveHandler( SelectedPropertyItemChangedEvent, value );
      }
    }
    #endregion

    #region SelectedObjectChangedEventRouted Routed Event

    public static readonly RoutedEvent SelectedObjectChangedEvent = EventManager.RegisterRoutedEvent( "SelectedObjectChanged", RoutingStrategy.Bubble, typeof( RoutedPropertyChangedEventHandler<object> ), typeof( PropertyGrid ) );
    public event RoutedPropertyChangedEventHandler<object> SelectedObjectChanged
    {
      add
      {
        AddHandler( SelectedObjectChangedEvent, value );
      }
      remove
      {
        RemoveHandler( SelectedObjectChangedEvent, value );
      }
    }

    #endregion

    #region PreparePropertyItemEvent Attached Routed Event

    /// <summary>
    /// Identifies the PreparePropertyItem event.
    /// This attached routed event may be raised by the PropertyGrid itself or by a
    /// PropertyItemBase containing sub-items.
    /// </summary>
    public static readonly RoutedEvent PreparePropertyItemEvent = EventManager.RegisterRoutedEvent( "PreparePropertyItem", RoutingStrategy.Bubble, typeof( PropertyItemEventHandler ), typeof( PropertyGrid ) );

    /// <summary>
    /// This event is raised when a property item is about to be displayed in the PropertyGrid.
    /// This allow the user to customize the property item just before it is displayed.
    /// </summary>
    public event PropertyItemEventHandler PreparePropertyItem
    {
      add
      {
        AddHandler( PropertyGrid.PreparePropertyItemEvent, value );
      }
      remove
      {
        RemoveHandler( PropertyGrid.PreparePropertyItemEvent, value );
      }
    }

    /// <summary>
    /// Adds a handler for the PreparePropertyItem attached event
    /// </summary>
    /// <param name="element">the element to attach the handler</param>
    /// <param name="handler">the handler for the event</param>
    public static void AddPreparePropertyItemHandler( UIElement element, PropertyItemEventHandler handler )
    {
      element.AddHandler( PropertyGrid.PreparePropertyItemEvent, handler );
    }

    /// <summary>
    /// Removes a handler for the PreparePropertyItem attached event
    /// </summary>
    /// <param name="element">the element to attach the handler</param>
    /// <param name="handler">the handler for the event</param>
    public static void RemovePreparePropertyItemHandler( UIElement element, PropertyItemEventHandler handler )
    {
      element.RemoveHandler( PropertyGrid.PreparePropertyItemEvent, handler );
    }

    internal static void RaisePreparePropertyItemEvent( UIElement source, PropertyItemBase propertyItem, object item )
    {
      source.RaiseEvent( new PropertyItemEventArgs( PropertyGrid.PreparePropertyItemEvent, source, propertyItem, item ) );
    }

    #endregion

    #region ClearPropertyItemEvent Attached Routed Event

    /// <summary>
    /// Identifies the ClearPropertyItem event.
    /// This attached routed event may be raised by the PropertyGrid itself or by a
    /// PropertyItemBase containing sub items.
    /// </summary>
    public static readonly RoutedEvent ClearPropertyItemEvent = EventManager.RegisterRoutedEvent( "ClearPropertyItem", RoutingStrategy.Bubble, typeof( PropertyItemEventHandler ), typeof( PropertyGrid ) );
    /// <summary>
    /// This event is raised when an property item is about to be remove from the display in the PropertyGrid
    /// This allow the user to remove any attached handler in the PreparePropertyItem event.
    /// </summary>
    public event PropertyItemEventHandler ClearPropertyItem
    {
      add
      {
        AddHandler( PropertyGrid.ClearPropertyItemEvent, value );
      }
      remove
      {
        RemoveHandler( PropertyGrid.ClearPropertyItemEvent, value );
      }
    }

    /// <summary>
    /// Adds a handler for the ClearPropertyItem attached event
    /// </summary>
    /// <param name="element">the element to attach the handler</param>
    /// <param name="handler">the handler for the event</param>
    public static void AddClearPropertyItemHandler( UIElement element, PropertyItemEventHandler handler )
    {
      element.AddHandler( PropertyGrid.ClearPropertyItemEvent, handler );
    }

    /// <summary>
    /// Removes a handler for the ClearPropertyItem attached event
    /// </summary>
    /// <param name="element">the element to attach the handler</param>
    /// <param name="handler">the handler for the event</param>
    public static void RemoveClearPropertyItemHandler( UIElement element, PropertyItemEventHandler handler )
    {
      element.RemoveHandler( PropertyGrid.ClearPropertyItemEvent, handler );
    }

    internal static void RaiseClearPropertyItemEvent( UIElement source, PropertyItemBase propertyItem, object item )
    {
      source.RaiseEvent( new PropertyItemEventArgs( PropertyGrid.ClearPropertyItemEvent, source, propertyItem, item ) );
    }

    #endregion

    #endregion //Events

    #region Interfaces

    #region ISupportInitialize Members

    public override void BeginInit()
    {
      base.BeginInit();
      _initializationCount++;
    }

    public override void EndInit()
    {
      base.EndInit();
      if( --_initializationCount == 0 )
      {
        if( _hasPendingSelectedObjectChanged )
        {
          //This will update SelectedObject, Type, Name based on the actual config.
          this.UpdateContainerHelper();
          _hasPendingSelectedObjectChanged = false;
        }
        _containerHelper.OnEndInit();
      }
    }

    #endregion

    #region IPropertyContainer Members

    FilterInfo IPropertyContainer.FilterInfo
    {
      get 
      {
        return new FilterInfo()
        {
          Predicate = this.CreateFilter(this.Filter),
          InputString = this.Filter
        };
      }
    }

    ContainerHelperBase IPropertyContainer.ContainerHelper
    {
      get
      {
        return _containerHelper;
      }
    }

    #endregion


    #endregion

  }

  #region PropertyValueChangedEvent Handler/Args
  public delegate void PropertyValueChangedEventHandler( object sender, PropertyValueChangedEventArgs e );
  public class PropertyValueChangedEventArgs : RoutedEventArgs
  {
    public object NewValue
    {
      get;
      set;
    }
    public object OldValue
    {
      get;
      set;
    }

    public PropertyValueChangedEventArgs( RoutedEvent routedEvent, object source, object oldValue, object newValue )
      : base( routedEvent, source )
    {
      NewValue = newValue;
      OldValue = oldValue;
    }
  }
  #endregion

  #region PropertyItemCreatedEvent Handler/Args
  public delegate void PropertyItemEventHandler( object sender, PropertyItemEventArgs e );
  public class PropertyItemEventArgs : RoutedEventArgs
  {
    public PropertyItemBase PropertyItem
    {
      get;
      private set;
    }

    public object Item
    {
      get;
      private set;
    }

    public PropertyItemEventArgs( RoutedEvent routedEvent, object source, PropertyItemBase propertyItem, object item )
      : base( routedEvent, source )
    {
      this.PropertyItem = propertyItem;
      this.Item = item;
    }
  }
  #endregion

}
