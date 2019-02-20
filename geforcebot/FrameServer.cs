using Rtmp.Net;
using Rtmp.Net.RtmpMessages;
using System;

namespace GeForce
{
    public static partial class Program
    {
        static RtmpServer frameServer;

        static async void FrameServerAsync()
        {
            Console.WriteLine("Frame server starting.");
            var options = new RtmpServer.Options();
            using (frameServer = await RtmpServer.ConnectAsync(options, client =>
            {
                client.DispatchMessage = s =>
                {
                    if (botClient == null || !botClient.Connected) 
                        return;
                    if (s is VideoData)
                        botClient.SendMessage(s);
                };
            }))
                frameServer.Wait();
            Console.WriteLine("Frame server stopped.");
        }
    }
}