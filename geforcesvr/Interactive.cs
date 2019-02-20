using Rtmp.Net;
using System;
using System.Linq;

namespace GeForce
{
    public static partial class Program
    {
        readonly static WeakReference<RtmpClient> selected = new WeakReference<RtmpClient>(null);

        static bool RedirectTest(ConsoleKeyInfo ki) =>
            ki.Key == ConsoleKey.Oem3 && (ki.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control;

        static bool Redirect()
        {
            if (!selected.TryGetTarget(out var client))
                if (!Select(ConsoleKey.F1))
                    return false;
            if (!selected.TryGetTarget(out client))
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
                client.Execute("k", keyInfo.KeyChar);
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
    }
}