﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace roll_irc {
    internal struct Message {
        internal Command Command { get; set; }
        internal string SeverName { get; set; }
        internal string Nick { get; set; }
        internal string User { get; set; }
        internal string Host { get; set; }
        internal string Receiver { get; set; }
        internal string Content { get; set; }
    }
}