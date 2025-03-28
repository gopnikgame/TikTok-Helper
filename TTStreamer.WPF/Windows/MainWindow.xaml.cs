﻿// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Reflection;

using TTStreamer.WPF.Models;

using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace TTStreamer.WPF.Windows
{
    public partial class MainWindow
    {
        public MainWindow(
            INavigationService navigationService,
            IServiceProvider serviceProvider,
            ISnackbarService snackbarService,
            IContentDialogService contentDialogService,
            SettingsViewModel settingsView
        )
        {
            SettingsView = settingsView;
            DataContext = this;

            InitializeComponent();

            TileBar.Title = $"TTStreamer v.{Assembly.GetExecutingAssembly().GetName().Version}";

            navigationService.SetNavigationControl(NavigationView);
            snackbarService.SetSnackbarPresenter(SnackbarPresenter);
            contentDialogService.SetContentPresenter(RootContentDialog);

            NavigationView.SetServiceProvider(serviceProvider);
            //var dict = App.Current.Resources.MergedDictionaries;
            //StreamWriter writer = new StreamWriter("colors.xaml");
            //XamlWriter.Save(dict[0], writer);
        }

        public SettingsViewModel SettingsView { get; }
    }
}
