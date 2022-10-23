namespace roll_irc {
    internal static class Logger {
        internal static string Timestamp { get {
                return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            } }

        private static string _logPath = "";

        public static Dictionary<IrcCommand, LogLevel> CommandLogLevel = new() {
            { IrcCommand.PONG, LogLevel.Debug }
        };

        internal static void CreateLogFile() {
            var logDirectory = "Logs";
            System.IO.Directory.CreateDirectory(logDirectory);
            var logFileName = $"[{DateTime.Now:yyyy-MM-dd}]rollchan-{DateTime.Now:HHmmss}.log";
            _logPath = Path.Combine(logDirectory, logFileName);
            try {
                using (StreamWriter sw = File.CreateText(_logPath)) {
                    sw.WriteLine($"[{Timestamp}] Roll-chan, ganbarimasu!");
                }
            } catch (Exception) {
                throw;
            }
            Console.WriteLine($"Log file initiated at {_logPath}");
        }

        internal static void WriteLine(string logMessage, string? callerName = null, LogLevel? logLevel = null) {
#if DEBUG
            if (Globals.SilenceLogger) {
                return;
            }
#endif
            logLevel ??= LogLevel.Info;
            callerName ??= "roll-core";
            string line;
            line = $"[{Timestamp}] [{logLevel}] [{callerName}] {logMessage}";
            CommitLog(line, logLevel);
        }

        internal static void WriteLine(string logMessage, Flow flow, LogLevel? logLevel = null) {
#if DEBUG
            if (Globals.SilenceLogger) {
                return;
            }
#endif
            logLevel ??= LogLevel.Info;
            string line;
            line = $"[{Timestamp}] [{logLevel}] [{flow}] {logMessage}";
            CommitLog(line, logLevel);
        }

        private static void CommitLog(string line, LogLevel? logLevel) {
            // console output, always happens
            Console.WriteLine(line);
            // write to file system only on appropriate config/loglevel
            if (Globals.RunConfig.LoggingLevel >= logLevel) {
                try {
                    using (StreamWriter sw = File.AppendText(_logPath)) {
                        sw.WriteLine(line);
                    }
                } catch (Exception) {
                    throw;
                }
            }
        }
    }
}
