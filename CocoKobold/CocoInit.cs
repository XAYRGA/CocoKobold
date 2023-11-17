using CocoCaptchaGenerator;
using CocoKobold.Telegram;

namespace CocoKobold
{
    internal class CocoInit
    {

        public static KoConfig Config;
        public static LocalizationEngine Localizer;
        public static Telegram.API TelegramAPI;
        public static KoBotLogic BotLogic;

        private const string INFOTAG = "boot";


        static void Main(string[] args)
        {
            Console.WriteLine("Yip Yip!!!");
            kmsg.message(INFOTAG, "Collecting gold coins (configuration)...");
            Config = KoConfig.LoadFromFile();
            Config.SaveConfig();
            if (Config == null || Config.TelegramBotToken == null || Config.TelegramBotToken.Length < 5)
            {
                kmsg.message(INFOTAG, "Oops! Your configuration doesn't look right. It looks like you're missing the TelegramBotToken field!", MessageLevel.ERROR);
                return;
            }


            kmsg.message(INFOTAG, "Loading words (localization)...");

            Localizer = new LocalizationEngine();
            Localizer.LoadLanguages();

            kmsg.message(INFOTAG, "Callings dragons... (Connecting to telegram API)");
            TelegramAPI = new Telegram.API(Config.TelegramBotToken);
            TGUser me;
            if ((me = TelegramAPI.getMe().Result) != null)
                kmsg.message(INFOTAG, $"Yip! I'm {me.first_name} from @{me.username}!");
            else
            {
                kmsg.message(INFOTAG, "Yiiouch... I can't reach the dragon. (Telegram API Unreachable or API token invalid)", MessageLevel.ERROR);
                return;
            }

            BotLogic = new KoBotLogic(TelegramAPI);
            while (true)            
                BotLogic.Update();
        }
    }
}