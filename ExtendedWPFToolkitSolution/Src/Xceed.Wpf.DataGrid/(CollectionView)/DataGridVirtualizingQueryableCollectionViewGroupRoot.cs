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
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using System.Linq.Expressions;
using Xceed.Wpf.DataGrid.FilterCriteria;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridVirtualizingQueryableCollectionViewGroupRoot : DataGridVirtualizingQueryableCollectionViewGroup
  {
    #region CONSTRUCTORS

    internal DataGridVirtualizingQueryableCollectionViewGroupRoot( 
      DataGridVirtualizingQueryableCollectionView collectionView, 
      object pageManagerSyncRoot, 
      bool isBottomLevel )
      : base( null, -1, 0, null, -1, isBottomLevel )
    {
      m_parentCollectionView = collectionView;
      m_queryableSource = m_parentCollectionView.QueryableSource;

      if( m_queryableSource != null )
      {
        m_primaryKeyPropertyNames = m_queryableSource.FindPrimaryKeys();
        // Primary key optimizations are only possible when only one primary key is identified in the source.
        // When dealing with sources defining more than one primary key, we would need to know in which order
        bool supportsPrimaryKeyOptimizations = ( ( m_primaryKeyPropertyNames != null ) && ( m_primaryKeyPropertyNames.Length == 1 ) );

        m_virtualPageManager = new DataGridLINQPageManager( collectionView,
          pageManagerSyncRoot, 
          supportsPrimaryKeyOptimizations );
      }
    }


    #endregion CONSTRUCTORS

    #region PROTECTED METHODS

    protected override DataGridVirtualizingCollectionViewBase GetCollectionView()
    {
      return m_parentCollectionView;
    }

    #endregion PROTECTED METHODS

    #region INTERNAL PROPERTIES

    public string[] PrimaryKeys
    {
      get
      {
        return m_primaryKeyPropertyNames;
      }
    }

    #endregion INTERNAL PROPERTIES

    #region INTERNAL METHODS

    internal override IQueryable CreateUnsortedFilteredQueryable()
    {
      if( m_unsortedFilteredQueryableSource == null )
      {
        if( m_queryableSource != null )
        {
          ParameterExpression sharedParameterExpression = m_queryableSource.CreateParameterExpression();

          Expression filterCriterionsExpression = this.GetFilterCriterionsExpression( m_queryableSource, sharedParameterExpression );
          Expression autoFilterValuesExpression = this.GetAutoFilterValuesExpression( m_queryableSource, sharedParameterExpression );

          Expression completeFilterExpression = null;

          if( filterCriterionsExpression != null )
            completeFilterExpression = filterCriterionsExpression;

          if( autoFilterValuesExpression != null )
          {
            if( completeFilterExpression == null )
            {
              completeFilterExpression = autoFilterValuesExpression;
            }
            else
            {
              completeFilterExpression = Expression.And( completeFilterExpression, autoFilterValuesExpression );
            }
          }

          // Apply the complete filter expression, or if null, set the root source.
          if( completeFilterExpression == null )
          {
            m_unsortedFilteredQueryableSource = m_queryableSource;
          }
          else
          {
            m_unsortedFilteredQueryableSource = m_queryableSource.WhereFilter( sharedParameterExpression, completeFilterExpression );
          }
        }
      }

      return m_unsortedFilteredQueryableSource;
    }

    private Expression GetFilterCriterionsExpression( IQueryable queryable, ParameterExpression sharedParameterExpression )
    {
      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      FilterCriteriaMode filterCriteriaMode = m_parentCollectionView.FilterCriteriaMode;

      if( filterCriteriaMode == FilterCriteriaMode.None )
        return null;


      DataGridItemPropertyCollection itemProperties = m_parentCollectionView.ItemProperties;
      int itemPropertiesCount = itemProperties.Count;

      Expression filterCriterionsExpression = null;

      for( int i = 0; i < itemPropertiesCount; i++ )
      {
        DataGridItemPropertyBase itemProperty = itemProperties[ i ];
        string itemPropertyName = itemProperty.Name;

        FilterCriterion filterCriterion = itemProperty.FilterCriterion;

        if( filterCriterion == null )
          continue;

        Expression propertyFilterCriterionExpression = filterCriterion.ToLinqExpression( queryable, sharedParameterExpression, itemPropertyName );

        if( propertyFilterCriterionExpression == null )
          continue;

        if( filterCriterionsExpression == null )
        {
          filterCriterionsExpression = propertyFilterCriterionExpression;
        }
        else
        {
          Debug.Assert( ( filterCriteriaMode == FilterCriteriaMode.And ) || ( filterCriteriaMode == FilterCriteriaMode.Or ) );

          // Merge this DataGridItemProperty FilterCriterionExpressions
          if( filterCriteriaMode == FilterCriteriaMode.And )
          {
            filterCriterionsExpression = Expression.And( filterCriterionsExpression, propertyFilterCriterionExpression );
          }
          else
          {
            filterCriterionsExpression = Expression.Or( filterCriterionsExpression, propertyFilterCriterionExpression );
          }
        }

        // Loop to next DataGridItemProperty.
      }

      return filterCriterionsExpression;
    }

    private Expression GetAutoFilterValuesExpression( IQueryable queryable, ParameterExpression sharedParameterExpression )
    {
      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      AutoFilterMode autoFilterMode = m_parentCollectionView.AutoFilterMode;

      if( autoFilterMode == AutoFilterMode.None )
        return null;

      DataGridItemPropertyCollection itemProperties = m_parentCollectionView.ItemProperties;
      int itemPropertiesCount = itemProperties.Count;

      Expression autoFilterValuesExpression = null;

      for( int i = 0; i < itemPropertiesCount; i++ )
      {
        DataGridItemPropertyBase itemProperty = itemProperties[ i ];
        string itemPropertyName = itemProperty.Name;

        IList itemPropertyAutoFilterValues;

        if( m_parentCollectionView.AutoFilterValues.TryGetValue( itemPropertyName, out itemPropertyAutoFilterValues ) )
        {
          int itemPropertyAutoFilterValuesCount = itemPropertyAutoFilterValues.Count;

          if( itemPropertyAutoFilterValuesCount == 0 )
            continue;


          object[] itemPropertyAutoFilterValuesArray = new object[ itemPropertyAutoFilterValuesCount ];
          itemPropertyAutoFilterValues.CopyTo( itemPropertyAutoFilterValuesArray, 0 );

          Expression itemPropertyAutoFilterExpression = queryable.CreateEqualExpression( sharedParameterExpression, itemPropertyName, itemPropertyAutoFilterValuesArray );

          if( autoFilterValuesExpression == null )
          {
            autoFilterValuesExpression = itemPropertyAutoFilterExpression;
          }
          else
          {
            Debug.Assert( ( autoFilterMode == AutoFilterMode.And ) || ( autoFilterMode == AutoFilterMode.Or ) );

            // Merge this DataGridItemProperty AutoFilterExpressions
            if( autoFilterMode == AutoFilterMode.And )
            {
              autoFilterValuesExpression = Expression.And( autoFilterValuesExpression, itemPropertyAutoFilterExpression );
            }
            else
            {
              autoFilterValuesExpression = Expression.Or( autoFilterValuesExpression, itemPropertyAutoFilterExpression );
            }
          }
        }
        // Loop to next DataGridItemProperty.
      }

      return autoFilterValuesExpression;
    }

    internal override int GetGlobalIndexOf( object item )
    {
      return m_virtualPageManager.GetGlobalIndexOf( item );
    }

    internal override DataGridPageManagerBase GetVirtualPageManager()
    {
      return m_virtualPageManager;
    }

    internal object[] GetItemPropertyDistinctValues( DataGridItemPropertyBase dataGridItemProperty )
    {
      if( m_queryableSource == null )
        return new object[ 0 ];

      IQueryable distinctQueryable = m_queryableSource.GetSubGroupsAndCountsQueryable( dataGridItemProperty.Name, false, ListSortDirection.Ascending );

      List<object> distinctValuesAndCounts = new List<object>();
      try
      {
        System.Collections.IEnumerator enumerator = distinctQueryable.GetEnumerator();

        while( enumerator.MoveNext() )
        {
          QueryableExtensions.IQueryableGroupNameCountPair current = enumerator.Current as QueryableExtensions.IQueryableGroupNameCountPair;

          if( current != null )
            distinctValuesAndCounts.Add( current.GroupName );
        }

        return distinctValuesAndCounts.ToArray();
      }
      catch
      {
        // TimeOut exception on the connection or other.
        return new object[ 0 ];
      }
    }

    internal override void DisposeCore()
    {
      m_parentCollectionView = null;
      m_queryableSource = null;
      m_unsortedFilteredQueryableSource = null;
      m_virtualPageManager = null;
      base.DisposeCore();
    }

    #endregion INTERNAL METHODS

    #region PRIVATE FIELDS

    private DataGridVirtualizingQueryableCollectionView m_parentCollectionView;

    private IQueryable m_queryableSource;
    private IQueryable m_unsortedFilteredQueryableSource;

    private string[] m_primaryKeyPropertyNames;

    private DataGridLINQPageManager m_virtualPageManager;

    #endregion PRIVATE FIELDS
  }
}
