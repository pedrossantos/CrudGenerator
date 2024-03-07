using System.Windows.Navigation;
using View.Abstractions.Wpf;

namespace CrudGenerator.App.Wpf
{
    internal class CrudGeneratorWpfNavigationControllerConfiguration : INavigationControllerConfiguration
    {
        public double Height => 576;

        public double Width => 1024;

        public bool IsExclusive => false;

        public NavigationUIVisibility NavigationUIVisibility => NavigationUIVisibility.Visible;
    }
}
