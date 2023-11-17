using CocoKobold.Telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoCaptchaGenerator;


namespace CocoKobold
{

    internal enum KChallengeMode
    {
        CAPTCHA = 0,
        CAPTCHA_VISUAL_ONLY = 1,
        GENERIC_ACCEPT = 2,
    }

    internal class KoboldChallengeSession
    {
        public TGChat chat;
        public TGUser user;
        public DateTime whenCreated;

        public long user_chat_id;

        public int attempt = 0;
        public int maxAttempts = 0;

        public int generation = 0;
        public int generationAttempt = 0;



        public KChallengeMode challengeMode = KChallengeMode.CAPTCHA;
        public List<string> solutions = new List<string>();
        public string lastSolution;

        public bool generatedAudio = false;

        public bool invalid = false;

        public bool trySolve(string data)
        {
            for (int i = 0; i < solutions.Count; i++)
                if (data == solutions[i])
                    return true;
            return false;
        }
    }

    internal class KoBotLogic
    {
        const string INFOTAG = "kobologic";
        const int CAPTCHA_CHALLENGE_SIZE = 6;

        Telegram.API TelegramAPI;
        long last_update = 0;

        Dictionary<long, KoboldChallengeSession> Sessions = new Dictionary<long, KoboldChallengeSession>();

        public KoBotLogic(Telegram.API API)
        {

            TelegramAPI = API;
        }

        private async void createCaptchaRequest(TGChatJoinRequest request)
        {
            var chat = request.chat;
            var user = request.from;
          
            var uchatID = request.user_chat_id;

            if (getSession(user) != null)
            {
                await sendLocalizedMessage(uchatID, user.language_code, "system/sessionizeLimit", chat.title);
                return;
            }

            var challenge = new KoboldChallengeSession
            {
                chat = chat,
                user = user,
                user_chat_id = uchatID,
                challengeMode = KChallengeMode.CAPTCHA,
                maxAttempts = 3,
                attempt = 0,

                generation = 1,
                generationAttempt = 0,

                generatedAudio = false,

            };

            // Generate soltion and image
            var solution = CaptchaChallengeGenerator.GenerateChallenge(CAPTCHA_CHALLENGE_SIZE);
            challenge.solutions.Add(solution);
            byte[] captchaImage = CaptchaImageGenerator.GenerateCaptchaImageBytes(solution);
            challenge.lastSolution = solution;
            // send message

            await sendLocalizedMessage(uchatID, user.language_code, "verify/Captcha", chat.title);
            await TelegramAPI.sendPhoto(uchatID, captchaImage);

            // Store the session for processing
            Sessions[user.id] = challenge;
        }


        private KoboldChallengeSession getSession(TGUser user)
        {
            if (!Sessions.ContainsKey(user.id))
                return null;
            return Sessions[user.id];
        }


        private async void handleJoinRequest(TGChatJoinRequest request)
        {
            var timeSentReal = Telegram.API.UnixTimeStampToDateTime(request.date);
            if ((DateTime.Now - timeSentReal).TotalMinutes > 5)
                return;  // We fell behind. The token we have to talk to the user expires after 5 minutes.  Would be useless to process it.

            var chat = request.chat;
            var user = request.from;

            kmsg.message(INFOTAG, $"New sesison initialization for {user.first_name} {user.id}", MessageLevel.WARNING);
            createCaptchaRequest(request);
        }

        private async Task<bool> sendLocalizedMessage(long chat, string lang, string locale, params object[] args)
        {
            var localizedText = CocoInit.Localizer.getStringLocalized(lang, locale, args);
            await TelegramAPI.sendMessageSimple(chat, localizedText);
            return true;
        }

