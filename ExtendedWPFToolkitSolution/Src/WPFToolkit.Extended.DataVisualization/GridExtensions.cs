// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// A set of extension methods for the Grid container.
    /// </summary>
    internal static class GridExtensions
    {
        /// <summary>
        /// Mirrors the grid either horizontally or vertically.
        /// </summary>
        /// <param name="grid">The grid to mirror.</param>
        /// <param name="orientation">The orientation to mirror the grid along.
        /// </param>
        public static void Mirror(this Grid grid, Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
            {
                IList<RowDefinition> rows = grid.RowDefinitions.Reverse().ToList();
                grid.RowDefinitions.Clear();
                foreach (FrameworkElement child in grid.Children.OfType<FrameworkElement>())
                {
                    Grid.SetRow(child, (rows.Count - 1) - Grid.GetRow(child));
                }
                foreach (RowDefinition row in rows)
                {
                    grid.RowDefinitions.Add(row);
                }
            }
            else if (orientation == Orientation.Vertical)
            {
                IList<ColumnDefinition> columns = grid.ColumnDefinitions.Reverse().ToList();
                grid.ColumnDefinitions.Clear();
                foreach (FrameworkElement child in grid.Children.OfType<FrameworkElement>())
                {
                    Grid.SetColumn(child, (columns.Count - 1) - Grid.GetColumn(child));
                }
                foreach (ColumnDefinition column in columns)
                {
                    grid.ColumnDefinitions.Add(column);
                }
            }
        }
    }
}
