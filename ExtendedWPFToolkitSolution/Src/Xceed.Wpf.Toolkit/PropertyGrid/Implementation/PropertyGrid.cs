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

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  [TemplatePart( Name = PART_DragThumb, Type = typeof( Thumb ) )]
  public class PropertyGrid : Control, ISupportInitialize, IPropertyParent
  {
    private const string PART_DragThumb = "PART_DragThumb";

    #region Members

    private Thumb _dragThumb;
    private bool _hasPendingSelectedObjectChanged;
    private int _initializationCount;
    private PropertyItemCollection _properties;
    private PropertyDefinitionCollection _propertyDefinitions;
    private EditorDefinitionCollection _editorDefinitions;

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

    public static readonly DependencyProperty AutoGeneratePropertiesProperty = DependencyProperty.Register( "AutoGenerateProperties", typeof( bool ), typeof( PropertyGrid ), new UIPropertyMetadata( true, OnAutoGeneratePropertiesChanged ) );
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

    private static void OnAutoGeneratePropertiesChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ( ( PropertyGrid )o ).UpdateProperties( true );
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
        EditorDefinitionCollection oldValue = _editorDefinitions;
        _editorDefinitions = value;
        this.OnEditorDefinitionsChanged( oldValue, value );
      }
    }

    protected virtual void OnEditorDefinitionsChanged( EditorDefinitionCollection oldValue, EditorDefinitionCollection newValue )
    {
      if( oldValue != null )
        oldValue.CollectionChanged -= new NotifyCollectionChangedEventHandler( OnEditorDefinitionsCollectionChanged );

      if( newValue != null )
        newValue.CollectionChanged += new NotifyCollectionChangedEventHandler( OnEditorDefinitionsCollectionChanged );

      UpdateProperties( true );
    }

    private void OnEditorDefinitionsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      UpdateProperties( true );
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
      Properties.Filter( newValue );
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
      this.UpdateProperties( false );
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

    public PropertyItemCollection Properties
    {
      get
      {
        return _properties;
      }
    }

    #endregion //Properties

    #region PropertyDefinitions

    public PropertyDefinitionCollection PropertyDefinitions
    {
      get
      {
        return _propertyDefinitions;
      }
      set
      {
        PropertyDefinitionCollection oldValue = _propertyDefinitions;
        _propertyDefinitions = value;
        this.OnPropertyDefinitionsChanged( oldValue, value );
      }
    }

    protected virtual void OnPropertyDefinitionsChanged( PropertyDefinitionCollection oldValue, PropertyDefinitionCollection newValue )
    {
      if( oldValue != null )
        oldValue.CollectionChanged -= new NotifyCollectionChangedEventHandler( OnPropertyDefinitionsCollectionChanged );

      if( newValue != null )
        newValue.CollectionChanged += new NotifyCollectionChangedEventHandler( OnPropertyDefinitionsCollectionChanged );

      UpdateProperties( true );
    }

    private void OnPropertyDefinitionsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      UpdateProperties( true );
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

      this.UpdateProperties( true );

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

    public static readonly DependencyProperty SelectedPropertyItemProperty = DependencyProperty.Register( "SelectedPropertyItem", typeof( PropertyItem ), typeof( PropertyGrid ), new UIPropertyMetadata( null, OnSelectedPropertyItemChanged ) );
    public PropertyItem SelectedPropertyItem
    {
      get
      {
        return ( PropertyItem )GetValue( SelectedPropertyItemProperty );
      }
      internal set
      {
        SetValue( SelectedPropertyItemProperty, value );
      }
    }

    private static void OnSelectedPropertyItemChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PropertyGrid propertyGrid = o as PropertyGrid;
      if( propertyGrid != null )
        propertyGrid.OnSelectedPropertyItemChanged( ( PropertyItem )e.OldValue, ( PropertyItem )e.NewValue );
    }

    protected virtual void OnSelectedPropertyItemChanged( PropertyItem oldValue, PropertyItem newValue )
    {
      if( oldValue != null )
        oldValue.IsSelected = false;

      if( newValue != null )
        newValue.IsSelected = true;

      RaiseEvent( new RoutedPropertyChangedEventArgs<PropertyItem>( oldValue, newValue, PropertyGrid.SelectedPropertyItemChangedEvent ) );
    }

    #endregion //SelectedPropertyItem

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
      _properties = new PropertyItemCollection( new ObservableCollection<PropertyItem>() );
      EditorDefinitions = new EditorDefinitionCollection();
      PropertyDefinitions = new PropertyDefinitionCollection();

      AddHandler( PropertyItem.ItemSelectionChangedEvent, new RoutedEventHandler( OnItemSelectionChanged ) );
      AddHandler( PropertyItem.ItemOrderingChangedEvent, new RoutedEventHandler( OnItemOrderingChanged ) );
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

    #endregion //Base Class Overrides

    #region Event Handlers

    private void OnItemSelectionChanged( object sender, RoutedEventArgs args )
    {
      PropertyItem item = ( PropertyItem )args.OriginalSource;
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

    private void OnItemOrderingChanged( object sender, RoutedEventArgs args )
    {
      Properties.RefreshView();
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

    private void UpdateProperties( bool regenerateItems )
    {
      IEnumerable<PropertyItem> newProperties = null;
      string defaultPropertyName = null;

      if( regenerateItems )
      {
        newProperties = GeneratePropertyItems();
        defaultPropertyName = GetDefaultPropertyName();

        this.SelectedPropertyItem = ( defaultPropertyName != null )
                                    ? newProperties.FirstOrDefault( ( prop ) => defaultPropertyName.Equals( prop.PropertyName ) )
                                    : null;
    }

      Properties.Update( newProperties, IsCategorized, Filter );
    }

    private List<PropertyItem> GeneratePropertyItems()
    {
        return GeneratePropertyItems( SelectedObject );
    }

    private string GetDefaultPropertyName()
    {
        return GetDefaultPropertyName( SelectedObject );
    }

    private List<PropertyItem> GeneratePropertyItems(object instance)
    {
      var propertyItems = new List<PropertyItem>();

      if( instance != null )
      {
        try
        {
          PropertyDescriptorCollection descriptors = PropertyGridUtilities.GetPropertyDescriptors( instance );

          if( !AutoGenerateProperties )
          {
            List<PropertyDescriptor> specificProperties = new List<PropertyDescriptor>();
            if( PropertyDefinitions != null )
            {
              foreach( PropertyDefinition pd in PropertyDefinitions )
              {
                foreach( PropertyDescriptor descriptor in descriptors )
                {
                  if( descriptor.Name == pd.Name )
                  {
                    specificProperties.Add( descriptor );
                    break;
                  }
                }
              }
            }

            descriptors = new PropertyDescriptorCollection( specificProperties.ToArray() );
          }

          foreach( PropertyDescriptor descriptor in descriptors )
          {
            if( descriptor.IsBrowsable )
            {
              propertyItems.Add( PropertyGridUtilities.CreatePropertyItem( descriptor, this ) );
            }
          }
        }
        catch( Exception )
        {
          //TODO: handle this some how
        }
      }

      return propertyItems;
    }

    private string GetDefaultPropertyName( object selectedObject )
    {
      return PropertyGridUtilities.GetDefaultPropertyName( selectedObject );
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
    /// Updates all property values in the PropertyGrid with the data from the SelectedObject
    /// </summary>
    public void Update()
    {
      foreach( var item in Properties )
      {
        BindingOperations.GetBindingExpressionBase( item, PropertyItem.ValueProperty ).UpdateTarget();
      }
    }

    #endregion //Methods

    #region Events

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

    public static readonly RoutedEvent SelectedPropertyItemChangedEvent = EventManager.RegisterRoutedEvent( "SelectedPropertyItemChanged", RoutingStrategy.Bubble, typeof( RoutedPropertyChangedEventHandler<PropertyItem> ), typeof( PropertyGrid ) );
    public event RoutedPropertyChangedEventHandler<PropertyItem> SelectedPropertyItemChanged
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
        ProcessInitializationParameters();
      }
    }

    private void ProcessInitializationParameters()
    {
      if( _hasPendingSelectedObjectChanged )
      {
        //This will update SelectedObject, Type, Name based on the actual config.
        this.UpdateProperties( true );
        _hasPendingSelectedObjectChanged = false;
      }
    }

    #endregion

    #region IPropertyParent Members

    object IPropertyParent.ValueInstance
    {
      get { return this.SelectedObject; }
    }

    EditorDefinitionCollection IPropertyParent.EditorDefinitions
    {
      get { return this.EditorDefinitions; }
    }


    #endregion





    #endregion
  }

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
}
