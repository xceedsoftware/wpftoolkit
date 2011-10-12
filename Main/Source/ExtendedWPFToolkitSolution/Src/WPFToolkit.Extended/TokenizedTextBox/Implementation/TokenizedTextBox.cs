//Based of the code written by Pavan Podila
//http://blog.pixelingene.com/2010/10/tokenizing-control-convert-text-to-tokens/
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Microsoft.Windows.Controls
{
    public class TokenizedTextBox : ItemsControl
    {
        #region Members

        private System.Windows.Controls.RichTextBox _rtb = null;
        private bool _surpressTextChanged = false;
        private bool _surpressTextChangedEvent = false;

        #endregion //Members

        #region Properties

        public static readonly DependencyProperty SearchMemberPathProperty = DependencyProperty.Register("SearchMemberPath", typeof(string), typeof(TokenizedTextBox), new UIPropertyMetadata(String.Empty));
        public string SearchMemberPath
        {
            get { return (string)GetValue(SearchMemberPathProperty); }
            set { SetValue(SearchMemberPathProperty, value); }
        }

        public static readonly DependencyProperty TokenDelimiterProperty = DependencyProperty.Register("TokenDelimiter", typeof(string), typeof(TokenizedTextBox), new UIPropertyMetadata(";"));
        public string TokenDelimiter
        {
            get { return (string)GetValue(TokenDelimiterProperty); }
            set { SetValue(TokenDelimiterProperty, value); }
        }

        public static readonly DependencyProperty TokenTemplateProperty = DependencyProperty.Register("TokenTemplate", typeof(DataTemplate), typeof(TokenizedTextBox), new UIPropertyMetadata(null));
        public DataTemplate TokenTemplate
        {
            get { return (DataTemplate)GetValue(TokenTemplateProperty); }
            set { SetValue(TokenTemplateProperty, value); }
        }

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TokenizedTextBox), new UIPropertyMetadata(null, OnTextChanged));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TokenizedTextBox tokenizedTextBox = o as TokenizedTextBox;
            if (tokenizedTextBox != null)
                tokenizedTextBox.OnTextChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnTextChanged(string oldValue, string newValue)
        {
            if (_rtb == null || _surpressTextChanged)
                return;

            //TODO: when text changes update tokens
        }

        #endregion //Text

        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register("ValueMemberPath", typeof(string), typeof(TokenizedTextBox), new UIPropertyMetadata(String.Empty));
        public string ValueMemberPath
        {
            get { return (string)GetValue(ValueMemberPathProperty); }
            set { SetValue(ValueMemberPathProperty, value); }
        }


        #endregion //Properties

        #region Constructors

        static TokenizedTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizedTextBox), new FrameworkPropertyMetadata(typeof(TokenizedTextBox)));
        }

        public TokenizedTextBox()
        {
            CommandBindings.Add(new CommandBinding(TokenizedTextBoxCommands.Delete, DeleteToken));
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_rtb != null)
            {
                _rtb.TextChanged -= RichTextBox_TextChanged;
                _rtb.PreviewKeyDown -= RichTextBox_PreviewKeyDown;
            }
            _rtb = GetTemplateChild("PART_ContentHost") as System.Windows.Controls.RichTextBox;
            if (_rtb != null)
            {
                _rtb.TextChanged += RichTextBox_TextChanged;
                _rtb.PreviewKeyDown += RichTextBox_PreviewKeyDown;
            }

            InitializeTokensFromText();
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_surpressTextChangedEvent)
                return;

            var text = _rtb.CaretPosition.GetTextInRun(LogicalDirection.Backward);
            var token = ResolveToken(text);
            if (token != null)
            {
                ReplaceTextWithToken(text.Trim(), token);
            }
        }

        void RichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InlineUIContainer container = null;

            if (e.Key == Key.Back)
            {
                container = _rtb.CaretPosition.GetAdjacentElement(LogicalDirection.Backward) as InlineUIContainer;
            }
            else if (e.Key == Key.Delete)
            {
                //if the selected text is a blank space, I will assume that a token item is selected.
                //if a token item is selected, we need to move the caret position to the left of the element so we can grab the InlineUIContainer
                if (_rtb.Selection.Text == " ")
                {
                    TextPointer moveTo = _rtb.CaretPosition.GetNextInsertionPosition(LogicalDirection.Backward);
                    _rtb.CaretPosition = moveTo;
                }

                //the cursor is to the left of a token item
                container = _rtb.CaretPosition.GetAdjacentElement(LogicalDirection.Forward) as InlineUIContainer;
            }

            //if the container is not null that means we have something to delete
            if (container != null)
            {
                var token = (container as InlineUIContainer).Child as TokenItem;
                if (token != null)
                {
                    SetTextInternal(Text.Replace(token.TokenKey, ""));
                }
            }
        }

        #endregion //Event Handlers

        #region Methods

        private void InitializeTokensFromText()
        {
            if (!String.IsNullOrEmpty(Text))
            {
                string[] tokenKeys = Text.Split(new string[] { TokenDelimiter }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string tokenKey in tokenKeys)
                {
                    var para = _rtb.CaretPosition.Paragraph;
                    var token = new Token(TokenDelimiter)
                    {
                        TokenKey = tokenKey,
                        Item = ResolveItemByTokenKey(tokenKey)
                    };
                    para.Inlines.Add(CreateTokenContainer(token));
                }
            }
        }

        private Token ResolveToken(string text)
        {
            if (text.EndsWith(TokenDelimiter))
                return ResolveTokenBySearchMemberPath(text.Substring(0, text.Length - 1).Trim());

            return null;
        }

        private Token ResolveTokenBySearchMemberPath(string searchText)
        {
            //create a new token and default the settings to the search text
            var token = new Token(TokenDelimiter)
            {
                TokenKey = searchText,
                Item = searchText
            };

            if (ItemsSource != null)
            {
                foreach (object item in ItemsSource)
                {
                    var searchProperty = item.GetType().GetProperty(SearchMemberPath);
                    if (searchProperty != null)
                    {
                        var searchValue = searchProperty.GetValue(item, null);
                        if (searchText.Equals(searchValue.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            var valueProperty = item.GetType().GetProperty(ValueMemberPath);
                            if (valueProperty != null)
                                token.TokenKey = valueProperty.GetValue(item, null).ToString();

                            token.Item = item;
                            break;
                        }
                    }
                }
            }

            return token;
        }

        private object ResolveItemByTokenKey(string tokenKey)
        {
            if (ItemsSource != null)
            {
                foreach (object item in ItemsSource)
                {
                    var property = item.GetType().GetProperty(ValueMemberPath);
                    if (property != null)
                    {
                        var value = property.GetValue(item, null);
                        if (tokenKey.Equals(value.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            return item;
                    }
                }
            }

            return tokenKey;
        }

        private void ReplaceTextWithToken(string inputText, Token token)
        {
            _surpressTextChangedEvent = true;

            var para = _rtb.CaretPosition.Paragraph;

            var matchedRun = para.Inlines.FirstOrDefault(inline =>
            {
                var run = inline as Run;
                return (run != null && run.Text.EndsWith(inputText));
            }) as Run;

            if (matchedRun != null) // Found a Run that matched the inputText
            {
                var tokenContainer = CreateTokenContainer(token);
                para.Inlines.InsertBefore(matchedRun, tokenContainer);

                // Remove only if the Text in the Run is the same as inputText, else split up
                if (matchedRun.Text == inputText)
                {
                    para.Inlines.Remove(matchedRun);
                }
                else // Split up
                {
                    var index = matchedRun.Text.IndexOf(inputText) + inputText.Length;
                    var tailEnd = new Run(matchedRun.Text.Substring(index));
                    para.Inlines.InsertAfter(matchedRun, tailEnd);
                    para.Inlines.Remove(matchedRun);
                }

                //now append the Text with the token key
                SetTextInternal(Text + token.TokenKey);
            }

            _surpressTextChangedEvent = false;
        }

        private InlineUIContainer CreateTokenContainer(Token token)
        {
            return new InlineUIContainer(CreateTokenItem(token)) { BaselineAlignment = BaselineAlignment.Center };
        }

        private TokenItem CreateTokenItem(Token token)
        {
            object item = token.Item;

            var tokenItem = new TokenItem()
            {
                TokenKey = token.TokenKey,
                Content = item,
                ContentTemplate = TokenTemplate
            };

            if (TokenTemplate == null)
            {
                //if no template was supplied let's try to get a value from the object using the DisplayMemberPath
                if (!String.IsNullOrEmpty(DisplayMemberPath))
                {
                    var property = item.GetType().GetProperty(DisplayMemberPath);
                    if (property != null)
                    {
                        var value = property.GetValue(item, null);
                        if (value != null)
                            tokenItem.Content = value;
                    }
                }
            }

            return tokenItem;
        }

        private void DeleteToken(object sender, ExecutedRoutedEventArgs e)
        {
            var para = _rtb.CaretPosition.Paragraph;

            Inline inlineToRemove = para.Inlines.Where(inline => inline is InlineUIContainer && ((inline as InlineUIContainer).Child as TokenItem).TokenKey.Equals(e.Parameter)).FirstOrDefault();

            if (inlineToRemove != null)
                para.Inlines.Remove(inlineToRemove);

            //update Text to remove delimited value
            SetTextInternal(Text.Replace(e.Parameter.ToString(), ""));
        }

        private void SetTextInternal(string text)
        {
            _surpressTextChanged = true;
            Text = text;
            _surpressTextChanged = false;
        }

        #endregion //Methods
    }
}