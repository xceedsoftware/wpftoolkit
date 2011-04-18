using System;
using System.Windows.Controls;
using Microsoft.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Windows.Controls.Primitives
{
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

        protected Spinner Spinner { get; private set; }
        protected TextBox TextBox { get; private set; }

        #region AllowSpin

        public static readonly DependencyProperty AllowSpinProperty = DependencyProperty.Register("AllowSpin", typeof(bool), typeof(UpDownBase<T>), new UIPropertyMetadata(true));
        public bool AllowSpin
        {
            get { return (bool)GetValue(AllowSpinProperty); }
            set { SetValue(AllowSpinProperty, value); }
        }

        #endregion //AllowSpin

        #region MouseWheelActiveOnFocus

        public static readonly DependencyProperty MouseWheelActiveOnFocusProperty = DependencyProperty.Register("MouseWheelActiveOnFocus", typeof(bool), typeof(UpDownBase<T>), new UIPropertyMetadata(true));
        public bool MouseWheelActiveOnFocus
        {
            get { return (bool)GetValue(MouseWheelActiveOnFocusProperty); }
            set { SetValue(MouseWheelActiveOnFocusProperty, value); }
        }        

        #endregion //MouseWheelActiveOnFocus

        #region ShowButtonSpinner

        public static readonly DependencyProperty ShowButtonSpinnerProperty = DependencyProperty.Register("ShowButtonSpinner", typeof(bool), typeof(UpDownBase<T>), new UIPropertyMetadata(true));
        public bool ShowButtonSpinner
        {
            get { return (bool)GetValue(ShowButtonSpinnerProperty); }
            set { SetValue(ShowButtonSpinnerProperty, value); }
        }

        #endregion //ShowButtonSpinner

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(T), typeof(UpDownBase<T>), new FrameworkPropertyMetadata(default(T), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));
        public T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            UpDownBase<T> upDownBase = o as UpDownBase<T>;
            if (upDownBase != null)
                upDownBase.OnValueChanged((T)e.OldValue, (T)e.NewValue);
        }

        protected virtual void OnValueChanged(T oldValue, T newValue)
        {
            ValidateValue(newValue);

            SyncTextAndValueProperties(ValueProperty, string.Empty);

            SetValidSpinDirection();

            RoutedPropertyChangedEventArgs<object> args = new RoutedPropertyChangedEventArgs<object>(oldValue, newValue);
            args.RoutedEvent = ValueChangedEvent;
            RaiseEvent(args);
        }

        #endregion //Value

        #endregion //Properties

        #region Constructors

        internal UpDownBase() { }

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

            TextBox = GetTemplateChild(ElementTextName) as TextBox;
            Spinner = GetTemplateChild(ElementSpinnerName) as Spinner;
            Spinner.Spin += OnSpinnerSpin;

            SetValidSpinDirection();
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
                        {
                            var binding = BindingOperations.GetBindingExpression(TextBox, System.Windows.Controls.TextBox.TextProperty);
                            binding.UpdateSource();
                        }
                        break;
                    }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!e.Handled && AllowSpin && ((TextBox.IsFocused && MouseWheelActiveOnFocus) || !MouseWheelActiveOnFocus))
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

        //Due to a bug in Visual Studio, you cannot create event handlers for generic T args in XAML, so I have to use object instead.
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(UpDownBase<T>));
        public event RoutedPropertyChangedEventHandler<object> ValueChanged
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

        protected void SyncTextAndValueProperties(DependencyProperty p, string text)
        {
            //prevents recursive syncing properties
            if (_isSyncingTextAndValueProperties)
                return;

            _isSyncingTextAndValueProperties = true;

            //this only occures when the user typed in the value
            if (InputBase.TextProperty == p)
            {
                Value = ConvertTextToValue(text);
            }

            Text = ConvertValueToText();

#if VS2008
            //there is a bug in .NET 3.5 which will not correctly update the textbox text through binding.
            if (TextBox != null)
                TextBox.Text = Text;
#endif

            _isSyncingTextAndValueProperties = false;
        }

        #region Abstract

        /// <summary>
        /// Coerces the value.
        /// </summary>
        protected abstract T CoerceValue(T value);

        /// <summary>
        /// Converts the formatted text to a value.
        /// </summary>
        protected abstract T ConvertTextToValue(string text);

        /// <summary>
        /// Converts the value to formatted text.
        /// </summary>
        /// <returns></returns>
        protected abstract string ConvertValueToText();

        /// <summary>
        /// Called by OnSpin when the spin direction is SpinDirection.Increase.
        /// </summary>
        protected abstract void OnIncrement();

        /// <summary>
        /// Called by OnSpin when the spin direction is SpinDirection.Descrease.
        /// </summary>
        protected abstract void OnDecrement();

        /// <summary>
        /// Sets the valid spin directions.
        /// </summary>
        protected abstract void SetValidSpinDirection();

        /// <summary>
        /// Validates the value and keeps it between the Min and Max values.
        /// </summary>
        /// <param name="value">The value.</param>
        protected abstract void ValidateValue(T value);

        #endregion //Abstract

        #endregion //Methods
    }
}
