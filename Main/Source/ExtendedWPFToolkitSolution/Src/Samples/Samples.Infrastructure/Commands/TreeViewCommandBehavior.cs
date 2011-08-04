using System;
using Microsoft.Practices.Prism.Commands;
using System.Windows.Controls;
using System.Windows;

namespace Samples.Infrastructure.Commands
{
    public class TreeViewCommandBehavior : CommandBehaviorBase<TreeView>
    {
        public TreeViewCommandBehavior(TreeView treeView)
            : base(treeView)
        {
            treeView.SelectedItemChanged += SelectedItemChanged;
        }

        void SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //This treeview command is specfic to the navigation region, 
            //so I always want to pass the tag of the selected treeviewitem
            //because it will conatin the Type of view to navigate to.
            var type = (e.NewValue as FrameworkElement).Tag as Type;
            CommandParameter = type != null ? type.FullName : null;
            ExecuteCommand();
        }
    }
}
