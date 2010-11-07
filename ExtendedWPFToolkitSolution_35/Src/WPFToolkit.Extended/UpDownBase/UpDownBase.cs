using System;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;

namespace Microsoft.Windows.Controls
{
    public abstract class UpDownBase<T> : Control
    {
        #region Members

        /// <summary>
        /// Name constant for Text template part.
        /// </summary>
        internal const string ElementTextName = "Text";

        /// <summary>
        /// Name constant for Spinner template part.
        /// </summary>
        internal const string ElementSpinnerName = "Spinner";

        /// <summary>
        /// Flags if the Text and Value properties are in the process of being sync'd
        /// </summary>
        bool _isSyncingTextAndValueProperties;

        #endregion //Members

        #region Properties

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(T), typeof(UpDownBase<T>), new FrameworkPropertyMetadata(default(T), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValuePropertyChanged));
        public virtual T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpDownBase<T> udb = (UpDownBase<T>)d;
            T oldValue = (T)e.OldValue;
            T newValue = (T)e.NewValue;

            udb.SyncTextAndValueProperties(e.Property, e.NewValue);

            RoutedPropertyChangedEventArgs<T> changedArgs = new RoutedPropertyChangedEventArgs<T>(oldValue, newValue);
            udb.OnValueChanged(changedArgs);
        }

        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<T> e)
        {
            if (ValueChanged != null)
                ValueChanged(this, e);
        }

        #endregion //Value

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(UpDownBase<T>), new FrameworkPropertyMetadata("0", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextPropertyChanged));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpDownBase<T> udb = (UpDownBase<T>)d;
            udb.SyncTextAndValueProperties(e.Property, e.NewValue);
        }

        #endregion //Text

        #region IsEditable

        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(UpDownBase<T>), new PropertyMetadata(true, OnIsEditablePropertyChanged));
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        private static void OnIsEditablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpDownBase<T> source = d as UpDownBase<T>;
            source.OnIsEditableChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsEditableChanged(bool oldValue, bool newValue)
        {
            if (TextBox != null)
                TextBox.IsReadOnly = !IsEditable;
        }

        #endregion //IsEditable

        internal TextBox TextBox { get; private set; }

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

        protected UpDownBase() { }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TextBox = GetTemplateChild(ElementTextName) as TextBox;
            Spinner = GetTemplateChild(ElementSpinnerName) as Spinner;

            if (TextBox != null)
                TextBox.IsReadOnly = !IsEditable;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    {
                        DoIncrement();
                        e.Handled = true;
                        break;
                    }
                case Key.Down:
                    {
                        DoDecrement();
                        e.Handled = true;
                        break;
                    }
                case Key.Enter:
                    {
                        SyncTextAndValueProperties(UpDownBase<T>.TextProperty, TextBox.Text);
                        break;
                    }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!e.Handled)
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

        #region Methods

        #region Abstract

        /// <summary>
        /// Called by ApplyValue to parse user input.
        /// </summary>
        /// <param name="text">User input.</param>
        /// <returns>Value parsed from user input.</returns>
        protected abstract T ParseValue(string text);

        /// <summary>
        /// Renders the value property into the textbox text.
        /// </summary>
        /// <returns>Formatted Value.</returns>
        protected internal abstract string FormatValue();

        /// <summary>
        /// Called by OnSpin when the spin direction is SpinDirection.Increase.
        /// </summary>
        protected abstract void OnIncrement();

        /// <summary>
        /// Called by OnSpin when the spin direction is SpinDirection.Descrease.
        /// </summary>
        protected abstract void OnDecrement();

        #endregion //Abstract

        #region Protected

        /// <summary>
        /// GetValue override to return Value property as object type.
        /// </summary>
        /// <returns>The Value property as object type.</returns>
        protected object GetValue()
        {
            return Value;
        }

        /// <summary>
        /// SetValue override to set value to Value property.
        /// </summary>
        /// <param name="value">New value.</param>
        protected void SetValue(object value)
        {
            Value = (T)value;
        }

        #endregion //Protected

        #region Private

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

        protected void SyncTextAndValueProperties(DependencyProperty p, object newValue)
        {
            //prevents recursive syncing properties
            if (_isSyncingTextAndValueProperties)
                return;

            _isSyncingTextAndValueProperties = true;

            //this only occures when the user typed in the value
            if (UpDownBase<T>.TextProperty == p)
            {
                SetValue(UpDownBase<T>.ValueProperty, ParseValue(newValue.ToString()));
            }

            //we need to update the text no matter what because the user could have used the spin buttons to change dthe value
            //or typed in the textbox so we need to reformat the entered value.
            SetValue(UpDownBase<T>.TextProperty, FormatValue());

            _isSyncingTextAndValueProperties = false;
        }

        #endregion //Private

        #region Virtual

        /// <summary>
        /// Occurs when the spinner spins.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected virtual void OnSpin(SpinEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (e.Direction == SpinDirection.Increase)
                DoIncrement();
            else
                DoDecrement();
        }

        #endregion //Virtual

        #endregion //Methods

        #region Event Handlers

        /// <summary>
        /// Event handler for Spinner template part's Spin event.
        /// </summary>
        /// <param name="sender">The Spinner template part.</param>
        /// <param name="e">Event args.</param>
        private void OnSpinnerSpin(object sender, SpinEventArgs e)
        {
            OnSpin(e);
        }

        #endregion //Event Handlers

        #region Events

        /// <summary>
        /// Occurs when Value property has changed.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<T> ValueChanged;

        #endregion //Events
    }
}
