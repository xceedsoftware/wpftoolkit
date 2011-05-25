using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Windows.Controls.Primitives;
using System.Globalization;

namespace Microsoft.Windows.Controls
{
    public abstract class NumericUpDown<T> : UpDownBase<T>
    {
        #region Properties

        #region DefaultValue

        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(T), typeof(NumericUpDown<T>), new UIPropertyMetadata(default(T)));
        public T DefaultValue
        {
            get { return (T)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        #endregion //DefaultValue

        #region FormatString

        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(NumericUpDown<T>), new UIPropertyMetadata(String.Empty, OnFormatStringChanged));
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        private static void OnFormatStringChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown<T> numericUpDown = o as NumericUpDown<T>;
            if (numericUpDown != null)
                numericUpDown.OnFormatStringChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnFormatStringChanged(string oldValue, string newValue)
        {
            if (IsInitialized)
                Text = ConvertValueToText();
        }

        #endregion //FormatString

        #region Increment

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(T), typeof(NumericUpDown<T>), new PropertyMetadata(default(T)));
        public T Increment
        {
            get { return (T)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        #endregion

        #region Maximum

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(T), typeof(NumericUpDown<T>), new UIPropertyMetadata(default(T), OnMaximumChanged));
        public T Maximum
        {
            get { return (T)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        private static void OnMaximumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown<T> numericUpDown = o as NumericUpDown<T>;
            if (numericUpDown != null)
                numericUpDown.OnMaximumChanged((T)e.OldValue, (T)e.NewValue);
        }

        protected virtual void OnMaximumChanged(T oldValue, T newValue)
        {
            SetValidSpinDirection();
        }

        #endregion //Maximum

        #region Minimum

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(T), typeof(NumericUpDown<T>), new UIPropertyMetadata(default(T), OnMinimumChanged));
        public T Minimum
        {
            get { return (T)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        private static void OnMinimumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown<T> numericUpDown = o as NumericUpDown<T>;
            if (numericUpDown != null)
                numericUpDown.OnMinimumChanged((T)e.OldValue, (T)e.NewValue);
        }

        protected virtual void OnMinimumChanged(T oldValue, T newValue)
        {
            SetValidSpinDirection();
        }

        #endregion //Minimum

        #region SelectAllOnGotFocus

        public static readonly DependencyProperty SelectAllOnGotFocusProperty = DependencyProperty.Register("SelectAllOnGotFocus", typeof(bool), typeof(NumericUpDown<T>), new PropertyMetadata(false));
        public bool SelectAllOnGotFocus
        {
            get { return (bool)GetValue(SelectAllOnGotFocusProperty); }
            set { SetValue(SelectAllOnGotFocusProperty, value); }
        }

        #endregion //SelectAllOnGotFocus

        #endregion //Properties

        #region Methods

        protected static decimal ParseDecimal(string text, IFormatProvider cultureInfo)
        {
            return Decimal.Parse(text, NumberStyles.Any, cultureInfo);
        }

        protected static double ParseDouble(string text, IFormatProvider cultureInfo)
        {
            return Double.Parse(text, NumberStyles.Any, cultureInfo);
        }

        protected static int ParseInt(string text, IFormatProvider cultureInfo)
        {
            return Int32.Parse(text, NumberStyles.Any, cultureInfo);
        }

        protected static decimal ParsePercent(string text, IFormatProvider cultureInfo)
        {
            NumberFormatInfo info = NumberFormatInfo.GetInstance(cultureInfo);

            text = text.Replace(info.PercentSymbol, null);

            decimal result = Decimal.Parse(text, NumberStyles.Any, info);
            result = result / 100;

            return result;
        }

        #endregion //Methods
    }
}
