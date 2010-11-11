// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace System.Windows.Controls.DataVisualization.Charting.Primitives
{
    /// <summary>
    /// Specifies the edge position of a child element that is inside an
    /// EdgePanel.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public enum Edge
    {
        /// <summary>
        /// A child element that is positioned in the center of a EdgePanel.
        /// </summary>
        Center,

        /// <summary>
        /// A child element that is positioned on the left side of the
        /// EdgePanel.
        /// </summary>
        Left,

        /// <summary>
        /// A child element that is positioned at the top of the EdgePanel.
        /// </summary>
        Top,
        
        /// <summary>
        /// A child element that is positioned on the right side of the
        /// EdgePanel.
        /// </summary>
        Right,

        /// <summary>
        /// A child element that is positioned at the bottom of the EdgePanel.
        /// </summary>
        Bottom,
    }
}