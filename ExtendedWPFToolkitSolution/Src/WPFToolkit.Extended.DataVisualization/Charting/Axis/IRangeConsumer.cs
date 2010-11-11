// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// An object that consumes a range.
    /// </summary>
    public interface IRangeConsumer
    {
        /// <summary>
        /// Informs a range consumer that a provider's range has changed.
        /// </summary>
        /// <param name="provider">The range provider.</param>
        /// <param name="range">The range of data.</param>
        void RangeChanged(IRangeProvider provider, Range<IComparable> range);
    }
}
