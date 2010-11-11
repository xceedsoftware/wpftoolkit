// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Globalization;
using System.Windows.Data;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Converts a value to a string using a format string.
    /// </summary>
    public class StringFormatConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value to a string by formatting it.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="parameter">The format string.</param>
        /// <param name="culture">The culture to use for conversion.</param>
        /// <returns>The formatted string.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return string.Format(CultureInfo.CurrentCulture, (parameter as string) ?? "{0}", value);
        }

        /// <summary>
        /// Converts a value from a string to a target type.
        /// </summary>
        /// <param name="value">The value to convert to a string.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="parameter">A parameter used during the conversion
        /// process.</param>
        /// <param name="culture">The culture to use for the conversion.</param>
        /// <returns>The converted object.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}