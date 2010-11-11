// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Extension methods for series hosts.
    /// </summary>
    internal static class ISeriesHostExtensions
    {
        /// <summary>
        /// Gets all series that track their global indexes recursively.
        /// </summary>
        /// <param name="rootSeriesHost">The root series host.</param>
        /// <returns>A sequence of series.</returns>
        public static IEnumerable<ISeries> GetDescendentSeries(this ISeriesHost rootSeriesHost)
        {
            Queue<ISeries> series = new Queue<ISeries>(rootSeriesHost.Series);
            while (series.Count != 0)
            {
                ISeries currentSeries = series.Dequeue();
                yield return currentSeries;

                ISeriesHost seriesHost = currentSeries as ISeriesHost;
                if (seriesHost != null)
                {
                    foreach (ISeries childSeries in seriesHost.Series)
                    {
                        series.Enqueue(childSeries);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether an axis is in use by the series 
        /// host.
        /// </summary>
        /// <param name="that">The series host.</param>
        /// <param name="axis">The axis that may or may not be used by a 
        /// series.</param>
        /// <returns>A value indicating whether an axis is in use by the series 
        /// host.</returns>
        public static bool IsUsedByASeries(this ISeriesHost that, IAxis axis)
        {
            return axis.RegisteredListeners.OfType<ISeries>().Intersect(that.Series).Any();
        }
    }
}