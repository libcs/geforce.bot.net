using Rtmp;
using Rtmp.Net;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GeForce
{
    public static class Program
    {
        #region Bot Server

        static RtmpServer botServer;

        static async void BotServerAsync()
        {
            Console.WriteLine("Bot server starting.");
            var options = new RtmpServer.Options
            {
                Url = $"rtmp://any:4000",
                Context = new SerializationContext()
            };
            using (botServer = await RtmpServer.ConnectAsync(options))
                botServer.Wait();
            Console.WriteLine("Bot server stopped.");
        }

        #endregion

        static void Shutdown(object s, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Task.WaitAll(new[] { botServer?.CloseAsync() }.Where(x => x != null).ToArray());
            Environment.Exit(1);
        }

        static bool RedirectTest(ConsoleKeyInfo ki) =>
            ki.Key == ConsoleKey.Oem3 && (ki.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control;

        static bool Redirect()
        {
            if ((!selected.TryGetTarget(out var client) || client == null) && Select(ConsoleKey.F1))
            {
                Console.WriteLine("No target selected");
                return false;
            }
            Console.WriteLine($"Redirect On. {Interop.MouseHook()}");
            while (true)
            {
                var keyInfo = Console.ReadKey(true);
                if (RedirectTest(keyInfo))
                    break;
                Console.WriteLine($"{client} -> {keyInfo.Key}");
            }
            Console.WriteLine($"Redirect Off. {Interop.MouseUnhook()}");
            return true;
        }

        static bool Select(ConsoleKey key)
        {
            var clientIdx = (int)key - (int)ConsoleKey.F1 + 1;
            if (clientIdx > botServer.clients.Count)
            {
                selected.SetTarget(null);
                Console.WriteLine($"No client at F{clientIdx}");
                return false;
            }
            var client = botServer.clients.ElementAt(clientIdx - 1).Value.client;
            selected.SetTarget(client);
            Console.WriteLine($"Selecting F{clientIdx} : {client}");
            return true;
        }

        readonly static WeakReference<RtmpClient> selected = new WeakReference<RtmpClient>(null);

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Shutdown;
            Console.WriteLine("GeForce Svr. Cntl-C to exit.\n");

            Task.Run(() => BotServerAsync());
            Interop.MsgPump();

            Console.WriteLine($"MouseHook: {Interop.MouseHook()}");
            while (true)
            {
                var keyInfo = Console.ReadKey(true);
                if (RedirectTest(keyInfo))
                    Redirect();
                else if (keyInfo.Key >= ConsoleKey.F1 && keyInfo.Key <= ConsoleKey.F5)
                    Select(keyInfo.Key);
            }
        }
    }
}