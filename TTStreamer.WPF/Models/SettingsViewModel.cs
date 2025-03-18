// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Windows.Media;

using TTStreamer.Services;

using Wpf.Ui.Appearance;

namespace TTStreamer.WPF.Models
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string speechVoice;

        [ObservableProperty]
        private int speechRate = 4;

        [ObservableProperty]
        private int notifyDelay = 200;

        [ObservableProperty]
        private string joinText = "@name подключился к стирмчику";

        [ObservableProperty]
        private string likeText = "@name активно лайкает";

        [ObservableProperty]
        private FontFamily fontFamily;

        [ObservableProperty]
        private string background = "https://kartinkis.cdnbro.com/posts/708675-krasivye-fony-dlia-afishi-61.jpg";

        [ObservableProperty]
        private ApplicationTheme theme = ApplicationThemeManager.GetAppTheme();

        public ObservableCollection<string> VoiceList { get; set; } = new ObservableCollection<string>();

        public SettingsViewModel(SpeechService speechService)
        {
            VoiceList = new ObservableCollection<string>(speechService.VoiceList());

            if (Settings.Default.Theme == default) Settings.Default.Theme = (int)theme;
            if (Settings.Default.MainFont == string.Empty) Settings.Default.MainFont = Fonts.SystemFontFamilies.First().ToString();
            if (Settings.Default.SpeechVoice == string.Empty) Settings.Default.SpeechVoice = VoiceList.First();
            if (Settings.Default.JoinText == string.Empty) Settings.Default.JoinText = joinText;
            if (Settings.Default.LikeText == string.Empty) Settings.Default.LikeText = likeText;
            if (Settings.Default.Background == string.Empty) Settings.Default.Background = background;
            if (Settings.Default.NotifyDelay == default) Settings.Default.NotifyDelay = notifyDelay;

            theme = (ApplicationTheme)Settings.Default.Theme;
            fontFamily = new FontFamily(Settings.Default.MainFont);
            speechVoice = Settings.Default.SpeechVoice;
            joinText = Settings.Default.JoinText;
            likeText = Settings.Default.LikeText;
            background = Settings.Default.Background;
            notifyDelay = Settings.Default.NotifyDelay;
        }

        partial void OnNotifyDelayChanged(int value)
        {
            Settings.Default.NotifyDelay = value;
            Settings.Default.Save();
        }

        partial void OnSpeechVoiceChanged(string value)
        {
            Settings.Default.SpeechVoice = value;
            Settings.Default.Save();
        }

        partial void OnSpeechRateChanged(int value)
        {
            Settings.Default.SpeechRate = value;
            Settings.Default.Save();
        }

        partial void OnLikeTextChanged(string value)
        {
            Settings.Default.LikeText = value;
            Settings.Default.Save();
        }

        partial void OnJoinTextChanged(string value)
        {
            Settings.Default.JoinText = value;
            Settings.Default.Save();
        }

        partial void OnBackgroundChanged(string value)
        {
            Settings.Default.Background = value;
            Settings.Default.Save();
        }

        partial void OnFontFamilyChanged(FontFamily value)
        {
            Settings.Default.MainFont = value.ToString();
            Settings.Default.Save();
        }

        partial void OnThemeChanged(ApplicationTheme value)
        {
            ApplicationThemeManager.Apply(value, Wpf.Ui.Controls.WindowBackdropType.Auto, true, true);
            Settings.Default.Theme = (int)value;
            Settings.Default.Save();
        }

        [RelayCommand]
        private void UpdateTheme(string parameter)
        {
            Theme = (ApplicationTheme)int.Parse(parameter);
        }
    }
}
