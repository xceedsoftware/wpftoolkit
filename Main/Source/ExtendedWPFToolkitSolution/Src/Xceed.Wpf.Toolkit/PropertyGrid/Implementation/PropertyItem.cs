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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid.Implementation;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public class PropertyItem : Control
  {
    #region Properties

    #region AdvancedOptionsIcon

    public static readonly DependencyProperty AdvancedOptionsIconProperty =
        DependencyProperty.Register( "AdvancedOptionsIcon", typeof( ImageSource ), typeof( PropertyItem ), new UIPropertyMetadata( null ) );

    public ImageSource AdvancedOptionsIcon
    {
      get { return ( ImageSource )GetValue( AdvancedOptionsIconProperty ); }
      set { SetValue( AdvancedOptionsIconProperty, value ); }
    }

    #endregion //AdvancedOptionsIcon

    #region AdvancedOptionsTooltip

    public static readonly DependencyProperty AdvancedOptionsTooltipProperty =
        DependencyProperty.Register( "AdvancedOptionsTooltip", typeof( object ), typeof( PropertyItem ), new UIPropertyMetadata( null ) );

    public object AdvancedOptionsTooltip
    {
      get { return ( object )GetValue( AdvancedOptionsTooltipProperty ); }
      set { SetValue( AdvancedOptionsTooltipProperty, value ); }
    }

    #endregion //AdvancedOptionsTooltip

    #region Category

    public static readonly DependencyProperty CategoryProperty =
        DependencyProperty.Register( "Category", typeof( string ), typeof( PropertyItem ), new UIPropertyMetadata( null ) );

    public string Category
    {
      get { return ( string )GetValue( CategoryProperty ); }
      set { SetValue( CategoryProperty, value ); }
    }

    #endregion //Category

    #region CategoryOrder

    public int CategoryOrder
    {
      get;
      internal set;
    }

    #endregion //CategoryOrder

    #region ChildrenDefinitions

    internal IEnumerable<IPropertyDefinition> ChildrenDefinitions
    {
      get;
      set;
    }

    #endregion

    #region Description

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register( "Description", typeof( string ), typeof( PropertyItem ), new UIPropertyMetadata( null ) );

    public string Description
    {
      get { return ( string )GetValue( DescriptionProperty ); }
      set { SetValue( DescriptionProperty, value ); }
    }

    #endregion //Description

    #region DisplayName

    public static readonly DependencyProperty DisplayNameProperty =
        DependencyProperty.Register( "DisplayName", typeof( string ), typeof( PropertyItem ), new UIPropertyMetadata( null ) );

    public string DisplayName
    {
      get { return ( string )GetValue( DisplayNameProperty ); }
      set { SetValue( DisplayNameProperty, value ); }
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
      internal set;
    }

    #endregion //Instance

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
      if( newValue && Properties.Count == 0 && ( this.ChildrenDefinitions != null ) )
      {
        IEnumerable<PropertyItem> children = this.ChildrenDefinitions
          .Select( 
          ( def ) => 
            {
              PropertyItem subProperty = PropertyGridUtilities.CreatePropertyItem( def );
              subProperty.Level = Level + 1;
              return subProperty;
            });

        Properties.Update( children, false, null );
      }
    }

    #endregion IsExpanded

    #region IsExpandable

    public static readonly DependencyProperty IsExpandableProperty =
        DependencyProperty.Register( "IsExpandable", typeof( bool ), typeof( PropertyItem ), new UIPropertyMetadata( false ) );

    public bool IsExpandable
    {
      get { return ( bool )GetValue( IsExpandableProperty ); }
      set { SetValue( IsExpandableProperty, value ); }
    }

    #endregion //IsExpandable

    #region IsReadOnly

    public bool IsReadOnly
    {
      get
      {
        return ( PropertyDescriptor != null )
        ? PropertyDescriptor.IsReadOnly
        : false;
      }
    }

    #endregion

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
      internal set;
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
      internal set;
    }

    #endregion //PropertyDescriptor

    #region PropertyName

    internal string PropertyName
    {
      get
      {
        PropertyDescriptor descriptor = this.PropertyDescriptor;
        return ( descriptor != null ) ? descriptor.Name : null;
      }
    }

    #endregion //PropertyDescriptor

    #region PropertyOrder

    public static readonly DependencyProperty PropertyOrderProperty =
        DependencyProperty.Register( "PropertyOrder", typeof( int ), typeof( PropertyItem ), new UIPropertyMetadata( 0 ) );

    public int PropertyOrder
    {
      get { return ( int )GetValue( PropertyOrderProperty ); }
      set { SetValue( PropertyOrderProperty, value ); }
    }

    #endregion //PropertyOrder

    #region PropertyType

    public Type PropertyType
    {
      get
      {
        return ( PropertyDescriptor != null )
          ? PropertyDescriptor.PropertyType
          : null;
      }
    }

    #endregion //PropertyType

    #region Value

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof( object ), typeof( PropertyItem ), new UIPropertyMetadata( null, OnValueChanged, OnCoerceValueChanged ) );
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

    private static object OnCoerceValueChanged( DependencyObject o, object baseValue )
    {
      PropertyItem prop = o as PropertyItem;
      if( prop != null )
        return prop.OnCoerceValueChanged( baseValue );

      return baseValue;
    }

    protected virtual object OnCoerceValueChanged( object baseValue )
    {
      // Propagate error from DescriptorPropertyDefinitionBase to PropertyItem.Value
      // to see the red error rectangle in the propertyGrid.
      BindingExpression be = GetBindingExpression( PropertyItem.ValueProperty );
      if( ( be != null ) && be.DataItem is DescriptorPropertyDefinitionBase )
      {
        DescriptorPropertyDefinitionBase descriptor = be.DataItem as DescriptorPropertyDefinitionBase;
        if( Validation.GetHasError( descriptor ) )
        {
          ReadOnlyObservableCollection<ValidationError> errors = Validation.GetErrors( descriptor );
          Validation.MarkInvalid( be, errors[ 0 ] );
        }
      }
      return baseValue;
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

    #endregion //Properties

    #region Events

    #region ItemSelectionChanged

    internal static readonly RoutedEvent ItemSelectionChangedEvent = EventManager.RegisterRoutedEvent(
        "ItemSelectionChangedEvent", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( PropertyItem ) );

    private void RaiseItemSelectionChangedEvent()
    {
      RaiseEvent( new RoutedEventArgs( PropertyItem.ItemSelectionChangedEvent ) );
    }

    #endregion

    #region ItemOrderingChanged

    internal static readonly RoutedEvent ItemOrderingChangedEvent = EventManager.RegisterRoutedEvent(
        "ItemOrderingChangedEvent", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( PropertyItem ) );

    private void RaiseItemOrderingChangedEvent()
    {
      RaiseEvent( new RoutedEventArgs( PropertyItem.ItemOrderingChangedEvent ) );
    }

    #endregion

    #endregion //Events

    #region Constructors

    static PropertyItem()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyItem ), new FrameworkPropertyMetadata( typeof( PropertyItem ) ) );
    }

    internal PropertyItem()
    {
      Properties = new PropertyItemCollection( new ObservableCollection<PropertyItem>() );
      AddHandler( Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler( PropertyItem_PreviewMouseDown ), true );
      AddHandler( PropertyItem.ItemOrderingChangedEvent, new RoutedEventHandler( OnItemOrderingChanged ), false );
    }

    #endregion //Constructors

    #region Event Handlers

    private void PropertyItem_PreviewMouseDown( object sender, MouseButtonEventArgs e )
    {
      IsSelected = true;
    }

    private void OnItemOrderingChanged( object sender, RoutedEventArgs args )
    {
      if( args.OriginalSource != this )
      {
        //If the OriginalSource is not this object, it comes from one of our children.
        Properties.RefreshView();
        args.Handled = true;
      }
    }

    #endregion  //Event Handlers

    #region Methods

    protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnPropertyChanged( e );

      if( PropertyItemCollection.IsItemOrderingProperty( e.Property.Name ) )
      {
        this.RaiseItemOrderingChangedEvent();
      }
    }

    #endregion //Methods
  }
}
