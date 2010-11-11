// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// An enumerator that dispenses ResourceDictionaries sequentially by coordinating with
    /// related enumerators.  Enumerators are related through an association
    /// with a parent ResourceDictionaryDispenser class.
    /// </summary>
    internal class ResourceDictionaryEnumerator : IEnumerator<ResourceDictionary>
    {
        /// <summary>
        /// The index of current item in the ResourceDictionaryDispenser's list.
        /// </summary>
        private int? index;

        /// <summary>
        /// Gets or sets the current ResourceDictionary.
        /// </summary>
        private ResourceDictionary CurrentResourceDictionary { get; set; }

        /// <summary>
        /// The parent enumerator.
        /// </summary>
        private IEnumerator<ResourceDictionary> _parentEnumerator;

        /// <summary>
        /// Gets the parent enumerator.
        /// </summary>
        private IEnumerator<ResourceDictionary> ParentEnumerator
        {
            get
            {
                if (_parentEnumerator == null && ResourceDictionaryDispenser.Parent != null)
                {
                    _parentEnumerator = ResourceDictionaryDispenser.Parent.GetResourceDictionariesWhere(Predicate);
                }
                return _parentEnumerator;
            }
        }

        /// <summary>
        /// Initializes a new instance of a ResourceDictionaryEnumerator.
        /// </summary>
        /// <param name="dispenser">The dispenser that dispensed this
        /// ResourceDictionaryEnumerator.</param>
        /// <param name="predicate">A predicate used to determine which
        /// ResourceDictionaries to return.</param>
        public ResourceDictionaryEnumerator(ResourceDictionaryDispenser dispenser, Func<ResourceDictionary, bool> predicate)
        {
            ResourceDictionaryDispenser = dispenser;
            Predicate = predicate;
        }

        /// <summary>
        /// Called when the parent has changed.
        /// </summary>
        internal void ResourceDictionaryDispenserParentChanged()
        {
            _parentEnumerator = null;
        }

        /// <summary>
        /// Returns the index of the next suitable style in the list.
        /// </summary>
        /// <param name="startIndex">The index at which to start looking.</param>
        /// <returns>The index of the next suitable ResourceDictionary.</returns>
        private int? GetIndexOfNextSuitableResourceDictionary(int startIndex)
        {
            if (ResourceDictionaryDispenser.ResourceDictionaries == null || ResourceDictionaryDispenser.ResourceDictionaries.Count == 0)
            {
                return new int?();
            }

            if (startIndex >= ResourceDictionaryDispenser.ResourceDictionaries.Count)
            {
                startIndex = 0;
            }

            int counter = startIndex;
            do
            {
                if (Predicate(ResourceDictionaryDispenser.ResourceDictionaries[counter]))
                {
                    return counter;
                }

                counter = (counter + 1) % ResourceDictionaryDispenser.ResourceDictionaries.Count;
            }
            while (startIndex != counter);

            return new int?();
        }

        /// <summary>
        /// Resets the dispenser.
        /// </summary>
        internal void ResourceDictionaryDispenserResetting()
        {
            if (!ShouldRetrieveFromParentEnumerator)
            {
                index = new int?();
            }
        }

        /// <summary>
        /// Gets or sets a predicate that returns a value indicating whether a 
        /// ResourceDictionary should be returned by this enumerator.
        /// </summary>
        /// <returns>A value indicating whether a ResourceDictionary can be returned by this
        /// enumerator.</returns>
        private Func<ResourceDictionary, bool> Predicate { get; set; }

        /// <summary>
        /// This method is invoked when one of the related enumerator's
        /// dispenses.  The enumerator checks to see if the item
        /// dispensed would've been the next item it would have returned.  If
        /// so it updates it's index to the position after the previously
        /// returned item.
        /// </summary>
        /// <param name="sender">The ResourceDictionaryDispenser.</param>
        /// <param name="e">Information about the event.</param>
        internal void ResourceDictionaryDispenserResourceDictionaryDispensed(object sender, ResourceDictionaryDispensedEventArgs e)
        {
            if (!ShouldRetrieveFromParentEnumerator && Predicate(e.ResourceDictionary))
            {
                int? nextStyleIndex = GetIndexOfNextSuitableResourceDictionary(index ?? 0);
                if ((nextStyleIndex ?? -1) == e.Index)
                {
                    index = (e.Index + 1) % ResourceDictionaryDispenser.ResourceDictionaries.Count;
                }
            }
        }

        /// <summary>
        /// Raises the EnumeratorResourceDictionaryDispensed.
        /// </summary>
        /// <param name="args">Information about the ResourceDictionary dispensed.</param>
        protected virtual void OnStyleDispensed(ResourceDictionaryDispensedEventArgs args)
        {
            ResourceDictionaryDispenser.EnumeratorResourceDictionaryDispensed(this, args);
        }

        /// <summary>
        /// Gets the dispenser that dispensed this enumerator.
        /// </summary>
        public ResourceDictionaryDispenser ResourceDictionaryDispenser { get; private set; }

        /// <summary>
        /// Gets the current ResourceDictionary.
        /// </summary>
        public ResourceDictionary Current
        {
            get { return CurrentResourceDictionary; }
        }

        /// <summary>
        /// Gets the current ResourceDictionary.
        /// </summary>
        object System.Collections.IEnumerator.Current
        {
            get { return CurrentResourceDictionary; }
        }

        /// <summary>
        /// Moves to the next ResourceDictionary.
        /// </summary>
        /// <returns>A value indicating whether there are any more suitable
        /// ResourceDictionary.</returns>
        public bool MoveNext()
        {
            if (ShouldRetrieveFromParentEnumerator && ParentEnumerator != null)
            {
                bool isMore = ParentEnumerator.MoveNext();
                if (isMore)
                {
                    this.CurrentResourceDictionary = ParentEnumerator.Current;
                }
                return isMore;
            }

            index = GetIndexOfNextSuitableResourceDictionary(index ?? 0);
            if (index == null)
            {
                CurrentResourceDictionary = null;
                Dispose();
                return false;
            }
            
            CurrentResourceDictionary = ResourceDictionaryDispenser.ResourceDictionaries[index.Value];
            OnStyleDispensed(new ResourceDictionaryDispensedEventArgs(index.Value, CurrentResourceDictionary));

            return true;
        }

        /// <summary>
        /// Gets a value indicating whether a enumerator should return ResourceDictionaries
        /// from its parent enumerator.
        /// </summary>
        private bool ShouldRetrieveFromParentEnumerator
        {
            get { return this.ResourceDictionaryDispenser.ResourceDictionaries == null; }
        }

        /// <summary>
        /// Resets the enumerator.
        /// </summary>
        public void Reset()
        {
            throw new NotSupportedException(Properties.Resources.ResourceDictionaryEnumerator_CantResetEnumeratorResetDispenserInstead);
        }

        /// <summary>
        /// Stops listening to the dispenser.
        /// </summary>
        public void Dispose()
        {
            if (_parentEnumerator != null)
            {
                _parentEnumerator.Dispose();
            }

            this.ResourceDictionaryDispenser.Unregister(this);
            GC.SuppressFinalize(this);
        }
    }
}