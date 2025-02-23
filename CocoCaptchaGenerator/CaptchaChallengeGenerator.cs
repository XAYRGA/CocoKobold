using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoCaptchaGenerator
{
    public static class CaptchaChallengeGenerator
    {
        private static Random Rng = new Random(DateTime.Now.Millisecond);
        private static char[] characterSet = {
            '1','2','3','4','5','6','7','8','9',
            'A','B','C','D','E','F','G','H',// 'I', has a really bad glyph. 
            'J','K','L','M','N','O','P','Q','R',
            'S','T','U','V','W','X','Y','Z',
        };

        private static string[] funnyHahaWords = new string[]
        {
            "SERGAL",
            "KOBOLD",
            "DRAGON",
            "DEER", 
            "PROTO",
            "XAYRGA",
            "69420",
            "MERPIN",
            "LIZARD", 
            "MANEYS",
            "VORE",
            "POOTIS",
            "BANANA",
        };

        public static string GenerateChallenge(int length)
        {
            var startingWord = "";
            var startingLength = 0;
            if (Rng.NextSingle() > 0.90f)
            {
                startingWord = funnyHahaWords[Rng.Next(0, funnyHahaWords.Length)];
                startingLength = startingWord.Length;
            }

            var addExtra = length - startingLength;
            char[] result = new char[addExtra];
            for (int i = 0; i < addExtra; i++)
                result[i] = characterSet[Rng.Next(0,characterSet.Length)];

            return startingWord + (new string(result));
        }
    }
}
