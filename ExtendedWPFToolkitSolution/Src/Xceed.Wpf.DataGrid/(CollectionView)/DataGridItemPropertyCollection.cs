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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Xceed.Utils.Wpf;
using Xceed.Wpf.DataGrid.Utils;

namespace Xceed.Wpf.DataGrid
{
  [DebuggerDisplay( "Count = {Count}" )]
  public sealed class DataGridItemPropertyCollection :
    IList<DataGridItemPropertyBase>,
    IList,
    ICollection<DataGridItemPropertyBase>,
    ICollection,
    IEnumerable<DataGridItemPropertyBase>,
    IEnumerable,
    INotifyCollectionChanged,
    INotifyPropertyChanged
  {
    #region Static Fields

    private static readonly string CountPropertyName = PropertyHelper.GetPropertyName( ( DataGridItemPropertyCollection c ) => c.Count );
    private static readonly string ItemsPropertyName = "Item[]";

    #endregion

    internal DataGridItemPropertyCollection()
      : this( null )
    {
    }

    internal DataGridItemPropertyCollection( DataGridItemPropertyBase owner )
    {
      m_owner = owner;
    }

    #region [] Property

    public DataGridItemPropertyBase this[ string name ]
    {
      get
      {
        if( !string.IsNullOrEmpty( name ) )
        {
          DataGridItemPropertyBase item;
          if( m_nameToItem.TryGetValue( name, out item ) )
            return item;
        }

        return null;
      }
      set
      {
        if( value == null )
          throw new ArgumentNullException( "value" );

        if( string.IsNullOrEmpty( value.Name ) )
          throw new ArgumentException( "An attempt was made to add an item that does not have a name.", "value" );

        if( value.Name != name )
          throw new ArgumentException( "The item's name is not the same as the parameter.", "name" );

        var oldItem = this[ name ];
        if( oldItem == null )
        {
          this.InsertItemAndNotify( m_collection.Count, value );
        }
        else
        {
          if( value == oldItem )
            return;

          var index = this.IndexOf( oldItem );
          Debug.Assert( index >= 0 );

          this.ReplaceItemAndNotify( index, oldItem, value );
        }
      }
    }

    #endregion

    #region DataGridItemPropertyBase Internal Owner

    internal DataGridItemPropertyBase Owner
    {
      get
      {
        return m_owner;
      }
    }

    private readonly DataGridItemPropertyBase m_owner;

    #endregion

    #region SyncRoot Private Property

    private object SyncRoot
    {
      get
      {
        return ( ( ICollection )this ).SyncRoot;
      }
    }

    #endregion

    #region ItemPropertyGroupSortStatNameChanged Internal Event

    internal event EventHandler ItemPropertyGroupSortStatNameChanged;

    private void OnItemPropertyGroupSortStatNameChanged()
    {
      var handler = this.ItemPropertyGroupSortStatNameChanged;
      if( handler == null )
        return;

      handler.Invoke( this, EventArgs.Empty );
    }

    #endregion

    #region InitializeItemProperty Internal Event

    internal event EventHandler<InitializeItemPropertyEventArgs> InitializeItemProperty;

    private void OnInitializeItemProperty( DataGridItemPropertyBase itemProperty )
    {
      var handler = this.InitializeItemProperty;
      if( handler == null )
        return;

      handler.Invoke( this, new InitializeItemPropertyEventArgs( itemProperty ) );
    }

    private void RelayInitializeItemProperty( InitializeItemPropertyEventArgs e )
    {
      var handler = this.InitializeItemProperty;
      if( handler == null )
        return;

      Debug.Assert( e != null );

      handler.Invoke( this, e );
    }

    #endregion

    internal IDisposable DeferCollectionChanged()
    {
      return new DeferredDisposable( new DeferState( this ) );
    }

    internal bool Contains( string name )
    {
      if( string.IsNullOrEmpty( name ) )
        return false;

      return m_nameToItem.ContainsKey( name );
    }

    internal DataGridItemPropertyBase GetForSynonym( string name )
    {
      if( string.IsNullOrEmpty( name ) )
        return null;

      DataGridItemPropertyBase[] synonyms;
      if( !m_synonymToItem.TryGetValue( name, out synonyms ) )
        return null;

      Debug.Assert( synonyms.Length > 0 );
      if( synonyms.Length <= 0 )
        return null;

      return synonyms[ 0 ];
    }

    internal void RefreshUnboundItemProperty( object component )
    {
      if( m_unboundItems.Count <= 0 || ( component == null ) )
        return;

      var unboundDataItem = UnboundDataItem.GetUnboundDataItem( component );
      if( unboundDataItem == null )
        return;

      var dataItem = unboundDataItem.DataItem;
      if( ( dataItem == null ) || ( dataItem is EmptyDataItem ) )
        return;

      State state;
      if( m_dataItems.TryGetValue( dataItem, out state ) && ( state.Refreshing || state.Suspended ) )
        return;

      m_dataItems.Add( dataItem, new State( true, false ) );

      try
      {
        foreach( var item in m_unboundItems )
        {
          item.Refresh( unboundDataItem );
        }
      }
      finally
      {
        state = m_dataItems[ dataItem ];

        if( state.Suspended )
        {
          m_dataItems[ dataItem ] = new State( false, true );
        }
        else
        {
          m_dataItems.Remove( dataItem );
        }
      }
    }

    internal void SuspendUnboundItemPropertyChanged( object component )
    {
      if( component == null )
        return;

      var unboundDataItem = UnboundDataItem.GetUnboundDataItem( component );
      if( unboundDataItem == null )
        return;

      var dataItem = unboundDataItem.DataItem;
      if( ( dataItem == null ) || ( dataItem is EmptyDataItem ) )
        return;

      State state;
      bool refreshing = false;

      if( m_dataItems.TryGetValue( dataItem, out state ) )
      {
        if( state.Suspended )
          return;

        refreshing = state.Refreshing;
      }

      m_dataItems[ dataItem ] = new State( refreshing, true );
    }

    internal void ResumeUnboundItemPropertyChanged( object component )
    {
      if( component == null )
        return;

      var unboundDataItem = UnboundDataItem.GetUnboundDataItem( component );
      if( unboundDataItem == null )
        return;

      var dataItem = unboundDataItem.DataItem;
      if( ( dataItem == null ) || ( dataItem is EmptyDataItem ) )
        return;

      State state;
      if( !m_dataItems.TryGetValue( dataItem, out state ) || !state.Suspended )
      {
        Debug.Fail( "The item is not suspended." );
        return;
      }

      if( state.Refreshing )
      {
        m_dataItems[ dataItem ] = new State( true, false );
      }
      else
      {
        m_dataItems.Remove( dataItem );

        this.RefreshUnboundItemProperty( dataItem );
      }
    }

    private static NotifyCollectionChangedEventArgs Combine( NotifyCollectionChangedEventArgs x, NotifyCollectionChangedEventArgs y )
    {
      if( x == null )
        return y;

      if( y == null )
        return x;

      var addedItems = DataGridItemPropertyCollection.GetAddedItems( x ).ToList();
      var removedItems = DataGridItemPropertyCollection.GetRemovedItems( x ).ToList();

      foreach( var item in DataGridItemPropertyCollection.GetAddedItems( y ) )
      {
        if( !removedItems.Remove( item ) )
        {
          Debug.Assert( !addedItems.Contains( item ) );
          addedItems.Add( item );
        }
      }

      foreach( var item in DataGridItemPropertyCollection.GetRemovedItems( y ) )
      {
        if( !addedItems.Remove( item ) )
        {
          Debug.Assert( !removedItems.Contains( item ) );
          removedItems.Add( item );
        }
      }

      var addedItemsCount = addedItems.Count;
      var removedItemsCount = removedItems.Count;

      if( ( addedItemsCount > 0 ) && ( removedItemsCount > 0 ) )
      {
        if( ( x.Action != NotifyCollectionChangedAction.Replace ) || ( y.Action != NotifyCollectionChangedAction.Replace ) || ( addedItemsCount != removedItemsCount ) )
          throw new NotSupportedException();

        return new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, addedItems, removedItems );
      }

      if( addedItemsCount > 0 )
        return new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, addedItems );

