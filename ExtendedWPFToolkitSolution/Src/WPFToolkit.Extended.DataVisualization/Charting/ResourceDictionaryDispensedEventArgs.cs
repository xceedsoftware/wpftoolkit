// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Information describing the ResourceDictionary dispensed when a
    /// ResourceDictionaryDispensed event is raised.
    /// </summary>
    internal class ResourceDictionaryDispensedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the ResourceDictionaryDispensedEventArgs class.
        /// </summary>
        /// <param name="index">The index of the ResourceDictionary dispensed.</param>
        /// <param name="resourceDictionary">The ResourceDictionary dispensed.</param>
        public ResourceDictionaryDispensedEventArgs(int index, ResourceDictionary resourceDictionary)
        {
            this.ResourceDictionary = resourceDictionary;
            this.Index = index;
        }

        /// <summary>
        /// Gets the index of the ResourceDictionary dispensed.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the ResourceDictionary dispensed.
        /// </summary>
        public ResourceDictionary ResourceDictionary { get; private set; }

        /// <summary>
        /// Returns a value indicating whether two objects are equal.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>
        /// A value indicating whether the two objects are equal.
        /// </returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}