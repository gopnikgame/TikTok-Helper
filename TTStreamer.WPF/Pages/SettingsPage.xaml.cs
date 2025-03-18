// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using TTStreamer.WPF.Models;

using Wpf.Ui.Controls;

namespace TTStreamer.WPF.Pages
{
    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            DataContext = this;

            InitializeComponent();
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.FontFamily))
                App.Current.MainWindow.FontFamily = ViewModel.FontFamily;
        }
    }
}
