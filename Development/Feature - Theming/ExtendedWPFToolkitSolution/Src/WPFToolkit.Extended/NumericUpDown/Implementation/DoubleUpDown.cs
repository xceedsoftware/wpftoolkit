using System;
using System.Windows;

namespace Microsoft.Windows.Controls
{
    public class DoubleUpDown : NumericUpDown<double?>
    {
        #region Constructors

        static DoubleUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DoubleUpDown), new FrameworkPropertyMetadata(typeof(DoubleUpDown)));
            DefaultValueProperty.OverrideMetadata(typeof(DoubleUpDown), new FrameworkPropertyMetadata(default(double)));
            IncrementProperty.OverrideMetadata(typeof(DoubleUpDown), new FrameworkPropertyMetadata(1d));
            MaximumProperty.OverrideMetadata(typeof(DoubleUpDown), new FrameworkPropertyMetadata(double.MaxValue));
            MinimumProperty.OverrideMetadata(typeof(DoubleUpDown), new FrameworkPropertyMetadata(double.MinValue));
        }

        #endregion //Constructors

        #region Base Class Overrides

        protected override double? CoerceValue(double? value)
        {
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
                result = FormatString.Contains("P") ? Decimal.ToDouble(ParsePercent(text, CultureInfo)) : ParseDouble(text, CultureInfo);
                result = CoerceValue(result);
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

        protected override void SetValidSpinDirection()
        {
            ValidSpinDirections validDirections = ValidSpinDirections.None;

            if (Value < Maximum || !Value.HasValue)
                validDirections = validDirections | ValidSpinDirections.Increase;

            if (Value > Minimum || !Value.HasValue)
                validDirections = validDirections | ValidSpinDirections.Decrease;

            if (Spinner != null)
                Spinner.ValidSpinDirection = validDirections;
        }

        protected override void ValidateValue(double? value)
        {
            if (value < Minimum)
                Value = Minimum;
            else if (value > Maximum)
                Value = Maximum;
        }

        #endregion //Base Class Overrides
    }
}
