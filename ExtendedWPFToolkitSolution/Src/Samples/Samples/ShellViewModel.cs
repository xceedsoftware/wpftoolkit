using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure;

namespace Samples
{
    public class ShellViewModel : IShellViewModel
    {
        private readonly IRegionManager _regionManager;

        public ICommand NavigateCommand { get; set; }

        public ShellViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        private void Navigate(string navigatePath)
        {
            if (!String.IsNullOrWhiteSpace(navigatePath))
                _regionManager.RequestNavigate(RegionNames.ContentRegion, navigatePath);
        }
    }
}
