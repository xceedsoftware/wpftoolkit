using System;
using System.Windows.Controls;
using System.Windows;

namespace Microsoft.Windows.Controls.Primitives
{
    public abstract class InputBase : Control
    {
        #region Properties

        #region IsEditable

        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(InputBase), new PropertyMetadata(true));
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        #endregion //IsEditable

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(InputBase), new PropertyMetadata(default(String), OnTextPropertyChanged));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InputBase input = (InputBase)d;
            input.OnTextChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnTextChanged(string previousValue, string currentValue)
        {

        }

        #endregion //Text

        #endregion //Properties
    }
}
