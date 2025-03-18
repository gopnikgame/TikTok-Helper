using System.Speech.Synthesis;

namespace TTStreamer.Services
{
    public class SpeechService
    {
        private SpeechSynthesizer synthesizer = new SpeechSynthesizer() { Rate = 4 };

        public List<string> VoiceList()
        {
            var voiceList = synthesizer.GetInstalledVoices();
            return voiceList.Select(i => i.VoiceInfo.Name).ToList();
        }

        public Task Speech(string text, string voice, int rate) => Task.Run(() =>
        {
            if (voice != null && synthesizer.Voice.Name != voice) synthesizer.SelectVoice(voice);
            if (synthesizer.Rate != rate) synthesizer.Rate = rate;
            synthesizer.Speak(text);
        });
    }
}