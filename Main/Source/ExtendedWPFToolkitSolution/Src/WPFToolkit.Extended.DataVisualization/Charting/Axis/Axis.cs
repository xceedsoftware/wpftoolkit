// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// An axis class used to determine the plot area coordinate of values.
    /// </summary>
    public abstract class Axis : Control, IAxis
    {
        #region public AxisLocation Location
        /// <summary>
        /// Gets or sets the axis location.
        /// </summary>
        public AxisLocation Location
        {
            get { return (AxisLocation)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        /// <summary>
        /// Identifies the Location dependency property.
        /// </summary>
        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register(
                "Location",
                typeof(AxisLocation),
                typeof(Axis),
                new PropertyMetadata(AxisLocation.Auto, OnLocationPropertyChanged));

        /// <summary>
        /// LocationProperty property changed handler.
        /// </summary>
        /// <param name="d">Axis that changed its Location.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnLocationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Axis source = (Axis)d;
            AxisLocation oldValue = (AxisLocation)e.OldValue;
            AxisLocation newValue = (AxisLocation)e.NewValue;
            source.OnLocationPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// LocationProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>        
        protected virtual void OnLocationPropertyChanged(AxisLocation oldValue, AxisLocation newValue)
        {
            RoutedPropertyChangedEventHandler<AxisLocation> handler = this.LocationChanged;
            if (handler != null)
            {
                handler(this, new RoutedPropertyChangedEventArgs<AxisLocation>(oldValue, newValue));
            }
        }

        /// <summary>
        /// This event is raised when the location property is changed.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<AxisLocation> LocationChanged;

        #endregion public AxisLocation Location

        /// <summary>
        /// Gets the list of child axes belonging to this axis.
        /// </summary>
        public ObservableCollection<IAxis> DependentAxes { get; private set; }

        #region public AxisOrientation Orientation
        /// <summary>
        /// Gets or sets the orientation of the axis.
        /// </summary>
        public AxisOrientation Orientation
        {
            get { return (AxisOrientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(AxisOrientation),
                typeof(Axis),
                new PropertyMetadata(AxisOrientation.None, OnOrientationPropertyChanged));

        /// <summary>
        /// OrientationProperty property changed handler.
        /// </summary>
        /// <param name="d">Axis that changed its Orientation.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Axis source = (Axis)d;
            AxisOrientation oldValue = (AxisOrientation)e.OldValue;
            AxisOrientation newValue = (AxisOrientation)e.NewValue;
            source.OnOrientationPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// OrientationProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>        
        protected virtual void OnOrientationPropertyChanged(AxisOrientation oldValue, AxisOrientation newValue)
        {
            RoutedPropertyChangedEventHandler<AxisOrientation> handler = OrientationChanged;
            if (handler != null)
            {
                handler(this, new RoutedPropertyChangedEventArgs<AxisOrientation>(oldValue, newValue));
            }
        }

        /// <summary>
        /// This event is raised when the Orientation property is changed.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<AxisOrientation> OrientationChanged;

        #endregion public AxisOrientation Orientation

        /// <summary>
        /// Raises the invalidated event.
        /// </summary>
        /// <param name="args">Information about the event.</param>
        protected virtual void OnInvalidated(RoutedEventArgs args)
        {
            foreach (IAxisListener listener in RegisteredListeners)
            {
                listener.AxisInvalidated(this);
            }
        }

        /// <summary>
        /// Gets or the collection of series that are using the Axis.
        /// </summary>
        public ObservableCollection<IAxisListener> RegisteredListeners { get; private set; }

        /// <summary>
        /// Returns a value indicating whether the axis can plot a value.
        /// </summary>
        /// <param name="value">The value to plot.</param>
        /// <returns>A value indicating whether the axis can plot a value.
        /// </returns>
        public abstract bool CanPlot(object value);

        /// <summary>
        /// The plot area coordinate of a value.
        /// </summary>
        /// <param name="value">The value for which to retrieve the plot area
        /// coordinate.</param>
        /// <returns>The plot area coordinate.</returns>
        public abstract UnitValue GetPlotAreaCoordinate(object value);

        /// <summary>
        /// Instantiates a new instance of the Axis class.
        /// </summary>
        protected Axis()
        {
            RegisteredListeners = new UniqueObservableCollection<IAxisListener>();
            this.RegisteredListeners.CollectionChanged += RegisteredListenersCollectionChanged;
            this.DependentAxes = new ObservableCollection<IAxis>();
            this.DependentAxes.CollectionChanged += OnChildAxesCollectionChanged;
        }

        /// <summary>
        /// Child axes collection changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void OnChildAxesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnDependentAxesCollectionChanged();
        }

        /// <summary>
        /// Child axes collection changed.
        /// </summary>
        protected virtual void OnDependentAxesCollectionChanged()
        {
        }

        /// <summary>
        /// This event is raised when the registered listeners collection is
        /// changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void RegisteredListenersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (IAxisListener obj in e.OldItems)
                {
                    OnObjectUnregistered(obj);
                }
            }
            if (e.NewItems != null)
            {
                foreach (IAxisListener obj in e.NewItems)
                {
                    OnObjectRegistered(obj);
                }
            }
        }

        /// <summary>
        /// This method is invoked when a series is registered.
        /// </summary>
        /// <param name="series">The series that has been registered.</param>
        protected virtual void OnObjectRegistered(IAxisListener series)
        {
        }

        /// <summary>
        /// This method is invoked when a series is unregistered.
        /// </summary>
        /// <param name="series">The series that has been unregistered.</param>
        protected virtual void OnObjectUnregistered(IAxisListener series)
        {
        }
    }
}