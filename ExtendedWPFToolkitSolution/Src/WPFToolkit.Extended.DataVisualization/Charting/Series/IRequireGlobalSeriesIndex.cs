// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Defines methods on classes that contain a global index.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public interface IRequireGlobalSeriesIndex
    {
        /// <summary>
        /// Occurs when a global series index changes.
        /// </summary>
        /// <param name="globalIndex">The global index that has changed.
        /// </param>
        void GlobalSeriesIndexChanged(int? globalIndex);
    }
}