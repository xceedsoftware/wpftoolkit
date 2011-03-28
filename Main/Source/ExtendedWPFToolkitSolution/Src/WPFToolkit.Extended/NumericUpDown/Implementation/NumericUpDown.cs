using System;
using System.Windows;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    public class NumericUpDown : UpDownBase
    {
        #region Properties

        #region Minimum

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(Decimal.MinValue, OnMinimumPropertyChanged));
        public decimal Minimum
        {
            get { return (decimal)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        private static void OnMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown nud = d as NumericUpDown;
            nud.SetValidSpinDirection();
        }

        #endregion Minimum

        #region Maximum

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(Decimal.MaxValue, OnMaximumPropertyChanged));
        public decimal Maximum
        {
            get { return (decimal)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        private static void OnMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown nud = d as NumericUpDown;
            nud.SetValidSpinDirection();
        }

        #endregion Maximum

        #region Increment

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(1M));
        public decimal Increment
        {
            get { return (decimal)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        #endregion

        #region FormatString

        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(NumericUpDown), new PropertyMetadata(string.Empty, OnStringFormatPropertyPropertyChanged));
        public string FormatString
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        private static void OnStringFormatPropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown nud = d as NumericUpDown;
            nud.OnStringFormatChanged(e.OldValue.ToString(), e.NewValue.ToString());
        }

        protected virtual void OnStringFormatChanged(string oldValue, string newValue)
        {
            SyncTextAndValueProperties(NumericUpDown.TextProperty, Value);
        }

        #endregion //FormatString

        #region SelectAllOnGotFocus

        public static readonly DependencyProperty SelectAllOnGotFocusProperty = DependencyProperty.Register("SelectAllOnGotFocus", typeof(bool), typeof(NumericUpDown), new PropertyMetadata(false));
        public bool SelectAllOnGotFocus
        {
            get { return (bool)GetValue(SelectAllOnGotFocusProperty); }
            set { SetValue(SelectAllOnGotFocusProperty, value); }
        }

        #endregion //SelectAllOnGotFocus

        #endregion

        #region Constructors

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
            ValueTypeProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(decimal)));
            ValueProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(default(decimal)));
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SetValidSpinDirection();

            if (SelectAllOnGotFocus)
            {
                //in order to select all the text we must handle both the keybord (tabbing) and mouse (clicking) events
                TextBox.GotKeyboardFocus += OnTextBoxGotKeyBoardFocus;
                TextBox.PreviewMouseLeftButtonDown += OnTextBoxPreviewMouseLeftButtonDown;
            }
        }

        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            if (TextBox != null)
                TextBox.Focus();

            base.OnAccessKey(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (TextBox != null)
                TextBox.Focus();
        }

        protected override void OnValueChanged(object oldValue, object newValue)
        {
            SetValidSpinDirection();
            base.OnValueChanged(oldValue, newValue);
        }

        protected override object OnCoerceValue(object value)
        {
            if (value == null) return value;

            decimal val = Convert.ToDecimal(value);

            if (val < Minimum)
            {
                return Minimum;
            }
            else if (val > Maximum)
            {
                return Maximum;
            }
            else
            {
                return value;
            }
        }

        protected override object ConvertTextToValue(string text)
        {
            object result = null;

            NumberFormatInfo info = NumberFormatInfo.GetInstance(CultureInfo.CurrentCulture);

            try
            {
                result = FormatString.Contains("P") ? ParsePercent(text, ValueType, info) : ParseDataValue(text, ValueType, info);
            }
            catch
            {
                TextBox.Text = Text = ConvertValueToText(Value);
                return Value;
            }

            return result;
        }

        protected override string ConvertValueToText(object value)
        {
            //TODO: create GetTextFromValue methods for each data type;
            if (value is double)
            {
                double d = (double)value;

                if (Double.IsNaN(d))
                    return "NaN";
                else if (Double.IsPositiveInfinity(d) || Double.MaxValue == d)
                    return "Infinity";
                else if (Double.IsNegativeInfinity(d) || Double.MinValue == d)
                    return "Negative-Infinity";
            }

            return (Convert.ToDecimal(Value)).ToString(FormatString, CultureInfo.CurrentCulture);
        }

        protected override void OnIncrement()
        {
            decimal newValue = (Convert.ToDecimal(Value) + Increment);
            Value = ValueType != typeof(Decimal) ? Convert.ChangeType(newValue, ValueType) : newValue;
        }

        protected override void OnDecrement()
        {
            decimal newValue = (Convert.ToDecimal(Value) - Increment);
            Value = ValueType != typeof(Decimal) ? Convert.ChangeType(newValue, ValueType) : newValue;
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        private void OnTextBoxGotKeyBoardFocus(object sender, RoutedEventArgs e)
        {
            TextBox.SelectAll();
        }

        void OnTextBoxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!TextBox.IsKeyboardFocused)
            {
                e.Handled = true;
                TextBox.Focus();
            }
        }

        #endregion //Event Handlers

        #region Methods

        /// <summary>
        /// Sets the valid spin direction based on current value, minimum and maximum.
        /// </summary>
        private void SetValidSpinDirection()
        {
            ValidSpinDirections validDirections = ValidSpinDirections.None;

            if (Convert.ToDecimal(Value) < Maximum)
            {
                validDirections = validDirections | ValidSpinDirections.Increase;
            }

            if (Convert.ToDecimal(Value) > Minimum)
            {
                validDirections = validDirections | ValidSpinDirections.Decrease;
            }

            if (Spinner != null)
            {
                Spinner.ValidSpinDirection = validDirections;
            }
        }

        #region Parsing

        private static object ParseDataValue(string text, Type dataType, NumberFormatInfo info)
        {
            try
            {
                if (typeof(decimal) == dataType)
                {
                    return ParseDecimal(text, info);
                }
                else if (typeof(double) == dataType)
                {
                    return ParseDouble(text, info);
                }
                else if (typeof(float) == dataType)
                {
                    return ParseFloat(text, info);
                }
                else if (typeof(byte) == dataType || typeof(sbyte) == dataType ||
                         typeof(short) == dataType || typeof(ushort) == dataType || typeof(Int16) == dataType ||
                         typeof(int) == dataType || typeof(uint) == dataType || typeof(Int32) == dataType ||
                         typeof(long) == dataType || typeof(ulong) == dataType || typeof(Int64) == dataType)
                {
                    return ParseWholeNumber(text, dataType, info);
                }
                else
                {
                    throw new ArgumentException("Type not supported");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static double ParseDouble(string text, NumberFormatInfo info)
        {
            return double.Parse(text, NumberStyles.Any, info);
        }

        private static float ParseFloat(string text, NumberFormatInfo info)
        {
            return float.Parse(text, NumberStyles.Any, info);
        }

        private static decimal ParseDecimal(string text, NumberFormatInfo info)
        {
            return decimal.Parse(text, NumberStyles.Any, info);
        }

        private static object ParseWholeNumber(string text, Type dataType, NumberFormatInfo info)
        {
            decimal result = decimal.Parse(text, NumberStyles.Any, info);
            return Convert.ChangeType(result, dataType, info);
        }

        private static object ParsePercent(string text, Type dataType, NumberFormatInfo info)
        {
            text = text.Replace(info.PercentSymbol, null);

            decimal result = decimal.Parse(text, NumberStyles.Any, info);
            result = result / 100;

            return Convert.ChangeType(result, dataType, info);
        }

        #endregion

        #endregion //Methods
    }
}
