// MonitoringViewModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TTStreamer.Common.Services;
using TTStreamer.WPF;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace TTStreamer.Models
{
    public record TableItemView(DateTime Date, string Name, string Event, int AlertLevel = 0);

    public partial class MonitoringViewModel : ObservableObject
    {
        private NodeConnector _nodeConnector;
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

            _nodeConnector = new NodeConnector();
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
                    await _nodeConnector.SendCommandAsync("disconnect", new Dictionary<string, object> { { "username", Stream } });
                    return;
                }

                ItemList.Clear();

                var options = new Dictionary<string, object> { { "enableExtendedGiftInfo", true } };
                string response = await _nodeConnector.SendCommandAsync("connect", new Dictionary<string, object> { { "username", Stream }, { "options", options } });
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response);

                if (jsonResponse.status != "success")
                {
                    throw new Exception(jsonResponse.message);
                }

                IsMonitoring = true;
            }
            catch (Exception ex)
            {
                IsMonitoring = false;
                snackbarService.Show("ошибка", ex.InnerException?.Message ?? ex.Message, ControlAppearance.Danger, null, snackbarService.DefaultTimeOut);
            }
        }

        private async void OnDisconnected(dynamic disconnectionInfo)
        {
            IsMonitoring = false;
            if (disconnectionInfo.IsFailure)
            {
                await Application.Current.Dispatcher.InvokeAsync(() => snackbarService.Show("ошибка", disconnectionInfo.FailureCause, ControlAppearance.Danger, null, snackbarService.DefaultTimeOut));
            }
        }

        private async void MemberUpdate(dynamic msg)
        {
            if (msg.User == null) return;
            if (SpeechMember)
            {
                await speechService.Speech(Settings.Default.JoinText.Replace("@name", msg.User.Nickname), Settings.Default.SpeechVoice, Settings.Default.SpeechRate);
            }
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ItemList.Insert(0, new TableItemView(DateTime.Now, msg.User.Nickname, "подключение"));
                if (ItemList.Count > 1000) ItemList.Remove(ItemList.Last());
            });
        }

        private async void LikeUpdate(dynamic msg)
        {
            if (SpeechLike)
            {
                await speechService.Speech(Settings.Default.LikeText.Replace("@name", msg.User.Nickname), Settings.Default.SpeechVoice, Settings.Default.SpeechRate);
            }
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ItemList.Insert(0, new TableItemView(DateTime.Now, msg.User.Nickname, "лайк"));
                if (ItemList.Count > 1000) ItemList.Remove(ItemList.Last());
            });
        }

        private async void GiftUpdate(dynamic msg)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ItemList.Insert(0, new TableItemView(DateTime.Now, msg.User.Nickname, $"донат {msg.giftDetails.giftName}", 1));
                if (ItemList.Count > 1000) ItemList.Remove(ItemList.Last());
            });

            if (SpeechGift)
            {
                await speechService.Speech($"{msg.User.Nickname} прислал {msg.giftDetails.giftName}", Settings.Default.SpeechVoice, Settings.Default.SpeechRate);
            }
            if (NotifyGift)
            {
                await soundService.Play(msg.giftId, Settings.Default.NotifyDelay);
            }
        }

        private async Task HandleEvents()
        {
            while (IsMonitoring)
            {
                try
                {
                    string response = await _nodeConnector.SendCommandAsync("room-info", new Dictionary<string, object> { { "username", Stream } });
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response);

                    if (jsonResponse.status == "success")
                    {
                        dynamic roomInfo = jsonResponse.roomInfo;
                        foreach (var eventInfo in roomInfo.events)
                        {
                            switch (eventInfo.type)
                            {
                                case "chat":
                                    MemberUpdate(eventInfo.data);
                                    break;
                                case "like":
                                    LikeUpdate(eventInfo.data);
                                    break;
                                case "gift":
                                    GiftUpdate(eventInfo.data);
                                    break;
                                case "disconnection":
                                    OnDisconnected(eventInfo.data);
                                    break;
                                default:
                                    logger.LogWarning($"Unknown event type: {eventInfo.type}");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        logger.LogError($"Error fetching room info: {jsonResponse.message}");
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() => snackbarService.Show("ошибка", ex.Message, ControlAppearance.Danger, null, snackbarService.DefaultTimeOut));
                    IsMonitoring = false;
                }
                await Task.Delay(1000); // Ожидание перед следующим запросом
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(IsMonitoring))
            {
                if (IsMonitoring)
                {
                    _ = HandleEvents();
                }
                else
                {
                    _nodeConnector.Dispose();
                }
            }
        }

        public void StartMonitoring()
        {
            if (IsMonitoring)
            {
                _ = HandleEvents();
            }
        }

        public void StopMonitoring()
        {
            _nodeConnector.Dispose();
        }
    }
}