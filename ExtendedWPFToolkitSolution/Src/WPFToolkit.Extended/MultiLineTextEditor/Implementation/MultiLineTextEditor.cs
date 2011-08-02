using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    public class MultiLineTextEditor : ContentControl
    {
        #region Members

        TextBox _textBox;
        Thumb _resizeThumb;

        #endregion //Members

        #region Properties

        #region IsOpen

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(MultiLineTextEditor), new UIPropertyMetadata(false, OnIsOpenChanged));
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        private static void OnIsOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MultiLineTextEditor multiLineTextEditor = o as MultiLineTextEditor;
            if (multiLineTextEditor != null)
                multiLineTextEditor.OnIsOpenChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsOpenChanged(bool oldValue, bool newValue)
        {

        }

        #endregion //IsOpen

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MultiLineTextEditor), new FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MultiLineTextEditor textEditor = o as MultiLineTextEditor;
            if (textEditor != null)
                textEditor.OnTextChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnTextChanged(string oldValue, string newValue)
        {

        }

        #endregion //Text

        #endregion //Properties

        #region Constructors

        static MultiLineTextEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiLineTextEditor), new FrameworkPropertyMetadata(typeof(MultiLineTextEditor)));
        }

        public MultiLineTextEditor()
        {
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
        }

        #endregion //Constructors

        #region Bass Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = (TextBox)GetTemplateChild("PART_TextBox");

            if (_resizeThumb != null)
                _resizeThumb.DragDelta -= ResizeThumb_DragDelta;
            _resizeThumb = (Thumb)GetTemplateChild("PART_ResizeThumb");

            if (_resizeThumb != null)
                _resizeThumb.DragDelta += ResizeThumb_DragDelta;

        }

        void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double yadjust = this._textBox.Height + e.VerticalChange;
            double xadjust = this._textBox.Width + e.HorizontalChange;

            if ((xadjust >= 0) && (yadjust >= 0))
            {
                this._textBox.Width = xadjust;
                this._textBox.Height = yadjust;
            }
        }

        #endregion //Bass Class Overrides

        #region Event Handlers

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                case Key.Tab:
                    {
                        CloseEditor();
                        break;
                    }
            }
        }

        private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
        {
            CloseEditor();
        }

        #endregion //Event Handlers

        #region Methods

        private void CloseEditor()
        {
            if (IsOpen)
                IsOpen = false;
            ReleaseMouseCapture();
        }

        #endregion //Methods
    }
}
