using System.Net.Sockets;
using System.Net.Security;

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
                            Logger.WriteLine($"{ircData}");

                            string[] line = ircData.Split(' ');
                            if (line[0] == "PING") {
                                await PingPong(line);
                                continue;
                            }

                            if (line.Length > 1) {
                                // irc codes are defined in RFC 1459: https://www.rfc-editor.org/rfc/rfc1459.html
                                switch (line[1]) {
                                    case "376": // end of motd
                                    case "422": // no motd
                                        await Authenticate();
                                        await JoinChannels();
                                        break;
                                    case "PRIVMSG":
                                        await ParseMessage(line);
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

        internal async Task PingPong(string[] line) {
            string pongWithHash = $"PONG {line[1]}";
            await _streamWriter.WriteLineAsync(pongWithHash);
            if (Globals.RunConfig.LoggingLevel == LogLevel.Debug) {
                Logger.WriteLine(pongWithHash);
            }
            await _streamWriter.FlushAsync();
        }

        internal async Task Authenticate() {
            await PrivMsg("NickServ", $"IDENTIFY {Globals.RunConfig.Password}");
        }

        internal async Task JoinChannels() {
            foreach (string channel in Globals.RunConfig.Channels) {
                await _streamWriter.WriteLineAsync($"JOIN {channel}");
                Logger.WriteLine($"Attempting to join: {channel}");
                await _streamWriter.FlushAsync();
            }
        }

        internal async static Task ParseMessage(string[] line) {
            Logger.WriteLine($"Parsing message {line}");
        }

        internal async Task PrivMsg(string receiver, string message) {
            string commandString = $"PRIVMSG {receiver} :{message}";
            await _streamWriter.WriteLineAsync(commandString);
            await _streamWriter.FlushAsync();
            Logger.WriteLine($"{receiver} {message}", "PRIVMSG", LogLevel.Info);
        }
    }
}
