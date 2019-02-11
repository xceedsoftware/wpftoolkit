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
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridCollectionViewGroup : CollectionViewGroup, ICustomTypeDescriptor
  {
    protected DataGridCollectionViewGroup( int capacity )
      : this( null, null, 0, capacity, 4 )
    {
    }

    protected DataGridCollectionViewGroup( DataGridCollectionViewGroup template, DataGridCollectionViewGroup parent )
      : this( template.Name, parent, template.m_unsortedIndex, template.m_sortedRawItems.Count, template.m_subGroups.Count )
    {
      m_nextSubGroupUnsortedIndex = template.m_nextSubGroupUnsortedIndex;
      m_subGroupBy = template.m_subGroupBy;
      m_groupByName = template.GroupByName;
    }

    private DataGridCollectionViewGroup( object name, DataGridCollectionViewGroup parent, int unsortedIndex )
      : this( name, parent, unsortedIndex, 4, 4 )
    {
    }

    private DataGridCollectionViewGroup( object name, DataGridCollectionViewGroup parent, int unsortedIndex, int rawCapacity, int groupCapacity )
      : base( name )
    {
      m_parent = parent;
      m_unsortedIndex = unsortedIndex;
      m_protectedItems = ObservableCollectionHelper.GetItems( base.ProtectedItems );
      m_protectedItemsCollectionChanged = ObservableCollectionHelper.GetCollectionChanged( base.ProtectedItems );
      m_optimizedItems = new OptimizedReadOnlyObservableCollection( this );
      m_subGroups = new Dictionary<object, DataGridCollectionViewGroup>( groupCapacity );
      m_sortedRawItems = new List<RawItem>( rawCapacity );
    }

    #region IsBottomLevel Property

    public override bool IsBottomLevel
    {
      get
      {
        // returns true if .Items contain DataItem
        return ( m_subGroupBy == null );
      }
    }

    #endregion

    #region Items Property

    internal new ReadOnlyObservableCollection<object> Items
    {
      get
      {
        return m_optimizedItems;
      }
    }

    #endregion

    #region ProtectedItems Property

    internal new ObservableCollection<object> ProtectedItems
    {
      get
      {
        return base.ProtectedItems;
      }
    }

    #endregion

    #region UnsortedIndex Property

    internal int UnsortedIndex
    {
      get
      {
        return m_unsortedIndex;
      }
    }

    #endregion

    #region GroupByName Property

    internal string GroupByName
    {
      get
      {
        return m_groupByName;
      }
      set
      {
        m_groupByName = value;
      }
    }

    private string m_groupByName;

    #endregion

    #region SubGroupBy Property

    internal GroupDescription SubGroupBy
    {
      get
      {
        return m_subGroupBy;
      }
    }

    #endregion

    #region Parent Property

    internal DataGridCollectionViewGroup Parent
    {
      get
      {
        return m_parent;
      }
    }

    #endregion

    #region GlobalRawItemCount Property

    internal int GlobalRawItemCount
    {
      get
      {
        return m_globalRawItemCount;
      }
    }

    #endregion

    #region RawItems Property

    internal List<RawItem> RawItems
    {
      get
      {
        return m_sortedRawItems;
      }
    }

    #endregion

    protected virtual DataGridCollectionView GetCollectionView()
    {
      if( m_parent != null )
        return m_parent.GetCollectionView();

      return null;
    }

    internal bool Contains( object item )
    {
      var group = item as DataGridCollectionViewGroup;
      if( group != null )
      {
        //Must make sure the group the group is ref equals, because there can be groups with a null name at more than one level.
        DataGridCollectionViewGroup foundGroup;
        if( m_subGroups.TryGetValue( DataGridCollectionViewGroup.GetHashKeyFromName( group.Name ), out foundGroup ) )
          return ( foundGroup == group );

        return false;
      }

      DataGridCollectionView collectionView = this.GetCollectionView();
      if( collectionView != null )
      {
        RawItem rawItem = collectionView.GetFirstRawItemFromDataItem( item );
        if( rawItem != null )
          return ( rawItem.ParentGroup == this );
      }

      return false;
    }

    internal int IndexOf( object item )
    {
      if( item is DataGridCollectionViewGroup )
        return this.ProtectedItems.IndexOf( item );

      DataGridCollectionView collectionView = this.GetCollectionView();

      if( collectionView != null )
      {
        RawItem rawItem = collectionView.GetFirstRawItemFromDataItem( item );

        if( ( rawItem != null ) && ( rawItem.ParentGroup == this ) )
          return rawItem.SortedIndex;
      }

      return -1;
    }

    internal int GetFirstRawItemGlobalSortedIndex()
    {
      var index = 0;
      var group = this;
      var parent = this.Parent;
      var currentGroup = default( DataGridCollectionViewGroup );

      while( parent != null )
      {
        var items = parent.ProtectedItems;
        var count = items.Count;

        for( int i = 0; i < count; i++ )
        {
          var value = items[ i ];
          if( value == group )
            break;

          currentGroup = value as DataGridCollectionViewGroup;
          index += currentGroup.GlobalRawItemCount;
        }

        group = parent;
        parent = parent.Parent;
      }

      return index;
    }

    internal void SetSubGroupBy( GroupDescription groupBy )
    {
      bool oldIsBottomLevel = this.IsBottomLevel;
      m_subGroupBy = groupBy;

      if( oldIsBottomLevel != this.IsBottomLevel )
      {
        this.OnPropertyChanged( new PropertyChangedEventArgs( "IsBottomLevel" ) );
      }
    }

    internal DataGridCollectionViewGroup GetGroup(
      RawItem rawItem,
      int level,
      CultureInfo culture,
      ObservableCollection<GroupDescription> groupByList,
      List<GroupSortComparer> groupSortComparers )
    {
      // If sortComparers is null, we are in massive group creation, no order check.

      if( this.IsBottomLevel )
        throw new InvalidOperationException( "An attempt was made to get a group for which a GroupDescription has not been provided." );

      object groupName = m_subGroupBy.GroupNameFromItem( rawItem.DataItem, level, culture );
      DataGridCollectionViewGroup group;

      if( ( m_subGroupBy is DataGridGroupDescription ) || ( m_subGroupBy is PropertyGroupDescription ) )
      {
        m_subGroups.TryGetValue( DataGridCollectionViewGroup.GetHashKeyFromName( groupName ), out group );
      }
      else
      {
        //If dealing with an unknown GroupDescription type, use the standard method to retrieve a group, in case group retrival is handle differently.
        group = null;

        foreach( var tempGroup in m_subGroups.Values )
        {
          if( m_subGroupBy.NamesMatch( tempGroup.Name, groupName ) )
          {
            group = tempGroup;
            break;
          }
        }
      }

      if( group == null )
      {
        group = this.CreateSubGroup( groupName, level, groupByList, groupSortComparers );
      }

      return group;
    }

    internal void SortItems(
      IList<SortDescriptionInfo> sortDescriptionInfos,
      List<GroupSortComparer> groupSortComparers,
      int level,
      List<RawItem> globalRawItems,
      DataGridCollectionViewGroup newSortedGroup )
    {
      var itemCount = this.ProtectedItemCount;
      if( itemCount == 0 )
        return;

      if( this.IsBottomLevel )
      {
        var indexes = new int[ itemCount + 1 ];

        for( int i = 0; i < itemCount; i++ )
        {
          indexes[ i ] = m_sortedRawItems[ i ].Index;
        }

        // "Weak heap sort" sort array[0..NUM_ELEMENTS-1] to array[1..NUM_ELEMENTS]
        var collectionViewSort = new DataGridCollectionViewSort( indexes, sortDescriptionInfos );

        collectionViewSort.Sort( itemCount );
        var index = 0;

        for( int i = 1; i <= itemCount; i++ )
        {
          newSortedGroup.InsertRawItem( index, globalRawItems[ indexes[ i ] ] );
          index++;
        }
      }
      else
      {
        var indexes = new int[ itemCount + 1 ];

        for( int i = 0; i < itemCount; i++ )
        {
          indexes[ i ] = i;
        }

        var subGroupsArray = new DataGridCollectionViewGroup[ itemCount ];
        m_subGroups.Values.CopyTo( subGroupsArray, 0 );

        // "Weak heap sort" sort array[0..NUM_ELEMENTS-1] to array[1..NUM_ELEMENTS]
        var collectionViewSort = new DataGridCollectionViewGroupSort( indexes, groupSortComparers[ level ], subGroupsArray );

        collectionViewSort.Sort( itemCount );
        int index = 0;
        level++;

        for( int i = 1; i <= itemCount; i++ )
        {
          DataGridCollectionViewGroup oldGroup = subGroupsArray[ indexes[ i ] ];
          DataGridCollectionViewGroup newGroup = new DataGridCollectionViewGroup( oldGroup, newSortedGroup );

          // Sort sub items
          oldGroup.SortItems( sortDescriptionInfos, groupSortComparers, level, globalRawItems, newGroup );

          newSortedGroup.InsertGroup( index, newGroup );
          index++;
        }
      }
    }

    internal void SortGroups( List<GroupSortComparer> groupSortComparers, int level )
    {
      int itemCount = this.ProtectedItemCount;

      if( itemCount == 0 )
        return;

      int[] indexes;
      indexes = new int[ itemCount + 1 ];
      for( int i = 0; i < itemCount; i++ )
      {
        indexes[ i ] = i;
      }

      DataGridCollectionViewGroup[] subGroupsArray = new DataGridCollectionViewGroup[ itemCount ];
      m_subGroups.Values.CopyTo( subGroupsArray, 0 );

      // "Weak heap sort" sort array[0..NUM_ELEMENTS-1] to array[1..NUM_ELEMENTS]
      DataGridCollectionViewGroupSort collectionViewSort = new DataGridCollectionViewGroupSort( indexes, groupSortComparers[ level ], subGroupsArray );

      collectionViewSort.Sort( itemCount );
      level++;
      m_protectedItems.Clear();

      for( int i = 1; i <= itemCount; i++ )
      {
        DataGridCollectionViewGroup group = subGroupsArray[ indexes[ i ] ];

        // Sort sub groups
        if( !group.IsBottomLevel )
        {
          group.SortGroups( groupSortComparers, level );
        }

        m_protectedItems.Add( group );
      }

      this.ProtectedItemCount = m_protectedItems.Count;
      m_protectedItemsCollectionChanged.Invoke( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
    }

    internal void CreateFixedGroupNames( int fixedGroupLevel, ObservableCollection<GroupDescription> groupByList, List<GroupSortComparer> groupSortComparers )
    {
      GroupDescription groupDescription = this.SubGroupBy;

      if( groupDescription == null )
        return;

      Debug.Assert( groupByList[ fixedGroupLevel ] == this.SubGroupBy );

      ObservableCollection<object> groupNames = groupDescription.GroupNames;
      int count = groupNames.Count;

      for( int i = 0; i < count; i++ )
      {
        this.CreateSubGroup( groupNames[ i ], fixedGroupLevel, groupByList, groupSortComparers );
      }
    }


    internal RawItem GetRawItemAtGlobalSortedIndex( int index )
    {
      if( this.IsBottomLevel )
      {
        return m_sortedRawItems[ index ];
      }
      else
      {
        foreach( object value in this.ProtectedItems )
        {
          DataGridCollectionViewGroup subGroup = value as DataGridCollectionViewGroup;

          int subGroupCount = subGroup.GlobalRawItemCount;

          if( index < subGroupCount )
            return subGroup.GetRawItemAtGlobalSortedIndex( index );

          index -= subGroupCount;
        }
      }

      throw new ArgumentOutOfRangeException( "index" );
    }

    internal int RawItemIndexOf( RawItem rawItem )
    {
      Debug.Assert( m_sortedRawItems != null );

      if( m_sortedRawItems == null )
        return -1;

      return m_sortedRawItems.IndexOf( rawItem );
    }

    internal virtual void InsertRawItem( int index, RawItem rawItem )
    {
      Debug.Assert( this.IsBottomLevel );

      m_globalRawItemCount++;
      DataGridCollectionViewGroup parent = m_parent;

      while( parent != null )
      {
        parent.m_globalRawItemCount++;
        parent = parent.m_parent;
      }

      int count = m_sortedRawItems.Count;

      for( int i = index; i < count; i++ )
      {
        m_sortedRawItems[ i ].SetSortedIndex( i + 1 );
      }

      m_sortedRawItems.Insert( index, rawItem );
      rawItem.SetParentGroup( this );
      rawItem.SetSortedIndex( index );

      this.ProtectedItemCount++;
      this.ProtectedItems.Insert( index, rawItem.DataItem );
    }

    internal virtual void RemoveRawItemAt( int index )
    {
      Debug.Assert( this.IsBottomLevel );
      Debug.Assert( m_sortedRawItems.Count > 0 );

      int count = m_sortedRawItems.Count;
      if( count == 0 )
        return;

      if( index != -1 )
      {
        m_globalRawItemCount--;
        DataGridCollectionViewGroup parent = m_parent;

        while( parent != null )
        {
          parent.m_globalRawItemCount--;
          parent = parent.Parent;
        }

        for( int i = index + 1; i < count; i++ )
        {
          m_sortedRawItems[ i ].SetSortedIndex( i - 1 );
        }

        RawItem rawItem = m_sortedRawItems[ index ];
        rawItem.SetParentGroup( null );
        rawItem.SetSortedIndex( -1 );
        m_sortedRawItems.RemoveAt( index );

        this.ProtectedItemCount--;
        this.ProtectedItems.RemoveAt( index );

        if( ( this.ProtectedItemCount == 0 ) && ( m_parent != null ) )
        {
          m_parent.RemoveGroup( this );
        }
      }
    }

    internal virtual void MoveRawItem( int oldIndex, int newIndex )
    {
      Debug.Assert( this.IsBottomLevel );
      Debug.Assert( m_sortedRawItems != null );

      if( m_sortedRawItems == null )
        return;

      RawItem rawItem = m_sortedRawItems[ oldIndex ];

      m_sortedRawItems.RemoveAt( oldIndex );
      m_sortedRawItems.Insert( newIndex, rawItem );

      int startIndex = Math.Min( oldIndex, newIndex );
      int endIndex = Math.Max( oldIndex, newIndex );

      for( int i = startIndex; i <= endIndex; i++ )
      {
        m_sortedRawItems[ i ].SetSortedIndex( i );
      }

      this.ProtectedItems.Move( oldIndex, newIndex );
    }

    internal int BinarySearchRawItem( RawItem value, IComparer<RawItem> comparer )
    {
      if( comparer == null )
        throw new ArgumentNullException( "comparer" );

      if( m_sortedRawItems == null )
        return -1; // ~0

      Debug.Assert( ( m_sortedRawItems.Count == this.ProtectedItemCount ) || ( this is DataGridCollectionViewGroupRoot ) );

      int low = 0;
      int hi = ( m_sortedRawItems.Count ) - 1;

      while( low <= hi )
      {
        int compareResult;
        int median = ( low + ( ( hi - low ) >> 1 ) );

        RawItem medianRawItem = m_sortedRawItems[ median ];

        // We exclude ourself from the research because we seek for a new valid position
        if( medianRawItem == value )
        {
          if( low == hi )
            return low;

          median++;
          medianRawItem = m_sortedRawItems[ median ];
        }

        try
        {
          compareResult = comparer.Compare( medianRawItem, value );
        }
        catch( Exception exception )
        {
          throw new InvalidOperationException( "IComparer has failed to compare the values.", exception );
        }

        if( compareResult == 0 )
        {
          return median;
        }
        if( compareResult < 0 )
        {
          low = median + 1;
        }
        else
        {
          hi = median - 1;
        }
      }

      return ~low;
    }

    private int BinarySearchGroup( DataGridCollectionViewGroup value, IComparer<DataGridCollectionViewGroup> comparer )
    {
      if( comparer == null )
        throw new ArgumentNullException( "comparer" );

      int low = 0;
      int hi = ( this.ProtectedItemCount ) - 1;
      int median;
      int compareResult;

      while( low <= hi )
      {
        median = ( low + ( ( hi - low ) >> 1 ) );

        DataGridCollectionViewGroup medianGroup = this.ProtectedItems[ median ] as DataGridCollectionViewGroup;

        if( medianGroup == value )
        {
          if( low == hi )
            return low;

          return median;
        }

        try
        {
          compareResult = comparer.Compare( medianGroup, value );
        }
        catch( Exception exception )
        {
          throw new InvalidOperationException( "IComparer has failed to compare the values.", exception );
        }

        if( compareResult == 0 )
        {
          return median;
        }
        if( compareResult < 0 )
        {
          low = median + 1;
        }
        else
        {
          hi = median - 1;
        }
      }

      return ~low;
    }

    private static object GetHashKeyFromName( object groupName )
    {
      if( groupName == null )
        return DBNull.Value;

      return groupName;
    }

    private DataGridCollectionViewGroup CreateSubGroup( object groupName, int level, ObservableCollection<GroupDescription> groupByList,
                                                        List<GroupSortComparer> groupSortComparers )
    {
      // If sortComparers is null, we are in massive group creation, no order check.
      var group = new DataGridCollectionViewGroup( groupName, this, m_nextSubGroupUnsortedIndex );

      var dataGridGroupDescription = groupByList[ level ] as DataGridGroupDescription;
      if( dataGridGroupDescription != null )
      {
        group.GroupByName = dataGridGroupDescription.PropertyName;
      }

      unchecked
      {
        m_nextSubGroupUnsortedIndex++;
      }

      int index;

      if( groupSortComparers == null )
      {
        Debug.Assert( this.ProtectedItemCount == this.ProtectedItems.Count );
        index = this.ProtectedItemCount;
      }
      else
      {
        index = this.BinarySearchGroup( group, groupSortComparers[ level ] );

        if( index < 0 )
        {
          index = ~index;
        }
      }

      level++;

      if( level < groupByList.Count )
      {
        group.SetSubGroupBy( groupByList[ level ] );
        group.CreateFixedGroupNames( level, groupByList, groupSortComparers );
      }

      this.InsertGroup( index, group );
      return group;
    }

    private void InsertGroup( int index, DataGridCollectionViewGroup group )
    {
      Debug.Assert( !this.IsBottomLevel );

      m_subGroups.Add( DataGridCollectionViewGroup.GetHashKeyFromName( group.Name ), group );
      this.ProtectedItemCount++;
      this.ProtectedItems.Insert( index, group );
    }

    private void RemoveGroup( DataGridCollectionViewGroup group )
    {
      if( group == null )
        throw new ArgumentNullException( "group" );

      Debug.Assert( this == group.m_parent );

      // We do not remove group forced in SubGroupBy.GroupNames
      if( group.UnsortedIndex < this.SubGroupBy.GroupNames.Count )
        return;

      Debug.Assert( !this.IsBottomLevel );
      Debug.Assert( ( group.m_globalRawItemCount == 0 ) && ( group.ProtectedItemCount == 0 ) );

      m_subGroups.Remove( DataGridCollectionViewGroup.GetHashKeyFromName( group.Name ) );

      this.ProtectedItemCount--;
      this.ProtectedItems.Remove( group );

      if( ( m_subGroups.Count == 0 ) && ( m_parent != null ) )
      {
        m_parent.RemoveGroup( this );
      }
    }

    #region ICustomTypeDescriptor Members

    AttributeCollection ICustomTypeDescriptor.GetAttributes()
    {
      return AttributeCollection.Empty;
    }

    string ICustomTypeDescriptor.GetClassName()
    {
      return null;
    }

    string ICustomTypeDescriptor.GetComponentName()
    {
      return null;
    }

    TypeConverter ICustomTypeDescriptor.GetConverter()
    {
      return null;
    }

    EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
    {
      return null;
    }

    PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
    {
      return null;
    }

    object ICustomTypeDescriptor.GetEditor( Type editorBaseType )
    {
      return null;
    }

    EventDescriptorCollection ICustomTypeDescriptor.GetEvents( Attribute[] attributes )
    {
      return EventDescriptorCollection.Empty;
    }

    EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
    {
      return EventDescriptorCollection.Empty;
    }

    // This method returns the StatFunction properties as defined by the "parent" 
    // DataGridCollectionView as well as the other "standard" properties for this class.
    // The StatFunction properties are NOT filtered by the specified attributes.
    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties( Attribute[] attributes )
    {
      if( attributes == null )
      {
        // We only cache the full property list.
        if( m_classProperties == null )
        {
          DataGridCollectionView view = this.GetCollectionView();

          Debug.Assert( view != null, "A group should always have a parent CollectionView for the StatFunctions to work." );

          if( view == null )
            return TypeDescriptor.GetProperties( typeof( DataGridCollectionViewGroup ) );

          PropertyDescriptorCollection classProperties = TypeDescriptor.GetProperties( typeof( DataGridCollectionViewGroup ) );
          PropertyDescriptor[] properties = new PropertyDescriptor[ classProperties.Count ];
          classProperties.CopyTo( properties, 0 );

          m_classProperties = new PropertyDescriptorCollection( properties );
        }

        return m_classProperties;
      }
      else
      {
        PropertyDescriptorCollection props = TypeDescriptor.GetProperties( this, attributes );
        DataGridCollectionView view = this.GetCollectionView();

        return props;
      }
    }

    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
    {
      return ( ( ICustomTypeDescriptor )this ).GetProperties( null );
    }

    object ICustomTypeDescriptor.GetPropertyOwner( PropertyDescriptor pd )
    {
      return this;
    }

    #endregion

    #region INotifyPropertyChanged Members

    protected sealed override event PropertyChangedEventHandler PropertyChanged
    {
      add
      {
        this.PropertyChangedImpl += value;
      }
      remove
      {
        this.PropertyChangedImpl -= value;
      }
    }

    protected override void OnPropertyChanged( PropertyChangedEventArgs e )
    {
      var handler = this.PropertyChangedImpl;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    private bool HasPropertyChangedListeners
    {
      get
      {
        return ( this.PropertyChangedImpl != null );
      }
    }

    private event PropertyChangedEventHandler PropertyChangedImpl;

    #endregion

    private PropertyDescriptorCollection m_classProperties;

    protected int m_globalRawItemCount;
    protected readonly List<RawItem> m_sortedRawItems;

    private readonly OptimizedReadOnlyObservableCollection m_optimizedItems;
    private readonly Dictionary<object, DataGridCollectionViewGroup> m_subGroups;
    private readonly IList<object> m_protectedItems;
    private readonly Action<NotifyCollectionChangedEventArgs> m_protectedItemsCollectionChanged;

    private GroupDescription m_subGroupBy;
    private readonly DataGridCollectionViewGroup m_parent;
    private readonly int m_unsortedIndex;
    private int m_nextSubGroupUnsortedIndex;

    #region OptimizedReadOnlyObservableCollection Private Class

    // We re-implement IList and IList<object> to override the implementation of the 
    // IndexOf and Contains methods to use our optimized way.
    private sealed class OptimizedReadOnlyObservableCollection : ReadOnlyObservableCollection<object>, IList, IList<object>
    {
      internal OptimizedReadOnlyObservableCollection( DataGridCollectionViewGroup dataGridCollectionViewGroup )
        : base( dataGridCollectionViewGroup.ProtectedItems )
      {
        if( dataGridCollectionViewGroup == null )
          throw new ArgumentNullException( "dataGridCollectionViewGroup" );

        m_dataGridCollectionViewGroup = dataGridCollectionViewGroup;
      }

      public new int IndexOf( object item )
      {
        // The DataGridCollectionViewGroup has been optimized to use the information
        // stored on the RawItem associated with the data item instead of searching the item in the list.
        return m_dataGridCollectionViewGroup.IndexOf( item );
      }

      public new bool Contains( object item )
      {
        // The DataGridCollectionViewGroup has been optimized to use the information
        // stored on the RawItem associated with the data item instead of searching the item in the list.
        return m_dataGridCollectionViewGroup.Contains( item );
      }

      private readonly DataGridCollectionViewGroup m_dataGridCollectionViewGroup;
    }

    #endregion

    #region ObservableCollectionHelper Private Class

    private static class ObservableCollectionHelper
    {
      private const string AssemblyName = "Xceed.Wpf.DataGrid.CollectionViewGroupExtractor";
      private const string GetItemsMethodName = "GetItems";
      private const string GetCollectionChangedMethodName = "GetCollectionChanged";

      private static readonly Func<ObservableCollection<object>, IList<object>> s_getItems;
      private static readonly Func<ObservableCollection<object>, Action<NotifyCollectionChangedEventArgs>> s_collectionChanged;

      static ObservableCollectionHelper()
      {
        var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly( new AssemblyName( ObservableCollectionHelper.AssemblyName ), AssemblyBuilderAccess.RunAndCollect );
        var moduleBuilder = assemblyBuilder.DefineDynamicModule( ObservableCollectionHelper.AssemblyName );

        var typeBuilder = moduleBuilder.DefineType( "ObservableCollectionExtractor", TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.AutoLayout, typeof( ObservableCollection<object> ) );

        ObservableCollectionHelper.DefineGetItemsMethod( typeBuilder );
        ObservableCollectionHelper.DefineGetCollectionChangedMethod( typeBuilder );

        var targetType = typeBuilder.CreateType();

        s_getItems = ( Func<ObservableCollection<object>, IList<object>> )Delegate.CreateDelegate( typeof( Func<ObservableCollection<object>, IList<object>> ), targetType.GetMethod( ObservableCollectionHelper.GetItemsMethodName, BindingFlags.Public | BindingFlags.Static ) );
        s_collectionChanged = ( Func<ObservableCollection<object>, Action<NotifyCollectionChangedEventArgs>> )Delegate.CreateDelegate( typeof( Func<ObservableCollection<object>, Action<NotifyCollectionChangedEventArgs>> ), targetType.GetMethod( ObservableCollectionHelper.GetCollectionChangedMethodName, BindingFlags.Public | BindingFlags.Static ) );
      }

      internal static IList<object> GetItems( ObservableCollection<object> source )
      {
        if( source == null )
          throw new ArgumentNullException( "source" );

        var storage = ( s_getItems != null ) ? s_getItems.Invoke( source ) : null;
        if( storage == null )
          throw new InvalidOperationException( "Unable to retrieve the ObservableCollection<>'s items storage." );

        return storage;
      }

      internal static Action<NotifyCollectionChangedEventArgs> GetCollectionChanged( ObservableCollection<object> source )
      {
        if( source == null )
          throw new ArgumentNullException( "source" );

        var collectionChanged = ( s_collectionChanged != null ) ? s_collectionChanged.Invoke( source ) : null;
        if( collectionChanged == null )
          throw new InvalidOperationException( "Unable to retrieve the ObservableCollection<>'s collection change method." );

        return collectionChanged;
      }

      private static void DefineGetItemsMethod( TypeBuilder typeBuilder )
      {
        var propertyInfo = typeof( ObservableCollection<object> ).GetProperty( "Items", BindingFlags.Instance | BindingFlags.NonPublic, null, typeof( IList<object> ), new Type[ 0 ], null );
        if( ( propertyInfo == null ) || !propertyInfo.CanRead )
          throw new InvalidOperationException( "Unable to retrieve the ObservableCollection<>.Items property." );

        var methodInfo = propertyInfo.GetGetMethod( true );
        if( ( methodInfo == null ) || !ObservableCollectionHelper.HasCallingConvention( methodInfo.CallingConvention, CallingConventions.HasThis ) )
          throw new InvalidOperationException( "Unable to retrieve the ObservableCollection<>.Items property." );

        var methodBuilder = typeBuilder.DefineMethod(
                              ObservableCollectionHelper.GetItemsMethodName,
                              MethodAttributes.Public | MethodAttributes.Static,
                              CallingConventions.Standard,
                              typeof( IList<object> ),
                              new Type[] { typeof( ObservableCollection<object> ) } );

        var body = methodBuilder.GetILGenerator();
        // Load the reference to the ObservableCollection and put it on top of the stack.
        body.Emit( OpCodes.Ldarg_0 );
        // Invoke the property getter of the reference.  This operation pops the reference
        // from the stack and pushes the result of the property getter.
        body.Emit( OpCodes.Callvirt, methodInfo );
        // Since the result of the propery getter will be retrieved by the method's caller,
        // we may leave it on the stack and simply return.
        body.Emit( OpCodes.Ret );
      }

      private static void DefineGetCollectionChangedMethod( TypeBuilder typeBuilder )
      {
        var methodInfo = typeof( ObservableCollection<object> ).GetMethod( "OnCollectionChanged", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof( NotifyCollectionChangedEventArgs ) }, null );
        if( ( methodInfo == null ) || methodInfo.IsPrivate || !ObservableCollectionHelper.HasCallingConvention( methodInfo.CallingConvention, CallingConventions.HasThis ) )
          throw new InvalidOperationException( "Unable to retrieve the ObservableCollection<>.OnCollectionChanged method." );

        var constructorInfo = typeof( Action<NotifyCollectionChangedEventArgs> ).GetConstructor( new Type[] { typeof( object ), typeof( IntPtr ) } );
        if( ( constructorInfo == null ) || constructorInfo.IsPrivate || !ObservableCollectionHelper.HasCallingConvention( constructorInfo.CallingConvention, CallingConventions.HasThis ) )
          throw new InvalidOperationException( "Unable to retrieve the Action<>'s constructor." );

        var methodBuilder = typeBuilder.DefineMethod(
                              ObservableCollectionHelper.GetCollectionChangedMethodName,
                              MethodAttributes.Public | MethodAttributes.Static,
                              CallingConventions.Standard,
                              typeof( Action<NotifyCollectionChangedEventArgs> ),
                              new Type[] { typeof( ObservableCollection<object> ) } );

        var body = methodBuilder.GetILGenerator();
        // Load the reference to the ObservableCollection and put it on top of the stack.
        body.Emit( OpCodes.Ldarg_0 );
        // The last loaded reference will be consume in the call to "new Action<>".
        // We must duplicate the value since it will be consume by the call that retrieve the
        // target method's address.
        body.Emit( OpCodes.Dup );
        // This operation pops the top ObservableCollection's reference from the stack
        // and pushes the ObservableCollection<>.OnCollectionChanged method's address.
        body.Emit( OpCodes.Ldvirtftn, methodInfo );
        // Create an Action<> delegate from the first ObservableCollection's reference we
        // have put on the stack and the target method's address.
        body.Emit( OpCodes.Newobj, constructorInfo );
        // The resulting delegate should be on top of the stack.  We simply leave it there
        // so it will be retrieved by the method's caller.
        body.Emit( OpCodes.Ret );
      }

      private static bool HasCallingConvention( CallingConventions source, CallingConventions value )
      {
        return ( ( source & value ) == value );
      }
    }

    #endregion
  }
}
