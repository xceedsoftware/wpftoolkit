// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// A class that rotates through a list of ResourceDictionaries.
    /// </summary>
    internal class ResourceDictionaryDispenser : IResourceDictionaryDispenser
    {
        /// <summary>
        /// A linked list of ResourceDictionaries dispensed.
        /// </summary>
        private LinkedList<ResourceDictionaryDispensedEventArgs> _resourceDictionariesDispensed = new LinkedList<ResourceDictionaryDispensedEventArgs>();

        /// <summary>
        /// A bag of weak references to connected style enumerators.
        /// </summary>
        private WeakReferenceBag<ResourceDictionaryEnumerator> _resourceDictionaryEnumerators = new WeakReferenceBag<ResourceDictionaryEnumerator>();

        /// <summary>
        /// Value indicating whether to ignore that the enumerator has 
        /// dispensed a ResourceDictionary.
        /// </summary>
        private bool _ignoreResourceDictionaryDispensedByEnumerator;

        /// <summary>
        /// The list of ResourceDictionaries of rotate.
        /// </summary>
        private IList<ResourceDictionary> _resourceDictionaries;

        /// <summary>
        /// Gets or sets the list of ResourceDictionaries to rotate.
        /// </summary>
        public IList<ResourceDictionary> ResourceDictionaries
        {
            get
            {
                return _resourceDictionaries;
            }
            set
            {
                if (value != _resourceDictionaries)
                {
                    {
                        INotifyCollectionChanged notifyCollectionChanged = _resourceDictionaries as INotifyCollectionChanged;
                        if (notifyCollectionChanged != null)
                        {
                            notifyCollectionChanged.CollectionChanged -= ResourceDictionariesCollectionChanged;
                        }
                    }
                    _resourceDictionaries = value;
                    {
                        INotifyCollectionChanged notifyCollectionChanged = _resourceDictionaries as INotifyCollectionChanged;
                        if (notifyCollectionChanged != null)
                        {
                            notifyCollectionChanged.CollectionChanged += ResourceDictionariesCollectionChanged;
                        }
                    }

                    Reset();
                }
            }
        }

        /// <summary>
        /// This method is raised when the ResourceDictionaries collection is changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void ResourceDictionariesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(e.Action == NotifyCollectionChangedAction.Add && (this.ResourceDictionaries.Count - e.NewItems.Count) == e.NewStartingIndex))
            {
                Reset();
            }
        }

        /// <summary>
        /// The parent of the ResourceDictionaryDispenser.
        /// </summary>
        private IResourceDictionaryDispenser _parent;

        /// <summary>
        /// Event that is invoked when the ResourceDictionaryDispenser's contents have changed.
        /// </summary>
        public event EventHandler ResourceDictionariesChanged;

        /// <summary>
        /// Gets or sets the parent of the ResourceDictionaryDispenser.
        /// </summary>
        public IResourceDictionaryDispenser Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (_parent != value)
                {
                    if (null != _parent)
                    {
                        _parent.ResourceDictionariesChanged -= new EventHandler(ParentResourceDictionariesChanged);
                    }
                    _parent = value;
                    if (null != _parent)
                    {
                        _parent.ResourceDictionariesChanged += new EventHandler(ParentResourceDictionariesChanged);
                    }
                    OnParentChanged();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the ResourceDictionaryDispenser class.
        /// </summary>
        public ResourceDictionaryDispenser()
        {
        }

        /// <summary>
        /// Resets the state of the ResourceDictionaryDispenser and its enumerators.
        /// </summary>
        private void Reset()
        {
            OnResetting();

            // Invoke event
            EventHandler handler = ResourceDictionariesChanged;
            if (null != handler)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Unregisters an enumerator so that it can be garbage collected.
        /// </summary>
        /// <param name="enumerator">The enumerator.</param>
        internal void Unregister(ResourceDictionaryEnumerator enumerator)
        {
            _resourceDictionaryEnumerators.Remove(enumerator);
        }

        /// <summary>
        /// Returns a rotating enumerator of ResourceDictionary objects that coordinates
        /// with the dispenser object to ensure that no two enumerators are on the same
        /// item. If the dispenser is reset or its collection is changed then the
        /// enumerators are also reset.
        /// </summary>
        /// <param name="predicate">A predicate that returns a value indicating
        /// whether to return an item.</param>
        /// <returns>An enumerator of ResourceDictionaries.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Returning a usable enumerator instance.")]
        public IEnumerator<ResourceDictionary> GetResourceDictionariesWhere(Func<ResourceDictionary, bool> predicate)
        {
            ResourceDictionaryEnumerator enumerator = new ResourceDictionaryEnumerator(this, predicate);

            _ignoreResourceDictionaryDispensedByEnumerator = true;
            try
            {
                foreach (ResourceDictionaryDispensedEventArgs args in _resourceDictionariesDispensed)
                {
                    enumerator.ResourceDictionaryDispenserResourceDictionaryDispensed(this, args);
                }
            }
            finally
            {
                _ignoreResourceDictionaryDispensedByEnumerator = false;
            }

            _resourceDictionaryEnumerators.Add(enumerator);
            return enumerator;
        }

        /// <summary>
        /// This method is raised when an enumerator dispenses a ResourceDictionary.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        internal void EnumeratorResourceDictionaryDispensed(object sender, ResourceDictionaryDispensedEventArgs e)
        {
            if (!_ignoreResourceDictionaryDispensedByEnumerator)
            {
                OnEnumeratorResourceDictionaryDispensed(this, e);
            }
        }

        /// <summary>
        /// Raises the ParentChanged event.
        /// </summary>
        private void OnParentChanged()
        {
            foreach (ResourceDictionaryEnumerator enumerator in _resourceDictionaryEnumerators)
            {
                enumerator.ResourceDictionaryDispenserParentChanged();
            }
        }

        /// <summary>
        /// Raises the EnumeratorResourceDictionaryDispensed event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">Information about the event.</param>
        private void OnEnumeratorResourceDictionaryDispensed(object source, ResourceDictionaryDispensedEventArgs args)
        {
            // Remove this item from the list of dispensed styles.
            _resourceDictionariesDispensed.Remove(args);

            // Add this item to the end of the list of dispensed styles.
            _resourceDictionariesDispensed.AddLast(args);

            foreach (ResourceDictionaryEnumerator enumerator in _resourceDictionaryEnumerators)
            {
                enumerator.ResourceDictionaryDispenserResourceDictionaryDispensed(source, args);
            }
        }

        /// <summary>
        /// This method raises the EnumeratorsResetting event.
        /// </summary>
        private void OnResetting()
        {
            _resourceDictionariesDispensed.Clear();

            foreach (ResourceDictionaryEnumerator enumerator in _resourceDictionaryEnumerators)
            {
                enumerator.ResourceDictionaryDispenserResetting();
            }
        }

        /// <summary>
        /// Handles the Parent's ResourceDictionariesChanged event.
        /// </summary>
        /// <param name="sender">Parent instance.</param>
        /// <param name="e">Event args.</param>
        private void ParentResourceDictionariesChanged(object sender, EventArgs e)
        {
            Reset();
        }
    }
}
