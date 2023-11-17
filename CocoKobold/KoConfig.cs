using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CocoKobold
{
    public class KoConfig
    {
        public string TelegramBotToken;

        /*
        Headless without database for now. 
        public string MySQL_Host; 
        public string MySQL_Port;
        public string MySQL_Username;
        public string MySQL_Password;
        public string MySQL_Database;
        */

        public static KoConfig LoadFromFile(string filename = "koconfig.json")
        {
            var config = new KoConfig();
            if (File.Exists(filename))
            {
                var json = File.ReadAllText(filename);
                config = JsonConvert.DeserializeObject<KoConfig>(json);
            }
            return config;
        }

        public void SaveConfig(string filename = "koconfig.json")
        {
            var json = JsonConvert.SerializeObject(this,Formatting.Indented);
            File.WriteAllText(filename, json);   
        }
    }
}
