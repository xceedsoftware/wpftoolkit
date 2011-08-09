using System;
using System.Windows.Input;

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
}
