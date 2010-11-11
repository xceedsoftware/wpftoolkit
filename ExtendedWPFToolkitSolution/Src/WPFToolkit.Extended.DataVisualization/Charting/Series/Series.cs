// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.ObjectModel;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Represents a control that contains a data series.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public abstract partial class Series : Control, ISeries, IRequireSeriesHost
    {
        /// <summary>
        /// The name of the Title property.
        /// </summary>
        protected const string TitleName = "Title";

        #region public ISeriesHost SeriesHost
        /// <summary>
        /// Gets or sets the parent instance the Series belongs to.
        /// </summary>
        public ISeriesHost SeriesHost
        {
            get { return _seriesHost; }
            set
            {
                ISeriesHost oldValue = _seriesHost;
                _seriesHost = value;
                if (oldValue != _seriesHost)
                {
                    OnSeriesHostPropertyChanged(oldValue, _seriesHost);
                }
            }
        }

        /// <summary>
        /// Stores the Parent instance the Series belongs to.
        /// </summary>
        private ISeriesHost _seriesHost;

        /// <summary>
        /// Called when the value of the SeriesHost property changes.
        /// </summary>
        /// <param name="oldValue">The value to be replaced.</param>
        /// <param name="newValue">The new series host value.</param>
        protected virtual void OnSeriesHostPropertyChanged(ISeriesHost oldValue, ISeriesHost newValue)
        {
            if (newValue != null && oldValue != null)
            {
                throw new InvalidOperationException(Properties.Resources.Series_SeriesHost_SeriesHostPropertyNotNull);
            }
        }
        #endregion public ISeriesHost SeriesHost

        #region public ObservableCollection<object> LegendItems
        /// <summary>
        /// Gets the legend items to be added to the legend.
        /// </summary>
        public ObservableCollection<object> LegendItems { get; private set; }
        #endregion public ObservableCollection<object> LegendItems

        #region public object Title
        /// <summary>
        /// Gets or sets the title content of the Series.
        /// </summary>
        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                TitleName,
                typeof(object),
                typeof(Series),
                new PropertyMetadata(OnTitleChanged));

        /// <summary>
        /// TitleProperty property changed callback.
        /// </summary>
        /// <param name="o">Series for which the Title changed.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnTitleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((Series)o).OnTitleChanged(e.OldValue, e.NewValue);
        }

        /// <summary>
        /// Called when the Title property changes.
        /// </summary>
        /// <param name="oldValue">The old value of the Title property.</param>
        /// <param name="newValue">The new value of the Title property.</param>
        protected virtual void OnTitleChanged(object oldValue, object newValue)
        {
        }
        #endregion public object Title

        /// <summary>
        /// Initializes a new instance of the Series class.
        /// </summary>
        protected Series()
        {
            LegendItems = new NoResetObservableCollection<object>();
        }
    }
}