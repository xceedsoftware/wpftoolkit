using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Microsoft.Windows.Controls
{
    public class RichTextBox : System.Windows.Controls.RichTextBox
    {
        #region Private Members

        private bool _textHasLoaded;
        bool isInvokePending;

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

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(RichTextBox), new FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnTextPropertyChanged), new CoerceValueCallback(CoerceTextProperty), true, System.Windows.Data.UpdateSourceTrigger.LostFocus));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RichTextBox rtb = (RichTextBox)d;

            if (!rtb._textHasLoaded)
            {
                rtb.TextFormatter.SetText(rtb.Document, (string)e.NewValue);
                rtb._textHasLoaded = true;
            }
        }

        private static object CoerceTextProperty(DependencyObject d, object value)
        {
            return value ?? "";
        }

        #endregion //Text

        #endregion //Properties

        #region Methods

        private void InvokeUpdateText()
        {
            if (!isInvokePending)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(UpdateText));
                isInvokePending = true;
            }
        }

        private void UpdateText()
        {
            //when the Text is null and the Text hasn't been loaded, it indicates that the OnTextPropertyChanged event hasn't exceuted
            //and since we are initializing the text from here, we don't want the OnTextPropertyChanged to execute, so set the loaded flag to true.
            //this prevents the cursor to jumping to the front of the textbox after the first letter is typed.
            if (!_textHasLoaded && string.IsNullOrEmpty(Text))
                _textHasLoaded = true;

            if (_textHasLoaded)
                Text = TextFormatter.GetText(Document);

            isInvokePending = false;
        }

        #endregion //Methods

        #region Event Hanlders

        private void RichTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            Binding binding = BindingOperations.GetBinding(this, TextProperty);

            if (binding.UpdateSourceTrigger == UpdateSourceTrigger.Default || binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
            {
                LostFocus += (o, ea) => InvokeUpdateText();
            }
            else
            {
                TextChanged += (o, ea) => InvokeUpdateText();
            }
        }

        #endregion //Event Hanlders
    }
}
