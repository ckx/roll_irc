namespace roll_irc {
    public enum LogLevel {
        Error,
        Warning,
        Info,
        Debug
    }

    public enum Command { 
        PRIVMSG,
        NOTICE,
        JOIN,
        WHO,
        WHOIS
    }

    public static class Reply {
        public const string RPL_ENDOFMOTD = "376";
        public const string ERR_NOMOTD = "422";
    }
}