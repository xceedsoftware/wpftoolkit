// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Media;

namespace System.Windows.Controls.DataVisualization.Charting.Primitives
{
    /// <summary>
    /// Defines an area where you can arrange child elements either horizontally
    /// or vertically, relative to each other.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public class EdgePanel : Panel
    {
        /// <summary>
        /// The maximum number of iterations.
        /// </summary>
        private const int MaximumIterations = 10;

        /// <summary>
        /// A flag that ignores a property change when set.
        /// </summary>
        private static bool _ignorePropertyChange;

        #region public attached Edge Edge
        /// <summary>
        /// Gets the value of the Edge attached property for a specified
        /// UIElement.
        /// </summary>
        /// <param name="element">
        /// The element from which the property value is read.
        /// </param>
        /// <returns>The Edge property value for the element.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "EdgePanel only has UIElement children")]
        public static Edge GetEdge(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (Edge)element.GetValue(EdgeProperty);
        }

        /// <summary>
        /// Sets the value of the Edge attached property to a specified element.
        /// </summary>
        /// <param name="element">
        /// The element to which the attached property is written.
        /// </param>
        /// <param name="edge">The needed Edge value.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "EdgePanel only has UIElement children")]
        public static void SetEdge(UIElement element, Edge edge)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(EdgeProperty, edge);
        }

        /// <summary>
        /// Identifies the Edge dependency property.
        /// </summary>
        public static readonly DependencyProperty EdgeProperty =
            DependencyProperty.RegisterAttached(
                "Edge",
                typeof(Edge),
                typeof(EdgePanel),
                new PropertyMetadata(Edge.Center, OnEdgePropertyChanged));

        /// <summary>
        /// EdgeProperty property changed handler.
        /// </summary>
        /// <param name="d">UIElement that changed its Edge.</param>
        /// <param name="e">Event arguments.</param>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "Almost always set from the attached property CLR setter.")]
        private static void OnEdgePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Ignore the change if requested
            if (_ignorePropertyChange)
            {
                _ignorePropertyChange = false;
                return;
            }

            UIElement element = (UIElement)d;
            Edge value = (Edge)e.NewValue;

            // Validate the Edge property
            if ((value != Edge.Left) &&
                (value != Edge.Top) &&
                (value != Edge.Right) &&
                (value != Edge.Center) &&
                (value != Edge.Bottom))
            {
                // Reset the property to its original state before throwing
                _ignorePropertyChange = true;
                element.SetValue(EdgeProperty, (Edge)e.OldValue);

                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.EdgePanel_OnEdgePropertyChanged,
                    value);
                 
