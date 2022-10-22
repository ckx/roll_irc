using System.Net.Sockets;
using System.Net.Security;

// Most of the implenetation is guided by RFC 1459: https://www.rfc-editor.org/rfc/rfc1459.html
namespace roll_irc {
    internal class Roll {
        internal Roll() {
            _dataStream = NetworkStream.Null;
            _streamWriter = StreamWriter.Null;
            _streamReader = StreamReader.Null;
        }

        private readonly TcpClient _ircClient = new();
        private Stream _dataStream;
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
                    Logger.WriteLine($"Connection failed.");
                }

                _dataStream = _ircClient.GetStream();
                if (Globals.RunConfig.SslEnabled) {
                    SslStream sslStream = new SslStream(_dataStream);
                    await sslStream.AuthenticateAsClientAsync(Globals.RunConfig.Server);
                    _dataStream = sslStream;
                }
                _streamReader = new(_dataStream);
                _streamWriter = new(_dataStream);

                using (_dataStream)
                using (_streamReader)
                using (_streamWriter) {
                    await _streamWriter.WriteLineAsync($"NICK {Globals.RunConfig.Nickname}");
                    await _streamWriter.WriteLineAsync($"USER {Globals.RunConfig.Ident} * 0 {Globals.RunConfig.RealName}");
                    await _streamWriter.FlushAsync();

                    while (_ircClient.Connected) {
                        string? ircData = await _streamReader.ReadLineAsync();
                        if (ircData != null) {
                            string[] line = ircData.Split(' ');
                            if (line[0] == "PING") {
                                await Pong(line);
                                continue;
                            }
                            Logger.WriteLine($"{ircData}");

                            if (line.Length > 1) {
                                switch (line[1]) {
                                    case Reply.RPL_ENDOFMOTD:
                                    case Reply.ERR_NOMOTD:
                                        await Identify();
                                        await JoinStartupChannels();
                                        break;
                                    case nameof(Command.PRIVMSG):
                                        ParseMessage(line);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        internal async Task Pong(string[] line) {
            string pongWithHash = $"PONG {line[1]}";
            await _streamWriter.WriteLineAsync(pongWithHash);
            if (Globals.RunConfig.LoggingLevel == LogLevel.Debug) {
                Logger.WriteLine(pongWithHash);
            }
            await _streamWriter.FlushAsync();
        }

        internal async Task Identify() {
            await PrivMsg("NickServ", $"IDENTIFY {Globals.RunConfig.Password}");
        }

        internal async Task JoinStartupChannels() {
            foreach (string channel in Globals.RunConfig.Channels) {
                await SendCommand(Command.JOIN, $"{channel}");
                await _streamWriter.WriteLineAsync($"JOIN {channel}");
                Logger.WriteLine($"Attempting to join: {channel}");
                await _streamWriter.FlushAsync();
            }
        }

        internal static Message ParseMessage(string[] line) {
            Message message = new();
            string sender = "";

            // strip the colon from the sender
            if (line[0].StartsWith(':')) {
                sender = line[0].Substring(1);
            } else {
                sender = line[0];
            }

            // check for presence of a User name, else assume it's a server name
            if (sender.Contains('!')) {
                message.Nick = sender.Split('!')[0];
                message.User = Utility.StringSplit(sender, '!', '@');
                message.Host = sender.Split('@')[1];
            } else {
                message.SeverName = sender;
            }
            Enum.TryParse(line[1], true, out Command command);
            message.Command = command;
            message.Receiver = line[2];
            if (line[3].StartsWith(':')) {
                message.Content = line[3].Substring(1);
            } else {
                message.Content = line[3];
            }

            return message;
        }

        internal async Task PrivMsg(string receiver, string message) {
            string commandString = $"{Command.PRIVMSG} {receiver} :{message}";
            await _streamWriter.WriteLineAsync(commandString);
            await _streamWriter.FlushAsync();
            Logger.WriteLine($"{receiver} {message}", nameof(Command.PRIVMSG), LogLevel.Info);
        }

        internal async Task SendCommand(Command command, string parameters) {
            string commandString = $"{command} {parameters}";
            await _streamWriter.WriteLineAsync(commandString);
            await _streamWriter.FlushAsync();
            Logger.WriteLine($"{parameters}", nameof(command), LogLevel.Info);
        }
    }
}
