// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Range axes look for this interface on series to determine whether to
    /// anchor the origin to the bottom or top of the screen where possible.
    /// </summary>
    /// <remarks>
    /// Implementing this interface ensures that value margins will not cause
    /// an origin to float above the bottom or top of the screen if no
    /// data exists below or above.
    /// </remarks>
    public interface IAnchoredToOrigin
    {
        /// <summary>
        /// Gets the axis to which the data is anchored.
        /// </summary>
        IRangeAxis AnchoredAxis { get; }
    }
}
