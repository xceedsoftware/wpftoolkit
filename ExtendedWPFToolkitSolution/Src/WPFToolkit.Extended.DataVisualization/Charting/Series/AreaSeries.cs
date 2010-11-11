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
    [StyleTypedProperty(Property = DataPointStyleName, StyleTargetType = typeof(AreaDataPoint))]
    [StyleTypedProperty(Property = "LegendItemStyle", StyleTargetType = typeof(LegendItem))]
    [StyleTypedProperty(Property = "PathStyle", StyleTargetType = typeof(Path))]
    [TemplatePart(Name = DataPointSeries.PlotAreaName, Type = typeof(Canvas))]
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Depth of hierarchy is necessary to avoid code duplication.")]
    public partial class AreaSeries : LineAreaBaseSeries<AreaDataPoint>, IAnchoredToOrigin
    {
        #region public Geometry Geometry
        /// <summary>
        /// Gets the geometry property.
        /// </summary>
        public Geometry Geometry
        {
            get { return GetValue(GeometryProperty) as Geometry; }
            private set { SetValue(GeometryProperty, value); }
        }

        /// <summary>
        /// Identifies the Geometry dependency property.
        /// </summary>
        public static readonly DependencyProperty GeometryProperty =
            DependencyProperty.Register(
                "Geometry",
                typeof(Geometry),
                typeof(AreaSeries),
                null);
        #endregion public Geometry Geometry

        #region public Style PathStyle
        /// <summary>
        /// Gets or sets the style of the Path object that follows the data 
        /// points.
        /// </summary>
        public Style PathStyle
        {
            get { return GetValue(PathStyleProperty) as Style; }
            set { SetValue(PathStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the PathStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty PathStyleProperty =
            DependencyProperty.Register(
                "PathStyle",
                typeof(Style),
                typeof(AreaSeries),
                null);
        #endregion public Style PathStyle

#if !SILVERLIGHT
        /// <summary>
        /// Initializes the static members of the AreaSeries class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Dependency properties are initialized in-line.")]
        static AreaSeries()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AreaSeries), new FrameworkPropertyMetadata(typeof(AreaSeries)));
        }

#endif
        /// <summary>
        /// Initializes a new instance of the AreaSeries class.
        /// </summary>
        public AreaSeries()
        {
#if SILVERLIGHT
            this.DefaultStyleKey = typeof(AreaSeries);
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
                (axis) => 
                    {
                        IRangeAxis rangeAxis = axis as IRangeAxis;
                        return rangeAxis != null && rangeAxis.Origin != null && axis.Orientation == AxisOrientation.Y;
                    },
                () =>
                {
                    DisplayAxis axis = (DisplayAxis)CreateRangeAxisFromData(firstDataPoint.DependentValue);
                    if (axis == null || (axis as IRangeAxis).Origin == null)
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
            UnitValue originCoordinate = ActualDependentRangeAxis.GetPlotAreaCoordinate(ActualDependentRangeAxis.Origin);
            UnitValue maximumCoordinate = ActualDependentRangeAxis.GetPlotAreaCoordinate(ActualDependentRangeAxis.Range.Maximum);
            if (points.Any() && ValueHelper.CanGraph(originCoordinate.Value) && ValueHelper.CanGraph(maximumCoordinate.Value))
            {
                double originY = Math.Floor(originCoordinate.Value);
                PathFigure figure = new PathFigure();
                figure.IsClosed = true;
                figure.IsFilled = true;

                double maximum = maximumCoordinate.Value;
                Point startPoint;
                IEnumerator<Point> pointEnumerator = points.GetEnumerator();
                pointEnumerator.MoveNext();
                startPoint = new Point(pointEnumerator.Current.X, maximum - originY);
                figure.StartPoint = startPoint;

                Point lastPoint;
                do
                {
                    lastPoint = pointEnumerator.Current;
                    figure.Segments.Add(new LineSegment { Point = pointEnumerator.Current });
                }
                while (pointEnumerator.MoveNext());
                figure.Segments.Add(new LineSegment { Point = new Point(lastPoint.X, maximum - originY) });

                if (figure.Segments.Count > 1)
                {
                    PathGeometry geometry = new PathGeometry();
                    geometry.Figures.Add(figure);
                    Geometry = geometry;
                    return;
                }
            }
            else
            {
                Geometry = null;
            }
        }

        /// <summary>
        /// Remove value margins from the side of the data points to ensure
        /// that area chart is flush against the edge of the chart.
        /// </summary>
        /// <param name="consumer">The value margin consumer.</param>
        /// <returns>A sequence of value margins.</returns>
        protected override IEnumerable<ValueMargin> GetValueMargins(IValueMarginConsumer consumer)
        {
            if (consumer == ActualIndependentAxis)
            {
                return Enumerable.Empty<ValueMargin>();
            }
            return base.GetValueMargins(consumer);
        }

        /// <summary>
        /// Gets the axis to which the series is anchored.
        /// </summary>
        IRangeAxis IAnchoredToOrigin.AnchoredAxis
        {
            get { return AnchoredAxis; }
        }

        /// <summary>
        /// Gets the axis to which the series is anchored.
        /// </summary>
        protected IRangeAxis AnchoredAxis
        {
            get { return ActualDependentRangeAxis; }
        }
    }
}

#endif
