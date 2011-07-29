using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Windows.Controls
{
    public class Wizard : ItemsControl
    {
        #region Constructors

        static Wizard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Wizard), new FrameworkPropertyMetadata(typeof(Wizard)));
        }

        public Wizard()
        {
            CommandBindings.Add(new CommandBinding(WizardCommands.Cancel, Cancel, CanCancel));
            CommandBindings.Add(new CommandBinding(WizardCommands.Finish, Finish, CanFinish));
            CommandBindings.Add(new CommandBinding(WizardCommands.Help, Help, CanHelp));
            CommandBindings.Add(new CommandBinding(WizardCommands.NextPage, SelectNextPage, CanSelectNextPage));
            CommandBindings.Add(new CommandBinding(WizardCommands.PreviousPage, SelectPreviousPage, CanSelectPreviousPage));
            CommandBindings.Add(new CommandBinding(WizardCommands.SelectPage, SelectPage, CanSelectPage));
        }

        #endregion //Constructors

        #region Base Class Overrides

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new WizardPage();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is WizardPage);
        }

        #endregion //Base Class Overrides

        #region Commands

        private void Cancel(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CanCancel(object sender, CanExecuteRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Finish(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CanFinish(object sender, CanExecuteRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Help(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CanHelp(object sender, CanExecuteRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SelectNextPage(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CanSelectNextPage(object sender, CanExecuteRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SelectPreviousPage(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CanSelectPreviousPage(object sender, CanExecuteRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SelectPage(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CanSelectPage(object sender, CanExecuteRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion //Commands
    }
}
