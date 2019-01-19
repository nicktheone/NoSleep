using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace NoSleep
{
    class Config
    {
        #region Properties

        public bool enabled { get; set; }

        #endregion

        #region Methods

        //Write the JSON file to the disk (https://stackoverflow.com/questions/16921652/how-to-write-a-json-file-in-c)
        public static void WriteConfig(Config config)
        {
            using (StreamWriter streamWriter = File.CreateText(Path.Combine(GetNoSleepPath(), @"config.json")))
            {
                JsonSerializer jsonSerializer = new JsonSerializer()
                {
                    Formatting = Formatting.Indented
                };

                jsonSerializer.Serialize(streamWriter, config);
            }
        }

        //Read the JSON config file
        public static Config ReadConfig()
        {
            string s = "";

            using (StreamReader streamReader = new StreamReader(Path.Combine(GetNoSleepPath(), @"config.json")))
            {
                s = streamReader.ReadToEnd();
            }

            Config config = JsonConvert.DeserializeObject<Config>(s);

            return config;
        }

        //Get the app folder path
        private static string GetNoSleepPath()
        {
            //Get the AppData path
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //Don't add the "\" at the beginning of the path2 or else it'll return path2 (https://stackoverflow.com/questions/18008276/why-the-path-combine-is-not-combining-the-path-and-file)
            string noSleepPath = Path.Combine(appDataPath, @"NoSleep");

            return noSleepPath;
        }

        //Create app folder and config file if not already existing
        public static void FirstSetup()
        {
            if (!Directory.Exists(GetNoSleepPath()))
            {
                Directory.CreateDirectory(GetNoSleepPath());
            }

            if (!File.Exists(Path.Combine(GetNoSleepPath(), @"config.json")))
            {
                WriteConfig(new Config() { enabled = false });
            }
        }

        #endregion
    }
}
