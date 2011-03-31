using System;
using Microsoft.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Windows.Controls
{
    public class DoubleUpDown : UpDownBase<double?>
    {
        #region Properties

        #region DefaultValue

        //can possibly be in base class
        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(double), typeof(DoubleUpDown), new UIPropertyMetadata(default(double)));
        public double DefaultValue
        {
            get { return (double)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        #endregion //DefaultValue

        #region FormatString

        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(DoubleUpDown), new UIPropertyMetadata(String.Empty));
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        #endregion //FormatString

        #region Increment

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(double), typeof(DoubleUpDown), new PropertyMetadata(1d));
        public double Increment
        {
            get { return (double)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        #endregion

        #region Maximum

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(DoubleUpDown), new UIPropertyMetadata(double.MaxValue, OnMaximumChanged));
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        private static void OnMaximumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DoubleUpDown doubleUpDown = o as DoubleUpDown;
            if (doubleUpDown != null)
                doubleUpDown.OnMaximumChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnMaximumChanged(double oldValue, double newValue)
        {
            //SetValidSpinDirection();
        }

        #endregion //Maximum

        #region Minimum

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(DoubleUpDown), new UIPropertyMetadata(double.MinValue, OnMinimumChanged));
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        private static void OnMinimumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DoubleUpDown doubleUpDown = o as DoubleUpDown;
            if (doubleUpDown != null)
                doubleUpDown.OnMinimumChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnMinimumChanged(double oldValue, double newValue)
        {
            //SetValidSpinDirection();
        }

        #endregion //Minimum

        #region SelectAllOnGotFocus

        public static readonly DependencyProperty SelectAllOnGotFocusProperty = DependencyProperty.Register("SelectAllOnGotFocus", typeof(bool), typeof(DoubleUpDown), new PropertyMetadata(false));
        public bool SelectAllOnGotFocus
        {
            get { return (bool)GetValue(SelectAllOnGotFocusProperty); }
            set { SetValue(SelectAllOnGotFocusProperty, value); }
        }

        #endregion //SelectAllOnGotFocus

        #endregion //Properties

        #region Constructors

        static DoubleUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DoubleUpDown), new FrameworkPropertyMetadata(typeof(DoubleUpDown)));
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

        protected override double? OnCoerceValue(double? value)
        {
            if (value == null) return value;

            double val = value.Value;

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

        protected override double? ConvertTextToValue(string text)
        {
            double? result = null;

            if (String.IsNullOrEmpty(text))
                return result;

            try
            {
                result = Double.Parse(text, System.Globalization.NumberStyles.Any, CultureInfo);
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

        protected override void OnValueChanged(double? oldValue, double? newValue)
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
