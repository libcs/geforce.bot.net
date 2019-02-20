using Rtmp;
using Rtmp.Net;
using System;

namespace GeForce
{
    public static partial class Program
    {
        #region Bot Server

        static RtmpServer botServer;

        static async void BotServerAsync()
        {
            Console.WriteLine("Bot server starting.");
            var options = new RtmpServer.Options
            {
                Url = "rtmp://any:4000",
            };
            using (botServer = await RtmpServer.ConnectAsync(options, client =>
            {
                client.DispatchMessage = s =>
                {
                    Console.WriteLine("MSG: " + s.GetType().Name);
                };
            }))
                botServer.Wait();
            Console.WriteLine("Bot server stopped.");
        }

        #endregion
    }
}