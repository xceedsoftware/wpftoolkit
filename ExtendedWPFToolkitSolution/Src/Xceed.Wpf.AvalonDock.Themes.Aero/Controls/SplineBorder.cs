/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Xceed.Wpf.AvalonDock.Themes.Controls
{
    public class SplineBorder : Control
    {

        public SplineBorder()
        {
            //RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }


        #region Thickness

        /// <summary>
        /// Thickness Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(double), typeof(SplineBorder),
                new FrameworkPropertyMetadata((double)1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the Thickness property.  This dependency property 
        /// indicates the border thickness.
        /// </summary>
        public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        #endregion

        #region Fill

        /// <summary>
        /// Fill Dependency Property
        /// </summary>
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(SplineBorder),
                new FrameworkPropertyMetadata((Brush)null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the Fill property.  This dependency property 
        /// indicates the fill color.
        /// </summary>
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        #endregion

        #region Stroke

        /// <summary>
        /// Stroke Dependency Property
        /// </summary>
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(SplineBorder),
                new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the Stroke property.  This dependency property 
        /// indicates the stroke brush.
        /// </summary>
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        #endregion

        #region BottomBorderMargin

        /// <summary>
        /// BottomBorderMargin Dependency Property
        /// </summary>
        public static readonly DependencyProperty BottomBorderMarginProperty =
            DependencyProperty.Register("BottomBorderMargin", typeof(double), typeof(SplineBorder),
                new FrameworkPropertyMetadata((double)0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the BottomBorderMargin property.  This dependency property 
        /// indicates the adjustment for the bottom margin.
        /// </summary>
        public double BottomBorderMargin
        {
            get { return (double)GetValue(BottomBorderMarginProperty); }
            set { SetValue(BottomBorderMarginProperty, value); }
        }

        #endregion



        protected override void OnRender(DrawingContext drawingContext)
        {
            var pgFill = new PathGeometry();
            var pfFill = new PathFigure() { IsFilled = true, IsClosed = true };
            pfFill.StartPoint = new Point(ActualWidth, 0.0);

            var q1Fill = new QuadraticBezierSegment() { Point1 = new Point(ActualWidth * 2 / 3, 0.0), Point2 = new Point(ActualWidth / 2.0, ActualHeight / 2.0), IsStroked = false };
            pfFill.Segments.Add(q1Fill);
            var q2Fill = new QuadraticBezierSegment() { Point1 = new Point(ActualWidth / 3, ActualHeight ), Point2 = new Point(0, ActualHeight ), IsStroked = false };
            pfFill.Segments.Add(q2Fill);

            pfFill.Segments.Add(new LineSegment() { Point = new Point(ActualWidth, ActualHeight ), IsStroked = false });

            pgFill.Figures.Add(pfFill);

            drawingContext.DrawGeometry(Fill, null, pgFill);

            var pgBorder = new PathGeometry();
            var pfBorder = new PathFigure() { IsFilled = false, IsClosed = false };
            pfBorder.StartPoint = new Point(ActualWidth, Thickness / 2);

            var q1Border = new QuadraticBezierSegment() { Point1 = new Point(ActualWidth * 2 / 3, 0.0), Point2 = new Point(ActualWidth / 2.0, ActualHeight / 2.0) };
            pfBorder.Segments.Add(q1Border);
            var q2Border = new QuadraticBezierSegment() { Point1 = new Point(ActualWidth / 3, ActualHeight), Point2 = new Point(0.0, ActualHeight - BottomBorderMargin) };
            pfBorder.Segments.Add(q2Border);

            pgBorder.Figures.Add(pfBorder);

            drawingContext.DrawGeometry(null, new Pen(Stroke, Thickness), pgBorder);

            base.OnRender(drawingContext);
        }
    }
}
