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
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using Xceed.Utils.Wpf;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal static class GroupingHelper
  {
    public static bool HasGroup( DataGridContext dataGridContext )
    {
      if( dataGridContext.GroupLevelDescriptions.Count > 0 )
        return true;

      return GroupingHelper.HasGroup( dataGridContext.DetailConfigurations );
    }

    private static bool HasGroup( DetailConfigurationCollection configurations )
    {
      bool returnValue = false;

      int detailConfigurationCount = configurations.Count;

      for( int i = 0; i < detailConfigurationCount; i++ )
      {
        DetailConfiguration detailConfiguration = configurations[ i ];

        if( detailConfiguration.DetailConfigurations != null )
          returnValue = GroupingHelper.HasGroup( detailConfiguration.DetailConfigurations );

        if( returnValue )
          break;

        returnValue = ( detailConfiguration.GroupLevelDescriptions.Count > 0 );

        if( returnValue )
          break;
      }

      return returnValue;
    }

    public static bool IsAlreadyGroupedBy( ColumnManagerCell cell )
    {
      DataGridContext cellDataGridContext = DataGridControl.GetDataGridContext( cell );

      GroupLevelDescriptionCollection cellGroupLevelDescriptions = cellDataGridContext.GroupLevelDescriptions;

      bool isAlreadyGroupedBy = false;
      if( cellDataGridContext != null )
      {
        foreach( GroupLevelDescription description in cellGroupLevelDescriptions )
        {
          if( description.FieldName == cell.FieldName )
          {
            isAlreadyGroupedBy = true;
            break;
          }
        }
      }

      return isAlreadyGroupedBy;
    }

    public static HierarchicalGroupByControl GetHierarchicalGroupByControl( UIElement element )
    {
      DependencyObject parent = TreeHelper.GetParent( element );

      while( parent != null )
      {
        if( parent is HierarchicalGroupByControl )
          break;

        parent = TreeHelper.GetParent( parent );
      }

      return parent as HierarchicalGroupByControl;
    }

    public static int GetGroupDescriptionIndex( ObservableCollection<GroupDescription> groupDescriptions, GroupLevelDescription groupLevelDescription, DropMarkAlignment alignment )
    {
      int groupIndex = groupDescriptions.IndexOf( groupLevelDescription.GroupDescription );

      if( groupIndex > -1 )
      {
        if( alignment == DropMarkAlignment.Far )
          groupIndex++;

        return groupIndex;
      }
      else
      {
        // return the size of the Collection if not found
        return groupDescriptions.Count;
      }
    }

    public static int GetGroupDescriptionIndexFromFieldName( DataGridContext dataGridContext, string fieldName, DropMarkAlignment alignment )
    {
      ObservableCollection<GroupDescription> groupDescriptions = dataGridContext.Items.GroupDescriptions;

      for( int i = groupDescriptions.Count - 1; i >= 0; i-- )
      {
        if( DataGridContext.GetColumnNameFromGroupDescription( groupDescriptions[ i ] ) == fieldName )
        {
          if( alignment == DropMarkAlignment.Far )
            i++;

          return i;
        }
      }

      return dataGridContext.Items.GroupDescriptions.Count;
    }

    public static bool IsColumnManagerCellInDataGridContext( DataGridContext dataGridContext, ColumnManagerCell cell )
    {
      DataGridContext cellDataGridContext = DataGridControl.GetDataGridContext( cell );

      Debug.Assert( cellDataGridContext != null );

      if( ( dataGridContext != null ) &&
          ( cellDataGridContext != null ) &&
          ( dataGridContext.GroupLevelDescriptions == cellDataGridContext.GroupLevelDescriptions ) )
        return true;

      return GroupingHelper.IsGroupLevelDescriptionsInDetailConfigurations( dataGridContext.DetailConfigurations, cellDataGridContext.GroupLevelDescriptions );
    }

    private static bool IsGroupLevelDescriptionsInDetailConfigurations( DetailConfigurationCollection configurations, GroupLevelDescriptionCollection groupLevelDescriptions )
    {
      bool returnValue = false;

      int detailConfigurationCount = configurations.Count;

      for( int i = 0; i < detailConfigurationCount; i++ )
      {
        DetailConfiguration detailConfiguration = configurations[ i ];

        if( detailConfiguration.DetailConfigurations != null )
          returnValue = GroupingHelper.IsGroupLevelDescriptionsInDetailConfigurations( detailConfiguration.DetailConfigurations, groupLevelDescriptions );

        if( returnValue )
          break;

        returnValue = ( detailConfiguration.GroupLevelDescriptions == groupLevelDescriptions );

        if( returnValue )
          break;
      }

      return returnValue;
    }

    public static void AddNewGroupFromColumnManagerCell( ColumnManagerCell cell, GroupLevelDescription draggedOverDescription, DropMarkAlignment alignment, DataGridControl parentDataGridControl )
    {
      if( cell == null )
        return;

      DataGridContext cellDataGridContext = DataGridControl.GetDataGridContext( cell );
      DataGridControl cellParentGrid = cellDataGridContext.DataGridControl;

      if( ( cellDataGridContext == null ) ||
          ( parentDataGridControl == null ) ||
          ( parentDataGridControl != cellParentGrid ) )
        return;

      // By default, add it at the end
      int newPos = cellDataGridContext.GroupLevelDescriptions.Count;

      if( draggedOverDescription != null )
        newPos = GroupingHelper.GetGroupDescriptionIndexFromFieldName( cellDataGridContext, draggedOverDescription.FieldName, alignment );

      ColumnBase column = cell.ParentColumn;

      if( column != null )
      {
        GroupingHelper.AddNewGroupFromColumn( column, cellDataGridContext.Items.GroupDescriptions, newPos );
      }
    }

    public static void AppendNewGroupFromColumnManagerCell( ColumnManagerCell cell, DataGridControl parentDataGridControl )
    {
      GroupingHelper.AddNewGroupFromColumnManagerCell( cell, null, DropMarkAlignment.Far, parentDataGridControl );
    }

    public static void AddNewGroupFromColumn( ColumnBase column, ObservableCollection<GroupDescription> targetGroupDescriptions, int position )
    {
      GroupDescription groupDescription = column.GroupDescription;

      if( groupDescription == null )
        groupDescription = new DataGridGroupDescription( column.FieldName );

      DataGridGroupDescription dataGridGroupDescription = groupDescription as DataGridGroupDescription;

      if( ( dataGridGroupDescription != null ) && ( dataGridGroupDescription.GroupConfiguration == null ) )
        dataGridGroupDescription.GroupConfiguration = column.GroupConfiguration;

      targetGroupDescriptions.Insert( position, groupDescription );
    }

    public static void MoveGroupDescription( ColumnCollection targetColumns, ObservableCollection<GroupDescription> targetGroupDescriptions, GroupLevelDescription targetGroupLevelDescription, DropMarkAlignment targetAlignment, GroupLevelDescription movedGroupLevelDescription, DataGridControl parentDataGridControl )
    {
      Debug.Assert( targetColumns != null );
      Debug.Assert( targetGroupDescriptions != null );
      Debug.Assert( targetGroupLevelDescription != null );
      Debug.Assert( movedGroupLevelDescription != null );

      if( ( parentDataGridControl == null ) ||
          ( targetColumns == null ) ||
          ( targetGroupDescriptions == null ) ||
          ( targetGroupLevelDescription == null ) ||
          ( movedGroupLevelDescription == null ) )
        return;

      int oldPos = GroupingHelper.GetGroupDescriptionIndex( targetGroupDescriptions, movedGroupLevelDescription, DropMarkAlignment.Near );
      int newPos = GroupingHelper.GetGroupDescriptionIndex( targetGroupDescriptions, targetGroupLevelDescription, targetAlignment );

      if( newPos > oldPos )
        newPos--;

      if( newPos != oldPos )
      {
        Debug.Assert( oldPos < targetGroupDescriptions.Count );

        targetGroupDescriptions.Move( oldPos, newPos );
      }
    }

    public static void RemoveGroupDescription( ObservableCollection<GroupDescription> groupDescriptions, GroupLevelDescription groupLevelDescription, DataGridControl parentDataGridControl )
    {
      if( ( groupLevelDescription == null ) ||
          ( parentDataGridControl == null ) )
        return;

      groupDescriptions.Remove( groupLevelDescription.GroupDescription );
    }

    public static bool ValidateMaxGroupDescriptions( DataGridContext draggedContext )
    {
      Debug.Assert( draggedContext != null );

      int maxGroupDescriptionCount = -1;
      int currentGroupDescriptionCount = -1;

      CollectionView collectionView = draggedContext.Items;

      if( collectionView == null )
        return true;

      ObservableCollection<GroupDescription> groupDescriptions = collectionView.GroupDescriptions;

      if( groupDescriptions == null )
        return true;

      maxGroupDescriptionCount = draggedContext.MaxGroupLevels;
      currentGroupDescriptionCount = groupDescriptions.Count;

      return ( ( maxGroupDescriptionCount == -1 ) || ( currentGroupDescriptionCount < maxGroupDescriptionCount ) );
    }
  }
}
