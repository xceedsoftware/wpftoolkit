/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/


namespace Xceed.Wpf.DataGrid.Diagnostics
{
  internal enum DataGridTraceEventId
  {
    CustomItemContainerGenerator_ApplyGroupChanges = 0,
    CustomItemContainerGenerator_CleanupGenerator,
    CustomItemContainerGenerator_CloseDetailsForItem,
    CustomItemContainerGenerator_CollapseDetails,
    CustomItemContainerGenerator_CollapseGroup,
    CustomItemContainerGenerator_ContainerFromIndex,
    CustomItemContainerGenerator_ContainerFromItem,
    CustomItemContainerGenerator_CreateDetailsForItem,
    CustomItemContainerGenerator_CreateDetailsHelper,
    CustomItemContainerGenerator_EnsureNodeTreeCreated,
    CustomItemContainerGenerator_ExpandDetails,
    CustomItemContainerGenerator_ExpandGroup,
    CustomItemContainerGenerator_FindIndexForItem,
    CustomItemContainerGenerator_GenerateNextLocalContainer,
    CustomItemContainerGenerator_GenerateStickyFootersForDetail,
    CustomItemContainerGenerator_GenerateStickyHeadersForDetail,
    CustomItemContainerGenerator_GenPosArraysRemoveAt,
    CustomItemContainerGenerator_GetGroupFromItem,
    CustomItemContainerGenerator_GetGroupIndex,
    CustomItemContainerGenerator_GetRealizedContainerForIndex,
    CustomItemContainerGenerator_GetRealizedIndexForContainer,
    CustomItemContainerGenerator_HandleDetailReset,
    CustomItemContainerGenerator_HandleGlobalItemsReset,
    CustomItemContainerGenerator_HandleHeadersFootersAddition,
    CustomItemContainerGenerator_HandleHeadersFootersRemoveMoveReplace,
    CustomItemContainerGenerator_HandleItemAddition,
    CustomItemContainerGenerator_HandleItemReset,
    CustomItemContainerGenerator_HandleSameLevelGroupAddition,
    CustomItemContainerGenerator_HandleSameLevelGroupMove,
    CustomItemContainerGenerator_HandleSameLevelGroupRemove,
    CustomItemContainerGenerator_HandleSameLevelGroupReset,
    CustomItemContainerGenerator_IItemContainerGenerator_GenerateNext,
    CustomItemContainerGenerator_IItemContainerGenerator_Remove,
    CustomItemContainerGenerator_IndexFromItem,
    CustomItemContainerGenerator_IsGroupExpanded,
    CustomItemContainerGenerator_ItemFromContainer,
    CustomItemContainerGenerator_ItemFromIndex,
    CustomItemContainerGenerator_MoveGeneratorBackward,
    CustomItemContainerGenerator_MoveGeneratorForward,
    CustomItemContainerGenerator_OnDetailConfigurationsChanged,
    CustomItemContainerGenerator_OnDetailGeneratorContentChanged,
    CustomItemContainerGenerator_OnGeneratorNodeExpansionStateChanged,
    CustomItemContainerGenerator_OnGeneratorNodeGroupsCollectionChanged,
    CustomItemContainerGenerator_OnGeneratorNodeHeadersFootersCollectionChanged,
    CustomItemContainerGenerator_OnGeneratorNodeItemsCollectionChanged,
    CustomItemContainerGenerator_OnGroupGeneratorNodeIsExpandedChanged,
    CustomItemContainerGenerator_OnGroupGeneratorNodeIsExpandedChanging,
    CustomItemContainerGenerator_OnGroupsChanged,
    CustomItemContainerGenerator_OnItemsChanged,
    CustomItemContainerGenerator_ProcessGroupRemoval,
    CustomItemContainerGenerator_RemapFloatingDetails,
    CustomItemContainerGenerator_SetupInitialItemsNodes,
    CustomItemContainerGenerator_ToggleDetails,
    CustomItemContainerGenerator_ToggleGroupExpansion,
    CustomItemContainerGenerator_UpdateFooters,
    CustomItemContainerGenerator_UpdateGenPosToIndexList,
    CustomItemContainerGenerator_UpdateHeaders,
  }
}
