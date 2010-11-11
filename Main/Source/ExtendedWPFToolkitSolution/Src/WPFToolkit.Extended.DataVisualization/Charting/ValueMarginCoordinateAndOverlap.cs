// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// A class used to calculate axis range. 
    /// </summary>
    internal class ValueMarginCoordinateAndOverlap
    {
        /// <summary>
        /// Gets or sets the value margin object.
        /// </summary>
        public ValueMargin ValueMargin { get; set; }

        /// <summary>
        /// Gets or sets the coordinate.
        /// </summary>
        public double Coordinate { get; set; }

        /// <summary>
        /// Gets or sets the left overlap.
        /// </summary>
        public double LeftOverlap { get; set; }

        /// <summary>
        /// Gets or sets the right overlap.
        /// </summary>
        public double RightOverlap { get; set; }

        /// <summary>
        /// Initializes a new instance of the ValueMarginCoordinateAndOverlap 
        /// class.
        /// </summary>
        public ValueMarginCoordinateAndOverlap()
        {
        }
    }
}