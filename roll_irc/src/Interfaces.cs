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
        public void ProcessCommand(string command);
        #endregion
    }
}
