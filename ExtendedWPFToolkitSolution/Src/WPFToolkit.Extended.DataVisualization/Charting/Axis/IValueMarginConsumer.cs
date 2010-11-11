// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Consumes value margins and uses them to lay out objects.
    /// </summary>
    public interface IValueMarginConsumer
    {
        /// <summary>
        /// Updates layout to accommodate for value margins.
        /// </summary>
        /// <param name="provider">A value margin provider.</param>
        /// <param name="valueMargins">A sequence of value margins.</param>
        void ValueMarginsChanged(IValueMarginProvider provider, IEnumerable<ValueMargin> valueMargins);
    }
}