        private async void handleSessionedMessage(KoboldChallengeSession session, TGMessage message)
        {
            var text = message.text.ToUpper();
            var userLanguage = session.user.language_code;
            var chat_id = session.user_chat_id;

            if (userLanguage == null)
                userLanguage = "en";

            if (text == "AUDIO")
                if (session.generatedAudio == false)
                {
                    await sendLocalizedMessage(chat_id, userLanguage, "verify/responseRequestAudioMessage");
                    var audioData = CaptchaAudioGenerator.GenerateCaptchaAudio(session.lastSolution);
                    var oggData = OggEncoder.WavToOgg(audioData);
                    await TelegramAPI.sendVoice(session.user_chat_id, oggData);
                    session.generatedAudio = true;
                }
                else
                    await sendLocalizedMessage(chat_id, userLanguage, "verify/cantGenerateDuplicateAudioRequest");
            else if (text == "RETRY")
            {
                session.generationAttempt++;

                if (session.generatedAudio)
                    await sendLocalizedMessage(chat_id, userLanguage, "verify/cantGenerateNewCaptchaAfterAudio");
                else if (session.generation < 3)
                {
                    session.generation++;
                    var solution = CaptchaChallengeGenerator.GenerateChallenge(CAPTCHA_CHALLENGE_SIZE);
                    session.solutions.Add(solution);
                    byte[] captchaImage = CaptchaImageGenerator.GenerateCaptchaImageBytes(solution);
                    session.lastSolution = solution;
                    await sendLocalizedMessage(chat_id, userLanguage, "verify/generateNewCaptcha");
                    await TelegramAPI.sendPhoto(session.user_chat_id, captchaImage);
                }
                else if (session.generation >= 3)
                {
                    if (session.generationAttempt < 5)
                        await sendLocalizedMessage(chat_id, userLanguage, "verify/tooManyGenerations", session.generation);
                    else
                        await sendLocalizedMessage(chat_id, userLanguage, "system/badDetected");
                }
            }
            else if (!session.trySolve(text))
            {
                session.attempt++;
                if (session.attempt >= session.maxAttempts)
                {
                    session.invalid = true;
                    await sendLocalizedMessage(chat_id, userLanguage, "verify/failedTooMany");
                }
                else
                    await sendLocalizedMessage(chat_id, userLanguage, "verify/captchaFailed", session.maxAttempts - session.attempt);
            }
            else
            {
                await sendLocalizedMessage(chat_id, userLanguage, "verify/captchaSuccess", session.chat.title);
                await TelegramAPI.acceptJoinRequestSimple(session.chat.id, message.from.id);
            }
        }


        private async void handleRecvTextMessage(TGMessage message)
        {
            if (message.chat.type == "private")
            {

                // need switch statement for commands!
                KoboldChallengeSession session;
                if ((session = getSession(message.from)) != null && !session.invalid)
                    handleSessionedMessage(session, message);
            }
        }

        public void UpdateSessions()
        {
            var keys = Sessions.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                if (!Sessions.ContainsKey(key)) // just to check if it's been nuked by one of the async threads. 
                    continue;
                var session = Sessions[key];
                var delete = false;

                if (session.invalid || (session.whenCreated - DateTime.Now).TotalMinutes > 5f)
                    delete = true;

                if (delete)
                    Sessions.Remove(key);
            }
        }

        public void Update()
        {
            var UpdateMessages = TelegramAPI.getUpdatesSimple(last_update).Result;

            if (UpdateMessages == null)
                return;

            UpdateSessions();

            for (int i = 0; i < UpdateMessages.Length; i++)
            {
                var update = UpdateMessages[i];
                if (update.update_id >= last_update)
                    last_update = update.update_id + 1; // update the last ID so we don't process the same message twice, or 1000 times

                if (update.chat_join_request != null)
                    handleJoinRequest(update.chat_join_request);

                if (update.message != null && update.message.text != null && update.message.text.Length > 0)
                    handleRecvTextMessage(update.message);
            }
            Thread.Sleep(100);
            if (UpdateMessages.Length > 0)
                kmsg.message(INFOTAG, $"Got {UpdateMessages.Length} updates!");
        }
    }
}
