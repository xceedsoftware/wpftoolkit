using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    public class CalculatorUpDown : DecimalUpDown
    {
        #region Members

        private Popup _calculatorPopup;
        private Calculator _calculator;

        #endregion //Members

        #region Properties

        #region DisplayText

        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText", typeof(string), typeof(CalculatorUpDown), new UIPropertyMetadata("0"));
        public string DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }

        #endregion //DisplayText

        #region EnterClosesCalculator

        public static readonly DependencyProperty EnterClosesCalculatorProperty = DependencyProperty.Register("EnterClosesCalculator", typeof(bool), typeof(CalculatorUpDown), new UIPropertyMetadata(false));
        public bool EnterClosesCalculator
        {
            get { return (bool)GetValue(EnterClosesCalculatorProperty); }
            set { SetValue(EnterClosesCalculatorProperty, value); }
        }

        #endregion //EnterClosesCalculator

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

        #region Memory

        public static readonly DependencyProperty MemoryProperty = DependencyProperty.Register("Memory", typeof(decimal), typeof(CalculatorUpDown), new UIPropertyMetadata(default(decimal)));
        public decimal Memory
        {
            get { return (decimal)GetValue(MemoryProperty); }
            set { SetValue(MemoryProperty, value); }
        }

        #endregion //Memory

        #region Precision

        public static readonly DependencyProperty PrecisionProperty = DependencyProperty.Register("Precision", typeof(int), typeof(CalculatorUpDown), new UIPropertyMetadata(6));
        public int Precision
        {
            get { return (int)GetValue(PrecisionProperty); }
            set { SetValue(PrecisionProperty, value); }
        }

        #endregion //Precision

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
                case Key.Enter:
                    {
                        if (EnterClosesCalculator && IsOpen)
                            CloseCalculatorUpDown();
                        break;
                    }
                case Key.Escape:
                    {
                        CloseCalculatorUpDown();
                        e.Handled = true;
                        break;
                    }
                case Key.Tab:
                    {
                        CloseCalculatorUpDown();
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
