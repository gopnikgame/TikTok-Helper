using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TTStreamer.Services;
using TTStreamer.WPF.Extensions;

namespace TTStreamer.WPF.Models
{
    [ObservableObject]
    public partial class SoundItemView
    {
        [ObservableProperty]
        bool soundEnabled;

        [ObservableProperty]
        private string sound;
        private readonly SoundService soundService;

        public int Id { get; set; }
        public string Name { get; set; }
        public BitmapImage Image { get; set; }
        public ObservableCollection<string> SoundList { get; set; }

        public SoundItemView(SoundService soundService)
        {
            this.soundService = soundService;
        }

        [RelayCommand]
        public void Play()
        {
            var mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "assets", Sound)));
            mediaPlayer.Play();
        }

        partial void OnSoundChanged(string value)
        {
            soundService.Update(Id, value);
            SoundEnabled = true;
        }
    }

    [ObservableObject]
    public partial class SoundViewModel
    {
        public ObservableCollection<SoundItemView> ItemList { get; set; } = new ObservableCollection<SoundItemView>();

        public SoundViewModel(SoundService soundService, GiftService giftService)
        {
            var soundList = soundService.SoundList();
            var keySoundList = soundService.PlayList();
            var giftList = giftService.List();

            foreach (var gift in giftList)
            {
                var soundKey = keySoundList.FirstOrDefault(i => i.Key == gift.Id);
                var sound = soundList.FirstOrDefault(i => i == soundKey.Value);
                using var bmp = new Bitmap(new MemoryStream(Convert.FromBase64String(gift.Image)));


                ItemList.Add(new SoundItemView(soundService)
                {
                    Id = gift.Id,
                    Name = gift.Name,
                    Image = bmp.ToBitmapImage(),
                    Sound = sound,
                    SoundEnabled = sound is not null,
                    SoundList = new ObservableCollection<string>(soundList)
                });

            }
        }
    }
}
