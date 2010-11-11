// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;

#if !DEFINITION_SERIES_COMPATIBILITY_MODE

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Represents a control that contains a data series to be rendered in X/Y 
    /// line format.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [StyleTypedProperty(Property = DataPointStyleName, StyleTargetType = typeof(LineDataPoint))]
    [StyleTypedProperty(Property = "LegendItemStyle", StyleTargetType = typeof(LegendItem))]
    [StyleTypedProperty(Property = "PolylineStyle", StyleTargetType = typeof(Polyline))]
    [TemplatePart(Name = DataPointSeries.PlotAreaName, Type = typeof(Canvas))]
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Depth of hierarchy is necessary to avoid code duplication.")]
    public partial class LineSeries : LineAreaBaseSeries<LineDataPoint>
    {
        #region public PointCollection Points
        /// <summary>
        /// Gets the collection of points that make up the line.
        /// </summary>
        public PointCollection Points
        {
            get { return GetValue(PointsProperty) as PointCollection; }
            private set { SetValue(PointsProperty, value); }
        }

        /// <summary>
        /// Identifies the Points dependency property.
        /// </summary>
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(
                "Points",
                typeof(PointCollection),
                typeof(LineSeries),
                null);
        #endregion public PointCollection Points

        #region public Style PolylineStyle
        /// <summary>
        /// Gets or sets the style of the Polyline object that follows the data 
        /// points.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Polyline", Justification = "Matches System.Windows.Shapes.Polyline.")]
        public Style PolylineStyle
        {
            get { return GetValue(PolylineStyleProperty) as Style; }
            set { SetValue(PolylineStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the PolylineStyle dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Polyline", Justification = "Matches System.Windows.Shapes.Polyline.")]
        public static readonly DependencyProperty PolylineStyleProperty =
            DependencyProperty.Register(
                "PolylineStyle",
                typeof(Style),
                typeof(LineSeries),
                null);
        #endregion public Style PolylineStyle

#if !SILVERLIGHT
        /// <summary>
        /// Initializes the static members of the LineSeries class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Dependency properties are initialized in-line.")]
        static LineSeries()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineSeries), new FrameworkPropertyMetadata(typeof(LineSeries)));
        }

#endif
        /// <summary>
        /// Initializes a new instance of the LineSeries class.
        /// </summary>
        public LineSeries()
        {
#if SILVERLIGHT
            this.DefaultStyleKey = typeof(LineSeries);
#endif
        }

        /// <summary>
        /// Acquire a horizontal linear axis and a vertical linear axis.
        /// </summary>
        /// <param name="firstDataPoint">The first data point.</param>
        protected override void GetAxes(DataPoint firstDataPoint)
        {
            GetAxes(
                firstDataPoint,
                (axis) => axis.Orientation == AxisOrientation.X,
                () =>
                {
                    IAxis axis = CreateRangeAxisFromData(firstDataPoint.IndependentValue);
                    if (axis == null)
                    {
                        axis = new CategoryAxis();
                    }
                    axis.Orientation = AxisOrientation.X;
                    return axis;
                },
                (axis) => axis.Orientation == AxisOrientation.Y && axis is IRangeAxis,
                () =>
                {
                    DisplayAxis axis = (DisplayAxis)CreateRangeAxisFromData(firstDataPoint.DependentValue);
                    if (axis == null)
                    {
                        throw new InvalidOperationException(Properties.Resources.DataPointSeriesWithAxes_NoSuitableAxisAvailableForPlottingDependentValue);
                    }
                    axis.ShowGridLines = true;
                    axis.Orientation = AxisOrientation.Y;
                    return axis;
                });
        }

        /// <summary>
        /// Updates the Series shape object from a collection of Points.
        /// </summary>
        /// <param name="points">Collection of Points.</param>
        protected override void UpdateShapeFromPoints(IEnumerable<Point> points)
        {
            if (points.Any())
            {
                PointCollection pointCollection = new PointCollection();
                foreach (Point point in points)
                {
                    pointCollection.Add(point);
                }
                Points = pointCollection;
            }
            else
            {
                Points = null;
            }
        }
    }
}

#endif
