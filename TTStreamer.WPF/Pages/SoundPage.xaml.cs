// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using TTStreamer.WPF.Models;

using Wpf.Ui.Controls;

namespace TTStreamer.WPF.Pages
{
    public partial class SoundPage : INavigableView<SoundViewModel>
    {
        public SoundViewModel ViewModel { get; }

        public SoundPage(SoundViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
