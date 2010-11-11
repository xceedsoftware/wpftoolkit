// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.ObjectModel;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// An observable collection that can only be written to by internal 
    /// classes.
    /// </summary>
    /// <typeparam name="T">The type of object in the observable collection.
    /// </typeparam>
    internal class ReadOnlyObservableCollection<T> : NoResetObservableCollection<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the owner is writing to the 
        /// collection.
        /// </summary>
        private bool IsMutating { get; set; }

        /// <summary>
        /// A method that mutates the collection.
        /// </summary>
        /// <param name="action">The action to mutate the collection.</param>
        public void Mutate(Action<ReadOnlyObservableCollection<T>> action)
        {
            IsMutating = true;
            try
            {
                action(this);
            }
            finally
            {
                IsMutating = false;
            }
        }

        /// <summary>
        /// Removes an item from the collection at an index.
        /// </summary>
        /// <param name="index">The index to remove.</param>
        protected override void RemoveItem(int index)
        {
            if (!IsMutating)
            {
                throw new NotSupportedException(Properties.Resources.ReadOnlyObservableCollection_CollectionIsReadOnly);
            }
            else
            {
                base.RemoveItem(index);
            }
        }

        /// <summary>
        /// Sets an item at a particular location in the collection.
        /// </summary>
        /// <param name="index">The location to set an item.</param>
        /// <param name="item">The item to set.</param>
        protected override void SetItem(int index, T item)
        {
            if (!IsMutating)
            {
                throw new NotSupportedException(Properties.Resources.ReadOnlyObservableCollection_CollectionIsReadOnly);
            }
            else
            {
                base.SetItem(index, item);
            }
        }

        /// <summary>
        /// Inserts an item in the collection.
        /// </summary>
        /// <param name="index">The index at which to insert the item.</param>
        /// <param name="item">The item to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            if (!IsMutating)
            {
                throw new NotSupportedException(Properties.Resources.ReadOnlyObservableCollection_CollectionIsReadOnly);
            }
            else
            {
                base.InsertItem(index, item);
            }
        }

        /// <summary>
        /// Clears the items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            if (!IsMutating)
            {
                throw new NotSupportedException(Properties.Resources.ReadOnlyObservableCollection_CollectionIsReadOnly);
            }
            else
            {
                base.ClearItems();
            }
        }
    }
}