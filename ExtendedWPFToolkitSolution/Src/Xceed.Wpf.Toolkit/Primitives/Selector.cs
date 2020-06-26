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
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Windows.Threading;

namespace Xceed.Wpf.Toolkit.Primitives
{
  public class Selector : ItemsControl, IWeakEventListener //should probably make this control an ICommandSource
  {
    #region Members

    private bool _surpressItemSelectionChanged;
    private bool _ignoreSelectedItemChanged;
    private bool _ignoreSelectedValueChanged;
    private int _ignoreSelectedItemsCollectionChanged;
    private int _ignoreSelectedMemberPathValuesChanged;
    private IList _selectedItems;
    private IList _removedItems = new ObservableCollection<object>();
    private object[] _internalSelectedItems;

    private ValueChangeHelper _selectedMemberPathValuesHelper;
    private ValueChangeHelper _valueMemberPathValuesHelper;



    #endregion //Members

    #region Constructors

    public Selector()
    {
      this.SelectedItems = new ObservableCollection<object>();
      AddHandler( Selector.SelectedEvent, new RoutedEventHandler( ( s, args ) => this.OnItemSelectionChangedCore( args, false ) ) );
      AddHandler( Selector.UnSelectedEvent, new RoutedEventHandler( ( s, args ) => this.OnItemSelectionChangedCore( args, true ) ) );
      _selectedMemberPathValuesHelper = new ValueChangeHelper( this.OnSelectedMemberPathValuesChanged );
      _valueMemberPathValuesHelper = new ValueChangeHelper( this.OnValueMemberPathValuesChanged );
    }

    #endregion //Constructors

    #region Properties

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register( "Command", typeof( ICommand ), typeof( Selector ), new PropertyMetadata( ( ICommand )null ) );
    [TypeConverter( typeof( CommandConverter ) )]
    public ICommand Command
    {
      get
      {
        return ( ICommand )GetValue( CommandProperty );
      }
      set
      {
        SetValue( CommandProperty, value );
      }
    }

    #region Delimiter

    public static readonly DependencyProperty DelimiterProperty = DependencyProperty.Register( "Delimiter", typeof( string ), typeof( Selector ), new UIPropertyMetadata( ",", OnDelimiterChanged ) );
    public string Delimiter
    {
      get
      {
        return ( string )GetValue( DelimiterProperty );
      }
      set
      {
        SetValue( DelimiterProperty, value );
      }
    }

    private static void OnDelimiterChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ( ( Selector )o ).OnSelectedItemChanged( ( string )e.OldValue, ( string )e.NewValue );
    }

    protected virtual void OnSelectedItemChanged( string oldValue, string newValue )
    {
      if( !this.IsInitialized )
        return;

      this.UpdateSelectedValue();
    }

    #endregion

    #region SelectedItem property

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register( "SelectedItem", typeof( object ), typeof( Selector ), new UIPropertyMetadata( null, OnSelectedItemChanged ) );
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

