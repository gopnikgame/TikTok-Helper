using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;

using Flurl.Http;

using TTStreamer.Data;

namespace TTStreamer.Services
{
    public class GiftService
    {
        private ConcurrentDictionary<int, GiftData>? giftDict = new ConcurrentDictionary<int, GiftData>();
        private readonly string storeName = "giftinfos.json";

        public GiftService()
        {
            if (File.Exists(storeName)) giftDict = new ConcurrentDictionary<int, GiftData>(JsonSerializer.Deserialize<Dictionary<int, GiftData>>(File.ReadAllText(storeName)));
        }

        public List<GiftData> List() => giftDict.Values.ToList();
        public GiftData Find(int id) => giftDict.ContainsKey(id) ? giftDict[id] : null;
        public async Task<GiftData> Create(int id, string name, string url)
        {
            var giftData = new GiftData() { Image = Convert.ToBase64String(await url.GetBytesAsync()), Id = id, Name = name };
            giftDict.TryAdd(id, giftData);
            return giftData;
        }
    }
}