using System.Net.Sockets;

namespace roll_irc {
    internal class Program {
        internal static async Task Main(string[] args) {
            Roll roll = new();
            await roll.Ikimasu();
        }
    }
}