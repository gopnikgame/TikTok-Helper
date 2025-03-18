using TTStreamer.Common.Services;

namespace TTStreamer.Tests
{
    [TestClass]
    public class SpeechServiceTest
    {

        [TestMethod]
        public async Task Speech()
        {
            var audio = new SpeechService();
            var voiceList = audio.VoiceList();
            await audio.Speech("������", voiceList[0], 4);
            await audio.Speech("hello how are you", voiceList[1], 4);
        }

    }
}