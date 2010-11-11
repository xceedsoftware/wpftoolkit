// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// An axis with a range.
    /// </summary>
    public interface IRangeAxis : IAxis, IRangeConsumer
    {
        /// <summary>
        /// Gets the range of values displayed on the axis.
        /// </summary>
        Range<IComparable> Range { get; }

        /// <summary>
        /// The plot area coordinate of a value.
        /// </summary>
        /// <param name="position">The position at which to retrieve the plot 
        /// area coordinate.</param>
        /// <returns>The plot area coordinate.</returns>
        IComparable GetValueAtPosition(UnitValue position);

        /// <summary>
        /// Gets the origin value on the axis.
        /// </summary>
        IComparable Origin { get; }
    }
}
