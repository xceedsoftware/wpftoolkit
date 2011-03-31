using System;
using Microsoft.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Windows.Controls
{
    public class DecimalUpDown : UpDownBase<decimal?>
    {
        #region Properties

        #region DefaultValue

        //can possibly be in base class
        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(decimal), typeof(DecimalUpDown), new UIPropertyMetadata(default(decimal)));
        public decimal DefaultValue
        {
            get { return (decimal)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        #endregion //DefaultValue

        #region FormatString

        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(DecimalUpDown), new UIPropertyMetadata(String.Empty));
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }        

        #endregion //FormatString

        #region Increment

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(decimal), typeof(DecimalUpDown), new PropertyMetadata(1m));
        public decimal Increment
        {
            get { return (decimal)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        #endregion

        #region Maximum

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(decimal), typeof(DecimalUpDown), new UIPropertyMetadata(decimal.MaxValue, OnMaximumChanged));
        public decimal Maximum
        {
            get { return (decimal)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        private static void OnMaximumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DecimalUpDown decimalUpDown = o as DecimalUpDown;
            if (decimalUpDown != null)
                decimalUpDown.OnMaximumChanged((decimal)e.OldValue, (decimal)e.NewValue);
        }

        protected virtual void OnMaximumChanged(decimal oldValue, decimal newValue)
        {
            //SetValidSpinDirection();
        }

        #endregion //Maximum

        #region Minimum

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(decimal), typeof(DecimalUpDown), new UIPropertyMetadata(decimal.MinValue, OnMinimumChanged));
        public decimal Minimum
        {
            get { return (decimal)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        private static void OnMinimumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DecimalUpDown decimalUpDown = o as DecimalUpDown;
            if (decimalUpDown != null)
                decimalUpDown.OnMinimumChanged((decimal)e.OldValue, (decimal)e.NewValue);
        }

        protected virtual void OnMinimumChanged(decimal oldValue, decimal newValue)
        {
            //SetValidSpinDirection();
        }

        #endregion //Minimum

        #region SelectAllOnGotFocus

        public static readonly DependencyProperty SelectAllOnGotFocusProperty = DependencyProperty.Register("SelectAllOnGotFocus", typeof(bool), typeof(DecimalUpDown), new PropertyMetadata(false));
        public bool SelectAllOnGotFocus
        {
            get { return (bool)GetValue(SelectAllOnGotFocusProperty); }
            set { SetValue(SelectAllOnGotFocusProperty, value); }
        }

        #endregion //SelectAllOnGotFocus

        #endregion //Properties

        #region Constructors

        static DecimalUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DecimalUpDown), new FrameworkPropertyMetadata(typeof(DecimalUpDown)));
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (SelectAllOnGotFocus)
            {
                //in order to select all the text we must handle both the keybord (tabbing) and mouse (clicking) events
                TextBox.GotKeyboardFocus += OnTextBoxGotKeyBoardFocus;
                TextBox.PreviewMouseLeftButtonDown += OnTextBoxPreviewMouseLeftButtonDown;
            }

            //SetValidSpinDirection();
        }

        protected override decimal? OnCoerceValue(decimal? value)
        {
            if (value == null) return value;

            decimal val = value.Value;

            if (value < Minimum)
                return Minimum;
            else if (value > Maximum)
                return Maximum;
            else
                return value;
        }

        protected override void OnIncrement()
        {
            if (Value.HasValue)
                Value += Increment;
            else
                Value = DefaultValue;
        }

        protected override void OnDecrement()
        {
            if (Value.HasValue)
                Value -= Increment;
            else
                Value = DefaultValue;
        }

        protected override decimal? ConvertTextToValue(string text)
        {
            decimal? result = null;

            if (String.IsNullOrEmpty(text))
                return result;

            try
            {
                result = Decimal.Parse(text, System.Globalization.NumberStyles.Any, CultureInfo);
            }
            catch
            {
                Text = ConvertValueToText();
                return Value;
            }

            return result;
        }

        protected override string ConvertValueToText()
        {
            if (Value == null)
                return string.Empty;

            return Value.Value.ToString(FormatString, CultureInfo);
        }

        protected override void OnValueChanged(decimal? oldValue, decimal? newValue)
        {
            //SetValidSpinDirection();
            base.OnValueChanged(oldValue, newValue);
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
        //private void SetValidSpinDirection()
        //{
        //    ValidSpinDirections validDirections = ValidSpinDirections.None;

        //    if (Value < Maximum)
        //        validDirections = validDirections | ValidSpinDirections.Increase;

        //    if (Value > Minimum)
        //        validDirections = validDirections | ValidSpinDirections.Decrease;

        //    if (Spinner != null)
        //        Spinner.ValidSpinDirection = validDirections;
        //}

        #endregion //Methods
    }
}
