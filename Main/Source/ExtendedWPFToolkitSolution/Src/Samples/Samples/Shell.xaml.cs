using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace Samples
{
    public partial class Shell : Window
    {
        public Shell(IShellViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }

        public IShellViewModel ViewModel
        {
            get { return (IShellViewModel)DataContext; }
            private set { DataContext = value; }
        }

        private void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
            TreeView tv = (TreeView)sender;
            tv.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));
        }
    }
}
