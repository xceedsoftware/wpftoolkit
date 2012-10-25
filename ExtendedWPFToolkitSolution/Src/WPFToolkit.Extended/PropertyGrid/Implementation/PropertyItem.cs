/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup.Primitives;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Commands;
using System.Collections.ObjectModel;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public class PropertyItem : Control, INotifyPropertyChanged, IPropertyParent
  {
    #region Members

    private readonly DependencyPropertyDescriptor _dpDescriptor;
    private readonly MarkupObject _markupObject;
    private readonly NotifyPropertyChangedHelper _propertyChangedHelper;

    private string _displayName;
    private string _description;
    private string _category;
    private int _categoryOrder;
    private int _propertyOrder;

    #endregion //Members

    #region Properties


    #region Category

    public string Category
    {
      get
      {
        return _category;
      }
      set
      {
        _propertyChangedHelper.HandleEqualityChanged( () => Category, ref _category, value );
      }
    }

    #endregion //Category

    #region CategoryOrder

    public int CategoryOrder
    {
      get
      {
        return _categoryOrder;
      }
      set
      {
        _propertyChangedHelper.HandleEqualityChanged( () => CategoryOrder, ref _categoryOrder, value );
      }
    }

    #endregion // CategoryOrder

    #region Description

    public string Description
    {
      get
      {
        return _description;
      }
      set
      {
        _propertyChangedHelper.HandleEqualityChanged( () => Description, ref _description, value );
      }
    }

    #endregion //Description

    #region DisplayName

    public string DisplayName
    {
      get
      {
        return _displayName;
      }
      set
      {
        _propertyChangedHelper.HandleEqualityChanged( () => DisplayName, ref _displayName, value );
      }
    }

    #endregion //DisplayName

    #region Editor

    public static readonly DependencyProperty EditorProperty = DependencyProperty.Register( "Editor", typeof( FrameworkElement ), typeof( PropertyItem ), new UIPropertyMetadata( null, OnEditorChanged ) );
    public FrameworkElement Editor
    {
      get
      {
        return ( FrameworkElement )GetValue( EditorProperty );
      }
      set
      {
        SetValue( EditorProperty, value );
      }
    }

    private static void OnEditorChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PropertyItem propertyItem = o as PropertyItem;
      if( propertyItem != null )
        propertyItem.OnEditorChanged( ( FrameworkElement )e.OldValue, ( FrameworkElement )e.NewValue );
    }

    protected virtual void OnEditorChanged( FrameworkElement oldValue, FrameworkElement newValue )
    {
      if( oldValue != null )
        oldValue.DataContext = null;

      if( newValue != null )
        newValue.DataContext = this;
    }

    #endregion //Editor

    #region Instance

    public object Instance
    {
      get;
      private set;
    }

    #endregion //Instance

    #region IsDataBound

    /// <summary>
    /// Gets if the property is data bound
    /// </summary>
    public bool IsDataBound
    {
      get
      {
        var dependencyObject = PropertyParent.ValueInstance as DependencyObject;
        if( dependencyObject != null && _dpDescriptor != null )
          return BindingOperations.GetBindingExpressionBase( dependencyObject, _dpDescriptor.DependencyProperty ) != null;

        return false;
      }
    }

    #endregion //IsDataBound

    #region IsDynamicResource

    public bool IsDynamicResource
    {
      get
      {
        var markupProperty = _markupObject.Properties.Where( p => p.Name == PropertyDescriptor.Name ).FirstOrDefault();
        if( markupProperty != null )
          return markupProperty.Value is DynamicResourceExtension;
        return false;
      }
    }

    #endregion //IsDynamicResource

    #region IsExpanded

    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register( "IsExpanded", typeof( bool ), typeof( PropertyItem ), new UIPropertyMetadata( false, OnIsExpandedChanged ) );
    public bool IsExpanded
    {
      get
      {
        return ( bool )GetValue( IsExpandedProperty );
      }
      set
      {
        SetValue( IsExpandedProperty, value );
      }
    }

    private static void OnIsExpandedChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PropertyItem propertyItem = o as PropertyItem;
      if( propertyItem != null )
        propertyItem.OnIsExpandedChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnIsExpandedChanged( bool oldValue, bool newValue )
    {
      if( newValue && Properties.Count == 0 )
      {
        GetChildProperties();
      }
    }

    #endregion IsExpanded

    #region HasChildProperties

    public static readonly DependencyProperty HasChildPropertiesProperty = DependencyProperty.Register( "HasChildProperties", typeof( bool ), typeof( PropertyItem ), new UIPropertyMetadata( false ) );
    public bool HasChildProperties
    {
      get
      {
        return ( bool )GetValue( HasChildPropertiesProperty );
      }
      set
      {
        SetValue( HasChildPropertiesProperty, value );
      }
    }

    #endregion HasChildProperties

    #region HasResourceApplied

    public bool HasResourceApplied
    {
      //TODO: need to find a better way to determine if a StaticResource has been applied to any property not just a style
      get
      {
        var markupProperty = _markupObject.Properties.Where( p => p.Name == PropertyDescriptor.Name ).FirstOrDefault();
        if( markupProperty != null )
          return markupProperty.Value is Style;

        return false;
      }
    }

    #endregion //HasResourceApplied

    public bool IsReadOnly
    {
      get
      {
        return PropertyDescriptor.IsReadOnly;
      }
    }

    #region IsSelected

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register( "IsSelected", typeof( bool ), typeof( PropertyItem ), new UIPropertyMetadata( false, OnIsSelectedChanged ) );
    public bool IsSelected
    {
      get
      {
        return ( bool )GetValue( IsSelectedProperty );
      }
      set
      {
        SetValue( IsSelectedProperty, value );
      }
    }

    private static void OnIsSelectedChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PropertyItem propertyItem = o as PropertyItem;
      if( propertyItem != null )
        propertyItem.OnIsSelectedChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnIsSelectedChanged( bool oldValue, bool newValue )
    {
      this.RaiseItemSelectionChangedEvent();
    }

    #endregion //IsSelected

    #region Level

    public int Level
    {
      get;
      private set;
    }

    #endregion //Level

    #region Properties

    public PropertyItemCollection Properties
    {
      get;
      private set;
    }

    #endregion //Properties

    #region PropertyDescriptor

    public PropertyDescriptor PropertyDescriptor
    {
      get;
      private set;
    }

    #endregion //PropertyDescriptor

    #region PropertyParent

    internal IPropertyParent PropertyParent
    {
      get;
      private set;
    }

    #endregion

    #region PropertyOrder

    public int PropertyOrder
    {
      get
      {
        return _propertyOrder;
      }
      set
      {
        _propertyChangedHelper.HandleEqualityChanged( () => PropertyOrder, ref _propertyOrder, value );
      }
    }

    #endregion

    #region PropertyType

    public Type PropertyType
    {
      get
      {
        return PropertyDescriptor.PropertyType;
      }
    }

    #endregion //PropertyType

    #region ResetValueCommand

    public ICommand ResetValueCommand
    {
      get;
      private set;
    }

    #endregion

    #region Value

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof( object ), typeof( PropertyItem ), new UIPropertyMetadata( null, OnValueChanged ) );
    public object Value
    {
      get
      {
        return ( object )GetValue( ValueProperty );
      }
      set
      {
        SetValue( ValueProperty, value );
      }
    }

    private static void OnValueChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PropertyItem propertyItem = o as PropertyItem;
      if( propertyItem != null )
      {
        propertyItem.OnValueChanged( ( object )e.OldValue, ( object )e.NewValue );
      }
    }

    protected virtual void OnValueChanged( object oldValue, object newValue )
    {
      if( IsInitialized )
      {
        RaiseEvent( new PropertyValueChangedEventArgs( PropertyGrid.PropertyValueChangedEvent, this, oldValue, newValue ) );
      }
    }

    #endregion //Value

    #region ValueSource

    /// <summary>
    /// Gets the value source.
    /// </summary>
    public BaseValueSource ValueSource
    {
      get
      {
        var dependencyObject = PropertyParent.ValueInstance as DependencyObject;
        if( _dpDescriptor != null && dependencyObject != null )
          return DependencyPropertyHelper.GetValueSource( dependencyObject, _dpDescriptor.DependencyProperty ).BaseValueSource;

        return BaseValueSource.Unknown;
      }
    }

    #endregion //ValueSource

    #endregion //Properties

    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #region ItemSelectionChanged

    internal static readonly RoutedEvent ItemSelectionChangedEvent = EventManager.RegisterRoutedEvent(
        "ItemSelectedEvent", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( PropertyItem ) );

    // This method raises the Tap event 
    private void RaiseItemSelectionChangedEvent()
    {
      RaiseEvent( new RoutedEventArgs( PropertyItem.ItemSelectionChangedEvent ) );
    }

    #endregion

    #endregion

    #region Constructors

    static PropertyItem()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyItem ), new FrameworkPropertyMetadata( typeof( PropertyItem ) ) );
    }

    internal PropertyItem( PropertyDescriptor property, IPropertyParent propertyParent, int level )
    {
      _propertyChangedHelper = new NotifyPropertyChangedHelper( this, RaisePropertyChanged );
      Properties = new PropertyItemCollection( new ObservableCollection<PropertyItem>() );
      PropertyParent = propertyParent;
      PropertyDescriptor = property;
      Level = level;


      Name = PropertyDescriptor.Name;
      Category = PropertyDescriptor.Category;
      CategoryOrder = 0;

      Description = ResolveDescription();
      DisplayName = ResolveDisplayName();
      HasChildProperties = ResolveExpandableObject();
      PropertyOrder = ResolvePropertyOrder();

      _dpDescriptor = DependencyPropertyDescriptor.FromProperty( PropertyDescriptor );
      _markupObject = MarkupWriter.GetMarkupObjectFor( PropertyParent.ValueInstance );

      CommandBindings.Add( new CommandBinding( PropertyItemCommands.ResetValue, ExecuteResetValueCommand, CanExecuteResetValueCommand ) );
      AddHandler( Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler( PropertyItem_PreviewMouseDown ), true );
    }

    #endregion //Constructors

    #region Event Handlers

    void PropertyItem_PreviewMouseDown( object sender, MouseButtonEventArgs e )
    {
      IsSelected = true;
    }

    #endregion  //Event Handlers

    #region Commands

    private void ExecuteResetValueCommand( object sender, ExecutedRoutedEventArgs e )
    {
      if( PropertyDescriptor.CanResetValue( PropertyParent.ValueInstance ) )
        PropertyDescriptor.ResetValue( PropertyParent.ValueInstance );

      //TODO: notify UI that the ValueSource may have changed to update the icon
    }

    private void CanExecuteResetValueCommand( object sender, CanExecuteRoutedEventArgs e )
    {
      bool canExecute = false;

      if( PropertyDescriptor.CanResetValue( PropertyParent.ValueInstance ) && !PropertyDescriptor.IsReadOnly )
      {
        canExecute = true;
      }

      e.CanExecute = canExecute;
    }

    #endregion //Commands

    #region Methods

    private void RaisePropertyChanged( string propertyName )
    {
      if( this.PropertyChanged != null )
      {
        this.PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
      }
    }

    private void GetChildProperties()
    {
      if( Value == null )
        return;

      var propertyItems = new List<PropertyItem>();

      try
      {
        PropertyDescriptorCollection descriptors = PropertyGridUtilities.GetPropertyDescriptors( Value );

        foreach( PropertyDescriptor descriptor in descriptors )
        {
          if( descriptor.IsBrowsable )
          {
            PropertyItem childPropertyItem = PropertyGridUtilities.CreatePropertyItem( descriptor, this, Level + 1 );
            propertyItems.Add( childPropertyItem );
          }
        }
      }
      catch( Exception )
      {
        //TODO: handle this some how
      }

      Properties.Update( propertyItems, false, null );
    }

    private string ResolveDescription()
    {
      //We do not simply rely on the "Description" property of PropertyDescriptor
      //since this value is cached bye PropertyDescriptor and the localized version 
      //(eg. LocalizedDescriptionAttribute) value can dynamicaly change
      DescriptionAttribute descriptionAtt = GetAttribute<DescriptionAttribute>();
      return ( descriptionAtt != null )
        ? descriptionAtt.Description
        : PropertyDescriptor.Description;
    }








    private string ResolveDisplayName()
    {
      string displayName = PropertyDescriptor.DisplayName;
      var attribute = GetAttribute<ParenthesizePropertyNameAttribute>();
      if( (attribute != null) && attribute.NeedParenthesis )
      {
        displayName = "(" + displayName + ")";
      }

      return displayName;
    }

    private bool ResolveExpandableObject()
    {
      bool isExpandable = false;
      var attribute = GetAttribute<ExpandableObjectAttribute>();
      if( attribute != null )
      {
        isExpandable = true;
      }

      return isExpandable;
    }

    private int ResolvePropertyOrder()
    {
      var attribute = GetAttribute<PropertyOrderAttribute>();

      // Max Value. Properties with no order will be displayed last.
      return ( attribute != null )
        ? attribute.Order
        : int.MaxValue;
    }

    private T GetAttribute<T>() where T : Attribute
    {
      return PropertyGridUtilities.GetAttribute<T>( PropertyDescriptor );
    }

    #endregion //Methods

    #region Interfaces

    #region IPropertyParent Members

    bool IPropertyParent.IsReadOnly
    {
      get { return PropertyParent.IsReadOnly; }
    }

    object IPropertyParent.ValueInstance
    {
      get { return Value; }
    }

    EditorDefinitionCollection IPropertyParent.EditorDefinitions
    {
      get { return PropertyParent.EditorDefinitions; }
    }

    #endregion

    #endregion
  }
}
