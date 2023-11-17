using OggVorbisEncoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xayrga.bananapeel;

namespace CocoCaptchaGenerator
{
    // Slightly adapted from  https://github.com/SteveLillis/.NET-Ogg-Vorbis-Encoder/blob/master/OggVorbisEncoder.StreamExample/Encoder.cs
    public static class OggEncoder
    {
        private static readonly int WriteBufferSize = 512;
        private static readonly int[] SampleRates = { 8000, 11025, 16000, 22050, 32000, 44100 };


        public static byte[] WavToOgg(PCM16WAV wav)
        {
            float[][] samples = new float[1][];
            samples[0] = new float[((wav.buffer.Length / WriteBufferSize) + 1) *WriteBufferSize];

            for (int i = 0; i < wav.buffer.Length; i++)
                samples[0][i] = wav.buffer[i] / 32768f;

             return GenerateFile(samples, 8000, 1);
        }




        private static byte[] GenerateFile(float[][] floatSamples, int sampleRate, int channels)
        {
            using MemoryStream outputData = new MemoryStream();

            // Stores all the static vorbis bitstream settings
            var info = VorbisInfo.InitVariableBitRate(channels, sampleRate, 0.5f);

            // set up our packet->stream encoder
            var serial = new Random().Next();
            var oggStream = new OggStream(serial);

            var comments = new Comments();
            comments.AddTag("ARTIST", "TEST");

            var infoPacket = HeaderPacketBuilder.BuildInfoPacket(info);
            var commentsPacket = HeaderPacketBuilder.BuildCommentsPacket(comments);
            var booksPacket = HeaderPacketBuilder.BuildBooksPacket(info);

            oggStream.PacketIn(infoPacket);
            oggStream.PacketIn(commentsPacket);
            oggStream.PacketIn(booksPacket);

            // Flush to force audio data onto its own page per the spec
            FlushPages(oggStream, outputData, true);

            // =========================================================
            // BODY (Audio Data)
            // =========================================================
            var processingState = ProcessingState.Create(info);

            for (int readIndex = 0; readIndex <= floatSamples[0].Length; readIndex += WriteBufferSize)
            {
                if (readIndex == floatSamples[0].Length)
                    processingState.WriteEndOfStream();
                else
                    processingState.WriteData(floatSamples, WriteBufferSize, readIndex);

                while (!oggStream.Finished && processingState.PacketOut(out OggPacket packet))
                {
                    oggStream.PacketIn(packet);
                    FlushPages(oggStream, outputData, false);
                }
            }

            FlushPages(oggStream, outputData, true);
            var w = outputData.ToArray(); ;
            outputData.Dispose();
            return w;
        }

        private static void FlushPages(OggStream oggStream, Stream output, bool force)
        {
            while (oggStream.PageOut(out OggPage page, force))
            {
                output.Write(page.Header, 0, page.Header.Length);
                output.Write(page.Body, 0, page.Body.Length);
            }
        }
    }
}
