// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// An axis that has a range.
    /// </summary>
    public abstract class DisplayAxis : Axis, IRequireSeriesHost
    {
        /// <summary>
        /// Maximum intervals per 200 pixels.
        /// </summary>
        protected const double MaximumAxisIntervalsPer200Pixels = 8;

        /// <summary>
        /// The name of the axis grid template part.
        /// </summary>
        protected const string AxisGridName = "AxisGrid";

        /// <summary>
        /// The name of the axis title template part.
        /// </summary>
        protected const string AxisTitleName = "AxisTitle";

        #region public Style AxisLabelStyle
        /// <summary>
        /// Gets or sets the style used for the axis labels.
        /// </summary>
        public Style AxisLabelStyle
        {
            get { return GetValue(AxisLabelStyleProperty) as Style; }
            set { SetValue(AxisLabelStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the AxisLabelStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty AxisLabelStyleProperty =
            DependencyProperty.Register(
                "AxisLabelStyle",
                typeof(Style),
                typeof(DisplayAxis),
                new PropertyMetadata(null, OnAxisLabelStylePropertyChanged));

        /// <summary>
        /// AxisLabelStyleProperty property changed handler.
        /// </summary>
        /// <param name="d">DisplayAxis that changed its AxisLabelStyle.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnAxisLabelStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayAxis source = (DisplayAxis)d;
            Style oldValue = (Style)e.OldValue;
            Style newValue = (Style)e.NewValue;
            source.OnAxisLabelStylePropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// AxisLabelStyleProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>        
        protected virtual void OnAxisLabelStylePropertyChanged(Style oldValue, Style newValue)
        {
        }
        #endregion public Style AxisLabelStyle

        /// <summary>
        /// Gets the actual length.
        /// </summary>
        protected double ActualLength
        {
            get
            {
                return GetLength(new Size(this.ActualWidth, this.ActualHeight));
            }
        }

        /// <summary>
        /// Returns the length of the axis given an available size.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The length of the axis given an available size.</returns>
        protected double GetLength(Size availableSize)
        {
            if (this.ActualHeight == 0.0 && this.ActualWidth == 0.0)
            {
                return 0.0;
            }
            if (this.Orientation == AxisOrientation.X)
            {
                return availableSize.Width;
            }
            else if (this.Orientation == AxisOrientation.Y)
            {
                return availableSize.Height;
            }
            else
            {
                throw new InvalidOperationException(Properties.Resources.DisplayAxis_GetLength_CannotDetermineTheLengthOfAnAxisWithAnOrientationOfNone);
            }
        }

        #region private GridLines GridLines

        /// <summary>
        /// This field stores the grid lines element.
        /// </summary>
        private DisplayAxisGridLines _gridLines;

        /// <summary>
        /// Gets or sets the grid lines property.
        /// </summary>
        internal DisplayAxisGridLines GridLines
        {
            get { return _gridLines; }
            set 
            { 
                if (value != _gridLines)
                {
                    DisplayAxisGridLines oldValue = _gridLines;
                    _gridLines = value;
                    OnGridLinesPropertyChanged(oldValue, value);
                }
            }
        }

        /// <summary>
        /// GridLinesProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>        
        private void OnGridLinesPropertyChanged(DisplayAxisGridLines oldValue, DisplayAxisGridLines newValue)
        {
            if (SeriesHost != null && oldValue != null)
            {
                SeriesHost.BackgroundElements.Remove(oldValue);
            }
            if (SeriesHost != null && newValue != null)
            {
                SeriesHost.BackgroundElements.Add(newValue);
            }
        }
        #endregion private GridLines GridLines

        #region public Style MajorTickMarkStyle
        /// <summary>
        /// Gets or sets the style applied to the Axis tick marks.
        /// </summary>
        /// <value>The Style applied to the Axis tick marks.</value>
        public Style MajorTickMarkStyle
        {
            get { return GetValue(MajorTickMarkStyleProperty) as Style; }
            set { SetValue(MajorTickMarkStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the MajorTickMarkStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty MajorTickMarkStyleProperty =
            DependencyProperty.Register(
                "MajorTickMarkStyle",
                typeof(Style),
                typeof(DisplayAxis),
                new PropertyMetadata(null, OnMajorTickMarkStylePropertyChanged));

        /// <summary>
        /// MajorTickMarkStyleProperty property changed handler.
        /// </summary>
        /// <param name="d">DisplayAxis that changed its MajorTickMarkStyle.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnMajorTickMarkStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayAxis source = (DisplayAxis)d;
            Style oldValue = (Style)e.OldValue;
            Style newValue = (Style)e.NewValue;
            source.OnMajorTickMarkStylePropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// MajorTickMarkStyleProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>        
        protected virtual void OnMajorTickMarkStylePropertyChanged(Style oldValue, Style newValue)
        {
        }
        #endregion public Style MajorTickMarkStyle

        #region public object Title
        /// <summary>
        /// Gets or sets the title property.
        /// </summary>
        public object Title
        {
            get { return GetValue(TitleProperty) as object; }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(object),
                typeof(DisplayAxis),
                new PropertyMetadata(null, OnTitlePropertyChanged));

        /// <summary>
        /// TitleProperty property changed handler.
        /// </summary>
        /// <param name="d">DisplayAxis that changed its Title.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnTitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayAxis source = (DisplayAxis)d;
            object oldValue = (object)e.OldValue;
            object newValue = (object)e.NewValue;
            source.OnTitlePropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// TitleProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>        
        protected virtual void OnTitlePropertyChanged(object oldValue, object newValue)
        {
            if (this.AxisTitle != null)
            {
                this.AxisTitle.Content = Title;
            }
        }
        #endregion public object Title

        /// <summary>
        /// Gets or sets the LayoutTransformControl used to rotate the title.
        /// </summary>
        private LayoutTransformControl TitleLayoutTransformControl { get; set; }

        #region public Style TitleStyle
        /// <summary>
        /// Gets or sets the style applied to the Axis title.
        /// </summary>
        /// <value>The Style applied to the Axis title.</value>
        public Style TitleStyle
        {
            get { return GetValue(TitleStyleProperty) as Style; }
            set { SetValue(TitleStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the TitleStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleStyleProperty =
            DependencyProperty.Register(
                "TitleStyle",
                typeof(Style),
                typeof(DisplayAxis),
                null);
        #endregion

        #region public bool ShowGridLines
        /// <summary>
        /// Gets or sets a value indicating whether grid lines should be shown.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GridLines", Justification = "This is the expected casing.")]
        public bool ShowGridLines
        {
            get { return (bool)GetValue(ShowGridLinesProperty); }
            set { SetValue(ShowGridLinesProperty, value); }
        }

        /// <summary>
        /// Identifies the ShowGridLines dependency property.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GridLines", Justification = "This is the expected capitalization.")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GridLine", Justification = "This is the expected capitalization.")]
        public static readonly DependencyProperty ShowGridLinesProperty =
            DependencyProperty.Register(
                "ShowGridLines",
                typeof(bool),
                typeof(DisplayAxis),
                new PropertyMetadata(false, OnShowGridLinesPropertyChanged));

        /// <summary>
        /// ShowGridLinesProperty property changed handler.
        /// </summary>
        /// <param name="d">Axis that changed its ShowGridLines.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnShowGridLinesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayAxis source = (DisplayAxis)d;
            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            source.OnShowGridLinesPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// ShowGridLinesProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>        
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GridLine", Justification = "This is the expected capitalization.")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GridLines", Justification = "This is the expected capitalization.")]
        protected virtual void OnShowGridLinesPropertyChanged(bool oldValue, bool newValue)
        {
            SetShowGridLines(newValue);
        }
        #endregion public bool ShowGridLines

        /// <summary>
        /// Creates and destroys a grid lines element based on the specified
        /// value.
        /// </summary>
        /// <param name="newValue">A value indicating whether to display grid 
        /// lines or not.</param>
        private void SetShowGridLines(bool newValue)
        {
            if (newValue == true)
            {
                this.GridLines = new OrientedAxisGridLines(this);
            }
            else
            {
                this.GridLines = null;
            }
        }

        #region public Style GridLineStyle
        /// <summary>
        /// Gets or sets the Style of the Axis's gridlines.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GridLine", Justification = "Current casing is the expected one.")]
        public Style GridLineStyle
        {
            get { return GetValue(GridLineStyleProperty) as Style; }
            set { SetValue(GridLineStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the GridlineStyle dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GridLine", Justification = "Current casing is the expected one.")]
        public static readonly DependencyProperty GridLineStyleProperty =
            DependencyProperty.Register(
                "GridLineStyle",
                typeof(Style),
                typeof(DisplayAxis),
                null);
        #endregion

        /// <summary>
        /// The grid used to layout the axis.
        /// </summary>
        private Grid _grid;

        /// <summary>
        /// Gets or sets the grid used to layout the axis.
        /// </summary>
        private Grid AxisGrid
        {
            get
            {
                return _grid;
            }
            set
            {
                if (_grid != value)
                {
                    if (_grid != null)
                    {
                        _grid.Children.Clear();
                    }

                    _grid = value;

                    if (_grid != null)
                    {
                        _grid.Children.Add(this.OrientedPanel);
                        if (this.AxisTitle != null)
                        {
                            _grid.Children.Add(this.AxisTitle);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a grid to lay out the dependent axis.
        /// </summary>
        private Grid DependentAxisGrid { get; set; }

        /// <summary>
        /// Gets the oriented panel used to layout the axis labels.
        /// </summary>
        internal OrientedPanel OrientedPanel { get; private set; }

        /// <summary>
        /// The control used to display the axis title.
        /// </summary>
        private Title _axisTitle;

        /// <summary>
        /// Gets or sets the title control used to display the title.
        /// </summary>
        private Title AxisTitle
        {
            get
            {
                return _axisTitle;
            }
            set
            {
                if (_axisTitle != value)
                {
                    if (_axisTitle != null)
                    {
                        _axisTitle.Content = null;
                    }

                    _axisTitle = value;
                    if (Title != null)
                    {
                        _axisTitle.Content = Title;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a major axis tick mark.
        /// </summary>
        /// <returns>A line to used to render a tick mark.</returns>
        protected virtual Line CreateMajorTickMark()
        {
            return CreateTickMark(MajorTickMarkStyle);
        }

        /// <summary>
        /// Creates a tick mark and applies a style to it.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The newly created tick mark.</returns>
        protected Line CreateTickMark(Style style)
        {
            Line line = new Line();
            line.Style = style;
            if (this.Orientation == AxisOrientation.Y)
            {
                line.Y1 = 0.5;
                line.Y2 = 0.5;
            }
            else if (this.Orientation == AxisOrientation.X)
            {
                line.X1 = 0.5;
                line.X2 = 0.5;
            }
            return line;
        }

        /// <summary>
        /// This method is used to share the grid line coordinates with the
        /// internal grid lines control.
        /// </summary>
        /// <returns>A sequence of the major grid line coordinates.</returns>
        internal IEnumerable<UnitValue> InternalGetMajorGridLinePositions()
        {
            return GetMajorGridLineCoordinates(new Size(this.ActualWidth, this.ActualHeight));
        }
        
        /// <summary>
        /// Returns the coordinates to use for the grid line control.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>A sequence of coordinates at which to draw grid lines.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GridLine", Justification = "This is the expected capitalization.")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Returns the coordinates of the grid lines.")]
        protected abstract IEnumerable<UnitValue> GetMajorGridLineCoordinates(Size availableSize);

#if !SILVERLIGHT
        /// <summary>
        /// Initializes the static members of the DisplayAxis class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Dependency properties are initialized in-line.")]
        static DisplayAxis()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DisplayAxis), new FrameworkPropertyMetadata(typeof(DisplayAxis)));
        }

#endif
        /// <summary>
        /// Instantiates a new instance of the DisplayAxis class.
        /// </summary>
        protected DisplayAxis()
        {
            this.OrientedPanel = new OrientedPanel();
#if SILVERLIGHT
            this.DefaultStyleKey = typeof(DisplayAxis);
            this.OrientedPanel.UseLayoutRounding = true;
#endif

            this.DependentAxisGrid = new Grid();

            this.TitleLayoutTransformControl = new LayoutTransformControl();
            this.TitleLayoutTransformControl.HorizontalAlignment = HorizontalAlignment.Center;
            this.TitleLayoutTransformControl.VerticalAlignment = VerticalAlignment.Center;

            this.SizeChanged += new SizeChangedEventHandler(DisplayAxisSizeChanged);
        }

        /// <summary>
        /// If display axis has just become visible, invalidate.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void DisplayAxisSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width == 0.0 && e.PreviousSize.Height == 0.0)
            {
                Invalidate();
            }
        }

        /// <summary>
        /// Creates an axis label.
        /// </summary>
        /// <returns>The new axis label.</returns>
        protected virtual Control CreateAxisLabel()
        {
            return new AxisLabel();
        }

        /// <summary>
        /// Updates the grid lines element if a suitable dependent axis has
        /// been added to a radial axis.
        /// </summary>
        protected override void OnDependentAxesCollectionChanged()
        {
            SetShowGridLines(ShowGridLines);
            base.OnDependentAxesCollectionChanged();
        }

        /// <summary>
        /// Prepares an axis label to be plotted.
        /// </summary>
        /// <param name="label">The axis label to prepare.</param>
        /// <param name="dataContext">The data context to use for the axis 
        /// label.</param>
        protected virtual void PrepareAxisLabel(Control label, object dataContext)
        {
            label.DataContext = dataContext;
            label.SetStyle(AxisLabelStyle);
        }

        /// <summary>
        /// Retrieves template parts and configures layout.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.AxisGrid = GetTemplateChild(AxisGridName) as Grid;
            this.AxisTitle = GetTemplateChild(AxisTitleName) as Title;
            if (this.AxisTitle != null && this.AxisGrid.Children.Contains(this.AxisTitle))
            {
                this.AxisGrid.Children.Remove(this.AxisTitle);
                this.TitleLayoutTransformControl.Child = this.AxisTitle;
                this.AxisGrid.Children.Add(this.TitleLayoutTransformControl);
            }

            ArrangeAxisGrid();
        }

        /// <summary>
        /// When the size of the oriented panel changes invalidate the axis.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void OnOrientedPanelSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Invalidate();
        }

        /// <summary>
        /// Arranges the grid when the location property is changed.
        /// </summary>
        /// <param name="oldValue">The old location.</param>
        /// <param name="newValue">The new location.</param>
        protected override void OnLocationPropertyChanged(AxisLocation oldValue, AxisLocation newValue)
        {
            ArrangeAxisGrid();
            base.OnLocationPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// Arranges the elements in the axis grid.
        /// </summary>
        private void ArrangeAxisGrid()
        {
            if (this.AxisGrid != null)
            {
                this.AxisGrid.ColumnDefinitions.Clear();
                this.AxisGrid.RowDefinitions.Clear();
                this.AxisGrid.Children.Clear();

                if (this.Orientation == AxisOrientation.Y)
                {
                    this.OrientedPanel.Orientation = System.Windows.Controls.Orientation.Vertical;
                    this.OrientedPanel.IsReversed = true;

                    if (this.Location == AxisLocation.Left || this.Location == AxisLocation.Right)
                    {
                        this.TitleLayoutTransformControl.Transform = new RotateTransform { Angle = -90.0 };

                        this.OrientedPanel.IsInverted = !(Location == AxisLocation.Right);
                        this.AxisGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        this.AxisGrid.RowDefinitions.Add(new RowDefinition());

                        int column = 0;
                        if (this.AxisTitle != null)
                        {
                            this.AxisGrid.ColumnDefinitions.Add(new ColumnDefinition());
                            Grid.SetRow(this.TitleLayoutTransformControl, 0);
                            Grid.SetColumn(this.TitleLayoutTransformControl, 0);
                            column++;
                        }
                        Grid.SetRow(this.OrientedPanel, 0);
                        Grid.SetColumn(this.OrientedPanel, column);

                        this.AxisGrid.Children.Add(this.TitleLayoutTransformControl);
                        this.AxisGrid.Children.Add(this.OrientedPanel);

                        if (this.Location == AxisLocation.Right)
                        {
                            AxisGrid.Mirror(System.Windows.Controls.Orientation.Vertical);
                            this.TitleLayoutTransformControl.Transform = new RotateTransform { Angle = 90 };
                        }
                    }
                }
                else if (this.Orientation == AxisOrientation.X)
                {
                    this.OrientedPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                    this.OrientedPanel.IsReversed = false;

                    if (this.Location == AxisLocation.Top || this.Location == AxisLocation.Bottom)
                    {
                        this.OrientedPanel.IsInverted = (Location == AxisLocation.Top);
                        this.TitleLayoutTransformControl.Transform = new RotateTransform { Angle = 0 };

                        this.AxisGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        this.AxisGrid.RowDefinitions.Add(new RowDefinition());

                        if (this.AxisTitle != null)
                        {
                            this.AxisGrid.RowDefinitions.Add(new RowDefinition());
                            Grid.SetColumn(this.TitleLayoutTransformControl, 0);
                            Grid.SetRow(this.TitleLayoutTransformControl, 1);
                        }

                        Grid.SetColumn(this.OrientedPanel, 0);
                        Grid.SetRow(this.OrientedPanel, 0);

                        this.AxisGrid.Children.Add(this.TitleLayoutTransformControl);
                        this.AxisGrid.Children.Add(this.OrientedPanel);

                        if (this.Location == AxisLocation.Top)
                        {
                            AxisGrid.Mirror(System.Windows.Controls.Orientation.Horizontal);
                        }
                    }
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Renders the axis.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The required size.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            RenderAxis(availableSize);
            return base.MeasureOverride(availableSize);
        }

        /// <summary>
        /// Reformulates the grid when the orientation is changed.  Grid is
        /// either separated into two columns or two rows.  The title is 
        /// inserted with the outermost section from the edge and an oriented
        /// panel is inserted into the innermost section.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnOrientationPropertyChanged(AxisOrientation oldValue, AxisOrientation newValue)
        {
            ArrangeAxisGrid();
            base.OnOrientationPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// Updates the visual appearance of the axis when it is invalidated.
        /// </summary>
        /// <param name="args">Information for the invalidated event.</param>
        protected override void OnInvalidated(RoutedEventArgs args)
        {
            InvalidateMeasure();
            base.OnInvalidated(args);
        }

        /// <summary>
        /// Renders the axis if there is a valid value for orientation.
        /// </summary>
        /// <param name="availableSize">The available size in which to render 
        /// the axis.</param>
        private void RenderAxis(Size availableSize)
        {
            if (Orientation != AxisOrientation.None && Location != AxisLocation.Auto)
            {
                Render(availableSize);
            }
        }

        /// <summary>
        /// Renders the axis labels, tick marks, and other visual elements.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        protected abstract void Render(Size availableSize);

        /// <summary>
        /// Invalidates the axis.
        /// </summary>
        protected void Invalidate()
        {
            OnInvalidated(new RoutedEventArgs());
        }

        /// <summary>
        /// The series host.
        /// </summary>
        private ISeriesHost _seriesHost;

        /// <summary>
        /// Gets or sets the series host.
        /// </summary>
        public ISeriesHost SeriesHost
        {
            get
            {
                return _seriesHost;
            }
            set
            {
                if (value != _seriesHost)
                {
                    ISeriesHost oldValue = _seriesHost;
                    _seriesHost = value;
                    OnSeriesHostPropertyChanged(oldValue, value);
                }
            }
        }

        /// <summary>
        /// This method is run when the series host property is changed.
        /// </summary>
        /// <param name="oldValue">The old series host.</param>
        /// <param name="newValue">The new series host.</param>
        protected virtual void OnSeriesHostPropertyChanged(ISeriesHost oldValue, ISeriesHost newValue)
        {
            if (oldValue != null && this.GridLines != null)
            {
                oldValue.BackgroundElements.Remove(this.GridLines);
            }

            if (newValue != null && this.GridLines != null)
            {
                newValue.BackgroundElements.Add(this.GridLines);
            }
        }
    }
}