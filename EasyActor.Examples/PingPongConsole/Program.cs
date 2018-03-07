using EasyActor.Examples;
using System;
using System.Threading.Tasks;

namespace PingPongConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            Console.WriteLine("ConcurrentQueue");
            await new PingPong().Test(false);
            Console.WriteLine("=================");

            Console.WriteLine("Retlang Queue");
            await new PingPong().TestRelang(false);

            Console.ReadLine();
        }
    }
}
