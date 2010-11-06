using System;
using System.Windows;
using System.Globalization;

namespace Microsoft.Windows.Controls
{

    public class NumericUpDown : UpDownBase<double>
    {
        #region Properties

        #region Minimum

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(NumericUpDown), new PropertyMetadata(0d, OnMinimumPropertyChanged));
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        private static void OnMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        protected virtual void OnMinimumChanged(double oldValue, double newValue)
        {
        }
        #endregion Minimum

        #region Maximum

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(NumericUpDown), new PropertyMetadata(100d, OnMaximumPropertyChanged));
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        private static void OnMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        protected virtual void OnMaximumChanged(double oldValue, double newValue)
        {
        }
        #endregion Maximum

        #region Increment

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(double), typeof(NumericUpDown), new PropertyMetadata(1d, OnIncrementPropertyChanged));
        public double Increment
        {
            get { return (double)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        private static void OnIncrementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        protected virtual void OnIncrementChanged(double oldValue, double newValue)
        {
        }

        #endregion

        #region FormatString

        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(NumericUpDown), new PropertyMetadata("F0", OnStringFormatPropertyPropertyChanged));
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
            Text = FormatValue();
        }

        #endregion //FormatString

        #endregion

        #region Constructors

        public NumericUpDown()
            : base()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SetValidSpinDirection();
        }

        protected override void OnValueChanged(RoutedPropertyChangedEventArgs<double> e)
        {
            SetValidSpinDirection();
        }

        protected override double ParseValue(string text)
        {
            return double.Parse(text, NumberStyles.Any, CultureInfo.CurrentCulture);
        }

        protected internal override string FormatValue()
        {
            return Value.ToString(FormatString, CultureInfo.CurrentCulture);
        }

        protected override void OnIncrement()
        {
            Value = (double)((decimal)Value + (decimal)Increment);
        }

        protected override void OnDecrement()
        {
            Value = (double)((decimal)Value - (decimal)Increment);
        }

        #endregion //Base Class Overrides

        #region Methods

        /// <summary>
        /// Sets the valid spin direction based on current value, minimum and maximum.
        /// </summary>
        private void SetValidSpinDirection()
        {
            ValidSpinDirections validDirections = ValidSpinDirections.None;

            if (Value < Maximum)
            {
                validDirections = validDirections | ValidSpinDirections.Increase;
            }

            if (Value > Minimum)
            {
                validDirections = validDirections | ValidSpinDirections.Decrease;
            }

            if (Spinner != null)
            {
                Spinner.ValidSpinDirection = validDirections;
            }
        }

        #endregion //Methods
    }
}
