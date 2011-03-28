using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    public class CalculatorUpDown : NumericUpDown
    {
        #region Members

        private Popup _calculatorPopup;
        private Calculator _calculator;

        #endregion //Members

        #region Properties

        #region IsOpen

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(CalculatorUpDown), new UIPropertyMetadata(false, OnIsOpenChanged));
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        private static void OnIsOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CalculatorUpDown calculatorUpDown = o as CalculatorUpDown;
            if (calculatorUpDown != null)
                calculatorUpDown.OnIsOpenChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsOpenChanged(bool oldValue, bool newValue)
        {
            
        }

        #endregion //IsOpen

        #endregion //Properties

        #region Constructors

        static CalculatorUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CalculatorUpDown), new FrameworkPropertyMetadata(typeof(CalculatorUpDown)));
        }

        public CalculatorUpDown()
        {
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _calculatorPopup = (Popup)GetTemplateChild("PART_CalculatorPopup");
            _calculatorPopup.Opened += CalculatorPopup_Opened;

            _calculator = (Calculator)GetTemplateChild("PART_Calculator");
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        void CalculatorPopup_Opened(object sender, EventArgs e)
        {
            _calculator.Focus();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                case Key.Tab:
                    {
                        CloseCalculatorUpDown();
                        e.Handled = true;
                        break;
                    }
            }
        }

        private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
        {
            CloseCalculatorUpDown();
        }

        #endregion //Event Handlers

        #region Methods

        private void CloseCalculatorUpDown()
        {
            if (IsOpen)
                IsOpen = false;
            ReleaseMouseCapture();
        }

        #endregion //Methods
    }
}
