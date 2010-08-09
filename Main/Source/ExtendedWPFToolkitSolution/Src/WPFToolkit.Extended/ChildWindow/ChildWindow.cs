using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    public class ChildWindow : ContentControl
    {
        #region Private Members

        private TranslateTransform _moveTransform = new TranslateTransform();
        private bool _startupPositionInitialized;
        private bool _isMouseCaptured;
        private Point _clickPoint;
        private Point _oldPosition;
        private Border _dragWidget;
        private FrameworkElement _parent;

        #endregion //Private Members

        #region Constructors

        static ChildWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChildWindow), new FrameworkPropertyMetadata(typeof(ChildWindow)));
        }

        public ChildWindow()
        {

        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _dragWidget = (Border)GetTemplateChild("PART_DragWidget");
            if (_dragWidget != null)
            {
                _dragWidget.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(HeaderLeftMouseButtonDown), true);
                _dragWidget.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(HeaderMouseLeftButtonUp), true);
                _dragWidget.MouseMove += (o, e) => HeaderMouseMove(e);
            }

            CloseButton = (Button)GetTemplateChild("PART_CloseButton");
            if (CloseButton != null)
                CloseButton.Click += (o, e) => Close();

            Overlay = GetTemplateChild("PART_Overlay") as Panel;
            WindowRoot = GetTemplateChild("PART_WindowRoot") as Grid;

            WindowRoot.RenderTransform = _moveTransform;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            _parent = VisualTreeHelper.GetParent(this) as FrameworkElement;
            _parent.LayoutUpdated += (o, e) =>
            {
                //we only want to set the start position if this is the first time the control has bee initialized
                if (!_startupPositionInitialized)
                {
                    _startupPositionInitialized = true;
                    SetStartupPosition();
                }
            };
            _parent.SizeChanged += (o, e) =>
            {
                Overlay.Height = e.NewSize.Height;
                Overlay.Width = e.NewSize.Width;
            };

            return base.ArrangeOverride(arrangeBounds);
        }

        #endregion //Base Class Overrides

        #region Properties

        #region Internal Properties

        internal Panel Overlay { get; private set; }
        internal Grid WindowRoot { get; private set; }
        internal Thumb DragWidget { get; private set; }
        internal Button MinimizeButton { get; private set; }
        internal Button MaximizeButton { get; private set; }
        internal Button CloseButton { get; private set; }

        #endregion //Internal Properties

        #region Dependency Properties

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(ChildWindow), new UIPropertyMetadata(String.Empty));
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public static readonly DependencyProperty CaptionForegroundProperty = DependencyProperty.Register("CaptionForeground", typeof(Brush), typeof(ChildWindow), new UIPropertyMetadata(null));
        public Brush CaptionForeground
        {
            get { return (Brush)GetValue(CaptionForegroundProperty); }
            set { SetValue(CaptionForegroundProperty, value); }
        }

        public static readonly DependencyProperty CloseButtonStyleProperty = DependencyProperty.Register("CloseButtonStyle", typeof(Style), typeof(ChildWindow), new PropertyMetadata(null));
        public Style CloseButtonStyle
        {
            get { return (Style)GetValue(CloseButtonStyleProperty); }
            set { SetValue(CloseButtonStyleProperty, value); }
        }

        public static readonly DependencyProperty CloseButtonVisibilityProperty = DependencyProperty.Register("CloseButtonVisibility", typeof(Visibility), typeof(ChildWindow), new PropertyMetadata(Visibility.Visible));
        public Visibility CloseButtonVisibility
        {
            get { return (Visibility)GetValue(CloseButtonVisibilityProperty); }
            set { SetValue(CloseButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(ChildWindow), new UIPropertyMetadata(default(ImageSource)));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }

        #region Left

        public static readonly DependencyProperty LeftProperty = DependencyProperty.Register("Left", typeof(double), typeof(ChildWindow), new PropertyMetadata(0.0, new PropertyChangedCallback(OnLeftPropertyChanged)));
        public double Left
        {
            get { return (double)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        private static void OnLeftPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ChildWindow dialog = (ChildWindow)obj;
            dialog.Left = dialog.GetRestrictedLeft();
            dialog.ProcessMove((double)e.NewValue - (double)e.OldValue, 0);
        }

        #endregion //Left

        #region OverlayBrush

        public static readonly DependencyProperty OverlayBrushProperty = DependencyProperty.Register("OverlayBrush", typeof(Brush), typeof(ChildWindow), new PropertyMetadata(OnOverlayBrushPropertyChanged));
        public Brush OverlayBrush
        {
            get { return (Brush)GetValue(OverlayBrushProperty); }
            set { SetValue(OverlayBrushProperty, value); }
        }

        private static void OnOverlayBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChildWindow cw = (ChildWindow)d;

            if (cw.Overlay != null)
            {
                cw.Overlay.Background = (Brush)e.NewValue;
            }
        }

        #endregion //OverlayBrush

        #region OverlayOpacity

        public static readonly DependencyProperty OverlayOpacityProperty = DependencyProperty.Register("OverlayOpacity", typeof(double), typeof(ChildWindow), new PropertyMetadata(OnOverlayOpacityPropertyChanged));
        public double OverlayOpacity
        {
            get { return (double)GetValue(OverlayOpacityProperty); }
            set { SetValue(OverlayOpacityProperty, value); }
        }

        private static void OnOverlayOpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChildWindow cw = (ChildWindow)d;

            if (cw.Overlay != null)
            {
                cw.Overlay.Opacity = (double)e.NewValue;
            }
        }

        #endregion //OverlayOpacity

        #region Top

        public static readonly DependencyProperty TopProperty = DependencyProperty.Register("Top", typeof(double), typeof(ChildWindow), new PropertyMetadata(0.0, new PropertyChangedCallback(OnTopPropertyChanged)));
        public double Top
        {
            get { return (double)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }

        private static void OnTopPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ChildWindow dialog = (ChildWindow)obj;
            dialog.Top = dialog.GetRestrictedTop();
            dialog.ProcessMove(0, (double)e.NewValue - (double)e.OldValue);
        }

        #endregion //TopProperty

        public static readonly DependencyProperty WindowBackgroundProperty = DependencyProperty.Register("WindowBackground", typeof(Brush), typeof(ChildWindow), new PropertyMetadata(null));
        public Brush WindowBackground
        {
            get { return (Brush)GetValue(WindowBackgroundProperty); }
            set { SetValue(WindowBackgroundProperty, value); }
        }

        public static readonly DependencyProperty WindowBorderBrushProperty = DependencyProperty.Register("WindowBorderBrush", typeof(Brush), typeof(ChildWindow), new PropertyMetadata(null));
        public Brush WindowBorderBrush
        {
            get { return (Brush)GetValue(WindowBorderBrushProperty); }
            set { SetValue(WindowBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty WindowOpacityProperty = DependencyProperty.Register("WindowOpacity", typeof(double), typeof(ChildWindow), new PropertyMetadata(null));
        public double WindowOpacity
        {
            get { return (double)GetValue(WindowOpacityProperty); }
            set { SetValue(WindowOpacityProperty, value); }
        }

        #endregion //Dependency Properties

        #endregion //Properties

        #region Event Handlers

        void HeaderLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Focus();
            _dragWidget.CaptureMouse();
            _isMouseCaptured = true;
            _clickPoint = e.GetPosition(null); //save off the mouse position
            _oldPosition = new Point(Left, Top); //save off our original window position
        }

        private void HeaderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _dragWidget.ReleaseMouseCapture();
            _isMouseCaptured = false;
        }

        private void HeaderMouseMove(MouseEventArgs e)
        {
            if (_isMouseCaptured && Visibility == Visibility.Visible)
            {
                Point currentPosition = e.GetPosition(null); //our current mouse position

                Left = _oldPosition.X + (currentPosition.X - _clickPoint.X);
                Top = _oldPosition.Y + (currentPosition.Y - _clickPoint.Y);

                //this helps keep our mouse position in sync with the drag widget position
                Point dragWidgetPosition = e.GetPosition(_dragWidget);
                if (dragWidgetPosition.X < 0 || dragWidgetPosition.X > _dragWidget.ActualWidth || dragWidgetPosition.Y < 0 || dragWidgetPosition.Y > _dragWidget.ActualHeight)
                {
                    return;
                }

                _oldPosition = new Point(Left, Top);
                _clickPoint = e.GetPosition(Window.GetWindow(this)); //store the point where we are relative to the window
            }
        }

        #endregion //Event Handlers

        #region Methods

        #region Private

        private double GetRestrictedLeft()
        {
            if (_parent != null)
            {
                if (Left < 0)
                {
                    return 0;
                }

                if (Left + WindowRoot.ActualWidth > _parent.ActualWidth)
                {
                    return _parent.ActualWidth - WindowRoot.ActualWidth;
                }
            }

            return Left;
        }

        private double GetRestrictedTop()
        {
            if (_parent != null)
            {
                if (Top < 0)
                {
                    return 0;
                }

                if (Top + WindowRoot.ActualHeight > _parent.ActualHeight)
                {
                    return _parent.ActualHeight - WindowRoot.ActualHeight;
                }
            }

            return Top;
        }

        private void SetStartupPosition()
        {
            CenterChildWindow();
        }

        private void CenterChildWindow()
        {
            _moveTransform.X = _moveTransform.Y = 0;

            if (_parent != null)
            {
                Left = (_parent.ActualWidth - WindowRoot.ActualWidth) / 2.0;
                Top = (_parent.ActualHeight - WindowRoot.ActualHeight) / 2.0;
            }
        }

        #endregion //Private

        #region Protected

        protected void ProcessMove(double x, double y)
        {
            _moveTransform.X += x;
            _moveTransform.Y += y;
        }

        #endregion //Protected

        #region Public

        public void Show()
        {
            Visibility = System.Windows.Visibility.Visible;
        }


        public void Close()
        {
            Visibility = System.Windows.Visibility.Hidden;
        }

        #endregion //Public

        #endregion //Methods
    }
}
