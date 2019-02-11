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

namespace Xceed.Wpf.DataGrid.Diagnostics
{
  internal static class DataGridTraceMessages
  {
    internal const string CannotCollapseDetail = "Cannot collapse the detail.";
    internal const string CannotExpandDetail = "Cannot expand the detail.";
    internal const string CannotProcessOnReset = "Cannot process while resetting.";
    internal const string CannotRemapDetails = "Cannot remap details when deferred.";
    internal const string CollapsedItemAdded = "Item(s) added under a collapsed node.";
    internal const string CollapsingDetail = "Collapsing a detail.";
    internal const string ContainerAdded = "The container was added.";
    internal const string ContainerFound = "The container was found.";
    internal const string ContainerGenerated = "A container was generated.";
    internal const string ContainerIsInDetail = "The container is located in a detail.";
    internal const string ContainerNotFound = "The container was not found.";
    internal const string ContainerRemoved = "The container was removed.";
    internal const string DetailAlreadyCollapsed = "The detail is already collapsed.";
    internal const string DetailAlreadyExpanded = "The detail is already expanded.";
    internal const string DetailExpanded = "The detail was expanded.";
    internal const string DetailCollapsed = "The detail was collapsed.";
    internal const string DetailExpected = "Detail expected.";
    internal const string DetailIsFloating = "The detail is floating.";
    internal const string DetailNodeAdded = "Detail node(s) added.";
    internal const string DetailNodeNotFound = "The detail node was not found.";
    internal const string DetailNotFound = "The detail was not found.";
    internal const string DetailNotSupported = "Details are not supported.";
    internal const string EmptyTree = "There is no node.";
    internal const string GroupFound = "The group was found.";
    internal const string GroupNodeAdded = "Group node(s) added.";
    internal const string GroupNodeRemoved = "Group node(s) removed.";
    internal const string GroupNotFound = "The group was not found.";
    internal const string InhibiterAlreadySet = "An inhibiter is already set.";
    internal const string IndexFound = "The index was found.";
    internal const string IndexUpdated = "The index was updated.";
    internal const string ItemAdded = "Item(s) added.";
    internal const string ItemFound = "The item was found.";
    internal const string ItemNotBelongingToGenerator = "The item does not belong to the generator.";
    internal const string ItemNotBelongingToNode = "The item does not belong to the node.";
    internal const string ItemNotFound = "The item was not found.";
    internal const string ItemNotFoundOrCollapsed = "The item was not found or is under a collapsed node.";
    internal const string NewStartNode = "New start node.";
    internal const string NewTreeCreated = "New tree node created.";
    internal const string NodeFound = "The node was found.";
    internal const string NodeIsCollapsed = "The node is collapsed.";
    internal const string NodeIsNotTheCurrentNode = "Node is not the current one.";
    internal const string RemapDetailNodes = "Remap details nodes.";
    internal const string RemapZombieDetail = "Master item not found for detail.";
    internal const string UnexpectedNode = "Unexpected node.";
  }
}