                throw new ArgumentException(message, "value");
            }
                 
            // Cause the EdgePanel to update its layout when a child changes
            EdgePanel panel = VisualTreeHelper.GetParent(element) as EdgePanel;
            if (panel != null)
            {
                panel.InvalidateMeasure();
            }
        }
        #endregion public attached Edge Edge

        /// <summary>
        /// Initializes a new instance of the EdgePanel class.
        /// </summary>
        public EdgePanel()
        {
            this.SizeChanged += new SizeChangedEventHandler(EdgePanelSizeChanged);
        }

        /// <summary>
        /// Invalidate measure when edge panel is resized.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void EdgePanelSizeChanged(object sender, SizeChangedEventArgs e)
        {
            InvalidateMeasure();
        }

        /// <summary>
        /// The left rectangle in which to render left elements.
        /// </summary>
        private Rect _leftRect;

        /// <summary>
        /// The right rectangle in which to render right elements.
        /// </summary>
        private Rect _rightRect;

        /// <summary>
        /// The top rectangle in which to render top elements.
        /// </summary>
        private Rect _topRect;

        /// <summary>
        /// The bottom rectangle in which to render bottom elements.
        /// </summary>
        private Rect _bottomRect;

        /// <summary>
        /// Measures the children of a EdgePanel in anticipation of arranging
        /// them during the ArrangeOverride pass.
        /// </summary>
        /// <param name="constraint">A maximum Size to not exceed.</param>
        /// <returns>The desired size of the EdgePanel.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "Code is by nature difficult to refactor into several methods.")] 
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#", Justification = "Compat with WPF.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Splitting up method will make it more difficult to understand.")]
        protected override Size MeasureOverride(Size constraint)
        {
            constraint = new Size(this.ActualWidth, this.ActualHeight);

            IList<UIElement> leftElements = this.Children.OfType<UIElement>().Where(element => GetEdge(element) == Edge.Left).ToList();
            IList<UIElement> rightElements = this.Children.OfType<UIElement>().Where(element => GetEdge(element) == Edge.Right).ToList();
            IList<UIElement> bottomElements = this.Children.OfType<UIElement>().Where(element => GetEdge(element) == Edge.Bottom).ToList();
            IList<UIElement> topElements = this.Children.OfType<UIElement>().Where(element => GetEdge(element) == Edge.Top).ToList();

            Rect totalRect = SafeCreateRect(0, 0, constraint.Width, constraint.Height);

            _leftRect = (leftElements.Count > 0) ? totalRect : Rect.Empty;
            _bottomRect = (bottomElements.Count > 0) ? totalRect : Rect.Empty;
            _rightRect = (rightElements.Count > 0) ? totalRect : Rect.Empty;
            _topRect = (topElements.Count > 0) ? totalRect : Rect.Empty;

            double rightAxesWidth = 0.0;
            double leftAxesWidth = 0.0;
            double topAxesHeight = 0.0;
            double bottomAxesHeight = 0.0;

            double maxRightRequestedWidth = 0;
            double maxLeftRequestedWidth = 0;
            double maxTopRequestedHeight = 0;
            double maxBottomRequestedHeight = 0;

            double previousRightAxesWidth = rightAxesWidth;
            double previousLeftAxesWidth = leftAxesWidth;
            double previousTopAxesHeight = topAxesHeight;
            double previousBottomAxesHeight = bottomAxesHeight;

            int iterations = 0;
            // Measure each of the Children
            while (true)
            {
                // Measure the children using the rectangle regions.
                if (rightElements.Count > 0)
                {
                    Size rightSize = new Size(constraint.Width, _rightRect.Height);
                    foreach (UIElement rightUIElement in rightElements)
                    {
                        rightUIElement.Measure(rightSize);
                    }

                    previousRightAxesWidth = rightAxesWidth;
                    rightAxesWidth = rightElements.Select(axis => axis.DesiredSize.Width).SumOrDefault();
                    maxRightRequestedWidth = Math.Max(maxRightRequestedWidth, rightAxesWidth);
                    _rightRect =
                        SafeCreateRect(
                            constraint.Width - rightAxesWidth,
                            _rightRect.Top,
                            rightAxesWidth,
                            _rightRect.Height);
                }

                if (topElements.Count > 0)
                {
                    Size topSize = new Size(_topRect.Width, constraint.Height);
                    foreach (UIElement topUIElement in topElements)
                    {
                        topUIElement.Measure(topSize);
                    }

                    previousTopAxesHeight = topAxesHeight;
                    topAxesHeight = topElements.Select(axis => axis.DesiredSize.Height).SumOrDefault();
                    maxTopRequestedHeight = Math.Max(maxTopRequestedHeight, topAxesHeight);
                    _topRect =
                        SafeCreateRect(
                            _topRect.Left,
                            _topRect.Top,
                            _topRect.Width,
                            topAxesHeight);
                }

                if (leftElements.Count > 0)
                {
                    Size leftSize = new Size(constraint.Width, _leftRect.Height);
                    foreach (UIElement leftUIElement in leftElements)
                    {
                        leftUIElement.Measure(leftSize);
                    }

                    previousLeftAxesWidth = leftAxesWidth;
                    leftAxesWidth = leftElements.Select(axis => axis.DesiredSize.Width).SumOrDefault();
                    maxLeftRequestedWidth = Math.Max(maxLeftRequestedWidth, leftAxesWidth);
                    _leftRect =
                        SafeCreateRect(
                            _leftRect.Left,
                            _leftRect.Top,
                            leftElements.Select(axis => axis.DesiredSize.Width).SumOrDefault(),
                            _leftRect.Height);
                }

                if (bottomElements.Count > 0)
                {
                    Size bottomSize = new Size(_bottomRect.Width, constraint.Height);
                    foreach (UIElement bottomUIElement in bottomElements)
                    {
                        bottomUIElement.Measure(bottomSize);
                    }

                    previousBottomAxesHeight = bottomAxesHeight;
                    bottomAxesHeight = bottomElements.Select(axis => axis.DesiredSize.Height).SumOrDefault();
                    maxBottomRequestedHeight = Math.Max(maxBottomRequestedHeight, bottomAxesHeight);
                    _bottomRect =
                        SafeCreateRect(
                            _bottomRect.Left,
                            constraint.Height - bottomAxesHeight,
                            _bottomRect.Width,
                            bottomAxesHeight);
                }

                // Ensuring that parallel axes don't collide
                Rect leftRightCollisionRect = _leftRect;
                leftRightCollisionRect.Intersect(_rightRect);

                Rect topBottomCollisionRect = _topRect;
                topBottomCollisionRect.Intersect(_bottomRect);

                if (!leftRightCollisionRect.IsEmptyOrHasNoSize() || !topBottomCollisionRect.IsEmptyOrHasNoSize())
                {
                    return new Size();
                }

                // Resolving perpendicular axes collisions
                Rect leftTopCollisionRect = _leftRect;
                leftTopCollisionRect.Intersect(_topRect);
                
                Rect rightTopCollisionRect = _rightRect;
                rightTopCollisionRect.Intersect(_topRect);

                Rect leftBottomCollisionRect = _leftRect;
                leftBottomCollisionRect.Intersect(_bottomRect);

                Rect rightBottomCollisionRect = _rightRect;
                rightBottomCollisionRect.Intersect(_bottomRect);

                if (leftBottomCollisionRect.IsEmptyOrHasNoSize()
                    && rightBottomCollisionRect.IsEmptyOrHasNoSize()
                    && leftTopCollisionRect.IsEmptyOrHasNoSize()
                    && rightTopCollisionRect.IsEmptyOrHasNoSize()                    
                    && previousBottomAxesHeight == bottomAxesHeight
                    && previousLeftAxesWidth == leftAxesWidth
                    && previousRightAxesWidth == rightAxesWidth
                    && previousTopAxesHeight == topAxesHeight)
                {
                    break;
                }

                if (iterations == MaximumIterations)
                {
                    _leftRect = SafeCreateRect(0, maxTopRequestedHeight, maxLeftRequestedWidth, (constraint.Height - maxTopRequestedHeight) - maxBottomRequestedHeight);
                    _rightRect = SafeCreateRect(constraint.Width - maxRightRequestedWidth, maxTopRequestedHeight, maxRightRequestedWidth, (constraint.Height - maxTopRequestedHeight) - maxBottomRequestedHeight);
                    _bottomRect = SafeCreateRect(maxLeftRequestedWidth, constraint.Height - maxBottomRequestedHeight, (constraint.Width - maxLeftRequestedWidth) - maxRightRequestedWidth, maxBottomRequestedHeight);
                    _topRect = SafeCreateRect(maxLeftRequestedWidth, 0, (constraint.Width - maxLeftRequestedWidth) - maxRightRequestedWidth, maxTopRequestedHeight);

                    foreach (UIElement leftElement in leftElements)
                    {
                        leftElement.Measure(new Size(_leftRect.Width, _leftRect.Height));
                    }

                    foreach (UIElement rightElement in rightElements)
                    {
                        rightElement.Measure(new Size(_rightRect.Width, _rightRect.Height));
                    }

                    foreach (UIElement bottomElement in bottomElements)
                    {
                        bottomElement.Measure(new Size(_bottomRect.Width, _bottomRect.Height));
                    }

                    foreach (UIElement topElement in topElements)
                    {
                        topElement.Measure(new Size(_topRect.Width, _topRect.Height));
                    }
                    break;
                }

                if (!leftBottomCollisionRect.IsEmptyOrHasNoSize())
                {
                    _leftRect =
                        SafeCreateRect(
                            _leftRect.Left,
                            _leftRect.Top,
                            _leftRect.Width,
                            _leftRect.Height - leftBottomCollisionRect.Height);

                    _bottomRect =
                        SafeCreateRect(
                            _bottomRect.Left + leftBottomCollisionRect.Width,
                            _bottomRect.Top,
                            _bottomRect.Width - leftBottomCollisionRect.Width,
                            _bottomRect.Height);
                }

                if (!leftTopCollisionRect.IsEmptyOrHasNoSize())
                {
                    _leftRect =
                        SafeCreateRect(
                            _leftRect.Left,
                            _leftRect.Top + leftTopCollisionRect.Height,
                            _leftRect.Width,
                            _leftRect.Height - leftTopCollisionRect.Height);

                    _topRect =
                        SafeCreateRect(
                            _topRect.Left + leftTopCollisionRect.Width,
                            _topRect.Top,
                            _topRect.Width - leftTopCollisionRect.Width,
                            _topRect.Height);
                }

                if (!rightBottomCollisionRect.IsEmptyOrHasNoSize())
                {
                    _rightRect =
                        SafeCreateRect(
                            _rightRect.Left,
                            _rightRect.Top,
                            _rightRect.Width,
                            _rightRect.Height - rightBottomCollisionRect.Height);

                    _bottomRect =
                        SafeCreateRect(
                            _bottomRect.Left,
                            _bottomRect.Top,
                            _bottomRect.Width - rightBottomCollisionRect.Width,
                            _bottomRect.Height);
                }

                if (!rightTopCollisionRect.IsEmptyOrHasNoSize())
                {
                    _rightRect =
                        SafeCreateRect(
                            _rightRect.Left,
                            _rightRect.Top + rightTopCollisionRect.Height,
                            _rightRect.Width,
                            _rightRect.Height - rightTopCollisionRect.Height);

                    _topRect =
                        SafeCreateRect(
                            _topRect.Left,
                            _topRect.Top,
                            _topRect.Width - rightTopCollisionRect.Width,
                            _topRect.Height);
                }

                // Bring axis measure rectangles together if there are gaps 
                // between them.
                if (!_leftRect.IsEmpty)
                {
                    _leftRect =
                        new Rect(
                            new Point(_leftRect.Left, _topRect.BottomOrDefault(0)),
                            new Point(_leftRect.Right, _bottomRect.TopOrDefault(constraint.Height)));
                }

                if (!_rightRect.IsEmpty)
                {
                    _rightRect =
                        new Rect(
                            new Point(_rightRect.Left, _topRect.BottomOrDefault(0)),
                            new Point(_rightRect.Right, _bottomRect.TopOrDefault(constraint.Height)));
                }

                if (!_bottomRect.IsEmpty)
                {
                    _bottomRect =
                        new Rect(
                            new Point(_leftRect.RightOrDefault(0), _bottomRect.Top),
                            new Point(_rightRect.LeftOrDefault(constraint.Width), _bottomRect.Bottom));
                }

                if (!_topRect.IsEmpty)
                {
                    _topRect =
                        new Rect(
                            new Point(_leftRect.RightOrDefault(0), _topRect.Top),
                            new Point(_rightRect.LeftOrDefault(constraint.Width), _topRect.Bottom));
                }

                iterations++;
            }

            Size centerSize = 
                new Size(
                    (constraint.Width - _leftRect.WidthOrDefault(0)) - _rightRect.WidthOrDefault(0), 
                    (constraint.Height - _topRect.HeightOrDefault(0)) - _bottomRect.HeightOrDefault(0));

            foreach (UIElement element in Children.OfType<UIElement>().Where(child => GetEdge(child) == Edge.Center))
            {
                element.Measure(centerSize);
            }

            return new Size();
        }

        /// <summary>
        /// Arranges the content (child elements) of a EdgePanel element.
        /// </summary>
        /// <param name="arrangeSize">
        /// The Size the EdgePanel uses to arrange its child elements.
        /// </param>
        /// <returns>The arranged size of the EdgePanel.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Splitting up method will make it more difficult to understand.")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#", Justification = "Compat with WPF.")]
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (arrangeSize.Width == 0 || arrangeSize.Height == 0 || !ValueHelper.CanGraph(arrangeSize.Width) || !ValueHelper.CanGraph(arrangeSize.Height))
            {
                return arrangeSize;
            }

            IList<UIElement> leftElements = this.Children.OfType<UIElement>().Where(element => GetEdge(element) == Edge.Left).ToList();
            IList<UIElement> rightElements = this.Children.OfType<UIElement>().Where(element => GetEdge(element) == Edge.Right).ToList();
            IList<UIElement> bottomElements = this.Children.OfType<UIElement>().Where(element => GetEdge(element) == Edge.Bottom).ToList();
            IList<UIElement> topElements = this.Children.OfType<UIElement>().Where(element => GetEdge(element) == Edge.Top).ToList();
            
            if (!_bottomRect.IsEmpty)
            {
                double workingHeight = _bottomRect.Top;
                foreach (UIElement bottomUIElement in bottomElements)
                {
                    bottomUIElement.Arrange(SafeCreateRect(_leftRect.RightOrDefault(0), workingHeight, (arrangeSize.Width - _leftRect.WidthOrDefault(0)) - _rightRect.WidthOrDefault(0), bottomUIElement.DesiredSize.Height));
                    workingHeight += bottomUIElement.DesiredSize.Height;
                }
            }
            if (!_topRect.IsEmpty)
            {
                double workingTop = _topRect.Bottom;
                foreach (UIElement topUIElement in topElements)
                {
                    workingTop -= topUIElement.DesiredSize.Height;
                    topUIElement.Arrange(SafeCreateRect(_leftRect.RightOrDefault(0), workingTop, (arrangeSize.Width - _leftRect.WidthOrDefault(0)) - _rightRect.WidthOrDefault(0), topUIElement.DesiredSize.Height));
                }
            }

            if (!_rightRect.IsEmpty)
            {
                double workingRight = _rightRect.Left;
                foreach (UIElement rightUIElement in rightElements)
                {
                    rightUIElement.Arrange(SafeCreateRect(workingRight, _topRect.BottomOrDefault(0), rightUIElement.DesiredSize.Width, (arrangeSize.Height - _bottomRect.HeightOrDefault(0)) - _topRect.HeightOrDefault(0)));
                    workingRight += rightUIElement.DesiredSize.Width;
                }
            }

            if (!_leftRect.IsEmpty)
            {
                double workingLeft = _leftRect.Right;
                foreach (UIElement leftUIElement in leftElements)
                {
                    workingLeft -= leftUIElement.DesiredSize.Width;
                    Rect leftRect = SafeCreateRect(workingLeft, _topRect.BottomOrDefault(0), leftUIElement.DesiredSize.Width, (arrangeSize.Height - _bottomRect.HeightOrDefault(0)) - _topRect.HeightOrDefault(0));
                    leftUIElement.Arrange(leftRect);
                }
            }

            Rect centerRect = SafeCreateRect(
                        _leftRect.RightOrDefault(0),
                        _topRect.BottomOrDefault(0),
                        ((arrangeSize.Width - _leftRect.WidthOrDefault(0)) - _rightRect.WidthOrDefault(0)),
                        ((arrangeSize.Height - _topRect.HeightOrDefault(0)) - _bottomRect.HeightOrDefault(0)));

            foreach (UIElement element in Children.OfType<UIElement>().Where(child => GetEdge(child) == Edge.Center))
            {
                element.Arrange(centerRect);
            }

            return arrangeSize;
        }

        /// <summary>
        /// Creates a Rect safely by forcing width/height to be valid.
        /// </summary>
        /// <param name="left">Rect left parameter.</param>
        /// <param name="top">Rect top parameter.</param>
        /// <param name="width">Rect width parameter.</param>
        /// <param name="height">Rect height parameter.</param>
        /// <returns>New Rect struct.</returns>
        private static Rect SafeCreateRect(double left, double top, double width, double height)
        {
            return new Rect(left, top, Math.Max(0.0, width), Math.Max(0.0, height));
        }
    }
}