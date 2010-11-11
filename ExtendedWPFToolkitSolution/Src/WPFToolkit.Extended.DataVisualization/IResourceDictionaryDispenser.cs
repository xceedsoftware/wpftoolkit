// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Represents a service that dispenses ResourceDictionaries.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public interface IResourceDictionaryDispenser
    {
        /// <summary>
        /// Returns a rotating enumerator of ResourceDictionaries coordinated with
        /// the style dispenser object to ensure that no two enumerators are
        /// currently on the same one if possible.  If the dispenser is reset or
        /// its collection is changed then the enumerators will also be reset.
        /// </summary>
        /// <param name="predicate">A predicate that returns a value
        /// indicating whether to return a ResourceDictionary.</param>
        /// <returns>An enumerator of ResourceDictionaries.</returns>
        IEnumerator<ResourceDictionary> GetResourceDictionariesWhere(Func<ResourceDictionary, bool> predicate);

        /// <summary>
        /// Event that is invoked when the StyleDispenser's ResourceDictionaries have changed.
        /// </summary>
        event EventHandler ResourceDictionariesChanged;
    }
}