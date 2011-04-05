using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Windows.Controls.Core.Utilities;

namespace Microsoft.Windows.Controls
{
    public class ColorPicker : Control
    {
        #region Members

        private Popup _colorPickerCanvasPopup;
        private ListBox _availableColors;
        private ListBox _standardColors;
        private ListBox _recentColors;

        #endregion //Members

        #region Properties

        #region AvailableColors

        public static readonly DependencyProperty AvailableColorsProperty = DependencyProperty.Register("AvailableColors", typeof(ObservableCollection<ColorItem>), typeof(ColorPicker), new UIPropertyMetadata(CreateAvailableColors()));
        public ObservableCollection<ColorItem> AvailableColors
        {
            get { return (ObservableCollection<ColorItem>)GetValue(AvailableColorsProperty); }
            set { SetValue(AvailableColorsProperty, value); }
        }

        #endregion //AvailableColors

        #region ButtonStyle

        public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register("ButtonStyle", typeof(Style), typeof(ColorPicker));
        public Style ButtonStyle
        {
            get { return (Style)GetValue(ButtonStyleProperty); }
            set { SetValue(ButtonStyleProperty, value); }
        }

        #endregion //ButtonStyle

        #region DisplayColorAndName

        public static readonly DependencyProperty DisplayColorAndNameProperty = DependencyProperty.Register("DisplayColorAndName", typeof(bool), typeof(ColorPicker), new UIPropertyMetadata(false));
        public bool DisplayColorAndName
        {
            get { return (bool)GetValue(DisplayColorAndNameProperty); }
            set { SetValue(DisplayColorAndNameProperty, value); }
        }

        #endregion //DisplayColorAndName

        #region IsOpen

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(ColorPicker), new UIPropertyMetadata(false));
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        #endregion //IsOpen

        #region RecentColors

        public static readonly DependencyProperty RecentColorsProperty = DependencyProperty.Register("RecentColors", typeof(ObservableCollection<ColorItem>), typeof(ColorPicker), new UIPropertyMetadata(null));
        public ObservableCollection<ColorItem> RecentColors
        {
            get { return (ObservableCollection<ColorItem>)GetValue(RecentColorsProperty); }
            set { SetValue(RecentColorsProperty, value); }
        }

        #endregion //RecentColors

