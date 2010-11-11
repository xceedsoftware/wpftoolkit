// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Defines the selection behavior for a series.
    /// </summary>
    public enum SeriesSelectionMode
    {
        /// <summary>
        /// Selection is disabled.
        /// </summary>
        None,

        /// <summary>
        /// The user can select only one item at a time.
        /// </summary>
        Single,

        /// <summary>
        /// The user can select multiple items without holding down a modifier key.
        /// </summary>
        Multiple,
    }
}
