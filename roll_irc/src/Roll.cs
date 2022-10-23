using System.Net.Sockets;
using System.Net.Security;

// Most of the implementation is guided by RFC 1459: https://www.rfc-editor.org/rfc/rfc1459.html
namespace roll_irc {
    internal class Roll {
        internal Roll() {
            _ircStream = NetworkStream.Null;
            _streamWriter = StreamWriter.Null;
            _streamReader = StreamReader.Null;
        }

        private readonly TcpClient _ircClient = new();
        private Stream _ircStream;
        private StreamWriter _streamWriter;
        private StreamReader _streamReader;

        internal async Task Ikimasu(){
            Logger.CreateLogFile();
            Globals.RunConfig = await Config.GetConfig();
            Config.StoreConfig(Globals.RunConfig);

            using (_ircClient) {
                Logger.WriteLine($"Connecting to {Globals.RunConfig.Server}:{Globals.RunConfig.ServerPort}");
                await _ircClient.ConnectAsync(Globals.RunConfig.Server, Globals.RunConfig.ServerPort);
                if (!_ircClient.Connected) {
                    Logger.WriteLine($"Connection failed.", null, LogLevel.Error);
                }

                _ircStream = _ircClient.GetStream();
                if (Globals.RunConfig.SslEnabled) {
                    SslStream sslStream = new SslStream(_ircStream);
                    await sslStream.AuthenticateAsClientAsync(Globals.RunConfig.Server);
                    _ircStream = sslStream;
                }
                _streamReader = new(_ircStream);
                _streamWriter = new(_ircStream);

                using (_ircStream)
                using (_streamReader)
                using (_streamWriter) {
                    await SendCommand(IrcCommand.NICK, Globals.RunConfig.Nickname);
                    await SendCommand(IrcCommand.USER, $"{Globals.RunConfig.Ident} * 0 {Globals.RunConfig.RealName}");

                    while (_ircClient.Connected) {
                        string? ircData = await _streamReader.ReadLineAsync();
                        if (ircData != null) {
                            Message message = ParseMessage(ircData);
                            if (message.Sender == "PING") {
                                await SendCommand(IrcCommand.PONG, message.Content);
                                continue;
                            }

                            switch(message.Reply) {
                                case Reply.NONE:
                                    break;
                                case Reply.RPL_ENDOFMOTD:
                                case Reply.ERR_NOMOTD:
                                    Console.WriteLine("REPLY OBTAINED");
                                    await Identify();
                                    await JoinStartupChannels();
                                break;
                            }

                            switch (message.Command) {
                                case IrcCommand.NONE:
                                    break;
                                case IrcCommand.PRIVMSG:
                                    Console.WriteLine("LOL PRIVMSG");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        internal async Task Identify() {
            // Rizon's ID string. Fairly standard, but this string can change per server.
            await PrivMsg("NickServ", $"IDENTIFY {Globals.RunConfig.Password}");
        }

        internal async Task JoinStartupChannels() {
            foreach (string channel in Globals.RunConfig.Channels) {
                await SendCommand(IrcCommand.JOIN, $"{channel}");
                await _streamWriter.FlushAsync();
            }
        }

        internal static Message ParseMessage(string ircData) {
            Message retMessage = new();
            string[] line = ircData.Split(' ');
            string sender = "";

            // strip the colon from the sender
            if (line[0].StartsWith(':')) {
                sender = line[0].Substring(1);
            } else {
                sender = line[0];
            }
            retMessage.Sender = sender;

            // If we're parsing a PING, we can get the hash and early out.
            if (sender == "PING") {
                retMessage.Content = line[1];
                return retMessage;
            }

            // check for presence of a User name, else assume it's a server name
            if (sender.Contains('!')) {
                retMessage.Nick = sender.Split('!')[0];
                retMessage.User = Utility.StringSplit(sender, '!', '@');
                retMessage.Host = sender.Split('@')[1];
            }

            // Check for reply/command
            string msgType = "Unknown";
            if (Int32.TryParse(line[1], out int reply)) {
                retMessage.Reply = (Reply)reply;
                msgType = $"{retMessage.Reply}";
            }
            if (Enum.TryParse(line[1], true, out IrcCommand command)) {
                retMessage.Command = command;
                msgType = $"{retMessage.Command}";
            }
            retMessage.Receiver = line[2];
            if (line.Length >= 4) {
                retMessage.Content = String.Join(" ", line, 3, line.Length - 3); //line[3].Substring(1);
            }
            string formattedMessage = $"[{msgType}] [{retMessage.Receiver}] <{retMessage.Sender}> {retMessage.Content}";
            Logger.WriteLine(formattedMessage, Flow.Incoming, LogLevel.Info);

            return retMessage;
        }

        internal async Task PrivMsg(string receiver, string message) {
            string commandString = $"{IrcCommand.PRIVMSG} {receiver} :{message}";
            await _streamWriter.WriteLineAsync(commandString);
            await _streamWriter.FlushAsync();
            Logger.WriteLine($"{nameof(IrcCommand.PRIVMSG)} {receiver} {message}", Flow.Outgoing, LogLevel.Info);
        }

        internal async Task SendCommand(IrcCommand command, string parameters) {
            string commandString = $"{command} {parameters}";
            await _streamWriter.WriteLineAsync(commandString);
            await _streamWriter.FlushAsync();
            LogLevel logLevel = LogLevel.Info;
            if (Logger.CommandLogLevel.ContainsKey(command)) {
                logLevel = Logger.CommandLogLevel[command];
            }
            Logger.WriteLine($"{command} {parameters}", Flow.Outgoing, logLevel);
        }
    }
}
