using System;
using System.Windows;

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
    }
}
