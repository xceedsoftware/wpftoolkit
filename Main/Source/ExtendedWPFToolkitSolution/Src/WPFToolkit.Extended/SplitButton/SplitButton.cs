using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    public class SplitButton : ContentControl
    {
        #region Members

        ToggleButton _toggleButton;
        Popup _popup;

        #endregion //Members

        #region Constructors

        static SplitButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
        }

        public SplitButton()
        {
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
        }

        #endregion //Constructors

        #region Properties

        #region DropDownContent

        public static readonly DependencyProperty DropDownContentProperty = DependencyProperty.Register("DropDownContent", typeof(object), typeof(SplitButton), new UIPropertyMetadata(null, OnDropDownContentChanged));
        public object DropDownContent
        {
            get { return (object)GetValue(DropDownContentProperty); }
            set { SetValue(DropDownContentProperty, value); }
        }

        private static void OnDropDownContentChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SplitButton splitButton = o as SplitButton;
            if (splitButton != null)
                splitButton.OnDropDownContentChanged((object)e.OldValue, (object)e.NewValue);
        }

        protected virtual void OnDropDownContentChanged(object oldValue, object newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }        

        #endregion //DropDownContent

        #region IsOpen

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(SplitButton), new UIPropertyMetadata(false, OnIsOpenChanged));
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        private static void OnIsOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SplitButton splitButton = o as SplitButton;
            if (splitButton != null)
                splitButton.OnIsOpenChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsOpenChanged(bool oldValue, bool newValue)
        {
            // TODO: check for cancel event args on an OnOpening event
        }

        #endregion //IsOpen

        #endregion //Properties

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _toggleButton = (ToggleButton)GetTemplateChild("PART_ToggleButton");
            _toggleButton.Click += ToggleButton_Click;      
            
            _popup = (Popup)GetTemplateChild("PART_Popup");
            _popup.Opened += Popup_Opened;
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        void ToggleButton_Click(object sender, RoutedEventArgs e)
        {

        }

        void Popup_Opened(object sender, EventArgs e)
        {
            
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                case Key.Tab:
                    {
                        CloseDropDown();
                        break;
                    }
            }
        }

        private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
        {
            CloseDropDown();
        }

        #endregion //Event Handlers

        #region Methods

        private void CloseDropDown()
        {
            if (IsOpen)
                IsOpen = false;
            ReleaseMouseCapture();
        }

        #endregion //Methods
    }
}
