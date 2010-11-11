// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Windows.Controls.DataVisualization.Collections
{
    /// <summary>
    /// Implements a dictionary that can store multiple values for the same key and sorts the values.
    /// </summary>
    /// <typeparam name="TKey">Type for keys.</typeparam>
    /// <typeparam name="TValue">Type for values.</typeparam>
    internal class OrderedMultipleDictionary<TKey, TValue> : MultipleDictionary<TKey, TValue>, IEnumerable<TValue>
        where TKey : IComparable
    {
        /// <summary>
        /// Initializes a new instance of the MultipleDictionary class.
        /// </summary>
        /// <param name="allowDuplicateValues">The parameter is not used.</param>
        /// <param name="keyComparison">Key comparison class.</param>
        /// <param name="valueComparison">Value comparison class.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "allowDuplicateValues", Justification = "Unused parameter exists for API compatibility.")]
        public OrderedMultipleDictionary(bool allowDuplicateValues, Comparison<TKey> keyComparison, Comparison<TValue> valueComparison)
        {
            Debug.Assert(null != keyComparison, "keyComparison must not be null.");
            Debug.Assert(null != valueComparison, "valueComparison must not be null.");
            BinaryTree = new LeftLeaningRedBlackTree<TKey, TValue>(keyComparison, valueComparison);
        }

        /// <summary>
        /// Gets a Range corresponding to the keys in the dictionary.
        /// </summary>
        /// <returns>Range of keys.</returns>
        public Range<TKey> GetKeyRange()
        {
            if (0 < BinaryTree.Count)
            {
                return new Range<TKey>(BinaryTree.MinimumKey, BinaryTree.MaximumKey);
            }
            else
            {
                return new Range<TKey>();
            }
        }

        /// <summary>
        /// Gets the largest and smallest key's extreme values from the dictionary.
        /// </summary>
        /// <returns>Tuple of the largest and smallest values.</returns>
        public Tuple<TValue, TValue> GetLargestAndSmallestValues()
        {
            if (0 < BinaryTree.Count)
            {
                return new Tuple<TValue, TValue>(BinaryTree.MinimumValue, BinaryTree.MaximumValue);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets an enumerator for the values in the dictionary.
        /// </summary>
        /// <returns>Enumerator for values.</returns>
        public IEnumerator<TValue> GetEnumerator()
        {
            return BinaryTree.GetValuesForAllKeys().GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerator for the values in the dictionary.
        /// </summary>
        /// <returns>Enumerator for the values.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return BinaryTree.GetValuesForAllKeys().GetEnumerator();
        }
    }
}