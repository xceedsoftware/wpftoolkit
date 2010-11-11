// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// An observable collection that cannot be reset.  When clear is called
    /// items are removed individually, giving listeners the chance to detect
    /// each remove event and perform operations such as unhooking event 
    /// handlers.
    /// </summary>
    /// <typeparam name="T">The type of item in the collection.</typeparam>
    internal class NoResetObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Instantiates a new instance of the NoResetObservableCollection 
        /// class.
        /// </summary>
        public NoResetObservableCollection()
        {
        }

        /// <summary>
        /// Clears all items in the collection by removing them individually.
        /// </summary>
        protected override void ClearItems()
        {
            IList<T> items = new List<T>(this);
            foreach (T item in items)
            {
                Remove(item);
            }
        }
    }
}