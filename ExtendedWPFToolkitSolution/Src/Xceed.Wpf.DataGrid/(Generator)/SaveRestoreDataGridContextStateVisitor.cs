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
using System.Text;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  /// <summary>
  /// This visitor saves and restores a DataGridContext's Expanded Groups and Expanded Items.
  /// 
  /// It will not handle the dataGridContext's details, beside expanding this dataGridContext's items
  /// which were expanded.
  /// 
  /// It will handle has many group levels as needed, or as specified.
  /// </summary>
  internal class SaveRestoreDataGridContextStateVisitor : SaveRestoreStateVisitor
  {
    #region CONSTRUCTORS

    public SaveRestoreDataGridContextStateVisitor()
      : this( true, int.MaxValue, false )
    {
    }

    public SaveRestoreDataGridContextStateVisitor( bool saveExpandedItems, int maxGroupLevel, bool stopAtFirstCollapsedGroup )
    {
      if( maxGroupLevel < 0 )
        throw new ArgumentOutOfRangeException( "maxGroupLevel" );

      m_saveExpandedItems = saveExpandedItems;
      m_maxGroupLevel = maxGroupLevel;
      m_stopAtFirstCollapsedGroup = stopAtFirstCollapsedGroup;
    }

    #endregion CONSTRUCTORS

    #region MaxGroupLevel Property

    private int m_maxGroupLevel;

    public int MaxGroupLevel
    {
      get
      {
        return m_maxGroupLevel;
      }
    }

    #endregion MaxGroupLevel Property

    #region SaveExpandedItems Property

    private bool m_saveExpandedItems;

    public bool SaveExpandedItems
    {
      get
      {
        return m_saveExpandedItems;
      }
    }

    #endregion SaveExpandedItems Property

    #region StopAtFirstCollapsedGroup Property

    private bool m_stopAtFirstCollapsedGroup;

    public bool StopAtFirstCollapsedGroup
    {
      get
      {
        return m_stopAtFirstCollapsedGroup;
      }
    }

    #endregion StopAtFirstCollapsedGroup Property

    #region PROTECTED METHODS

    protected override void InitializeCore()
    {
      m_rootDataGridContext = null;
      m_itemsToExpand = new List<WeakReference>();
      m_groupsStateDictionary = new Dictionary<GroupNamesTreeKey, bool>();
    }

    protected override void SaveStateCore( IDataGridContextVisitable dataGridContextVisitable )
    {
      try
      {
        bool visitWasStopped;
        m_rootDataGridContext = SaveRestoreStateVisitor.GetDataGridContextFromVisitable( dataGridContextVisitable );

        if( m_saveExpandedItems )
          dataGridContextVisitable.AcceptVisitor( 0, int.MaxValue, this, DataGridContextVisitorType.DataGridContext, true, out visitWasStopped );

        dataGridContextVisitable.AcceptVisitor( 0, int.MaxValue, this, DataGridContextVisitorType.Groups, false, out visitWasStopped );
      }
      finally
      {
        m_rootDataGridContext = null;
      }
    }

    protected override void RestoreStateCore( IDataGridContextVisitable dataGridContextVisitable )
    {
      try
      {
        m_rootDataGridContext = SaveRestoreDataGridContextStateVisitor.GetDataGridContextFromVisitable( dataGridContextVisitable );

        if( m_saveExpandedItems )
        {
          for( int i = 0; i < m_itemsToExpand.Count; i++ )
          {
            var dataItemToExpand = m_itemsToExpand[ i ].Target;
            if( dataItemToExpand == null )
              continue;

            // Verify if we have a System.Data.DataView as ItemsSource to 
            // ensure to restore the System.Data.DataRowView that represents
            // the System.Data.DataRow saved previously
            System.Data.DataView dataView = ItemsSourceHelper.TryGetDataViewFromDataGridContext( m_rootDataGridContext );

            if( ( dataView != null ) && ( dataItemToExpand is System.Data.DataRow ) )
            {
              foreach( System.Data.DataRowView dataRowView in dataView )
              {
                if( dataRowView.Row == dataItemToExpand )
                {
                  dataItemToExpand = dataRowView;
                  break;
                }
              }
            }
          }
        }

        bool visitWasStopped;
        dataGridContextVisitable.AcceptVisitor( 0, int.MaxValue, this, DataGridContextVisitorType.Groups, false, out visitWasStopped );
      }
      finally
      {
        m_itemsToExpand = null;
        m_rootDataGridContext = null;
      }
    }

    protected override void SavingVisit( DataGridContext sourceContext )
    {
      if( sourceContext.ParentDataGridContext == m_rootDataGridContext )
      {
        // Ensure to get a reference to the System.Data.DataRow when doing a 
        // save/restore of a System.Data.DataRowView since the view is recreated
        // for every detail views
        object parentItem = ItemsSourceHelper.TryGetDataRowFromDataItem( sourceContext.ParentItem );

        m_itemsToExpand.Add( new WeakReference( parentItem ) );
      }
    }

    protected override void SavingVisit( DataGridContext sourceContext, CollectionViewGroup group, object[] namesTree, int groupLevel, bool isExpanded, bool isComputedExpanded )
    {
      if( sourceContext != m_rootDataGridContext )
        throw new InvalidOperationException( "Group does not belong to the root DataGridContext." );

      if( groupLevel > m_maxGroupLevel )
        return;

      if( ( m_stopAtFirstCollapsedGroup ) && ( !isComputedExpanded ) )
        return;

      GroupNamesTreeKey groupNamesTreeKey = new GroupNamesTreeKey( namesTree );

      m_groupsStateDictionary.Add( groupNamesTreeKey, isExpanded );
    }

    protected override void RestoringVisit( DataGridContext sourceContext, CollectionViewGroup group, object[] namesTree, int groupLevel, bool isExpanded, bool isComputedExpanded )
    {
      if( sourceContext != m_rootDataGridContext )
        throw new InvalidOperationException( "Group does not belong to the root DataGridContext." );

      if( groupLevel > m_maxGroupLevel )
        return;

      GroupNamesTreeKey groupNamesTreeKey = new GroupNamesTreeKey( namesTree );

      bool wasExpanded;
      if( m_groupsStateDictionary.TryGetValue( groupNamesTreeKey, out wasExpanded ) )
      {
        if( wasExpanded )
        {
          sourceContext.ExpandGroup( group, true );
        }
        else
        {
          sourceContext.CollapseGroup( group, true );
        }
      }
      else if( m_stopAtFirstCollapsedGroup )
      {
        sourceContext.CollapseGroup( group, true );
      }
    }

    #endregion PROTECTED METHODS

    #region PRIVATE FIELDS

    private DataGridContext m_rootDataGridContext;
    private List<WeakReference> m_itemsToExpand;
    private Dictionary<GroupNamesTreeKey, bool> m_groupsStateDictionary;

    #endregion PRIVATE FIELDS
  }
}
