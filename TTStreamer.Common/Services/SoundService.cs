using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Windows.Media;

namespace TTStreamer.Services
{
    public class SoundService
    {
        private string storeName = "giftsounds.json";
        private ConcurrentDictionary<int, string> store { get; set; } = new ConcurrentDictionary<int, string>();
        private DateTime playTime;
        private SemaphoreSlim sync = new(1);

        public SoundService()
        {
            if (File.Exists(storeName)) store = JsonSerializer.Deserialize<ConcurrentDictionary<int, string>>(File.ReadAllText(storeName));
        }

        public List<string> SoundList()
        {
            if (!Directory.Exists("assets")) return new List<string>();
            return new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "assets")).EnumerateFiles()
                .Where(info => info.Name.ToLower().EndsWith(".wav") || info.Name.ToLower().EndsWith(".mp3"))
                .Select(info => info.Name).ToList();
        }

        public List<KeyValuePair<int, string>> PlayList() => store.ToList();

        public void Update(int key, string value)
        {
            store.AddOrUpdate(key, value, (key, old) =>
            {
                if (old != value)
                {
                    store[key] = value;
                    File.WriteAllText(storeName, JsonSerializer.Serialize(store));
                }

                return value;
            });
        }

        public string Any()
        {
            var audioList = SoundList();
            audioList.RemoveAll(store.Values.Contains);
            return audioList[Random.Shared.Next(0, audioList.Count - 1)];
        }

        public async Task Play(int key, int delay)
        {
            try
            {
                await sync.WaitAsync();

                store.TryGetValue(key, out var sound);
                if (sound == null)
                {
                    sound = Any();
                    Update(key, sound);
                }

                var elapsed = DateTime.Now - playTime;
                var waitTime = TimeSpan.FromMilliseconds(delay) - elapsed;
                if (waitTime.Milliseconds > 0) await Task.Delay(waitTime);

                playTime = DateTime.Now;

                var mediaPlayer = new MediaPlayer();
                mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "assets", sound)));
                mediaPlayer.Play();
            }
            finally { sync.Release(); }
        }

    }
}
