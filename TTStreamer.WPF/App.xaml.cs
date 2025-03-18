// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.IO;
using System.Reflection;
using System.Windows.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog.Extensions.Logging;

using TTStreamer.Models;
using TTStreamer.Common.Extensions;
using TTStreamer.WPF.Models;
using TTStreamer.WPF.Pages;
using TTStreamer.WPF.Services;
using TTStreamer.WPF.Windows;

using Wpf.Ui;

namespace TTStreamer.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static IHost host;
        public static T GetService<T>()
            where T : class
        {
            return host.Services.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(c => { c.SetBasePath(Directory.GetCurrentDirectory()); })
                .ConfigureServices((context, services) =>
                {
                    #region common

                    services.AddAppServices();
                    services.AddMediatR(f => f.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
                    services.AddHostedService<ApplicationHostService>();
                    services.AddLogging(b =>
                    {
                        b.ClearProviders();
                        b.AddNLog();
                    });

                    #endregion

                    #region views

                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<INavigationService, NavigationService>();
                    services.AddSingleton<ISnackbarService, SnackbarService>();
                    services.AddSingleton<IContentDialogService, ContentDialogService>();
                    services.AddSingleton<MonitoringPage>();
                    services.AddSingleton<MonitoringViewModel>();
                    services.AddSingleton<SoundPage>();
                    services.AddSingleton<SoundViewModel>();
                    services.AddSingleton<SettingsPage>();
                    services.AddSingleton<SettingsViewModel>();

                    #endregion

                }).Build();

            host.Start();

        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await host.StopAsync();
            host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}
