using System;
using System.Windows.Controls;
using System.Windows;

namespace Microsoft.Windows.Controls.Primitives
{
    public abstract class InputBase : Control
    {
        #region Members

        /// <summary>
        /// Flags if the Text and Value properties are in the process of being sync'd
        /// </summary>
        private bool _isSyncingTextAndValueProperties;
        private bool _isInitialized;

        #endregion //Members

        #region Properties

        public virtual object PreviousValue { get; internal set; }

        #region IsEditable

        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(InputBase), new PropertyMetadata(true));
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        #endregion //IsEditable

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(InputBase), new FrameworkPropertyMetadata(default(String), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextPropertyChanged));
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InputBase input = (InputBase)d;
            input.OnTextChanged((string)e.OldValue, (string)e.NewValue);
            if (input._isInitialized)
                input.SyncTextAndValueProperties(e.Property, e.NewValue);
        }

        protected virtual void OnTextChanged(string previousValue, string currentValue)
        {

        }

        #endregion //Text

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(InputBase), new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValuePropertyChanged, OnCoerceValuePropertyCallback));
        public virtual object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InputBase input = (InputBase)d;

            if (e.OldValue != e.NewValue)
            {
                input.PreviousValue = e.OldValue;
                input.OnValueChanged(e.OldValue, e.NewValue);

                if (input._isInitialized)
                    input.SyncTextAndValueProperties(e.Property, e.NewValue);
            }
        }

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
            RoutedPropertyChangedEventArgs<object> args = new RoutedPropertyChangedEventArgs<object>(oldValue, newValue);
            args.RoutedEvent = InputBase.ValueChangedEvent;
            RaiseEvent(args);
        }

        private static object OnCoerceValuePropertyCallback(DependencyObject d, object baseValue)
        {
            InputBase inputBase = d as InputBase;
            if (inputBase != null)
                return inputBase.OnCoerceValue(baseValue);
            else
                return baseValue;
        }

        protected virtual object OnCoerceValue(object value)
        {
            return value;
        }

        #endregion //Value

        #region ValueType

        public static readonly DependencyProperty ValueTypeProperty = DependencyProperty.Register("ValueType", typeof(Type), typeof(InputBase), new FrameworkPropertyMetadata(typeof(String), FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(OnValueTypePropertyChanged)));
        public Type ValueType
        {
            get { return (Type)GetValue(ValueTypeProperty); }
            set { SetValue(ValueTypeProperty, value); }
        }

        private static void OnValueTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InputBase input = (InputBase)d;
            input.OnValueTypeChanged((Type)e.OldValue, (Type)e.NewValue);
        }

        protected virtual void OnValueTypeChanged(Type oldValue, Type newType)
        {
            if (_isInitialized)
                SyncTextAndValueProperties(TextProperty, Text);
        }

        #endregion //ValueType

        #endregion //Properties

        #region Base Class Overrides

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (!_isInitialized)
            {
                _isInitialized = true;
                SyncTextAndValueProperties(ValueProperty, Value);
            }
        }

        #endregion //Base Class Overrides

        #region Events

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(InputBase));
        public event RoutedPropertyChangedEventHandler<object> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        #endregion //Events

        #region Methods

        protected void SyncTextAndValueProperties(DependencyProperty p, object newValue)
        {
            //prevents recursive syncing properties
            if (_isSyncingTextAndValueProperties)
                return;

            _isSyncingTextAndValueProperties = true;

            //this only occures when the user typed in the value
            if (InputBase.TextProperty == p)
            {
                SetValue(InputBase.ValueProperty, ConvertTextToValue(newValue.ToString()));
            }

            SetValue(InputBase.TextProperty, ConvertValueToText(newValue));

            _isSyncingTextAndValueProperties = false;
        }

        #endregion //Methods

        #region Abstract

        protected abstract object ConvertTextToValue(string text);

        protected abstract string ConvertValueToText(object value);

        #endregion //Abstract
    }
}
