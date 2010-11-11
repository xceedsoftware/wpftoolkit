// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// This control draws gridlines with the help of an axis.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GridLine", Justification = "This is the expected capitalization.")]
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DisplayAxisGridLines", Justification = "This is the expected capitalization.")]
    internal abstract class DisplayAxisGridLines : Canvas, IAxisListener
    {
        #region public DisplayAxis Axis

        /// <summary>
        /// The field that stores the axis that the grid lines are connected to.
        /// </summary>
        private DisplayAxis _axis;

        /// <summary>
        /// Gets the axis that the grid lines are connected to.
        /// </summary>
        public DisplayAxis Axis
        {
            get { return _axis; }
            private set
            {
                if (_axis != value)
                {
                    DisplayAxis oldValue = _axis;
                    _axis = value;
                    if (oldValue != _axis)
                    {
                        OnAxisPropertyChanged(oldValue, value);
                    }
                }
            }
        }

        /// <summary>
        /// AxisProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        private void OnAxisPropertyChanged(DisplayAxis oldValue, DisplayAxis newValue)
        {
            Debug.Assert(newValue != null, "Don't set the axis property to null.");

            if (newValue != null)
            {
                newValue.RegisteredListeners.Add(this);
            }

            if (oldValue != null)
            {
                oldValue.RegisteredListeners.Remove(this);
            }
        }
        #endregion public DisplayAxis Axis

        /// <summary>
        /// Instantiates a new instance of the DisplayAxisGridLines class.
        /// </summary>
        /// <param name="axis">The axis used by the DisplayAxisGridLines.</param>
        public DisplayAxisGridLines(DisplayAxis axis)
        {
            this.Axis = axis;
            this.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
        }

        /// <summary>
        /// Redraws grid lines when the size of the control changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Invalidate();
        }

        /// <summary>
        /// Redraws grid lines when the axis is invalidated.
        /// </summary>
        /// <param name="axis">The invalidated axis.</param>
        public void AxisInvalidated(IAxis axis)
        {
            Invalidate();
        }

        /// <summary>
        /// Draws the grid lines.
        /// </summary>
        protected abstract void Invalidate();
    }
}