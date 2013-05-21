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
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xceed.Wpf.Toolkit
{
  [TemplatePart( Name = PART_NewItemTypesComboBox, Type = typeof( ComboBox ) )]
  public class CollectionControl : Control
  {
    private const string PART_NewItemTypesComboBox = "PART_NewItemTypesComboBox";

    #region Private Members

    private ComboBox _newItemTypesComboBox;

    #endregion

    #region Properties

    #region IsReadOnly Property

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register( "IsReadOnly", typeof( bool ), typeof( CollectionControl ), new UIPropertyMetadata( false ) );
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

    #endregion  //Items

    #region Items Property

    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register( "Items", typeof( ObservableCollection<object> ), typeof( CollectionControl ), new UIPropertyMetadata( null ) );
    public ObservableCollection<object> Items
    {
      get
      {
        return ( ObservableCollection<object> )GetValue( ItemsProperty );
      }
      set
      {
        SetValue( ItemsProperty, value );
      }
    }

    #endregion  //Items

    #region ItemsSource Property

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register( "ItemsSource", typeof( IList ), typeof( CollectionControl ), new UIPropertyMetadata( null, OnItemsSourceChanged ) );
    public IList ItemsSource
    {
      get
      {
        return ( IList )GetValue( ItemsSourceProperty );
      }
      set
      {
        SetValue( ItemsSourceProperty, value );
      }
    }

    private static void OnItemsSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      CollectionControl CollectionControl = ( CollectionControl )d;
      if( CollectionControl != null )
        CollectionControl.OnItemSourceChanged( ( IList )e.OldValue, ( IList )e.NewValue );
    }

    public void OnItemSourceChanged( IList oldValue, IList newValue )
    {
      if( newValue != null )
      {
        foreach( var item in newValue )
          Items.Add( CreateClone( item ) );
      }
    }

    #endregion  //ItemsSource

    #region ItemsSourceType Property

    public static readonly DependencyProperty ItemsSourceTypeProperty = DependencyProperty.Register( "ItemsSourceType", typeof( Type ), typeof( CollectionControl ), new UIPropertyMetadata( null ) );
    public Type ItemsSourceType
    {
      get
      {
        return ( Type )GetValue( ItemsSourceTypeProperty );
      }
      set
      {
        SetValue( ItemsSourceTypeProperty, value );
      }
    }

    #endregion //ItemsSourceType

    #region NewItemType Property

    public static readonly DependencyProperty NewItemTypesProperty = DependencyProperty.Register( "NewItemTypes", typeof( IList ), typeof( CollectionControl ), new UIPropertyMetadata( null ) );
    public IList<Type> NewItemTypes
    {
      get
      {
        return ( IList<Type> )GetValue( NewItemTypesProperty );
      }
      set
      {
        SetValue( NewItemTypesProperty, value );
      }
    }

    #endregion  //NewItemType

    #region SelectedItem Property

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register( "SelectedItem", typeof( object ), typeof( CollectionControl ), new UIPropertyMetadata( null ) );
    public object SelectedItem
    {
      get
      {
        return ( object )GetValue( SelectedItemProperty );
      }
      set
      {
        SetValue( SelectedItemProperty, value );
      }
    }

    #endregion  //SelectedItem

    #endregion //Properties

    #region Override Methods

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( _newItemTypesComboBox != null )
        _newItemTypesComboBox.Loaded -= new RoutedEventHandler( this.NewItemTypesComboBox_Loaded );

      _newItemTypesComboBox = GetTemplateChild( PART_NewItemTypesComboBox ) as ComboBox;

      if( _newItemTypesComboBox != null )
        _newItemTypesComboBox.Loaded += new RoutedEventHandler( this.NewItemTypesComboBox_Loaded );
    }

    #endregion

    #region Constructors

    static CollectionControl()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( CollectionControl ), new FrameworkPropertyMetadata( typeof( CollectionControl ) ) );
    }

    public CollectionControl()
    {
      Items = new ObservableCollection<object>();
      CommandBindings.Add( new CommandBinding( ApplicationCommands.New, AddNew, CanAddNew ) );
      CommandBindings.Add( new CommandBinding( ApplicationCommands.Delete, Delete, CanDelete ) );
      CommandBindings.Add( new CommandBinding( ComponentCommands.MoveDown, MoveDown, CanMoveDown ) );
      CommandBindings.Add( new CommandBinding( ComponentCommands.MoveUp, MoveUp, CanMoveUp ) );
    }

    #endregion //Constructors

    #region Events

    #region ItemDeleting Event

    public delegate void ItemDeletingRoutedEventHandler( object sender, ItemDeletingEventArgs e );

    public static readonly RoutedEvent ItemDeletingEvent = EventManager.RegisterRoutedEvent( "ItemDeleting", RoutingStrategy.Bubble, typeof( ItemDeletingRoutedEventHandler ), typeof( CollectionControl ) );
    public event ItemDeletingRoutedEventHandler ItemDeleting
    {
      add
      {
        AddHandler( ItemDeletingEvent, value );
      }
      remove
      {
        RemoveHandler( ItemDeletingEvent, value );
      }
    }

    #endregion //ItemDeleting Event

    #region ItemDeleted Event

    public delegate void ItemDeletedRoutedEventHandler( object sender, ItemEventArgs e );

    public static readonly RoutedEvent ItemDeletedEvent = EventManager.RegisterRoutedEvent( "ItemDeleted", RoutingStrategy.Bubble, typeof( ItemDeletedRoutedEventHandler ), typeof( CollectionControl ) );
    public event ItemDeletedRoutedEventHandler ItemDeleted
    {
      add
      {
        AddHandler( ItemDeletedEvent, value );
      }
      remove
      {
        RemoveHandler( ItemDeletedEvent, value );
      }
    }

    #endregion //ItemDeleted Event

    #region ItemAdding Event

    public delegate void ItemAddingRoutedEventHandler( object sender, ItemAddingEventArgs e );

    public static readonly RoutedEvent ItemAddingEvent = EventManager.RegisterRoutedEvent( "ItemAdding", RoutingStrategy.Bubble, typeof( ItemAddingRoutedEventHandler ), typeof( CollectionControl ) );
    public event ItemAddingRoutedEventHandler ItemAdding
    {
      add
      {
        AddHandler( ItemAddingEvent, value );
      }
      remove
      {
        RemoveHandler( ItemAddingEvent, value );
      }
    }

    #endregion //ItemAdding Event

    #region ItemAdded Event

    public delegate void ItemAddedRoutedEventHandler( object sender, ItemEventArgs e );

    public static readonly RoutedEvent ItemAddedEvent = EventManager.RegisterRoutedEvent( "ItemAdded", RoutingStrategy.Bubble, typeof( ItemAddedRoutedEventHandler ), typeof( CollectionControl ) );
    public event ItemAddedRoutedEventHandler ItemAdded
    {
      add
      {
        AddHandler( ItemAddedEvent, value );
      }
      remove
      {
        RemoveHandler( ItemAddedEvent, value );
      }
    }

    #endregion //ItemAdded Event

    #endregion

    #region EventHandlers

    void NewItemTypesComboBox_Loaded( object sender, RoutedEventArgs e )
    {
      if( _newItemTypesComboBox != null )
        _newItemTypesComboBox.SelectedIndex = 0;
    }

    #endregion

    #region Commands

    private void AddNew( object sender, ExecutedRoutedEventArgs e )
    {
      var newItem = CreateNewItem( ( Type )e.Parameter );

      var eventArgs = new ItemAddingEventArgs( ItemAddingEvent, newItem );
      this.RaiseEvent( eventArgs );
      if( eventArgs.Cancel )
        return;
      newItem = eventArgs.Item;

      Items.Add( newItem );

      this.RaiseEvent( new ItemEventArgs( ItemAddedEvent, newItem ) );

      SelectedItem = newItem;
    }

    private void CanAddNew( object sender, CanExecuteRoutedEventArgs e )
    {
      Type t = e.Parameter as Type;
      if( t != null && t.GetConstructor( Type.EmptyTypes ) != null && !IsReadOnly)
        e.CanExecute = true;
    }

    private void Delete( object sender, ExecutedRoutedEventArgs e )
    {
      var eventArgs = new ItemDeletingEventArgs( ItemDeletingEvent, e.Parameter );
      this.RaiseEvent( eventArgs );
      if( eventArgs.Cancel )
        return;

      Items.Remove( e.Parameter );

      this.RaiseEvent( new ItemEventArgs( ItemDeletedEvent, e.Parameter ) );
    }

    private void CanDelete( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = e.Parameter != null && !IsReadOnly;
    }

    private void MoveDown( object sender, ExecutedRoutedEventArgs e )
    {
      var selectedItem = e.Parameter;
      var index = Items.IndexOf( selectedItem );
      Items.RemoveAt( index );
      Items.Insert( ++index, selectedItem );
      SelectedItem = selectedItem;
    }

    private void CanMoveDown( object sender, CanExecuteRoutedEventArgs e )
    {
      if( e.Parameter != null && Items.IndexOf( e.Parameter ) < ( Items.Count - 1 ) && !IsReadOnly )
        e.CanExecute = true;
    }

    private void MoveUp( object sender, ExecutedRoutedEventArgs e )
    {
      var selectedItem = e.Parameter;
      var index = Items.IndexOf( selectedItem );
      Items.RemoveAt( index );
      Items.Insert( --index, selectedItem );
      SelectedItem = selectedItem;
    }

    private void CanMoveUp( object sender, CanExecuteRoutedEventArgs e )
    {
      if( e.Parameter != null && Items.IndexOf( e.Parameter ) > 0 && !IsReadOnly )
        e.CanExecute = true;
    }

    #endregion //Commands

    #region Methods

    private static void CopyValues( object source, object destination )
    {
      Type currentType = source.GetType();

      while( currentType != null )
      {
        FieldInfo[] myObjectFields = currentType.GetFields( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
        foreach( FieldInfo fi in myObjectFields )
        {
          fi.SetValue( destination, fi.GetValue( source ) );
        }

        currentType = currentType.BaseType;
      }
    }

    private object CreateClone( object source )
    {
      object clone = null;

      Type type = source.GetType();
      clone = Activator.CreateInstance( type );
      CopyValues( source, clone );

      return clone;
    }

    private IList CreateItemsSource()
    {
      IList list = null;

      if( ItemsSourceType != null )
      {
        ConstructorInfo constructor = ItemsSourceType.GetConstructor( Type.EmptyTypes );
        list = ( IList )constructor.Invoke( null );
      }

      return list;
    }

    private object CreateNewItem( Type type )
    {
      return Activator.CreateInstance( type );
    }

    public void PersistChanges()
    {
      IList list = ComputeItemsSource();
      if( list == null )
        return;

      //the easiest way to persist changes to the source is to just clear the source list and then add all items to it.
      list.Clear();

      foreach( var item in Items )
      {
        list.Add( item );
      }
    }

    private IList ComputeItemsSource()
    {
      if( ItemsSource == null )
        ItemsSource = CreateItemsSource();

      return ItemsSource;
    }

    #endregion //Methods
  }
}
