using Rtmp;
using Rtmp.Net;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GeForce
{
    public static class Program
    {
        #region Frame Server

        static RtmpServer frameServer;

        static async void FrameServerAsync()
        {
            Console.WriteLine("Frame server starting.");
            var options = new RtmpServer.Options
            {
                Context = new SerializationContext()
            };
            using (frameServer = await RtmpServer.ConnectAsync(options))
                frameServer.Wait();
            Console.WriteLine("Frame server stopped.");
        }

        #endregion

        #region Bot Client

        static async Task<RtmpClient> BotClientAsync()
        {
            try
            {
                Console.WriteLine("Client connecting.");
                var options = new RtmpClient.Options()
                {
                    Url = $"rtmp://localhost:4000",
                    Context = new SerializationContext(),
                    AppName = "bot",
                };
                var client = await RtmpClient.ConnectAsync(options);
                //client.Disconnected += (o, s) => { ((RtmpClient)o).InvokeAsync<object>("FCUnpublish").Wait(); };
                return client;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        #endregion

        static void Shutdown(object s, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Task.WaitAll(new[] { botClient?.CloseAsync(), frameServer?.CloseAsync() }.Where(x => x != null).ToArray());
            Environment.Exit(1);
        }

        static RtmpClient botClient;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Shutdown;
            Console.WriteLine("GeForce Bot. Press spacebar to connect or disconnect. Cntl-C to exit.\n");

            Task.Run(() => FrameServerAsync());

            while (true)
            {
                var keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Spacebar)
                {
                    botClient?.CloseAsync().Wait();
                    botClient = BotClientAsync().Result;
                }
                Console.Write(".");
            }
        }
    }
}