// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// An object that synchronizes changes in an observable collection to 
    /// a list.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the collection.
    /// </typeparam>
    internal class ObservableCollectionListAdapter<T>
        where T : class
    {
        /// <summary>
        /// The collection to synchronize with a list.
        /// </summary>
        private IEnumerable _collection;

        /// <summary>
        /// Gets or sets the collection to synchronize with a list.
        /// </summary>
        public IEnumerable Collection
        {
            get
            {
                return _collection;
            }
            set
            {
                IEnumerable oldValue = _collection;
                INotifyCollectionChanged oldObservableCollection = oldValue as INotifyCollectionChanged;
                INotifyCollectionChanged newObservableCollection = value as INotifyCollectionChanged;
                _collection = value;

                if (oldObservableCollection != null)
                {
                    oldObservableCollection.CollectionChanged -= OnCollectionChanged;
                }

                if (value == null && TargetList != null)
                {
                    TargetList.Clear();
                }
                if (newObservableCollection != null)
                {
                    newObservableCollection.CollectionChanged += OnCollectionChanged;
                }
            }
        }

        /// <summary>
        /// Gets or sets the panel to synchronize with the collection.
        /// </summary>
        public IList TargetList { get; set; }

        /// <summary>
        /// Method that synchronizes the panel's child collection with the 
        /// contents of the observable collection when it changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        public void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (TargetList != null)
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    TargetList.Clear();
                }
                else if (e.Action == NotifyCollectionChangedAction.Replace)
                {
                    for (int cnt = 0; cnt < e.OldItems.Count; cnt++)
                    {
                        T oldItem = e.OldItems[cnt] as T;
                        T newItem = e.NewItems[cnt] as T;

                        int index = TargetList.IndexOf(oldItem);

                        if (index != -1)
                        {
                            TargetList[index] = newItem;
                        }
                        else
                        {
                            TargetList.Remove(oldItem);
                            TargetList.Add(newItem);
                        }
                    }
                }
                else
                {
                    if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
                    {
                        foreach (T element in e.OldItems)
                        {
                            TargetList.Remove(element);
                        }
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                    {
                        int startingIndex = e.NewStartingIndex;
                        if (startingIndex != 0)
                        {
                            T previousElement = Collection.FastElementAt<T>(startingIndex - 1);
                            int targetListIndex = TargetList.IndexOf(previousElement);
                            if (targetListIndex != -1)
                            {
                                startingIndex = targetListIndex + 1;
                            }
                        }
                        else if (Collection.FastCount() > 1)
                        {
                            T nextElement = Collection.FastElementAt<T>(startingIndex + 1);
                            int targetListIndex = TargetList.IndexOf(nextElement);
                            if (targetListIndex == -1)
                            {
                                startingIndex = 0;
                            }
                            else
                            {
                                startingIndex = targetListIndex;
                            }
                        }

                        e.NewItems
                            .OfType<T>()
                            .ForEachWithIndex((item, index) =>
                                {
                                    TargetList.Insert(startingIndex + index, item);
                                });
                    }
                }
            }
        }

        /// <summary>
        /// A method that populates a panel with the items in the collection.
        /// </summary>
        public void Populate()
        {
            if (TargetList != null)
            {
                if (Collection != null)
                {
                    foreach (T item in Collection)
                    {
                        TargetList.Add(item);
                    }
                }
                else
                {
                    TargetList.Clear();
                }
            }
        }

        /// <summary>
        /// Removes the items in the adapted list from the target list.
        /// </summary>
        public void ClearItems()
        {
            foreach (T item in this.Collection)
            {
                this.TargetList.Remove(item);
            }
        }
    }
}