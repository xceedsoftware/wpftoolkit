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
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GridLines", Justification = "This is the expected capitalization.")]
    internal class OrientedAxisGridLines : DisplayAxisGridLines
    {
        /// <summary>
        /// A pool of grid lines.
        /// </summary>
        private ObjectPool<Line> _gridLinePool;

        /// <summary>
        /// Initializes a new instance of the OrientedAxisGridLines class.
        /// </summary>
        /// <param name="displayAxis">The axis to draw grid lines for.</param>
        public OrientedAxisGridLines(DisplayAxis displayAxis)
            : base(displayAxis)
        {
            _gridLinePool = new ObjectPool<Line>(() => new Line { Style = Axis.GridLineStyle });
        }

        /// <summary>
        /// Draws the grid lines.
        /// </summary>
        protected override void Invalidate()
        {
            _gridLinePool.Reset();

            try
            {
                IList<UnitValue> intervals = Axis.InternalGetMajorGridLinePositions().ToList();

                this.Children.Clear();

                double maximumHeight = Math.Max(Math.Round(ActualHeight - 1), 0);
                double maximumWidth = Math.Max(Math.Round(ActualWidth - 1), 0);
                for (int index = 0; index < intervals.Count; index++)
                {
                    double currentValue = intervals[index].Value;

                    double position = currentValue;
                    if (!double.IsNaN(position))
                    {
                        Line line = _gridLinePool.Next();
                        if (Axis.Orientation == AxisOrientation.Y)
                        {
                            line.Y1 = line.Y2 = maximumHeight - Math.Round(position - (line.StrokeThickness / 2));
                            line.X1 = 0.0;
                            line.X2 = maximumWidth;
                        }
                        else if (Axis.Orientation == AxisOrientation.X)
                        {
                            line.X1 = line.X2 = Math.Round(position - (line.StrokeThickness / 2));
                            line.Y1 = 0.0;
                            line.Y2 = maximumHeight;
                        }
                        // workaround for '1px line thickness issue'
                        if (line.StrokeThickness % 2 > 0)
                        {
                            line.SetValue(Canvas.LeftProperty, 0.5);
                            line.SetValue(Canvas.TopProperty, 0.5);
                        }
                        this.Children.Add(line);
                    }
                }
            }
            finally
            {
                _gridLinePool.Done();
            }
        }
    }
}