    private static void OnSelectedItemChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args )
    {
      ( ( Selector )sender ).OnSelectedItemChanged( args.OldValue, args.NewValue );
    }

    protected virtual void OnSelectedItemChanged( object oldValue, object newValue )
    {
      if( !this.IsInitialized || _ignoreSelectedItemChanged )
        return;

      _ignoreSelectedItemsCollectionChanged++;
      this.SelectedItems.Clear();
      if( newValue != null )
      {
        this.SelectedItems.Add( newValue );
      }
      this.UpdateFromSelectedItems();
      _ignoreSelectedItemsCollectionChanged--;
    }

    #endregion

    #region SelectedItems Property

    public IList SelectedItems
    {
      get
      {
        return _selectedItems;
      }
      private set
      {
        if( value == null )
          throw new ArgumentNullException( "value" );

        INotifyCollectionChanged oldCollection = _selectedItems as INotifyCollectionChanged;
        INotifyCollectionChanged newCollection = value as INotifyCollectionChanged;

        if( oldCollection != null )
        {
          CollectionChangedEventManager.RemoveListener( oldCollection, this );
        }

        if( newCollection != null )
        {
          CollectionChangedEventManager.AddListener( newCollection, this );
        }

        var newValue = value;
        var oldValue = _selectedItems;
        if( oldValue != null )
        {
          foreach( var item in oldValue )
          {
            if( ( ( newValue != null ) && !newValue.Contains( item ) ) || ( newValue == null ) )
            {
              this.OnItemSelectionChanged( new ItemSelectionChangedEventArgs( Selector.ItemSelectionChangedEvent, this, item, false ) );

              if( Command != null )
              {
                this.Command.Execute( item );
              }
            }
          }
        }
        if( newValue != null )
        {
          foreach( var item in newValue )
          {
            this.OnItemSelectionChanged( new ItemSelectionChangedEventArgs( Selector.ItemSelectionChangedEvent, this, item, true ) );

            if( ( ( oldValue != null ) && !oldValue.Contains( item ) ) || ( oldValue == null ) )
            {
              if( Command != null )
              {
                this.Command.Execute( item );
              }
            }
          }
        }


        _selectedItems = value;
      }
    }

    #endregion SelectedItems


    #region SelectedItemsOverride property

    public static readonly DependencyProperty SelectedItemsOverrideProperty = DependencyProperty.Register( "SelectedItemsOverride", typeof( IList ), typeof( Selector ), new UIPropertyMetadata( null, SelectedItemsOverrideChanged ) );
    public IList SelectedItemsOverride
    {
      get
      {
        return ( IList )GetValue( SelectedItemsOverrideProperty );
      }
      set
      {
        SetValue( SelectedItemsOverrideProperty, value );
      }
    }

    private static void SelectedItemsOverrideChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args )
    {
      ( ( Selector )sender ).OnSelectedItemsOverrideChanged( ( IList )args.OldValue, ( IList )args.NewValue );
    }

    protected virtual void OnSelectedItemsOverrideChanged( IList oldValue, IList newValue )
    {
      if( !this.IsInitialized )
        return;

      this.SelectedItems = ( newValue != null ) ? newValue : new ObservableCollection<object>();
      this.UpdateFromSelectedItems();
    }

    #endregion


    #region SelectedMemberPath Property

    public static readonly DependencyProperty SelectedMemberPathProperty = DependencyProperty.Register( "SelectedMemberPath", typeof( string ), typeof( Selector ), new UIPropertyMetadata( null, OnSelectedMemberPathChanged ) );
    public string SelectedMemberPath
    {
      get
      {
        return ( string )GetValue( SelectedMemberPathProperty );
      }
      set
      {
        SetValue( SelectedMemberPathProperty, value );
      }
    }

    private static void OnSelectedMemberPathChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      Selector sel = ( ( Selector )o );
      sel.OnSelectedMemberPathChanged( ( string )e.OldValue, ( string )e.NewValue );
    }

    protected virtual void OnSelectedMemberPathChanged( string oldValue, string newValue )
    {
      if( !this.IsInitialized )
        return;

      this.UpdateSelectedMemberPathValuesBindings();
    }

    #endregion //SelectedMemberPath

    #region SelectedValue

    public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register( "SelectedValue", typeof( string ), typeof( Selector ), new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged ) );
    public string SelectedValue
    {
      get
      {
        return ( string )GetValue( SelectedValueProperty );
      }
      set
      {
        SetValue( SelectedValueProperty, value );
      }
    }

    private static void OnSelectedValueChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      Selector selector = o as Selector;
      if( selector != null )
        selector.OnSelectedValueChanged( ( string )e.OldValue, ( string )e.NewValue );
    }

    protected virtual void OnSelectedValueChanged( string oldValue, string newValue )
    {
      if( !this.IsInitialized || _ignoreSelectedValueChanged )
        return;

      UpdateFromSelectedValue();
    }

    #endregion //SelectedValue

    #region ValueMemberPath

    public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register( "ValueMemberPath", typeof( string ), typeof( Selector ), new UIPropertyMetadata( OnValueMemberPathChanged ) );
    public string ValueMemberPath
    {
      get
      {
        return ( string )GetValue( ValueMemberPathProperty );
      }
      set
      {
        SetValue( ValueMemberPathProperty, value );
      }
    }

    private static void OnValueMemberPathChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      Selector sel = ( ( Selector )o );
      sel.OnValueMemberPathChanged( ( string )e.OldValue, ( string )e.NewValue );
    }

    protected virtual void OnValueMemberPathChanged( string oldValue, string newValue )
    {
      if( !this.IsInitialized )
        return;

      this.UpdateValueMemberPathValuesBindings();
    }

    #endregion

    #region ItemsCollection Property

    protected IEnumerable ItemsCollection
    {
      get
      {
        return ItemsSource ?? ( ( IEnumerable )Items ?? ( IEnumerable )new object[ 0 ] );
      }
    }

    #endregion

    #endregion //Properties

    #region Base Class Overrides

    protected override bool IsItemItsOwnContainerOverride( object item )
    {
      return item is SelectorItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
      return new SelectorItem();
    }

    protected override void PrepareContainerForItemOverride( DependencyObject element, object item )
    {
      base.PrepareContainerForItemOverride( element, item );

      _surpressItemSelectionChanged = true;
      var selectorItem = element as FrameworkElement;

      selectorItem.SetValue( SelectorItem.IsSelectedProperty, SelectedItems.Contains( item ) );

      _surpressItemSelectionChanged = false;
    }

    protected override void OnItemsSourceChanged( IEnumerable oldValue, IEnumerable newValue )
    {
      base.OnItemsSourceChanged( oldValue, newValue );

      var oldCollection = oldValue as INotifyCollectionChanged;
      var newCollection = newValue as INotifyCollectionChanged;

      if( oldCollection != null )
      {
        CollectionChangedEventManager.RemoveListener( oldCollection, this );
      }

      if( newCollection != null )
      {
        CollectionChangedEventManager.AddListener( newCollection, this );
      }

      if( !this.IsInitialized )
        return;

      if( !VirtualizingStackPanel.GetIsVirtualizing( this )
        || ( VirtualizingStackPanel.GetIsVirtualizing( this ) && ( newValue != null ) ) )
      {
        this.RemoveUnavailableSelectedItems();
      }

      this.UpdateSelectedMemberPathValuesBindings();
      this.UpdateValueMemberPathValuesBindings();
    }

    protected override void OnItemsChanged( NotifyCollectionChangedEventArgs e )
    {
      base.OnItemsChanged( e );

      this.RemoveUnavailableSelectedItems();
    }

    // When a DataTemplate includes a CheckComboBox, some bindings are
    // not working, like SelectedValue.
    // We use a priority system to select the good items after initialization.
    public override void EndInit()
    {
      base.EndInit();

      if( this.SelectedItemsOverride != null )
      {
        this.OnSelectedItemsOverrideChanged( null, this.SelectedItemsOverride );
      }
      else if( this.SelectedMemberPath != null )
      {
        this.OnSelectedMemberPathChanged( null, this.SelectedMemberPath );
      }
      else if( this.SelectedValue != null )
      {
        this.OnSelectedValueChanged( null, this.SelectedValue );
      }
      else if( this.SelectedItem != null )
      {
        this.OnSelectedItemChanged( null, this.SelectedItem );
      }

      if( this.ValueMemberPath != null )
      {
        this.OnValueMemberPathChanged( null, this.ValueMemberPath );
      }
    }

    #endregion //Base Class Overrides

    #region Events

    public static readonly RoutedEvent SelectedEvent = EventManager.RegisterRoutedEvent( "SelectedEvent", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Selector ) );
    public static readonly RoutedEvent UnSelectedEvent = EventManager.RegisterRoutedEvent( "UnSelectedEvent", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Selector ) );

    public static readonly RoutedEvent ItemSelectionChangedEvent = EventManager.RegisterRoutedEvent( "ItemSelectionChanged", RoutingStrategy.Bubble, typeof( ItemSelectionChangedEventHandler ), typeof( Selector ) );
    public event ItemSelectionChangedEventHandler ItemSelectionChanged
    {
      add
      {
        AddHandler( ItemSelectionChangedEvent, value );
      }
      remove
      {
        RemoveHandler( ItemSelectionChangedEvent, value );
      }
    }

    #endregion //Events

    #region Methods

    protected object GetPathValue( object item, string propertyPath )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      if( String.IsNullOrEmpty( propertyPath )
        || propertyPath == "." )
        return item;


      PropertyInfo prop = item.GetType().GetProperty( propertyPath );
      return ( prop != null )
        ? prop.GetValue( item, null )
        : null;
    }

    protected object GetItemValue( object item )
    {
      return ( item != null )
        ? this.GetPathValue( item, this.ValueMemberPath )
        : null;
    }

    protected object ResolveItemByValue( string value )
    {
      if( !String.IsNullOrEmpty( ValueMemberPath ) )
      {
        foreach( object item in ItemsCollection )
        {
          var property = item.GetType().GetProperty( ValueMemberPath );
          if( property != null )
          {
            var propertyValue = property.GetValue( item, null );
            if( value.Equals( propertyValue.ToString(), StringComparison.InvariantCultureIgnoreCase ) )
              return item;
          }
        }
      }

      return value;
    }

    internal void UpdateFromList( List<string> selectedValues, Func<object, object> GetItemfunction )
    {
      _ignoreSelectedItemsCollectionChanged++;
      // Just update the SelectedItems collection content 
      // and let the synchronization be made from UpdateFromSelectedItems();
      SelectedItems.Clear();

      if( ( selectedValues != null ) && ( selectedValues.Count > 0 ) )
      {
        ValueEqualityComparer comparer = new ValueEqualityComparer();

        foreach( object item in ItemsCollection )
        {
          object itemValue = GetItemfunction( item );

          bool isSelected = ( itemValue != null )
            && selectedValues.Contains( itemValue.ToString(), comparer );

          if( isSelected )
          {
            SelectedItems.Add( item );
          }
        }
      }
      _ignoreSelectedItemsCollectionChanged--;

      this.UpdateFromSelectedItems();
    }

    internal void UpdateSelectedItemsWithoutNotifications( List<object> selectedValues )
    {
      _ignoreSelectedItemsCollectionChanged++;
      // Just update the SelectedItems collection content 
      // and let the synchronization be made from UpdateFromSelectedItems();
      this.SelectedItems.Clear();

      if( ( selectedValues != null ) && ( selectedValues.Count > 0 ) )
      {
        foreach( object item in this.ItemsCollection )
        {
          this.SelectedItems.Add( item );
        }
      }
      _ignoreSelectedItemsCollectionChanged--;

      this.UpdateFromSelectedItems();
    }

    private bool? GetSelectedMemberPathValue( object item )
    {
      if( String.IsNullOrEmpty( this.SelectedMemberPath ) )
        return null;
      if( item == null )
        return null;

      string[] nameParts = this.SelectedMemberPath.Split( '.' );
      if( nameParts.Length == 1 )
      {
        var property = item.GetType().GetProperty( this.SelectedMemberPath );
        if( ( property != null ) && ( property.PropertyType == typeof( bool ) ) )
          return property.GetValue( item, null ) as bool?;
        return null;
      }

      for( int i = 0; i < nameParts.Count(); ++i )
      {
        var type = item.GetType();
        var info = type.GetProperty( nameParts[ i ] );
        if( info == null )
        {
          return null;
        }

        if( i == nameParts.Count() - 1 )
        {
          if( info.PropertyType == typeof( bool ) )
            return info.GetValue( item, null ) as bool?;
        }
        else
        {
          item = info.GetValue( item, null );
        }
      }
      return null;
    }

    private void SetSelectedMemberPathValue( object item, bool value )
    {
      if( String.IsNullOrEmpty( this.SelectedMemberPath ) )
        return;
      if( item == null )
        return;

      string[] nameParts = this.SelectedMemberPath.Split( '.' );
      if( nameParts.Length == 1 )
      {
        var property = item.GetType().GetProperty( this.SelectedMemberPath );
        if( ( property != null ) && ( property.PropertyType == typeof( bool ) ) && ( ( bool )property.GetValue( item, null ) != value ) )
        {
          property.SetValue( item, value, null );
        }
        return;
      }

      for( int i = 0; i < nameParts.Count(); ++i )
      {
        var type = item.GetType();
        var info = type.GetProperty( nameParts[ i ] );
        if( info == null )
          return;

        if( i == nameParts.Count() - 1 )
        {
          if( info.PropertyType == typeof( bool ) )
          {
            info.SetValue( item, value, null );
          }
        }
        else
        {
          item = info.GetValue( item, null );
        }
      }
    }

    /// <summary>
    /// When SelectedItems collection implements INotifyPropertyChanged, this is the callback.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnSelectedItemsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( _ignoreSelectedItemsCollectionChanged > 0 )
        return;

      // Keep it simple for now. Just update all
      this.UpdateFromSelectedItems();

      if( e.Action == NotifyCollectionChangedAction.Reset )
      {
        if( _internalSelectedItems != null )
        {
          foreach( var item in _internalSelectedItems )
          {
            this.OnItemSelectionChanged( new ItemSelectionChangedEventArgs( Selector.ItemSelectionChangedEvent, this, item, false ) );

            if( Command != null )
            {
              this.Command.Execute( item );
            }
          }
        }
      }
      if( e.OldItems != null )
      {
        foreach( var item in e.OldItems )
        {
          this.OnItemSelectionChanged( new ItemSelectionChangedEventArgs( Selector.ItemSelectionChangedEvent, this, item, false ) );

          if( Command != null )
          {
            this.Command.Execute( item );
          }
        }
      }
      if( e.NewItems != null )
      {
        foreach( var item in e.NewItems )
        {
          this.OnItemSelectionChanged( new ItemSelectionChangedEventArgs( Selector.ItemSelectionChangedEvent, this, item, true ) );

          if( Command != null )
          {
            this.Command.Execute( item );
          }
        }
      }
    }

    private void OnItemSelectionChangedCore( RoutedEventArgs args, bool unselected )
    {
      object item = this.ItemContainerGenerator.ItemFromContainer( ( DependencyObject )args.OriginalSource );

      // When the item is it's own container, "UnsetValue" will be returned.
      if( item == DependencyProperty.UnsetValue )
      {
        item = args.OriginalSource;
      }

      if( unselected )
      {
        while( SelectedItems.Contains( item ) )
          SelectedItems.Remove( item );
      }
      else
      {
        if( !SelectedItems.Contains( item ) )
          SelectedItems.Add( item );
      }
    }

    /// <summary>
    /// When the ItemsSource implements INotifyPropertyChanged, this is the change callback.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnItemsSourceCollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
    {
      this.RemoveUnavailableSelectedItems();
      this.AddAvailableRemovedItems();
      this.UpdateSelectedMemberPathValuesBindings();
      this.UpdateValueMemberPathValuesBindings();
    }

    /// <summary>
    /// This is called when any value of any item referenced by SelectedMemberPath
    /// is modified. This may affect the SelectedItems collection.
    /// </summary>
    private void OnSelectedMemberPathValuesChanged()
    {
      if( _ignoreSelectedMemberPathValuesChanged > 0 )
        return;

      this.UpdateFromSelectedMemberPathValues();
    }

    /// <summary>
    /// This is called when any value of any item referenced by ValueMemberPath
    /// is modified. This will affect the SelectedValue property
    /// </summary>
    private void OnValueMemberPathValuesChanged()
    {
      this.UpdateSelectedValue();
    }

    private void UpdateSelectedMemberPathValuesBindings()
    {
      _selectedMemberPathValuesHelper.UpdateValueSource( ItemsCollection, SelectedMemberPath );
      this.UpdateFromSelectedMemberPathValues();
    }

    private void UpdateValueMemberPathValuesBindings()
    {
      _valueMemberPathValuesHelper.UpdateValueSource( ItemsCollection, ValueMemberPath );
    }

    /// <summary>
    /// This method will be called when the "IsSelected" property of an SelectorItem
    /// has been modified.
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnItemSelectionChanged( ItemSelectionChangedEventArgs args )
    {
      if( _surpressItemSelectionChanged )
        return;

      RaiseEvent( args );
    }

    /// <summary>
    /// Updates the SelectedValue property based on what is present in the SelectedItems property.
    /// </summary>
    private void UpdateSelectedValue()
    {
#if VS2008
      string newValue = String.Join( Delimiter, SelectedItems.Cast<object>().Select( x => GetItemValue( x ).ToString() ).ToArray() );
#else
      string newValue = String.Join( Delimiter, SelectedItems.Cast<object>().Select( x => GetItemValue( x ) ) );
#endif
      if( String.IsNullOrEmpty( SelectedValue ) || !SelectedValue.Equals( newValue ) )
      {
        _ignoreSelectedValueChanged = true;
        SelectedValue = newValue;
        _ignoreSelectedValueChanged = false;
      }
    }

    /// <summary>
    /// Updates the SelectedItem property based on what is present in the SelectedItems property.
    /// </summary>
    private void UpdateSelectedItem()
    {
      if( !SelectedItems.Contains( SelectedItem ) )
      {
        _ignoreSelectedItemChanged = true;
        SelectedItem = ( SelectedItems.Count > 0 ) ? SelectedItems[ 0 ] : null;
        _ignoreSelectedItemChanged = false;
      }
    }

    /// <summary>
    /// Update the SelectedItems collection based on the values 
    /// refered to by the SelectedMemberPath property.
    /// </summary>
    private void UpdateFromSelectedMemberPathValues()
    {
      _ignoreSelectedItemsCollectionChanged++;
      foreach( var item in this.ItemsCollection )
      {
        var isSelected = this.GetSelectedMemberPathValue( item );
        if( isSelected != null )
        {
          if( isSelected.Value )
          {
            if( !this.SelectedItems.Contains( item ) )
            {
              this.SelectedItems.Add( item );
            }
          }
          else
          {
            if( this.SelectedItems.Contains( item ) )
            {
              this.SelectedItems.Remove( item );
            }
          }

          this.UpdateSelectorItem( item, isSelected.Value );
        }
      }
      _ignoreSelectedItemsCollectionChanged--;

      this.UpdateSelectedItem();
      this.UpdateSelectedValue();

      this.UpdateInternalSelectedItems();
    }

    internal void UpdateSelectedItems( IList selectedItems )
    {
      if( selectedItems == null )
        throw new ArgumentNullException( "selectedItems" );

      // Just check if the collection is the same..
      if( selectedItems.Count == this.SelectedItems.Count
        && selectedItems.Cast<object>().SequenceEqual( this.SelectedItems.Cast<object>() ) )
        return;

      _ignoreSelectedItemsCollectionChanged++;
      this.SelectedItems.Clear();
      foreach( object newItem in selectedItems )
      {
        this.SelectedItems.Add( newItem );
      }
      _ignoreSelectedItemsCollectionChanged--;
      this.UpdateFromSelectedItems();
    }

    /// <summary>
    /// Updates the following based on the content of SelectedItems:
    /// - All SelectorItems "IsSelected" properties
    /// - Values refered to by SelectedMemberPath
    /// - SelectedItem property
    /// - SelectedValue property
    /// Refered to by the SelectedMemberPath property.
    /// </summary>
    private void UpdateFromSelectedItems()
    {
      foreach( var o in this.ItemsCollection )
      {
        bool isSelected = this.SelectedItems.Contains( o );

        _ignoreSelectedMemberPathValuesChanged++;
        this.SetSelectedMemberPathValue( o, isSelected );
        _ignoreSelectedMemberPathValuesChanged--;

        this.UpdateSelectorItem( o, isSelected );
      }

      this.UpdateSelectedItem();
      this.UpdateSelectedValue();

      this.UpdateInternalSelectedItems();
    }

    private void UpdateInternalSelectedItems()
    {
      _internalSelectedItems = new object[ this.SelectedItems.Count ];
      this.SelectedItems.CopyTo( _internalSelectedItems, 0 );
    }

    private void UpdateSelectorItem( object item, bool isSelected )
    {
      var selectorItem = ItemContainerGenerator.ContainerFromItem( item ) as SelectorItem;
      if( selectorItem != null )
      {
        selectorItem.IsSelected = isSelected;
      }
    }

    /// <summary>
    /// Removes all items from SelectedItems that are no longer in ItemsSource.
    /// </summary>
    private void RemoveUnavailableSelectedItems()
    {
      _ignoreSelectedItemsCollectionChanged++;
      HashSet<object> hash = new HashSet<object>( ItemsCollection.Cast<object>() );

      for( int i = 0; i < SelectedItems.Count; i++ )
      {
        if( !hash.Contains( SelectedItems[ i ] ) )
        {
          _removedItems.Add( SelectedItems[ i ] );
          SelectedItems.RemoveAt( i );
          i--;
        }
      }
      _ignoreSelectedItemsCollectionChanged--;

      UpdateSelectedItem();
      UpdateSelectedValue();
    }

    private void AddAvailableRemovedItems()
    {
      HashSet<object> hash = new HashSet<object>( ItemsCollection.Cast<object>() );

      for( int i = 0; i < _removedItems.Count; i++ )
      {
        if( hash.Contains( _removedItems[ i ] ) )
        {
          SelectedItems.Add( _removedItems[ i ] );
          _removedItems.RemoveAt( i );
          i--;
        }
      }
    }

    /// <summary>
    /// Updates the SelectedItems collection based on the content of
    /// the SelectedValue property.
    /// </summary>
    private void UpdateFromSelectedValue()
    {
      List<string> selectedValues = null;
      if( !String.IsNullOrEmpty( SelectedValue ) )
      {
        selectedValues = SelectedValue.Split( new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries ).ToList();
      }

      this.UpdateFromList( selectedValues, this.GetItemValue );
    }

    #endregion //Methods

    #region IWeakEventListener Members

    public bool ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        if( object.ReferenceEquals( _selectedItems, sender ) )
        {
          this.OnSelectedItemsCollectionChanged( sender, ( NotifyCollectionChangedEventArgs )e );
          return true;
        }
        else if( object.ReferenceEquals( ItemsCollection, sender ) )
        {
          this.OnItemsSourceCollectionChanged( sender, ( NotifyCollectionChangedEventArgs )e );
          return true;
        }
      }

      return false;
    }

    #endregion

    #region ValueEqualityComparer private class

    private class ValueEqualityComparer : IEqualityComparer<string>
    {
      public bool Equals( string x, string y )
      {
        return string.Equals( x, y, StringComparison.InvariantCultureIgnoreCase );
      }

      public int GetHashCode( string obj )
      {
        return 1;
      }
    }

    #endregion
  }


  public delegate void ItemSelectionChangedEventHandler( object sender, ItemSelectionChangedEventArgs e );
  public class ItemSelectionChangedEventArgs : RoutedEventArgs
  {
    public bool IsSelected
    {
      get;
      private set;
    }
    public object Item
    {
      get;
      private set;
    }

    public ItemSelectionChangedEventArgs( RoutedEvent routedEvent, object source, object item, bool isSelected )
      : base( routedEvent, source )
    {
      Item = item;
      IsSelected = isSelected;
    }
  }
}
