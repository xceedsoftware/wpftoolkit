// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// An axes collection used by a series host.
    /// </summary>
    internal class SeriesHostAxesCollection : UniqueObservableCollection<IAxis>
    {
        /// <summary>
        /// Gets or sets the series host field.
        /// </summary>
        private ISeriesHost SeriesHost { get; set; }

        /// <summary>
        /// Gets or sets a collection of axes cannot be removed under any 
        /// circumstances.
        /// </summary>
        private UniqueObservableCollection<IAxis> PersistentAxes { get; set; }

        /// <summary>
        /// Instantiates a new instance of the SeriesHostAxesCollection class.
        /// </summary>
        /// <param name="seriesHost">The series host.</param>
        internal SeriesHostAxesCollection(ISeriesHost seriesHost)
        {
            this.SeriesHost = seriesHost;
            this.PersistentAxes = new UniqueObservableCollection<IAxis>();
            this.CollectionChanged += ThisCollectionChanged;
        }

        /// <summary>
        /// Instantiates a new instance of the SeriesHostAxesCollection class.
        /// </summary>
        /// <param name="seriesHost">The series host.</param>
        /// <param name="persistentAxes">A collection of axes that can never be 
        /// removed from the chart.</param>
        internal SeriesHostAxesCollection(ISeriesHost seriesHost, UniqueObservableCollection<IAxis> persistentAxes)
            : this(seriesHost)
        {
            Debug.Assert(persistentAxes != null, "Persistent axes collection cannot be null.");
            this.SeriesHost = seriesHost;
            this.PersistentAxes = persistentAxes;
            this.PersistentAxes.CollectionChanged += PersistentAxesCollectionChanged;
        }

        /// <summary>
        /// A method that attaches and removes listeners to axes added to this
        /// collection.
        /// </summary>
        /// <param name="sender">This object.</param>
        /// <param name="e">Information about the event.</param>
        private void ThisCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (IAxis axis in e.NewItems)
                {
                    axis.RegisteredListeners.CollectionChanged += AxisRegisteredListenersCollectionChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (IAxis axis in e.OldItems)
                {
                    axis.RegisteredListeners.CollectionChanged -= AxisRegisteredListenersCollectionChanged;
                }
            }
        }

        /// <summary>
        /// Remove an axis from the collection if it is no longer used.
        /// </summary>
        /// <param name="sender">The axis that has had its registered 
        /// listeners collection changed.</param>
        /// <param name="e">Information about the event.</param>
        private void AxisRegisteredListenersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IAxis axis = this.Where(currentAxis => currentAxis.RegisteredListeners == sender).First();

            if (e.OldItems != null)
            {
                if (!PersistentAxes.Contains(axis) && !SeriesHost.IsUsedByASeries(axis))
                {
                    this.Remove(axis);
                }
            }
        }

        /// <summary>
        /// This method synchronizes the collection with the persistent axes 
        /// collection when it is changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        public void PersistentAxesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (IAxis axis in e.NewItems)
                {
                    if (!this.Contains(axis))
                    {
                        this.Add(axis);
                    }
                }
            }
            if (e.OldItems != null)
            {
                foreach (IAxis axis in e.OldItems)
                {
                    if (this.Contains(axis) && !SeriesHost.IsUsedByASeries(axis))
                    {
                        this.Remove(axis);
                    }
                }
            }
        }

        /// <summary>
        /// Removes an item from the axes collection but throws an exception
        /// if a series in the series host is listening to it.
        /// </summary>
        /// <param name="index">The index of the item being removed.</param>
        protected override void RemoveItem(int index)
        {
            IAxis axis = this[index];

            if (SeriesHost.IsUsedByASeries(axis))
            {
                throw new InvalidOperationException(Properties.Resources.SeriesHostAxesCollection_RemoveItem_AxisCannotBeRemovedFromASeriesHostWhenOneOrMoreSeriesAreListeningToIt);
            }
            else if (PersistentAxes.Contains(axis))
            {
                throw new InvalidOperationException(Properties.Resources.SeriesHostAxesCollection_InvalidAttemptToRemovePermanentAxisFromSeriesHost);
            }
            else
            {
                base.RemoveItem(index);
            }
        }
    }
}