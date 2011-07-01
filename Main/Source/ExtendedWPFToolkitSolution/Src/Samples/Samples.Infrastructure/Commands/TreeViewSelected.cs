using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Samples.Infrastructure.Commands
{
    public class TreeViewSelected
    {
        private static readonly DependencyProperty SelectedCommandBehaviorProperty = DependencyProperty.RegisterAttached("SelectedCommandBehavior", typeof(TreeViewCommandBehavior), typeof(TreeViewSelected), null);

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(TreeViewSelected), new PropertyMetadata(OnSetCommandCallback));
        public static void SetCommand(TreeView menuItem, ICommand command)
        {
            menuItem.SetValue(CommandProperty, command);
        }
        public static ICommand GetCommand(TreeView menuItem)
        {
            return menuItem.GetValue(CommandProperty) as ICommand;
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(TreeViewSelected), new PropertyMetadata(OnSetCommandParameterCallback));
        public static void SetCommandParameter(TreeView menuItem, object parameter)
        {
            menuItem.SetValue(CommandParameterProperty, parameter);
        }
        public static object GetCommandParameter(TreeView menuItem)
        {
            return menuItem.GetValue(CommandParameterProperty);
        }

        private static void OnSetCommandCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            TreeView menuItem = dependencyObject as TreeView;
            if (menuItem != null)
            {
                TreeViewCommandBehavior behavior = GetOrCreateBehavior(menuItem);
                behavior.Command = e.NewValue as ICommand;
            }
        }

        private static void OnSetCommandParameterCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            TreeView menuItem = dependencyObject as TreeView;
            if (menuItem != null)
            {
                TreeViewCommandBehavior behavior = GetOrCreateBehavior(menuItem);
                behavior.CommandParameter = e.NewValue;
            }
        }

        private static TreeViewCommandBehavior GetOrCreateBehavior(TreeView menuItem)
        {
            TreeViewCommandBehavior behavior = menuItem.GetValue(SelectedCommandBehaviorProperty) as TreeViewCommandBehavior;
            if (behavior == null)
            {
                behavior = new TreeViewCommandBehavior(menuItem);
                menuItem.SetValue(SelectedCommandBehaviorProperty, behavior);
            }

            return behavior;
        }
    }
}
