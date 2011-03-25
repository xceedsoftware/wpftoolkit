using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Windows.Controls.Core.Utilities;

namespace Microsoft.Windows.Controls
{
    public class Calculator : Control
    {
        #region Members

        private ContentControl _buttonPanel;
        private readonly DispatcherTimer _timer;
        private Button _calculatorButton;

        bool _showNewNumber = true;
        decimal _previousValue;
        CalculatorButtonType _lastButtonPressed;
        Operation _lastOperation = Operation.None;

        #endregion //Members

        #region Enumerations

        public enum CalculatorButtonType
        {
            Add,
            Back,
            Cancel,
            Clear,
            Decimal,
            Divide,
            Eight,
            Equal,
            Five,
            Four,
            Fract,
            MAdd,
            MC,
            MR,
            MS,
            MSub,
            Multiply,
            Nine,
            None,
            One,
            Percent,
            Seven,
            Sign,
            Six,
            Sqrt,
            Subtract,
            Three,
            Two,
            Zero
        }

        public enum Operation
        {
            Add,
            Subtract,
            Divide,
            Multiply,
            None
        }

        #endregion //Enumerations

        #region Properties

        public ICommand CalculaterButtonClickCommand { get; private set; }

        #region CalculatorButtonType

        public static readonly DependencyProperty CalculatorButtonTypeProperty = DependencyProperty.RegisterAttached("CalculatorButtonType", typeof(CalculatorButtonType), typeof(Calculator), new UIPropertyMetadata(CalculatorButtonType.None, OnCalculatorButtonTypeChanged));
        public static CalculatorButtonType GetCalculatorButtonType(DependencyObject target)
        {
            return (CalculatorButtonType)target.GetValue(CalculatorButtonTypeProperty);
        }
        public static void SetCalculatorButtonType(DependencyObject target, CalculatorButtonType value)
        {
            target.SetValue(CalculatorButtonTypeProperty, value);
        }
        private static void OnCalculatorButtonTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            OnCalculatorButtonTypeChanged(o, (CalculatorButtonType)e.OldValue, (CalculatorButtonType)e.NewValue);
        }
        private static void OnCalculatorButtonTypeChanged(DependencyObject o, CalculatorButtonType oldValue, CalculatorButtonType newValue)
        {
            Button button = o as Button;
            button.CommandParameter = newValue;
            button.Content = CalculatorUtilities.GetCalculatorButtonContent(newValue);
        }

        #endregion //CalculatorButtonType

