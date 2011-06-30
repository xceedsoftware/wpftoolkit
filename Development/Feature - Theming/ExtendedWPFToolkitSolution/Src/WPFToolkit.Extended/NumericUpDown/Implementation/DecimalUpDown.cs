using System;
using System.Windows;

namespace Microsoft.Windows.Controls
{
    public class DecimalUpDown : NumericUpDown<decimal?>
    {
        #region Constructors

        static DecimalUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DecimalUpDown), new FrameworkPropertyMetadata(typeof(DecimalUpDown)));
            DefaultValueProperty.OverrideMetadata(typeof(DecimalUpDown), new FrameworkPropertyMetadata(default(decimal)));
            IncrementProperty.OverrideMetadata(typeof(DecimalUpDown), new FrameworkPropertyMetadata(1m));
            MaximumProperty.OverrideMetadata(typeof(DecimalUpDown), new FrameworkPropertyMetadata(decimal.MaxValue));
            MinimumProperty.OverrideMetadata(typeof(DecimalUpDown), new FrameworkPropertyMetadata(decimal.MinValue));
        }

        #endregion //Constructors

        #region Base Class Overrides

        protected override decimal? CoerceValue(decimal? value)
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

        protected override decimal? ConvertTextToValue(string text)
        {
            decimal? result = null;

            if (String.IsNullOrEmpty(text))
                return result;

            try
            {
                result = FormatString.Contains("P") ? ParsePercent(text, CultureInfo) : ParseDecimal(text, CultureInfo);
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

        protected override void ValidateValue(decimal? value)
        {
            if (value < Minimum)
                Value = Minimum;
            else if (value > Maximum)
                Value = Maximum;
        }

        #endregion //Base Class Overrides
    }
}
