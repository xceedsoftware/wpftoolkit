using System;
using System.Windows;

namespace Microsoft.Windows.Controls
{
    public class IntegerUpDown : NumericUpDown<int?>
    {
        #region Constructors

        static IntegerUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IntegerUpDown), new FrameworkPropertyMetadata(typeof(IntegerUpDown)));
            DefaultValueProperty.OverrideMetadata(typeof(IntegerUpDown), new FrameworkPropertyMetadata(0));
            IncrementProperty.OverrideMetadata(typeof(IntegerUpDown), new FrameworkPropertyMetadata(1));
            MaximumProperty.OverrideMetadata(typeof(IntegerUpDown), new FrameworkPropertyMetadata(int.MaxValue));
            MinimumProperty.OverrideMetadata(typeof(IntegerUpDown), new FrameworkPropertyMetadata(int.MinValue));
        }

        #endregion //Constructors

        #region Base Class Overrides

        protected override int? OnCoerceValue(int? value)
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

        protected override int? ConvertTextToValue(string text)
        {
            int? result = null;

            if (String.IsNullOrEmpty(text))
                return result;

            try
            {
                //don't know why someone would format an integer as %, but just in case they do.
                result = FormatString.Contains("P") ? Decimal.ToInt32(ParsePercent(text, CultureInfo)) : ParseInt(text, CultureInfo);
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
