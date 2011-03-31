using System;
using System.Windows;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    [Obsolete("This control has been replaced with numeric data type specific controls; DecimalUpDown, DoubleUpDown, IntegerUpDown")]
    public class NumericUpDown : UpDownBase
    {
        #region Members

        /// <summary>
        /// Flags if the Text and Value properties are in the process of being sync'd
        /// </summary>
        private bool _isSyncingTextAndValueProperties;

        #endregion //Members

        #region Properties

        #region DefaultValue

        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(decimal), typeof(NumericUpDown), new UIPropertyMetadata(default(decimal)));
        public decimal DefaultValue
        {
            get { return (decimal)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        #endregion //DefaultValue

        #region Minimum

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(Decimal.MinValue, OnMinimumPropertyChanged));
        public decimal Minimum
        {
            get { return (decimal)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        private static void OnMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown nud = d as NumericUpDown;
            nud.SetValidSpinDirection();
        }

        #endregion Minimum

        #region Maximum

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(Decimal.MaxValue, OnMaximumPropertyChanged));
        public decimal Maximum
        {
            get { return (decimal)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        private static void OnMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown nud = d as NumericUpDown;
            nud.SetValidSpinDirection();
        }

        #endregion Maximum

        #region Increment

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(1M));
        public decimal Increment
        {
            get { return (decimal)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        #endregion

        #region FormatString

        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(NumericUpDown), new PropertyMetadata(string.Empty, OnStringFormatPropertyPropertyChanged));
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
            //Don't think this is needed anymore
            //SyncTextAndValueProperties(NumericUpDown.TextProperty, Value);
        }

        #endregion //FormatString

        #region SelectAllOnGotFocus

        public static readonly DependencyProperty SelectAllOnGotFocusProperty = DependencyProperty.Register("SelectAllOnGotFocus", typeof(bool), typeof(NumericUpDown), new PropertyMetadata(false));
        public bool SelectAllOnGotFocus
        {
            get { return (bool)GetValue(SelectAllOnGotFocusProperty); }
            set { SetValue(SelectAllOnGotFocusProperty, value); }
        }

        #endregion //SelectAllOnGotFocus

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(decimal?), typeof(NumericUpDown), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, OnCoerceValue));
        public decimal? Value
        {
            get { return (decimal?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static object OnCoerceValue(DependencyObject o, object value)
        {
            NumericUpDown numericUpDown = o as NumericUpDown;
            if (numericUpDown != null)
                return numericUpDown.OnCoerceValue((decimal?)value);
            else
                return value;
        }

        protected virtual decimal? OnCoerceValue(decimal? value)
        {
            if (value == null) return value;

            decimal val = value.Value;

            if (val < Minimum)
            {
                return Minimum;
            }
            else if (val > Maximum)
            {
                return Maximum;
            }
            else
            {
                return value;
            }
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown numericUpDown = o as NumericUpDown;
            if (numericUpDown != null)
                numericUpDown.OnValueChanged((decimal?)e.OldValue, (decimal?)e.NewValue);
        }

        protected virtual void OnValueChanged(decimal? oldValue, decimal? newValue)
        {
            SetValidSpinDirection();

            SyncTextAndValueProperties(NumericUpDown.ValueProperty, newValue);

            RoutedPropertyChangedEventArgs<decimal?> args = new RoutedPropertyChangedEventArgs<decimal?>(oldValue, newValue);
            args.RoutedEvent = NumericUpDown.ValueChangedEvent;
            RaiseEvent(args);
        }

        #endregion //Value

        #endregion

        #region Constructors

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
        }

        #endregion //Constructors

        #region Base Class Overrides

        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            if (TextBox != null)
                TextBox.Focus();

            base.OnAccessKey(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SetValidSpinDirection();

            if (SelectAllOnGotFocus)
            {
                //in order to select all the text we must handle both the keybord (tabbing) and mouse (clicking) events
                TextBox.GotKeyboardFocus += OnTextBoxGotKeyBoardFocus;
                TextBox.PreviewMouseLeftButtonDown += OnTextBoxPreviewMouseLeftButtonDown;
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (TextBox != null)
                TextBox.Focus();
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

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Enter)
            {
                if (IsEditable)
                    SyncTextAndValueProperties(InputBase.TextProperty, TextBox.Text);
            }
        }

        protected override void OnTextChanged(string previousValue, string currentValue)
        {
            SyncTextAndValueProperties(InputBase.TextProperty, currentValue);
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

        #region Events

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<decimal?>), typeof(NumericUpDown));
        public event RoutedPropertyChangedEventHandler<decimal?> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        #endregion //Events

        #region Methods

        private decimal? ConvertTextToValue(string text)
        {
            decimal? result = null;

            if (String.IsNullOrEmpty(text))
                return result;

            NumberFormatInfo info = NumberFormatInfo.GetInstance(CultureInfo.CurrentCulture);

            try
            {
                result = FormatString.Contains("P") ? ParsePercent(text, info) : ParseDecimal(text, info);
            }
            catch
            {
                Text = ConvertValueToText(Value);
                return Value;
            }

            return result;
        }

        private string ConvertValueToText(object value)
        {
            if (!Value.HasValue)
                return String.Empty;

            return Value.Value.ToString(FormatString, CultureInfo.CurrentCulture);
        }

        private void SyncTextAndValueProperties(DependencyProperty p, object newValue)
        {
            //prevents recursive syncing properties
            if (_isSyncingTextAndValueProperties)
                return;

            _isSyncingTextAndValueProperties = true;

            //this only occures when the user typed in the value
            if (InputBase.TextProperty == p)
            {
                string text = newValue == null ? String.Empty : newValue.ToString();
                SetValue(NumericUpDown.ValueProperty, ConvertTextToValue(text));
            }

            SetValue(InputBase.TextProperty, ConvertValueToText(newValue));

            _isSyncingTextAndValueProperties = false;
        }

        private static decimal ParseDecimal(string text, NumberFormatInfo info)
        {
            return decimal.Parse(text, NumberStyles.Any, info);
        }

        private static decimal ParsePercent(string text, NumberFormatInfo info)
        {
            text = text.Replace(info.PercentSymbol, null);

            decimal result = decimal.Parse(text, NumberStyles.Any, info);
            result = result / 100;

            return result;
        }

        /// <summary>
        /// Sets the valid spin direction based on current value, minimum and maximum.
        /// </summary>
        private void SetValidSpinDirection()
        {
            ValidSpinDirections validDirections = ValidSpinDirections.None;

            if (Convert.ToDecimal(Value) < Maximum)
            {
                validDirections = validDirections | ValidSpinDirections.Increase;
            }

            if (Convert.ToDecimal(Value) > Minimum)
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

        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(NumericUpDown<T>), new UIPropertyMetadata(String.Empty));
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
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
            NumericUpDown<T> integerUpDown = o as NumericUpDown<T>;
            if (integerUpDown != null)
                integerUpDown.OnMaximumChanged((T)e.OldValue, (T)e.NewValue);
        }

        protected virtual void OnMaximumChanged(T oldValue, T newValue)
        {
            //SetValidSpinDirection();
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
            NumericUpDown<T> integerUpDown = o as NumericUpDown<T>;
            if (integerUpDown != null)
                integerUpDown.OnMinimumChanged((T)e.OldValue, (T)e.NewValue);
        }

        protected virtual void OnMinimumChanged(T oldValue, T newValue)
        {
            //SetValidSpinDirection();
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
