// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// A generic equality comparer.
    /// </summary>
    /// <typeparam name="T">The type of the objects being compared.</typeparam>
    internal class GenericEqualityComparer<T> : EqualityComparer<T>
    {
        /// <summary>
        /// Gets or sets a function which determines whether two items are equal.
        /// </summary>
        public Func<T, T, bool> EqualityFunction { get; set; }

        /// <summary>
        /// Gets or sets a function that returns a hash code for an object.
        /// </summary>
        public Func<T, int> HashCodeFunction { get; set; }

        /// <summary>
        /// Initializes a new instance of the GenericEqualityComparer class.
        /// </summary>
        /// <param name="equalityFunction">A function which determines whether 
        /// two items are equal.</param>
        /// <param name="hashCodeFunction">A function that returns a hash code 
        /// for an object.</param>
        public GenericEqualityComparer(Func<T, T, bool> equalityFunction, Func<T, int> hashCodeFunction)
        {
            this.EqualityFunction = equalityFunction;
            this.HashCodeFunction = hashCodeFunction;
        }

        /// <summary>
        /// A function which determines whether two items are equal.
        /// </summary>
        /// <param name="x">The left object.</param>
        /// <param name="y">The right object.</param>
        /// <returns>A value indicating whether the objects. are equal.</returns>
        public override bool Equals(T x, T y)
        {
            return EqualityFunction(x, y);
        }

        /// <summary>
        /// A function that returns a hash code for an object.
        /// </summary>
        /// <param name="obj">The object to returns a hash code for.</param>
        /// <returns>The hash code for the object.</returns>
        public override int GetHashCode(T obj)
        {
            return HashCodeFunction(obj);
        }
    }
}