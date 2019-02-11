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
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DataGridItemContainerManager
  {
    internal DataGridItemContainerManager( IDataGridItemContainer owner )
    {
      if( owner == null )
        throw new ArgumentNullException( "owner" );

      if( !( owner is FrameworkElement ) )
        throw new ArgumentException( "The owner must derive from FrameworkElement.", "owner" );

      m_owner = owner;
    }

    #region CanBeRecycled Property

    internal bool CanBeRecycled
    {
      get
      {
        if( m_preparedContainers.Any( item => !item.CanBeRecycled ) )
          return false;

        if( m_unpreparedContainers.Any( item => !item.CanBeRecycled ) )
          return false;

        return true;
      }
    }

    #endregion

    internal void Prepare( DataGridContext dataGridContext, object dataItem )
    {
      m_containersPrepared = true;
      m_dataGridContext = dataGridContext;
      m_dataItem = dataItem;

      this.Update();
    }

    internal void Clear( bool isRecyclingCandidate )
    {
      m_containersPrepared = false;
      m_dataGridContext = null;
      m_dataItem = null;

      try
      {
        this.ClearContainers( isRecyclingCandidate );
      }
      finally
      {
        m_unpreparedContainers.Clear();
        m_preparedContainers.Clear();
      }
    }

    internal void CleanRecyclingCandidates()
    {
      var newContainers = m_owner.GetTemplatedChildDataGridItemContainers().ToList();

      foreach( var container in newContainers )
      {
        container.IsRecyclingCandidate = false;
        container.CleanRecyclingCandidate();
      }
    }

    internal void Update()
    {
      var newContainers = m_owner.GetTemplatedChildDataGridItemContainers().ToList();

      try
      {
        this.ClearContainers( newContainers, false );
      }
      finally
      {
        foreach( var container in newContainers )
        {
          if( m_preparedContainers.Contains( container ) || m_unpreparedContainers.Contains( container ) )
            continue;

          m_unpreparedContainers.Add( container );
        }
      }

      this.PrepareContainers();
    }

    private void PrepareContainers()
    {
      if( !m_containersPrepared )
        return;

      foreach( var container in m_unpreparedContainers.ToList() )
      {
        m_unpreparedContainers.Remove( container );
        m_preparedContainers.Add( container );

        // Row based objects apply their template through their implementation of PrepareContainer.
        // No need to call ApplyTemplate beforehand.
        var row = container as Row;
        if( row == null )
        {
          var fe = container as FrameworkElement;
          if( fe != null )
          {
            fe.ApplyTemplate();
          }
        }

        container.PrepareContainer( m_dataGridContext, m_dataItem );
      }
    }

    private void ClearContainers( bool isRecyclingCandidate )
    {
      this.ClearContainers( new List<IDataGridItemContainer>( 0 ), isRecyclingCandidate );
    }

    private void ClearContainers( ICollection<IDataGridItemContainer> keep, bool isRecyclingCandidate )
    {
      foreach( var container in m_unpreparedContainers.Except( keep ).ToList() )
      {
        m_unpreparedContainers.Remove( container );

        container.IsRecyclingCandidate = isRecyclingCandidate;

        if( isRecyclingCandidate )
          continue;

        var row = container as Row;
        if( row != null )
        {
          row.ClearCellsHost();
        }
      }

      foreach( var container in m_preparedContainers.Except( keep ).ToList() )
      {
        m_preparedContainers.Remove( container );

        container.IsRecyclingCandidate = isRecyclingCandidate;
        container.ClearContainer();
      }
    }

    private readonly IDataGridItemContainer m_owner;
    private readonly ICollection<IDataGridItemContainer> m_unpreparedContainers = new HashSet<IDataGridItemContainer>();
    private readonly ICollection<IDataGridItemContainer> m_preparedContainers = new HashSet<IDataGridItemContainer>();

    private DataGridContext m_dataGridContext;
    private object m_dataItem;
    private bool m_containersPrepared; //false
  }
}
