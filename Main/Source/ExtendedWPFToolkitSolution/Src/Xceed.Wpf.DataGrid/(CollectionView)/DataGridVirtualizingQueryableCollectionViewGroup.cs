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
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridVirtualizingQueryableCollectionViewGroup : DataGridVirtualizingCollectionViewGroupBase
  {
    #region CONSTRUCTORS

    public DataGridVirtualizingQueryableCollectionViewGroup(
      object name,
      int initialItemsCount,
      int startGlobalIndex,
      DataGridVirtualizingQueryableCollectionViewGroup parent,
      int level,
      bool isBottomLevel )
      : base( name, initialItemsCount, startGlobalIndex, parent, level, isBottomLevel )
    {
    }

    #endregion CONSTRUCTORS

    #region INTERNAL PROPERTIES

    internal IQueryable UnsortedFilteredQueryable
    {
      get
      {
        if( m_unsortedFilteredQueryable == null )
          m_unsortedFilteredQueryable = this.CreateUnsortedFilteredQueryable();

        return m_unsortedFilteredQueryable;
      }
    }

    internal IQueryable Queryable
    {
      get
      {
        if( m_selectQueryable == null )
          m_selectQueryable = this.CreateSelectQueryable( false );

        return m_selectQueryable;
      }
    }

    internal IQueryable ReversedQueryable
    {
      get
      {
        if( m_reversedSelectQueryable == null )
          m_reversedSelectQueryable = this.CreateSelectQueryable( true );

        return m_reversedSelectQueryable;
      }
    }

    #endregion INTERNAL PROPERTIES

    #region INTERNAL METHODS

    internal override int QueryItemCount()
    {
      if( this.UnsortedFilteredQueryable != null )
      {
        try
        {
          return this.UnsortedFilteredQueryable.Count();
        }
        catch
        {
          // Timeout or other error occured.
        }
      }

      return 0;
    }

    internal override ObservableCollection<object> QuerySubCollectionViewGroupList( GroupDescription childGroupBy, int nextLevel, bool nextLevelIsBottom )
    {
      string childGroupByPropertyName = DataGridCollectionViewBase.GetPropertyNameFromGroupDescription( childGroupBy );

      if( String.IsNullOrEmpty( childGroupByPropertyName ) )
        throw new NotSupportedException( "Custom groups are not supported when using a DataGridVirtualizingQueryableCollectionView." );

      DataGridVirtualizingCollectionViewBase collectionView = this.GetCollectionView();

      bool sortGroupBy = false;
      ListSortDirection groupByDirection = ListSortDirection.Ascending;

      foreach( SortDescription sortDescription in collectionView.SortDescriptions )
      {
        if( sortDescription.PropertyName == childGroupByPropertyName )
        {
          sortGroupBy = true;
          groupByDirection = sortDescription.Direction;
          break;
        }
      }

      IQueryable groupsAndCountsQueryable = this.Queryable.GetSubGroupsAndCountsQueryable( childGroupByPropertyName, sortGroupBy, groupByDirection );

      List<QueryableExtensions.IQueryableGroupNameCountPair> distinctValuesAndCounts = new List<QueryableExtensions.IQueryableGroupNameCountPair>();
      try
      {
        System.Collections.IEnumerator enumerator = groupsAndCountsQueryable.GetEnumerator();

        while( enumerator.MoveNext() )
        {
          QueryableExtensions.IQueryableGroupNameCountPair current = enumerator.Current as QueryableExtensions.IQueryableGroupNameCountPair;

          if( current != null )
            distinctValuesAndCounts.Add( current );
        }
      }
      catch
      {
        // TimeOut exception on the connection or other.
        distinctValuesAndCounts.Clear();
      }


      // If we are not the bottom level, we should have subgroups.
      // However, if the connection timed out and the catch statement set the coundAndDistinctValues to an empty array,
      // then we shouldn't add anything.  We cannot reset on the spot since we might already be resetting.


      // Create the collection of sub CollectionViewGroups
      ObservableCollection<object> subCollectionViewGroupList = new ObservableCollection<object>();

      int runningCount = this.StartGlobalIndex;
      int distinctValuesCount = distinctValuesAndCounts.Count;
      for( int i = 0; i < distinctValuesCount; i++ )
      {
        QueryableExtensions.IQueryableGroupNameCountPair queryableGroupNameCountPair = distinctValuesAndCounts[ i ];

        subCollectionViewGroupList.Add(
          new DataGridVirtualizingQueryableCollectionViewGroup( queryableGroupNameCountPair.GroupName, queryableGroupNameCountPair.Count, runningCount, this, nextLevel, nextLevelIsBottom ) );

        runningCount += queryableGroupNameCountPair.Count;
      }

      return subCollectionViewGroupList;
    }

    internal virtual IQueryable CreateUnsortedFilteredQueryable()
    {
      int level = this.Level;

      Debug.Assert( ( level > -1 ), "The DataGridVirtualizingGroupRoot should have overriden this method without calling base." );

      // Sub group.  Only group from parent, which will already be sorted.
      DataGridVirtualizingQueryableCollectionViewGroup parentCollectionViewGroup = this.Parent as DataGridVirtualizingQueryableCollectionViewGroup;

      IQueryable parentUnsortedQueryable = parentCollectionViewGroup.UnsortedFilteredQueryable;

      Debug.Assert( parentUnsortedQueryable != null );

      if( parentUnsortedQueryable == null )
        return null;

      // Narrow the filter to this group's clauses.
      return this.FilterQueryable( parentUnsortedQueryable );
    }

    internal IQueryable CreateSelectQueryable( bool reversed )
    {
      // Sub group.  Only group from parent.  The sorting will be appended just before the Slice statement.
      IQueryable queryable = this.UnsortedFilteredQueryable;

      return this.SortQueryable( queryable, reversed );
    }

    internal override void DisposeCore()
    {
      m_unsortedFilteredQueryable = null;
      m_reversedSelectQueryable = null;
      m_selectQueryable = null;
      base.DisposeCore();
    }

    #endregion INTERNAL METHODS

    #region PRIVATE METHODS

    private IQueryable FilterQueryable( IQueryable queryable )
    {
      // Filters a queryable to match this Group's clause.
      // This method does not apply the AutoFilter clauses or the FilterRow clauses.
      // It is the DataGridVirtualizingQueryableCollectionViewGroupRoot's job to do so on the
      // base queryable used as a base for all other queryables.

      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      int level = this.Level;

      DataGridVirtualizingCollectionViewBase collectionView = this.GetCollectionView();

      ObservableCollection<GroupDescription> groupDescriptions = collectionView.GroupDescriptions;

      Debug.Assert( ( groupDescriptions != null ) && ( level < groupDescriptions.Count ) );

      DataGridGroupDescription groupBy = groupDescriptions[ level ] as DataGridGroupDescription;

      Debug.Assert( groupBy != null );

      ParameterExpression sharedParameterExpression = queryable.CreateParameterExpression();

      Expression expression = queryable.CreateEqualExpression( sharedParameterExpression, groupBy.PropertyName, this.Name );
      return queryable.WhereFilter( sharedParameterExpression, expression );
    }

    private IQueryable SortQueryable( IQueryable queryable, bool reverseSort )
    {
      DataGridVirtualizingCollectionViewBase parentCollectionView = this.GetCollectionView();

      Debug.Assert( parentCollectionView != null );

      SortDescriptionCollection explicitSortDescriptions = parentCollectionView.SortDescriptions;

      ListSortDirection directionToUseForImplicitSortDescriptions = ListSortDirection.Ascending;

      if( explicitSortDescriptions.Count > 0 )
        directionToUseForImplicitSortDescriptions = explicitSortDescriptions[ explicitSortDescriptions.Count - 1 ].Direction;

      SortDescriptionCollection implicitSortDescriptions = new SortDescriptionCollection();

      DataGridVirtualizingQueryableCollectionViewGroupRoot groupRoot =
        parentCollectionView.RootGroup as DataGridVirtualizingQueryableCollectionViewGroupRoot;

      Debug.Assert( groupRoot != null );

      string[] primaryKeys = groupRoot.PrimaryKeys;

      if( primaryKeys != null )
      {
        for( int i = 0; i < primaryKeys.Length; i++ )
        {
          string primaryKey = primaryKeys[ i ];

          Debug.Assert( !string.IsNullOrEmpty( primaryKey ) );

          implicitSortDescriptions.Add( new SortDescription( primaryKey, directionToUseForImplicitSortDescriptions ) );
        }
      }

      return queryable.OrderBy( implicitSortDescriptions, explicitSortDescriptions, reverseSort );
    }

    #endregion PRIVATE METHODS

    #region PRIVATE FIELDS

    private IQueryable m_unsortedFilteredQueryable;
    private IQueryable m_selectQueryable;
    private IQueryable m_reversedSelectQueryable;

    #endregion PRIVATE FIELDS
  }
}
