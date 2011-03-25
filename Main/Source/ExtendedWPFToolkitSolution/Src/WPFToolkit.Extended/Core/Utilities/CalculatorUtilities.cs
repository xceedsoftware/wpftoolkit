using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Windows.Controls.Core.Utilities
{
    static class CalculatorUtilities
    {
        public static Calculator.CalculatorButtonType GetCalculatorButtonTypeFromText(string text)
        {
            switch (text)
            {
                case "0": return Calculator.CalculatorButtonType.Zero;
                case "1": return Calculator.CalculatorButtonType.One;
                case "2": return Calculator.CalculatorButtonType.Two;
                case "3": return Calculator.CalculatorButtonType.Three;
                case "4": return Calculator.CalculatorButtonType.Four;
                case "5": return Calculator.CalculatorButtonType.Five;
                case "6": return Calculator.CalculatorButtonType.Six;
                case "7": return Calculator.CalculatorButtonType.Seven;
                case "8": return Calculator.CalculatorButtonType.Eight;
                case "9": return Calculator.CalculatorButtonType.Nine;
                case "+": return Calculator.CalculatorButtonType.Add;
                case "-": return Calculator.CalculatorButtonType.Subtract;
                case "*": return Calculator.CalculatorButtonType.Multiply;
                case "/": return Calculator.CalculatorButtonType.Divide;
                case "%": return Calculator.CalculatorButtonType.Percent;
                case "\b": return Calculator.CalculatorButtonType.Back;
                case "\r":
                case "=": return Calculator.CalculatorButtonType.Equal;
            }

            //check for the escape key
            if (text == ((char)27).ToString())
                return Calculator.CalculatorButtonType.Clear;

            return Calculator.CalculatorButtonType.None;
        }

        public static Button FindButtonByCalculatorButtonType(DependencyObject parent, Calculator.CalculatorButtonType type)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                object buttonType = child.GetValue(Button.CommandParameterProperty);

                if (buttonType != null && (Calculator.CalculatorButtonType)buttonType == type)
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

        public static string GetCalculatorButtonContent(Calculator.CalculatorButtonType type)
        {
            string content = string.Empty;
            switch (type)
            {
                case Calculator.CalculatorButtonType.Add:
                    content = "+";
                    break;
                case Calculator.CalculatorButtonType.Back:
                    content = "Back";
                    break;
                case Calculator.CalculatorButtonType.Cancel:
                    content = "CE";
                    break;
                case Calculator.CalculatorButtonType.Clear:
                    content = "C";
                    break;
                case Calculator.CalculatorButtonType.Decimal:
                    content = ".";
                    break;
                case Calculator.CalculatorButtonType.Divide:
                    content = "/";
                    break;
                case Calculator.CalculatorButtonType.Eight:
                    content = "8";
                    break;
                case Calculator.CalculatorButtonType.Equal:
                    content = "=";
                    break;
                case Calculator.CalculatorButtonType.Five:
                    content = "5";
                    break;
                case Calculator.CalculatorButtonType.Four:
                    content = "4";
                    break;
                case Calculator.CalculatorButtonType.Fract:
                    content = "1/x";
                    break;
                case Calculator.CalculatorButtonType.MAdd:
                    content = "M+";
                    break;
                case Calculator.CalculatorButtonType.MC:
                    content = "MC";
                    break;
                case Calculator.CalculatorButtonType.MR:
                    content = "MR";
                    break;
                case Calculator.CalculatorButtonType.MS:
                    content = "MS";
                    break;
                case Calculator.CalculatorButtonType.MSub:
                    content = "M-";
                    break;
                case Calculator.CalculatorButtonType.Multiply:
                    content = "*";
                    break;
                case Calculator.CalculatorButtonType.Nine:
                    content = "9";
                    break;
                case Calculator.CalculatorButtonType.None:
                    break;
                case Calculator.CalculatorButtonType.One:
                    content = "1";
                    break;
                case Calculator.CalculatorButtonType.Percent:
                    content = "%";
                    break;
                case Calculator.CalculatorButtonType.Seven:
                    content = "7";
                    break;
                case Calculator.CalculatorButtonType.Sign:
                    content = "+/-";
                    break;
                case Calculator.CalculatorButtonType.Six:
                    content = "6";
                    break;
                case Calculator.CalculatorButtonType.Sqrt:
                    content = "Sqrt";
                    break;
                case Calculator.CalculatorButtonType.Subtract:
                    content = "-";
                    break;
                case Calculator.CalculatorButtonType.Three:
                    content = "3";
                    break;
                case Calculator.CalculatorButtonType.Two:
                    content = "2";
                    break;
                case Calculator.CalculatorButtonType.Zero:
                    content = "0";
                    break;
            }
            return content;
        }

        public static bool IsDigit(Calculator.CalculatorButtonType buttonType)
        {
            switch (buttonType)
            {
                case Calculator.CalculatorButtonType.Zero:
                case Calculator.CalculatorButtonType.One:
                case Calculator.CalculatorButtonType.Two:
                case Calculator.CalculatorButtonType.Three:
                case Calculator.CalculatorButtonType.Four:
                case Calculator.CalculatorButtonType.Five:
                case Calculator.CalculatorButtonType.Six:
                case Calculator.CalculatorButtonType.Seven:
                case Calculator.CalculatorButtonType.Eight:
                case Calculator.CalculatorButtonType.Nine:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsOperation(Calculator.CalculatorButtonType buttonType)
        {
            switch (buttonType)
            {
                case Calculator.CalculatorButtonType.Add:
                case Calculator.CalculatorButtonType.Subtract:
                case Calculator.CalculatorButtonType.Multiply:
                case Calculator.CalculatorButtonType.Divide:
                case Calculator.CalculatorButtonType.Percent:
                    return true;
                default:
                    return false;
            }
        }
    }
}
