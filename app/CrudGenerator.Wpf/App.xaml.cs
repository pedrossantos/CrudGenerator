using CrudGenerator.Wpf.DependencyInversion;
using DependencyInversion;
using System.Windows;
using View.Abstractions;
using View.Abstractions.Wpf;

namespace CrudGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IContainer _serviceProvider;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            _serviceProvider = new CrudGeneratorContainerBuilder().Build();

            IMessageDialog messageDialog = _serviceProvider.Resolve<IMessageDialog>();
            MainWindow mainWindow = _serviceProvider.Resolve<MainWindow>();
            if (mainWindow == null)
            {
                if (messageDialog != null)
                    await messageDialog.ShowAsync($"Erro ao resolver objeto {nameof(IMessageDialog)}", "Erro");
                else
                    MessageBox.Show($"Erro ao carregar {nameof(MainWindow)}", "Erro", MessageBoxButton.OK);

                Shutdown();
            }
            else
            {
                WpfNavigationController wpfNavigationController = _serviceProvider.Resolve<INavigationController>() as WpfNavigationController;
                if (wpfNavigationController == null)
                {
                    await messageDialog.ShowAsync($"Erro ao resolver objeto {nameof(WpfNavigationController)}", "Erro");
                    Shutdown();
                }
                else
                {
                    wpfNavigationController.Closed += MainWindow_Closed;
                    await wpfNavigationController.RootAsync(mainWindow);
                    wpfNavigationController.Show();
                }
            }
        }

        private void MainWindow_Closed(object sender, System.EventArgs e)
        {
            if (sender is Window window)
                window.Closed -= MainWindow_Closed;

            Shutdown();
        }
    }
}