        #region SelectedColor

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectedColorPropertyChanged)));
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        private static void OnSelectedColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker colorPicker = (ColorPicker)d;
            if (colorPicker != null)
                colorPicker.OnSelectedColorChanged((Color)e.OldValue, (Color)e.NewValue);
        }

        private void OnSelectedColorChanged(Color oldValue, Color newValue)
        {
            SelectedColorText = newValue.GetColorName();

            RoutedPropertyChangedEventArgs<Color> args = new RoutedPropertyChangedEventArgs<Color>(oldValue, newValue);
            args.RoutedEvent = ColorPicker.SelectedColorChangedEvent;
            RaiseEvent(args);
        }

        #endregion //SelectedColor

        #region SelectedColorText

        public static readonly DependencyProperty SelectedColorTextProperty = DependencyProperty.Register("SelectedColorText", typeof(string), typeof(ColorPicker), new UIPropertyMetadata("Black"));
        public string SelectedColorText
        {
            get { return (string)GetValue(SelectedColorTextProperty); }
            protected set { SetValue(SelectedColorTextProperty, value); }
        }

        #endregion //SelectedColorText

        #region ShowAdvancedButton

        public static readonly DependencyProperty ShowAdvancedButtonProperty = DependencyProperty.Register("ShowAdvancedButton", typeof(bool), typeof(ColorPicker), new UIPropertyMetadata(true));
        public bool ShowAdvancedButton
        {
            get { return (bool)GetValue(ShowAdvancedButtonProperty); }
            set { SetValue(ShowAdvancedButtonProperty, value); }
        }        

        #endregion //ShowAdvancedButton

        #region ShowAvailableColors

        public static readonly DependencyProperty ShowAvailableColorsProperty = DependencyProperty.Register("ShowAvailableColors", typeof(bool), typeof(ColorPicker), new UIPropertyMetadata(true));
        public bool ShowAvailableColors
        {
            get { return (bool)GetValue(ShowAvailableColorsProperty); }
            set { SetValue(ShowAvailableColorsProperty, value); }
        }

        #endregion //ShowAvailableColors

        #region ShowRecentColors

        public static readonly DependencyProperty ShowRecentColorsProperty = DependencyProperty.Register("ShowRecentColors", typeof(bool), typeof(ColorPicker), new UIPropertyMetadata(false));
        public bool ShowRecentColors
        {
            get { return (bool)GetValue(ShowRecentColorsProperty); }
            set { SetValue(ShowRecentColorsProperty, value); }
        }

        #endregion //DisplayRecentColors

        #region ShowStandardColors

        public static readonly DependencyProperty ShowStandardColorsProperty = DependencyProperty.Register("ShowStandardColors", typeof(bool), typeof(ColorPicker), new UIPropertyMetadata(true));
        public bool ShowStandardColors
        {
            get { return (bool)GetValue(ShowStandardColorsProperty); }
            set { SetValue(ShowStandardColorsProperty, value); }
        }

        #endregion //DisplayStandardColors

        #region StandardColors

        public static readonly DependencyProperty StandardColorsProperty = DependencyProperty.Register("StandardColors", typeof(ObservableCollection<ColorItem>), typeof(ColorPicker), new UIPropertyMetadata(CreateStandardColors()));
        public ObservableCollection<ColorItem> StandardColors
        {
            get { return (ObservableCollection<ColorItem>)GetValue(StandardColorsProperty); }
            set { SetValue(StandardColorsProperty, value); }
        }

        #endregion //StandardColors

        #endregion //Properties

        #region Constructors

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        public ColorPicker()
        {
            RecentColors = new ObservableCollection<ColorItem>();
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _colorPickerCanvasPopup = (Popup)GetTemplateChild("PART_ColorPickerPalettePopup");
            //_colorPickerCanvasPopup.Opened += ColorPickerCanvasPopup_Opened;

            _availableColors = (ListBox)GetTemplateChild("PART_AvailableColors");
            _availableColors.SelectionChanged += Color_SelectionChanged;

            _standardColors = (ListBox)GetTemplateChild("PART_StandardColors");
            _standardColors.SelectionChanged += Color_SelectionChanged;

            _recentColors = (ListBox)GetTemplateChild("PART_RecentColors");
            _recentColors.SelectionChanged += Color_SelectionChanged;
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        void ColorPickerCanvasPopup_Opened(object sender, EventArgs e)
        {

        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                case Key.Tab:
                    {
                        CloseColorPicker();
                        break;
                    }
            }
        }

        private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
        {
            CloseColorPicker();
        }

        private void Color_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (ListBox)sender;

            if (e.AddedItems.Count > 0)
            {
                var colorItem = (ColorItem)e.AddedItems[0];
                SelectedColor = colorItem.Color;
                UpdateRecentColors(colorItem);
                CloseColorPicker();
                lb.SelectedIndex = -1; //for now I don't care about keeping track of the selected color
            }
        }

        #endregion //Event Handlers

        #region Events

        public static readonly RoutedEvent SelectedColorChangedEvent = EventManager.RegisterRoutedEvent("SelectedColorChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<Color>), typeof(ColorPicker));
        public event RoutedPropertyChangedEventHandler<Color> SelectedColorChanged
        {
            add { AddHandler(SelectedColorChangedEvent, value); }
            remove { RemoveHandler(SelectedColorChangedEvent, value); }
        }

        #endregion //Events

        #region Methods

        private void CloseColorPicker()
        {
            if (IsOpen)
                IsOpen = false;
            ReleaseMouseCapture();
        }

        private void UpdateRecentColors(ColorItem colorItem)
        {
            if (!RecentColors.Contains(colorItem))
                RecentColors.Add(colorItem);

            if (RecentColors.Count > 10) //don't allow more than ten, maybe make a property that can be set by the user.
                RecentColors.RemoveAt(0);
        }

        private static ObservableCollection<ColorItem> CreateStandardColors()
        {
            ObservableCollection<ColorItem> _standardColors = new ObservableCollection<ColorItem>();
            _standardColors.Add(new ColorItem(Colors.Transparent, "Transparent"));
            _standardColors.Add(new ColorItem(Colors.White, "White"));
            _standardColors.Add(new ColorItem(Colors.Gray, "Gray"));
            _standardColors.Add(new ColorItem(Colors.Black, "Black"));
            _standardColors.Add(new ColorItem(Colors.Red, "Red"));
            _standardColors.Add(new ColorItem(Colors.Green, "Green"));
            _standardColors.Add(new ColorItem(Colors.Blue, "Blue"));
            _standardColors.Add(new ColorItem(Colors.Yellow, "Yellow"));
            _standardColors.Add(new ColorItem(Colors.Orange, "Orange"));
            _standardColors.Add(new ColorItem(Colors.Purple, "Purple"));
            return _standardColors;
        }

        private static ObservableCollection<ColorItem> CreateAvailableColors()
        {
            ObservableCollection<ColorItem> _standardColors = new ObservableCollection<ColorItem>();

            foreach (var item in ColorUtilities.KnownColors)
            {
                if (!String.Equals(item.Key, "Transparent"))
                {
                    var colorItem = new ColorItem(item.Value, item.Key);
                    if (!_standardColors.Contains(colorItem))
                        _standardColors.Add(colorItem);
                }
            }

            return _standardColors;
        }

        #endregion //Methods
    }
}