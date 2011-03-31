using System;
using System.Windows.Controls;
using Microsoft.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;

namespace Microsoft.Windows.Controls.Primitives
{
    public abstract class UpDownBase : InputBase
    {
        #region Members

        /// <summary>
        /// Name constant for Text template part.
        /// </summary>
        internal const string ElementTextName = "TextBox";

        /// <summary>
        /// Name constant for Spinner template part.
        /// </summary>
        internal const string ElementSpinnerName = "Spinner";

        #endregion //Members

        #region Properties

        #region AllowSpin

        public static readonly DependencyProperty AllowSpinProperty = DependencyProperty.Register("AllowSpin", typeof(bool), typeof(UpDownBase), new UIPropertyMetadata(true));
        public bool AllowSpin
        {
            get { return (bool)GetValue(AllowSpinProperty); }
            set { SetValue(AllowSpinProperty, value); }
        }

        #endregion //AllowSpin

        #region ShowButtonSpinner

        public static readonly DependencyProperty ShowButtonSpinnerProperty = DependencyProperty.Register("ShowButtonSpinner", typeof(bool), typeof(UpDownBase), new UIPropertyMetadata(true));
        public bool ShowButtonSpinner
        {
            get { return (bool)GetValue(ShowButtonSpinnerProperty); }
            set { SetValue(ShowButtonSpinnerProperty, value); }
        }

        #endregion //ShowButtonSpinner

        protected TextBox TextBox { get; private set; }

        private Spinner _spinner;
        internal Spinner Spinner
        {
            get { return _spinner; }
            private set
            {
                _spinner = value;
                _spinner.Spin += OnSpinnerSpin;
            }
        }

        #endregion //Properties

        #region Constructors

        internal UpDownBase() { }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TextBox = GetTemplateChild(ElementTextName) as TextBox;
            Spinner = GetTemplateChild(ElementSpinnerName) as Spinner;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    {
                        if (AllowSpin)
                            DoIncrement();
                        e.Handled = true;
                        break;
                    }
                case Key.Down:
                    {
                        if (AllowSpin)
                            DoDecrement();
                        e.Handled = true;
                        break;
                    }
                //case Key.Enter:
                //    {
                //        if (IsEditable)
                //            SyncTextAndValueProperties(UpDownBase.TextProperty, TextBox.Text);
                //        break;
                //    }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!e.Handled && AllowSpin)
            {
                if (e.Delta < 0)
                {
                    DoDecrement();
                }
                else if (0 < e.Delta)
                {
                    DoIncrement();
                }

                e.Handled = true;
            }
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        private void OnSpinnerSpin(object sender, SpinEventArgs e)
        {
            if (AllowSpin)
                OnSpin(e);
        }

        #endregion //Event Handlers

        #region Methods

        protected virtual void OnSpin(SpinEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (e.Direction == SpinDirection.Increase)
                DoIncrement();
            else
                DoDecrement();
        }

        /// <summary>
        /// Performs an increment if conditions allow it.
        /// </summary>
        private void DoDecrement()
        {
            if (Spinner == null || (Spinner.ValidSpinDirection & ValidSpinDirections.Decrease) == ValidSpinDirections.Decrease)
            {
                OnDecrement();
            }
        }

        /// <summary>
        /// Performs a decrement if conditions allow it.
        /// </summary>
        private void DoIncrement()
        {
            if (Spinner == null || (Spinner.ValidSpinDirection & ValidSpinDirections.Increase) == ValidSpinDirections.Increase)
            {
                OnIncrement();
            }
        }

        #region Abstract

        /// <summary>
        /// Called by OnSpin when the spin direction is SpinDirection.Increase.
        /// </summary>
        protected abstract void OnIncrement();

        /// <summary>
        /// Called by OnSpin when the spin direction is SpinDirection.Descrease.
        /// </summary>
        protected abstract void OnDecrement();

        #endregion //Abstract

        #endregion //Methods
    }

