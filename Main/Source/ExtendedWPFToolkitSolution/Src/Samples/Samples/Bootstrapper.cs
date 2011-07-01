using System;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using System.Windows;

namespace Samples
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            App.Current.MainWindow = (Shell)Shell;
            App.Current.MainWindow.Show();
        }
    }
}
