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

        protected override void CoerceValue(double? value)
        {
            if (value < Minimum)
                Value = Minimum;
            else if (value > Maximum)
                Value = Maximum;
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
