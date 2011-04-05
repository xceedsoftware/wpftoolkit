using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls
{
    public class Magnifier : Control
    {
        #region Properties

        #region DefaultSize

        internal Size DefaultSize
        {
            get
            {
                return new Size(2 * Radius, 2 * Radius);
            }
        }

        #endregion //DefaultSize

        #region MagnifierWidth

        public static readonly DependencyProperty MagnifierWidthProperty = DependencyProperty.Register("MagnifierWidth", typeof(double), typeof(Magnifier), new UIPropertyMetadata(50.0));
        internal double MagnifierWidth
        {
            get { return (double)GetValue(MagnifierWidthProperty); }
            set { SetValue(MagnifierWidthProperty, value); }
        }

        #endregion //MagnifierWidth

        #region MagnifierHeight

        public static readonly DependencyProperty MagnifierHeightProperty = DependencyProperty.Register("MagnifierHeight", typeof(double), typeof(Magnifier), new UIPropertyMetadata(50.0));
        internal double MagnifierHeight
        {
            get { return (double)GetValue(MagnifierHeightProperty); }
            set { SetValue(MagnifierHeightProperty, value); }
        }

        #endregion //MagnifierWidth

        #region Radius

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(Magnifier), new FrameworkPropertyMetadata(50.0, new PropertyChangedCallback(OnRadiusPropertyChanged)));
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        private static void OnRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Magnifier m = (Magnifier)d;
            m.OnRadiusChanged(e);
        }

        protected virtual void OnRadiusChanged(DependencyPropertyChangedEventArgs e)
        {
            ResolveViewBox();
        }

        #endregion //Radius

        #region Target

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target", typeof(UIElement), typeof(Magnifier));
        public UIElement Target
        {
            get { return (UIElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        #endregion //Target

        #region ViewBox

        public static readonly DependencyProperty ViewBoxProperty = DependencyProperty.Register("ViewBox", typeof(Rect), typeof(Magnifier), new FrameworkPropertyMetadata(default(Rect)));
        internal Rect ViewBox
        {
            get { return (Rect)GetValue(ViewBoxProperty); }
            set { SetValue(ViewBoxProperty, value); }
        }

        #endregion //ViewBox

        #region ZoomFactor

        public static readonly DependencyProperty ZoomFactorProperty = DependencyProperty.Register("ZoomFactor", typeof(double), typeof(Magnifier), new FrameworkPropertyMetadata(0.5, OnZoomFactorPropertyChanged, OnCoerceZoomFactorProperty));
        public double ZoomFactor
        {
            get { return (double)GetValue(ZoomFactorProperty); }
            set { SetValue(ZoomFactorProperty, value); }
        }

        private static void OnZoomFactorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Magnifier m = (Magnifier)d;
            m.OnZoomFactorChanged(e);
        }

        protected virtual void OnZoomFactorChanged(DependencyPropertyChangedEventArgs e)
        {
            ResolveViewBox();
        }

        private static object OnCoerceZoomFactorProperty(DependencyObject d, object value)
        {
            Magnifier m = (Magnifier)d;
            return m.OnCoerceZoomFactor(value);
        }

        protected virtual object OnCoerceZoomFactor(object value)
        {
            double zoomFactor = (double)value;

            if (zoomFactor > 1)
                zoomFactor = 1;
            else if (zoomFactor < 0)
                zoomFactor = 0;

            return zoomFactor;
        }

        #endregion //ZoomFactor

        #endregion //Properties

        #region Constructors

        /// <summary>
        /// Initializes static members of the <see cref="Magnifier"/> class.
        /// </summary>
        static Magnifier()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Magnifier), new FrameworkPropertyMetadata(typeof(Magnifier)));
        }

        public Magnifier()
        {
            ResolveViewBox();
        }

        #endregion

        #region Methods

        private void ResolveViewBox()
        {
            double correction = (BorderThickness.Bottom + BorderThickness.Left + BorderThickness.Right + BorderThickness.Top == 0) ? 1 : 0;

            double width = DefaultSize.Width * ZoomFactor;
            double height = DefaultSize.Height * ZoomFactor;

            MagnifierWidth = DefaultSize.Width - correction;
            MagnifierHeight = DefaultSize.Height - correction;

            ViewBox = new Rect(ViewBox.Location, new Size(width, height));
        }

        #endregion //Methods

    }
}
