// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Abstract class representing an interpolator which projects values to
    /// a continuous range defined by the From and To properties.
    /// </summary>
    /// <typeparam name="T">The data type of the values in the target range.</typeparam>
    /// <QualityBand>Preview</QualityBand>
    public abstract class RangeInterpolator<T> : Interpolator
    {
        /// <summary>
        /// Gets or sets a value representing the start value of the target range.
        /// </summary>
        public T From { get; set; }

        /// <summary>
        /// Gets or sets a value representing the end value of the target range.
        /// </summary>
        public T To { get; set; }
    }
}
