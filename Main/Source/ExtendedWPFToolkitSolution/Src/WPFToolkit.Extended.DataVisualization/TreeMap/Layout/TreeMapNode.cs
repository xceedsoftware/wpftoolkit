// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Holds the information needed by the tree map layout algorithm, such as the area
    /// associated with this node and the list of children. The class also contains
    /// an DataContext object which is the real user context, and a reference to the UI
    /// container associated with this node.
    /// </summary>
    internal class TreeMapNode
    {
        /// <summary>
        /// Gets or sets a value representing the area associated with this node.
        /// This value is relative to all the other values in the hierarchy; the layout
        /// algorithm will allocated a real area proportional to this value.
        /// </summary>
        public double Area { get; set; }

        /// <summary>
        /// Gets or sets the list of children under this node.
        /// </summary>
        public IEnumerable<TreeMapNode> Children { get; set; }

        /// <summary>
        /// Gets or sets a value representing the WeakEventListener associated with the
        /// ItemsSource that created the children of this node.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by implementation; warning is only for the test project.")]
        internal WeakEventListener<TreeMap, object, NotifyCollectionChangedEventArgs> WeakEventListener { get; set; }

        /// <summary>
        /// Gets or sets a value representing a reference to the user's custom data object.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by implementation; warning is only for the test project.")]
        public object DataContext { get; set; }

        /// <summary>
        /// Gets or sets a value representing the associated Silverlight UI element.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by implementation; warning is only for the test project.")]
        public FrameworkElement Element { get; set; }

        /// <summary>
        /// Gets or sets a value representing the TreeMapItemDefinition used to describe 
        /// properties of this item.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by implementation; warning is only for the test project.")]
        public TreeMapItemDefinition ItemDefinition { get; set; }

        /// <summary>
        /// Gets or sets a value representing the padding between this node and its children.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by implementation; warning is only for the test project.")]
        public Thickness ChildItemPadding { get; set; }

        /// <summary>
        /// Gets or sets a value representing the level of this node in the tree (the
        /// root node is at level 0).
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by implementation; warning is only for the test project.")]
        public int Level { get; set; }
    }
}
