
using SkiaSharp;

namespace CocoCaptchaGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var challenge = "sergal";
            var waveData = CaptchaAudioGenerator.GenerateCaptchaAudio(challenge);
            File.WriteAllBytes("test.ogg",OggEncoder.WavToOgg(waveData));
            CaptchaImageGenerator.GenerateCaptchaImageToFile($"{challenge}.jpg", challenge);
        }

 
    }
}