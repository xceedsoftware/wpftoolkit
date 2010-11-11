// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Specifies the supported interpolation modes.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public enum InterpolationMode
    {
        /// <summary>
        /// Interpolation shall be applied to leaf nodes only in the tree.
        /// </summary>
        LeafNodesOnly = 0,

        /// <summary>
        /// Interpolation shall be applied to all nodes in the tree.
        /// </summary>
        AllNodes = 1,
    }
}