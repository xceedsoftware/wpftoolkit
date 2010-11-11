// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ResxResources = System.Windows.Controls.DataVisualization.Properties.Resources;
using System.ComponentModel;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Converts a string or base value to a <see cref="Nullable"/> value.
    /// </summary>
    /// <typeparam name="T">The type should be value type.</typeparam>
    /// <QualityBand>Preview</QualityBand>
    public class NullableConverter<T> : TypeConverter where T : struct
    {
        /// <summary>
        /// Returns whether the type converter can convert an object from the 
        /// specified type to the type of this converter.
        /// </summary>
        /// <param name="context">An object that provides a format context.
        /// </param>
        /// <param name="sourceType">The type you want to convert from.</param>
        /// <returns>
        /// Returns true if this converter can perform the conversion; 
        /// otherwise, false.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(T))
            {
                return true;
            }
            else if (sourceType == typeof(string))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether the type converter can convert an object from the 
        /// specified type to the type of this converter.
        /// </summary>
        /// <param name="context">An object that provides a format context.
        /// </param>
        /// <param name="destinationType">The type you want to convert to.
        /// </param>
        /// <returns>
        /// Returns true if this converter can perform the conversion; 
        /// otherwise, false.
        /// </returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(T));
        }

        /// <summary>
        /// Converts from the specified value to the type of this converter.
        /// </summary>
        /// <param name="context">An object that provides a format context.
        /// </param>
        /// <param name="culture">The 
        /// <see cref="T:System.Globalization.CultureInfo"/> to use as the 
        /// current culture.</param>
        /// <param name="value">The value to convert to the type of this 
        /// converter.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="T:System.NotSupportedException">
        /// The conversion cannot be performed.
        /// </exception>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string stringValue = value as string;
            if (value is T)
            {
                return new Nullable<T>((T)value);
            }
            else if (string.IsNullOrEmpty(stringValue) || String.Equals(stringValue, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                return new Nullable<T>();
            }
            return new Nullable<T>((T)Convert.ChangeType(value, typeof(T), culture));
        }

        /// <summary>
        /// Converts from the specified value to the a specified type from the
        /// type of this converter.
        /// </summary>
        /// <param name="context">An object that provides a format context.
        /// </param>
        /// <param name="culture">The 
        /// <see cref="T:System.Globalization.CultureInfo"/> to use as the 
        /// current culture.</param>
        /// <param name="value">The value to convert to the type of this 
        /// converter.</param>
        /// <param name="destinationType">The type of convert the value to
        /// .</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="T:System.NotSupportedException">
        /// The conversion cannot be performed.
        /// </exception>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
            {
                return string.Empty;
            }
            else if (destinationType == typeof(string))
            {
                return value.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
