using TTStreamer.Services;

namespace TikTokStream.Tests
{
    [TestClass]
    public class SoundServiceTest
    {
        [TestMethod]
        public async Task Play()
        {
            var service = new SoundService();
            for (int i = 0; i < 10; i++) await service.Play(5719,200);
            Console.ReadLine();
        }

    }
}