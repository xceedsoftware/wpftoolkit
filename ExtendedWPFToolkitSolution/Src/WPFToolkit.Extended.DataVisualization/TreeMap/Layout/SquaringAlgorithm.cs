// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Class encapsulating the logic of sub-dividing a parent rectangular area into child rectangles.
    /// It implements the squaring tree map algorithm where all child nodes are allocated
    /// areas proportional to their values, but the aspect ratio of each rectangle is kept 
    /// as close as possible to a square.
    /// </summary>
    internal class SquaringAlgorithm
    {
        /// <summary>
        /// Holds the list of nodes being considered by the algorithm.
        /// </summary>
        private IList<TreeMapNode> _areas;

        /// <summary>
        /// The current rectangle being divided.
        /// </summary>
        private Rect _currentRectangle;

        /// <summary>
        /// Internal index in the list of nodes being divided.
        /// </summary>
        private int _currentStart;

        /// <summary>
        /// Temporary variable used during the algorithm. Represents the ratio between 
        /// the real area of the rectangle and the virtual value associated with the node.
        /// </summary>
        private double _factor;

        /// <summary>
        /// Subdivides the parent rectangle using squaring tree map algorithm into
        /// rectangles with areas specified by the children. The areas must add up 
        /// to at most the area of the rectangle.
        /// </summary>
        /// <param name="parentRectangle">Total area being split.</param>
        /// <param name="parentNode">The node associated with the total area. The 
        /// children of this node will be allocated small chunks of the parent rectangle.</param>
        /// <param name="margin">How much of a gap should be left between the parent rectangle and the children.</param>
        /// <returns>A list of RectangularArea objects describing areas associated with each of the children of parentNode.</returns>
        public IEnumerable<Tuple<Rect, TreeMapNode>> Split(Rect parentRectangle, TreeMapNode parentNode, Thickness margin)
        {
            IEnumerable<Tuple<Rect, TreeMapNode>> retVal;

            double area = parentNode.Area;
            if (parentNode.Children == null || parentNode.Children.Count() == 0 || area == 0)
            {
                retVal = Enumerable.Empty<Tuple<Rect, TreeMapNode>>();
            }
            else
            {
                if (parentRectangle.Width - margin.Left - margin.Right <= 0 ||
                    parentRectangle.Height - margin.Top - margin.Bottom <= 0)
                {
                    // Margins too big, no more room for children. Returning
                    // zero sized rectangles for all children.
                    retVal = from child in parentNode.Children
                             select new Tuple<Rect, TreeMapNode>(new Rect(0, 0, 0, 0), child);
                }
                else
                {
                    // Leave as much room as specified by the margin
                    _currentRectangle = new Rect(
                        parentRectangle.X + margin.Left,
                        parentRectangle.Y + margin.Top,
                        parentRectangle.Width - margin.Left - margin.Right,
                        parentRectangle.Height - margin.Top - margin.Bottom);

                    _areas = (from child in parentNode.Children
                             where child.Area != 0
                             orderby child.Area descending
                             select child).ToArray();

                    // Factor is only computed once and used during the algorithm
                    _factor = _currentRectangle.Width * _currentRectangle.Height / area;

                    retVal = BuildTreeMap().ToArray();
                }
            }

            return retVal;
        }

        /// <summary>
        /// This function returns an IEnumerable over the rectangles associated with the children,
        /// as divided using the tree map algorithm.
        /// </summary>
        /// <returns>A list of RectangularArea objects describing areas associated with each of the children.</returns>
        private IEnumerable<Tuple<Rect, TreeMapNode>> BuildTreeMap()
        {
            _currentStart = 0;
            while (_currentStart < _areas.Count)
            {
                foreach (Tuple<Rect, TreeMapNode> rectangle in BuildTreeMapStep())
                {
                    yield return rectangle;
                }
            }
        }

        /// <summary>
        /// Performs one step of the body of the squaring tree map algorithm.
        /// </summary>
        /// <returns>List of rectangles calculated by this step.</returns>
        private IEnumerable<Tuple<Rect, TreeMapNode>> BuildTreeMapStep()
        {
            int last = _currentStart;
            double total = 0;
            double prevAspect = double.PositiveInfinity;
            double wh = 0;
            bool horizontal = _currentRectangle.Width > _currentRectangle.Height;
            for (; last < _areas.Count; last++)
            {
                total += GetArea(last);
                wh = total / (horizontal ? _currentRectangle.Height : _currentRectangle.Width);
                double curAspect = Math.Max(GetAspect(_currentStart, wh), GetAspect(last, wh));
                if (curAspect > prevAspect)
                {
                    total -= GetArea(last);
                    wh = total / (horizontal ? _currentRectangle.Height : _currentRectangle.Width);
                    last--;
                    break;
                }

                prevAspect = curAspect;
            }

            if (last == _areas.Count)
            {
                last--;
            }

            double x = _currentRectangle.Left;
            double y = _currentRectangle.Top;

            for (int i = _currentStart; i <= last; i++)
            {
                if (horizontal)
                {
                    double h = GetArea(i) / wh;
                    Rect rect = new Rect(x, y, wh, h);
                    yield return new Tuple<Rect, TreeMapNode>(rect, _areas[i]);
                    y += h;
                }
                else
                {
                    double w = GetArea(i) / wh;
                    Rect rect = new Rect(x, y, w, wh);
                    yield return new Tuple<Rect, TreeMapNode>(rect, _areas[i]);
                    x += w;
                }
            }

            _currentStart = last + 1;

            if (horizontal)
            {
                _currentRectangle = new Rect(_currentRectangle.Left + wh, _currentRectangle.Top, Math.Max(0, _currentRectangle.Width - wh), _currentRectangle.Height);
            }
            else
            {
                _currentRectangle = new Rect(_currentRectangle.Left, _currentRectangle.Top + wh, _currentRectangle.Width, Math.Max(0, _currentRectangle.Height - wh));
            }
        }

        /// <summary>
        /// Returns the calculated area of the node at the given index.
        /// </summary>
        /// <param name="i">Index of the node to consider.</param>
        /// <returns>Area of the node, calculated based on the node's value multiplied by the current factor.</returns>
        private double GetArea(int i)
        {
            return _areas[i].Area * _factor;
        }

        /// <summary>
        /// Returns the aspect ratio of the area associated with the node at the given index.
        /// </summary>
        /// <param name="i">Index of the node to consider.</param>
        /// <param name="wh">Width of the area.</param>
        /// <returns>Positive supra-unitary ratio of the given area.</returns>
        private double GetAspect(int i, double wh)
        {
            double aspect = GetArea(i) / (wh * wh);
            if (aspect < 1)
            {
                aspect = 1.0 / aspect;
            }

            return aspect;
        }
    }
}
