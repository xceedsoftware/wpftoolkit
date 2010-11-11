// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// A panel that plots elements on a one dimensional plane.  In order to 
    /// minimize collisions it moves elements further and further from the edge 
    /// of the plane based on their priority.  Elements that have the same
    /// priority level are always the same distance from the edge.
    /// </summary>
    internal class OrientedPanel : Panel
    {
        #region public double ActualMinimumDistanceBetweenChildren
        /// <summary>
        /// Gets the actual minimum distance between children.
        /// </summary>
        public double ActualMinimumDistanceBetweenChildren
        {
            get { return (double)GetValue(ActualMinimumDistanceBetweenChildrenProperty); }
            private set { SetValue(ActualMinimumDistanceBetweenChildrenProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualMinimumDistanceBetweenChildren dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualMinimumDistanceBetweenChildrenProperty =
            DependencyProperty.Register(
                "ActualMinimumDistanceBetweenChildren",
                typeof(double),
                typeof(OrientedPanel),
                new PropertyMetadata(0.0));

        #endregion public double ActualMinimumDistanceBetweenChildren

        #region public double MinimumDistanceBetweenChildren
        /// <summary>
        /// Gets or sets the minimum distance between children.
        /// </summary>
        public double MinimumDistanceBetweenChildren
        {
            get { return (double)GetValue(MinimumDistanceBetweenChildrenProperty); }
            set { SetValue(MinimumDistanceBetweenChildrenProperty, value); }
        }

        /// <summary>
        /// Identifies the MinimumDistanceBetweenChildren dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumDistanceBetweenChildrenProperty =
            DependencyProperty.Register(
                "MinimumDistanceBetweenChildren",
                typeof(double),
                typeof(OrientedPanel),
                new PropertyMetadata(0.0, OnMinimumDistanceBetweenChildrenPropertyChanged));

        /// <summary>
        /// MinimumDistanceBetweenChildrenProperty property changed handler.
        /// </summary>
        /// <param name="d">OrientedPanel that changed its MinimumDistanceBetweenChildren.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnMinimumDistanceBetweenChildrenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OrientedPanel source = (OrientedPanel)d;
            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;
            source.OnMinimumDistanceBetweenChildrenPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// MinimumDistanceBetweenChildrenProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>        
        protected virtual void OnMinimumDistanceBetweenChildrenPropertyChanged(double oldValue, double newValue)
        {
            InvalidateMeasure();
        }
        #endregion public double MinimumDistanceBetweenChildren

        #region public double ActualLength
        /// <summary>
        /// Gets the actual length of the panel.
        /// </summary>
        public double ActualLength
        {
            get { return (double)GetValue(ActualLengthProperty); }
        }

        /// <summary>
        /// Identifies the ActualLength dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualLengthProperty =
            DependencyProperty.Register(
                "ActualLength",
                typeof(double),
                typeof(OrientedPanel),
                new PropertyMetadata(0.0));
        #endregion public double ActualLength

        #region public attached double CenterCoordinate
        /// <summary>
        /// Gets the value of the CenterCoordinate attached property for a specified UIElement.
        /// </summary>
        /// <param name="element">The UIElement from which the property value is read.</param>
        /// <returns>The CenterCoordinate property value for the UIElement.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This is an attached property and is only intended to be set on UIElement's")]
        public static double GetCenterCoordinate(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (double) element.GetValue(CenterCoordinateProperty);
        }

        /// <summary>
        /// Sets the value of the CenterCoordinate attached property to a specified UIElement.
        /// </summary>
        /// <param name="element">The UIElement to which the attached property is written.</param>
        /// <param name="value">The needed CenterCoordinate value.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This is an attached property and is only intended to be set on UIElement's")]
        public static void SetCenterCoordinate(UIElement element, double value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(CenterCoordinateProperty, value);
        }

        /// <summary>
        /// Identifies the CenterCoordinate dependency property.
        /// </summary>
        public static readonly DependencyProperty CenterCoordinateProperty =
            DependencyProperty.RegisterAttached(
                "CenterCoordinate",
                typeof(double),
                typeof(OrientedPanel),
                new PropertyMetadata(OnCenterCoordinatePropertyChanged));

        /// <summary>
        /// CenterCoordinateProperty property changed handler.
        /// </summary>
        /// <param name="dependencyObject">UIElement that changed its CenterCoordinate.</param>
        /// <param name="eventArgs">Event arguments.</param>
        public static void OnCenterCoordinatePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            UIElement source = dependencyObject as UIElement;
            if (source == null)
            {
                throw new ArgumentNullException("dependencyObject");
            }
            OrientedPanel parent = VisualTreeHelper.GetParent(source) as OrientedPanel;
            if (parent != null)
            {
                parent.InvalidateMeasure();
            }
        }
        #endregion public attached double CenterCoordinate

        #region public double OffsetPadding
        /// <summary>
        /// Gets or sets the amount of offset padding to add between items.
        /// </summary>
        public double OffsetPadding
        {
            get { return (double)GetValue(OffsetPaddingProperty); }
            set { SetValue(OffsetPaddingProperty, value); }
        }

        /// <summary>
        /// Identifies the OffsetPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty OffsetPaddingProperty =
            DependencyProperty.Register(
                "OffsetPadding",
                typeof(double),
                typeof(OrientedPanel),
                new PropertyMetadata(0.0, OnOffsetPaddingPropertyChanged));

        /// <summary>
        /// OffsetPaddingProperty property changed handler.
        /// </summary>
        /// <param name="d">OrientedPanel that changed its OffsetPadding.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnOffsetPaddingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OrientedPanel source = (OrientedPanel)d;
            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;
            source.OnOffsetPaddingPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// OffsetPaddingProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>        
        protected virtual void OnOffsetPaddingPropertyChanged(double oldValue, double newValue)
        {
            this.InvalidateMeasure();
        }
        #endregion public double OffsetPadding

        #region public attached int Priority
        /// <summary>
        /// Gets the value of the Priority attached property for a specified UIElement.
        /// </summary>
        /// <param name="element">The UIElement from which the property value is read.</param>
        /// <returns>The Priority property value for the UIElement.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This is an attached property and is only intended to be set on UIElement's")]
        public static int GetPriority(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (int) element.GetValue(PriorityProperty);
        }

        /// <summary>
        /// Sets the value of the Priority attached property to a specified UIElement.
        /// </summary>
        /// <param name="element">The UIElement to which the attached property is written.</param>
        /// <param name="value">The needed Priority value.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This is an attached property and is only intended to be set on UIElement's")]
        public static void SetPriority(UIElement element, int value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(PriorityProperty, value);
        }

        /// <summary>
        /// Identifies the Priority dependency property.
        /// </summary>
        public static readonly DependencyProperty PriorityProperty =
            DependencyProperty.RegisterAttached(
                "Priority",
                typeof(int),
                typeof(OrientedPanel),
                new PropertyMetadata(OnPriorityPropertyChanged));

        /// <summary>
        /// PriorityProperty property changed handler.
        /// </summary>
        /// <param name="dependencyObject">UIElement that changed its Priority.</param>
        /// <param name="eventArgs">Event arguments.</param>
        public static void OnPriorityPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            UIElement source = dependencyObject as UIElement;
            if (source == null)
            {
                throw new ArgumentNullException("dependencyObject");
            }
            OrientedPanel parent = VisualTreeHelper.GetParent(source) as OrientedPanel;
            if (parent != null)
            {
                parent.InvalidateMeasure();
            }
        }
        #endregion public attached int Priority

        #region public bool IsInverted
        /// <summary>
        /// Gets or sets a value indicating whether the panel is inverted.
        /// </summary>
        public bool IsInverted
        {
            get { return (bool)GetValue(IsInvertedProperty); }
            set { SetValue(IsInvertedProperty, value); }
        }

        /// <summary>
        /// Identifies the IsInverted dependency property.
        /// </summary>
        public static readonly DependencyProperty IsInvertedProperty =
            DependencyProperty.Register(
                "IsInverted",
                typeof(bool),
                typeof(OrientedPanel),
                new PropertyMetadata(false, OnIsInvertedPropertyChanged));

        /// <summary>
        /// IsInvertedProperty property changed handler.
        /// </summary>
        /// <param name="d">OrientedPanel that changed its IsInverted.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsInvertedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OrientedPanel source = (OrientedPanel)d;
            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            source.OnIsInvertedPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// IsInvertedProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>        
        protected virtual void OnIsInvertedPropertyChanged(bool oldValue, bool newValue)
        {
            InvalidateMeasure();
        }
        #endregion public bool IsInverted

        #region public bool IsReversed
        /// <summary>
        /// Gets or sets a value indicating whether the direction is reversed. 
        /// </summary>
        public bool IsReversed
        {
            get { return (bool)GetValue(IsReversedProperty); }
            set { SetValue(IsReversedProperty, value); }
        }

        /// <summary>
        /// Identifies the IsReversed dependency property.
        /// </summary>
        public static readonly DependencyProperty IsReversedProperty =
            DependencyProperty.Register(
                "IsReversed",
                typeof(bool),
                typeof(OrientedPanel),
                new PropertyMetadata(false, OnIsReversedPropertyChanged));

        /// <summary>
        /// IsReversedProperty property changed handler.
        /// </summary>
        /// <param name="d">OrientedPanel that changed its IsReversed.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsReversedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OrientedPanel source = (OrientedPanel)d;
            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            source.OnIsReversedPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// IsReversedProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>        
        protected virtual void OnIsReversedPropertyChanged(bool oldValue, bool newValue)
        {
            InvalidateMeasure();
        }
        #endregion public bool IsReversed

        #region public Orientation Orientation
        /// <summary>
        /// Gets or sets the orientation of the panel.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(Orientation),
                typeof(OrientedPanel),
                new PropertyMetadata(Orientation.Horizontal, OnOrientationPropertyChanged));

        /// <summary>
        /// OrientationProperty property changed handler.
        /// </summary>
        /// <param name="d">OrientedPanel that changed its Orientation.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OrientedPanel source = (OrientedPanel)d;
            Orientation newValue = (Orientation)e.NewValue;
            source.OnOrientationPropertyChanged(newValue);
        }

        /// <summary>
        /// OrientationProperty property changed handler.
        /// </summary>
        /// <param name="newValue">New value.</param>        
        protected virtual void OnOrientationPropertyChanged(Orientation newValue)
        {
            UpdateActualLength();
            InvalidateMeasure();
        }
        #endregion public Orientation Orientation

        /// <summary>
        /// Gets or sets the offset of the edge to use for each priority group.
        /// </summary>
        private IDictionary<int, double> PriorityOffsets { get; set; }

        /// <summary>
        /// Instantiates a new instance of the OrientedPanel class.
        /// </summary>
        public OrientedPanel()
        {
            UpdateActualLength();
        }

        /// <summary>
        /// Updates the actual length property.
        /// </summary>
        private void UpdateActualLength()
        {
            this.SetBinding(ActualLengthProperty, new Binding((Orientation == Orientation.Horizontal) ? "ActualWidth" : "ActualHeight") { Source = this });
        }

        /// <summary>
        /// Returns a sequence of ranges for a given sequence of children and a
        /// length selector.
        /// </summary>
        /// <param name="children">A sequence of children.</param>
        /// <param name="lengthSelector">A function that returns a length given
        /// a UIElement.</param>
        /// <returns>A sequence of ranges.</returns>
        private static IEnumerable<Range<double>> GetRanges(IEnumerable<UIElement> children, Func<UIElement, double> lengthSelector)
        {
            return 
                children
                    .Select(child =>
                    {
                        double centerCoordinate = GetCenterCoordinate(child);
                        double halfLength = lengthSelector(child) / 2;
                        return new Range<double>(centerCoordinate - halfLength, centerCoordinate + halfLength);
                    });
        }

        /// <summary>
        /// Measures children and determines necessary size.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The necessary size.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Linq use artificially increases cyclomatic complexity.  Linq functions are well-understood.")]
        protected override Size MeasureOverride(Size availableSize)
        {
            double offset = 0.0;
            if (Children.Count > 0)
            {
                Size totalSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
                foreach (UIElement child in this.Children)
                {
                    child.Measure(totalSize);
                }

                Func<UIElement, double> lengthSelector = null;
                Func<UIElement, double> offsetSelector = null;

                if (Orientation == Orientation.Horizontal)
                {
                    lengthSelector = child => GetCorrectedDesiredSize(child).Width;
                    offsetSelector = child => GetCorrectedDesiredSize(child).Height;
                }
                else
                {
                    lengthSelector = child => GetCorrectedDesiredSize(child).Height;
                    offsetSelector = child => GetCorrectedDesiredSize(child).Width;
                }

                IEnumerable<IGrouping<int, UIElement>> priorityGroups =
                    from child in Children.CastWrapper<UIElement>()
                    group child by GetPriority(child) into priorityGroup
                    select priorityGroup;

                ActualMinimumDistanceBetweenChildren =
                    (from priorityGroup in priorityGroups
                     let orderedElements =
                         (from element in priorityGroup
                          orderby GetCenterCoordinate(element) ascending
                          select element).ToList()
                     where orderedElements.Count >= 2
                     select
                         (EnumerableFunctions.Zip(
                             orderedElements,
                             orderedElements.Skip(1),
                             (leftElement, rightElement) =>
                             {
                                 double halfLeftLength = lengthSelector(leftElement) / 2;
                                 double leftCenterCoordinate = GetCenterCoordinate(leftElement);

                                 double halfRightLength = lengthSelector(rightElement) / 2;
                                 double rightCenterCoordinate = GetCenterCoordinate(rightElement);

                                 return (rightCenterCoordinate - halfRightLength) - (leftCenterCoordinate + halfLeftLength);
                             }))
                             .Min())
                        .MinOrNullable() ?? MinimumDistanceBetweenChildren;

                IEnumerable<int> priorities =
                    Children
                        .CastWrapper<UIElement>()
                        .Select(child => GetPriority(child)).Distinct().OrderBy(priority => priority).ToList();

                PriorityOffsets = new Dictionary<int, double>();
                foreach (int priority in priorities)
                {
                    PriorityOffsets[priority] = 0.0;
                }

                IEnumerable<Tuple<int, int>> priorityPairs =
                    EnumerableFunctions.Zip(priorities, priorities.Skip(1), (previous, next) => new Tuple<int, int>(previous, next));

                foreach (Tuple<int, int> priorityPair in priorityPairs)
                {
                    IEnumerable<UIElement> currentPriorityChildren = Children.CastWrapper<UIElement>().Where(child => GetPriority(child) == priorityPair.Item1).ToList();

                    IEnumerable<Range<double>> currentPriorityRanges =
                        GetRanges(currentPriorityChildren, lengthSelector);

                    IEnumerable<UIElement> nextPriorityChildren = Children.CastWrapper<UIElement>().Where(child => GetPriority(child) == priorityPair.Item2).ToList();

                    IEnumerable<Range<double>> nextPriorityRanges =
                        GetRanges(nextPriorityChildren, lengthSelector);

                    bool intersects =
                        (from currentPriorityRange in currentPriorityRanges
                         from nextPriorityRange in nextPriorityRanges
                         select currentPriorityRange.IntersectsWith(nextPriorityRange))
                            .Any(value => value);

                    if (intersects)
                    {
                        double maxCurrentPriorityChildOffset =
                            currentPriorityChildren
                                .Select(child => offsetSelector(child))
                                .MaxOrNullable() ?? 0.0;

                        offset += maxCurrentPriorityChildOffset + OffsetPadding;
                    }
                    PriorityOffsets[priorityPair.Item2] = offset;
                }

                offset =
                    (Children
                        .CastWrapper<UIElement>()
                        .GroupBy(child => GetPriority(child))
                        .Select(
                            group =>
                                group
                                    .Select(child => PriorityOffsets[group.Key] + offsetSelector(child))
                                    .MaxOrNullable()))
                    .Where(num => num.HasValue)
                    .Select(num => num.Value)
                    .MaxOrNullable() ?? 0.0;
            }

            if (Orientation == Orientation.Horizontal)
            {
                return new Size(0, offset);
            }
            else
            {
                return new Size(offset, 0);
            }
        }

        /// <summary>
        /// Arranges items according to position and priority.
        /// </summary>
        /// <param name="finalSize">The final size of the panel.</param>
        /// <returns>The final size of the control.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in Children)
            {
                double x = 0.0;
                double y = 0.0;

                x = GetCenterCoordinate(child);
                y = PriorityOffsets[GetPriority(child)];

                double totalLength = 0.0;
                double totalOffsetLength = 0.0;
                double length = 0.0;
                double offsetLength = 0.0;
                Size childCorrectedDesiredSize = GetCorrectedDesiredSize(child);
                if (Orientation == Orientation.Horizontal)
                {
                    totalLength = finalSize.Width;
                    length = childCorrectedDesiredSize.Width;
                    offsetLength = childCorrectedDesiredSize.Height;
                    totalOffsetLength = finalSize.Height;
                }
                else if (Orientation == Orientation.Vertical)
                {
                    totalLength = finalSize.Height;
                    length = childCorrectedDesiredSize.Height;
                    offsetLength = childCorrectedDesiredSize.Width;
                    totalOffsetLength = finalSize.Width;
                }

                double halfLength = length / 2;

                double left = 0.0;
                double top = 0.0;
                if (!IsReversed)
                {
                    left = x - halfLength;
                }
                else
                {
                    left = totalLength - Math.Round(x + halfLength);
                }
                if (!IsInverted)
                {
                    top = y;
                }
                else
                {
                    top = totalOffsetLength - Math.Round(y + offsetLength);
                }

                left = Math.Min(Math.Round(left), totalLength - 1);
                top = Math.Round(top);
                if (Orientation == Orientation.Horizontal)
                {
                    child.Arrange(new Rect(left, top, length, offsetLength));
                }
                else if (Orientation == Orientation.Vertical)
                {
                    child.Arrange(new Rect(top, left, offsetLength, length));
                }
            }

            return finalSize;
        }

        /// <summary>
        /// Gets the "corrected" DesiredSize (for Line instances); one that is
        /// more consistent with how the elements actually render.
        /// </summary>
        /// <param name="element">UIElement to get the size for.</param>
        /// <returns>Corrected size.</returns>
        private static Size GetCorrectedDesiredSize(UIElement element)
        {
            Line elementAsLine = element as Line;
            if (null != elementAsLine)
            {
                return new Size(
                    Math.Max(elementAsLine.StrokeThickness, elementAsLine.X2 - elementAsLine.X1),
                    Math.Max(elementAsLine.StrokeThickness, elementAsLine.Y2 - elementAsLine.Y1));
            }
            else
            {
                return element.DesiredSize;
            }
        }
    }
}