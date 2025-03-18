using TTStreamer.Models;

//using Wpf.Ui.Controls;
using Wpf.Ui.Abstractions.Controls;

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
