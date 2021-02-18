/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

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
using System.Collections.ObjectModel;
using System.Collections;
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Linq.Expressions;

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

    #region CategoryGroupHeaderTemplate

    public static readonly DependencyProperty CategoryGroupHeaderTemplateProperty = DependencyProperty.Register( "CategoryGroupHeaderTemplate", typeof( DataTemplate ), typeof( PropertyGrid ) );
    public DataTemplate CategoryGroupHeaderTemplate
    {
      get
      {
        return (DataTemplate)GetValue( CategoryGroupHeaderTemplateProperty );
      }
      set
      {
        SetValue( CategoryGroupHeaderTemplateProperty, value );
      }
    }

    #endregion //CategoryGroupHeaderTemplate

    #region ShowDescriptionByTooltip

    public static readonly DependencyProperty ShowDescriptionByTooltipProperty = DependencyProperty.Register( "ShowDescriptionByTooltip", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( false ) );
    public bool ShowDescriptionByTooltip
    {
      get
      {
        return ( bool )GetValue( ShowDescriptionByTooltipProperty );
      }
      set
      {
        SetValue( ShowDescriptionByTooltipProperty, value );
      }
    }

    #endregion //ShowDescriptionByTooltip

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

    public static readonly DependencyProperty EditorDefinitionsProperty = DependencyProperty.Register( "EditorDefinitions", typeof( EditorDefinitionCollection ), typeof( PropertyGrid )
      , new UIPropertyMetadata( null, OnEditorDefinitionsChanged ) );
    public EditorDefinitionCollection EditorDefinitions
    {
      get
      {
        return ( EditorDefinitionCollection )GetValue( EditorDefinitionsProperty );
      }
      set
      {
        SetValue( EditorDefinitionsProperty, value );
      }
    }

    private static void OnEditorDefinitionsChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PropertyGrid propertyGrid = o as PropertyGrid;
      if( propertyGrid != null )
        propertyGrid.OnEditorDefinitionsChanged( ( EditorDefinitionCollection )e.OldValue, ( EditorDefinitionCollection )e.NewValue );
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
      if( _containerHelper != null )
      {
        _containerHelper.NotifyEditorDefinitionsCollectionChanged();
      }
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

    #region HideInheritedProperties

    public static readonly DependencyProperty HideInheritedPropertiesProperty = DependencyProperty.Register( "HideInheritedProperties", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( false ) );
    public bool HideInheritedProperties
    {
      get
      {
        return ( bool )GetValue( HideInheritedPropertiesProperty );
      }
      set
      {
        SetValue( HideInheritedPropertiesProperty, value );
      }
    }

    #endregion //HideInheritedProperties

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

    #region IsMiscCategoryLabelHidden

    public static readonly DependencyProperty IsMiscCategoryLabelHiddenProperty = DependencyProperty.Register( "IsMiscCategoryLabelHidden", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( false ) );
    public bool IsMiscCategoryLabelHidden
    {
      get
      {
        return ( bool )GetValue( IsMiscCategoryLabelHiddenProperty );
      }
      set
      {
        SetValue( IsMiscCategoryLabelHiddenProperty, value );
      }
    }

    #endregion //IsMiscCategoryLabelHidden

    #region IsScrollingToTopAfterRefresh

    public static readonly DependencyProperty IsScrollingToTopAfterRefreshProperty = DependencyProperty.Register( "IsScrollingToTopAfterRefresh", typeof( bool ), typeof( PropertyGrid )
      , new UIPropertyMetadata( true ) );
    public bool IsScrollingToTopAfterRefresh
    {
      get
      {
        return ( bool )GetValue( IsScrollingToTopAfterRefreshProperty );
      }
      set
      {
        SetValue( IsScrollingToTopAfterRefreshProperty, value );
      }
    }

    #endregion //IsScrollingToTopAfterRefresh

    #region IsVirtualizing

    public static readonly DependencyProperty IsVirtualizingProperty = DependencyProperty.Register( "IsVirtualizing", typeof( bool ), typeof( PropertyGrid )
      , new UIPropertyMetadata( false, OnIsVirtualizingChanged ) );
    public bool IsVirtualizing
    {
      get
      {
        return ( bool )GetValue( IsVirtualizingProperty );
      }
      set
      {
        SetValue( IsVirtualizingProperty, value );
      }
    }

    private static void OnIsVirtualizingChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      var propertyGrid = o as PropertyGrid;
      if( propertyGrid != null )
        propertyGrid.OnIsVirtualizingChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnIsVirtualizingChanged( bool oldValue, bool newValue )
    {
      this.UpdateContainerHelper();
    }

    #endregion //IsVirtualizing




























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

    #region PropertyNameLeftPadding

    public static readonly DependencyProperty PropertyNameLeftPaddingProperty = DependencyProperty.Register( "PropertyNameLeftPadding", typeof( double ), typeof( PropertyGrid ), new UIPropertyMetadata( 15.0 ) );
    public double PropertyNameLeftPadding
    {
      get
      {
        return (double)GetValue( PropertyNameLeftPaddingProperty );
      }
      set
      {
        SetValue( PropertyNameLeftPaddingProperty, value );
      }
    }

    #endregion //PropertyNameLeftPadding

    #region Properties

    public IList Properties
    {
      get
      {
        return (_containerHelper != null) ? _containerHelper.Properties : null;
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

    public static readonly DependencyProperty PropertyDefinitionsProperty =
        DependencyProperty.Register( "PropertyDefinitions", typeof( PropertyDefinitionCollection ), typeof( PropertyGrid ), new UIPropertyMetadata( null, OnPropertyDefinitionsChanged ) );

    public PropertyDefinitionCollection PropertyDefinitions
    {
      get
      {
        return (PropertyDefinitionCollection)GetValue( PropertyDefinitionsProperty );
      }
      set
      {
        SetValue( PropertyDefinitionsProperty, value );
      }
    }

    private static void OnPropertyDefinitionsChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      var owner = o as PropertyGrid;
      if( owner != null )
        owner.OnPropertyDefinitionsChanged( (PropertyDefinitionCollection)e.OldValue, (PropertyDefinitionCollection)e.NewValue );
    }

    protected virtual void OnPropertyDefinitionsChanged( PropertyDefinitionCollection oldValue, PropertyDefinitionCollection newValue )
    {
      if( oldValue != null )
      {
        CollectionChangedEventManager.RemoveListener( oldValue, _propertyDefinitionsListener );
      }

      if( newValue != null )
      {
        CollectionChangedEventManager.AddListener( newValue, _propertyDefinitionsListener );
      }

      this.Notify( this.PropertyChanged, () => this.PropertyDefinitions );
    }

    private void OnPropertyDefinitionsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( _containerHelper != null )
      {
        _containerHelper.NotifyPropertyDefinitionsCollectionChanged();
      }
      if( this.IsLoaded )
      {
        this.UpdateContainerHelper();
      }
    }

    #endregion //PropertyDefinitions

    #region IsReadOnly

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register( "IsReadOnly", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( false, OnIsReadOnlyChanged ) );
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

    private static void OnIsReadOnlyChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      var propertyGrid = o as PropertyGrid;
      if( propertyGrid != null )
        propertyGrid.OnIsReadOnlyChanged( (bool)e.OldValue, (bool)e.NewValue );
    }

    protected virtual void OnIsReadOnlyChanged( bool oldValue, bool newValue )
    {
      this.UpdateContainerHelper();
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

      this.SelectedProperty = ( (newValue != null) && (_containerHelper != null) ) ? _containerHelper.ItemFromContainer( newValue ) : null;

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
      if( _containerHelper != null )
      {
        object currentSelectedProperty = _containerHelper.ItemFromContainer( this.SelectedPropertyItem );
        if( !object.Equals( currentSelectedProperty, newValue ) )
        {
          this.SelectedPropertyItem = _containerHelper.ContainerFromItem( newValue );
        }
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

    #region ShowHorizontalScrollBar

    public static readonly DependencyProperty ShowHorizontalScrollBarProperty = DependencyProperty.Register( "ShowHorizontalScrollBar", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( false ) );
    public bool ShowHorizontalScrollBar
    {
      get
      {
        return ( bool )GetValue( ShowHorizontalScrollBarProperty );
      }
      set
      {
        SetValue( ShowHorizontalScrollBarProperty, value );
      }
    }

    #endregion //ShowHorizontalScrollBar

    #region ShowPreview

    public static readonly DependencyProperty ShowPreviewProperty = DependencyProperty.Register( "ShowPreview", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( false ) );
    public bool ShowPreview
    {
      get
      {
        return ( bool )GetValue( ShowPreviewProperty );
      }
      set
      {
        SetValue( ShowPreviewProperty, value );
      }
    }

    #endregion //ShowPreview

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

    #region UpdateTextBoxSourceOnEnterKey

    public static readonly DependencyProperty UpdateTextBoxSourceOnEnterKeyProperty = DependencyProperty.Register( "UpdateTextBoxSourceOnEnterKey", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( true ) );
    public bool UpdateTextBoxSourceOnEnterKey
    {
      get
      {
        return ( bool )GetValue( UpdateTextBoxSourceOnEnterKeyProperty );
      }
      set
      {
        SetValue( UpdateTextBoxSourceOnEnterKeyProperty, value );
      }
    }

    #endregion //UpdateTextBoxSourceOnEnterKey

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
#if VS2008
        EditorDefinitions = new EditorDefinitionCollection();
#else
      this.SetCurrentValue( PropertyGrid.EditorDefinitionsProperty, new EditorDefinitionCollection() );
#endif

      PropertyDefinitions = new PropertyDefinitionCollection();      
      this.PropertyValueChanged += this.PropertyGrid_PropertyValueChanged;

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

      if( _containerHelper != null )
      {
        _containerHelper.ChildrenItemsControl = GetTemplateChild( PART_PropertyItemsControl ) as PropertyItemsControl;
      }

      //Update TranslateTransform in code-behind instead of XAML to remove the
      //output window error.
      //When we use FindAncesstor in custom control template for binding internal elements property 
      //into its ancestor element, Visual Studio displays data warning messages in output window when 
      //binding engine meets unmatched target type during visual tree traversal though it does the proper 
      //binding when it receives expected target type during visual tree traversal
      //ref : http://www.codeproject.com/Tips/124556/How-to-suppress-the-System-Windows-Data-Error-warn
      TranslateTransform _moveTransform = new TranslateTransform();
      _moveTransform.X = NameColumnWidth;
      if( _dragThumb != null )
      {
        _dragThumb.RenderTransform = _moveTransform;
      }

      this.UpdateThumb();
    }

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      var textBox = e.OriginalSource as TextBox;

      //hitting enter on textbox will update value of underlying source if UpdateTextBoxSourceOnEnterKey is true
      if( (this.SelectedPropertyItem != null) 
          && (e.Key == Key.Enter)
          && this.UpdateTextBoxSourceOnEnterKey
          && (textBox != null)
          && !textBox.AcceptsReturn )
      {
        BindingExpression be = textBox.GetBindingExpression( TextBox.TextProperty );
        if( be != null )
          be.UpdateSource();
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
      if( _containerHelper != null )
      {
        _containerHelper.PrepareChildrenPropertyItem( args.PropertyItem, args.Item );
      }
      args.Handled = true;
    }

    private void OnClearPropertyItemInternal( object sender, PropertyItemEventArgs args )
    {
      if( _containerHelper != null )
      {
        _containerHelper.ClearChildrenPropertyItem( args.PropertyItem, args.Item );
      }
      args.Handled = true;
    }

    private void DragThumb_DragDelta( object sender, DragDeltaEventArgs e )
    {
      NameColumnWidth = Math.Min( Math.Max( this.ActualWidth * 0.1, NameColumnWidth + e.HorizontalChange ), this.ActualWidth * 0.9 );
    }


    private void PropertyGrid_PropertyValueChanged( object sender, PropertyValueChangedEventArgs e )
    {
      var modifiedPropertyItem = e.OriginalSource as PropertyItem;
      if( modifiedPropertyItem != null )
      {
        // Need to refresh the PropertyGrid Properties.
        if( modifiedPropertyItem.WillRefreshPropertyGrid )
        {
          // Refresh the PropertyGrid...this will set the initial Categories states.
          this.UpdateContainerHelper();
        }

        var parentPropertyItem = modifiedPropertyItem.ParentNode as PropertyItem;
        if( ( parentPropertyItem != null ) && parentPropertyItem.IsExpandable )
        {
          //Rebuild Editor for parent propertyItem if one of its sub-propertyItem have changed.
          this.RebuildPropertyItemEditor( parentPropertyItem );
        }
      }
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

























    public double GetScrollPosition()
    {
      var scrollViewer = this.GetScrollViewer();
      if( scrollViewer != null )
      {
        return scrollViewer.VerticalOffset;
      }
      return 0d;
    }

    public void ScrollToPosition( double position )
    {
      var scrollViewer = this.GetScrollViewer();
      if( scrollViewer != null )
      {
        scrollViewer.ScrollToVerticalOffset( position );
      }
    }

    public void ScrollToTop()
    {
      var scrollViewer = this.GetScrollViewer();
      if( scrollViewer != null )
      {
        scrollViewer.ScrollToTop();
      }
    }

    public void ScrollToBottom()
    {
      var scrollViewer = this.GetScrollViewer();
      if( scrollViewer != null )
      {
        scrollViewer.ScrollToBottom();
      }
    }

    public void CollapseAllProperties()
    {
      if( _containerHelper != null )
      {
        _containerHelper.SetPropertiesExpansion( false );
      }
    }

    public void ExpandAllProperties()
    {
      if( _containerHelper != null )
      {
        _containerHelper.SetPropertiesExpansion( true );
      }
    }

    public void ExpandProperty( string propertyName )
    {
      if( _containerHelper != null )
      {
        _containerHelper.SetPropertiesExpansion( propertyName, true );
      }
    }

    public void CollapseProperty( string propertyName )
    {
      if( _containerHelper != null )
      {
        _containerHelper.SetPropertiesExpansion( propertyName, false );
      }
    }

    private ScrollViewer GetScrollViewer()
    {
      if( (_containerHelper != null) && (_containerHelper.ChildrenItemsControl != null) )
      {
        return TreeHelper.FindChild<ScrollViewer>( _containerHelper.ChildrenItemsControl );
      }
      return null;
    }

    private void RebuildPropertyItemEditor( PropertyItem propertyItem )
    {
      if( propertyItem != null )
      {
        propertyItem.RebuildEditor();
      }
    }

    private void UpdateContainerHelper()
    {
      // Keep a backup of the template element and initialize the
      // new helper with it.
      ItemsControl childrenItemsControl = ( _containerHelper != null ) ? _containerHelper.ChildrenItemsControl : null;
      ObjectContainerHelperBase objectContainerHelper = null;


      objectContainerHelper = new ObjectContainerHelper( this, SelectedObject );
      objectContainerHelper.ObjectsGenerated += this.ObjectContainerHelper_ObjectsGenerated;
      objectContainerHelper.GenerateProperties();
    }

    private void SetContainerHelper( ContainerHelperBase containerHelper )
    {
      if( _containerHelper != null )
      {
        _containerHelper.ClearHelper();
      }
      _containerHelper = containerHelper;
    }

    private void FinalizeUpdateContainerHelper( ItemsControl childrenItemsControl )
    {

      if( _containerHelper != null )
      {
        _containerHelper.ChildrenItemsControl = childrenItemsControl;
      }

      if( this.IsScrollingToTopAfterRefresh )
      {
        this.ScrollToTop();
      }

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
      if( _containerHelper != null )
      {
        _containerHelper.UpdateValuesFromSource();
      }
    }




    #endregion //Methods

    #region Event Handlers

    private void ObjectContainerHelper_ObjectsGenerated( object sender, EventArgs e )
    {
      var objectContainerHelper = sender as ObjectContainerHelperBase;
      if( objectContainerHelper != null )
      {
        objectContainerHelper.ObjectsGenerated -= this.ObjectContainerHelper_ObjectsGenerated;
        this.SetContainerHelper( objectContainerHelper );
        this.FinalizeUpdateContainerHelper( objectContainerHelper.ChildrenItemsControl );

        RaiseEvent( new RoutedEventArgs( PropertyGrid.PropertiesGeneratedEvent, this ) );
      }
    }

    #endregion

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

    #region IsPropertyBrowsable Event

    public event IsPropertyBrowsableHandler IsPropertyBrowsable;

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

    #region PropertiesGenerated Event

    public static readonly RoutedEvent PropertiesGeneratedEvent = EventManager.RegisterRoutedEvent( "PropertiesGenerated", RoutingStrategy.Bubble, typeof( EventHandler ), typeof( PropertyGrid ) );
    public event RoutedEventHandler PropertiesGenerated
    {
      add
      {
        AddHandler( PropertiesGeneratedEvent, value );
      }
      remove
      {
        RemoveHandler( PropertiesGeneratedEvent, value );
      }
    }

    #endregion //PropertiesGenerated Event

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
        if( _containerHelper != null )
        {
          _containerHelper.OnEndInit();
        }
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

    bool IPropertyContainer.IsSortedAlphabetically
    {
      get
      {
        return true;
      }
    }


    bool? IPropertyContainer.IsPropertyVisible( PropertyDescriptor pd )
    {
      var handler = this.IsPropertyBrowsable;
      //If anyone is registered to PropertyGrid.IsPropertyBrowsable event
      if( handler != null )
      {
        var isBrowsableArgs = new IsPropertyBrowsableArgs( pd );
        handler( this, isBrowsableArgs );

        return isBrowsableArgs.IsBrowsable;
      }

      return null;
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

  #region IsPropertyArgs class

  public class PropertyArgs : RoutedEventArgs
  {
    #region Constructors

    public PropertyArgs( PropertyDescriptor pd )
    {
      this.PropertyDescriptor = pd;
    }

    #endregion

    #region Properties

    #region PropertyDescriptor Property

    public PropertyDescriptor PropertyDescriptor
    {
      get;
      private set;
    }

    #endregion

    #endregion
  }

  #endregion

  #region isPropertyBrowsableEvent Handler/Args

  public delegate void IsPropertyBrowsableHandler( object sender, IsPropertyBrowsableArgs e );

  public class IsPropertyBrowsableArgs : PropertyArgs
  {
    #region Constructors

    public IsPropertyBrowsableArgs( PropertyDescriptor pd )
      : base( pd )
    {
    }

    #endregion

    #region Properties

    #region IsBrowsable Property

    public bool? IsBrowsable
    {
      get;
      set;
    }

    #endregion

    #endregion
  }

  #endregion












}
