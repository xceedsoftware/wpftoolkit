// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Represents the title of a data visualization control.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public partial class Title : ContentControl
    {
#if !SILVERLIGHT
        /// <summary>
        /// Initializes the static members of the Title class.
        /// </summary>
        static Title()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Title), new FrameworkPropertyMetadata(typeof(Title)));
        }

#endif
        /// <summary>
        /// Initializes a new instance of the Title class.
        /// </summary>
        public Title()
        {
#if SILVERLIGHT
            DefaultStyleKey = typeof(Title);
#endif
        }
    }
}