    /// <summary>
    /// This is going to be the new UpDownBase class.  In order to reliably support the different numeric data types, I need to create a specific control for each data type.
    /// So instead of copying and pasting I am going to try to create a generic base class that has most of the logic in it.  This will make the NumericUpDown control obsolete
    /// and it will be removed from the toolkit.  The new controls will be DecimalUpDown, DoubleUpDown, and IntegerUpDown.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UpDownBase<T> : InputBase
    {
        #region Members

        /// <summary>
        /// Name constant for Text template part.
        /// </summary>
        internal const string ElementTextName = "TextBox";

        /// <summary>
        /// Name constant for Spinner template part.
        /// </summary>
        internal const string ElementSpinnerName = "Spinner";

        /// <summary>
        /// Flags if the Text and Value properties are in the process of being sync'd
        /// </summary>
        private bool _isSyncingTextAndValueProperties;

        #endregion //Members

        #region Properties

        protected TextBox TextBox { get; private set; }
        internal Spinner Spinner { get; private set; }

        #region AllowSpin

        public static readonly DependencyProperty AllowSpinProperty = DependencyProperty.Register("AllowSpin", typeof(bool), typeof(UpDownBase<T>), new UIPropertyMetadata(true));
        public bool AllowSpin
        {
            get { return (bool)GetValue(AllowSpinProperty); }
            set { SetValue(AllowSpinProperty, value); }
        }

        #endregion //AllowSpin

        #region ShowButtonSpinner

        public static readonly DependencyProperty ShowButtonSpinnerProperty = DependencyProperty.Register("ShowButtonSpinner", typeof(bool), typeof(UpDownBase<T>), new UIPropertyMetadata(true));
        public bool ShowButtonSpinner
        {
            get { return (bool)GetValue(ShowButtonSpinnerProperty); }
            set { SetValue(ShowButtonSpinnerProperty, value); }
        }

        #endregion //ShowButtonSpinner

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(T), typeof(UpDownBase<T>), new FrameworkPropertyMetadata(default(T), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, OnCoerceValue));
        public T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static object OnCoerceValue(DependencyObject o, object value)
        {
            UpDownBase<T> upDownBase = o as UpDownBase<T>;
            if (upDownBase != null)
                return upDownBase.OnCoerceValue((T)value);
            else
                return value;
        }

        protected virtual T OnCoerceValue(T value)
        {
            return value;
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            UpDownBase<T> upDownBase = o as UpDownBase<T>;
            if (upDownBase != null)
                upDownBase.OnValueChanged((T)e.OldValue, (T)e.NewValue);
        }

        protected virtual void OnValueChanged(T oldValue, T newValue)
        {
            SyncTextAndValueProperties(NumericUpDown.ValueProperty, newValue);

            RoutedPropertyChangedEventArgs<T> args = new RoutedPropertyChangedEventArgs<T>(oldValue, newValue);
            args.RoutedEvent = NumericUpDown.ValueChangedEvent;
            RaiseEvent(args);
        }        

        #endregion //Value

        #endregion //Properties

        #region Constructors

        internal UpDownBase() { }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TextBox = GetTemplateChild(ElementTextName) as TextBox;
            Spinner = GetTemplateChild(ElementSpinnerName) as Spinner;
            Spinner.Spin += OnSpinnerSpin;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (TextBox != null)
                TextBox.Focus();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    {
                        if (AllowSpin)
                            DoIncrement();
                        e.Handled = true;
                        break;
                    }
                case Key.Down:
                    {
                        if (AllowSpin)
                            DoDecrement();
                        e.Handled = true;
                        break;
                    }
                case Key.Enter:
                    {
                        if (IsEditable)
                            SyncTextAndValueProperties(UpDownBase.TextProperty, TextBox.Text);
                        break;
                    }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!e.Handled && AllowSpin)
            {
                if (e.Delta < 0)
                {
                    DoDecrement();
                }
                else if (0 < e.Delta)
                {
                    DoIncrement();
                }

                e.Handled = true;
            }
        }

        protected override void OnTextChanged(string oldValue, string newValue)
        {
            SyncTextAndValueProperties(InputBase.TextProperty, newValue);
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        private void OnSpinnerSpin(object sender, SpinEventArgs e)
        {
            if (AllowSpin)
                OnSpin(e);
        }

        #endregion //Event Handlers

        #region Events

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<T>), typeof(UpDownBase<T>));
        public event RoutedPropertyChangedEventHandler<T> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        #endregion //Events

        #region Methods

        protected virtual void OnSpin(SpinEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (e.Direction == SpinDirection.Increase)
                DoIncrement();
            else
                DoDecrement();
        }

        /// <summary>
        /// Performs an increment if conditions allow it.
        /// </summary>
        private void DoDecrement()
        {
            if (Spinner == null || (Spinner.ValidSpinDirection & ValidSpinDirections.Decrease) == ValidSpinDirections.Decrease)
            {
                OnDecrement();
            }
        }

        /// <summary>
        /// Performs a decrement if conditions allow it.
        /// </summary>
        private void DoIncrement()
        {
            if (Spinner == null || (Spinner.ValidSpinDirection & ValidSpinDirections.Increase) == ValidSpinDirections.Increase)
            {
                OnIncrement();
            }
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
                SetValue(UpDownBase<T>.ValueProperty, ConvertTextToValue(text));
            }

            SetValue(InputBase.TextProperty, ConvertValueToText(newValue));

            _isSyncingTextAndValueProperties = false;
        }

        #region Abstract

        /// <summary>
        /// Called by OnSpin when the spin direction is SpinDirection.Increase.
        /// </summary>
        protected abstract void OnIncrement();

        /// <summary>
        /// Called by OnSpin when the spin direction is SpinDirection.Descrease.
        /// </summary>
        protected abstract void OnDecrement();

        protected abstract T ConvertTextToValue(string text);

        protected abstract string ConvertValueToText(object value);

        #endregion //Abstract

        #endregion //Methods
    }
}
