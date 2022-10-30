using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace roll_irc {
    internal class GuildWars2 : IModule {
        #region IModule Properties
        public string Name { get { return "Guild Wars 2"; } }
        public List<string>? Commands { get; private set; }
        public List<string>? Subscribers { get; set; }
        #endregion

        #region Singleton
        // Lazy Singleton https://csharpindepth.com/articles/singleton
        private static readonly Lazy<GuildWars2> _gw2 = new(() => new GuildWars2());

        public static IModule Instance { get { return _gw2.Value; } }
        #endregion

        #region IModule Methods
        public void AddCommands() {
            Commands = new List<string> {
                "gw2"
            };
        }

        public void AddSubscribers() {

        }

        public void ProcessCommand(string command) {

        }
        #endregion
    }
}
