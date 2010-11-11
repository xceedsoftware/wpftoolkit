// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Defines properties, methods and events for classes that host a 
    /// collection of Series objects.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public interface ISeriesHost : IRequireSeriesHost, IResourceDictionaryDispenser
    {
        /// <summary>
        /// Gets the collection of axes the series host has available.
        /// </summary>
        ObservableCollection<IAxis> Axes { get; }

        /// <summary>
        /// Gets the collection of series the series host has available.
        /// </summary>
        ObservableCollection<ISeries> Series { get; }

        /// <summary>
        /// Gets the foreground elements.
        /// </summary>
        ObservableCollection<UIElement> ForegroundElements { get; }

        /// <summary>
        /// Gets the background elements.
        /// </summary>
        ObservableCollection<UIElement> BackgroundElements { get; }
    }
}