        #region DisplayText

        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText", typeof(string), typeof(Calculator), new UIPropertyMetadata("0", OnDisplayTextChanged));
        public string DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }

        private static void OnDisplayTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Calculator calculator = o as Calculator;
            if (calculator != null)
                calculator.OnDisplayTextChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnDisplayTextChanged(string oldValue, string newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //DisplayText

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(decimal), typeof(Calculator), new UIPropertyMetadata(default(decimal), OnValueChanged));
        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Calculator calculator = o as Calculator;
            if (calculator != null)
                calculator.OnValueChanged((decimal)e.OldValue, (decimal)e.NewValue);
        }

        protected virtual void OnValueChanged(decimal oldValue, decimal newValue)
        {
            DisplayText = newValue.ToString();
        }

        #endregion //Value

        #endregion //Properties

        #region Constructors

        static Calculator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Calculator), new FrameworkPropertyMetadata(typeof(Calculator)));
        }

        public Calculator()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;

            CommandBindings.Add(new CommandBinding(CalculatorCommands.CalculatorButtonClick, ExecuteCalculatorButtonClick));
            AddHandler(MouseDownEvent, new MouseButtonEventHandler(Calculator_OnMouseDown), true);
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _buttonPanel = (ContentControl)GetTemplateChild("PART_CalculatorButtonPanel");
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            var buttonType = CalculatorUtilities.GetCalculatorButtonTypeFromText(e.Text);
            if (buttonType != CalculatorButtonType.None)
            {
                AnimateCalculatorButtonClick(buttonType);
                ProcessCalculatorButton(buttonType);
            }
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        private void Calculator_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(_calculatorButton, _calculatorButton.IsMouseOver ? "MouseOver" : "Normal", true);
            _timer.Stop();
        }

        #endregion //Event Handlers

        #region Methods

        private void AnimateCalculatorButtonClick(CalculatorButtonType buttonType)
        {
            _calculatorButton = CalculatorUtilities.FindButtonByCalculatorButtonType(_buttonPanel, buttonType);
            VisualStateManager.GoToState(_calculatorButton, "Pressed", true);
            _timer.Start();
        }

        private void ProcessCalculatorButton(CalculatorButtonType buttonType)
        {
            if (CalculatorUtilities.IsDigit(buttonType))
                ProcessDigit(buttonType);
            else if (CalculatorUtilities.IsOperation(buttonType))
                ProcessOperation(buttonType);
            else
                ProcessMisc(buttonType);

            _lastButtonPressed = buttonType;
        }

        private void ProcessDigit(CalculatorButtonType buttonType)
        {
            if (_showNewNumber)
                DisplayText = CalculatorUtilities.GetCalculatorButtonContent(buttonType);
            else
                DisplayText += CalculatorUtilities.GetCalculatorButtonContent(buttonType);

            _showNewNumber = false;
        }

        private void ProcessOperation(CalculatorButtonType buttonType)
        {
            if (_lastButtonPressed != CalculatorButtonType.Add &&
                _lastButtonPressed != CalculatorButtonType.Subtract &&
                _lastButtonPressed != CalculatorButtonType.Multiply &&
                _lastButtonPressed != CalculatorButtonType.Divide)
            {

                Operation operation = Operation.None;

                switch (buttonType)
                {
                    case CalculatorButtonType.Add:
                        operation = Operation.Add;
                        break;
                    case CalculatorButtonType.Subtract:
                        operation = Operation.Subtract;
                        break;
                    case CalculatorButtonType.Multiply:
                        operation = Operation.Multiply;
                        break;
                    case CalculatorButtonType.Divide:
                        operation = Operation.Divide;
                        break;
                    default:
                        operation = Operation.None;
                        break;
                }

                Calculate();

                _previousValue = Decimal.Parse(DisplayText);

                _showNewNumber = true;
                _lastOperation = operation;
            }
        }

        private void ProcessMisc(CalculatorButtonType buttonType)
        {
            switch (buttonType)
            {
                case CalculatorButtonType.Clear:
                    Value = decimal.Zero;
                    _showNewNumber = true;
                    break;
                case CalculatorButtonType.Equal:
                    Calculate();
                    _lastOperation = Operation.None;
                    _showNewNumber = true;
                    break;
            }
        }

        private void Calculate()
        {
            if (_lastOperation == Operation.None)
                return;

            Value = CalculateValue(_lastOperation);
        }

        private decimal CalculateValue(Operation operation)
        {
            decimal currentValue = Decimal.Parse(DisplayText);

            switch (operation)
            {
                case Operation.Add:
                   return CalculatorUtilities.Add(_previousValue, currentValue);
                case Operation.Subtract:
                    return CalculatorUtilities.Subtract(_previousValue, currentValue);
                case Operation.Multiply:
                    return CalculatorUtilities.Multiply(_previousValue, currentValue);
                case Operation.Divide:
                    return CalculatorUtilities.Divide(_previousValue, currentValue);
                default:
                    return decimal.Zero;
            }
        }

        #endregion //Methods

        #region Commands

        private void ExecuteCalculatorButtonClick(object sender, ExecutedRoutedEventArgs e)
        {
            var buttonType = (CalculatorButtonType)e.Parameter;
            ProcessCalculatorButton(buttonType);
        }

        #endregion //Commands
    }
}
