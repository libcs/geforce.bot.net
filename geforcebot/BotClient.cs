using Rtmp.Net;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace GeForce
{
    public static partial class Program
    {
        static RtmpClient botClient;
        static IntPtr botHandle;

        static async Task<RtmpClient> BotClientAsync()
        {
            try
            {
                Console.WriteLine("Client connecting...");
                var options = new RtmpClient.Options
                {
                    Url = "rtmp://localhost:4000",
                    AppName = "bot",
                };
                var client = await RtmpClient.ConnectAsync(options);
                Console.WriteLine("Client connected.");
                //client.Disconnected += (o, s) => { ((RtmpClient)o).InvokeAsync<object>("FCUnpublish").Wait(); };
                client.DispatchInvoke = s =>
                {
                    switch (s.MethodName)
                    {
                        case "a":
                            var windowName = (string)s.Arguments[0];
                            botHandle = Interop.GetHandleByWindow(windowName ?? "Star Citizen");
                            break;
                        case "k":
                            var key = (char)s.Arguments[0];
                            Interop.PressKey((byte)key);
                            break;
                        case "c":
                            Interop.ClickMouseButton(botHandle, InteropMouseButton.Left, new Point(545, 300));
                            break;
                        case "bf":
                            Interop.BringToFront(botHandle);
                            break;
                        default:
                            Console.WriteLine(s.MethodName);
                            break;
                    }
                };
                return client;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}