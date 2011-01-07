using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using Microsoft.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{

    public class MaskedTextBox : InputBase
    {
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
        }

        protected override object ConvertTextToValue(string text)
        {
            object convertedValue = null;

            Type dataType = ValueType;

            string valueToConvert = MaskProvider.ToString();

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

        protected override string ConvertValueToText(object value)
        {
            if (value == null)
                value = string.Empty;

            //I have only seen this occur while in Blend, but we need it here so the Blend designer doesn't crash.
            if (MaskProvider == null) 
                return value.ToString();

            MaskProvider.Set(value.ToString());
            return MaskProvider.ToDisplayString();
        }

        #endregion

        #region Event Handlers

        void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //if the text is readonly do not add the text
            if (TextBox.IsReadOnly)
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
            base.OnPreviewKeyDown(e);
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
        }

        #endregion //Event Handlers

        #region Methods

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

        #endregion //Methods
    }
}
