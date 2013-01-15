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
using System.Data;
using System.Diagnostics;
using System.Collections;
using Xceed.Utils.Collections;

namespace Xceed.Wpf.DataGrid
{
  public sealed class DataGridVirtualizingQueryableCollectionView : DataGridVirtualizingCollectionViewBase
  {
    #region CONSTRUCTORS

    public DataGridVirtualizingQueryableCollectionView()
      : this( null, true, DataGridVirtualizingCollectionViewBase.DefaultPageSize, DataGridVirtualizingCollectionViewBase.DefaultMaxRealizedItemCount )
    {
    }

    public DataGridVirtualizingQueryableCollectionView( IQueryable queryableSource )
      : this( queryableSource, true, DataGridVirtualizingCollectionViewBase.DefaultPageSize, DataGridVirtualizingCollectionViewBase.DefaultMaxRealizedItemCount )
    {
    }

    public DataGridVirtualizingQueryableCollectionView( IQueryable queryableSource, bool autoCreateItemProperties, int pageSize, int maxRealizedItemCount )
      : base( queryableSource, null, autoCreateItemProperties, pageSize, maxRealizedItemCount )
    {
      m_pageManagerSyncRoot = new object();
    }

    #endregion CONSTRUCTORS

    #region QueryableSource PROPERTY

    public IQueryable QueryableSource
    {
      get
      {
        return this.ModelSource as IQueryable;
      }
    }

    #endregion QueryableSource PROPERTY


    #region INTERNAL METHODS

    internal override DataGridVirtualizingCollectionViewGroupBase CreateNewRootGroup()
    {
      bool rootIsBottomLevel = ( this.GroupDescriptions == null ) ? true : ( this.GroupDescriptions.Count == 0 );

      return new DataGridVirtualizingQueryableCollectionViewGroupRoot( this, m_pageManagerSyncRoot, rootIsBottomLevel );
    }

    internal override System.Collections.IEnumerator GetVirtualEnumerator()
    {
      return ( ( DataGridVirtualizingQueryableCollectionViewGroupRoot )this.RootGroup ).GetVirtualPageManager().GetEnumerator();
    }

    internal override void RefreshDistinctValuesForField( DataGridItemPropertyBase dataGridItemProperty )
    {
      if( dataGridItemProperty == null )
        return;

      if( dataGridItemProperty.CalculateDistinctValues == false )
        return;

      // List containing current column distinct values
      HashSet<object> currentColumnDistinctValues = new HashSet<object>();

      ReadOnlyObservableHashList readOnlyColumnDistinctValues = null;

      // If the key is not set in DistinctValues yet, do not calculate distinct values for this field
      if( !( ( DistinctValuesDictionary )this.DistinctValues ).InternalTryGetValue( dataGridItemProperty.Name, out readOnlyColumnDistinctValues ) )
        return;

      ObservableHashList columnDistinctValues = readOnlyColumnDistinctValues.InnerObservableHashList;

      // We use the DistinctValuesSortComparer if present, else the SortComparer for the DataGridItemProperty, else, 
      // the Comparer used is the one of the base class.
      IComparer distinctValuesSortComparer = dataGridItemProperty.DistinctValuesSortComparer;

      if( distinctValuesSortComparer == null )
        distinctValuesSortComparer = dataGridItemProperty.SortComparer;

      using( columnDistinctValues.DeferINotifyCollectionChanged() )
      {
        DataGridVirtualizingQueryableCollectionViewGroupRoot rootGroup = this.RootGroup as DataGridVirtualizingQueryableCollectionViewGroupRoot;

        Debug.Assert( rootGroup != null );

        object[] distinctValues = rootGroup.GetItemPropertyDistinctValues( dataGridItemProperty );

        foreach( object distinctValue in distinctValues )
        {
          // Compute current value to be able to remove unused values
          currentColumnDistinctValues.Add( distinctValue );
        }

        DataGridCollectionViewBase.RemoveUnusedDistinctValues( 
          distinctValuesSortComparer, currentColumnDistinctValues, columnDistinctValues, null );
      }
    }

    #endregion

    #region PRIVATE FIELDS

    private object m_pageManagerSyncRoot;

    #endregion
  }
}
