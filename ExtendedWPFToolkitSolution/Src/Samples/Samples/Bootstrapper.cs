using System;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using System.Windows;
using Microsoft.Practices.Prism.Modularity;

namespace Samples
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            Container.RegisterType<IShellViewModel, ShellViewModel>();
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            App.Current.MainWindow = (Shell)Shell;
            App.Current.MainWindow.Show();
        }

        protected override Microsoft.Practices.Prism.Modularity.IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }
    }
}
