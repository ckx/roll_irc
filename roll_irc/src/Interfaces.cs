using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace roll_irc {
    public interface IModule {
        #region Properties
        public string Name { get; }
        public List<string>? Subscribers { get; set; }
        public List<string>? Commands { get; }
        #endregion

        #region Methods
        public void AddCommands();
        public void AddSubscribers();
        public void ProcessCommand(string command);
        #endregion
    }

    public interface IModuleConfig {
        #region Properties
        public List<string> Subscribers { get; set; }
        #endregion
    }

    public interface IRestApiConfig {
        #region Properties
        public string BaseUri { get; set; }
        #endregion
    }
}
