// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization
{
#if SILVERLIGHT
    /// <summary>
    /// Extension methods for the ResourceDictionary class.
    /// </summary>
    public static class ResourceDictionaryExtensions
    {
        /// <summary>
        /// Makes a shallow copy of the specified ResourceDictionary.
        /// </summary>
        /// <param name="dictionary">ResourceDictionary to copy.</param>
        /// <returns>Copied ResourceDictionary.</returns>
        public static ResourceDictionary ShallowCopy(this ResourceDictionary dictionary)
        {
            ResourceDictionary clone = new ResourceDictionary();
            foreach (object key in dictionary.Keys)
            {
                clone.Add(key, dictionary[key]);
            }
            return clone;
        }
    }
#endif
}