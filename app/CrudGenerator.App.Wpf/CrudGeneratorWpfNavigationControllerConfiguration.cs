using System.Windows.Navigation;
using View.Abstractions.Wpf;

namespace CrudGenerator.App.Wpf
{
    internal class CrudGeneratorWpfNavigationControllerConfiguration : INavigationControllerConfiguration
    {
        public double Height => 720;

        public double Width => 1280;

        public bool IsExclusive => false;

        public NavigationUIVisibility NavigationUIVisibility => NavigationUIVisibility.Visible;
    }
}
