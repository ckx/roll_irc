using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace roll_irc {
    internal class GuildWars2 : IModule {
        #region IModule Properties
        public string Name { get { return "GW2"; } }
        public List<string>? Commands { get; private set; }
        public List<string>? Subscribers { get; set; }
        #endregion

        #region IModule Methods
        public void AddCommands() {
            Commands = new List<string> {
                "gw2"
            };
        }

        public void ProcessCommand(string command) {

        }
        #endregion
    }
}
