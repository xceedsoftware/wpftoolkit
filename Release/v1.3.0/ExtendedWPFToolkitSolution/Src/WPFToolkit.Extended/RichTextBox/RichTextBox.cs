using System;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Windows.Controls
{
    public class RichTextBox : System.Windows.Controls.RichTextBox
    {
        #region Private Members

        private bool _textSetInternally;

        #endregion //Private Members

        #region Constructors

        public RichTextBox()
        {
            Loaded += RichTextBox_Loaded;
        }

        public RichTextBox(System.Windows.Documents.FlowDocument document)
            : base(document)
        {

        }

        #endregion //Constructors

        #region Properties

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(RichTextBox), new FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextPropertyChanged, CoerceTextProperty, true, UpdateSourceTrigger.LostFocus));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RichTextBox rtb = (RichTextBox)d;

            // if the text is not being set internally then load the text into the RichTextBox
            if (!rtb._textSetInternally)
                rtb.TextFormatter.SetText(rtb.Document, (string)e.NewValue);
        }

        private static object CoerceTextProperty(DependencyObject d, object value)
        {
            return value ?? "";
        }

        #endregion //Text

        #region TextFormatter

        private ITextFormatter _textFormatter;
        /// <summary>
        /// The ITextFormatter the is used to format the text of the RichTextBox.
        /// Deafult formatter is the RtfFormatter
        /// </summary>
        public ITextFormatter TextFormatter
        {
            get
            {
                if (_textFormatter == null)
                    _textFormatter = new RtfFormatter(); //default is rtf

                return _textFormatter;
            }
            set
            {
                _textFormatter = value;
            }
        }

        #endregion //TextFormatter

        #endregion //Properties

        #region Methods

        private void UpdateText()
        {
            _textSetInternally = true;

            Text = TextFormatter.GetText(Document);

            _textSetInternally = false;
        }

        #endregion //Methods

        #region Event Hanlders

        private void RichTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            Binding binding = BindingOperations.GetBinding(this, TextProperty);

            if (binding != null)
            {
                if (binding.UpdateSourceTrigger == UpdateSourceTrigger.Default || binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
                {
                    PreviewLostKeyboardFocus += (o, ea) => UpdateText();
                }
                else
                {
                    TextChanged += (o, ea) => UpdateText();
                }
            }
        }

        #endregion //Event Hanlders
    }
}
