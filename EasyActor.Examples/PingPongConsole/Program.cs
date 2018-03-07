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
            for (var index = 0; index < 2; index++)
            {
                Console.WriteLine("ConcurrentQueue");
                await new PingPong().Test(false);
                Console.WriteLine("=================");

                Console.WriteLine("ConcurrentQueue no task");
                await new PingPong().TestNoTask(false);
                Console.WriteLine("=================");
            }
            Console.ReadLine();
        }
    }
}
