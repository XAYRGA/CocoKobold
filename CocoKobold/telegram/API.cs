using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CocoKobold.Telegram
{

    public class APIRequestParameters : IDisposable
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();

        public void Dispose()
        {
            parameters.Clear();
            parameters = null;
        }

        public void setItem(string key, string value)
        {
            parameters[key] = value;
        }
        public void setItem(string key, int value)
        {
            parameters[key] = value.ToString();
        }
        public void setItem(string key, long value)
        {
            parameters[key] = value.ToString();
        }

        public Dictionary<string,string> toDictionary()
        {
            return parameters;
        }

        public string toJson()
        {
            return JsonConvert.SerializeObject(parameters);
        }
    }

    public abstract class TGAPIRequest { }


    internal class API
    {
        public const string INFOTAG = "TelegramAPI";

        private string APIKey;
        private string APIPath = "https://api.telegram.org/bot";
        private string APIEndpoint;
        private JsonSerializerSettings JsonSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
        private string lastError;

        public API(string APIToken)
        {
            APIKey = APIToken;
            APIEndpoint = $"{APIPath}{APIKey}/";
        }

        public class APIResponse
        {
            public bool ok;
            public JToken result;
        }

        public async Task<TGUser> getMe()
        {
            var data = await apiPostRequest("getMe");
            if (data.ok)
                return data.result.ToObject<TGUser>();
            return null;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public async Task<TGUpdate[]> getUpdatesSimple(long offset, int limit = 100, int timeout = 0)
        {
            var apiRequest = new APIRequestParameters();
            apiRequest.setItem("offset", offset);
            apiRequest.setItem("limit", limit);
            apiRequest.setItem("timeout", timeout);
            var data = await apiPostRequest("getUpdates", apiRequest);
            if (data.ok)
                return data.result.ToObject<TGUpdate[]>();
            return null;
        }

        public async Task<bool> sendMessageSimple(long chat, string message)
        {
            var apiRequest = new APIRequestParameters();
            apiRequest.setItem("chat_id", chat);
            apiRequest.setItem("text", message);
            var data = await apiPostRequest("sendMessage", apiRequest);
            if (data.ok)
                return true;
            return false;
        }


        public async Task<bool> acceptJoinRequestSimple(long chat, long user)
        {
            var apiRequest = new APIRequestParameters();
            apiRequest.setItem("chat_id", chat);
            apiRequest.setItem("user_id", user);
            var data = await apiPostRequest("approveChatJoinRequest", apiRequest);
            if (data.ok)
                return true;
            return false;
        }




        public async Task<bool> replyMessageSimple(TGMessage message, string text)
        {
            var apiRequest = new APIRequestParameters();
            apiRequest.setItem("chat_id", message.chat.id);
            apiRequest.setItem("text", text);
            apiRequest.setItem("reply_to_message_id", message.message_id);
            var data = await apiPostRequest("sendMessage", apiRequest);
            if (data.ok)
                return true;
            return false;
        }


        public async Task<bool> sendPhoto(long id, byte[] file)
        {
            var apiRequest = new APIRequestParameters();
            apiRequest.setItem("chat_id", id);
            var data = await apiPostRequestWithFile("sendPhoto", file,"photo","photo.jpg","image/jpeg",apiRequest); 
            if (data.ok)
                return true;
            return false;
        }


        public async Task<bool> sendVoice(long id, byte[] file)
        {
            var apiRequest = new APIRequestParameters();
            apiRequest.setItem("chat_id", id);
            var data = await apiPostRequestWithFile("sendVoice", file, "voice", "audio.ogg", "audio/ogg", apiRequest);
            if (data.ok)
                return true;
            return false;
        }




        public string encodeJSON(object anything)
        {
            return JsonConvert.SerializeObject(anything, Formatting.None, JsonSettings);
        }

        public T decodeJSON<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        public async Task<APIResponse> apiPostRequest(string endpoint, TGAPIRequest obj)
        {
            var json = encodeJSON(obj);
            return await apiPostRequest(endpoint, json);
        }
        public async Task<APIResponse> apiPostRequest(string endpoint, APIRequestParameters parameters)
        {
            return await apiPostRequest(endpoint, parameters.toJson());
        }
        public async Task<APIResponse> apiPostRequest(string endpoint, string data = "")
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
          
                    var cnt = new StringContent(data, Encoding.UTF8, "application/json");
                    var resultData = await client.PostAsync($"{APIEndpoint}{endpoint}", cnt);
                    //Console.WriteLine($"{APIEndpoint}{endpoint} -- {data}");
                    if (resultData == null || resultData.Content == null)
                        throw new Exception("No data received from endpoint");
                    var downloadedResponse = await resultData.Content.ReadAsStringAsync();

                    JObject tree = JObject.Parse(downloadedResponse);

                    return new APIResponse
                    {
                        ok = (bool)tree["ok"],
                        result = tree["result"],
                    };
                }
                catch (Exception F)
                {
                    lastError = F.ToString();
                    return new APIResponse
                    {
                        ok = false,
                    };
                }
            }
        }

        public async Task<APIResponse> apiPostRequestWithFile(string endpoint, byte[] fileData, string contentValue,string fileName, string contentType, APIRequestParameters apiData)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var w = new MultipartFormDataContent();
                    var dataX = new ByteArrayContent(fileData, 0, fileData.Length);
                    dataX.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);             

                    foreach (KeyValuePair<string, string> kvp in apiData.toDictionary())
                        w.Add(new StringContent(kvp.Value, Encoding.UTF8), kvp.Key);
                    w.Add(dataX,contentValue,fileName);

                    var resultData = await client.PostAsync($"{APIEndpoint}{endpoint}", w);

                    if (resultData == null || resultData.Content == null)
                        throw new Exception("No data received from endpoint");
                    var downloadedResponse = await resultData.Content.ReadAsStringAsync();
                    JObject tree = JObject.Parse(downloadedResponse);
                    return new APIResponse
                    {
                        ok = (bool)tree["ok"],
                        result = tree["result"],
                    };
                }
                catch (Exception F)
                {
                    lastError = F.ToString();
                    return new APIResponse
                    {
                        ok = false,
                    };
                }
            }

        }
    }
}