      if( removedItemsCount > 0 )
        return new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, removedItems );

      return null;
    }

    private static IEnumerable<DataGridItemPropertyBase> GetAddedItems( NotifyCollectionChangedEventArgs source )
    {
      if( ( source != null ) && ( source.NewItems != null ) )
        return source.NewItems.Cast<DataGridItemPropertyBase>();

      return Enumerable.Empty<DataGridItemPropertyBase>();
    }

    private static IEnumerable<DataGridItemPropertyBase> GetRemovedItems( NotifyCollectionChangedEventArgs source )
    {
      if( ( source != null ) && ( source.OldItems != null ) )
        return source.OldItems.Cast<DataGridItemPropertyBase>();

      return Enumerable.Empty<DataGridItemPropertyBase>();
    }

    private void InsertItemAndNotify( int index, DataGridItemPropertyBase item )
    {
      this.InsertItem( index, item );

      this.OnPropertyChanged( DataGridItemPropertyCollection.CountPropertyName );
      this.OnPropertyChanged( DataGridItemPropertyCollection.ItemsPropertyName );
      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, item, index ) );
    }

    private void ReplaceItemAndNotify( int index, DataGridItemPropertyBase oldItem, DataGridItemPropertyBase newItem )
    {
      this.RemoveItem( index, oldItem );
      this.InsertItem( index, newItem );

      this.OnPropertyChanged( DataGridItemPropertyCollection.ItemsPropertyName );
      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, newItem, oldItem, index ) );
    }

    private void RemoveItemAndNotify( int index, DataGridItemPropertyBase item )
    {
      this.RemoveItem( index, item );

      this.OnPropertyChanged( DataGridItemPropertyCollection.CountPropertyName );
      this.OnPropertyChanged( DataGridItemPropertyCollection.ItemsPropertyName );
      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, item, index ) );
    }

    private void InsertItem( int index, DataGridItemPropertyBase item )
    {
      Debug.Assert( ( index >= 0 ) && ( index <= m_collection.Count ) );
      Debug.Assert( item != null );
      Debug.Assert( !string.IsNullOrEmpty( item.Name ) );

      var name = item.Name;
      if( m_nameToItem.ContainsKey( name ) )
        throw new InvalidOperationException( "An item with the same name is already in the collection." );

      if( item.ContainingCollection != null )
        throw new InvalidOperationException( "The item is already contained in a collection." );

      item.IsNameSealed = true;

      m_collection.Insert( index, item );
      m_nameToItem.Add( name, item );

      var unboundItem = item as DataGridUnboundItemProperty;
      if( unboundItem != null )
      {
        m_unboundItems.Add( unboundItem );
      }

      item.AttachToContainingCollection( this );

      this.OnInitializeItemProperty( item );
      this.RegisterEvents( item );

      item.IsSealed = true;
    }

    private void RemoveItem( int index, DataGridItemPropertyBase item )
    {
      Debug.Assert( ( index >= 0 ) && ( index < m_collection.Count ) );
      Debug.Assert( item != null );
      Debug.Assert( object.ReferenceEquals( item, m_collection[ index ] ) );

      this.UnregisterEvents( item );

      m_collection.RemoveAt( index );
      m_nameToItem.Remove( item.Name );

      var unboundItem = item as DataGridUnboundItemProperty;
      if( unboundItem != null )
      {
        m_unboundItems.Remove( unboundItem );
      }

      this.RemoveSynonym( item );
      this.ClearItem( item );
    }

    private void AddSynonym( DataGridItemPropertyBase item )
    {
      Debug.Assert( item != null );

      var synonym = item.Synonym;
      if( string.IsNullOrEmpty( synonym ) )
        return;

      DataGridItemPropertyBase[] synonyms;
      if( m_synonymToItem.TryGetValue( synonym, out synonyms ) )
      {
        Array.Resize( ref synonyms, synonyms.Length + 1 );
      }
      else
      {
        Array.Resize( ref synonyms, 1 );
      }

      synonyms[ synonyms.Length - 1 ] = item;
      m_synonymToItem[ synonym ] = synonyms;
    }

    private void RemoveSynonym( DataGridItemPropertyBase item )
    {
      Debug.Assert( item != null );

      var synonym = item.Synonym;
      if( string.IsNullOrEmpty( synonym ) )
        return;

      DataGridItemPropertyBase[] synonyms;
      if( !m_synonymToItem.TryGetValue( synonym, out synonyms ) )
        return;

      var removeAt = Array.IndexOf( synonyms, item );
      Debug.Assert( removeAt >= 0 );

      if( removeAt < 0 )
        return;

      if( synonyms.Length > 1 )
      {
        for( int i = removeAt + 1; i < synonyms.Length; i++ )
        {
          synonyms[ i - 1 ] = synonyms[ i ];
        }

        Array.Resize( ref synonyms, synonyms.Length - 1 );

        m_synonymToItem[ synonym ] = synonyms;
      }
      else
      {
        m_synonymToItem.Remove( synonym );
      }
    }

    private void ClearItem( DataGridItemPropertyBase item )
    {
      Debug.Assert( item != null );

      item.IsNameSealed = false;
      item.IsSealed = false;
      item.DetachFromContainingCollection();
    }

    private void RegisterEvents( DataGridItemPropertyBase item )
    {
      Debug.Assert( item != null );

      item.PropertyChanged += new PropertyChangedEventHandler( this.OnItemPropertyChanged );
      item.ValueChanged += new EventHandler<DataGridItemPropertyBase.ValueChangedEventArgs>( this.OnItemValueChanged );

      if( item.ItemPropertiesInternal != null )
      {
        item.ItemPropertiesInternal.InitializeItemProperty += new EventHandler<InitializeItemPropertyEventArgs>( this.OnItemInitializeItemProperty );
      }
    }

    private void UnregisterEvents( DataGridItemPropertyBase item )
    {
      Debug.Assert( item != null );

      item.PropertyChanged -= new PropertyChangedEventHandler( this.OnItemPropertyChanged );
      item.ValueChanged -= new EventHandler<DataGridItemPropertyBase.ValueChangedEventArgs>( this.OnItemValueChanged );

      if( item.ItemPropertiesInternal != null )
      {
        item.ItemPropertiesInternal.InitializeItemProperty -= new EventHandler<InitializeItemPropertyEventArgs>( this.OnItemInitializeItemProperty );
      }
    }

    private void OnItemPropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      var item = ( DataGridItemPropertyBase )sender;
      var propertyName = e.PropertyName;

      if( string.IsNullOrEmpty( e.PropertyName ) || ( propertyName == DataGridItemPropertyBase.GroupSortStatResultPropertyNamePropertyName ) )
      {
        this.OnItemPropertyGroupSortStatNameChanged();
      }

      if( string.IsNullOrEmpty( e.PropertyName ) || ( propertyName == DataGridItemPropertyBase.ItemPropertiesInternalPropertyName ) )
      {
        this.OnItemItemPropertiesInternalChanged( ( DataGridItemPropertyBase )sender );
      }

      if( string.IsNullOrEmpty( e.PropertyName ) || ( propertyName == DataGridItemPropertyBase.IsSealedPropertyName ) )
      {
        if( item.IsSealed )
        {
          this.AddSynonym( item );
        }
        else
        {
          this.RemoveSynonym( item );
        }
      }
    }

    private void OnItemValueChanged( object sender, DataGridItemPropertyBase.ValueChangedEventArgs e )
    {
      this.RefreshUnboundItemProperty( e.Component );
    }

    private void OnItemItemPropertiesInternalChanged( DataGridItemPropertyBase itemProperty )
    {
      if( ( itemProperty == null ) || ( itemProperty.ItemPropertiesInternal == null ) )
        return;

      itemProperty.ItemPropertiesInternal.InitializeItemProperty += new EventHandler<InitializeItemPropertyEventArgs>( this.OnItemInitializeItemProperty );
    }

    private void OnItemInitializeItemProperty( object sender, InitializeItemPropertyEventArgs e )
    {
      var itemProperties = sender as DataGridItemPropertyCollection;
      if( itemProperties == null )
        return;

      this.RelayInitializeItemProperty( e );
    }

    #region IList<> Members

    public DataGridItemPropertyBase this[ int index ]
    {
      get
      {
        if( ( index < 0 ) || ( index >= m_collection.Count ) )
          throw new ArgumentOutOfRangeException( "index" );

        return m_collection[ index ];
      }
      set
      {
        if( ( index < 0 ) || ( index > m_collection.Count ) )
          throw new ArgumentOutOfRangeException( "index" );

        if( value == null )
          throw new ArgumentNullException( "value" );

        if( string.IsNullOrEmpty( value.Name ) )
          throw new ArgumentException( "The item must have a non empty name.", "value" );

        if( index < m_collection.Count )
        {
          var oldItem = m_collection[ index ];
          if( value == oldItem )
            return;

          this.ReplaceItemAndNotify( index, oldItem, value );
        }
        else
        {
          this.InsertItemAndNotify( index, value );
        }
      }
    }

    public int IndexOf( DataGridItemPropertyBase item )
    {
      if( !this.Contains( item ) )
        return -1;

      return m_collection.IndexOf( item );
    }

    public void Insert( int index, DataGridItemPropertyBase item )
    {
      if( ( index < 0 ) || ( index > m_collection.Count ) )
        throw new ArgumentOutOfRangeException( "index" );

      if( item == null )
        throw new ArgumentNullException( "item" );

      if( string.IsNullOrEmpty( item.Name ) )
        throw new ArgumentException( "The item must have a non empty name.", "item" );

      this.InsertItemAndNotify( index, item );
    }

    public void RemoveAt( int index )
    {
      if( ( index < 0 ) || ( index >= m_collection.Count ) )
        throw new ArgumentOutOfRangeException( "index" );

      this.RemoveItemAndNotify( index, m_collection[ index ] );
    }

    #endregion

    #region IList Members

    bool IList.IsFixedSize
    {
      get
      {
        return false;
      }
    }

    bool IList.IsReadOnly
    {
      get
      {
        return ( ( ICollection<DataGridItemPropertyBase> )this ).IsReadOnly;
      }
    }

    object IList.this[ int index ]
    {
      get
      {
        return this[ index ];
      }
      set
      {
        this[ index ] = ( DataGridItemPropertyBase )value;
      }
    }

    int IList.Add( object value )
    {
      var item = value as DataGridItemPropertyBase;
      if( item == null )
        return -1;

      this.Add( item );

      return m_collection.Count - 1;
    }

    bool IList.Contains( object value )
    {
      return this.Contains( value as DataGridItemPropertyBase );
    }

    int IList.IndexOf( object value )
    {
      var item = value as DataGridItemPropertyBase;
      if( !this.Contains( item ) )
        return -1;

      return m_collection.IndexOf( item );
    }

    void IList.Insert( int index, object value )
    {
      this.Insert( index, ( DataGridItemPropertyBase )value );
    }

    void IList.Remove( object value )
    {
      this.Remove( value as DataGridItemPropertyBase );
    }

    void IList.RemoveAt( int index )
    {
      this.RemoveAt( index );
    }

    #endregion

    #region ICollection<> Members

    bool ICollection<DataGridItemPropertyBase>.IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public void Add( DataGridItemPropertyBase item )
    {
      this.Insert( m_collection.Count, item );
    }

    public void Clear()
    {
      if( m_collection.Count == 0 )
        return;

      var removedItems = m_collection.ToList();

      m_collection.Clear();
      m_nameToItem.Clear();
      m_synonymToItem.Clear();
      m_unboundItems.Clear();

      foreach( var item in removedItems )
      {
        this.UnregisterEvents( item );
        this.ClearItem( item );
      }

      this.OnPropertyChanged( DataGridItemPropertyCollection.CountPropertyName );
      this.OnPropertyChanged( DataGridItemPropertyCollection.ItemsPropertyName );
      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, removedItems ) );
    }

    public bool Contains( DataGridItemPropertyBase item )
    {
      if( item == null )
        return false;

      var name = item.Name;
      if( string.IsNullOrEmpty( name ) )
        return false;

      DataGridItemPropertyBase stored;
      if( !m_nameToItem.TryGetValue( name, out stored ) )
        return false;

      return ( item == stored );
    }

    void ICollection<DataGridItemPropertyBase>.CopyTo( DataGridItemPropertyBase[] array, int index )
    {
      m_collection.CopyTo( array, index );
    }

    public bool Remove( DataGridItemPropertyBase item )
    {
      if( !this.Contains( item ) )
        return false;

      var index = m_collection.IndexOf( item );
      Debug.Assert( index >= 0 );

      this.RemoveItemAndNotify( index, item );

      return true;
    }

    #endregion

    #region ICollection Members

    public int Count
    {
      get
      {
        return m_collection.Count;
      }
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        return false;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        return ( ( ICollection )m_collection ).SyncRoot;
      }
    }

    void ICollection.CopyTo( Array array, int index )
    {
      ( ( ICollection )m_collection ).CopyTo( array, index );
    }

    #endregion

    #region IEnumerable<> Members

    public IEnumerator<DataGridItemPropertyBase> GetEnumerator()
    {
      return m_collection.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion

    #region INotifyCollectionChanged Members

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    private void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      if( e.Action == NotifyCollectionChangedAction.Reset )
        throw new NotSupportedException();

      var handler = this.CollectionChanged;
      if( handler == null )
        return;

      lock( this.SyncRoot )
      {
        if( m_deferCollectionChangedCount != 0 )
        {
          m_deferCollectionChangedEventArgs = DataGridItemPropertyCollection.Combine( m_deferCollectionChangedEventArgs, e );
          return;
        }
      }

      handler.Invoke( this, e );
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged( string propertyName )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    #endregion

    private int m_deferCollectionChangedCount; //0
    private NotifyCollectionChangedEventArgs m_deferCollectionChangedEventArgs; //null

    private readonly List<DataGridItemPropertyBase> m_collection = new List<DataGridItemPropertyBase>();
    private readonly Dictionary<string, DataGridItemPropertyBase> m_nameToItem = new Dictionary<string, DataGridItemPropertyBase>();
    private readonly Dictionary<string, DataGridItemPropertyBase[]> m_synonymToItem = new Dictionary<string, DataGridItemPropertyBase[]>();
    private readonly HashSet<DataGridUnboundItemProperty> m_unboundItems = new HashSet<DataGridUnboundItemProperty>();
    private readonly Dictionary<object, State> m_dataItems = new Dictionary<object, State>( 0 );

    #region DeferState Private Class

    private sealed class DeferState : DeferredDisposableState
    {
      internal DeferState( DataGridItemPropertyCollection target )
      {
        Debug.Assert( target != null );
        m_target = target;
      }

      protected override object SyncRoot
      {
        get
        {
          return m_target.SyncRoot;
        }
      }

      protected override bool IsDeferred
      {
        get
        {
          return ( m_target.m_deferCollectionChangedCount != 0 );
        }
      }

      protected override void Increment()
      {
        m_target.m_deferCollectionChangedCount++;
      }

      protected override void Decrement()
      {
        m_target.m_deferCollectionChangedCount--;
      }

      protected override void OnDeferEnding( bool disposing )
      {
        m_eventArgs = m_target.m_deferCollectionChangedEventArgs;
        m_target.m_deferCollectionChangedEventArgs = null;

        base.OnDeferEnding( disposing );
      }

      protected override void OnDeferEnded( bool disposing )
      {
        if( m_eventArgs == null )
          return;

        m_target.OnCollectionChanged( m_eventArgs );
      }

      private readonly DataGridItemPropertyCollection m_target;
      private NotifyCollectionChangedEventArgs m_eventArgs;
    }

    #endregion

    #region State Private Struct

    private struct State
    {
      internal State( bool refreshing, bool suspended )
      {
        this.Refreshing = refreshing;
        this.Suspended = suspended;
      }

      internal readonly bool Refreshing;
      internal readonly bool Suspended;
    }

    #endregion
  }
}
