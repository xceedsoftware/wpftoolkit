using System;
using System.Windows;
using System.Globalization;
using Microsoft.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    public class NumericUpDown : UpDownBase
    {
        #region Properties

        #region Minimum

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(NumericUpDown), new PropertyMetadata(Double.MinValue, OnMinimumPropertyChanged));
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        private static void OnMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown nud = d as NumericUpDown;
            nud.SetValidSpinDirection();
        }

        #endregion Minimum

        #region Maximum

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(NumericUpDown), new PropertyMetadata(Double.MaxValue, OnMaximumPropertyChanged));
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        private static void OnMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown nud = d as NumericUpDown;
            nud.SetValidSpinDirection();
        }

        #endregion Maximum

        #region Increment

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(double), typeof(NumericUpDown), new PropertyMetadata(1.0));
        public double Increment
        {
            get { return (double)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
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
            SyncTextAndValueProperties(InputBase.DisplayTextProperty, Value);
        }

        #endregion //FormatString

        #endregion

        #region Constructors

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
            ValueTypeProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(double)));
            ValueProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(default(Double)));
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SetValidSpinDirection();
        }

        protected override void OnValueChanged(object oldValue, object newValue)
        {
            SetValidSpinDirection();
        }

        protected override object ConvertTextToValue(string text)
        {
            NumberFormatInfo info = NumberFormatInfo.GetInstance(CultureInfo.CurrentCulture);
            if (text.Contains(info.PercentSymbol))
            {
                if (ValueType == typeof(decimal))
                    return TryParceDecimalPercent(text, info);
                else
                    return TryParceDoublePercent(text, info);
            }
            else
            {
                if (ValueType == typeof(decimal))
                    return TryParceDecimal(text, info);
                else if (ValueType == typeof(int))
                    return TryParceInteger(text, info);
                else
                    return TryParceDouble(text, info);
            }
        }

        protected override string ConvertValueToText(object value)
        {
            return (Convert.ToDecimal(Value)).ToString(FormatString, CultureInfo.CurrentCulture);
        }

        protected override void OnIncrement()
        {
            double newValue = (double)(Convert.ToDecimal(Value) + (decimal)Increment);

            if (ValueType != typeof(Double))
                Value = Convert.ChangeType(newValue, ValueType);
            else
                Value = newValue;
        }

        protected override void OnDecrement()
        {
            double newValue = (double)(Convert.ToDecimal(Value) - (decimal)Increment);

            if (ValueType != typeof(Double))
                Value = Convert.ChangeType(newValue, ValueType);
            else
                Value = newValue;
        }

        #endregion //Base Class Overrides

        #region Methods

        /// <summary>
        /// Sets the valid spin direction based on current value, minimum and maximum.
        /// </summary>
        private void SetValidSpinDirection()
        {
            ValidSpinDirections validDirections = ValidSpinDirections.None;

            if (Convert.ToDouble(Value) < Maximum)
            {
                validDirections = validDirections | ValidSpinDirections.Increase;
            }

            if (Convert.ToDouble(Value) > Minimum)
            {
                validDirections = validDirections | ValidSpinDirections.Decrease;
            }

            if (Spinner != null)
            {
                Spinner.ValidSpinDirection = validDirections;
            }
        }

        private double TryParceDoublePercent(string text, NumberFormatInfo info)
        {
            double result;
            text = text.Replace(info.PercentSymbol, null);
            result = TryParceDouble(text, info);
            return result / 100;
        }

        private decimal TryParceDecimalPercent(string text, NumberFormatInfo info)
        {
            decimal result;
            text = text.Replace(info.PercentSymbol, null);
            result = TryParceDecimal(text, info);
            return result / 100;
        }

        private decimal TryParceDecimal(string text, NumberFormatInfo info)
        {
            decimal result;
            if (!decimal.TryParse(text, NumberStyles.Any, info, out result))
            {
                //an error occured now lets reset our value
                result = Convert.ToDecimal(Value);
                TextBox.Text = DisplayText = ConvertValueToText(result);
            }
            return result;
        }

        private double TryParceDouble(string text, NumberFormatInfo info)
        {
            double result;
            if (!double.TryParse(text, NumberStyles.Any, info, out result))
            {
                //an error occured now lets reset our value
                result = Convert.ToDouble(Value);
                TextBox.Text = DisplayText = ConvertValueToText(result);
            }
            return result;
        }

        private int TryParceInteger(string text, NumberFormatInfo info)
        {
            int result;
            if (!int.TryParse(text, NumberStyles.Any, info, out result))
            {
                //an error occured now lets reset our value
                result = Convert.ToInt32(Value);
                TextBox.Text = DisplayText = ConvertValueToText(result);
            }
            return result;
        }

        #endregion //Methods
    }
}
