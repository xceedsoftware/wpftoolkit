using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Microsoft.Windows.Controls
{
    public static class CalculatorCommands
    {
        private static RoutedCommand _calculatorButtonClickCommand = new RoutedCommand();
        public static RoutedCommand CalculatorButtonClick
        {
            get { return _calculatorButtonClickCommand; }
        }
    }

    public class Calculator : Control
    {
        private ContentControl _buttonPanel;
        private readonly DispatcherTimer _timer;
        private Button _calculatorButton;

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
            button.Content = GetCalculatorButtonContent(newValue);
        }

        #endregion //CalculatorButtonType



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
            // TODO: Add your property changed side-effects. Descendants can override as well.
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
            var buttonType = GetCalculatorButtonTypeFromText(e.Text);

            AnimateCalculatorButtonClick(buttonType);
        }

        private void AnimateCalculatorButtonClick(CalculatorButtonType buttonType)
        {
            _calculatorButton = FindButtonByCalculatorButtonType(_buttonPanel, buttonType);
            if (_calculatorButton != null)
            {
                VisualStateManager.GoToState(_calculatorButton, "Pressed", true);
                _timer.Start();
            }
        }

        private CalculatorButtonType GetCalculatorButtonTypeFromText(string text)
        {
            switch (text)
            {
                case "0": return CalculatorButtonType.Zero;
                case "1": return CalculatorButtonType.One;
                case "2": return CalculatorButtonType.Two;
                case "3": return CalculatorButtonType.Three;
                case "4": return CalculatorButtonType.Four;
                case "5": return CalculatorButtonType.Five;
                case "6": return CalculatorButtonType.Six;
                case "7": return CalculatorButtonType.Seven;
                case "8": return CalculatorButtonType.Eight;
                case "9": return CalculatorButtonType.Nine;
                case "%": return CalculatorButtonType.Percent;
                case "+": return CalculatorButtonType.Add;
                case "-": return CalculatorButtonType.Subtract;
                case "*": return CalculatorButtonType.Mul;
                case "/":
                case ":": return CalculatorButtonType.Div;
                case " ":
                case "\r":
                case "=": return CalculatorButtonType.Equal;
                case "\b": return CalculatorButtonType.Back;
            }

            //check for the escape key
            if (text == ((char)27).ToString())
                return CalculatorButtonType.Clear;

            return CalculatorButtonType.None;
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

        public static Button FindButtonByCalculatorButtonType(DependencyObject parent, CalculatorButtonType type)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                object buttonType = child.GetValue(Button.CommandParameterProperty);

                if (buttonType != null && (CalculatorButtonType)buttonType == type)
                {
                    return child as Button;
                }
                else
                {
                    var result = FindButtonByCalculatorButtonType(child, type);

                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        private static string GetCalculatorButtonContent(CalculatorButtonType type)
        {
            string content = string.Empty;
            switch (type)
            {
                case CalculatorButtonType.Add:
                    content = "+";
                    break;
                case CalculatorButtonType.Back:
                    content = "Back";
                    break;
                case CalculatorButtonType.Cancel:
                    content = "CE";
                    break;
                case CalculatorButtonType.Clear:
                    content = "C";
                    break;
                case CalculatorButtonType.Decimal:
                    content = ".";
                    break;
                case CalculatorButtonType.Div:
                    content = "/";
                    break;
                case CalculatorButtonType.Eight:
                    content = "8";
                    break;
                case CalculatorButtonType.Equal:
                    content = "=";
                    break;
                case CalculatorButtonType.Five:
                    content = "5";
                    break;
                case CalculatorButtonType.Four:
                    content = "4";
                    break;
                case CalculatorButtonType.Fract:
                    content = "1/x";
                    break;
                case CalculatorButtonType.MAdd:
                    content = "M+";
                    break;
                case CalculatorButtonType.MC:
                    content = "MC";
                    break;
                case CalculatorButtonType.MR:
                    content = "MR";
                    break;
                case CalculatorButtonType.MS:
                    content = "MS";
                    break;
                case CalculatorButtonType.MSub:
                    content = "M-";
                    break;
                case CalculatorButtonType.Mul:
                    content = "*";
                    break;
                case CalculatorButtonType.Nine:
                    content = "9";
                    break;
                case CalculatorButtonType.None:
                    break;
                case CalculatorButtonType.One:
                    content = "1";
                    break;
                case CalculatorButtonType.Percent:
                    content = "%";
                    break;
                case CalculatorButtonType.Seven:
                    content = "7";
                    break;
                case CalculatorButtonType.Sign:
                    content = "+/-";
                    break;
                case CalculatorButtonType.Six:
                    content = "6";
                    break;
                case CalculatorButtonType.Sqrt:
                    content = "Sqrt";
                    break;
                case CalculatorButtonType.Subtract:
                    content = "-";
                    break;
                case CalculatorButtonType.Three:
                    content = "3";
                    break;
                case CalculatorButtonType.Two:
                    content = "2";
                    break;
                case CalculatorButtonType.Zero:
                    content = "0";
                    break;
            }
            return content;
        }

        #endregion //Methods

        #region Commands

        private void ExecuteCalculatorButtonClick(object sender, ExecutedRoutedEventArgs e)
        {

        }

        #endregion //Commands


        public enum CalculatorButtonType
        {
            Add,
            Back,
            Cancel,
            Clear,
            Decimal,
            Div,
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
            Mul,
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
    }
}
