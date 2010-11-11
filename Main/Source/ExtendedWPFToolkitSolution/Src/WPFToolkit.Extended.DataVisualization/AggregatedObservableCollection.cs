// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Aggregated observable collection.
    /// </summary>
    /// <typeparam name="T">The type of the items in the observable collections.
    /// </typeparam>
    internal class AggregatedObservableCollection<T> : ReadOnlyObservableCollection<T>
    {
        /// <summary>
        /// Initializes a new instance of an aggregated observable collection.
        /// </summary>
        public AggregatedObservableCollection()
        {
            this.ChildCollections = new NoResetObservableCollection<IList>();
            this.ChildCollections.CollectionChanged += new NotifyCollectionChangedEventHandler(ChildCollectionsCollectionChanged);
        }

        /// <summary>
        /// Rebuilds the list if a collection changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void ChildCollectionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(e.Action != NotifyCollectionChangedAction.Reset, "Reset is not supported.");

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                e.NewItems
                    .OfType<IList>()
                    .ForEachWithIndex((newCollection, index) =>
                    {
                        int startingIndex = GetStartingIndexOfCollectionAtIndex(e.NewStartingIndex + index);
                        foreach (T item in newCollection.OfType<T>().Reverse())
                        {
                            this.Mutate(items => items.Insert(startingIndex, item));
                        }

                        INotifyCollectionChanged notifyCollectionChanged = newCollection as INotifyCollectionChanged;
                        if (notifyCollectionChanged != null)
                        {
                            notifyCollectionChanged.CollectionChanged += ChildCollectionCollectionChanged;
                        }
                    });
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (IList oldCollection in e.OldItems)
                {
                    INotifyCollectionChanged notifyCollectionChanged = oldCollection as INotifyCollectionChanged;
                    if (notifyCollectionChanged != null)
                    {
                        notifyCollectionChanged.CollectionChanged -= ChildCollectionCollectionChanged;
                    }

                    foreach (T item in oldCollection)
                    {
                        this.Mutate(items => items.Remove(item));
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (IList oldCollection in e.OldItems)
                {
                    INotifyCollectionChanged notifyCollectionChanged = oldCollection as INotifyCollectionChanged;
                    if (notifyCollectionChanged != null)
                    {
                        notifyCollectionChanged.CollectionChanged -= ChildCollectionCollectionChanged;
                    }
                }

                foreach (IList newCollection in e.NewItems)
                {
                    INotifyCollectionChanged notifyCollectionChanged = newCollection as INotifyCollectionChanged;
                    if (notifyCollectionChanged != null)
                    {
                        notifyCollectionChanged.CollectionChanged += ChildCollectionCollectionChanged;
                    }
                }

                Rebuild();
            }
        }

        /// <summary>
        /// Synchronizes the collection with changes made in a child collection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void ChildCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(e.Action != NotifyCollectionChangedAction.Reset, "Reset is not supported.");
            IList collectionSender = sender as IList;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                int startingIndex = GetStartingIndexOfCollectionAtIndex(ChildCollections.IndexOf(collectionSender));
                e.NewItems
                    .OfType<T>()
                    .ForEachWithIndex((item, index) =>
                        {
                            this.Mutate(that => that.Insert(startingIndex + e.NewStartingIndex + index, item));
                        });
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (T item in e.OldItems.OfType<T>())
                {
                    this.Mutate(that => that.Remove(item));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                for (int cnt = 0; cnt < e.NewItems.Count; cnt++)
                {
                    T oldItem = (T)e.OldItems[cnt];
                    T newItem = (T)e.NewItems[cnt];
                    int oldItemIndex = this.IndexOf(oldItem);
                    this.Mutate((that) =>
                        {
                            that[oldItemIndex] = newItem;
                        });
                }
            }
        }

        /// <summary>
        /// Returns the starting index of a collection in the aggregate
        /// collection.
        /// </summary>
        /// <param name="index">The starting index of a collection.</param>
        /// <returns>The starting index of the collection in the aggregate 
        /// collection.</returns>
        private int GetStartingIndexOfCollectionAtIndex(int index)
        {
            return ChildCollections.OfType<IEnumerable>().Select(collection => collection.CastWrapper<T>()).Take(index).SelectMany(collection => collection).Count();
        }

        /// <summary>
        /// Rebuild the list in the correct order when a child collection 
        /// changes.
        /// </summary>
        private void Rebuild()
        {
            this.Mutate(that => that.Clear());
            this.Mutate(that =>
                {
                    IList<T> items = ChildCollections.OfType<IEnumerable>().Select(collection => collection.CastWrapper<T>()).SelectMany(collection => collection).ToList();
                    foreach (T item in items)
                    {
                        that.Add(item);
                    }
                });
        }
        
        /// <summary>
        /// Gets child collections of the aggregated collection.
        /// </summary>
        public ObservableCollection<IList> ChildCollections { get; private set; }
    }
}