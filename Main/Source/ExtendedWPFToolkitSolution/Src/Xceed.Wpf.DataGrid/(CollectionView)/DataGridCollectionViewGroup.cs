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
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridCollectionViewGroup : CollectionViewGroup, ICustomTypeDescriptor
  {
    public DataGridCollectionViewGroup( object name, DataGridCollectionViewGroup parent, int unsortedIndex )
      : base( name )
    {
      m_parent = parent;
      m_unsortedIndex = unsortedIndex;
    }

    internal DataGridCollectionViewGroup(
      DataGridCollectionViewGroup template,
      DataGridCollectionViewGroup parent )
      : this( template.Name, parent, template.m_unsortedIndex )
    {
      m_nextSubGroupUnsortedIndex = template.m_nextSubGroupUnsortedIndex;
      m_subGroupBy = template.m_subGroupBy;

      if( template.m_groupsDictionary != null )
        m_groupsDictionary = new Hashtable();

      if( template.m_sortedRawItems != null )
        m_sortedRawItems = new List<RawItem>( template.m_sortedRawItems.Count );
    }

    #region IsBottomLevel Property

    public override bool IsBottomLevel
    {
      get
      {
        // return true if .Items contain DataItem
        return ( m_subGroupBy == null );
      }
    }

    #endregion IsBottomLevel Property

    #region Items Property

    public new ReadOnlyObservableCollection<object> Items
    {
      get
      {
        // The optimized list is only able to handle Items. When we are not at the bottom level,
        // this means that our Items collection return sub-groups.
        if( this.IsBottomLevel )
        {
          if( m_optimizedItems == null )
            m_optimizedItems = new OptimizedReadOnlyObservableCollection( this );

          return m_optimizedItems;
        }

        // The group hierarchy can change over time. If, at this point, 
        // we are not eligible to return the optimized list (we contains
        // sub groups or our source item list is not a DataGridCollectionView),
        // we must reset our cached list.
        m_optimizedItems = null;

        return base.Items;
      }
    }

    #endregion Items Property

    #region ProtectedItems Property

    internal new ObservableCollection<object> ProtectedItems
    {
      get
      {
        return base.ProtectedItems;
      }
    }

    #endregion ProtectedItems Property

    #region UnsortedIndex Property

    internal int UnsortedIndex
    {
      get
      {
        return m_unsortedIndex;
      }
    }

    #endregion UnsortedIndex Property

    #region SubGroupBy Property

    internal GroupDescription SubGroupBy
    {
      get
      {
        return m_subGroupBy;
      }
    }

    #endregion SubGroupBy Property

    #region Parent Property

    internal DataGridCollectionViewGroup Parent
    {
      get
      {
        return m_parent;
      }
    }

    #endregion Parent Property

    protected virtual DataGridCollectionView GetCollectionView()
    {
      if( m_parent != null )
        return m_parent.GetCollectionView();

      return null;
    }

    internal bool Contains( object dataItem )
    {
      DataGridCollectionView collectionView = this.GetCollectionView();
      if( collectionView != null )
      {
        RawItem rawItem = collectionView.GetFirstRawItemFromDataItem( dataItem );
        if( rawItem != null )
        {
          return ( rawItem.ParentGroup == this );
        }
      }

      return false;
    }

    internal int IndexOf( object dataItem )
    {
      DataGridCollectionView collectionView = this.GetCollectionView();

      if( collectionView != null )
      {
        RawItem rawItem = collectionView.GetFirstRawItemFromDataItem( dataItem );

        if( ( rawItem != null )
          && ( rawItem.ParentGroup == this ) )
        {
          return rawItem.SortedIndex;
        }
      }

      return -1;
    }

    internal int GetFirstRawItemGlobalSortedIndex()
    {
      int index = 0;
      DataGridCollectionViewGroup group = this;
      DataGridCollectionViewGroup parent = group;

      if( parent != null )
        parent = parent.Parent;

      while( parent != null )
      {
        ReadOnlyObservableCollection<object> subGroups = parent.Items;
        int subGroupCount = subGroups.Count;
        DataGridCollectionViewGroup currentGroup = null;

        for( int i = 0; i < subGroupCount; i++ )
        {
          currentGroup = subGroups[ i ] as DataGridCollectionViewGroup;

          if( currentGroup == group )
            break;

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

      if( m_subGroupBy != null )
      {
        if( m_groupsDictionary == null )
          m_groupsDictionary = new Hashtable();
      }

      if( oldIsBottomLevel != this.IsBottomLevel )
        this.OnPropertyChanged( new PropertyChangedEventArgs( "IsBottomLevel" ) );
    }

    internal DataGridCollectionViewGroup GetGroup(
      RawItem rawItem,
      int level,
      CultureInfo culture,
      ObservableCollection<GroupDescription> groupByList,
      GroupSortComparer[] groupSortComparers )
    {
      // If sortComparers is null, we are in massive group creation, no order check.

      if( this.IsBottomLevel )
        throw new InvalidOperationException( "An attempt was made to get a group for which a GroupDescription has not been provided." );

      object groupName = m_subGroupBy.GroupNameFromItem( rawItem.DataItem, level, culture );
      DataGridGroupDescription dataGridGroupDescription = m_subGroupBy as DataGridGroupDescription;
      DataGridCollectionViewGroup group;

      if( dataGridGroupDescription != null )
      {
        group = m_groupsDictionary[ DataGridCollectionViewGroup.GetHashKeyFromName( groupName ) ]
          as DataGridCollectionViewGroup;
      }
      else
      {
        int itemCount = this.ItemCount;
        group = null;

        for( int i = 0; i < itemCount; i++ )
        {
          DataGridCollectionViewGroup tempGroup = this.ProtectedItems[ i ] as DataGridCollectionViewGroup;

          if( m_subGroupBy.NamesMatch( tempGroup.Name, groupName ) )
          {
            group = tempGroup;
            break;
          }
        }
      }

      if( group == null )
      {
        group = this.CreateSubGroup(
          groupName, level, groupByList, groupSortComparers );
      }

      return group;
    }

    internal void SortItems(
      SortDescriptionInfo[] sortDescriptionInfos,
      GroupSortComparer[] groupSortComparers,
      int level,
      List<RawItem> globalRawItems,
      DataGridCollectionViewGroup newSortedGroup )
    {
      int itemCount = this.ItemCount;

      if( itemCount == 0 )
        return;

      ObservableCollection<object> groupItems = this.ProtectedItems;

      if( this.IsBottomLevel )
      {
        int[] indexes;

        indexes = new int[ itemCount + 1 ];

        for( int i = 0; i < itemCount; i++ )
        {
          indexes[ i ] = m_sortedRawItems[ i ].Index;
        }

        // "Weak heap sort" sort array[0..NUM_ELEMENTS-1] to array[1..NUM_ELEMENTS]
        DataGridCollectionViewSort collectionViewSort =
          new DataGridCollectionViewSort( indexes, sortDescriptionInfos );

        collectionViewSort.Sort( itemCount );
        int index = 0;

        for( int i = 1; i <= itemCount; i++ )
        {
          newSortedGroup.InsertRawItem( index, globalRawItems[ indexes[ i ] ] );
          index++;
        }
      }
      else
      {
        int[] indexes;

        indexes = new int[ itemCount + 1 ];

        for( int i = 0; i < itemCount; i++ )
        {
          indexes[ i ] = i;
        }

        // "Weak heap sort" sort array[0..NUM_ELEMENTS-1] to array[1..NUM_ELEMENTS]
        DataGridCollectionViewGroupSort collectionViewSort =
          new DataGridCollectionViewGroupSort( indexes, groupSortComparers[ level ], this );

        collectionViewSort.Sort( itemCount );
        int index = 0;
        level++;

        for( int i = 1; i <= itemCount; i++ )
        {
          DataGridCollectionViewGroup oldGroup = ( DataGridCollectionViewGroup )groupItems[ indexes[ i ] ];
          DataGridCollectionViewGroup newGroup = new DataGridCollectionViewGroup( oldGroup, newSortedGroup );

          // Sort sub items
          oldGroup.SortItems( sortDescriptionInfos, groupSortComparers, level, globalRawItems, newGroup );

          newSortedGroup.InsertGroup( index, newGroup );
          index++;
        }
      }
    }

    internal int BinarySearchGroup( DataGridCollectionViewGroup value, IComparer<DataGridCollectionViewGroup> comparer )
    {
      if( comparer == null )
        throw new ArgumentNullException( "comparer" );

      ObservableCollection<object> items = this.ProtectedItems;

      Debug.Assert( items.Count == this.ItemCount );

      int low = 0;
      int hi = ( items.Count ) - 1;

      while( low <= hi )
      {
        int compareResult;
        int median = ( low + ( ( hi - low ) >> 1 ) );

        DataGridCollectionViewGroup medianGroup = items[ median ] as DataGridCollectionViewGroup;

        // We exclude ourself from the research because we seek for a new valid position
        if( medianGroup == value )
        {
          if( low == hi )
            return low;

          median++;
          medianGroup = items[ median ] as DataGridCollectionViewGroup;
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

    internal void CreateFixedGroupNames( int fixedGroupLevel, ObservableCollection<GroupDescription> groupByList, GroupSortComparer[] groupSortComparers )
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


    internal int GlobalRawItemCount
    {
      get
      {
        return m_globalRawItemCount;
      }
    }

    internal List<RawItem> RawItems
    {
      get
      {
        return m_sortedRawItems;
      }
    }

    internal void GetGlobalItems( List<object> items )
    {
      if( this.IsBottomLevel )
      {
        items.AddRange( this.ProtectedItems );
        return;
      }

      int count = this.ItemCount;

      for( int i = 0; i < count; i++ )
      {
        DataGridCollectionViewGroup subGroup = this.ProtectedItems[ i ] as DataGridCollectionViewGroup;

        if( subGroup == null )
          throw new InvalidOperationException( "Sub-groups cannot be null (Nothing in Visual Basic)." );

        subGroup.GetGlobalItems( items );
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
        int count = this.ItemCount;

        for( int i = 0; i < count; i++ )
        {
          DataGridCollectionViewGroup subGroups = this.ProtectedItems[ i ] as DataGridCollectionViewGroup;

          if( subGroups == null )
            throw new InvalidOperationException( "Sub-groups cannot be null (Nothing in Visual Basic)." );

          int subGroupCount = subGroups.GlobalRawItemCount;

          if( index < subGroupCount )
            return subGroups.GetRawItemAtGlobalSortedIndex( index );

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

      if( m_sortedRawItems == null )
        m_sortedRawItems = new List<RawItem>( 4 );

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

    internal void RemoveRawItem( RawItem rawItem )
    {
      if( !this.IsBottomLevel )
        throw new InvalidOperationException( "An attempt was made to remove a data item from a group other than the bottom-level group." );

      int index = this.RawItemIndexOf( rawItem );
      this.RemoveRawItemAt( index );
    }

    internal virtual void RemoveRawItemAt( int index )
    {
      Debug.Assert( this.IsBottomLevel );
      Debug.Assert( m_sortedRawItems != null );

      if( m_sortedRawItems == null )
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

        int count = m_sortedRawItems.Count;

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
          m_parent.RemoveGroup( this );
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

      Debug.Assert( ( m_sortedRawItems.Count == this.ItemCount ) || ( this is DataGridCollectionViewGroupRoot ) );

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


    internal object GetStatFunctionValue( string propertyName )
    {
      DataGridCollectionView view = this.GetCollectionView();

      Debug.Assert( view != null );

      if( view == null )
        return null;

      Stats.StatFunction statFunction = view.StatFunctions[ propertyName ];

      Debug.Assert( view != null );

      if( statFunction == null )
        return null;

      return this.GetStatFunctionResult( statFunction, view ).Value;
    }

    internal Stats.StatResult GetStatFunctionResult( Stats.StatFunction statFunction, DataGridCollectionView view )
    {
      if( !m_statFunctionValues.ContainsKey( statFunction ) )
        this.CalculateStatFunctionValue( statFunction, view );

      return m_statFunctionValues[ statFunction ];
    }

    // This clears the value of all the previously calculated StatFunction Results for  this group.
    internal void ClearStatFunctionsResult()
    {
      m_statFunctionValues.Clear();
    }

    internal void InvokeStatFunctionsPropertyChanged( DataGridCollectionView view )
    {
      if( view.CalculateChangedPropertyStatsOnly )
      {
        foreach( Stats.StatFunction statFunction in view.InvalidatedStatFunctions )
        {
          {
            this.OnPropertyChanged( new PropertyChangedEventArgs( statFunction.ResultPropertyName ) );
          }
        }
      }
      else
      {
        foreach( PropertyDescriptor statFunctionPropertyDescriptor in view.GetStatFunctionProperties() )
        {
          this.OnPropertyChanged( new PropertyChangedEventArgs( statFunctionPropertyDescriptor.Name ) );
        }
      }
    }

    private void CalculateStatFunctionValue( Stats.StatFunction statFunction, DataGridCollectionView view )
    {
      m_statFunctionValues[ statFunction ] = new Stats.StatResult(
        new InvalidOperationException( Log.NotToolkitStr( "Statistical functions" ) )
        );
    }

    private static object GetHashKeyFromName( object groupName )
    {
      if( groupName == null )
        return DBNull.Value;

      return groupName;
    }

    private DataGridCollectionViewGroup CreateSubGroup( object groupName, int level, ObservableCollection<GroupDescription> groupByList, GroupSortComparer[] groupSortComparers )
    {
      // If sortComparers is null, we are in massive group creation, no order check.
      DataGridCollectionViewGroup group = new DataGridCollectionViewGroup( groupName, this, m_nextSubGroupUnsortedIndex );

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
          index = ~index;
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

      m_groupsDictionary.Add( DataGridCollectionViewGroup.GetHashKeyFromName( group.Name ), group );
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

      this.ProtectedItemCount--;
      m_groupsDictionary.Remove( DataGridCollectionViewGroup.GetHashKeyFromName( group.Name ) );
      this.ProtectedItems.Remove( group );

      if( ( this.ProtectedItemCount == 0 ) && ( m_parent != null ) )
        m_parent.RemoveGroup( this );
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
          PropertyDescriptorCollection statProperties = view.GetStatFunctionProperties();
          PropertyDescriptor[] properties = new PropertyDescriptor[ classProperties.Count + statProperties.Count ];
          classProperties.CopyTo( properties, 0 );
          statProperties.CopyTo( properties, classProperties.Count );

          m_classProperties = new PropertyDescriptorCollection( properties );
        }

        return m_classProperties;
      }
      else
      {
        PropertyDescriptorCollection props = TypeDescriptor.GetProperties( this, attributes );
        DataGridCollectionView view = this.GetCollectionView();

        if( view != null )
        {
          foreach( PropertyDescriptor property in view.GetStatFunctionProperties() )
            props.Add( property );
        }

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

    private PropertyDescriptorCollection m_classProperties;
    private Dictionary<Stats.StatFunction, Stats.StatResult> m_statFunctionValues = new Dictionary<Stats.StatFunction, Stats.StatResult>( s_statFunctionComparer );

    protected int m_globalRawItemCount;
    protected List<RawItem> m_sortedRawItems;

    private Hashtable m_groupsDictionary;
    private GroupDescription m_subGroupBy;
    private DataGridCollectionViewGroup m_parent;
    private int m_unsortedIndex;
    private int m_nextSubGroupUnsortedIndex;

    private static Stats.StatFunctionComparer s_statFunctionComparer = new Stats.StatFunctionComparer();

    private OptimizedReadOnlyObservableCollection m_optimizedItems;
  }
}
