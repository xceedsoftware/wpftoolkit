using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Windows.Shapes;

namespace Microsoft.Windows.Controls
{
    [TemplateVisualState(GroupName = VisualStates.WindowStatesGroup, Name = VisualStates.Open)]
    [TemplateVisualState(GroupName = VisualStates.WindowStatesGroup, Name = VisualStates.Closed)]
    public class ChildWindow : ContentControl
    {
        #region Private Members

        private Grid _root;
        private TranslateTransform _moveTransform = new TranslateTransform();
        private bool _startupPositionInitialized;
        private bool _isMouseCaptured;
        private Point _clickPoint;
        private Point _oldPosition;
        private Border _dragWidget;
        private FrameworkElement _parentContainer;
        private Rectangle _modalLayer = new Rectangle();
        private Canvas _modalLayerPanel = new Canvas();

        #endregion //Private Members

        #region Constructors

        static ChildWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChildWindow), new FrameworkPropertyMetadata(typeof(ChildWindow)));
        }

        public ChildWindow()
        {
            _modalLayer.Fill = new SolidColorBrush(Colors.Gray);
            _modalLayer.Opacity = 0.5;
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

            //Overlay = GetTemplateChild("PART_Overlay") as Panel;
            WindowRoot = GetTemplateChild("PART_WindowRoot") as Grid;

            WindowRoot.RenderTransform = _moveTransform;


            _parentContainer = VisualTreeHelper.GetParent(this) as FrameworkElement;

            //this is for XBAP applications only. When inside an XBAP the parent container has no height or width until it has loaded. Therefore
            //we need to handle the loaded event and reposition the window.
            if (System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted)
            {
                _parentContainer.Loaded += (o, e) =>
                    {
                        CenterChildWindow();
                    };
            }

            _parentContainer.LayoutUpdated += ParentContainer_LayoutUpdated;
            _parentContainer.SizeChanged += (o, ea) =>
            {
                _modalLayer.Height = ea.NewSize.Height;
                _modalLayer.Width = ea.NewSize.Width;
            };

            _root = GetTemplateChild("Root") as Grid;
            _root.Children.Add(_modalLayerPanel);

            ChangeVisualState();
        }



        #endregion //Base Class Overrides

        #region Properties

        #region Internal Properties

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

        public static readonly DependencyProperty IsModalProperty = DependencyProperty.Register("IsModal", typeof(bool), typeof(ChildWindow), new UIPropertyMetadata(false, new PropertyChangedCallback(OnIsModalPropertyChanged)));
        public bool IsModal
        {
            get { return (bool)GetValue(IsModalProperty); }
            set { SetValue(IsModalProperty, value); }
        }

        private static void OnIsModalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChildWindow childWindow = (ChildWindow)d;
            if ((bool)e.NewValue)
                childWindow.ShowModalLayer();
            else
                childWindow.HideModalLayer();
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
            ChildWindow window = (ChildWindow)obj;
            window.Left = window.GetRestrictedLeft();
            window.ProcessMove((double)e.NewValue - (double)e.OldValue, 0);
        }

        #endregion //Left

        #region OverlayBrush

        public static readonly DependencyProperty OverlayBrushProperty = DependencyProperty.Register("OverlayBrush", typeof(Brush), typeof(ChildWindow));
        public Brush OverlayBrush
        {
            get { return (Brush)GetValue(OverlayBrushProperty); }
            set { SetValue(OverlayBrushProperty, value); }
        }

        #endregion //OverlayBrush

        #region OverlayOpacity

        public static readonly DependencyProperty OverlayOpacityProperty = DependencyProperty.Register("OverlayOpacity", typeof(double), typeof(ChildWindow));
        public double OverlayOpacity
        {
            get { return (double)GetValue(OverlayOpacityProperty); }
            set { SetValue(OverlayOpacityProperty, value); }
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
            ChildWindow window = (ChildWindow)obj;
            window.Top = window.GetRestrictedTop();
            window.ProcessMove(0, (double)e.NewValue - (double)e.OldValue);
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

        #region WindowState

        public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register("WindowState", typeof(WindowState), typeof(ChildWindow), new PropertyMetadata(WindowState.Open, new PropertyChangedCallback(OnWindowStatePropertyChanged)));
        public WindowState WindowState
        {
            get { return (WindowState)GetValue(WindowStateProperty); }
            set { SetValue(WindowStateProperty, value); }
        }

        private static void OnWindowStatePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ChildWindow window = (ChildWindow)obj;
            window.SetWindowState((WindowState)e.NewValue);
        }

        #endregion //WindowState

        #endregion //Dependency Properties

        private bool? _dialogResult;
        /// <summary>
        /// Gets or sets a value indicating whether the ChildWindow was accepted or canceled.
        /// </summary>
        /// <value>
        /// True if the child window was accepted; false if the child window was
        /// canceled. The default is null.
        /// </value>
        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? DialogResult
        {
            get { return _dialogResult; }
            set
            {
                if (_dialogResult != value)
                {
                    _dialogResult = value;
                    Close();
                }
            }
        }

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

        private void ParentContainer_LayoutUpdated(object sender, EventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            //we only want to set the start position if this is the first time the control has bee initialized
            if (!_startupPositionInitialized)
            {
                SetStartupPosition();
                _startupPositionInitialized = true;
            }
        }

        #endregion //Event Handlers

        #region Methods

        #region Private

        private double GetRestrictedLeft()
        {
            if (_parentContainer != null)
            {
                if (Left < 0)
                {
                    return 0;
                }

                if (Left + WindowRoot.ActualWidth > _parentContainer.ActualWidth && _parentContainer.ActualWidth != 0)
                {
                    return _parentContainer.ActualWidth - WindowRoot.ActualWidth;
                }
            }

            return Left;
        }

        private double GetRestrictedTop()
        {
            if (_parentContainer != null )
            {
                if (Top < 0)
                {
                    return 0;
                }

                if (Top + WindowRoot.ActualHeight > _parentContainer.ActualHeight && _parentContainer.ActualHeight != 0)
                {
                    return _parentContainer.ActualHeight - WindowRoot.ActualHeight;
                }
            }

            return Top;
        }

        private void SetWindowState(WindowState state)
        {
            switch (state)
            {
                case WindowState.Closed:
                    {
                        ExecuteClose();
                        break;
                    }
                case WindowState.Open:
                    {
                        ExecuteOpen();
                        break;
                    }
            }

            ChangeVisualState();
        }

        private void ExecuteClose()
        {
            CancelEventArgs e = new CancelEventArgs();
            OnClosing(e);

            if (!e.Cancel)
            {
                if (!_dialogResult.HasValue)
                    _dialogResult = false;

                OnClosed(EventArgs.Empty);
            }
            else
            {
                _dialogResult = null;  //if the Close is cancelled, DialogResult should always be NULL:
            }
        }

        private void ExecuteOpen()
        {
            _dialogResult = null; //reset the dialogResult to null each time the window is opened
            BringToFront();
        }

        private void BringToFront()
        {
            int index = 0;

            if (_parentContainer != null)
            {
                index = (int)_parentContainer.GetValue(Canvas.ZIndexProperty);
            }

            this.SetValue(Canvas.ZIndexProperty, ++index);

            if (IsModal)
            {
                Canvas.SetZIndex(_modalLayerPanel, -1);
            }
        }

        private void SetStartupPosition()
        {
            CenterChildWindow();
            BringToFront();
        }

        private void CenterChildWindow()
        {
            _moveTransform.X = _moveTransform.Y = 0;

            if (_parentContainer != null)
            {
                Left = (_parentContainer.ActualWidth - WindowRoot.ActualWidth) / 2.0;
                Top = (_parentContainer.ActualHeight - WindowRoot.ActualHeight) / 2.0;
            }
        }

        protected virtual void ChangeVisualState()
        {
            if (WindowState == WindowState.Closed)
            {
                VisualStateManager.GoToState(this, VisualStates.Closed, true);
            }
            else
            {
                VisualStateManager.GoToState(this, VisualStates.Open, true);
            }
        }

        private void ShowModalLayer()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (!_modalLayerPanel.Children.Contains(_modalLayer))
                    _modalLayerPanel.Children.Add(_modalLayer);

                BringToFront();
            }
        }

        private void HideModalLayer()
        {
            if (_modalLayerPanel.Children.Contains(_modalLayer))
                _modalLayerPanel.Children.Remove(_modalLayer);
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
            WindowState = WindowState.Open;
        }

        public void Close()
        {
            WindowState = WindowState.Closed;
        }

        #endregion //Public

        #endregion //Methods

        #region Events

        /// <summary>
        /// Occurs when the ChildWindow is closed.
        /// </summary>
        public event EventHandler Closed;
        protected virtual void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }

        /// <summary>
        /// Occurs when the ChildWindow is closing.
        /// </summary>
        public event EventHandler<CancelEventArgs> Closing;
        protected virtual void OnClosing(CancelEventArgs e)
        {
            if (Closing != null)
                Closing(this, e);
        }

        #endregion //Events
    }
}
