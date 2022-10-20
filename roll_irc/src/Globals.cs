namespace roll_irc {
    internal class Globals {

        //TODO: refactor to IHttpClientFactory
        //https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0
        public static readonly HttpClient WebClient = new();

        /// <summary>
        /// Global config for simplicity
        /// </summary>
        public static Config RunConfig = new();

#if DEBUG
        public static bool SilenceLogger = false;
#endif
    }
}
