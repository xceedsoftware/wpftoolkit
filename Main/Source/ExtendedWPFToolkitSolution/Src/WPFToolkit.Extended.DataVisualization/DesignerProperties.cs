// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics.CodeAnalysis;

namespace System.Windows.Controls
{
    /// <summary>
    /// Provides a custom implementation of DesignerProperties.GetIsInDesignMode
    /// to work around an issue.
    /// </summary>
    internal static class DesignerProperties
    {
        /// <summary>
        /// Returns whether the control is in design mode (running under Blend
        /// or Visual Studio).
        /// </summary>
        /// <param name="element">The element from which the property value is
        /// read.</param>
        /// <returns>True if in design mode.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "element", Justification = "Matching declaration of System.ComponentModel.DesignerProperties.GetIsInDesignMode (which has a bug and is not reliable).")]
        public static bool GetIsInDesignMode(DependencyObject element)
        {
            if (!_isInDesignMode.HasValue)
            {
#if SILVERLIGHT
                _isInDesignMode =
                    (null == Application.Current) ||
                    Application.Current.GetType() == typeof(Application);
#else
                _isInDesignMode = System.ComponentModel.DesignerProperties.GetIsInDesignMode(element);
#endif
            }
            return _isInDesignMode.Value;
        }

        /// <summary>
        /// Stores the computed InDesignMode value.
        /// </summary>
        private static bool? _isInDesignMode;
    }
}