// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using TTStreamer.Models;

using Wpf.Ui.Controls;

namespace TTStreamer.WPF.Pages
{
    public partial class MonitoringPage : INavigableView<MonitoringViewModel>
    {
        public MonitoringViewModel ViewModel { get; }

        public MonitoringPage(MonitoringViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
