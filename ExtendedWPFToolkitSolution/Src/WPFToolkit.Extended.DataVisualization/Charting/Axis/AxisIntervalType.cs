// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Specifies an interval type.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    internal enum AxisIntervalType
    {
        /// <summary>
        /// Automatically determined by the ISeriesHost control.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// The interval type is numerical.
        /// </summary>
        Number = 1,

        /// <summary>
        /// The interval type is years.
        /// </summary>
        Years = 2,

        /// <summary>
        /// The interval type is months.
        /// </summary>
        Months = 3,

        /// <summary>
        /// The interval type is weeks.
        /// </summary>
        Weeks = 4,

        /// <summary>
        /// The interval type is days.
        /// </summary>
        Days = 5,

        /// <summary>
        /// The interval type is hours.
        /// </summary>
        Hours = 6,

        /// <summary>
        /// The interval type is minutes.
        /// </summary>
        Minutes = 7,

        /// <summary>
        /// The interval type is seconds.
        /// </summary>
        Seconds = 8,

        /// <summary>
        /// The interval type is milliseconds.
        /// </summary>
        Milliseconds = 9,
    }
}