// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Windows.Controls.DataVisualization.Collections
{
    /// <summary>
    /// Implements a dictionary that can store multiple values for the same key.
    /// </summary>
    /// <typeparam name="TKey">Type for keys.</typeparam>
    /// <typeparam name="TValue">Type for values.</typeparam>
    internal class MultipleDictionary<TKey, TValue>
    {
        /// <summary>
        /// Gets or sets the BinaryTree instance used to store the dictionary values.
        /// </summary>
        protected LeftLeaningRedBlackTree<TKey, TValue> BinaryTree { get; set; }

        /// <summary>
        /// Initializes a new instance of the MultipleDictionary class.
        /// </summary>
        protected MultipleDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the MultipleDictionary class.
        /// </summary>
        /// <param name="allowDuplicateValues">The parameter is not used.</param>
        /// <param name="keyEqualityComparer">The parameter is not used.</param>
        /// <param name="valueEqualityComparer">The parameter is not used.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "allowDuplicateValues", Justification = "Unused parameter exists for API compatibility.")]
        public MultipleDictionary(bool allowDuplicateValues, IEqualityComparer<TKey> keyEqualityComparer, IEqualityComparer<TValue> valueEqualityComparer)
        {
            Debug.Assert(null != keyEqualityComparer, "keyEqualityComparer must not be null.");
            Debug.Assert(null != valueEqualityComparer, "valueEqualityComparer must not be null.");
            BinaryTree = new LeftLeaningRedBlackTree<TKey, TValue>(
                (left, right) => keyEqualityComparer.GetHashCode(left).CompareTo(keyEqualityComparer.GetHashCode(right)),
                (left, right) => valueEqualityComparer.GetHashCode(left).CompareTo(valueEqualityComparer.GetHashCode(right)));
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary.
        /// </summary>
        /// <param name="key">Key to add.</param>
        /// <param name="value">Value to add.</param>
        public void Add(TKey key, TValue value)
        {
            BinaryTree.Add(key, value);
        }

        /// <summary>
        /// Removes a key/value pair from the dictionary.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        /// <param name="value">Value to remove.</param>
        /// <returns>True if the value was present and removed.</returns>
        public bool Remove(TKey key, TValue value)
        {
            return BinaryTree.Remove(key, value);
        }

        /// <summary>
        /// Gets the count of values in the dictionary.
        /// </summary>
        public int Count
        {
            get
            {
                return BinaryTree.Count;
            }
        }

        /// <summary>
        /// Returns the collection of values corresponding to a key.
        /// </summary>
        /// <param name="key">Specified key.</param>
        /// <returns>Collection of values.</returns>
        public ICollection<TValue> this[TKey key]
        {
            get
            {
                return BinaryTree.GetValuesForKey(key).ToList();
            }
        }

        /// <summary>
        /// Clears the items in the dictionary.
        /// </summary>
        public void Clear()
        {
            BinaryTree.Clear();
        }
    }
}