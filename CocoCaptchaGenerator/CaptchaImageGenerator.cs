﻿
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CocoCaptchaGenerator
{
    public static class CaptchaImageGenerator
    {
        private static Random Rng = new Random(DateTime.Now.Millisecond);
        private struct CaptchaImagePallet
        {
            public SKColor background;
            public List<SKColor> textColors;
            public List<SKColor> artifactColors;
        }

        private static List<CaptchaImagePallet> pallets = new List<CaptchaImagePallet>()
        {
            new CaptchaImagePallet
            {
                background = new SKColor(22,0,73),
                textColors =new List<SKColor>()
                {
                    new SKColor(0xF90068FF),
                    new SKColor(0xFCFF00FF),
                    new SKColor(0xFFC700FF),
                    new SKColor(0xFFF4F4FF)
                },
                artifactColors = new List<SKColor>()
                {
                    new SKColor(0x9D00FF47),
                    new SKColor(0xFF7B0047)
                }
            },
            
            new CaptchaImagePallet
            {
                background = new SKColor(0x722D00FFu),
                textColors =new List<SKColor>()
                {
                    new SKColor(0xFFFF90FFu),
                    new SKColor(0xCB00FFFFu),
                    new SKColor(0xFF00FFFFu),
                    new SKColor(0x8CFFDCFFu)
                },
                artifactColors = new List<SKColor>()
                {
                    new SKColor(0x7FC9FFFFu),
                    new SKColor(0xD67FFFFFu)
                }
            },
            
            
            new CaptchaImagePallet
            {
                background = new SKColor(0x57007FFFu),
                textColors =new List<SKColor>()
                {
                    new SKColor(0xCFFF00FFu),
                    new SKColor(0xF6FF00FFu),
                    new SKColor(0xFFBBFFFFu),
                    new SKColor(0xFF00B2FFu)
                },
                artifactColors = new List<SKColor>()
                {
                    new SKColor(0xFF26FFFFu),
                    new SKColor(0xFFFFE5FFu)
                }
            }
        };

        private static SKBitmap GetArtifact()
        {
            var files = Directory.GetFiles("ImageArtifacts");
            var rndArti = files[Rng.Next(0, files.Length)];
            SKBitmap rtn;
            using (var strm = File.OpenRead(rndArti))
            {
                rtn = SKBitmap.Decode(rndArti);
            }
            return rtn;
        }

        public static void GenerateCaptchaImageStream(Stream output, string text)
        {
            text = text.ToUpper();

            var info = new SKBitmap(256, 128);
            var imageYMidpoint = 128f / 2f;

            CaptchaImagePallet pallet = pallets[Rng.Next(0, pallets.Count())];

            using (var surface = new SKCanvas(info))
            {
                surface.Clear(pallet.background);
                var randomArtifactColor = pallet.artifactColors[Rng.Next(0, pallet.artifactColors.Count)];

                var lastX = 30f;

                for (int i = 0; i < text.Length; i++)
                {
                    var character = text[i];
                    var palletColorIndex = Rng.Next(0, pallet.textColors.Count);
                    var randomPalletColor = pallet.textColors[palletColorIndex];
#if DEBUG 
                    Console.WriteLine($"Selecting color index {palletColorIndex}");
#endif


                    using (var paint = new SKPaint())
                    {
                        paint.TextSize = 50.0f + ((Rng.NextSingle() - 0.2f) * 20f);
                        paint.IsAntialias = true;
                        paint.Color = randomPalletColor;
                        paint.IsStroke = (Rng.NextSingle() > 0.5f);
                        paint.StrokeWidth = (Rng.NextSingle() * 2f) + 1f;
                        paint.TextAlign = SKTextAlign.Center;
                        paint.TextSkewX = Rng.NextSingle() * 0.7f;                        
                        lastX += paint.MeasureText(character.ToString()) * (i==0 ? 0 : 1); // Don't shift text left if we're 0th character
                        

                        surface.DrawText(character.ToString(), lastX, imageYMidpoint + ((Rng.NextSingle() - 0.5f) * (imageYMidpoint / 3f)), paint);

                    }
                }
                if (Rng.NextSingle() > 0.30)
                {
                    using (var artifact = GetArtifact())
                    using (var paint = new SKPaint())
                    {
                        paint.Color = randomArtifactColor;
                        surface.RotateDegrees(30 * ((Rng.NextSingle() - 0.5f) * 2f));
                        surface.DrawBitmap(artifact, (Rng.NextSingle() * (info.Width)), 0);
                    }
                }
            }
            var data = info.Encode(SKEncodedImageFormat.Jpeg, 50);
            data.SaveTo(output); ;
            info.Dispose();
        }

        public static void GenerateCaptchaImageToFile(string file, string text)
        {           
            var kk = File.OpenWrite(file);
            GenerateCaptchaImageStream(kk, text);
            kk.Flush();
            kk.Close();
        }

        public static byte[] GenerateCaptchaImageBytes(string text)
        {
            using (var kk = new MemoryStream())
            {
                GenerateCaptchaImageStream(kk, text);
                return kk.ToArray();
            }
        }
    }
}
