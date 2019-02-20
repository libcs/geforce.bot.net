using System;
using System.Linq;
using System.Threading.Tasks;

namespace GeForce
{
    public static partial class Program
    {
        static void Shutdown(object s, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Task.WaitAll(new[] { botClient?.CloseAsync(), frameServer?.CloseAsync() }.Where(x => x != null).ToArray());
            Environment.Exit(1);
        }

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