using System.Net.Sockets;

namespace roll_irc {
    internal class Roll {
        internal async Task Ikimasu(){
            Logger.CreateLogFile();
            Globals.RunConfig = await Config.GetConfig();
            Config.StoreConfig(Globals.RunConfig);

            using (TcpClient ircClient = new()) {
                Logger.WriteLine($"Connecting to {Globals.RunConfig.Server}:{Globals.RunConfig.ServerPort}");
                await ircClient.ConnectAsync(Globals.RunConfig.Server, Globals.RunConfig.ServerPort);
                if (!ircClient.Connected) {
                    Logger.WriteLine($"Connection failed.");
                }

                using (var dataStream = ircClient.GetStream())
                using (var streamWriter = new StreamWriter(dataStream))
                using (var streamReader = new StreamReader(dataStream)) {
                    await Authenticate(streamWriter);

                    while (ircClient.Connected) {
                        string? ircData = await streamReader.ReadLineAsync();
                        if (ircData != null) {
                            Logger.WriteLine($"{ircData}");

                            string[] line = ircData.Split(' ');
                            if (line[0] == "PING") {
                                await PingPong(streamWriter, line);
                            }

                            if (line.Length > 1) {
                                // irc codes are defined in RFC 1459: https://www.rfc-editor.org/rfc/rfc1459.html
                                switch (line[1]) {
                                    case "376": // end of motd
                                    case "422": // no motd
                                        await JoinChannels(streamWriter);
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

        internal static async Task PingPong(StreamWriter sw, string[] line) {
            string pongWithHash = $"PONG {line[1]}";
            await sw.WriteLineAsync(pongWithHash);
            if (Globals.RunConfig.LoggingLevel == LogLevel.Debug) {
                Logger.WriteLine(pongWithHash);
            }
            await sw.FlushAsync();
        }

        internal static async Task Authenticate(StreamWriter sw) {
            await sw.WriteLineAsync($"NICK {Globals.RunConfig.Nickname}");
            await sw.WriteLineAsync($"USER {Globals.RunConfig.Ident} * 0 {Globals.RunConfig.RealName}");
            await sw.WriteLineAsync($"PRIVMSG NickServ :IDENTIFY {Globals.RunConfig.Password}");
            await sw.FlushAsync();
        }

        internal static async Task JoinChannels(StreamWriter sw) {
            foreach (string channel in Globals.RunConfig.Channels) {
                await sw.WriteLineAsync($"JOIN {channel}");
                Logger.WriteLine($"Attempting to join: {channel}");
                await sw.FlushAsync();
            }
        }

        //internal async Task SendMessage() {

        //    return;
        //}


        // TODO: part channel
    }
}
