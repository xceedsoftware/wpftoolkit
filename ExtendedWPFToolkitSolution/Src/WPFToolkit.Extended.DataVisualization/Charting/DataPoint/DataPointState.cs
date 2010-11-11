// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Describes the state a data point is in.
    /// </summary>
    public enum DataPointState
    {
        /// <summary>
        /// Data point has been created.
        /// </summary>
        Created,

        /// <summary>
        /// Data point is in the process of being revealed.
        /// </summary>
        Showing,

        /// <summary>
        /// Data point is visible in the plot area.
        /// </summary>
        Normal,

        /// <summary>
        /// Data point is in the process of being removed from the plot area.
        /// </summary>
        PendingRemoval,

        /// <summary>
        /// Data point is in the process of being hidden.
        /// </summary>
        Hiding,

        /// <summary>
        /// Data point is hidden.
        /// </summary>
        Hidden,
    }
}