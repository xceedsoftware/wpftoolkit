using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls
{
    public class MaskedTextBox : TextBox
    {
        #region Members

        /// <summary>
        /// Flags if the Text and Value properties are in the process of being sync'd
        /// </summary>
        private bool _isSyncingTextAndValueProperties;
        private bool _isInitialized;
        private bool _convertExceptionOccurred = false;

        #endregion //Members

        #region Properties

        protected MaskedTextProvider MaskProvider { get; set; }

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

        #region PromptChar

        public static readonly DependencyProperty PromptCharProperty = DependencyProperty.Register("PromptChar", typeof(char), typeof(MaskedTextBox), new UIPropertyMetadata('_', OnPromptCharChanged));
        public char PromptChar
        {
            get { return (char)GetValue(PromptCharProperty); }
            set { SetValue(PromptCharProperty, value); }
        }

        private static void OnPromptCharChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MaskedTextBox maskedTextBox = o as MaskedTextBox;
            if (maskedTextBox != null)
                maskedTextBox.OnPromptCharChanged((char)e.OldValue, (char)e.NewValue);
        }

        protected virtual void OnPromptCharChanged(char oldValue, char newValue)
        {
            ResolveMaskProvider(Mask);
        }

        #endregion //PromptChar

        #region SelectAllOnGotFocus

        public static readonly DependencyProperty SelectAllOnGotFocusProperty = DependencyProperty.Register("SelectAllOnGotFocus", typeof(bool), typeof(MaskedTextBox), new PropertyMetadata(false));
        public bool SelectAllOnGotFocus
        {
            get { return (bool)GetValue(SelectAllOnGotFocusProperty); }
            set { SetValue(SelectAllOnGotFocusProperty, value); }
        }

        #endregion //SelectAllOnGotFocus

        #region Text

        private static void OnTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MaskedTextBox inputBase = o as MaskedTextBox;
            if (inputBase != null)
                inputBase.OnTextChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnTextChanged(string oldValue, string newValue)
        {
            if (_isInitialized)
                SyncTextAndValueProperties(MaskedTextBox.TextProperty, newValue);
        }

        #endregion //Text

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
                SyncTextAndValueProperties(MaskedTextBox.TextProperty, Text);
        }

        #endregion //ValueType

        #endregion //Properties

        #region Constructors

        static MaskedTextBox()
        {
            TextProperty.OverrideMetadata(typeof(MaskedTextBox), new FrameworkPropertyMetadata(OnTextChanged));
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PreviewTextInput += TextBox_PreviewTextInput;
            PreviewKeyDown += TextBox_PreviewKeyDown;

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, Paste)); //handle paste
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, null, CanCut)); //surpress cut

            UpdateText(MaskProvider, 0);
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

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (SelectAllOnGotFocus)
                SelectAll();

            base.OnGotKeyboardFocus(e);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!IsKeyboardFocused)
            {
                e.Handled = true;
                Focus();
            }

            base.OnPreviewMouseLeftButtonDown(e);
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //if the text is readonly do not add the text
            if (IsReadOnly)
            {
                e.Handled = true;
                return;
            }

            int position = SelectionStart;
            MaskedTextProvider provider = MaskProvider;

            if (position < Text.Length)
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
            if (IsReadOnly)
                return;

            MaskedTextProvider provider = MaskProvider;
            int position = SelectionStart;
            int selectionlength = SelectionLength;
            // If no selection use the start position else use end position
            int endposition = (selectionlength == 0) ? position : position + selectionlength - 1;

            if (e.Key == Key.Delete && position < Text.Length)//handle the delete key
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

            //if all text is selected and the user begins to type, we want to delete all selected text and continue typing the new values
            if (SelectionLength == Text.Length)
            {
                if (provider.RemoveAt(position, endposition))
                    UpdateText(provider, position);
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

            SelectionStart = position;
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
                IncludeLiterals = this.IncludeLiterals,
                PromptChar = this.PromptChar
            };
        }

        private object ConvertTextToValue(string text)
        {
            object convertedValue = null;
            
            Type dataType = ValueType;

            string valueToConvert = MaskProvider.ToString().Trim();

            try
            {
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
            }
            catch
            {
                //if an excpetion occurs revert back to original value
                _convertExceptionOccurred = true;
                return Value;
            }

            return convertedValue;
        }

        private string ConvertValueToText(object value)
        {
            if (value == null)
                value = string.Empty;

            if (_convertExceptionOccurred)
            {
                value = Value;
                _convertExceptionOccurred = false;
            }

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
            if (MaskedTextBox.TextProperty == p)
            {
                if (newValue != null)
                    SetValue(MaskedTextBox.ValueProperty, ConvertTextToValue(newValue.ToString()));
            }

            SetValue(MaskedTextBox.TextProperty, ConvertValueToText(newValue));

            _isSyncingTextAndValueProperties = false;
        }

        #endregion //Private

        #endregion //Methods

        #region Commands

        private void Paste(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly)
                return;

            MaskedTextProvider provider = MaskProvider;
            int position = SelectionStart;

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
