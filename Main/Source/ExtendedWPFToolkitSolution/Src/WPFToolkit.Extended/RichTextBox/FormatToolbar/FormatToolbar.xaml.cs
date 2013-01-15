using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls.Formatting
{
    /// <summary>
    /// Interaction logic for FormatToolbar.xaml
    /// </summary>
    public partial class FormatToolbar : UserControl
    {
        #region Properties

        public static readonly DependencyProperty RichTextBoxProperty = DependencyProperty.Register("RichTextBox", typeof(RichTextBox), typeof(FormatToolbar));
        public RichTextBox RichTextBox
        {
            get { return (RichTextBox)GetValue(RichTextBoxProperty); }
            set { SetValue(RichTextBoxProperty, value); }
        }

        public double[] FontSizes
        {
            get
            {
                return new double[] { 
		            3.0, 4.0, 5.0, 6.0, 6.5, 7.0, 7.5, 8.0, 8.5, 9.0, 9.5, 
		            10.0, 10.5, 11.0, 11.5, 12.0, 12.5, 13.0, 13.5, 14.0, 15.0,
		            16.0, 17.0, 18.0, 19.0, 20.0, 22.0, 24.0, 26.0, 28.0, 30.0,
		            32.0, 34.0, 36.0, 38.0, 40.0, 44.0, 48.0, 52.0, 56.0, 60.0, 64.0, 68.0, 72.0, 76.0,
		            80.0, 88.0, 96.0, 104.0, 112.0, 120.0, 128.0, 136.0, 144.0
		            };
            }
        }

        #endregion

        #region Constructors

        public FormatToolbar(RichTextBox richTextBox)
        {
            InitializeComponent();
            Loaded += FormatToolbar_Loaded;
            RichTextBox = richTextBox;
            RichTextBox.SelectionChanged += RichTextBox_SelectionChanged;
        }

        #endregion //Constructors

        #region Event Hanlders

        void FormatToolbar_Loaded(object sender, RoutedEventArgs e)
        {
            _cmbFontFamilies.ItemsSource = System.Windows.Media.Fonts.SystemFontFamilies;
            _cmbFontSizes.ItemsSource = FontSizes;
        }

        private void FontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            FontFamily editValue = (FontFamily)e.AddedItems[0];
            ApplyPropertyValueToSelectedText(TextElement.FontFamilyProperty, editValue);
        }

        private void FontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            ApplyPropertyValueToSelectedText(TextElement.FontSizeProperty, e.AddedItems[0]);
        }

        void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateVisualState();
        }

        private void DragWidget_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ProcessMove(e);
        }

        #endregion //Event Hanlders

        #region Methods

        private void UpdateVisualState()
        {
            UpdateToggleButtonState();
            UpdateSelectedFontFamily();
            UpdateSelectedFontSize();
        }

        private void UpdateToggleButtonState()
        {
            UpdateItemCheckedState(_btnBold, TextElement.FontWeightProperty, FontWeights.Bold);
            UpdateItemCheckedState(_btnItalic, TextElement.FontStyleProperty, FontStyles.Italic);
            UpdateItemCheckedState(_btnUnderline, Inline.TextDecorationsProperty, TextDecorations.Underline);

            UpdateItemCheckedState(_btnAlignLeft, Paragraph.TextAlignmentProperty, TextAlignment.Left);
            UpdateItemCheckedState(_btnAlignCenter, Paragraph.TextAlignmentProperty, TextAlignment.Center);
            UpdateItemCheckedState(_btnAlignRight, Paragraph.TextAlignmentProperty, TextAlignment.Right);
        }

        void UpdateItemCheckedState(ToggleButton button, DependencyProperty formattingProperty, object expectedValue)
        {
            object currentValue = RichTextBox.Selection.GetPropertyValue(formattingProperty);
            button.IsChecked = (currentValue == DependencyProperty.UnsetValue) ? false : currentValue != null && currentValue.Equals(expectedValue);
        }

        private void UpdateSelectedFontFamily()
        {
            object value = RichTextBox.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            FontFamily currentFontFamily = (FontFamily)((value == DependencyProperty.UnsetValue) ? null : value);
            if (currentFontFamily != null)
            {
                _cmbFontFamilies.SelectedItem = currentFontFamily;
            }
        }

        private void UpdateSelectedFontSize()
        {
            object value = RichTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            _cmbFontSizes.SelectedValue = (value == DependencyProperty.UnsetValue) ? null : value;
        }

        void ApplyPropertyValueToSelectedText(DependencyProperty formattingProperty, object value)
        {
            if (value == null)
                return;

            RichTextBox.Selection.ApplyPropertyValue(formattingProperty, value);
        }

        private void ProcessMove(DragDeltaEventArgs e)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(RichTextBox);
            UIElementAdorner<Control> adorner = layer.GetAdorners(RichTextBox)[0] as UIElementAdorner<Control>;
            adorner.SetOffsets(adorner.OffsetLeft + e.HorizontalChange, adorner.OffsetTop + e.VerticalChange);
        }

        #endregion //Methods
    }
}
