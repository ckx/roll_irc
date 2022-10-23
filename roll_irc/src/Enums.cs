namespace roll_irc {
    public enum LogLevel {
        Error,
        Warning,
        Info,
        Debug
    }

    public enum IrcCommand {
        NONE,
        JOIN,
        NICK,
        NOTICE,
        PONG,
        PRIVMSG,
        USER,
        WHO,
        WHOIS
    }

    public enum Reply {
        NONE = 0,
        RPL_ENDOFMOTD = 376,
        ERR_NOMOTD = 422
    }

    public enum Flow {
        Incoming,
        Outgoing
    }
}