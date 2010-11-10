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

        #region DisplayText

        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText", typeof(string), typeof(InputBase), new FrameworkPropertyMetadata(default(String), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDisplayTextPropertyChanged));
        public string DisplayText
        {
            get { return (string)this.GetValue(DisplayTextProperty); }
            set { this.SetValue(DisplayTextProperty, value); }
        }

        private static void OnDisplayTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InputBase input = (InputBase)d;
            input.OnDisplayTextChanged((string)e.OldValue, (string)e.NewValue);
            if (input._isInitialized)
                input.SyncTextAndValueProperties(e.Property, e.NewValue);
        }

        protected virtual void OnDisplayTextChanged(string previousValue, string currentValue)
        {

        }

        #endregion //DisplayText

        #region IsEditable

        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(InputBase), new PropertyMetadata(true));
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        #endregion //IsEditable

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
                SyncTextAndValueProperties(DisplayTextProperty, DisplayText);
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

        #region Methods

        protected void SyncTextAndValueProperties(DependencyProperty p, object newValue)
        {
            //prevents recursive syncing properties
            if (_isSyncingTextAndValueProperties)
                return;

            _isSyncingTextAndValueProperties = true;

            //this only occures when the user typed in the value
            if (InputBase.DisplayTextProperty == p)
            {
                SetValue(InputBase.ValueProperty, ConvertTextToValue(newValue.ToString()));
            }

            SetValue(InputBase.DisplayTextProperty, ConvertValueToText(newValue));

            _isSyncingTextAndValueProperties = false;
        }

        #endregion //Methods

        #region Abstract

        protected abstract object ConvertTextToValue(string text);

        protected abstract string ConvertValueToText(object value);

        #endregion //Abstract
    }
}
