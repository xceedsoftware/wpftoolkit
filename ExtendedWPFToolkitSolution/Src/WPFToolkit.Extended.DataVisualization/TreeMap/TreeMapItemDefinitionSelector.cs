// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.ObjectModel;
using System.Windows.Markup;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Provides a way to choose a TreeMapItemDefinition based on the data item and 
    /// the level of the item in the tree.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public abstract class TreeMapItemDefinitionSelector
    {
        /// <summary>
        /// Initializes a new instance of the TreeMapItemDefinitionSelector class.
        /// </summary>
        protected TreeMapItemDefinitionSelector()
        {
        }

        /// <summary>
        /// Returns an instance of a TreeMapItemDefinition class used to specify properties for the
        /// current item.
        /// </summary>
        /// <param name="treeMap">Reference to the TreeMap class.</param>
        /// <param name="item">One of the nodes in the ItemsSource hierarchy.</param>
        /// <param name="level">The level of the node in the hierarchy.</param>
        /// <returns>The TreeMapItemDefinition to be used for this node. If this method returns null
        /// the TreeMap will use the value of its ItemDefinition property.</returns>
        public abstract TreeMapItemDefinition SelectItemDefinition(TreeMap treeMap, object item, int level);
    }
}
