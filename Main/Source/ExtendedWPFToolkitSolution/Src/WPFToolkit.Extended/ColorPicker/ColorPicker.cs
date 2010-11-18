using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Microsoft.Windows.Controls
{
    public class ColorPicker : Control
    {
        #region Members

        ToggleButton _colorPickerToggleButton;
        Popup _colorPickerCanvasPopup;
        ListBox _availableColors;
        ListBox _standardColors;
        ListBox _recentColors;

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

        #region RecenColors

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
            RoutedPropertyChangedEventArgs<Color> args = new RoutedPropertyChangedEventArgs<Color>(oldValue, newValue);
            args.RoutedEvent = ColorPicker.SelectedColorChangedEvent;
            RaiseEvent(args);
        }

        #endregion //SelectedColor

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
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _colorPickerToggleButton = (ToggleButton)GetTemplateChild("PART_ColorPickerToggleButton");
            _colorPickerToggleButton.Click += ColorPickerToggleButton_Clicked;

            _colorPickerCanvasPopup = (Popup)GetTemplateChild("PART_ColorPickerPalettePopup");

            _availableColors = (ListBox)GetTemplateChild("PART_AvailableColors");
            _availableColors.SelectionChanged += Color_SelectionChanged;

            _standardColors = (ListBox)GetTemplateChild("PART_StandardColors");
            _standardColors.SelectionChanged += Color_SelectionChanged;

            _recentColors = (ListBox)GetTemplateChild("PART_RecentColors");
            _recentColors.SelectionChanged += Color_SelectionChanged;
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        void ColorPickerToggleButton_Clicked(object sender, RoutedEventArgs e)
        {
            _colorPickerCanvasPopup.IsOpen = _colorPickerToggleButton.IsChecked ?? false;
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
            _colorPickerToggleButton.IsChecked = false;
            _colorPickerCanvasPopup.IsOpen = false;
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
            _standardColors.Add(new ColorItem(Colors.White, "White"));
            _standardColors.Add(new ColorItem(Colors.Gray, "Gray"));
            _standardColors.Add(new ColorItem(Colors.Black, "Black"));
            _standardColors.Add(new ColorItem(Colors.Red, "Red"));
            _standardColors.Add(new ColorItem(Colors.Green, "Geen"));
            _standardColors.Add(new ColorItem(Colors.Blue, "Blue"));
            _standardColors.Add(new ColorItem(Colors.Yellow, "Yellow"));
            _standardColors.Add(new ColorItem(Colors.Orange, "Orange"));
            _standardColors.Add(new ColorItem(Colors.Brown, "Brown"));
            _standardColors.Add(new ColorItem(Colors.Purple, "Purple"));
            return _standardColors;
        }

        private static ObservableCollection<ColorItem> CreateAvailableColors()
        {
            ObservableCollection<ColorItem> _standardColors = new ObservableCollection<ColorItem>();

            PropertyInfo[] properties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (PropertyInfo info in properties)
            {
                if (String.Compare(info.Name, "Transparent", false) != 0)
                {
                    Color c = (Color)info.GetValue(typeof(Colors), null);
                    var colorItem = new ColorItem(c, info.Name);
                    if (!_standardColors.Contains(colorItem))
                        _standardColors.Add(colorItem);
                }
            }

            return _standardColors;
        }

        #endregion //Methods
    }
}
