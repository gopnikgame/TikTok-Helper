using System.Collections.ObjectModel;
using System.Windows.Threading;

using MediatR;

using Microsoft.Extensions.Logging;

using TikTokLiveDotNet;
using TikTokLiveDotNet.Protobuf;

using TTStreamer.Services;
using TTStreamer.WPF;
using TTStreamer.WPF.Extensions;

using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace TTStreamer.Models
{
    public record TableItemView(DateTime Date, string Name, string Event, int AlertLevel = 0);

    public partial class MonitoringViewModel : ObservableObject
    {
        private TikTokLiveClient tikTokLiveClient;
        private readonly IMediator mediator;
        private readonly ISnackbarService snackbarService;
        private readonly IContentDialogService dialogService;
        private readonly SpeechService speechService;
        private readonly SoundService soundService;
        private readonly ILogger<MonitoringViewModel> logger;

        [ObservableProperty]
        private bool isMonitoring;

        [ObservableProperty]
        private bool isProcessing;

        [ObservableProperty]
        private string stream = string.Empty;

        [ObservableProperty]
        bool notifyGift;

        [ObservableProperty]
        bool speechGift;

        [ObservableProperty]
        bool speechLike;

        [ObservableProperty]
        bool speechMember;

        public ObservableCollection<TableItemView> ItemList { get; set; } = new ObservableCollection<TableItemView>();


        public MonitoringViewModel(
            IMediator mediator,
            ISnackbarService snackbarService,
            IContentDialogService dialogService,
            SpeechService speechService,
            SoundService soundService,
            ILogger<MonitoringViewModel> logger)
        {
            this.mediator = mediator;
            this.snackbarService = snackbarService;
            this.dialogService = dialogService;
            this.speechService = speechService;
            this.soundService = soundService;
            this.logger = logger;

            stream = Settings.Default.UserId;
            notifyGift = Settings.Default.NotifyGift;
            speechGift = Settings.Default.SpeechGift;
            speechLike = Settings.Default.SpeechLike;
            speechMember = Settings.Default.SpeechMember;
        }

        partial void OnNotifyGiftChanged(bool value)
        {
            Settings.Default.NotifyGift = value;
            Settings.Default.Save();
        }

        partial void OnSpeechGiftChanged(bool value)
        {
            Settings.Default.SpeechGift = value;
            Settings.Default.Save();
        }

        partial void OnSpeechLikeChanged(bool value)
        {
            Settings.Default.SpeechLike = value;
            Settings.Default.Save();
        }

        partial void OnSpeechMemberChanged(bool value)
        {
            Settings.Default.SpeechMember = value;
            Settings.Default.Save();
        }

        partial void OnStreamChanged(string value)
        {
            Settings.Default.UserId = value;
            Settings.Default.Save();
        }


        [RelayCommand]
        private async Task Monitoring()
        {
            try
            {
                if (!IsMonitoring)
                {
                    tikTokLiveClient.Disconnect();
                    tikTokLiveClient.Dispose();
                    return;
                }

                ItemList.Clear();

                tikTokLiveClient = new TikTokLiveClient(Stream);
                tikTokLiveClient.GiftMessageReceived.Subscribe(GiftUpdate);
                tikTokLiveClient.LikeMessageReceived.Subscribe(LikeUpdate);
                tikTokLiveClient.MemberMessageReceived.Subscribe(MemberUpdate);
                tikTokLiveClient.DisconnectionHappened.Subscribe(OnDisconnected);
                await tikTokLiveClient.Connect();

                if (tikTokLiveClient.ClientState.IsConnecting)
                {
                    tikTokLiveClient.Disconnect();
                    throw new Exception("неудалсь подключиться");
                }

                if (!tikTokLiveClient.ClientState.IsConnected) throw new Exception("неудалсь подключиться");
            }
            catch (Exception ex)
            {
                tikTokLiveClient.Dispose();
                IsMonitoring = false;
                snackbarService.Show("ошибка", ex.InnerException?.Message ?? ex.Message, ControlAppearance.Danger);
            }
        }

        private void OnDisconnected(TikTokLiveDotNet.Notifications.DisconnectionInfo disconnectionInfo)
        {
            tikTokLiveClient.Dispose();
            IsMonitoring = false;
            if (disconnectionInfo.IsFailure) Application.Current.Dispatcher.Invoke(() => snackbarService.Show("ошибка", disconnectionInfo.FailureCause, ControlAppearance.Danger));
        }

        private async void MemberUpdate(WebcastMemberMessage msg)
        {
            if (msg.User == null) return;
            if (SpeechMember) await speechService.Speech(Settings.Default.JoinText.Replace("@name", msg.User.Nickname), Settings.Default.SpeechVoice, Settings.Default.SpeechRate);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ItemList.Insert(0, new TableItemView(msg.Event.createTime.JavaTimeToDateTime(), msg.User.Nickname, "подключение"));
                if (ItemList.Count > 1000) ItemList.Remove(ItemList.Last());
            });
        }

        private async void LikeUpdate(WebcastLikeMessage msg)
        {
            if (SpeechLike) await speechService.Speech(Settings.Default.LikeText.Replace("@name", msg.User.Nickname), Settings.Default.SpeechVoice, Settings.Default.SpeechRate);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ItemList.Insert(0, new TableItemView(msg.Event.createTime.JavaTimeToDateTime(), msg.User.Nickname, "лайк"));
                if (ItemList.Count > 1000) ItemList.Remove(ItemList.Last());
            });
        }


        private async void GiftUpdate(WebcastGiftMessage msg)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ItemList.Insert(0, new TableItemView(msg.Event.createTime.JavaTimeToDateTime(), msg.User.Nickname, $"донат {msg.giftDetails.giftName}", 1));
                if (ItemList.Count > 1000) ItemList.Remove(ItemList.Last());
            });

            if (SpeechGift) await speechService.Speech($"{msg.User.Nickname} прислал {msg.giftDetails.giftName}", Settings.Default.SpeechVoice, Settings.Default.SpeechRate);
            if (NotifyGift) await soundService.Play(msg.giftId, Settings.Default.NotifyDelay);
        }
    }
}