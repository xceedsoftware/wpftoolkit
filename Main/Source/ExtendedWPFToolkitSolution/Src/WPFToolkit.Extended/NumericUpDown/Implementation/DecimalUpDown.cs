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

        protected override decimal? OnCoerceValue(decimal? value)
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

        #endregion //Base Class Overrides
    }
}
