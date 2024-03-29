﻿using CrudGenerator.App.Wpf.DependencyInversion;
using DependencyInversion;
using System.Windows;
using View.Abstractions;
using View.Abstractions.Wpf;

namespace CrudGenerator.App.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IContainer _serviceProvider;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            _serviceProvider = new CrudGeneratorContainerBuilder().Build();

            IMessageDialog messageDialog = _serviceProvider.Resolve<IMessageDialog>();
            MainWindow mainWindow = _serviceProvider.Resolve<MainWindow>();
            if (mainWindow == null)
            {
                if (messageDialog != null)
                    await messageDialog.ShowAsync(string.Format(Messages.ErrorToResolveObject, nameof(IMessageDialog)), Messages.ErrorTitle);
                else
                    MessageBox.Show(string.Format(Messages.ErrorToResolveObject, nameof(MainWindow)), Messages.ErrorTitle, MessageBoxButton.OK);

                Shutdown();
            }
            else
            {
                WpfNavigationController wpfNavigationController = _serviceProvider.Resolve<INavigationController>() as WpfNavigationController;
                if (wpfNavigationController == null)
                {
                    await messageDialog.ShowAsync(string.Format(Messages.ErrorToResolveObject, nameof(WpfNavigationController)), Messages.ErrorTitle);
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

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            try
            {
                return AppDomain.CurrentDomain.Load(args.Name.Split(',')[0]);
            }
            catch
            {
                return args.RequestingAssembly;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
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
