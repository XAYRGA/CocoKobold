using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xayrga.bananapeel;

namespace CocoCaptchaGenerator
{
    public static class CaptchaAudioGenerator
    {
        const int OUTPUT_SAMPLERATE = 6000;

        static Dictionary<char,PCM16WAV> cache = new Dictionary<char,PCM16WAV>();
        private static PCM16WAV GetVoicePCMBuffer(char letter)
        {
            if (cache.ContainsKey(letter))
                return cache[letter];

            var ltrStr = letter.ToString().ToUpper();
            Console.WriteLine($"Loaded PCM buffer: VoiceBank/{ltrStr}.wav");
            using (var stm = File.OpenRead($"VoiceBank/{ltrStr}.wav"))
            using (var br = new BinaryReader(stm))            
                return (cache[letter] = PCM16WAV.readStream(br));
            
        }
        public static PCM16WAV GenerateCaptchaAudio(string text)
        {
            var outWave = new PCM16WAV()
            {
                sampleRate = OUTPUT_SAMPLERATE,
                format = 1,
                channels = 1,
                blockAlign = 2,
                bitsPerSample = 16
            };
           
            // Collect sample count;
            var totalSamples = 0;
            for (int i = 0; i < text.Length; i++)
                totalSamples += GetVoicePCMBuffer(text[i]).buffer.Length;

            outWave.buffer = new short[totalSamples];

            var sOffset = 0;
            for (int i=0; i < text.Length; i++)
            {
                var pcmData = GetVoicePCMBuffer(text[i]);
                Array.Copy(pcmData.buffer,0,outWave.buffer,sOffset, pcmData.buffer.Length);
                sOffset += pcmData.buffer.Length;
            }
            return outWave;
        }

        public static void GenerateCaptchaAudioToFile(string outFile, string text)
        {
            var outWave = GenerateCaptchaAudio(text);
            using (var buffer = File.OpenWrite(outFile))
            using (var bw = new BinaryWriter(buffer))
            {
                outWave.writeStreamLazy(bw);
                bw.Flush();
                buffer.Flush();
            }
        }

   
    }
}
