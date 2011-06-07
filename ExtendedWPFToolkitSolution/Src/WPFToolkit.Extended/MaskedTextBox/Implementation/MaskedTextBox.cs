using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    public class MaskedTextBox : InputBase
    {
        #region Members

        /// <summary>
        /// Flags if the Text and Value properties are in the process of being sync'd
        /// </summary>
        private bool _isSyncingTextAndValueProperties;
        private bool _isInitialized;

        #endregion //Members

        #region Properties

        protected MaskedTextProvider MaskProvider { get; set; }
        private TextBox TextBox { get; set; }

        #region IncludePrompt

        public static readonly DependencyProperty IncludePromptProperty = DependencyProperty.Register("IncludePrompt", typeof(bool), typeof(MaskedTextBox), new UIPropertyMetadata(false, OnIncludePromptPropertyChanged));
        public bool IncludePrompt
        {
            get { return (bool)GetValue(IncludePromptProperty); }
            set { SetValue(IncludePromptProperty, value); }
        }

        private static void OnIncludePromptPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MaskedTextBox maskedTextBox = o as MaskedTextBox;
            if (maskedTextBox != null)
                maskedTextBox.OnIncludePromptChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIncludePromptChanged(bool oldValue, bool newValue)
        {
            ResolveMaskProvider(Mask);
        }

        #endregion //IncludePrompt

        #region IncludeLiterals

        public static readonly DependencyProperty IncludeLiteralsProperty = DependencyProperty.Register("IncludeLiterals", typeof(bool), typeof(MaskedTextBox), new UIPropertyMetadata(true, OnIncludeLiteralsPropertyChanged));
        public bool IncludeLiterals
        {
            get { return (bool)GetValue(IncludeLiteralsProperty); }
            set { SetValue(IncludeLiteralsProperty, value); }
        }

        private static void OnIncludeLiteralsPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MaskedTextBox maskedTextBox = o as MaskedTextBox;
            if (maskedTextBox != null)
                maskedTextBox.OnIncludeLiteralsChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIncludeLiteralsChanged(bool oldValue, bool newValue)
        {
            ResolveMaskProvider(Mask);
        }

        #endregion //IncludeLiterals

        #region Mask

        public static readonly DependencyProperty MaskProperty = DependencyProperty.Register("Mask", typeof(string), typeof(MaskedTextBox), new UIPropertyMetadata(default(String), OnMaskPropertyChanged));
        public string Mask
        {
            get { return (string)GetValue(MaskProperty); }
            set { SetValue(MaskProperty, value); }
        }

        private static void OnMaskPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MaskedTextBox maskedTextBox = o as MaskedTextBox;
            if (maskedTextBox != null)
                maskedTextBox.OnMaskChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnMaskChanged(string oldValue, string newValue)
        {
            ResolveMaskProvider(newValue);
            UpdateText(MaskProvider, 0);
        }

        #endregion //Mask

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(MaskedTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MaskedTextBox maskedTextBox = o as MaskedTextBox;
            if (maskedTextBox != null)
                maskedTextBox.OnValueChanged((object)e.OldValue, (object)e.NewValue);
        }

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
            if (_isInitialized)
                SyncTextAndValueProperties(DateTimeUpDown.ValueProperty, newValue);

            RoutedPropertyChangedEventArgs<object> args = new RoutedPropertyChangedEventArgs<object>(oldValue, newValue);
            args.RoutedEvent = MaskedTextBox.ValueChangedEvent;
            RaiseEvent(args);
        }

        #endregion //Value

        #region ValueType

        public static readonly DependencyProperty ValueTypeProperty = DependencyProperty.Register("ValueType", typeof(Type), typeof(MaskedTextBox), new UIPropertyMetadata(typeof(String), OnValueTypeChanged));
        public Type ValueType
        {
            get { return (Type)GetValue(ValueTypeProperty); }
            set { SetValue(ValueTypeProperty, value); }
        }

        private static void OnValueTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MaskedTextBox maskedTextBox = o as MaskedTextBox;
            if (maskedTextBox != null)
                maskedTextBox.OnValueTypeChanged((Type)e.OldValue, (Type)e.NewValue);
        }

        protected virtual void OnValueTypeChanged(Type oldValue, Type newValue)
        {
            if (_isInitialized)
                SyncTextAndValueProperties(InputBase.TextProperty, Text);
        }

        #endregion //ValueType

        #endregion //Properties

        #region Constructors

        static MaskedTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MaskedTextBox), new FrameworkPropertyMetadata(typeof(MaskedTextBox)));
        }

        #endregion //Constructors

        #region Overrides

        public override void OnApplyTemplate()
        {
            TextBox = GetTemplateChild("TextBox") as TextBox;
            TextBox.PreviewTextInput += TextBox_PreviewTextInput;
            TextBox.PreviewKeyDown += TextBox_PreviewKeyDown;

            TextBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, Paste)); //handle paste
            TextBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, null, CanCut)); //surpress cut
        }

        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            if (TextBox != null)
                TextBox.Focus();

            base.OnAccessKey(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (TextBox != null)
                TextBox.Focus();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (!_isInitialized)
            {
                _isInitialized = true;
                SyncTextAndValueProperties(ValueProperty, Value);
            }
        }

        protected override void OnTextChanged(string previousValue, string currentValue)
        {
            if (_isInitialized)
                SyncTextAndValueProperties(InputBase.TextProperty, currentValue);
        }

        #endregion

        #region Event Handlers

        void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //if the text is readonly do not add the text
            if (!IsEditable)
            {
                e.Handled = true;
                return;
            }

            int position = TextBox.SelectionStart;
            MaskedTextProvider provider = MaskProvider;
            if (position < TextBox.Text.Length)
            {
                position = GetNextCharacterPosition(position);

                if (Keyboard.IsKeyToggled(Key.Insert))
                {
                    if (provider.Replace(e.Text, position))
                        position++;
                }
                else
                {
                    if (provider.InsertAt(e.Text, position))
                        position++;
                }

                position = GetNextCharacterPosition(position);
            }

            UpdateText(provider, position);
            e.Handled = true;

            base.OnPreviewTextInput(e);
        }

        void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsEditable)
                return;

            MaskedTextProvider provider = MaskProvider;
            int position = TextBox.SelectionStart;
            int selectionlength = TextBox.SelectionLength;
            // If no selection use the start position else use end position
            int endposition = (selectionlength == 0) ? position : position + selectionlength - 1;

            if (e.Key == Key.Delete && position < TextBox.Text.Length)//handle the delete key
            {
                if (provider.RemoveAt(position, endposition))
                    UpdateText(provider, position);

                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                if (provider.InsertAt(" ", position))
                    UpdateText(provider, position);
                e.Handled = true;
            }
            else if (e.Key == Key.Back)//handle the back space
            {
                if ((position > 0) && (selectionlength == 0))
                {
                    position--;
                    if (provider.RemoveAt(position))
                        UpdateText(provider, position);
                }

                if (selectionlength != 0)
                {
                    if (provider.RemoveAt(position, endposition))
                    {
                        if (position > 0)
                            position--;

                        UpdateText(provider, position);
                    }
                }

                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
        }

        #endregion //Event Handlers

        #region Events

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(MaskedTextBox));
        public event RoutedPropertyChangedEventHandler<object> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        #endregion //Events

        #region Methods

        #region Private

        private void UpdateText(MaskedTextProvider provider, int position)
        {
            if (provider == null)
                throw new ArgumentNullException("MaskedTextProvider", "Mask cannot be null.");

            Text = provider.ToDisplayString();

            if (TextBox != null)
                TextBox.SelectionStart = position;
        }

        private int GetNextCharacterPosition(int startPosition)
        {
            int position = MaskProvider.FindEditPositionFrom(startPosition, true);
            return position == -1 ? startPosition : position;
        }

        private void ResolveMaskProvider(string mask)
        {
            //do not create a mask provider if the Mask is empty, which can occur if the IncludePrompt and IncludeLiterals properties
            //are set prior to the Mask.
            if (String.IsNullOrEmpty(mask))
                return;

            MaskProvider = new MaskedTextProvider(mask)
            {
                IncludePrompt = this.IncludePrompt,
                IncludeLiterals = this.IncludeLiterals
            };
        }

        private object ConvertTextToValue(string text)
        {
            object convertedValue = null;

            Type dataType = ValueType;

            string valueToConvert = MaskProvider.ToString().Trim();

            if (valueToConvert.GetType() == dataType || dataType.IsInstanceOfType(valueToConvert))
            {
                convertedValue = valueToConvert;
            }
#if !VS2008
            else if (String.IsNullOrWhiteSpace(valueToConvert))
            {
                convertedValue = Activator.CreateInstance(dataType);
            }
#else
            else if (String.IsNullOrEmpty(valueToConvert))
            {
                convertedValue = Activator.CreateInstance(dataType);
            }
#endif
            else if (null == convertedValue && valueToConvert is IConvertible)
            {
                convertedValue = Convert.ChangeType(valueToConvert, dataType);
            }

            return convertedValue;
        }

        private string ConvertValueToText(object value)
        {
            if (value == null)
                value = string.Empty;

            //I have only seen this occur while in Blend, but we need it here so the Blend designer doesn't crash.
            if (MaskProvider == null)
                return value.ToString();

            MaskProvider.Set(value.ToString());
            return MaskProvider.ToDisplayString();
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
                if (newValue != null)
                    SetValue(MaskedTextBox.ValueProperty, ConvertTextToValue(newValue.ToString()));
            }

            SetValue(InputBase.TextProperty, ConvertValueToText(newValue));

            _isSyncingTextAndValueProperties = false;
        }

        #endregion //Private

        #region Public

        /// <summary>
        /// Attempts to set focus to this element.
        /// </summary>
        public new void Focus()
        {
            if (TextBox != null)
                TextBox.Focus();
            else
                base.Focus();
        }

        #endregion //Public

        #endregion //Methods

        #region Commands

        private void Paste(object sender, RoutedEventArgs e)
        {
            if (!IsEditable)
                return;

            MaskedTextProvider provider = MaskProvider;
            int position = TextBox.SelectionStart;

            object data = Clipboard.GetData(DataFormats.Text);
            if (data != null)
            {
                string text = data.ToString().Trim();
                if (text.Length > 0)
                {
                    provider.Set(text);
                    UpdateText(provider, position);
                }
            }
        }

        private void CanCut(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }

        #endregion //Commands
    }
}
