// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Shapes;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// An axis that displays categories.
    /// </summary>
    [StyleTypedProperty(Property = "GridLineStyle", StyleTargetType = typeof(Line))]
    [StyleTypedProperty(Property = "MajorTickMarkStyle", StyleTargetType = typeof(Line))]
    [StyleTypedProperty(Property = "AxisLabelStyle", StyleTargetType = typeof(AxisLabel))]
    [StyleTypedProperty(Property = "TitleStyle", StyleTargetType = typeof(Title))]
    [TemplatePart(Name = AxisGridName, Type = typeof(Grid))]
    [TemplatePart(Name = AxisTitleName, Type = typeof(Title))]
    public class CategoryAxis : DisplayAxis, ICategoryAxis
    {
        /// <summary>
        /// A pool of major tick marks.
        /// </summary>
        private ObjectPool<Line> _majorTickMarkPool;

        /// <summary>
        /// A pool of labels.
        /// </summary>
        private ObjectPool<Control> _labelPool;

        #region public CategorySortOrder SortOrder
        /// <summary>
        /// Gets or sets the sort order used for the categories.
        /// </summary>
        public CategorySortOrder SortOrder
        {
            get { return (CategorySortOrder)GetValue(SortOrderProperty); }
            set { SetValue(SortOrderProperty, value); }
        }

        /// <summary>
        /// Identifies the SortOrder dependency property.
        /// </summary>
        public static readonly DependencyProperty SortOrderProperty =
            DependencyProperty.Register(
                "SortOrder",
                typeof(CategorySortOrder),
                typeof(CategoryAxis),
                new PropertyMetadata(CategorySortOrder.None, OnSortOrderPropertyChanged));

        /// <summary>
        /// SortOrderProperty property changed handler.
        /// </summary>
        /// <param name="d">CategoryAxis that changed its SortOrder.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSortOrderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CategoryAxis source = (CategoryAxis)d;
            source.OnSortOrderPropertyChanged();
        }

        /// <summary>
        /// SortOrderProperty property changed handler.
        /// </summary>
        private void OnSortOrderPropertyChanged()
        {
            Invalidate();
        }
        #endregion public CategorySortOrder SortOrder

        /// <summary>
        /// Gets or sets a list of categories to display.
        /// </summary>
        private IList<object> Categories { get; set; }

        /// <summary>
        /// Gets or sets the grid line coordinates to display.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GridLine", Justification = "This is the expected capitalization.")]
        private IList<UnitValue> GridLineCoordinatesToDisplay { get; set; }

        /// <summary>
        /// Instantiates a new instance of the CategoryAxis class.
        /// </summary>
        public CategoryAxis()
        {
            this._labelPool = new ObjectPool<Control>(() => CreateAxisLabel());
            this._majorTickMarkPool = new ObjectPool<Line>(() => CreateMajorTickMark());
            this.Categories = new List<object>();
            this.GridLineCoordinatesToDisplay = new List<UnitValue>();
        }

        /// <summary>
        /// Updates categories when a series is registered.
        /// </summary>
        /// <param name="series">The series to be registered.</param>
        protected override void OnObjectRegistered(IAxisListener series)
        {
            base.OnObjectRegistered(series);
            if (series is IDataProvider)
            {
                UpdateCategories();
            }
        }

        /// <summary>
        /// Updates categories when a series is unregistered.
        /// </summary>
        /// <param name="series">The series to be unregistered.</param>
        protected override void OnObjectUnregistered(IAxisListener series)
        {
            base.OnObjectUnregistered(series);
            if (series is IDataProvider)
            {
                UpdateCategories();
            }
        }

        /// <summary>
        /// Returns range of coordinates for a given category.
        /// </summary>
        /// <param name="category">The category to return the range for.</param>
        /// <returns>The range of coordinates corresponding to the category.
        /// </returns>
        public Range<UnitValue> GetPlotAreaCoordinateRange(object category)
        {
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }
            int index = Categories.IndexOf(category);
            if (index == -1)
            {
                return new Range<UnitValue>();
            }

            if (Orientation == AxisOrientation.X || Orientation == AxisOrientation.Y)
            {
                double maximumLength = Math.Max(ActualLength - 1, 0);
                double lower = (index * maximumLength) / Categories.Count;
                double upper = ((index + 1) * maximumLength) / Categories.Count;

                if (Orientation == AxisOrientation.X)
                {
                    return new Range<UnitValue>(new UnitValue(lower, Unit.Pixels), new UnitValue(upper, Unit.Pixels));
                }
                else if (Orientation == AxisOrientation.Y)
                {
                    return new Range<UnitValue>(new UnitValue(maximumLength - upper, Unit.Pixels), new UnitValue(maximumLength - lower, Unit.Pixels));
                }
            }
            else
            {
                double startingAngle = 270.0;
                double angleOffset = 360 / this.Categories.Count;
                double halfAngleOffset = angleOffset / 2.0;
                int categoryIndex = this.Categories.IndexOf(category);
                double angle = startingAngle + (categoryIndex * angleOffset);

                return new Range<UnitValue>(new UnitValue(angle - halfAngleOffset, Unit.Degrees), new UnitValue(angle + halfAngleOffset, Unit.Degrees));
            }

            return new Range<UnitValue>();
        }

        /// <summary>
        /// Returns the category at a given coordinate.
        /// </summary>
        /// <param name="position">The plot area position.</param>
        /// <returns>The category at the given plot area position.</returns>
        public object GetCategoryAtPosition(UnitValue position)
        {
            if (this.ActualLength == 0.0 || this.Categories.Count == 0)
            {
                return null;
            }
            if (position.Unit == Unit.Pixels)
            {
                double coordinate = position.Value;
                int index = (int)Math.Floor(coordinate / (this.ActualLength / this.Categories.Count));
                if (index >= 0 && index < this.Categories.Count)
                {
                    if (Orientation == AxisOrientation.X)
                    {
                        return this.Categories[index];
                    }
                    else
                    {
                        return this.Categories[(this.Categories.Count - 1) - index];
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            return null;
        }

        /// <summary>
        /// Updates the categories in response to an update from a registered
        /// axis data provider.
        /// </summary>
        /// <param name="dataProvider">The category axis information
        /// provider.</param>
        /// <param name="data">A sequence of categories.</param>
        public void DataChanged(IDataProvider dataProvider, IEnumerable<object> data)
        {
            UpdateCategories();
        }

        /// <summary>
        /// Updates the list of categories.
        /// </summary>
        private void UpdateCategories()
        {
            IEnumerable<object> categories =
                this.RegisteredListeners
                .OfType<IDataProvider>()
                .SelectMany(infoProvider => infoProvider.GetData(this))
                .Distinct();

            if (SortOrder == CategorySortOrder.Ascending)
            {
                categories = categories.OrderBy(category => category);
            }
            else if (SortOrder == CategorySortOrder.Descending)
            {
                categories = categories.OrderByDescending(category => category);
            }

            this.Categories = categories.ToList();

            Invalidate();
        }

        /// <summary>
        /// Returns the major axis grid line coordinates.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>A sequence of the major grid line coordinates.</returns>
        protected override IEnumerable<UnitValue> GetMajorGridLineCoordinates(Size availableSize)
        {
            return GridLineCoordinatesToDisplay;
        }

        /// <summary>
        /// The plot area coordinate of a value.
        /// </summary>
        /// <param name="value">The value for which to retrieve the plot area
        /// coordinate.</param>
        /// <returns>The plot area coordinate.</returns>
        public override UnitValue GetPlotAreaCoordinate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Range<UnitValue> range = GetPlotAreaCoordinateRange(value);
            if (range.HasData)
            {
                double minimum = range.Minimum.Value;
                double maximum = range.Maximum.Value;
                return new UnitValue(((maximum - minimum) / 2.0) + minimum, range.Minimum.Unit);
            }
            else
            {
                return UnitValue.NaN();
            }
        }

        /// <summary>
        /// Creates and prepares a new axis label.
        /// </summary>
        /// <param name="value">The axis label value.</param>
        /// <returns>The axis label content control.</returns>
        private Control CreateAndPrepareAxisLabel(object value)
        {
            Control axisLabel = _labelPool.Next();
            PrepareAxisLabel(axisLabel, value);
            return axisLabel;
        }

        /// <summary>
        /// Renders as an oriented axis.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        private void RenderOriented(Size availableSize)
        {
            _labelPool.Reset();
            _majorTickMarkPool.Reset();

            try
            {
                OrientedPanel.Children.Clear();
                this.GridLineCoordinatesToDisplay.Clear();

                if (this.Categories.Count > 0)
                {
                    double maximumLength = Math.Max(GetLength(availableSize) - 1, 0);

                    Action<double> placeTickMarkAt =
                        (pos) =>
                        {
                            Line tickMark = _majorTickMarkPool.Next();
                            OrientedPanel.SetCenterCoordinate(tickMark, pos);
                            OrientedPanel.SetPriority(tickMark, 0);
                            this.GridLineCoordinatesToDisplay.Add(new UnitValue(pos, Unit.Pixels));
                            OrientedPanel.Children.Add(tickMark);
                        };

                    int index = 0;
                    int priority = 0;

                    foreach (object category in Categories)
                    {
                        Control axisLabel = CreateAndPrepareAxisLabel(category);
                        double lower = ((index * maximumLength) / Categories.Count) + 0.5;
                        double upper = (((index + 1) * maximumLength) / Categories.Count) + 0.5;
                        placeTickMarkAt(lower);
                        OrientedPanel.SetCenterCoordinate(axisLabel, (lower + upper) / 2);
                        OrientedPanel.SetPriority(axisLabel, priority + 1);
                        OrientedPanel.Children.Add(axisLabel);
                        index++;
                        priority = (priority + 1) % 2;
                    }
                    placeTickMarkAt(maximumLength + 0.5);
                }
            }
            finally
            {
                _labelPool.Done();
                _majorTickMarkPool.Done();
            }
        }

        /// <summary>
        /// Renders the axis labels, tick marks, and other visual elements.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        protected override void Render(Size availableSize)
        {
            RenderOriented(availableSize);
        }

        /// <summary>
        /// Returns a value indicating whether a value can be plotted on the
        /// axis.
        /// </summary>
        /// <param name="value">A value which may or may not be able to be
        /// plotted.</param>
        /// <returns>A value indicating whether a value can be plotted on the
        /// axis.</returns>
        public override bool CanPlot(object value)
        {
            return true;
        }
    }
}