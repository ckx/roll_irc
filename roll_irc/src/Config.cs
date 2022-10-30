using YamlDotNet.Serialization;

namespace roll_irc {
    /// <summary>
    /// YAML config file, default location is at config.yml.
    /// </summary>
    public class Config {
        public Config() {
            Server = "";
            ServerPort = 6667;
            SslEnabled = false;
            Nickname = "roll-chan";
            Password = "";
            Ident = "roll-chan";
            RealName = "roll-chan";
            CommandSequence = ".";
            Channels = new List<string>();
            ActiveModules = new List<string>();
            LoggingLevel = LogLevel.Info;

            GW2Config = new();
        }

        #region Properties
        public static string ConfigPath { get { return "config.yml"; } }
        public string Server { get; set; }
        public int ServerPort { get; set; }
        public bool SslEnabled { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public string Ident { get; set; }
        public string RealName { get; set; }
        public string CommandSequence { get; set; }
        public List<string> Channels { get; set; }
        public List<string> ActiveModules { get; set; }

        /// <summary>
        /// 0 = Error
        /// 1 = Warning
        /// 2 = Information
        /// 3 = Debug
        /// </summary>
        public LogLevel LoggingLevel { get; set; }
        #endregion

        #region Module Configs
        public GuildWars2Config GW2Config { get; set; }
        #endregion

        #region Static Methods
        public static async Task<Config> GetConfig() {
            Config retConf = new();
            var deserializer = new DeserializerBuilder().Build();
            using (var configString = File.ReadAllTextAsync(ConfigPath)) {
                retConf = deserializer.Deserialize<Config>(await configString);
            }

            return retConf;
        }

        public static void StoreConfig(Config runningConfig) {
            var serializer = new SerializerBuilder().Build();
            var yamlString = serializer.Serialize(runningConfig);
            try {
                using (StreamWriter sw = File.CreateText(ConfigPath)) {
                    sw.Write(yamlString);
                }
            } catch (Exception) {
                throw;
            }
        }
        #endregion
    }

    public class GuildWars2Config : IModuleConfig, IRestApiConfig {
        #region Constructor
        public GuildWars2Config() {
            Subscribers = new();
            BaseUri = "https://api.guildwars2.com";
        }
        #endregion

        #region IModuleConfig Properties
        public List<string> Subscribers { get; set; }
        #endregion

        #region IRestApiConfig Properties
        public string BaseUri { get; set; }
        #endregion
    }
}
