// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Globalization;
using System.Windows.Data;

namespace System.Windows.Controls.DataVisualization.Charting.Compatible
{
    /// <summary>
    /// Converts from a true/false value indicating whether selection is enabled to a SeriesSelectionMode.
    /// </summary>
    internal class SelectionEnabledToSelectionModeConverter : IValueConverter
    {
        /// <summary>
        /// Initializes a new instance of the SelectionEnabledToSelectionModeConverter class.
        /// </summary>
        public SelectionEnabledToSelectionModeConverter()
        {
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Converted value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SeriesSelectionMode selectionMode = SeriesSelectionMode.None;
            if ((value is bool) && (bool)value)
            {
                selectionMode = SeriesSelectionMode.Single;
            }
            return selectionMode;
        }

        /// <summary>
        /// Converts a value back.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Converted value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
