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
            Task.WaitAll(new[] { botServer?.CloseAsync() }.Where(x => x != null).ToArray());
            Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Shutdown;
            Console.WriteLine("GeForce Svr. Cntl-C to exit.\n");

            Task.Run(() => BotServerAsync());

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