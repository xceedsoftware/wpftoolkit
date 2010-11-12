using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    public class ColorPicker : Control
    {
        #region Private Members

        ToggleButton _colorPickerToggleButton;
        Popup _colorPickerCanvasPopup;
        Button _okButton;
        private TranslateTransform _colorShadeSelectorTransform = new TranslateTransform();
        private Canvas _colorShadingCanvas;
        private Canvas _colorShadeSelector;
        private ColorSpectrumSlider _spectrumSlider;
        private Point? _currentColorPosition;
        private Color _currentColor = Colors.White;
        private bool _isLoaded;

        #endregion //Private Members

        #region Constructors

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        public ColorPicker()
        {

        }

        #endregion //Constructors

        #region Properties

        public static readonly DependencyProperty CurrentColorProperty = DependencyProperty.Register("CurrentColor", typeof(Color), typeof(ColorPicker), new PropertyMetadata(Colors.White));
        public Color CurrentColor
        {
            get { return (Color)GetValue(CurrentColorProperty); }
            set { SetValue(CurrentColorProperty, value); }
        }

        #region SelectedColor

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(SelectedColorPropertyChanged)));
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        private static void SelectedColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker colorPicker = (ColorPicker)d;
            colorPicker.SetSelectedColor((Color)e.NewValue);
        }

        #endregion //SelectedColor

        #region ScRGB

        #region ScA

        public static readonly DependencyProperty ScAProperty = DependencyProperty.Register("ScA", typeof(float), typeof(ColorPicker), new PropertyMetadata((float)1, new PropertyChangedCallback(OnScAPropertyChangedChanged)));
        public float ScA
        {
            get { return (float)GetValue(ScAProperty); }
            set { SetValue(ScAProperty, value); }
        }

        private static void OnScAPropertyChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker c = (ColorPicker)d;
            c.SetScA((float)e.NewValue);
        }

        protected virtual void SetScA(float newValue)
        {
            _currentColor.ScA = newValue;
            A = _currentColor.A;
            CurrentColor = _currentColor;
            HexadecimalString = _currentColor.ToString();
        }

        #endregion //ScA

        #region ScR

        public static readonly DependencyProperty ScRProperty = DependencyProperty.Register("ScR", typeof(float), typeof(ColorPicker), new PropertyMetadata((float)1, new PropertyChangedCallback(OnScRPropertyChanged)));
        public float ScR
        {
            get { return (float)GetValue(ScRProperty); }
            set { SetValue(RProperty, value); }
        }

        private static void OnScRPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion //ScR

        #region ScG

        public static readonly DependencyProperty ScGProperty = DependencyProperty.Register("ScG", typeof(float), typeof(ColorPicker), new PropertyMetadata((float)1, new PropertyChangedCallback(OnScGPropertyChanged)));
        public float ScG
        {
            get { return (float)GetValue(ScGProperty); }
            set { SetValue(GProperty, value); }
        }

        private static void OnScGPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion //ScG

        #region ScB

        public static readonly DependencyProperty ScBProperty = DependencyProperty.Register("ScB", typeof(float), typeof(ColorPicker), new PropertyMetadata((float)1, new PropertyChangedCallback(OnScBPropertyChanged)));
        public float ScB
        {
            get { return (float)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }

        private static void OnScBPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion //ScB

        #endregion //ScRGB

        #region RGB

        #region A

        public static readonly DependencyProperty AProperty = DependencyProperty.Register("A", typeof(byte), typeof(ColorPicker), new PropertyMetadata((byte)255, new PropertyChangedCallback(OnAPropertyChanged)));
        public byte A
        {
            get { return (byte)GetValue(AProperty); }
            set { SetValue(AProperty, value); }
        }

        private static void OnAPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker c = (ColorPicker)d;
            c.SetA((byte)e.NewValue);
        }

        protected virtual void SetA(byte newValue)
        {
            _currentColor.A = newValue;
            SetValue(CurrentColorProperty, _currentColor);
        }

        #endregion //A

        #region R

        public static readonly DependencyProperty RProperty = DependencyProperty.Register("R", typeof(byte), typeof(ColorPicker), new PropertyMetadata((byte)255, new PropertyChangedCallback(OnRPropertyChanged)));
        public byte R
        {
            get { return (byte)GetValue(RProperty); }
            set { SetValue(RProperty, value); }
        }

        private static void OnRPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion //R

        #region G

        public static readonly DependencyProperty GProperty = DependencyProperty.Register("G", typeof(byte), typeof(ColorPicker), new PropertyMetadata((byte)255, new PropertyChangedCallback(OnGPropertyChanged)));
        public byte G
        {
            get { return (byte)GetValue(GProperty); }
            set { SetValue(GProperty, value); }
        }

        private static void OnGPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion //G

        #region B

        public static readonly DependencyProperty BProperty = DependencyProperty.Register("B", typeof(byte), typeof(ColorPicker), new PropertyMetadata((byte)255, new PropertyChangedCallback(OnBPropertyChanged)));
        public byte B
        {
            get { return (byte)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }

        private static void OnBPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion //B

        #endregion //RGB

        #region HexadecimalString

        public static readonly DependencyProperty HexadecimalStringProperty = DependencyProperty.Register("HexadecimalString", typeof(string), typeof(ColorPicker), new PropertyMetadata("#FFFFFFFF", new PropertyChangedCallback(OnHexadecimalStringPropertyChanged)));
        public string HexadecimalString
        {
            get { return (string)GetValue(HexadecimalStringProperty); }
            set { SetValue(HexadecimalStringProperty, value); }
        }

        private static void OnHexadecimalStringPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion //HexadecimalString

        #endregion //Properties

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _colorPickerToggleButton = (ToggleButton)GetTemplateChild("PART_ColorPickerToggleButton");
            _colorPickerToggleButton.Click += ColorPickerToggleButton_Clicked;

            _colorPickerCanvasPopup = (Popup)GetTemplateChild("PART_ColorPickerCanvasPopup");

            _colorShadingCanvas = (Canvas)GetTemplateChild("PART_ColorShadingCanvas");
            _colorShadingCanvas.MouseLeftButtonDown += ColorShadingCanvas_MouseLeftButtonDown;
            _colorShadingCanvas.MouseMove += ColorShadingCanvas_MouseMove;
            _colorShadingCanvas.SizeChanged += ColorShadingCanvas_SizeChanged;

            _colorShadeSelector = (Canvas)GetTemplateChild("PART_ColorShadeSelector");
            _colorShadeSelector.RenderTransform = _colorShadeSelectorTransform;

            _spectrumSlider = (ColorSpectrumSlider)GetTemplateChild("PART_SpectrumSlider");
            _spectrumSlider.ValueChanged += SpectrumSlider_ValueChanged;

            _okButton = (Button)GetTemplateChild("PART_OkButton");
            _okButton.Click += OkButton_Click;

            SetSelectedColor(SelectedColor);
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        void ColorShadingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(_colorShadingCanvas);
            UpdateColorShadeSelectorPositionAndCalculateColor(p, true);
        }

        void ColorShadingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(_colorShadingCanvas);
                UpdateColorShadeSelectorPositionAndCalculateColor(p, true);
                Mouse.Synchronize();
            }
        }

        void ColorShadingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_currentColorPosition != null)
            {
                Point _newPoint = new Point
                {
                    X = ((Point)_currentColorPosition).X * e.NewSize.Width,
                    Y = ((Point)_currentColorPosition).Y * e.NewSize.Height
                };

                UpdateColorShadeSelectorPositionAndCalculateColor(_newPoint, false);
            }
        }

        void SpectrumSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_currentColorPosition != null)
            {
                CalculateColor((Point)_currentColorPosition);
            }
        }

        void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (_colorPickerCanvasPopup.IsOpen || _colorPickerToggleButton.IsChecked == true)
            {
                CloseColorPicker();
                SelectedColor = CurrentColor;
            }
        }

        void ColorPickerToggleButton_Clicked(object sender, RoutedEventArgs e)
        {
            _colorPickerCanvasPopup.IsOpen = _colorPickerToggleButton.IsChecked ?? false;
        }

        #endregion //Event Handlers

        #region Methods

        private void CloseColorPicker()
        {
            _colorPickerToggleButton.IsChecked = false;
            _colorPickerCanvasPopup.IsOpen = false;
        }

        private void SetSelectedColor(Color theColor)
        {
            _currentColor = theColor;
            SetValue(AProperty, _currentColor.A);
            SetValue(RProperty, _currentColor.R);
            SetValue(GProperty, _currentColor.G);
            SetValue(BProperty, _currentColor.B);
            UpdateColorShadeSelectorPosition(_currentColor);
        }

        private void UpdateColorShadeSelectorPositionAndCalculateColor(Point p, bool calculateColor)
        {
            if (p.Y < 0)
                p.Y = 0;

            if (p.X < 0)
                p.X = 0;

            if (p.X > _colorShadingCanvas.ActualWidth)
                p.X = _colorShadingCanvas.ActualWidth;

            if (p.Y > _colorShadingCanvas.ActualHeight)
                p.Y = _colorShadingCanvas.ActualHeight;

            _colorShadeSelectorTransform.X = p.X - (_colorShadeSelector.Width / 2);
            _colorShadeSelectorTransform.Y = p.Y - (_colorShadeSelector.Height / 2);

            p.X = p.X / _colorShadingCanvas.ActualWidth;
            p.Y = p.Y / _colorShadingCanvas.ActualHeight;

            _currentColorPosition = p;

            if (calculateColor)
                CalculateColor(p);
        }

        private void UpdateColorShadeSelectorPosition(Color color)
        {
            if (_spectrumSlider == null || _colorShadingCanvas == null)
                return;

            _currentColorPosition = null;

            HsvColor hsv = ColorUtilities.ConvertRgbToHsv(color.R, color.G, color.B);
            _spectrumSlider.Value = hsv.H;

            Point p = new Point(hsv.S, 1 - hsv.V);

            _currentColorPosition = p;

            _colorShadeSelectorTransform.X = (p.X * _colorShadingCanvas.Width) - 5;
            _colorShadeSelectorTransform.Y = (p.Y * _colorShadingCanvas.Height) - 5;
        }

        private void CalculateColor(Point p)
        {
            HsvColor hsv = new HsvColor(360 - _spectrumSlider.Value, 1, 1) { S = p.X, V = 1 - p.Y };
            _currentColor = ColorUtilities.ConvertHsvToRgb(hsv.H, hsv.S, hsv.V); ;
            _currentColor.ScA = ScA;
            CurrentColor = _currentColor;
            HexadecimalString = _currentColor.ToString();
        }

        #endregion //Methods
    }
}
